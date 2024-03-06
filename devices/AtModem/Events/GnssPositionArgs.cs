// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.AtModem.Gnss;

namespace Iot.Device.AtModem.Events
{
    /// <summary>
    /// Represents the arguments for a Global Navigation Satellite System position event.
    /// </summary>
    public class GnssPositionArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GnssPositionArgs"/> class.
        /// </summary>
        /// <param name="position">The GNSS position.</param>
        public GnssPositionArgs(GnssPosition position)
        {
            Position = position;
        }

        /// <summary>
        /// Gets the GNSS position.
        /// </summary>
        public GnssPosition Position { get; }
    }
}
