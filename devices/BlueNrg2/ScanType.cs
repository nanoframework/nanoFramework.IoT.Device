// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2
{
    /// <summary>
    /// If the scan is active or passive.
    /// </summary>
    public enum ScanType : byte
    {
        /// <summary>
        /// Passive scan.
        /// </summary>
        Passive = 0x00,
        /// <summary>
        /// Active scan.
        /// </summary>
        Active = 0x01
    }
}
