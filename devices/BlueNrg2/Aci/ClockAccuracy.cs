// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Values for clock accuracy.
    /// </summary>
    public enum ClockAccuracy : byte
    {
        /// <summary>
        /// 500 ppm
        /// </summary>
        Ppm500 = 0x00,

        /// <summary>
        /// 250 ppm
        /// </summary>
        Ppm250 = 0x01,

        /// <summary>
        /// 150 ppm
        /// </summary>
        Ppm150 = 0x02,

        /// <summary>
        /// 100 ppm
        /// </summary>
        Ppm100 = 0x03,

        /// <summary>
        /// 75 ppm
        /// </summary>
        Ppm75 = 0x04,

        /// <summary>
        /// 50 ppm
        /// </summary>
        Ppm50 = 0x05,

        /// <summary>
        /// 30 ppm
        /// </summary>
        Ppm30 = 0x06,

        /// <summary>
        /// 20 ppm
        /// </summary>
        Ppm20 = 0x07
    }
}
