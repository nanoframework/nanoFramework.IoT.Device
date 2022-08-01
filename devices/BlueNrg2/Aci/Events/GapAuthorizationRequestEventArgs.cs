using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing GapAuthorizationRequestEventArgs.
    /// </summary>
    public class GapAuthorizationRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle for which authorization has been requested.
        /// </summary>
        public readonly ushort ConnectionHandle;

        internal GapAuthorizationRequestEventArgs(ushort connectionHandle)
        {
            ConnectionHandle = connectionHandle;
        }
    }
}