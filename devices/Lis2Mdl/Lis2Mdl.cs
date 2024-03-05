// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Device.Model;
using System.Numerics;
using UnitsNet;

namespace Iot.Device.Lis2Mdl
{
    /// <summary>
    /// Class for the LIS2MDL magnetometer.
    /// </summary>
    [Interface("Lis2Mdl - Ultra-low-power, high-performance 3-axis digital magnetic sensor")]
    public class Lis2Mdl : IDisposable
    {
        /// <summary>
        /// Configuration for enabling temperature compensation.
        /// </summary>
        private const byte ConfATemperatureCompensation = 0x80;

        /// <summary>
        /// Configuration for enabling block data read.
        /// </summary>
        private const byte ConfCBDU = 0x10;

        /// <summary>
        /// Device ID when reading the WHO_AM_I register.
        /// </summary>
        private const byte DeviceId = 0x40;

        private I2cDevice _i2c;

        /// <summary>
        /// This holds the magnetic field reading from the sensor.
        /// Elements index is 0 for X, 1 for Y and 2 for Z.
        /// </summary>
        private float[] _magneticFieldReadings = new float[3];

        /// <summary>
        /// Device I2C Address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x1E;

        /// <summary>
        /// Initializes a new instance of the <see cref="Lis2Mdl" /> class.
        /// </summary>
        /// <param name="i2cDevice">I2C device.</param>
        /// <exception cref="ArgumentNullException" >If the provided I2C device is <see langword="null"/>.</exception>
        /// <exception cref="Exception">If the device is not found.</exception>
        public Lis2Mdl(I2cDevice i2cDevice)
        {
            _i2c = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

            // check if the device is present
            var id = Read(Register.WhoAmI);
            if (id != DeviceId)
            {
                throw new Exception("Device not found");
            }

            InitializeRegisters();
        }

        /// <summary>
        /// Temperature reading.
        /// </summary>
        [Telemetry]
        public Temperature Temperature => Temperature.FromDegreesCelsius(GetTemperature());

        /// <summary>
        /// Gets the magnetic field reading for X axis.
        /// </summary>
        [Telemetry]
        public MagneticField MagneticFieldX
        {
            get
            {
                GetMagneticField();

                return UnitsNet.MagneticField.FromMilligausses(_magneticFieldReadings[0]);
            }
        }

        /// <summary>
        /// Gets the magnetic field reading for Y axis.
        /// </summary>
        [Telemetry]
        public MagneticField MagneticFieldY
        {
            get
            {
                GetMagneticField();

                return UnitsNet.MagneticField.FromMilligausses(_magneticFieldReadings[1]);
            }
        }

        /// <summary>
        /// Gets the magnetic field reading for Z axis.
        /// </summary>
        [Telemetry]
        public MagneticField MagneticFieldZ
        {
            get
            {
                GetMagneticField();

                return UnitsNet.MagneticField.FromMilligausses(_magneticFieldReadings[2]);
            }
        }

        /// <summary>
        /// Gets the magnetic field reading for all axes.
        /// Values are in milligauss.
        /// </summary>
        public Vector3 MagneticField
        {
            get
            {
                GetMagneticField();

                return new Vector3
                {
                    X = _magneticFieldReadings[0],
                    Y = _magneticFieldReadings[1],
                    Z = _magneticFieldReadings[2]
                };
            }
        }

        private void WriteByte(Register register, byte data)
        {
            SpanByte buff = new byte[2]
            {
                (byte)register,
                data
            };

            _i2c.Write(buff);
        }

        private short ReadInt16(Register register)
        {
            SpanByte val = new byte[2];
            Read(register, val);
            return BinaryPrimitives.ReadInt16LittleEndian(val);
        }

        private void Read(Register register, SpanByte buffer)
        {
            _i2c.WriteByte((byte)register);
            _i2c.Read(buffer);
        }

        private byte Read(Register register)
        {
            _i2c.WriteByte((byte)register);
            return _i2c.ReadByte();
        }

        private float GetTemperature()
        {
            // read the 2 raw data registers into data array      
            var tempRaw = ReadInt16(Register.TemperatureLow);

            // from data sheet: 8 LSB /�C in 12bit resolution
            return (tempRaw / 16.0f / 8.0f) + 25.0f;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_i2c != null)
            {
                _i2c?.Dispose();
                _i2c = null;
            }
        }

        private void InitializeRegisters()
        {
            // Temperature compensation enabled, ODR = 10Hz, continuous and high-resolution modes
            WriteByte(Register.ConfigA, ConfATemperatureCompensation);

            // enable block data read (bit 4 == 1)
            WriteByte(Register.ConfigC, ConfCBDU);
        }

        private void GetMagneticField()
        {
            // read the output registers into data array
            // 3 registers for X, Y and Z, each 16bit
            SpanByte magneticFieldRaw = new byte[3 * 2];
            Read(Register.OutputXLow, magneticFieldRaw);

            _magneticFieldReadings[0] = BinaryPrimitives.ReadInt16LittleEndian(magneticFieldRaw.Slice(0, 2));
            _magneticFieldReadings[1] = BinaryPrimitives.ReadInt16LittleEndian(magneticFieldRaw.Slice(2, 2));
            _magneticFieldReadings[2] = BinaryPrimitives.ReadInt16LittleEndian(magneticFieldRaw.Slice(4, 2));
        }
    }
}
