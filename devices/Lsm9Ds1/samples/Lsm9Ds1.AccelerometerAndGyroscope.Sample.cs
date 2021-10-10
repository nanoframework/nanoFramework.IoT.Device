// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Device.I2c;
using System.Diagnostics;

namespace Iot.Device.Lsm9Ds1.Samples
{
    internal class AccelerometerAndGyroscope
    {
        public const int I2cAddress = 0x6A;

        public static void Run()
        {
            //////////////////////////////////////////////////////////////////////
            // when connecting to an ESP32 device, need to configure the I2C GPIOs
            // used for the bus
            //Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
            //Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

            using Lsm9Ds1AccelerometerAndGyroscope ag = new(CreateI2cDevice());
            while (true)
            {
                Debug.WriteLine($"Acceleration={ag.Acceleration}");
                Debug.WriteLine($"AngularRate={ag.AngularRate}");
                Thread.Sleep(100);
            }
        }

        private static I2cDevice CreateI2cDevice()
        {
            I2cConnectionSettings settings = new(1, I2cAddress);
            return I2cDevice.Create(settings);
        }
    }
}
