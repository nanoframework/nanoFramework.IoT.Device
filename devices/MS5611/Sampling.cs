namespace Iot.Device.MS5611
{
    /// <summary>
    /// MS5611 sampling
    /// </summary>
    public enum Sampling : byte
    {
        /// <summary>
        /// 256 ratio
        /// </summary>
        UltraLowPower = 0x00,

        /// <summary>
        /// 512 ratio
        /// </summary>
        LowPower = 0x02,

        /// <summary>
        /// 1024 ratio
        /// </summary>
        Standard = 0x04,

        /// <summary>
        /// 2048 ratio
        /// </summary>
        HighResolution = 0x06,

        /// <summary>
        /// 4096 ratio
        /// </summary>
        UltraHighResolution = 0x08,
    }
}