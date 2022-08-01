// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// The role of the device.
    /// </summary>
    public enum DeviceRole : byte
    {
        /// <summary>
        /// Master.
        /// </summary>
        Master = 0x00,

        /// <summary>
        /// Slave.
        /// </summary>
        Slave = 0x01
    }
}
