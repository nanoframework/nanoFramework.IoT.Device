using System;
using nanoFramework.Runtime.Events;

namespace NFAppCore2App2
{
    public delegate void TouchEventHandler(object sender, TouchEventArgs e);
    public class TouchPanelProcessor
    {
        public static event TouchEventHandler TouchedEvent;
        public static void OnTouchedEvent(int eventSubCategory, int x1, int y1, int x2 = 0, int y2 = 0)
        {
            TouchEventArgs args = new TouchEventArgs();

            args.TouchEventCategory = EventCategory.Unknown;
            args.TouchEventSubCategory = eventSubCategory;
            args.X1 = x1;
            args.Y1 = y1;
            args.X2 = x2;
            args.Y2 = y1;

            TouchedEvent?.Invoke(null, args);

        }

    }
}
