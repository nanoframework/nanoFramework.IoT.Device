using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing GattTxPoolAvailableEventArgs.
    /// </summary>
    public class GattTxPoolAvailableEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle related to the request.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// Not used.
        /// </summary>
        public readonly ushort AvailableBuffers;

        internal GattTxPoolAvailableEventArgs(ushort connectionHandle, ushort availableBuffers)
        {
            ConnectionHandle = connectionHandle;
            AvailableBuffers = availableBuffers;
        }
    }
}
