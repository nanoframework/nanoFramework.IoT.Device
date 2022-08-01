using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing GenerateDhKeyCompleteEventArgs.
    /// </summary>
    public class GenerateDhKeyCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// For standard error codes see Bluetooth specification, Vol. 2,
        /// part D. For proprietary error code refer to Error codes section.
        /// </summary>
        public readonly byte Status;

        /// <summary>
        /// Diffie Hellman Key.
        /// </summary>
        public readonly byte[] DhKey;

        internal GenerateDhKeyCompleteEventArgs(byte status, byte[] dhKey)
        {
            Status = status;
            DhKey = dhKey;
        }
    }
}
