using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing EncryptionKeyRefreshCompleteEventArgs.
    /// </summary>
    public class EncryptionKeyRefreshCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// For standard error codes see Bluetooth specification, Vol. 2,
        /// part D. For proprietary error code refer to Error codes section.
        /// </summary>
        public readonly byte Status;

        /// <summary>
        /// Connection handle that identifies the connection.
        /// </summary>
        public readonly ushort ConnectionHandle;

        internal EncryptionKeyRefreshCompleteEventArgs(byte status, ushort connectionHandle)
        {
            Status = status;
            ConnectionHandle = connectionHandle;
        }
    }
}
