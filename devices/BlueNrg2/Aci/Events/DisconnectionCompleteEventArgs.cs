using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing DisconnectionCompleteEventArgs.
    /// </summary>
    public class DisconnectionCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// For standard error codes see Bluetooth specification, Vol. 2,
        /// part D. For proprietary error code refer to Error codes section.
        /// </summary>
        public readonly byte Status;

        /// <summary>
        /// Connection_Handle which was disconnected.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// Reason for disconnection. See Error Codes.
        /// </summary>
        public readonly byte Reason;

        internal DisconnectionCompleteEventArgs(byte status, ushort connectionHandle, byte reason)
        {
            Status = status;
            ConnectionHandle = connectionHandle;
            Reason = reason;
        }
    }
}
