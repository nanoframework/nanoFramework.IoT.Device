// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using nanoFramework.Hardware.Esp32;

namespace Iot.Device.Qmi8658.Samples
{
    /// <summary>
    /// QMI8658 sample for the Waveshare ESP32-S3-Touch-LCD-2.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The application entry point.
        /// </summary>
        public static void Main()
        {
            Configuration.SetPinFunction(48, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(47, DeviceFunction.I2C1_CLOCK);

            I2cConnectionSettings settings = new I2cConnectionSettings(1, Qmi8658AccelerometerGyroscope.SecondaryI2cAddress);
            using Qmi8658AccelerometerGyroscope imu = new Qmi8658AccelerometerGyroscope(new I2cDevice(settings));

            while (true)
            {
                if (imu.IsDataReady)
                {
                    imu.ReadAccelerationAndGyroscope(out Vector3 acceleration, out Vector3 angularRate);
                    Debug.WriteLine($"Acceleration (g): X={acceleration.X:F3}, Y={acceleration.Y:F3}, Z={acceleration.Z:F3}");
                    Debug.WriteLine($"Angular rate (dps): X={angularRate.X:F3}, Y={angularRate.Y:F3}, Z={angularRate.Z:F3}");
                    Debug.WriteLine($"Die temperature: {imu.GetInternalTemperature().DegreesCelsius:F2} C");
                }

                Thread.Sleep(100);
            }
        }
    }
}
