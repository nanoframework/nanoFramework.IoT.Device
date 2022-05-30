//
// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace Iot.Device.SPS30.Entities
{
    /// <summary>
    /// Parsed response after requesting version information.
    /// </summary>
    public class VersionInformation
    {
        /// <summary>
        /// Parsed response after requesting version information.
        /// </summary>
        /// <param name="data">Raw data from the response</param>
        /// <exception cref="ArgumentOutOfRangeException">When less than 7 bytes are provided</exception>
        public VersionInformation(byte[] data)
        {
            if (data.Length < 7)
                throw new ArgumentOutOfRangeException(nameof(data), "Unexpected array size. Expecting at least 7 bytes.");
            FirmwareMajorVersion = data[0];
            FirmwareMinorVersion = data[1];
            // data[2] is reserved
            HardwareRevision = data[3];
            // data[4] is reserved
            SHDLCProtocolMajorVersion = data[5];
            SHDLCProtocolMinorVersion = data[6];
        }

        /// <summary>
        /// Firmware major version
        /// </summary>
        public int FirmwareMajorVersion { get; private set; }

        /// <summary>
        /// Firmware minor version
        /// </summary>
        public int FirmwareMinorVersion { get; private set; }

        /// <summary>
        /// Hardware rivision
        /// </summary>
        public int HardwareRevision { get; private set; }

        /// <summary>
        /// SHDLC protocol major version
        /// </summary>
        public int SHDLCProtocolMajorVersion { get; private set; }

        /// <summary>
        /// SHDCL protocol minor version
        /// </summary>
        public int SHDLCProtocolMinorVersion { get; private set; }

        /// <summary>
        /// Conveniently show the version information in a single string.
        /// </summary>
        /// <returns>The version information as a convenient string</returns>
        public override string ToString()
        {
            return $"Firmware V{FirmwareMajorVersion}.{FirmwareMinorVersion}, Hardware V{HardwareRevision}, SHDLC V{SHDLCProtocolMajorVersion}.{SHDLCProtocolMinorVersion}";
        }

    }
}
