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

            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.PowerManagement1, 0b0000_0000 }));
            Thread.Sleep(10);

            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.PowerManagement1, 0b0100_0000 }));
            Thread.Sleep(10);

            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.PowerManagement1, 0b0000_0001 }));
            Thread.Sleep(10);

            // ACCEL_CONFIG(0x1c) : +-8G
            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.AccelerometerConfiguration1, 0b0001_0000 }));
            Thread.Sleep(1);

            // GYRO_CONFIG(0x1b) : +-2000dps
            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.GyroscopeConfiguration, 0b0001_1000 }));
            Thread.Sleep(1);

            // CONFIG(0x1a) 1khz output
            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.Configuration, 0b0000_0001 }));
            Thread.Sleep(1);

            // SMPLRT_DIV(0x19) 2 div, FIFO 500hz out
            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.SampleRateDevicer, 0b0000_0001 }));
            Thread.Sleep(1);

            // INT_ENABLE(0x38)
            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.InteruptEnable, 0b0000_0000 }));
            Thread.Sleep(1);

            // ACCEL_CONFIG 2(0x1d)
            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.AccelerometerConfiguration2, 0b0000_0000 }));
            Thread.Sleep(1);

            // USER_CTRL(0x6a)
            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.UserControl, 0b0000_0000 }));
            Thread.Sleep(1);

            // FIFO_EN(0x23)
            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.FifoEnable, 0b0000_0000 }));
            Thread.Sleep(1);

            // INT_PIN_CFG(0x37)
            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.IntPinBypassEnabled, 0b0010_0010 }));
            Thread.Sleep(1);

            // INT_ENABLE(0x38)
            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.InteruptEnable, 0b0000_0001 }));
            Thread.Sleep(100);

            //setGyroFsr
            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.GyroscopeConfiguration, 0b0001_1000 }));
            Thread.Sleep(10);

            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.AccelerometerConfiguration1, 0b0001_0000 }));
            Thread.Sleep(10);

            _i2c.Write(new SpanByte(new byte[] { (byte)Mpu6886.Register.AccelerometerIntelligenceControl, 0b0000_0010 }));
            Thread.Sleep(10);
        }

        private Vector3 GetRawAccelerometer()
        {
            SpanByte vec = new byte[6];
            Read(Mpu6886.Register.AccelerometerMeasurementXHighByte, vec);

            short x = (short)(vec[0] << 8 | vec[1]);
            short y = (short)(vec[2] << 8 | vec[3]);
            short z = (short)(vec[4] << 8 | vec[5]);

            return new Vector3(x, y, z);
        }

        private Vector3 GetRawGyroscope()
        {
            SpanByte vec = new byte[6];
            Read(Mpu6886.Register.GyropscopeMeasurementXHighByte, vec);

            short x = (short)(vec[0] << 8 | vec[1]);
            short y = (short)(vec[2] << 8 | vec[3]);
            short z = (short)(vec[4] << 8 | vec[5]);

            return new Vector3(x, y, z);
        }

        private short GetRawInternalTemperature()
        {
            SpanByte vec = new byte[2];
            Read(Mpu6886.Register.TemperatureMeasurementHighByte, vec);

            return (short)(vec[0] << 8 | vec[1]); ;
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

        /// <summary>
        /// Calibrate the gyroscope by calculating the offset values and storing them in the GyroscopeOffsetAdjustment registers of the MPU6886.
        /// </summary>
        /// <param name="iterations">The number of sample gyroscope values to read</param>
        /// <returns>The calulated offset vector</returns>
        public Vector3 Calibrate(int iterations)
        {
            SetGyroscopeOffset(new Vector3(0, 0, 0));
            Thread.Sleep(2); 

            var gyrSum = new double[3];

            for (int i = 0; i < iterations; i++)
            {
                var gyr = GetRawGyroscope();

                gyrSum[0] += gyr.X;
                gyrSum[1] += gyr.Y;
                gyrSum[2] += gyr.Z;
                
                Thread.Sleep(2);
            }

            Vector3 offset = new Vector3(gyrSum[0] / iterations, gyrSum[1] / iterations, gyrSum[2] / iterations) ;

            SetGyroscopeOffset(offset);

            return offset;
        }

        /// <summary>
        /// Write the gyroscope offset in the GyroscopeOffsetAdjustment registers of the MPU6886.
        /// This function can be usefull when a custom callibration calculation is used, instead of the Calibrate function of this class.
        /// </summary>
        /// <param name="offset">The vector containing the offsets for the 3 axises.</param>
        public void SetGyroscopeOffset(Vector3 offset)
        {
            SpanByte registerAndOffset = new byte[7];
            SpanByte offsetbyte = new byte[2];

            registerAndOffset[0] = (byte)Mpu6886.Register.GyroscopeOffsetAdjustmentXHighByte;

            BinaryPrimitives.WriteInt16BigEndian(offsetbyte, (short)offset.X);
            registerAndOffset[1] = offsetbyte[0];
            registerAndOffset[2] = offsetbyte[1];

            BinaryPrimitives.WriteInt16BigEndian(offsetbyte, (short)offset.Y);
            registerAndOffset[3] = offsetbyte[0];
            registerAndOffset[4] = offsetbyte[1];

            BinaryPrimitives.WriteInt16BigEndian(offsetbyte, (short)offset.Z);
            registerAndOffset[5] = offsetbyte[0];
            registerAndOffset[6] = offsetbyte[1];

            _i2c.Write(registerAndOffset);
        }

        /// <summary>
        /// Read the gyroscope offset from the GyroscopeOffsetAdjustment registers of the MPU6886.
        /// </summary>
        /// <returns>The vector containing the offsets for the 3 axises.</returns>
        public Vector3 GetGyroscopeOffset()
        {
            SpanByte vec = new byte[6];
            Read(Mpu6886.Register.GyroscopeOffsetAdjustmentXHighByte, vec);

            Vector3 v = new Vector3();
            v.X = (short)(vec[0] << 8 | vec[1]);
            v.Y = (short)(vec[2] << 8 | vec[3]);
            v.Z = (short)(vec[4] << 8 | vec[5]);

            return v;
        }
    }
}
