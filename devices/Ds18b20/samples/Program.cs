// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
using nanoFramework.Device.OneWire;
using nanoFramework.Hardware.Esp32;
using UnitsNet;

namespace Iot.Device.Ds18b20.Samples
{
    public class Program
    {
        public static void Main()
        {
            if (!Debugger.IsAttached)
            {
                Console.WriteLine("App stopped because not connected to debugger. Remove this code when running outside VS");
                Thread.Sleep(Timeout.Infinite);
            }

            Console.WriteLine("Hello from Ds18b20!");
            Configuration.SetPinFunction(16, DeviceFunction.COM3_RX);
            Configuration.SetPinFunction(17, DeviceFunction.COM3_TX);
            ReadingFromOneSensor();
        }

        private static void UsingAlarms()
        {
            using OneWireHost oneWire = new OneWireHost();

            Ds18b20 ds18b20 = new Ds18b20(oneWire, null, false, TemperatureResolution.VeryHigh);

            if (ds18b20.Initialize())
            {
                for (int i = 0; i < ds18b20.AddressNet.Length; i++)
                {
                    string devAddrStr = "";
                    ds18b20.Address = ds18b20.AddressNet[i];

                    foreach (var addrByte in ds18b20.AddressNet[i])
                    {
                        devAddrStr += addrByte.ToString("X2");
                    }

                    Console.WriteLine("18b20-" + i.ToString("X2") + " " + devAddrStr);

                    ds18b20.ConfigurationRead(false);
                    Console.WriteLine("Alarm set-points before changes:");
                    Console.WriteLine("Hi alarm = " + ds18b20.TemperatureHighAlarm.DegreesCelsius + " C");
                    Console.WriteLine("Lo alarm = " + ds18b20.TemperatureLowAlarm.DegreesCelsius + " C");
                    SetAlarmSetting();
                }
                alarmSearch();
            }
            else
            {
                Console.WriteLine("No devices found.");
            }

            oneWire.Dispose();

            void alarmSearch()
            {
                int loopRead = 1000;
                ds18b20.IsAlarmSearchCommandEnabled = true;

                while (loopRead > 0)
                {
                    Console.WriteLine("LoopRead " + loopRead);

                    if (ds18b20.SearchForAlarmCondition())
                    {
                        for (int index = 0; index < ds18b20.AddressNet.Length; index++)
                        {
                            ds18b20.Address = ds18b20.AddressNet[index];
                            if (ds18b20.TryReadTemperature(out var currentTemperature))
                            {
                                break;
                            }

                            string devAddrStr = "";
                            foreach (var addrByte in ds18b20.AddressNet[index]) devAddrStr += addrByte.ToString("X2");
                            Console.WriteLine("DS18B20[" + devAddrStr + "] Sensor reading in One-Shot-mode; T = " + currentTemperature.DegreesCelsius.ToString("f2") + " C");

                            ds18b20.ConfigurationRead(false);
                            Console.WriteLine("Alarm set-points:");
                            Console.WriteLine("Hi alarm = " + ds18b20.TemperatureHighAlarm.DegreesCelsius + " C");
                            Console.WriteLine("Lo alarm = " + ds18b20.TemperatureLowAlarm.DegreesCelsius + " C");
                        }
                    }
                    else
                    {
                        Console.WriteLine("***** No devices in alarm ****");
                    }

                    loopRead--;
                }

                Console.WriteLine("");
            }

            void SetAlarmSetting()
            {
                ds18b20.TemperatureHighAlarm = Temperature.FromDegreesCelsius(30);
                ds18b20.TemperatureLowAlarm = Temperature.FromDegreesCelsius(25);
                // Write configuration on ScratchPad.
                ds18b20.ConfigurationWrite(false);
                // Write configuration on EEPROM too.
                ds18b20.ConfigurationWrite(true);
                // Read configuration to check if changes were applied
                ds18b20.ConfigurationRead(true);
                Console.WriteLine("Alarm set-points after changes:");
                Console.WriteLine("Hi alarm = " + ds18b20.TemperatureHighAlarm.DegreesCelsius.ToString("F") + " C");
                Console.WriteLine("Lo alarm = " + ds18b20.TemperatureLowAlarm.DegreesCelsius.ToString("F") + " C");
            }
        }

        private static void NotificationWhenValueHasChanged()
        {
            OneWireHost oneWire = new OneWireHost();

            Ds18b20 ds18b20 = new Ds18b20(oneWire, null, false, TemperatureResolution.VeryHigh);

            if (ds18b20.Initialize())
            {
                ds18b20.SensorValueChanged += (currentTemperature) =>
                {
                    Console.WriteLine($"Temperature: {currentTemperature.DegreesCelsius.ToString("F")}\u00B0C");
                };
                ds18b20.BeginTrackChanges(TimeSpan.FromMilliseconds(2000));
                // do whatever you want or sleep
                Thread.Sleep(60000);
                ds18b20.EndTrackChanges();
            }

            oneWire.Dispose();
        }

        private static void ReadingFromOneSensor()
        {
            OneWireHost oneWire = new OneWireHost();


            Ds18b20 ds18b20 = new Ds18b20(oneWire, null, false, TemperatureResolution.VeryHigh);

            ds18b20.IsAlarmSearchCommandEnabled = false;
            if (ds18b20.Initialize())
            {
                Console.WriteLine($"Is sensor parasite powered?:{ds18b20.IsParasitePowered}");
                string devAddrStr = "";
                foreach (var addrByte in ds18b20.Address)
                {
                    devAddrStr += addrByte.ToString("X2");
                }

                Console.WriteLine($"Sensor address:{devAddrStr}");

                while (true)
                {
                    if (!ds18b20.TryReadTemperature(out var currentTemperature))
                    {
                        Console.WriteLine("Can't read!");
                    }
                    else
                    {
                        Console.WriteLine($"Temperature: {currentTemperature.DegreesCelsius.ToString("F")}\u00B0C");
                    }

                    Thread.Sleep(5000);
                }
            }


            oneWire.Dispose();
        }
    }
}
