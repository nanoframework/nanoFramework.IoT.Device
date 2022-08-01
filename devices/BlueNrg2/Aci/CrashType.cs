// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Crash type.
    /// </summary>
    public enum CrashType : byte
    {
        /// <summary>
        /// Assert failed.
        /// </summary>
        AssertFailed = 0x00,

        /// <summary>
        /// NMI fault.
        /// </summary>
        NmiFault = 0x01,

        /// <summary>
        /// Hard fault.
        /// </summary>
        HardFault = 0x02
    }
}
