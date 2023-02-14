// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.ADXL343
{
    /// <summary>
    /// Active Inactive Control Map.
    /// </summary>
    internal enum ActInactCtlMap : byte
    {
        /// <summary>
        /// Active AC/DC.
        /// </summary>
        ActAcDc = 0x80,

        /// <summary>
        /// Active X Enable.
        /// </summary>
        ActXEnable = 0x40,

        /// <summary>
        /// Active Y Enable.
        /// </summary>
        ActYEnable = 0x20,

        /// <summary>
        /// Active Z Enable.
        /// </summary>
        ActZEnable = 0x10,

        /// <summary>
        /// Inactive AC/DC.
        /// </summary>
        InactAcDc = 0x08,

        /// <summary>
        /// Inactive X Enable.
        /// </summary>
        InactXEnable = 0x04,

        /// <summary>
        /// Inactive Y Enable.
        /// </summary>
        InactYEnable = 0x02,

        /// <summary>
        /// Inactive Z Enable.
        /// </summary>
        InactZEnable = 0x01
    }
}
