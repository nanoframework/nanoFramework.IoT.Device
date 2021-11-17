//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace Iot.Device.Swarm
{
    /// <summary>
    /// GPS fix Quality information of the Tile.
    /// </summary>
    public class GpsFixQuality
    {
        /// <summary>
        /// Horizontal dilution of precision (0..9999).
        /// </summary>
        /// <remarks>
        /// This value it's the actual hdop * 100.
        /// </remarks>
        public uint Hdop { get; internal set; }

        /// <summary>
        /// Vertical dilution of precision (0..9999).
        /// </summary>
        /// <remarks>
        /// This value it's the actual vdop * 100.
        /// </remarks>
        public uint Vdop { get; internal set; }

        /// <summary>
        /// Number of GNSS satellites used in solution.
        /// </summary>
        public uint GnssSatellitesCount { get; internal set; }

        /// <summary>
        /// Fix type.
        /// </summary>
        public GpsFixType FixType { get; internal set; }
    }
}
