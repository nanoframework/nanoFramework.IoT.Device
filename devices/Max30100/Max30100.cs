// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Device.Model;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Max30100
{
    /// <summary>
    /// MAX30100 pulse oximeter and heart-rate sensor.
    /// </summary>
    [Interface("MAX30100 pulse oximeter and heart-rate sensor.")]
    public sealed class Max30100 : IDisposable
    {
        /// <summary>The default I2C address.</summary>
        public const byte DefaultI2cAddress = 0x57;

        /// <summary>The expected MAX30100 part identifier.</summary>
        public const byte ExpectedPartId = 0x11;

        private const byte ModeShutdownMask = 0x80;
        private const byte ModeResetMask = 0x40;
        private const byte ModeTemperatureEnableMask = 0x08;
        private const byte SpO2HighResolutionMask = 0x40;
        private const int SpO2SampleRateOffset = 2;
        private const int RedLedCurrentOffset = 4;
        private const byte FifoAlmostFullInterruptMask = 0x80;
        private const byte NewFifoDataInterruptMask = 0x20;
        private const byte FifoPointerMask = 0x0F;
        private const byte TemperatureReadyMask = 0x40;
        private const int TemperatureConversionAttempts = 10;
        private const int TemperatureConversionDelayMilliseconds = 10;
        private const int ResetAttempts = 20;
        private const int ResetDelayMilliseconds = 5;
        private readonly bool _shouldDispose;
        private readonly byte[] _fifoSampleBuffer = new byte[4];
        private readonly byte[] _fifoStatusBuffer = new byte[3];
        private readonly byte[] _registerAddressBuffer = new byte[1];
        private readonly byte[] _registerReadBuffer = new byte[1];
        private readonly byte[] _registerWriteBuffer = new byte[2];
        private I2cDevice _i2cDevice;
        private uint _droppedSampleCount;
        private byte _unreadSampleCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="Max30100"/> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="shouldDispose">True to dispose the I2C device when this instance is disposed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="i2cDevice"/> is null.</exception>
        public Max30100(I2cDevice i2cDevice, bool shouldDispose = true)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException();
            _shouldDispose = shouldDispose;
        }

        /// <summary>Gets the device part identifier.</summary>
        /// <value>The device part identifier.</value>
        [Property]
        public byte PartId => ReadRegister(Register.PartId);

        /// <summary>Gets the device revision identifier.</summary>
        /// <value>The device revision identifier.</value>
        [Property]
        public byte RevisionId => ReadRegister(Register.RevisionId);

        /// <summary>Gets the number of samples discarded because the FIFO overflowed.</summary>
        /// <value>The number of discarded samples observed since this instance was created.</value>
        public uint DroppedSampleCount => _droppedSampleCount;

        /// <summary>
        /// Configures the measurement mode, sample rate, pulse width, and LED currents.
        /// </summary>
        /// <param name="mode">The operating mode.</param>
        /// <param name="sampleRate">The samples acquired per second.</param>
        /// <param name="pulseWidth">The LED pulse width and ADC resolution.</param>
        /// <param name="redLedCurrent">The red LED current.</param>
        /// <param name="infraredLedCurrent">The infrared LED current.</param>
        /// <exception cref="ArgumentOutOfRangeException">A configuration value is not defined by the MAX30100.</exception>
        public void Configure(
            Max30100Mode mode = Max30100Mode.SpO2,
            Max30100SampleRate sampleRate = Max30100SampleRate.SamplesPerSecond100,
            Max30100PulseWidth pulseWidth = Max30100PulseWidth.Microseconds1600,
            Max30100LedCurrent redLedCurrent = Max30100LedCurrent.Milliamps27Point1,
            Max30100LedCurrent infraredLedCurrent = Max30100LedCurrent.Milliamps27Point1)
        {
            ValidateMode(mode);
            ValidateSampleRate(sampleRate);
            ValidatePulseWidth(pulseWidth);
            ValidateLedCurrent(redLedCurrent);
            ValidateLedCurrent(infraredLedCurrent);

            WriteRegister(Register.ModeConfiguration, (byte)mode);
            byte spO2Configuration = (byte)(
                SpO2HighResolutionMask
                | ((byte)sampleRate << SpO2SampleRateOffset)
                | (byte)pulseWidth);
            byte ledConfiguration = (byte)(
                ((byte)redLedCurrent << RedLedCurrentOffset)
                | (byte)infraredLedCurrent);
            WriteRegister(Register.SpO2Configuration, spO2Configuration);
            WriteRegister(Register.LedConfiguration, ledConfiguration);
            ClearFifo();
        }

        /// <summary>Enables or disables the FIFO-almost-full interrupt.</summary>
        /// <param name="enabled">True to enable the interrupt.</param>
        public void EnableFifoAlmostFullInterrupt(bool enabled) => SetInterrupt(FifoAlmostFullInterruptMask, enabled);

        /// <summary>Enables or disables the new-FIFO-data interrupt.</summary>
        /// <param name="enabled">True to enable the interrupt.</param>
        public void EnableNewFifoDataInterrupt(bool enabled) => SetInterrupt(NewFifoDataInterruptMask, enabled);

        /// <summary>Gets the number of unread samples currently stored in the FIFO.</summary>
        /// <returns>The number of unread samples.</returns>
        public byte GetUnreadSampleCount()
        {
            RefreshUnreadSampleCount();
            return _unreadSampleCount;
        }

        /// <summary>Attempts to read one red and infrared sample from the FIFO.</summary>
        /// <param name="reading">The acquired sample, or null when no sample is available.</param>
        /// <returns>True when a sample was read; otherwise, false.</returns>
        [Telemetry]
        public bool TryReadSample(out Max30100Reading reading)
        {
            if (_unreadSampleCount == 0)
            {
                RefreshUnreadSampleCount();
            }

            if (_unreadSampleCount == 0)
            {
                reading = default(Max30100Reading);
                return false;
            }

            ReadRegisters(Register.FifoData, _fifoSampleBuffer);
            reading = Max30100DataConverter.ConvertFifoSample(_fifoSampleBuffer);
            _unreadSampleCount--;
            return true;
        }

        /// <summary>Reads the MAX30100 internal die temperature.</summary>
        /// <returns>The internal die temperature.</returns>
        /// <exception cref="InvalidOperationException">The MAX30100 temperature conversion did not complete.</exception>
        [Telemetry]
        public Temperature ReadTemperature()
        {
            byte mode = ReadRegister(Register.ModeConfiguration);
            WriteRegister(Register.ModeConfiguration, (byte)(mode | ModeTemperatureEnableMask));

            for (int attempt = 0; attempt < TemperatureConversionAttempts; attempt++)
            {
                Thread.Sleep(TemperatureConversionDelayMilliseconds);
                if ((ReadRegister(Register.InterruptStatus) & TemperatureReadyMask) != 0)
                {
                    return Temperature.FromDegreesCelsius(
                        Max30100DataConverter.ConvertTemperature(
                            ReadRegister(Register.TemperatureInteger),
                            ReadRegister(Register.TemperatureFraction)));
                }
            }

            throw new InvalidOperationException();
        }

        /// <summary>Clears all FIFO pointers and unread samples.</summary>
        public void ClearFifo()
        {
            WriteRegister(Register.FifoWritePointer, 0);
            WriteRegister(Register.FifoOverflowCounter, 0);
            WriteRegister(Register.FifoReadPointer, 0);
            _unreadSampleCount = 0;
        }

        /// <summary>Places the sensor in or out of shutdown mode.</summary>
        /// <param name="shutdown">True to enter low-power shutdown mode.</param>
        public void SetShutdown(bool shutdown)
        {
            byte mode = ReadRegister(Register.ModeConfiguration);
            mode = shutdown ? (byte)(mode | ModeShutdownMask) : (byte)(mode & ~ModeShutdownMask);
            WriteRegister(Register.ModeConfiguration, mode);
        }

        /// <summary>Resets the sensor and waits for the reset to complete.</summary>
        /// <exception cref="InvalidOperationException">The MAX30100 reset did not complete.</exception>
        public void Reset()
        {
            byte mode = ReadRegister(Register.ModeConfiguration);
            WriteRegister(Register.ModeConfiguration, (byte)(mode | ModeResetMask));
            for (int attempt = 0; attempt < ResetAttempts; attempt++)
            {
                Thread.Sleep(ResetDelayMilliseconds);
                if ((ReadRegister(Register.ModeConfiguration) & ModeResetMask) == 0)
                {
                    return;
                }
            }

            throw new InvalidOperationException();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_shouldDispose && _i2cDevice != null)
            {
                _i2cDevice.Dispose();
            }

            _i2cDevice = null;
        }

        private static void ValidateMode(Max30100Mode mode)
        {
            if (mode != Max30100Mode.HeartRate && mode != Max30100Mode.SpO2)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        private static void ValidateSampleRate(Max30100SampleRate sampleRate)
        {
            if ((byte)sampleRate > (byte)Max30100SampleRate.SamplesPerSecond1000)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        private static void ValidatePulseWidth(Max30100PulseWidth pulseWidth)
        {
            if ((byte)pulseWidth > (byte)Max30100PulseWidth.Microseconds1600)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        private static void ValidateLedCurrent(Max30100LedCurrent ledCurrent)
        {
            if ((byte)ledCurrent > (byte)Max30100LedCurrent.Milliamps50)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        private void SetInterrupt(byte mask, bool enabled)
        {
            byte interrupts = ReadRegister(Register.InterruptEnable);
            interrupts = enabled ? (byte)(interrupts | mask) : (byte)(interrupts & ~mask);
            WriteRegister(Register.InterruptEnable, interrupts);
        }

        private void RefreshUnreadSampleCount()
        {
            ReadRegisters(Register.FifoWritePointer, _fifoStatusBuffer);
            byte writePointer = (byte)(_fifoStatusBuffer[0] & FifoPointerMask);
            byte overflowCount = (byte)(_fifoStatusBuffer[1] & FifoPointerMask);
            byte readPointer = (byte)(_fifoStatusBuffer[2] & FifoPointerMask);

            if (overflowCount != 0)
            {
                _droppedSampleCount += overflowCount;
                WriteRegister(Register.FifoOverflowCounter, 0);
            }

            _unreadSampleCount = Max30100DataConverter.CalculateUnreadSampleCount(
                writePointer,
                readPointer,
                overflowCount);
        }

        private byte ReadRegister(Register register)
        {
            ReadRegisters(register, _registerReadBuffer);
            return _registerReadBuffer[0];
        }

        private void ReadRegisters(Register register, byte[] readBuffer)
        {
            EnsureNotDisposed();
            _registerAddressBuffer[0] = (byte)register;
            _i2cDevice.WriteRead(_registerAddressBuffer, readBuffer);
        }

        private void WriteRegister(Register register, byte value)
        {
            EnsureNotDisposed();
            _registerWriteBuffer[0] = (byte)register;
            _registerWriteBuffer[1] = value;
            _i2cDevice.Write(_registerWriteBuffer);
        }

        private void EnsureNotDisposed()
        {
            if (_i2cDevice == null)
            {
                throw new ObjectDisposedException(null);
            }
        }
    }
}
