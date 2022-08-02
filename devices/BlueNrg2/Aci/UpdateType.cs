using System;

namespace Iot.Device.BlueNrg2.Aci
{
    [Flags]
    public enum UpdateType : byte
    {
        LocalUpdate = 0x00,
        Notification = 0x01,
        Indication = 0x02,
        DisableRetransmission = 0x04
    }
}
