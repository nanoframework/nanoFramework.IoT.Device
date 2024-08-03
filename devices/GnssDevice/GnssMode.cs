// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Common.Gnss
{
    /// <summary>
    /// The Global Navigation Satellite System mode.
    /// </summary>
    [Flags]
    public enum GnssMode
    {
        /// <summary>
        /// No GNSS mode selected.
        /// </summary>
        None = 0,

        /// <summary>
        /// Global Positioning System.
        /// </summary>
        Gps = 0b0000_0001,

        /// <summary>
        /// BeiDou Navigation Satellite System.
        /// </summary>
        Bds = 0b0000_0010,

        /// <summary>
        /// Global Navigation Satellite System.
        /// </summary>
        Glonass = 0b0000_0100,

        /// <summary>
        /// Quasi-Zenith Satellite System.
        /// </summary>
        Qzss = 0b0000_1000,

        /// <summary>
        /// Galileo.
        /// </summary>
        Galileo = 0b0001_0000,

        /// <summary>
        /// Satellite-Based Augmentation Systems.
        /// </summary>
        Sbas = 0b0010_0000,

        /// <summary>
        /// Wide Area Augmentation System.
        /// </summary>
        Wass = 0b0100_0000,
    }
}
