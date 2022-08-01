using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    public class GattProcessTimeoutEventArgs : EventArgs
    {
        public readonly ushort ConnectionHandle;

        internal GattProcessTimeoutEventArgs(ushort connectionHandle)
        {
            ConnectionHandle = connectionHandle;
        }
    }
}