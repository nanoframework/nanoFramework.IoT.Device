using System;

namespace Iot.Device.BlueNrg2.Aci
{
    [Flags]
    public enum AccessPermissions : byte
    {
        None = 0x00,
        Read = 0x01,
        Write = 0x02,
        WriteWithoutResponse = 0x04,
        SignedWrite = 0x08
    }
}
