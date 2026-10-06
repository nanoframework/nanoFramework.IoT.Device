// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Device.Model;
using System.Threading;

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
        private const byte TemperatureReadyMask = 0x40;
        private readonly bool _shouldDispose;
        private I2cDevice _i2cDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="Max30100"/> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="shouldDispose">True to dispose the I2C device when this instance is disposed.</param>
        public Max30100(I2cDevice i2cDevice, bool shouldDispose = true)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
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

        /// <summary>
        /// Configures the measurement mode, sample rate, pulse width, and LED currents.
        /// </summary>
        /// <param name="mode">The operating mode.</param>
        /// <param name="sampleRate">The samples acquired per second.</param>
        /// <param name="pulseWidth">The LED pulse width and ADC resolution.</param>
        /// <param name="redLedCurrent">The red LED current.</param>
        /// <param name="infraredLedCurrent">The infrared LED current.</param>
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
            WriteRegister(Register.SpO2Configuration, (byte)(0x40 | ((byte)sampleRate << 2) | (byte)pulseWidth));
            WriteRegister(Register.LedConfiguration, (byte)(((byte)redLedCurrent << 4) | (byte)infraredLedCurrent));
            ClearFifo();
        }

        /// <summary>Enables or disables the FIFO-almost-full interrupt.</summary>
        /// <param name="enabled">True to enable the interrupt.</param>
        public void EnableFifoAlmostFullInterrupt(bool enabled) => SetInterrupt(0x80, enabled);

        /// <summary>Enables or disables the new-FIFO-data interrupt.</summary>
        /// <param name="enabled">True to enable the interrupt.</param>
        public void EnableNewFifoDataInterrupt(bool enabled) => SetInterrupt(0x20, enabled);

        /// <summary>Gets the number of unread samples currently stored in the FIFO.</summary>
        /// <returns>The number of unread samples.</returns>
        public byte GetUnreadSampleCount()
        {
            byte writePointer = (byte)(ReadRegister(Register.FifoWritePointer) & 0x0F);
            byte readPointer = (byte)(ReadRegister(Register.FifoReadPointer) & 0x0F);
            return (byte)((writePointer - readPointer) & 0x0F);
        }

        /// <summary>Attempts to read one red and infrared sample from the FIFO.</summary>
        /// <param name="reading">The acquired sample, or null when no sample is available.</param>
        /// <returns>True when a sample was read; otherwise, false.</returns>
        [Telemetry]
        public bool TryReadSample(out Max30100Reading reading)
        {
            if (GetUnreadSampleCount() == 0)
            {
                reading = null;
                return false;
            }

            reading = Max30100DataConverter.ConvertFifoSample(ReadRegisters(Register.FifoData, 4));
            return true;
        }

        /// <summary>Reads the MAX30100 die temperature in degrees Celsius.</summary>
        /// <returns>The die temperature in degrees Celsius.</returns>
        [Telemetry]
        public double ReadTemperature()
        {
            byte mode = ReadRegister(Register.ModeConfiguration);
            WriteRegister(Register.ModeConfiguration, (byte)(mode | ModeTemperatureEnableMask));

            for (int attempt = 0; attempt < 10; attempt++)
            {
                Thread.Sleep(10);
                if ((ReadRegister(Register.InterruptStatus) & TemperatureReadyMask) != 0)
                {
                    return Max30100DataConverter.ConvertTemperature(
                        ReadRegister(Register.TemperatureInteger),
                        ReadRegister(Register.TemperatureFraction));
                }
            }

            throw new InvalidOperationException("The MAX30100 temperature conversion did not complete.");
        }

        /// <summary>Clears all FIFO pointers and unread samples.</summary>
        public void ClearFifo()
        {
            WriteRegister(Register.FifoWritePointer, 0);
            WriteRegister(Register.FifoOverflowCounter, 0);
            WriteRegister(Register.FifoReadPointer, 0);
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
        public void Reset()
        {
            byte mode = ReadRegister(Register.ModeConfiguration);
            WriteRegister(Register.ModeConfiguration, (byte)(mode | ModeResetMask));
            for (int attempt = 0; attempt < 20; attempt++)
            {
                Thread.Sleep(5);
                if ((ReadRegister(Register.ModeConfiguration) & ModeResetMask) == 0)
                {
                    return;
                }
            }

            throw new InvalidOperationException("The MAX30100 reset did not complete.");
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
                throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }

        private static void ValidateSampleRate(Max30100SampleRate sampleRate)
        {
            if ((byte)sampleRate > (byte)Max30100SampleRate.SamplesPerSecond1000)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleRate));
            }
        }

        private static void ValidatePulseWidth(Max30100PulseWidth pulseWidth)
        {
            if ((byte)pulseWidth > (byte)Max30100PulseWidth.Microseconds1600)
            {
                throw new ArgumentOutOfRangeException(nameof(pulseWidth));
            }
        }

        private static void ValidateLedCurrent(Max30100LedCurrent ledCurrent)
        {
            if ((byte)ledCurrent > (byte)Max30100LedCurrent.Milliamps50)
            {
                throw new ArgumentOutOfRangeException(nameof(ledCurrent));
            }
        }

        private void SetInterrupt(byte mask, bool enabled)
        {
            byte interrupts = ReadRegister(Register.InterruptEnable);
            interrupts = enabled ? (byte)(interrupts | mask) : (byte)(interrupts & ~mask);
            WriteRegister(Register.InterruptEnable, interrupts);
        }

        private byte ReadRegister(Register register) => ReadRegisters(register, 1)[0];

        private byte[] ReadRegisters(Register register, int length)
        {
            EnsureNotDisposed();
            byte[] readBuffer = new byte[length];
            _i2cDevice.WriteRead(new byte[] { (byte)register }, readBuffer);
            return readBuffer;
        }

        private void WriteRegister(Register register, byte value)
        {
            EnsureNotDisposed();
            _i2cDevice.Write(new byte[] { (byte)register, value });
        }

        private void EnsureNotDisposed()
        {
            if (_i2cDevice == null)
            {
                throw new ObjectDisposedException(nameof(Max30100));
            }
        }
    }
}
