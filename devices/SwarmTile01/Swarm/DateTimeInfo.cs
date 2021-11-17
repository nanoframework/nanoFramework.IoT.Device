//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace Iot.Device.Swarm
{
    /// <summary>
    /// <see cref="DateTime"/> information from the Tile.
    /// </summary>
    public class DateTimeInfo
    {
        /// <summary>
        /// <see cref="DateTime"/> value available from the Tile.
        /// </summary>
        public DateTime Value { get; internal set; } = DateTime.MinValue;

        /// <summary>
        /// Information if the available <see cref="Value"/> is valid.
        /// </summary>
        public bool IsValid { get; internal set; }
    }
}
