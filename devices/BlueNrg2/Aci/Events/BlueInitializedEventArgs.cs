using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing BlueInitializedEventArgs.
    /// </summary>
    public class BlueInitializedEventArgs : EventArgs
    {
        /// <summary>
        /// Reason code describing why device was reset and in which
        /// mode is operating (Updater or Normal mode)
        /// </summary>
        public readonly ReasonCode ReasonCode;

        internal BlueInitializedEventArgs(ReasonCode reasonCode)
        {
            ReasonCode = reasonCode;
        }
    }
}
