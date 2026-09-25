// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Device.Model;
using System.Numerics;
using UnitsNet;

namespace Iot.Device.Lis3Mdl
{
    /// <summary>
    /// LIS3MDL low-power, high-performance three-axis magnetometer.
    /// </summary>
    [Interface("LIS3MDL low-power, high-performance three-axis magnetometer")]
    public class Lis3Mdl : IDisposable
    {
        private const byte DeviceId = 0x3D;
        private const byte TemperatureEnabled = 0x80;
        private const byte BlockDataUpdateEnabled = 0x40;
        private I2cDevice _i2c;
        private MagneticInductionScale _magneticInductionScale;

        /// <summary>
        /// Default I2C address when the SDO/SA1 pin is low.
        /// </summary>
        public const byte DefaultI2cAddress = 0x1C;

        /// <summary>
        /// Alternate I2C address when the SDO/SA1 pin is high.
        /// </summary>
        public const byte SecondaryI2cAddress = 0x1E;

        /// <summary>
        /// Initializes a new instance of the <see cref="Lis3Mdl"/> class.
        /// </summary>
        /// <param name="i2cDevice">I2C device.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is null.</exception>
        /// <exception cref="Exception">Thrown when the connected device is not a LIS3MDL.</exception>
        public Lis3Mdl(I2cDevice i2cDevice)
        {
            _i2c = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

            if (ReadByte(Register.WhoAmI) != DeviceId)
            {
                throw new Exception("Device not found");
            }

            _magneticInductionScale = MagneticInductionScale.Scale04G;
            WriteByte(Register.Control1, (byte)(TemperatureEnabled | ((byte)PerformanceMode.UltraHigh << 5) | ((byte)DataRate.Rate10Hz << 1)));
            WriteByte(Register.Control2, 0x00);
            WriteByte(Register.Control3, (byte)OperationMode.Continuous);
            WriteByte(Register.Control4, (byte)((byte)PerformanceMode.UltraHigh << 2));
            WriteByte(Register.Control5, BlockDataUpdateEnabled);
        }

        /// <summary>
        /// Gets or sets the magnetic induction full-scale range. Invalid values default to <see cref="MagneticInductionScale.Scale04G"/>.
        /// </summary>
        [Property]
        public MagneticInductionScale MagneticInductionScale
        {
            get => _magneticInductionScale;
            set
            {
                if ((byte)value > (byte)MagneticInductionScale.Scale16G)
                {
                    value = MagneticInductionScale.Scale04G;
                }

                UpdateRegister(Register.Control2, 0x60, (byte)((byte)value << 5));
                _magneticInductionScale = value;
            }
        }

        /// <summary>
        /// Gets or sets the output data rate. Default for wrong values is Rate10Hz.
        /// </summary>
        [Property]
        public DataRate DataRate
        {
            get => (DataRate)((ReadByte(Register.Control1) & 0x1E) >> 1);
            set
            {
                if (value == DataRate.Rate155Hz)
                {
                    PerformanceMode = PerformanceMode.UltraHigh;
                }
                else if (value == DataRate.Rate300Hz)
                {
                    PerformanceMode = PerformanceMode.High;
                }
                else if (value == DataRate.Rate560Hz)
                {
                    PerformanceMode = PerformanceMode.Medium;
                }
                else
                {
                    PerformanceMode = PerformanceMode.LowPower;
                }                

                UpdateRegister(Register.Control1, 0x1E, (byte)((byte)value << 1));
            }
        }

        /// <summary>
        /// Gets or sets the performance mode for all axes. Invalid values default to <see cref="PerformanceMode.LowPower"/>.
        /// </summary>
        [Property]
        public PerformanceMode PerformanceMode
        {
            get => (PerformanceMode)((ReadByte(Register.Control1) >> 5) & 0x03);
            set
            {
                if ((byte)value > (byte)PerformanceMode.UltraHigh)
                {
                    value = PerformanceMode.LowPower;
                }

                UpdateRegister(Register.Control1, 0x60, (byte)((byte)value << 5));
                UpdateRegister(Register.Control4, 0x0C, (byte)((byte)value << 2));
            }
        }

        /// <summary>
        /// Gets or sets the magnetic field conversion mode. Invalid values default to <see cref="OperationMode.Continuous"/>.
        /// </summary>
        [Property]
        public OperationMode OperationMode
        {
            get => (OperationMode)(ReadByte(Register.Control3) & 0x03);
            set
            {
                if (value != OperationMode.Continuous && value != OperationMode.Single && value != OperationMode.PowerDown)
                {
                    value = OperationMode.Continuous;
                }

                UpdateRegister(Register.Control3, 0x03, (byte)value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether a new magnetic field sample is available.
        /// </summary>
        [Telemetry]
        public bool IsDataReady => (ReadByte(Register.Status) & 0x08) != 0;

        /// <summary>
        /// Gets the magnetic field reading for the X axis.
        /// </summary>
        [Telemetry]
        public UnitsNet.MagneticField MagneticFieldX => UnitsNet.MagneticField.FromMilligausses(MagneticField.X);

        /// <summary>
        /// Gets the magnetic field reading for the Y axis.
        /// </summary>
        [Telemetry]
        public UnitsNet.MagneticField MagneticFieldY => UnitsNet.MagneticField.FromMilligausses(MagneticField.Y);

        /// <summary>
        /// Gets the magnetic field reading for the Z axis.
        /// </summary>
        [Telemetry]
        public UnitsNet.MagneticField MagneticFieldZ => UnitsNet.MagneticField.FromMilligausses(MagneticField.Z);

        /// <summary>
        /// Gets the magnetic field for all axes, in milligauss.
        /// </summary>
        [Telemetry]
        public Vector3 MagneticField
        {
            get
            {
                SpanByte data = new byte[6];
                Read(Register.OutputXLow, data);
                float milligaussPerLsb = 1000.0f / GetSensitivity();

                return new Vector3(
                    BinaryPrimitives.ReadInt16LittleEndian(data.Slice(0, 2)) * milligaussPerLsb,
                    BinaryPrimitives.ReadInt16LittleEndian(data.Slice(2, 2)) * milligaussPerLsb,
                    BinaryPrimitives.ReadInt16LittleEndian(data.Slice(4, 2)) * milligaussPerLsb);
            }
        }

        /// <summary>
        /// Gets the sensor temperature.
        /// </summary>
        [Telemetry]
        public Temperature Temperature
        {
            get
            {
                SpanByte data = new byte[2];
                Read(Register.TemperatureLow, data);
                short rawTemperature = BinaryPrimitives.ReadInt16LittleEndian(data);
                return Temperature.FromDegreesCelsius(25.0f + (rawTemperature / 8.0f));
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2c?.Dispose();
            _i2c = null;
        }

        private float GetSensitivity()
        {
            switch (_magneticInductionScale)
            {
                case MagneticInductionScale.Scale04G:
                    return 6842.0f;
                case MagneticInductionScale.Scale08G:
                    return 3421.0f;
                case MagneticInductionScale.Scale12G:
                    return 2281.0f;
                case MagneticInductionScale.Scale16G:
                    return 1711.0f;
                default:
                    throw new ArgumentException(nameof(_magneticInductionScale));
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