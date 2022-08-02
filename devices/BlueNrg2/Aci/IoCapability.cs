namespace Iot.Device.BlueNrg2.Aci
{
    public enum IoCapability : byte
    {
        DisplayOnly = 0x00,
        DisplayYesNo = 0x01,
        KeyboardOnly = 0x02,
        NoInputNoOutput = 0x03,
        KeyboardDisplay = 0x04
    }
}
