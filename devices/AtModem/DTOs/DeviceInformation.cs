// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.AtModem.DTOs
{
    /// <summary>
    /// Device Information. 
    /// </summary>
    public class DeviceInformation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceInformation"/> class. 
        /// </summary>
        public DeviceInformation()
        {
        }

        /// <summary>
        /// Gets or sets the name of the device manufacturer.
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets  the device model name.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets  the device product number.
        /// </summary>
        public string ProductNumber { get; set; }

        /// <summary>
        /// Gets or sets  the current firmware version.
        /// </summary>
        public string FirmwareVersion { get; set; }

        /// <summary>
        /// Gets or sets the network system mode for wireless and cellular network communication service.
        /// </summary>
        public SystemMode SystemMode { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Manufacturer: {Manufacturer}, Model: {Model}, ProductNumber: {ProductNumber}, FirmwareVersion: {FirmwareVersion}, SystemMode: {SystemMode}";
        }
    }
}
