using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing BlueEventsLostEventArgs.
    /// </summary>
    public class BlueEventsLostEventArgs : EventArgs
    {
        /// <summary>
        /// Bitmap of lost events. Each bit indicates one or more
        /// occurrences of the specific event.
        /// </summary>
        public readonly LostEventBitmap LostEvents;

        internal BlueEventsLostEventArgs(LostEventBitmap lostEvents)
        {
            LostEvents = lostEvents;
        }
    }
}
