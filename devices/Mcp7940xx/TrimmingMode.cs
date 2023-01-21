// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp7940xx
{
    /// <summary>
    /// The rate that oscillator trimming adjustments are applied.
    /// </summary>
    public enum TrimmingMode
    {
        /// <summary>
        /// The oscillator trimming adjustment occurs once per minute.
        /// </summary>
        NormalTrimMode,

        /// <summary>
        /// The oscillator trimming adjustment occurs 128 times per second.
        /// </summary>
        CoarseTrimMode
    }
}
