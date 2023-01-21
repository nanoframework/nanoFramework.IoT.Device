// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.

namespace Iot.Device.Swarm
{
    /// <summary>
    /// GPS fix Quality information of the Tile.
    /// </summary>
    public class GpsFixQuality
    {
        /// <summary>
        /// Gets horizontal dilution of precision (0..9999).
        /// </summary>
        /// <remarks>
        /// This value it's the actual hdop * 100.
        /// </remarks>
        public uint Hdop { get; internal set; }

        /// <summary>
        /// Gets vertical dilution of precision (0..9999).
        /// </summary>
        /// <remarks>
        /// This value it's the actual vdop * 100.
        /// </remarks>
        public uint Vdop { get; internal set; }

        /// <summary>
        /// Gets number of GNSS satellites used in solution.
        /// </summary>
        public uint GnssSatellitesCount { get; internal set; }

        /// <summary>
        /// Gets fix type.
        /// </summary>
        public GpsFixType FixType { get; internal set; }
    }
}
