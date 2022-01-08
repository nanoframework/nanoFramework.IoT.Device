// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ft6xx6x;
using nanoFramework.M5Stack;
using nanoFramework.Runtime.Events;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

namespace NFAppCore2App2
{
    public class Program
    {
        public delegate void TouchEventHandler(object sender, TouchEventArgs e);

        public static event TouchEventHandler TouchedEvent;

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

            TouchedEvent += TouchPanelUserAppLogicSkeleton;

            // User defined code example; if not needed you may substitute the while loop with Thread.Sleep(Timeout.Infinite); 
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
                    DiscrimitateButtons(point.X, point.Y);
                }
                else if (points == 2)
                {
                    var dp = sensor.GetDoublePoints();
                    Debug.WriteLine($"ID: {dp.Point1.TouchId}, X: {dp.Point1.X}, Y: {dp.Point1.Y}, Weight: {dp.Point1.Weigth}, Misc: {dp.Point1.Miscelaneous}, Event: {dp.Point1.Event}");
                    Debug.WriteLine($"ID: {dp.Point2.TouchId}, X: {dp.Point2.X}, Y: {dp.Point2.Y}, Weight: {dp.Point2.Weigth}, Misc: {dp.Point2.Miscelaneous}, Event: {dp.Point1.Event}");
                }
            }
        }

        private static void DiscrimitateButtons(int x, int y)
        {
            const double PxPerMm = 6.90415;
            // r in milimeters is the radious of sensitivity to detect any of the given 3 buttons
            const double Rmm = 3.5;
            const int X0L = 83;
            const int X0M = 182;
            const int X0R = 271;
            const int Y0 = 263;
            // r^2 in pixels^2
            const int R2 = (int)((Rmm * PxPerMm) * (Rmm * PxPerMm));

            TouchEventArgs touchEventArgs = new TouchEventArgs();

            // WARNING: Next assigment should be EventCategory.Touch but Touch Category not yet defined on main/develop
            touchEventArgs.TouchEventCategory = EventCategory.Unknown;
            touchEventArgs.X1 = x;
            touchEventArgs.Y1 = y;

            int tmpL = 0;
            int tmpM = 0;
            int tmpR = 0;
            int tmpY = 0;

            tmpY = (y - Y0) * (y - Y0);
            tmpL = (x - X0L) * (x - X0L);
            tmpL += tmpY;
            tmpM = (x - X0M) * (x - X0M);
            tmpM += tmpY;
            tmpR = (x - X0R) * (x - X0R);
            tmpR += tmpY;

            if (tmpL <= R2)
            {
                touchEventArgs.TouchEventSubCategory = (int)TouchEventSubcategory.LeftButton;
            }
            else if (tmpM <= R2)
            {
                touchEventArgs.TouchEventSubCategory = (int)TouchEventSubcategory.MiddleButton;
            }
            else if (tmpR <= R2)
            {
                touchEventArgs.TouchEventSubCategory = (int)TouchEventSubcategory.RightButton;
            }
            else
            {
                touchEventArgs.TouchEventSubCategory = (int)TouchEventSubcategory.SingleTouch;
            }

            OnTouchedEvent(touchEventArgs.TouchEventSubCategory, touchEventArgs.X1, touchEventArgs.Y1);
        }

        private static void TouchPanelUserAppLogicSkeleton(object sender, TouchEventArgs e)
        {
            const string strLB = "LEFT BUTTON PRESSED";
            const string strMB = "MIDDLE BUTTON PRESSED";
            const string strRB = "RIGHT BUTTON PRESSED";
            const string strXY1 = "TOUCH PANEL TOUCHED at X= ";
            const string strXY2 = ",Y= ";
            const string strXY1XY2 = "DOUBLE TOUCH X1Y1 X2Y2";

            Debug.WriteLine("Touch Panel Event Received Category= " + e.TouchEventCategory.ToString() + " Subcategory= " + e.TouchEventSubCategory.ToString());

            nanoFramework.Console.Clear();

            if (e.TouchEventSubCategory == (int)TouchEventSubcategory.LeftButton)
            {
                Debug.WriteLine(strLB);
                nanoFramework.Console.WriteLine(strLB);

            }
            else if (e.TouchEventSubCategory == (int)TouchEventSubcategory.MiddleButton)
            {
                Debug.WriteLine(strMB);
                nanoFramework.Console.WriteLine(strMB);

            }
            else if (e.TouchEventSubCategory == (int)TouchEventSubcategory.RightButton)
            {
                Debug.WriteLine(strRB);
                nanoFramework.Console.WriteLine(strRB);

            }
            else if (e.TouchEventSubCategory == (int)TouchEventSubcategory.SingleTouch)
            {
                Debug.WriteLine(strXY1 + e.X1.ToString() + strXY2 + e.Y1.ToString());
                nanoFramework.Console.WriteLine(strXY1 + e.X1.ToString() + strXY2 + e.Y1.ToString());

            }
            else if (e.TouchEventSubCategory == (int)TouchEventSubcategory.DoubleTouch)
            {
                Debug.WriteLine(strXY1XY2);

            }
            else
            {
                Debug.WriteLine("ERROR: UKNOWN TouchEventSubcategory");
            }
        }

        private static void OnTouchedEvent(int eventSubCategory, int x1, int y1, int x2 = 0, int y2 = 0)
        {
            TouchEventArgs args = new TouchEventArgs();

            args.TouchEventCategory = EventCategory.Unknown;
            args.TouchEventSubCategory = eventSubCategory;
            args.X1 = x1;
            args.Y1 = y1;
            args.X2 = x2;
            args.Y2 = y2;

            TouchedEvent?.Invoke(null, args);
        }
    }
}
