// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Device.Model;
using System.IO;
using System.Numerics;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Mpu6886
{
    /// <summary>
    /// Mpu6886 accelerometer and gyroscope
    /// </summary>
    [Interface("Mpu6886 accelerometer and gyroscope")]
    public class Mpu6886AccelerometerGyroscope : IDisposable
    {
        private I2cDevice _i2c;
        private const double GyroscopeResolution = 2000.0 / 32768.0; // default gyro scale 2000 dps
        private const double AccelerometerResolution = 8.0 / 32768.0; // default accelerometer res 8G
        /// <summary>
        /// Mpu6886 - Accelerometer and Gyroscope bus
        /// </summary>
        public Mpu6886AccelerometerGyroscope(
            I2cDevice i2cDevice)
        {
            _i2c = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

            SpanByte readBuffer = new byte[1];

            _i2c.WriteByte((byte)Mpu6886.Register.WhoAmI);
            _i2c.Read(readBuffer);
            if (readBuffer[0] != 0x19)
            {
                throw new IOException($"This device does not contain the correct signature 0x19 for a MPU6886");
            }

            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.PowerManagement1, 0b00000_00000 }));
            Thread.Sleep(10);

            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.PowerManagement1, 0b01000_0000 }));
            Thread.Sleep(10);

            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.PowerManagement1, 0b00000_00001 }));
            Thread.Sleep(10);

            // ACCEL_CONFIG(0x1c) : +-8G
            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.AccelerometerConfiguration1, 0b00001_00000 }));
            Thread.Sleep(1);

            // GYRO_CONFIG(0x1b) : +-2000dps
            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.GyroscopeConfiguration, 0b00001_10000 }));
            Thread.Sleep(1);

            // CONFIG(0x1a) 1khz output
            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.Configuration, 0b00000_00001 }));
            Thread.Sleep(1);

            // SMPLRT_DIV(0x19) 2 div, FIFO 500hz out
            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.SampleRateDevicer, 0b00000_00001 }));
            Thread.Sleep(1);

            // INT_ENABLE(0x38)
            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.InteruptEnable, 0b00000_00000 }));
            Thread.Sleep(1);

            // ACCEL_CONFIG 2(0x1d)
            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.AccelerometerConfiguration2, 0b00000_00000 }));
            Thread.Sleep(1);

            // USER_CTRL(0x6a)
            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.UserControl, 0b00000_00000 }));
            Thread.Sleep(1);

            // FIFO_EN(0x23)
            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.FifoEnable, 0b00000_00000 }));
            Thread.Sleep(1);

            // INT_PIN_CFG(0x37)
            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.IntPinBypassEnabled, 0b00010_00010 }));
            Thread.Sleep(1);

            // INT_ENABLE(0x38)
            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.InteruptEnable, 0b00000_00001 }));
            Thread.Sleep(100);

            //setGyroFsr
            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.GyroscopeConfiguration, 0b00001_10000 }));
            Thread.Sleep(10);

            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.AccelerometerConfiguration1, 0b00001_00000 }));
            Thread.Sleep(10);
        }

        private Vector3 GetRawAccelerometer()
        {
            SpanByte vec = new byte[6];
            Read(Mpu6886.Register.AccelerometerMeasurementXHighByte, vec);

            var x = (vec[0] << 8 | vec[1]);
            var y = (vec[2] << 8 | vec[3]);
            var z = (vec[4] << 8 | vec[5]);

            return new Vector3(x, y, z);
        }

        private Vector3 GetRawGyroscope()
        {
            SpanByte vec = new byte[6];
            Read(Mpu6886.Register.GyropscopeMeasurementXHighByte, vec);

            var x = (vec[0] << 8 | vec[1]);
            var y = (vec[2] << 8 | vec[3]);
            var z = (vec[4] << 8 | vec[5]);

            return new Vector3(x, y, z);
        }

        private int GetRawInternalTemperature()
        {
            SpanByte vec = new byte[2];
            Read(Mpu6886.Register.TemperatureMeasurementHighByte, vec);

            return (vec[0] << 8 | vec[1]); ;
        }

        /// <summary>
        /// Reads the current accelerometer values from the registers, and compensates them with the accelerometer resolution.
        /// </summary>
        /// <returns>Vector of acceleration</returns>
        public Vector3 GetAccelerometer() => GetRawAccelerometer() * AccelerometerResolution;

        /// <summary>
        /// Reads the current gyroscope values from the registers, and compensates them with the gyroscope resolution.
        /// </summary>
        /// <returns>Vector of the rotation</returns>
        public Vector3 GetGyroscope() => GetRawGyroscope() * GyroscopeResolution;

        /// <summary>
        /// Reads the register of the on-chip temperature sensor which represents the MPU-6886 die temperature.
        /// </summary>
        /// <returns>Temperature in degrees Celcius</returns>
        public Temperature GetInternalTemperature()
        {
            var rawInternalTemperature = GetRawInternalTemperature(); 
            
            // p43 of datasheet describes the room temp. compensation calcuation
            return new Temperature(rawInternalTemperature / 326.8 + 25.0, UnitsNet.Units.TemperatureUnit.DegreeCelsius);
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
            _i2c.WriteByte((byte)((byte)register));
            _i2c.Read(buffer);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2c?.Dispose();
            _i2c = null!;
        }
    }
}
