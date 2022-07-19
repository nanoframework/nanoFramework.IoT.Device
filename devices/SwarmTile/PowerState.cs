// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.

namespace Iot.Device.Swarm
{
    /// <summary>
    /// Power state of the Swarm Tile.
    /// </summary>
    public enum PowerState
    {
        /// <summary>
        /// Unknown power state.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Device is on.
        /// </summary>
        On,

        /// <summary>
        /// Device is off.
        /// </summary>
        Off,

        /// <summary>
        /// Device is in sleep mode.
        /// </summary>
        Sleep,
    }
}
