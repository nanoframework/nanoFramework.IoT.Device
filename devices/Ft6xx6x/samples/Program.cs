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
            nanoFramework.Console.WriteLine("Touch Chip type: " + Iot.Device.Ft6xx6x.ChipType.Ft6336U.ToString());

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

            touchPanelUserAppTemplate touchPanelUserApp = new touchPanelUserAppTemplate();
            TouchPanelProcessor.TouchedEvent += touchPanelUserApp.userDefinedTouchPanelBehaviourSkeleton;

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
                const double rMm = 3.5;
                const int x0L = 83;
                const int x0M = 182;
                const int x0R = 271;
                const int y0 = 263;

                TouchEventArgs touchEventArgs = new TouchEventArgs();

                //WARNING: Next assigment should be EventCategory.Touch but Touch Category not yet defined on main/develop
                touchEventArgs.eventCategory = EventCategory.Unknown;
                touchEventArgs.X1 = x;
                touchEventArgs.Y1 = y;

                //r^2 in pixels^2
                int r2 = (int)((rMm * pxPerMm) * (rMm * pxPerMm));

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

                if (tmpL <= r2)
                {
                    touchEventArgs.eventSubCategory = (int)TouchEventSubcategory.LeftButton;
                }
                else if (tmpM <= r2)
                {
                    touchEventArgs.eventSubCategory = (int) TouchEventSubcategory.MiddleButton;
                }
                else if (tmpR <= r2)
                {
                    touchEventArgs.eventSubCategory = (int) TouchEventSubcategory.RightButton;
                }
                else
                {
                    touchEventArgs.eventSubCategory = (int) TouchEventSubcategory.SingleTouch;
                }

                TouchPanelProcessor.OnTouchedEvent(touchEventArgs.eventSubCategory, touchEventArgs.X1, touchEventArgs.Y1);

            }

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }

    }
    public class TouchEventArgs : EventArgs
    {
        public EventCategory eventCategory { get; set; }
        public int eventSubCategory { get; set; }
        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }

        //public DateTime TimeReached { get; set; }
    }
    public enum TouchEventSubcategory
    {
        Unknown = 1000,
        LeftButton = 1001,
        MiddleButton = 1002,
        RightButton = 1003,
        SingleTouch = 1004,
        DoubleTouch = 1005
    }

    public delegate void TouchEventHandler(object sender, TouchEventArgs e);

    public class TouchPanelProcessor
    {
        public static event TouchEventHandler TouchedEvent;
        public static void OnTouchedEvent(int eventSubCategory, int x1, int y1, int x2 = 0, int y2 = 0)
        {
            TouchEventArgs args = new TouchEventArgs();

            args.eventCategory = EventCategory.Unknown;
            args.eventSubCategory = eventSubCategory;
            args.X1 = x1;
            args.Y1 = y1;
            args.X2 = x2;
            args.Y2 = y2;

            TouchedEvent?.Invoke(null, args);

        }

    }
    public class touchPanelUserAppTemplate
    {
        public void userDefinedTouchPanelBehaviourSkeleton(object sender, TouchEventArgs e)
        {
            const string strLB = "LEFT BUTTON PRESSED";
            const string strMB = "MIDDLE BUTTON PRESSED";
            const string strRB = "RIGHT BUTTON PRESSED";
            const string strXY1 = "TOUCH PANEL TOUCHED at X= ";
            const string strXY2 = ",Y= ";
            const string strXY1XY2 = "DOUBLE TOUCH X1Y1 X2Y2";

            Debug.WriteLine("Touch Panel Event Received Category= " + e.eventCategory.ToString() + " Subcategory= " + e.eventSubCategory.ToString());
            nanoFramework.Console.Clear();

            if (e.eventSubCategory == (int)TouchEventSubcategory.LeftButton)
            {
                Debug.WriteLine(strLB);
                nanoFramework.Console.WriteLine(strLB);

            }
            else if (e.eventSubCategory == (int)TouchEventSubcategory.MiddleButton)
            {
                Debug.WriteLine(strMB);
                nanoFramework.Console.WriteLine(strMB);

            }
            else if (e.eventSubCategory == (int)TouchEventSubcategory.RightButton)
            {
                Debug.WriteLine(strRB);
                nanoFramework.Console.WriteLine(strRB);

            }
            else if (e.eventSubCategory == (int)TouchEventSubcategory.SingleTouch)
            {
                Debug.WriteLine(strXY1 + e.X1.ToString() + strXY2 + e.Y1.ToString());
                nanoFramework.Console.WriteLine(strXY1 + e.X1.ToString() + strXY2 + e.Y1.ToString());

            }
            else if (e.eventSubCategory == (int)TouchEventSubcategory.DoubleTouch)
            {
                Debug.WriteLine(strXY1XY2);

            }
            else
            {
                Debug.WriteLine("ERROR: UKNOWN TouchEventSubcategory");
            }

        }

    }
}
