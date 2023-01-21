// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.DHTxx.Esp32;

namespace Dhtxx_sample
{
    public class Program
    {
        public static void Main()
        {
            // Set these values to test according to the list below:
            var pinEcho = 12;
            var pinTrigger = 14;
            var device = 5;
            // -----------------------------------------------------

            Debug.WriteLine("Hello DHT!");
            Debug.WriteLine("This is a sample program for the following sensors:");
            Debug.WriteLine(" 1. DHT10 on I2C");
            Debug.WriteLine(" 2. DHT11 on GPIO");
            Debug.WriteLine(" 3. DHT12 on GPIO");
            Debug.WriteLine(" 4. DHT21 on GPIO");
            Debug.WriteLine(" 5. DHT22 on GPIO");

            if (device == 0)
            {
                Debug.WriteLine("Set the device and pin in the source of this program before running.");
                return;
            }

            if (device == 1)
            {
                //////////////////////////////////////////////////////////////////////
                // when connecting to an ESP32 device, need to configure the I2C GPIOs
                // used for the bus
                //Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
                //Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

                // Init DHT10 through I2C
                Debug.WriteLine($"Using DHT10 on I2C.");
                I2cConnectionSettings settings = new(1, Dht10.DefaultI2cAddress);
                I2cDevice i2cDevice = I2cDevice.Create(settings);

                using Dht10 dht = new(i2cDevice);
                Dht(dht);
                return;
            }

            switch (device)
            {
                case 1:
                    // Init DHT10 through I2C
                    Debug.WriteLine($"Using DHT10 on I2C.");
                    I2cConnectionSettings settings = new(1, Dht10.DefaultI2cAddress);
                    I2cDevice i2cDevice = I2cDevice.Create(settings);
                    {
                        using Dht10 dht = new(i2cDevice);
                        {
                            Dht(dht);
                        }
                    }
                    return;

                case 2:
                    Debug.WriteLine($"Reading temperature and humidity on DHT11, pins {pinEcho} and {pinTrigger}");
                    using (Dht11 dht11 = new(pinEcho, pinTrigger))
                    {
                        Dht(dht11);
                    }

                    break;
                case 3:
                    Debug.WriteLine($"Reading temperature and humidity on DHT12, pins {pinEcho} and {pinTrigger}");
                    {
                        using (Dht12 dht12 = new(pinEcho, pinTrigger))
                            Dht(dht12);
                    }

                    break;
                case 4:
                    Debug.WriteLine($"Reading temperature and humidity on DHT21, pins {pinEcho} and {pinTrigger}");
                    using (Dht21 dht21 = new(pinEcho, pinTrigger))
                    {
                        Dht(dht21);
                    }

                    break;
                case 5:
                    Debug.WriteLine($"Reading temperature and humidity on DHT22, pins {pinEcho} and {pinTrigger}");
                    using (Dht22 dht22 = new(pinEcho, pinTrigger))
                    {
                        Dht(dht22);
                    }

                    break;
                default:
                    Debug.WriteLine("Please select one of the option.");
                    break;
            }
        }

        private static void Dht(DhtBase dht)
        {
            while (true)
            {
                var temp = dht.Temperature;
                var hum = dht.Humidity;
                // You can only display temperature and humidity if the read is successful otherwise, this will raise an exception as
                // both temperature and humidity are NAN
                if (dht.IsLastReadSuccessful)
                {
                    Debug.WriteLine($"Temperature: {temp.DegreesCelsius}\u00B0C, Relative humidity: {hum.Percent}%");
                }
                else
                {
                    Debug.WriteLine("Error reading DHT sensor");
                }

                // You must wait some time before trying to read the next value
                Thread.Sleep(2000);
            }
        }
    }
}
