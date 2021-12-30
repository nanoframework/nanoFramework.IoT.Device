// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ft6xx6x;
using nanoFramework.M5Stack;
using nanoFramework.Runtime.Events;
using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Console = nanoFramework.M5Stack.Console;

namespace NFAppCore2App2
{
    public class Program
    {
        private static GpioPin _touchInterruptPin;
        private static Ft6xx6x _touchController;

        public static void Main()
        {
            // Note: this sample requires a M5Core2.
            // If you want to use another device, remove the M5Core2 NuGet and it's dependencies
            // And comments the following line
            // You will need as well to set the pins if needed.

            // initialize M5Core2 display
            M5Core2.InitializeScreen();
            Console.Clear();
            Console.WriteLine("Core2 Touch screen test");
            Console.WriteLine("Touch Chip type: " + Iot.Device.Ft6xx6x.ChipType.Ft6336U.ToString());

            // setup I2C to communicate with touch controller
            I2cConnectionSettings settings = new(1, Ft6xx6x.DefaultI2cAddress);
            I2cDevice device = I2cDevice.Create(settings);

            // instantiate touch controller
            _touchController = new Ft6xx6x(device);
            _touchController.SetInterruptMode(false);

            // output details about touch controller
            Debug.WriteLine($"version: {_touchController.GetVersion()}");
            Debug.WriteLine($"Period active: {_touchController.PeriodActive}");
            Debug.WriteLine($"Period active in monitor mode: {_touchController.MonitorModePeriodActive}");
            Debug.WriteLine($"Time to enter monitor: {_touchController.MonitorModeDelaySeconds} seconds");
            Debug.WriteLine($"Monitor mode: {_touchController.MonitorModeEnabled}");
            Debug.WriteLine($"Proximity sensing: {_touchController.ProximitySensingEnabled}");

            // instantiate GPIO controller to receive interrupt events
            GpioController gpio = new();
            _touchInterruptPin = gpio.OpenPin(39, PinMode.Input);
            // register event handler for screen touched
            _touchInterruptPin.ValueChanged += TouchInterruptPin_ValueChanged;

            TouchPanelUserAppTemplate touchPanelUserApp = new TouchPanelUserAppTemplate();
            TouchPanelProcessor.TouchedEvent += touchPanelUserApp.UserDefinedTouchPanelBehaviourSkeleton;

            Thread.Sleep(Timeout.Infinite);
        }

        private static void TouchInterruptPin_ValueChanged(object sender, PinValueChangedEventArgs e)
        {
            // only care about falling events
            if (e.ChangeType == PinEventTypes.Falling)
            {
                Debug.WriteLine("Touch interrupt");

                var points = _touchController.GetNumberPoints();

                if (points == 1)
                {
                    var point = _touchController.GetPoint(true);
                    Debug.WriteLine($"ID: {point.TouchId}, X: {point.X}, Y: {point.Y}, Weight: {point.Weigth}, Misc: {point.Miscelaneous}, Event: {point.Event}");

                    DiscriminateButtons(point.X, point.Y);
                }
                else if (points == 2)
                {
                    var dp = _touchController.GetDoublePoints();
                    Debug.WriteLine($"ID: {dp.Point1.TouchId}, X: {dp.Point1.X}, Y: {dp.Point1.Y}, Weight: {dp.Point1.Weigth}, Misc: {dp.Point1.Miscelaneous}, Event: {dp.Point1.Event}");
                    Debug.WriteLine($"ID: {dp.Point2.TouchId}, X: {dp.Point2.X}, Y: {dp.Point2.Y}, Weight: {dp.Point2.Weigth}, Misc: {dp.Point2.Miscelaneous}, Event: {dp.Point1.Event}");
                }
            }
        }

        private static void DiscriminateButtons(int x, int y)
        {
            const double pxPerMm = 6.90415;

            //r in millimeters is the radius of sensitivity to detect any of the given 3 buttons
            const double rMm = 3.5;
            const int x0L = 83;
            const int x0M = 182;
            const int x0R = 271;
            const int y0 = 263;

            TouchEventArgs touchEventArgs = new TouchEventArgs();

            touchEventArgs.Category = EventCategory.Touch;
            touchEventArgs.X1 = x;
            touchEventArgs.Y1 = y;

            //r^2 in pixels^2
            int r2 = (int)((rMm * pxPerMm) * (rMm * pxPerMm));
            
            int tmpY = (y - y0) * (y - y0);
            int tmpL = (x - x0L) * (x - x0L);
            tmpL += tmpY;
            int tmpM = (x - x0M) * (x - x0M);
            tmpM += tmpY;
            int tmpR = (x - x0R) * (x - x0R);
            tmpR += tmpY;

            if (tmpL <= r2)
            {
                touchEventArgs.SubCategory = (int)TouchEventSubcategory.LeftButton;
            }
            else if (tmpM <= r2)
            {
                touchEventArgs.SubCategory = (int)TouchEventSubcategory.MiddleButton;
            }
            else if (tmpR <= r2)
            {
                touchEventArgs.SubCategory = (int)TouchEventSubcategory.RightButton;
            }
            else
            {
                touchEventArgs.SubCategory = (int)TouchEventSubcategory.SingleTouch;
            }

            TouchPanelProcessor.OnTouchedEvent(touchEventArgs.SubCategory, touchEventArgs.X1, touchEventArgs.Y1);
        }
    }

    internal class TouchEventArgs : EventArgs
    {
        public EventCategory Category { get; set; }
        public int SubCategory { get; set; }
        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }

        //public DateTime TimeReached { get; set; }
    }

    internal enum TouchEventSubcategory
    {
        Unknown = 1000,
        LeftButton = 1001,
        MiddleButton = 1002,
        RightButton = 1003,
        SingleTouch = 1004,
        DoubleTouch = 1005
    }

    internal delegate void TouchEventHandler(object sender, TouchEventArgs e);

    internal class TouchPanelProcessor
    {
        public static event TouchEventHandler TouchedEvent;

        public static void OnTouchedEvent(int eventSubCategory, int x1, int y1, int x2 = 0, int y2 = 0)
        {
            TouchEventArgs args = new();

            args.Category = EventCategory.Unknown;
            args.SubCategory = eventSubCategory;
            args.X1 = x1;
            args.Y1 = y1;
            args.X2 = x2;
            args.Y2 = y2;

            TouchedEvent?.Invoke(null, args);
        }
    }

    internal class TouchPanelUserAppTemplate
    {
        public void UserDefinedTouchPanelBehaviourSkeleton(object sender, TouchEventArgs e)
        {
            const string strLB = "LEFT BUTTON PRESSED";
            const string strMB = "MIDDLE BUTTON PRESSED";
            const string strRB = "RIGHT BUTTON PRESSED";
            const string strXY1 = "TOUCH PANEL TOUCHED at X= ";
            const string strXY2 = ",Y= ";
            const string strXY1XY2 = "DOUBLE TOUCH X1Y1 X2Y2";

            Debug.WriteLine("Touch Panel Event Received Category= " + e.Category.ToString() + " Subcategory= " + e.SubCategory.ToString());
            Console.Clear();

            switch (e.SubCategory)
            {
                case (int)TouchEventSubcategory.LeftButton:
                    Debug.WriteLine(strLB);
                    Console.WriteLine(strLB);
                    break;

                case (int)TouchEventSubcategory.MiddleButton:
                    Debug.WriteLine(strMB);
                    Console.WriteLine(strMB);
                    break;

                case (int)TouchEventSubcategory.RightButton:
                    Debug.WriteLine(strRB);
                    Console.WriteLine(strRB);
                    break;

                case (int)TouchEventSubcategory.SingleTouch:
                    Debug.WriteLine(strXY1 + e.X1.ToString() + strXY2 + e.Y1.ToString());
                    Console.WriteLine(strXY1 + e.X1.ToString() + strXY2 + e.Y1.ToString());
                    break;

                case (int)TouchEventSubcategory.DoubleTouch:
                    Debug.WriteLine(strXY1XY2);
                    break;

                default:
                    Debug.WriteLine("ERROR: UKNOWN TouchEventSubcategory");
                    break;
            }
        }
    }
}
