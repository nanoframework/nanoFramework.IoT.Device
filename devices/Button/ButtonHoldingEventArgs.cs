using System;

namespace Iot.Device.Button
{
    public class ButtonHoldingEventArgs : EventArgs
    {
        /// <summary>
        /// The button states.
        /// </summary>
        public ButtonHoldingState HoldingState { get; set; }
    }
}
