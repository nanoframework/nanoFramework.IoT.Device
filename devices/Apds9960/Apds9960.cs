// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Device.Model;
#if DEBUG
using System.Diagnostics;
#endif
using System.Drawing;
using System.IO;
using System.Threading;

namespace Iot.Device.Apds9960
{
    /// <summary>
    /// APDS9960 digital proximity, ambient light, RGB, and gesture sensor.
    /// </summary>
    [Interface("APDS9960 proximity, ambient light, RGB, and gesture sensor")]
    public sealed class Apds9960 : IDisposable
    {
        /// <summary>
        /// Default I2C address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x39;

        private const int DefaultAdcMaximumCount = 37888;

        private readonly byte[] _registerAddress = new byte[1];
        private readonly byte[] _registerValue = new byte[1];
        private readonly byte[] _registerWrite = new byte[2];
        private readonly byte[] _colorData = new byte[8];
        private double _redChannelCompensation = 1.0;
        private double _greenChannelCompensation = 1.0;
        private double _blueChannelCompensation = 1.0;
        private I2cDevice _i2cDevice;

#if DEBUG
        /// <summary>
        /// Gets or sets a value indicating whether raw gesture FIFO diagnostics are written to the debugger.
        /// </summary>
        /// <value><see langword="true"/> to write FIFO levels, raw directional samples, ratios, and deltas; otherwise, <see langword="false"/>.</value>
        [Property]
        public bool GestureDiagnosticsEnabled { get; set; }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="Apds9960"/> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <exception cref="ArgumentNullException"><paramref name="i2cDevice"/> is null.</exception>
        public Apds9960(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException();
            Initialize();
        }

        /// <summary>
        /// Gets the device identifier.
        /// </summary>
        /// <value>The device identifier.</value>
        /// <exception cref="IOException">The APDS9960 did not complete the I2C read transfer.</exception>
        /// <exception cref="ObjectDisposedException">The APDS9960 has been disposed.</exception>
        [Property]
        public byte DeviceId => ReadRegister(Register.Id);

        /// <summary>
        /// Gets or sets the ambient light ADC gain.
        /// </summary>
        /// <value>The ambient light ADC gain.</value>
        /// <exception cref="IOException">The APDS9960 did not complete the I2C transfer.</exception>
        /// <exception cref="ObjectDisposedException">The APDS9960 has been disposed.</exception>
        [Property]
        public AmbientLightGain AmbientLightGain
        {
            get => (AmbientLightGain)(ReadRegister(Register.Control) & 0x03);
            set => UpdateRegister(Register.Control, 0x03, (byte)value);
        }

        /// <summary>
        /// Gets or sets the proximity receiver gain.
        /// </summary>
        /// <value>The proximity receiver gain.</value>
        /// <exception cref="IOException">The APDS9960 did not complete the I2C transfer.</exception>
        /// <exception cref="ObjectDisposedException">The APDS9960 has been disposed.</exception>
        [Property]
        public ProximityGain ProximityGain
        {
            get => (ProximityGain)((ReadRegister(Register.Control) >> 2) & 0x03);
            set => UpdateRegister(Register.Control, 0x0C, (byte)((byte)value << 2));
        }

        /// <summary>
        /// Gets or sets the proximity and gesture LED drive strength.
        /// </summary>
        /// <value>The LED drive strength.</value>
        /// <exception cref="IOException">The APDS9960 did not complete the I2C transfer.</exception>
        /// <exception cref="ObjectDisposedException">The APDS9960 has been disposed.</exception>
        [Property]
        public LedDriveStrength LedDriveStrength
        {
            get => (LedDriveStrength)((ReadRegister(Register.Control) >> 6) & 0x03);
            set => UpdateRegister(Register.Control, 0xC0, (byte)((byte)value << 6));
        }

        /// <summary>
        /// Gets or sets the red-channel white-balance compensation multiplier.
        /// </summary>
        /// <value>The positive red-channel compensation multiplier.</value>
        /// <exception cref="ArgumentOutOfRangeException">The value is not greater than zero.</exception>
        [Property]
        public double RedChannelCompensation
        {
            get => _redChannelCompensation;
            set
            {
                ValidateChannelCompensation(value);
                _redChannelCompensation = value;
            }
        }

        /// <summary>
        /// Gets or sets the green-channel white-balance compensation multiplier.
        /// </summary>
        /// <value>The positive green-channel compensation multiplier.</value>
        /// <exception cref="ArgumentOutOfRangeException">The value is not greater than zero.</exception>
        [Property]
        public double GreenChannelCompensation
        {
            get => _greenChannelCompensation;
            set
            {
                ValidateChannelCompensation(value);
                _greenChannelCompensation = value;
            }
        }

        /// <summary>
        /// Gets or sets the blue-channel white-balance compensation multiplier.
        /// </summary>
        /// <value>The positive blue-channel compensation multiplier.</value>
        /// <exception cref="ArgumentOutOfRangeException">The value is not greater than zero.</exception>
        [Property]
        public double BlueChannelCompensation
        {
            get => _blueChannelCompensation;
            set
            {
                ValidateChannelCompensation(value);
                _blueChannelCompensation = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a color measurement is ready.
        /// </summary>
        /// <value><see langword="true"/> when color data is ready; otherwise, <see langword="false"/>.</value>
        /// <exception cref="IOException">The APDS9960 did not complete the I2C read transfer.</exception>
        /// <exception cref="ObjectDisposedException">The APDS9960 has been disposed.</exception>
        [Telemetry]
        public bool IsColorDataReady => (ReadRegister(Register.Status) & 0x01) != 0;

        /// <summary>
        /// Gets a value indicating whether gesture FIFO data is available.
        /// </summary>
        /// <value><see langword="true"/> when gesture data is available; otherwise, <see langword="false"/>.</value>
        /// <exception cref="IOException">The APDS9960 did not complete the I2C read transfer.</exception>
        /// <exception cref="ObjectDisposedException">The APDS9960 has been disposed.</exception>
        [Telemetry]
        public bool IsGestureAvailable => (ReadRegister(Register.GestureStatus) & 0x01) != 0;

        private static int ScaleClearChannel(ushort value)
        {
            int scaled = value * 255 / DefaultAdcMaximumCount;
            return scaled > 255 ? 255 : scaled;
        }

        private static int NormalizeColorChannel(double value, double maximum)
        {
            int scaled = (int)((value * 255 / maximum) + 0.5);
            return scaled > 255 ? 255 : scaled;
        }

        private static void ValidateChannelCompensation(double value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        // Using this conversion for simplicity rather than using the Binary converter
        private static ushort ToUInt16(byte[] data, int index) => (ushort)(data[index] | (data[index + 1] << 8));

        /// <summary>
        /// Configures the sensor with general-purpose defaults and powers it on.
        /// </summary>
        /// <exception cref="IOException">The APDS9960 did not complete the I2C write transfer.</exception>
        /// <exception cref="ObjectDisposedException">The APDS9960 has been disposed.</exception>
        private void Initialize()
        {
            // Disable all sensor engines while configuring.
            WriteRegister(Register.Enable, 0x00);

            // Set the ALS integration time to approximately 103 ms.
            WriteRegister(Register.AdcIntegrationTime, 0xDB);

            // Set the wait time between measurements to approximately 27.8 ms.
            WriteRegister(Register.WaitTime, 0xF6);

            // Set the low byte of the ALS low threshold to 0.
            WriteRegister(Register.AmbientLightInterruptLowThresholdLow, 0x00);

            // Set the high byte of the ALS low threshold to 0.
            WriteRegister(Register.AmbientLightInterruptLowThresholdHigh, 0x00);

            // Set the low byte of the ALS high threshold to 255.
            WriteRegister(Register.AmbientLightInterruptHighThresholdLow, 0xFF);

            // Set the high byte of the ALS high threshold to 255.
            WriteRegister(Register.AmbientLightInterruptHighThresholdHigh, 0xFF);

            // Set the proximity low threshold to 0.
            WriteRegister(Register.ProximityInterruptLowThreshold, 0x00);

            // Set the proximity high threshold to 50.
            WriteRegister(Register.ProximityInterruptHighThreshold, 50);

            // Require two consecutive ALS or proximity events for an interrupt.
            WriteRegister(Register.InterruptPersistence, 0x11);

            // Preserve required bits and disable the 12-times wait multiplier.
            WriteRegister(Register.Configuration1, 0x60);

            // Use eight 16 microsecond proximity pulses.
            WriteRegister(Register.ProximityPulseCountAndLength, 0x87);

            // Use 100 mA LED drive, 2-times proximity gain, and 4-times ALS gain.
            byte control = (byte)(
                ((byte)LedDriveStrength.MilliAmps100 << 6)
                | ((byte)ProximityGain.Two << 2)
                | (byte)AmbientLightGain.Four);
            WriteRegister(Register.Control, control);

            // Disable saturation interrupts and use 100 percent LED boost.
            WriteRegister(Register.Configuration2, 0x01);

            // Enable all proximity photodiodes and disable sleep-after-interrupt.
            WriteRegister(Register.Configuration3, 0x00);

            // Apply the default gesture thresholds, gain, timing, and pulse configuration.
            ConfigureGestureEngine();

            // Power on the sensor while leaving its measurement engines disabled.
            WriteRegister(Register.Enable, 0x01);

            // Allow the oscillator and analog circuitry to stabilize.
            Thread.Sleep(10);
        }

        /// <summary>
        /// Gets or sets a value indicating whether ambient light and RGB measurements are enabled.
        /// </summary>
        /// <value><see langword="true"/> when ambient light and RGB measurements are enabled; otherwise, <see langword="false"/>.</value>
        /// <exception cref="IOException">The APDS9960 did not complete the I2C transfer.</exception>
        /// <exception cref="ObjectDisposedException">The APDS9960 has been disposed.</exception>
        [Property]
        public bool ColorSensorEnabled
        {
            get => (ReadRegister(Register.Enable) & 0x02) != 0;
            set
            {
                if (value)
                {
                    SetEnableBits(0x03, true);
                }
                else
                {
                    SetEnableBits(0x02, false);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether ambient light interrupts are enabled.
        /// </summary>
        /// <value><see langword="true"/> when ambient light interrupts are enabled; otherwise, <see langword="false"/>.</value>
        /// <exception cref="IOException">The APDS9960 did not complete the I2C transfer.</exception>
        /// <exception cref="ObjectDisposedException">The APDS9960 has been disposed.</exception>
        [Property]
        public bool ColorInterruptEnabled
        {
            get => (ReadRegister(Register.Enable) & 0x10) != 0;
            set => SetEnableBits(0x10, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether proximity measurements are enabled.
        /// </summary>
        /// <value><see langword="true"/> when proximity measurements are enabled; otherwise, <see langword="false"/>.</value>
        /// <exception cref="IOException">The APDS9960 did not complete the I2C transfer.</exception>
        /// <exception cref="ObjectDisposedException">The APDS9960 has been disposed.</exception>
        [Property]
        public bool ProximitySensorEnabled
        {
            get => (ReadRegister(Register.Enable) & 0x04) != 0;
            set
            {
                if (value)
                {
                    SetEnableBits(0x05, true);
                }
                else
                {
                    SetEnableBits(0x04, false);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether proximity interrupts are enabled.
        /// </summary>
        /// <value><see langword="true"/> when proximity interrupts are enabled; otherwise, <see langword="false"/>.</value>
        /// <exception cref="IOException">The APDS9960 did not complete the I2C transfer.</exception>
        /// <exception cref="ObjectDisposedException">The APDS9960 has been disposed.</exception>
        [Property]
        public bool ProximityInterruptEnabled
        {
            get => (ReadRegister(Register.Enable) & 0x20) != 0;
            set => SetEnableBits(0x20, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether gesture recognition is enabled.
        /// </summary>
        /// <value><see langword="true"/> when gesture recognition is enabled; otherwise, <see langword="false"/>.</value>
        /// <exception cref="IOException">The APDS9960 did not complete the I2C transfer.</exception>
        /// <exception cref="ObjectDisposedException">The APDS9960 has been disposed.</exception>
        [Property]
        public bool GestureSensorEnabled
        {
            get => (ReadRegister(Register.Enable) & 0x40) != 0 && (ReadRegister(Register.GestureConfiguration4) & 0x01) != 0;
            set
            {
                if (value)
                {
                    bool interruptEnabled = GestureInterruptEnabled;
                    ConfigureGestureEngine();
                    UpdateRegister(Register.GestureConfiguration4, 0x03, interruptEnabled ? (byte)0x03 : (byte)0x01);
                    SetEnableBits(0x4D, true);
                }
                else
                {
                    UpdateRegister(Register.GestureConfiguration4, 0x01, 0x00);
                    SetEnableBits(0x40, false);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether gesture interrupts are enabled.
        /// </summary>
        /// <value><see langword="true"/> when gesture interrupts are enabled; otherwise, <see langword="false"/>.</value>
        /// <exception cref="IOException">The APDS9960 did not complete the I2C transfer.</exception>
        /// <exception cref="ObjectDisposedException">The APDS9960 has been disposed.</exception>
        [Property]
        public bool GestureInterruptEnabled
        {
            get => (ReadRegister(Register.GestureConfiguration4) & 0x02) != 0;
            set => UpdateRegister(Register.GestureConfiguration4, 0x02, value ? (byte)0x02 : (byte)0x00);
        }

        /// <summary>
        /// Reads clear, red, green, and blue ADC values. Clear is mapped on the A channel.
        /// </summary>
        /// <returns>The latest color measurement.</returns>
        /// <exception cref="IOException">The APDS9960 did not complete the I2C read transfer.</exception>
        /// <exception cref="ObjectDisposedException">The APDS9960 has been disposed.</exception>
        [Telemetry]
        public Color ReadColorData()
        {
            ReadRegisters(Register.ClearDataLow, _colorData);
            return ConvertColorData(
                _colorData,
                _redChannelCompensation,
                _greenChannelCompensation,
                _blueChannelCompensation);
        }

        /// <summary>
        /// Reads the latest proximity value.
        /// </summary>
        /// <returns>A value from 0 for no detection to 255 for maximum detection.</returns>
        /// <exception cref="IOException">The APDS9960 did not complete the I2C read transfer.</exception>
        /// <exception cref="ObjectDisposedException">The APDS9960 has been disposed.</exception>
        [Telemetry]
        public byte ReadProximity() => ReadRegister(Register.ProximityData);

        /// <summary>
        /// Reads and decodes a complete gesture.
        /// </summary>
        /// <returns>The detected gesture, or <see cref="Gesture.None"/> when insufficient directional movement was recorded.</returns>
        /// <remarks>This method blocks while gesture data remains valid and returns after the gesture engine exits.</remarks>
        /// <exception cref="IOException">The APDS9960 did not complete the I2C read transfer.</exception>
        /// <exception cref="ObjectDisposedException">The APDS9960 has been disposed.</exception>
        [Telemetry]
        public Gesture ReadGesture()
        {
            if (!IsGestureAvailable)
            {
                return Gesture.None;
            }

            bool hasGestureData = false;
            int initialUpDownRatio = 0;
            int initialLeftRightRatio = 0;
            int finalUpDownRatio = 0;
            int finalLeftRightRatio = 0;

            while (true)
            {
                if (!IsGestureAvailable)
                {
                    break;
                }

                int dataSetCount = ReadRegister(Register.GestureFifoLevel);
#if DEBUG
                if (GestureDiagnosticsEnabled)
                {
                    Debug.WriteLine($"GFIFO level={dataSetCount}");
                }
#endif

                if (dataSetCount <= 0 || dataSetCount > 32)
                {
                    Thread.Sleep(10);
                    continue;
                }

                byte[] fifoData = new byte[dataSetCount * 4];
                ReadRegisters(Register.GestureFifoUp, fifoData);
#if DEBUG
                if (GestureDiagnosticsEnabled)
                {
                    for (int dataSet = 0; dataSet < dataSetCount; dataSet++)
                    {
                        int index = dataSet * 4;
                        Debug.WriteLine(
                            $"GFIFO[{dataSet}] U={fifoData[index]} D={fifoData[index + 1]} L={fifoData[index + 2]} R={fifoData[index + 3]}");
                    }
                }
#endif

                if (GestureDecoder.TryGetRatios(
                    fifoData,
                    dataSetCount,
                    out int batchInitialUpDownRatio,
                    out int batchInitialLeftRightRatio,
                    out int batchFinalUpDownRatio,
                    out int batchFinalLeftRightRatio))
                {
                    if (!hasGestureData)
                    {
                        initialUpDownRatio = batchInitialUpDownRatio;
                        initialLeftRightRatio = batchInitialLeftRightRatio;
                        hasGestureData = true;
                    }

                    finalUpDownRatio = batchFinalUpDownRatio;
                    finalLeftRightRatio = batchFinalLeftRightRatio;

#if DEBUG
                    if (GestureDiagnosticsEnabled)
                    {
                        Debug.WriteLine(
                            $"GRATIO firstUD={batchInitialUpDownRatio} firstLR={batchInitialLeftRightRatio} lastUD={batchFinalUpDownRatio} lastLR={batchFinalLeftRightRatio}");
                    }
#endif
                }
#if DEBUG
                else if (GestureDiagnosticsEnabled)
                {
                    Debug.WriteLine("GRATIO no valid data");
                }
#endif

                Thread.Sleep(10);
            }

            if (!hasGestureData)
            {
#if DEBUG
                if (GestureDiagnosticsEnabled)
                {
                    Debug.WriteLine("GRESULT no valid data");
                }
#endif

                return Gesture.None;
            }

            int upDownDelta = finalUpDownRatio - initialUpDownRatio;
            int leftRightDelta = finalLeftRightRatio - initialLeftRightRatio;
            Gesture gesture = GestureDecoder.DecodeDeltas(upDownDelta, leftRightDelta);
#if DEBUG
            if (GestureDiagnosticsEnabled)
            {
                Debug.WriteLine(
                    $"GRESULT initialUD={initialUpDownRatio} initialLR={initialLeftRightRatio} finalUD={finalUpDownRatio} finalLR={finalLeftRightRatio} deltaUD={upDownDelta} deltaLR={leftRightDelta} gesture={(int)gesture}");
            }
#endif

            return gesture;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the sensor is powered.
        /// </summary>
        /// <value><see langword="true"/> when the sensor is powered; otherwise, <see langword="false"/>.</value>
        /// <exception cref="IOException">The APDS9960 did not complete the I2C transfer.</exception>
        /// <exception cref="ObjectDisposedException">The APDS9960 has been disposed.</exception>
        [Property]
        public bool PowerEnabled
        {
            get => (ReadRegister(Register.Enable) & 0x01) != 0;
            set => SetEnableBits(0x01, value);
        }

        internal static Color ConvertColorData(
            byte[] data,
            double redCompensation = 1.0,
            double greenCompensation = 1.0,
            double blueCompensation = 1.0)
        {
            ushort clear = ToUInt16(data, 0);
            double red = ToUInt16(data, 2) * redCompensation;
            double green = ToUInt16(data, 4) * greenCompensation;
            double blue = ToUInt16(data, 6) * blueCompensation;
            double maximum = red > green ? red : green;
            maximum = maximum > blue ? maximum : blue;
            if (clear == 0 || maximum <= 0)
            {
                return Color.FromArgb(0, 0, 0, 0);
            }

            return Color.FromArgb(
                ScaleClearChannel(clear),
                NormalizeColorChannel(red, maximum),
                NormalizeColorChannel(green, maximum),
                NormalizeColorChannel(blue, maximum));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }

        private void ConfigureGestureEngine()
        {
            // Enter gesture mode above the normal background level while using the reduced optical sensitivity.
            WriteRegister(Register.GestureEntryThreshold, 10);

            // Exit gesture mode when the signal returns to the normal background level.
            WriteRegister(Register.GestureExitThreshold, 7);

            // Use four FIFO datasets and one exit-persistence event.
            WriteRegister(Register.GestureConfiguration1, 0x40);

            // Use 1-times gain, 25 mA LED drive, and a 2.8 ms wait to avoid saturating the gesture FIFO.
            byte gestureConfiguration2 = (byte)(
                ((byte)ProximityGain.One << 5)
                | ((byte)LedDriveStrength.MilliAmps25 << 3)
                | 0x01);
            WriteRegister(Register.GestureConfiguration2, gestureConfiguration2);

            // Apply no up photodiode offset.
            WriteRegister(Register.GestureUpOffset, 0x00);

            // Apply no down photodiode offset.
            WriteRegister(Register.GestureDownOffset, 0x00);

            // Apply no left photodiode offset.
            WriteRegister(Register.GestureLeftOffset, 0x00);

            // Apply no right photodiode offset.
            WriteRegister(Register.GestureRightOffset, 0x00);

            // Use ten 32 microsecond gesture pulses.
            WriteRegister(Register.GesturePulseCountAndLength, 0xC9);

            // Enable all photodiodes and both gesture dimensions.
            WriteRegister(Register.GestureConfiguration3, 0x00);

            // Leave gesture mode and gesture interrupts disabled.
            WriteRegister(Register.GestureConfiguration4, 0x00);
        }

        private void SetEnableBits(byte mask, bool enabled)
        {
            UpdateRegister(Register.Enable, mask, enabled ? mask : (byte)0x00);
        }

        private void UpdateRegister(Register register, byte mask, byte value)
        {
            byte current = ReadRegister(register);
            WriteRegister(register, (byte)((current & ~mask) | (value & mask)));
        }

        private byte ReadRegister(Register register)
        {
            ReadRegisters(register, _registerValue);
            return _registerValue[0];
        }

        private void ReadRegisters(Register register, byte[] buffer)
        {
            EnsureNotDisposed();
            _registerAddress[0] = (byte)register;
            I2cTransferResult result = _i2cDevice.WriteRead(_registerAddress, buffer);
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new IOException();
            }
        }

        private void WriteRegister(Register register, byte value)
        {
            EnsureNotDisposed();
            _registerWrite[0] = (byte)register;
            _registerWrite[1] = value;
            I2cTransferResult result = _i2cDevice.Write(_registerWrite);
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new IOException();
            }
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
