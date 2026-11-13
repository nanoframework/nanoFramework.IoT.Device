// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Gc0308
{
    /// <summary>
    /// White balance mode for the GC0308 camera sensor.
    /// </summary>
    public enum WhiteBalanceMode : byte
    {
        /// <summary>Automatic white balance (AWB). The sensor adjusts white balance continuously.</summary>
        Auto = 0x00,

        /// <summary>Sunny / daylight preset (approximately 5500K).</summary>
        Sunny = 0x01,

        /// <summary>Cloudy preset (approximately 6500K).</summary>
        Cloudy = 0x02,

        /// <summary>Office / fluorescent light preset (approximately 4000K).</summary>
        Office = 0x03,

        /// <summary>Home / incandescent light preset (approximately 2800K).</summary>
        Home = 0x04,
    }
}
