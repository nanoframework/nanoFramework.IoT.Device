// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.

namespace Iot.Device.Swarm
{
    /// <summary>
    /// Tile Status events.
    /// </summary>
    public enum TileStatus
    {
        /// <summary>
        /// Unknown status.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// A firmware crash occurred that caused a restart.
        /// </summary>
        Abort,

        /// <summary>
        /// The first time GPS has acquired a valid date/time reference.
        /// </summary>
        DateTimeAvailable,

        /// <summary>
        /// The first time GPS has acquired a valid position 3D fix.
        /// </summary>
        PositionAvailable,

        /// <summary>
        /// An error message has been received from the Tile. Check the content.
        /// </summary>
        Error,
    }
}
