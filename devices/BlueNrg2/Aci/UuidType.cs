namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// The type of an UUID, determines the length of UUIDs.
    /// </summary>
    public enum UuidType : byte
    {
        /// <summary>
        /// 16-bit UUID.
        /// </summary>
        Uuid16 = 0x01,

        /// <summary>
        /// 128-bit UUID.
        /// </summary>
        Uuid128 = 0x02
    }
}