using System;

namespace Iot.Device.Swarm
{
    /// <summary>
    /// <see cref="DateTime"/> information from the Tile.
    /// </summary>
    public class DateTimeInfo
    {
        /// <summary>
        /// <see cref="DateTime"/> value availabel from the Tile.
        /// </summary>
        public DateTime Value { get; internal set; } = DateTime.MinValue;

        /// <summary>
        /// Information if the available <see cref="Value"/> is valid.
        /// </summary>
        public bool IsValid { get; internal set; }
    }
}
