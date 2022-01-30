// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Am2320
{
    /// <summary>
    /// Device information.
    /// </summary>
    public class DeviceInformation
    {
        /// <summary>
        /// The model.
        /// </summary>
        public ushort Model { get; set; }

        /// <summary>
        /// The version.
        /// </summary>
        public byte Version { get; set; }

        /// <summary>
        /// The device ID
        /// </summary>
        public uint DeviceId { get; set; }
    }
}
