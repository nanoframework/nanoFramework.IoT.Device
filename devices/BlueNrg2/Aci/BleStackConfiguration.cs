using System;

namespace Iot.Device.BlueNrg2.Aci
{
    [Flags]
    public enum BleStackConfiguration : ushort
    {
        ControllerPrivacyEnabled = 0x0001,
        SecureConnectionsEnabled = 0x0002,
        ControllerMasterEnabled = 0x0004,
        ControllerDataLengthExtensionEnabled = 0x0008,
        LinkLayerOnly = 0x0010
    }
}
