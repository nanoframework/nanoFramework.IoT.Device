using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing ReadLocalP256PublicKeyCompleteEventArgs.
    /// </summary>
    public class ReadLocalP256PublicKeyCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// For standard error codes see Bluetooth specification, Vol. 2, part D. For proprietary error code refer to Error codes section.
        /// </summary>
        public readonly byte Status;

        /// <summary>
        /// Local P-256 public key.
        /// </summary>
        public readonly byte[] LocalP356PublicKey;

        internal ReadLocalP256PublicKeyCompleteEventArgs(byte status, byte[] localP356PublicKey)
        {
            Status = status;
            LocalP356PublicKey = localP356PublicKey;
        }
    }
}
