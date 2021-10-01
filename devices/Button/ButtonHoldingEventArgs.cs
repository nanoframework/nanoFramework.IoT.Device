using System;

namespace Iot.Device.Button
{
    public class ButtonHoldingEventArgs : EventArgs
    {
        /// <summary>
        /// Button holding state.
        /// </summary>
        public ButtonHoldingState HoldingState { get; set; }
    }
}
