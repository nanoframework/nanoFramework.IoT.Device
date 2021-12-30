using System;
using nanoFramework.Runtime.Events;

namespace NFAppCore2App2
{
    public class TouchEventArgs : EventArgs
    {
        public EventCategory TouchEventCategory { get; set; }
        public int TouchEventSubCategory { get; set; }
        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }

        //NOTE: In order to be able to differentiate a short press from a long press a time stamp maybe needed later
        //public DateTime TimeReached { get; set; }
    }
}
