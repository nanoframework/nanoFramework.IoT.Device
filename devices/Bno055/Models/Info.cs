// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Bno055
{
    /// <summary>
    /// Information for the various sensor ID, firmware and bootloader versions.
    /// </summary>
    public class Info
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Info" /> class.
        /// </summary>
        /// <param name="chipId">Chip identifier.</param>
        /// <param name="acceleratorId">Accelerometer identifier.</param>
        /// <param name="magnetometerId">Magnetometer identifier.</param>
        /// <param name="gyroscopeId">Gyroscope identifier.</param>
        /// <param name="firmwareVersion">Firmware version.</param>
        /// <param name="bootloaderVersion">Bootloader version.</param>
        public Info(byte chipId, byte acceleratorId, byte magnetometerId, byte gyroscopeId, Version firmwareVersion, Version bootloaderVersion)
        {
            ChipId = chipId;
            AcceleratorId = acceleratorId;
            MagnetometerId = magnetometerId;
            GyroscopeId = gyroscopeId;
            FirmwareVersion = firmwareVersion;
            BootloaderVersion = bootloaderVersion;
        }

        /// <summary>
        /// Gets or sets chip identifier.
        /// </summary>
        public byte ChipId { get; set; }

        /// <summary>
        /// Gets or sets accelerometer identifier.
        /// </summary>
        public byte AcceleratorId { get; set; }

        /// <summary>
        /// Gets or sets magnetometer identifier.
        /// </summary>
        public byte MagnetometerId { get; set; }

        /// <summary>
        /// Gets or sets gyroscope identifier.
        /// </summary>
        public byte GyroscopeId { get; set; }

        /// <summary>
        /// Gets or sets firmware version.
        /// </summary>
        public Version FirmwareVersion { get; set; }

        /// <summary>
        /// Gets or sets bootloader version.
        /// </summary>
        public Version BootloaderVersion { get; set; }
    }
}
