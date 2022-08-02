using System;

namespace Iot.Device.BlueNrg2.Aci
{
    [Flags]
    public enum TransportLayerMode : byte
    {
        Uart = 0x01,
        Spi = 0x02
    }
}
