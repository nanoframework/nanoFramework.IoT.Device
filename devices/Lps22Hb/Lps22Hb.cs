// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Device.Model;
using UnitsNet;

namespace Iot.Device.Lps22Hb
{
    /// <summary>
    /// Class for the LIS2MDL magnetometer.
    /// </summary>
    [Interface("Lps22Hb - MEMS nano pressure sensor: 260-1260 hPa absolute digital output barometer")]
    public class Lps22Hb : IDisposable
    {
        private const float PressureScale = 4096.0f;
        private const float TemperatureScale = 100.0f;

        /// <summary>
        /// Configuration: ODR = 25 Hz(continuous mode), LPF active with ODR/9, BDU activ.
        /// </summary>
        private const int ConfigFifoBypass = 0x3A;

        /// <summary>
        /// Device ID when reading the WHO_AM_I register.
        /// </summary>
        public const byte DefaultI2cAddress = 0x5C;

        // storage for raw data
        private readonly byte[] _dataRaw;

        private I2cDevice _i2c;

        /// <summary>
        /// This holds the magnetic field reading from the sensor.
        /// Elements index is 0 for X, 1 for Y and 2 for Z.
        /// </summary>
        private float[] _magneticFieldReadings = new float[3];

        /// <summary>
        /// Device I2C Address.
        /// </summary>
        public const byte I2cAddress = 0x1E;

        /// <summary>
        /// Initializes a new instance of the <see cref="Lps22Hb" /> class.
        /// </summary>
        /// <param name="i2cDevice">I2C device.</param>
        /// <param name="mode">FIFO mode selection.</param>
        /// <exception cref="ArgumentNullException" >If the provided I2C device is <see langword="null"/>.</exception>
        /// <exception cref="Exception">If the device is not found.</exception>
        public Lps22Hb(
            I2cDevice i2cDevice,
            FifoMode mode)
        {
            _i2c = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

            // check if the device is present
            var id = Read(Register.WhoAmI);
            if (id != DefaultI2cAddress)
            {
                throw new Exception("Device not found");
            }

            InitializeRegisters(mode);

            // instantiate the raw data array
            _dataRaw = new byte[3];
        }

        /// <summary>
        /// Temperature reading.
        /// </summary>
        [Telemetry]
        public Temperature Temperature => Temperature.FromDegreesCelsius(GetTemperature());

        /// <summary>
        /// Pressure reading.
        /// </summary>
        [Telemetry]
        public Pressure Pressure => Pressure.FromHectopascals(GePressure());

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
            var tempRaw = ReadInt16(Register.TemperatureOutL);

            // Convert tempRaw from two's complement to signed integer
            short raw = tempRaw;

            return raw / TemperatureScale;
        }

        private float GePressure()
        {
            Read(Register.PressureOutXl, _dataRaw);

            int raw = ((_dataRaw[2] << 16) | (_dataRaw[1] << 8)) | _dataRaw[0];

            if ((raw & 0x800000) == 0x800000)
            {
                raw = -(0x1000000 - raw);
            }

            return raw / PressureScale;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_i2c != null)
            {
                // Clear register to default value (Power off)
                WriteByte(Register.ControlRegister1, 0x00);

                _i2c?.Dispose();
                _i2c = null;
            }
        }

        private void InitializeRegisters(FifoMode mode)
        {
            switch (mode)
            {
                case FifoMode.Bypass:
                    // ODR = 25 Hz(continuous mode), LPF active with ODR/9, BDU activ
                    WriteByte(Register.ControlRegister1, ConfigFifoBypass);

                    // FIFO OFF and Multiple reading ON 
                    WriteByte(Register.ControlRegister2, 0x10);

                    break;

                case FifoMode.FIFO:
                case FifoMode.Stream:
                case FifoMode.StreamToFifo:
                case FifoMode.BypassToStream:
                case FifoMode.BypassToFifo:
                case FifoMode.DynamicStream:
                default:
                    // not implemented
                    throw new NotImplementedException();
            }
        }
    }
}
