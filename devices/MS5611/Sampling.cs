// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ms5611
{
    /// <summary>
    /// MS5611 sampling, check data sheet, page 9, commands section
    /// </summary>
    public enum Sampling
    {
        /// <summary>
        /// 256 ratio
        /// </summary>
        UltraLowPower,

        /// <summary>
        /// 512 ratio
        /// </summary>
        LowPower,

        /// <summary>
        /// 1024 ratio
        /// </summary>
        Standard,

        /// <summary>
        /// 2048 ratio
        /// </summary>
        HighResolution,

        /// <summary>
        /// 4096 ratio
        /// </summary>
        UltraHighResolution,
    }
}