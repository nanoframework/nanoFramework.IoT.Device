// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Ft6xx6x;
using nanoFramework.M5Stack;

// Note: this sample requires a M5Core2.
// If you want to use another device, just remove all the related nugets
// And comments the following line
// You will need as well to set the pins if needed.
M5Core2.InitializeScreen();

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

while (true)
{
    Thread.Sleep(20);
}

void TouchInterrupCallback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
{
    Debug.WriteLine("Touch interrupt");
    var points = sensor.GetNumberPoints();
    if (points == 1)
    {
        var point = sensor.GetPoint(true);
        Debug.WriteLine($"ID: {point.TouchId}, X: {point.X}, Y: {point.Y}, Weight: {point.Weigth}, Misc: {point.Miscelaneous}, Event: {point.Event}");
        DiscrimitateButtons(point.X, point.Y);
    }
    else if (points == 2)
    {
        var dp = sensor.GetDoublePoints();
        Debug.WriteLine($"ID: {dp.Point1.TouchId}, X: {dp.Point1.X}, Y: {dp.Point1.Y}, Weight: {dp.Point1.Weigth}, Misc: {dp.Point1.Miscelaneous}, Event: {dp.Point1.Event}");
        Debug.WriteLine($"ID: {dp.Point2.TouchId}, X: {dp.Point2.X}, Y: {dp.Point2.Y}, Weight: {dp.Point2.Weigth}, Misc: {dp.Point2.Miscelaneous}, Event: {dp.Point1.Event}");
    }
}

void DiscrimitateButtons(int x, int y)
            {  
                const double pxPerMm = 6.90415;
                //r in milimeters is the radious of sensitivity to detect any of the given 3 buttons
                const double  rMm = 3.5; 
                const int x0L = 83;
                const int x0M = 182;
                const int x0R = 271;
                const int y0 = 263;
                const String strLB = "LEFT BUTTON PRESSED";
                const String strMB = "MIDDLE BUTTON PRESSED";
                const String strRB = "RIGHT BUTTON PRESSED";
                const String strXY1 = "TOUCH PANEL TOUCHED at X= ";
                const String strXY2 = ",Y= ";

                //r^2 in pixels^2
                int r2 = (int) ((rMm * pxPerMm) *(rMm * pxPerMm)); 
                
                int tmpL = 0;
                int tmpM = 0;
                int tmpR = 0;
                int tmpY = 0;

                tmpY = (y - y0) * (y - y0);
                tmpL = (x - x0L) * (x - x0L);
                tmpL += tmpY;
                tmpM = (x - x0M) * (x - x0M);
                tmpM += tmpY;
                tmpR = (x - x0R) * (x - x0R);
                tmpR += tmpY;

                nanoFramework.Console.Clear();

                if (tmpL <= r2)
                {
                    Debug.WriteLine(strLB);
                    nanoFramework.Console.WriteLine(strLB);
                }
                else if (tmpM <= r2)
                {
                    Debug.WriteLine(strMB);
                    nanoFramework.Console.WriteLine(strMB);
                }
                else if (tmpR <= r2)
                {
                    Debug.WriteLine(strRB);
                    nanoFramework.Console.WriteLine(strRB);
                }
                else {
                    Debug.WriteLine(strXY1 + x.ToString() + strXY1 + y.ToString());
                    nanoFramework.Console.WriteLine(strXY1 + x.ToString() + strXY2 + y.ToString());
                }
            }
