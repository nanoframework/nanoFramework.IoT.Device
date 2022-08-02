using System;

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum RadioStateMask : ushort
    {
        Idle = 0x0001,
        Advertising = 0x0002,
        ConnectionEventSlave = 0x0004,
        Scanning = 0x0008,
        ConnectionRequest = 0x0010,
        ConnectionEventMaster = 0x0020,
        TxTestMode = 0x0040,
        RxTestMode = 0x0080
    }
}
