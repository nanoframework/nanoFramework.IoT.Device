// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System;

namespace Iot.Device.Sps30.Entities
{
    /// <summary>
    /// Parsed response after requesting version information.
    /// </summary>
    public class VersionInformation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionInformation" /> class.
        /// </summary>
        /// <param name="data">Raw data from the response.</param>
        /// <exception cref="ArgumentOutOfRangeException">When less than 7 bytes are provided.</exception>
        public VersionInformation(byte[] data)
        {
            if (data.Length < 7)
            {
                throw new ArgumentOutOfRangeException(nameof(data), "Unexpected array size. Expecting at least 7 bytes.");
            }

            FirmwareVersion = new Version(data[0], data[1]);
            
            // data[2] is reserved
            HardwareRevision = data[3];

            // data[4] is reserved
            ShdlcProtocolVersion = new Version(data[5], data[6]);
        }

        /// <summary>
        /// Gets firmware version.
        /// </summary>
        public Version FirmwareVersion { get; private set; }

        /// <summary>
        /// Gets hardware rivision.
        /// </summary>
        public int HardwareRevision { get; private set; }

        /// <summary>
        /// Gets SHDLC protocol version.
        /// </summary>
        public Version ShdlcProtocolVersion { get; private set; }

        /// <summary>
        /// Conveniently show the version information in a single string.
        /// </summary>
        /// <returns>The version information as a convenient string.</returns>
        public override string ToString()
        {
            return $"Firmware V{FirmwareVersion}, Hardware V{HardwareRevision}, SHDLC V{ShdlcProtocolVersion}";
        }
    }
}
