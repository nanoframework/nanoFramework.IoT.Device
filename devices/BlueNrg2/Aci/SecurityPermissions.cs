using System;

namespace Iot.Device.BlueNrg2.Aci
{
    [Flags]
    public enum SecurityPermissions : byte
    {
        None = 0x00,
        AuthenticatedRead = 0x01,
        AuthorizedRead = 0x02,
        EncryptedRead = 0x04,
        AuthenticatedWrite = 0x08,
        AuthorizedWrite = 0x10,
        EncryptedWrite = 0x20
    }
}
