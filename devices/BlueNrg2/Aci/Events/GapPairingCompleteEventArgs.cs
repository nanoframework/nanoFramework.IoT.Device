using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing GapPairingCompleteEventArgs.
    /// </summary>
    public class GapPairingCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle on which the pairing procedure completed.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// Pairing status. If <see cref="GapPairingCompleteStatus.PairingFailed"/>, see Reason code.
        /// </summary>
        public readonly GapPairingCompleteStatus Status;

        /// <summary>
        /// Pairing reason error code. Valid if <see cref="Status"/> is <see cref="GapPairingCompleteStatus.PairingFailed"/>.
        /// </summary>
        public readonly GapPairingCompleteReason Reason;

        internal GapPairingCompleteEventArgs(ushort connectionHandle, GapPairingCompleteStatus status, GapPairingCompleteReason reason)
        {
            ConnectionHandle = connectionHandle;
            Status = status;
            Reason = reason;
        }
    }
}
