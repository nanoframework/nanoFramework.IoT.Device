// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
using nanoFramework.M5Stack;
using nanoFramework.M5Core2;
using Iot.Device.Button;
using Iot.Device.Ft6xx6x;
using System.Device.Gpio;
using System.Device.I2c;
using System.IO.Ports;
using UnitsNet;
using nanoFramework.Runtime.Events;

namespace NFAppCore2App2
{
    public class Program
    {
        public static void Main()
        {
            // Note: this sample requires a M5Core2.
            // If you want to use another device, just remove all the related nugets
            // And comments the following line
            // You will need as well to set the pins if needed.
            M5Core2.InitializeScreen();
            nanoFramework.Console.Clear();
            nanoFramework.Console.WriteLine("Core2 Touch screen test");
            
            I2cConnectionSettings settings = new(1, Ft6xx6x.DefaultI2cAddress);
            using I2cDevice device = I2cDevice.Create(settings);
            using GpioController gpio = new();

            using Ft6xx6x sensor = new(device);
            var ver = sensor.GetVersion();
            Debug.WriteLine($"version: {ver}");
            sensor.SetInterruptMode(false);
            Debug.WriteLine($"Period active: {sensor.PeriodActive}");
            Debug.WriteLine($"Period active in monitor mode: {sensor.MonitorModePeriodActive}");
            Debug.WriteLine($"Time to enter monitor: {sensor.MonitorModeDelaySeconds} seconds");
            Debug.WriteLine($"Monitor mode: {sensor.MonitorModeEnabled}");
            Debug.WriteLine($"Proximity sensing: {sensor.ProximitySensingEnabled}");

            gpio.OpenPin(39, PinMode.Input);
            // This will enable an event on GPIO39 on falling edge when the screen if touched
            gpio.RegisterCallbackForPinValueChangedEvent(39, PinEventTypes.Falling, TouchInterrupCallback);

            TouchPanelUserAppLogic touchPanelUserApp = new TouchPanelUserAppLogic();
            TouchPanelProcessor.TouchedEvent += touchPanelUserApp.TouchPanelUserAppLogicSkeleton;

            //User defined code example; if not needed you may substitute the while loop with Thread.Sleep(Timeout.Infinite); 
            while (true)
            {
                Thread.Sleep(5000);
                Debug.WriteLine("Main Thread Reporting");
            }
            
            void TouchInterrupCallback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
            {
                Debug.WriteLine("Touch interrupt");
                var points = sensor.GetNumberPoints();
                if (points == 1)
                {
                    var point = sensor.GetPoint(true);
                    Debug.WriteLine($"ID: {point.TouchId}, X: {point.X}, Y: {point.Y}, Weight: {point.Weigth}, Misc: {point.Miscelaneous}, Event: {point.Event}");
                    TouchPanelDiscriminator.DiscrimitateButtons(point.X, point.Y);
                }
                else if (points == 2)
                {
                    var dp = sensor.GetDoublePoints();
                    Debug.WriteLine($"ID: {dp.Point1.TouchId}, X: {dp.Point1.X}, Y: {dp.Point1.Y}, Weight: {dp.Point1.Weigth}, Misc: {dp.Point1.Miscelaneous}, Event: {dp.Point1.Event}");
                    Debug.WriteLine($"ID: {dp.Point2.TouchId}, X: {dp.Point2.X}, Y: {dp.Point2.Y}, Weight: {dp.Point2.Weigth}, Misc: {dp.Point2.Miscelaneous}, Event: {dp.Point1.Event}");
                }
            }

            
       }

    }
    
    
}
