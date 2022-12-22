// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Si7021
{
    /// <summary>
    /// Si7021 firmware revision.
    /// </summary>
    public enum FirmwareRevision : byte
    {
        /// <summary>
        /// Unknown firmware revision.
        /// </summary>
        Unknow = 00,

        /// <summary>
        /// Firmware version 2.0.
        /// </summary>
        V2_0 = 0x20,

        /// <summary>
        /// Firmware version 1.0.
        /// </summary>
        V1_0 = 0xFF,
    }
}
