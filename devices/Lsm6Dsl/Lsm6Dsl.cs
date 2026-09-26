// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Device.Model;
using System.Numerics;
using UnitsNet;

namespace Iot.Device.Lsm6Dsl
{
    /// <summary>
    /// LSM6DSL three-axis accelerometer and gyroscope.
    /// </summary>
    [Interface("LSM6DSL three-axis accelerometer and gyroscope")]
    public class Lsm6Dsl : IDisposable
    {
        private const byte DeviceId = 0x6A;
        private const byte BlockDataUpdateAndAutoIncrement = 0x44;
        private I2cDevice _i2c;
        private AccelerationScale _accelerationScale;
        private AngularRateScale _angularRateScale;

        /// <summary>
        /// Default I2C address when the SA0 pin is low.
        /// </summary>
        public const byte DefaultI2cAddress = 0x6A;

        /// <summary>
        /// Alternate I2C address when the SA0 pin is high.
        /// </summary>
        public const byte SecondaryI2cAddress = 0x6B;

        /// <summary>
        /// Initializes a new instance of the <see cref="Lsm6Dsl"/> class.
        /// </summary>
        /// <param name="i2cDevice">I2C device.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is null.</exception>
        /// <exception cref="Exception">Thrown when the connected device is not an LSM6DSL.</exception>
        public Lsm6Dsl(I2cDevice i2cDevice)
        {
            _i2c = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

            if (ReadByte(Register.WhoAmI) != DeviceId)
            {
                throw new Exception("Device not found");
            }

            WriteByte(Register.Control3, BlockDataUpdateAndAutoIncrement);
            _accelerationScale = AccelerationScale.Scale02G;
            _angularRateScale = AngularRateScale.Scale0250Dps;
            WriteByte(Register.Control1Accelerometer, (byte)((byte)OutputDataRate.Rate104Hz << 4));
            WriteByte(Register.Control2Gyroscope, (byte)((byte)OutputDataRate.Rate104Hz << 4));
        }

        /// <summary>
        /// Gets or sets the accelerometer full-scale range. Invalid values default to <see cref="AccelerationScale.Scale02G"/>.
        /// </summary>
        [Property]
        public AccelerationScale AccelerationScale
        {
            get => _accelerationScale;
            set
            {
                if ((byte)value > (byte)AccelerationScale.Scale08G)
                {
                    value = AccelerationScale.Scale02G;
                }

                UpdateRegister(Register.Control1Accelerometer, 0x0C, (byte)((byte)value << 2));
                _accelerationScale = value;
            }
        }

        /// <summary>
        /// Gets or sets the gyroscope full-scale range. Invalid values default to <see cref="AngularRateScale.Scale0250Dps"/>.
        /// </summary>
        [Property]
        public AngularRateScale AngularRateScale
        {
            get => _angularRateScale;
            set
            {
                if (value != AngularRateScale.Scale0125Dps && value != AngularRateScale.Scale0250Dps &&
                    value != AngularRateScale.Scale0500Dps && value != AngularRateScale.Scale1000Dps &&
                    value != AngularRateScale.Scale2000Dps)
                {
                    value = AngularRateScale.Scale0250Dps;
                }

                UpdateRegister(Register.Control2Gyroscope, 0x0E, (byte)value);
                _angularRateScale = value;
            }
        }

        /// <summary>
        /// Gets or sets the accelerometer output data rate. Invalid values default to 104 Hz.
        /// </summary>
        [Property]
        public OutputDataRate AccelerationOutputDataRate
        {
            get => (OutputDataRate)((ReadByte(Register.Control1Accelerometer) >> 4) & 0x0F);
            set
            {
                if ((byte)value > (byte)OutputDataRate.Rate1Point66KHz)
                {
                    value = OutputDataRate.Rate104Hz;
                }

                UpdateRegister(Register.Control1Accelerometer, 0xF0, (byte)((byte)value << 4));
            }
        }

        /// <summary>
        /// Gets or sets the gyroscope output data rate. Invalid values default to 104 Hz.
        /// </summary>
        [Property]
        public OutputDataRate AngularRateOutputDataRate
        {
            get => (OutputDataRate)((ReadByte(Register.Control2Gyroscope) >> 4) & 0x0F);
            set
            {
                if ((byte)value > (byte)OutputDataRate.Rate1Point66KHz)
                {
                    value = OutputDataRate.Rate104Hz;
                }

                UpdateRegister(Register.Control2Gyroscope, 0xF0, (byte)((byte)value << 4));
            }
        }

        /// <summary>
        /// Gets a value indicating whether new accelerometer data is available.
        /// </summary>
        [Telemetry]
        public bool IsAccelerationDataReady => (ReadByte(Register.Status) & 0x01) != 0;

        /// <summary>
        /// Gets a value indicating whether new gyroscope data is available.
        /// </summary>
        [Telemetry]
        public bool IsAngularRateDataReady => (ReadByte(Register.Status) & 0x02) != 0;

        /// <summary>
        /// Gets acceleration for all axes, in g.
        /// </summary>
        [Telemetry]
        public Vector3 Acceleration => ReadVector(Register.AccelerometerXLow, GetAccelerationSensitivity() / 1000.0);

        /// <summary>
        /// Gets angular rate for all axes, in degrees per second.
        /// </summary>
        [Telemetry]
        public Vector3 AngularRate => ReadVector(Register.GyroscopeXLow, GetAngularRateSensitivity() / 1000.0);

        /// <summary>
        /// Gets the internal sensor temperature.
        /// </summary>
        [Telemetry]
        public Temperature Temperature
        {
            get
            {
                SpanByte data = new byte[2];
                Read(Register.TemperatureLow, data);
                short rawTemperature = BinaryPrimitives.ReadInt16LittleEndian(data);
                return Temperature.FromDegreesCelsius(25.0 + (rawTemperature / 256.0));
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2c?.Dispose();
            _i2c = null;
        }

        private Vector3 ReadVector(Register register, double multiplier)
        {
            SpanByte data = new byte[6];
            Read(register, data);

            return new Vector3(
                BinaryPrimitives.ReadInt16LittleEndian(data.Slice(0, 2)) * multiplier,
                BinaryPrimitives.ReadInt16LittleEndian(data.Slice(2, 2)) * multiplier,
                BinaryPrimitives.ReadInt16LittleEndian(data.Slice(4, 2)) * multiplier);
        }

        private double GetAccelerationSensitivity()
        {
            switch (_accelerationScale)
            {
                case AccelerationScale.Scale02G:
                    return 0.061;
                case AccelerationScale.Scale04G:
                    return 0.122;
                case AccelerationScale.Scale08G:
                    return 0.244;
                case AccelerationScale.Scale16G:
                    return 0.488;
                default:
                    return 0.061;
            }
        }

        private double GetAngularRateSensitivity()
        {
            switch (_angularRateScale)
            {
                case AngularRateScale.Scale0125Dps:
                    return 4.375;
                case AngularRateScale.Scale0250Dps:
                    return 8.75;
                case AngularRateScale.Scale0500Dps:
                    return 17.5;
                case AngularRateScale.Scale1000Dps:
                    return 35.0;
                case AngularRateScale.Scale2000Dps:
                    return 70.0;
                default:
                    return 8.75;
            }
        }

        private void UpdateRegister(Register register, byte mask, byte value)
        {
            byte currentValue = ReadByte(register);
            WriteByte(register, (byte)((currentValue & ~mask) | (value & mask)));
        }

        private void WriteByte(Register register, byte value)
        {
            SpanByte data = new byte[] { (byte)register, value };
            _i2c.Write(data);
        }

        private byte ReadByte(Register register)
        {
            _i2c.WriteByte((byte)register);
            return _i2c.ReadByte();
        }

        private void Read(Register register, SpanByte data)
        {
            _i2c.WriteByte((byte)register);
            _i2c.Read(data);
        }
    }
}