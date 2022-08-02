using System;

namespace Iot.Device.BlueNrg2.Aci
{
    [Flags]
    public enum CharacteristicProperties : byte
    {
        None = 0x00,
        Broadcast = 0x01,
        Read = 0x02,
        WriteWithoutResponse = 0x04,
        Write = 0x08,
        Notify = 0x10,
        Indicate = 0x20,
        SignedWrite = 0x40,
        Extended = 0x80
    }
}
