////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// ADC resolution.
    /// </summary>
    public enum AdcResolution : byte
    {
        /// <summary>
        /// 15 bits effective resolution.
        /// </summary>
        Effective15Bits = 0b0000_0000,

        /// <summary>
        /// 14 bits effective resolution.
        /// </summary>
        Effective14Bits = 0b0001_0000,

        /// <summary>
        /// 13 bits effective resolution.
        /// </summary>
        Effective13Bits = 0b0010_0000,

        /// <summary>
        /// 12 bits effective resolution.
        /// </summary>
        Effective12Bits = 0b0011_0000,
    }
}
