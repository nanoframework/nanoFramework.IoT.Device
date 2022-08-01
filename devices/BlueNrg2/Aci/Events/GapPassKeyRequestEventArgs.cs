using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing GapPassKeyRequestEventArgs.
    /// </summary>
    public class GapPassKeyRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle for which the passkey has been requested.
        /// </summary>
        public readonly ushort ConnectionHandle;

        internal GapPassKeyRequestEventArgs(ushort connectionHandle)
        {
            ConnectionHandle = connectionHandle;
        }
    }
}
