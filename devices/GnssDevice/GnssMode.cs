// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// The Global Navigation Satellite System mode.
    /// </summary>
    public enum GnssMode
    {
        /// <summary>
        /// No GNSS mode selected.
        /// </summary>
        None,

        /// <summary>
        /// GP for Global Positioning System.
        /// </summary>
        Gps,

        /// <summary>
        /// BD for BeiDou Navigation Satellite System.
        /// </summary>
        BeiDou,

        /// <summary>
        /// GL for Global Navigation Satellite System.
        /// </summary>
        Glonass,

        /// <summary>
        /// GQ for Quasi-Zenith Satellite System.
        /// </summary>
        Qzss,

        /// <summary>
        /// GA for Galileo.
        /// </summary>
        Galileo,

        /// <summary>
        /// Satellite-Based Augmentation Systems.
        /// </summary>
        Sbas,

        /// <summary>
        /// Wide Area Augmentation System.
        /// </summary>
        Wass,

        /// <summary>
        /// GI for Navigation with Indian Constellation.
        /// </summary>
        NavIC,

        /// <summary>
        /// GN for Global Navigation Satellite System including multiple constellations.
        /// </summary>
        Gnss,

        /// <summary>
        /// Other system such as instrument-specific.
        /// </summary>
        Other,
    }
}
