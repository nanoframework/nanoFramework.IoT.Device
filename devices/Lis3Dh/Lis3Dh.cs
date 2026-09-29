// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Model;
using System.Numerics;

namespace Iot.Device.Lis3DhAccelerometer
{
    /// <summary>
    /// LIS3DH ultra-low-power, high-performance three-axis accelerometer.
    /// </summary>
    [Interface("LIS3DH accelerometer")]
    public abstract class Lis3Dh : IDisposable
    {
        /// <summary>
        /// Default I2C address when the SDO/SA0 pin is low.
        /// </summary>
        public const byte DefaultI2cAddress = 0x18;

        /// <summary>
        /// Alternate I2C address when the SDO/SA0 pin is high.
        /// </summary>
        public const byte SecondaryI2cAddress = 0x19;

        private const byte DeviceId = 0x33;
        private const byte BlockDataUpdateEnabled = 0x80;
        private const int Max = 1 << 15;

        private DataRate _dataRate;
        private OperatingMode _operatingMode;
        private AccelerationScale _accelerationScale;

        /// <summary>
        /// Gets or sets the output data rate.
        /// </summary>
        [Property]
        public DataRate DataRate
        {
            get => _dataRate;
            set => ChangeSettings(value, _operatingMode, _accelerationScale);
        }

        /// <summary>
        /// Gets or sets the operating mode.
        /// </summary>
        [Property]
        public OperatingMode OperatingMode
        {
            get => _operatingMode;
            set => ChangeSettings(_dataRate, value, _accelerationScale);
        }

        /// <summary>
        /// Gets or sets the acceleration full-scale range.
        /// </summary>
        [Property]
        public AccelerationScale AccelerationScale
        {
            get => _accelerationScale;
            set => ChangeSettings(_dataRate, _operatingMode, value);
        }

        /// <summary>
        /// Gets I2C address depending on SDO/SA0 pin.
        /// </summary>
        /// <param name="sdoPinValue">SDO pin value. The pin may also be called SA0.</param>
        /// <returns>The selected I2C address.</returns>
        public static byte GetI2cAddress(PinValue sdoPinValue)
            => sdoPinValue == PinValue.High ? SecondaryI2cAddress : DefaultI2cAddress;

        /// <summary>
        /// Creates and initializes a LIS3DH instance using an I2C device.
        /// </summary>
        /// <param name="i2cDevice">I2C device.</param>
        /// <param name="dataRate">Output data rate.</param>
        /// <param name="operatingMode">Operating mode.</param>
        /// <param name="accelerationScale">Acceleration full-scale range.</param>
        /// <returns>An initialized LIS3DH instance.</returns>
        public static Lis3Dh Create(I2cDevice i2cDevice, DataRate dataRate = DataRate.DataRate100Hz, OperatingMode operatingMode = OperatingMode.HighResolutionMode, AccelerationScale accelerationScale = AccelerationScale.Scale04G)
            => new Lis3DhI2c(i2cDevice).Initialize(dataRate, operatingMode, accelerationScale);

        internal Lis3Dh()
        {
        }

        internal abstract void WriteRegister(Register register, byte data, bool autoIncrement = true);

        internal abstract void ReadRegister(Register register, SpanByte data, bool autoIncrement = true);

        /// <summary>
        /// Gets the acceleration measured in g for all three axes.
        /// </summary>
        [Telemetry]
        public Vector3 Acceleration
        {
            get
            {
                Vector3 rawAcceleration = ReadRawAcceleration();
                float divisor = GetAccelerationDivisor();
                return new Vector3(rawAcceleration.X / divisor, rawAcceleration.Y / divisor, rawAcceleration.Z / divisor);
            }
        }

        /// <inheritdoc/>
        public abstract void Dispose();

        private void ResetUnusedSettings()
        {
            // Reset to default values

            // msb: pull-up on SA0, remainder must be hard-coded
            this[Register.CTRL_REG0] = 0b00010000;

            // disable temperature sensor and ADC
            this[Register.TEMP_CFG_REG] = 0b00000000;

            // CTRL_REG1 is handled by ChangeSettings (DataRate and low-power mode bit)

            // high-pass filter settings default
            this[Register.CTRL_REG2] = 0b00000000;

            // interrupts settings to default
            this[Register.CTRL_REG3] = 0b00000000;

            // CTRL_REG4 is handled by ChangeSettings (AccelerationScale and high-resolution mode bit)

            // reboot, FIFO, interrupt settings to default
            this[Register.CTRL_REG5] = 0b00000000;

            // other boot and interrupt settings to default
            this[Register.CTRL_REG6] = 0b00000000;
        }

        private void ChangeSettings(DataRate dataRate, OperatingMode operatingMode, AccelerationScale accelerationScale)
        {
            if (dataRate == DataRate.LowPowerMode1600Hz && operatingMode != OperatingMode.LowPowerMode)
            {
                throw new ArgumentException("The 1.6 kHz data rate is available only in low-power mode.");
            }

            byte dataRateBits = (byte)((byte)dataRate << 4);
            byte lowPowerModeBitAndAxesEnable = (byte)(operatingMode == OperatingMode.LowPowerMode ? 0b1111 : 0b0111);
            this[Register.CTRL_REG1] = (byte)(dataRateBits | lowPowerModeBitAndAxesEnable);

            // Enable block data update; leave endianness, self-test, and SPI settings at their defaults
            byte fullScaleBits = (byte)((byte)accelerationScale << 4);
            byte highResolutionModeBit = (byte)(operatingMode == OperatingMode.HighResolutionMode ? 0b1000 : 0b0000);
            this[Register.CTRL_REG4] = (byte)(BlockDataUpdateEnabled | fullScaleBits | highResolutionModeBit);

            _dataRate = dataRate;
            _operatingMode = operatingMode;
            _accelerationScale = accelerationScale;
        }

        private Lis3Dh Initialize(DataRate dataRate, OperatingMode operatingMode, AccelerationScale accelerationScale)
        {
            SpanByte deviceId = new byte[1];
            ReadRegister(Register.WHO_AM_I, deviceId, false);
            if (deviceId[0] != DeviceId)
            {
                throw new Exception("Device is not a LIS3DH.");
            }

            ResetUnusedSettings();
            ChangeSettings(dataRate, operatingMode, accelerationScale);

            // return this to simplify syntax
            return this;
        }

        private Vector3 ReadRawAcceleration()
        {
            SpanByte vec = new byte[6];
            ReadRegister(Register.OUT_X_L, vec);

            short x = BinaryPrimitives.ReadInt16LittleEndian(vec.Slice(0, 2));
            short y = BinaryPrimitives.ReadInt16LittleEndian(vec.Slice(2, 2));
            short z = BinaryPrimitives.ReadInt16LittleEndian(vec.Slice(4, 2));
            return new Vector3(x, y, z);
        }

        private float GetAccelerationDivisor()
        {
            switch (_accelerationScale)
            {
                case AccelerationScale.Scale02G:
                    return Max / 2;
                case AccelerationScale.Scale04G:
                    return Max / 4;
                case AccelerationScale.Scale08G:
                    return Max / 8;
                case AccelerationScale.Scale16G:
                    return Max / 24;
                default:
                    throw new ArgumentException("Value is unknown.", nameof(_accelerationScale));
            }
        }

        private byte this[Register register]
        {
            get
            {
                SpanByte reg = new byte[1];
                ReadRegister(register, reg);
                return reg[0];
            }

            set
            {
                WriteRegister(register, value);
            }
        }
    }
}
