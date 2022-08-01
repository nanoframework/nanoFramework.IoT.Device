using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing GattServerConfirmationEventArgs.
    /// </summary>
    public class GattServerConfirmationEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle related to the event.
        /// </summary>
        public readonly ushort ConnectionHandle;

        internal GattServerConfirmationEventArgs(ushort connectionHandle)
        {
            ConnectionHandle = connectionHandle;
        }
    }
}
