// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Mpu6886;
using System.Device.I2c;
using System.Diagnostics;
using nanoFramework.Hardware.Esp32;
using System.Threading;
using System;

namespace mpu8668test
{
    public class Program
    {
        public static void Main()
        {
            // I2C pins need to be configured, for example for pin 22 & 21 for 
            // the M5StickC Plus. These pins might be different for other boards.
            Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
            Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);

            I2cConnectionSettings settings = new(1, Mpu6886AccelerometerGyroscope.DefaultI2cAddress);

            using (Mpu6886AccelerometerGyroscope ag = new(I2cDevice.Create(settings)))
            {
                Debug.WriteLine("Start calibration ...");
                var offset = ag.Calibrate(1000);
                Debug.WriteLine($"Calibration done, calculated offsets {offset.X} {offset.Y} {offset.Y}");

                Debug.WriteLine($"Internal temperature: {ag.GetInternalTemperature().DegreesCelsius} C");

                while (true)
                {
                    var acc = ag.GetAccelerometer();
                    var gyr = ag.GetGyroscope();
                    Debug.WriteLine($"Accelerometer data x:{acc.X} y:{acc.Y} z:{acc.Z}");
                    Debug.WriteLine($"Gyroscope data x:{gyr.X} y:{gyr.Y} z:{gyr.Z}\n");
                    Thread.Sleep(100);
                }
            }
        }
    }
}