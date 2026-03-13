// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Bmi270;
using Iot.Device.Magnetometer;
using System.Device.I2c;
using System.Diagnostics;
using nanoFramework.Hardware.Esp32;
using System.Threading;

namespace Bmi270Sample
{
    public class Program
    {
        public static void Main()
        {
            // I2C pins for the M5Stack CoreS3 (internal I2C bus)
            Configuration.SetPinFunction(12, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(11, DeviceFunction.I2C1_CLOCK);

            I2cConnectionSettings settings = new(1, Bmi270AccelerometerGyroscope.SecondaryI2cAddress);

            using (Bmi270AccelerometerGyroscope imu = new(I2cDevice.Create(settings)))
            {
                Debug.WriteLine("BMI270 initialized successfully!");

                // Enable auxiliary I2C for BMM150 magnetometer
                imu.EnableAuxiliaryI2c(Bmm150.SecondaryI2cAddress);
                Debug.WriteLine("Auxiliary I2C enabled for BMM150.");

                // Create BMM150 using the BMI270 auxiliary I2C bridge
                I2cConnectionSettings bmm150Settings = new(1, Bmi270AccelerometerGyroscope.SecondaryI2cAddress);
                using (Bmm150 mag = new(I2cDevice.Create(bmm150Settings), new Bmm150I2cBmi270(Bmm150.SecondaryI2cAddress)))
                {
                    Debug.WriteLine("BMM150 initialized successfully via BMI270 aux I2C!");

                    Debug.WriteLine("Start calibration ...");
                    var offset = imu.Calibrate(1000);
                    Debug.WriteLine($"Calibration done, calculated offsets X:{offset.X} Y:{offset.Y} Z:{offset.Z}");

                    Debug.WriteLine($"Internal temperature: {imu.GetInternalTemperature().DegreesCelsius} C");

                    while (true)
                    {
                        var acc = imu.GetAccelerometer();
                        var gyr = imu.GetGyroscope();
                        var magData = mag.ReadMagnetometer();
                        Debug.WriteLine($"Accelerometer data x:{acc.X} y:{acc.Y} z:{acc.Z}");
                        Debug.WriteLine($"Gyroscope data x:{gyr.X} y:{gyr.Y} z:{gyr.Z}");
                        Debug.WriteLine($"Magnetometer data x:{magData.X} y:{magData.Y} z:{magData.Z}\n");
                        Thread.Sleep(100);
                    }
                }
            }
        }
    }
}
