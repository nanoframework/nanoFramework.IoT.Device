// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp2101
{
    /// <summary>
    /// Power output channels available on the AXP2101.
    /// </summary>
    public enum PowerChannel
    {
        /// <summary>DCDC1 switching regulator.</summary>
        DcDc1 = 0,

        /// <summary>DCDC2 switching regulator.</summary>
        DcDc2 = 1,

        /// <summary>DCDC3 switching regulator.</summary>
        DcDc3 = 2,

        /// <summary>DCDC4 switching regulator.</summary>
        DcDc4 = 3,

        /// <summary>DCDC5 switching regulator.</summary>
        DcDc5 = 4,

        /// <summary>ALDO1 linear regulator.</summary>
        Aldo1 = 5,

        /// <summary>ALDO2 linear regulator.</summary>
        Aldo2 = 6,

        /// <summary>ALDO3 linear regulator.</summary>
        Aldo3 = 7,

        /// <summary>ALDO4 linear regulator.</summary>
        Aldo4 = 8,

        /// <summary>BLDO1 linear regulator.</summary>
        Bldo1 = 9,

        /// <summary>BLDO2 linear regulator.</summary>
        Bldo2 = 10,

        /// <summary>CPUSLDO linear regulator.</summary>
        CpusLdo = 11,

        /// <summary>DLDO1 linear regulator.</summary>
        Dldo1 = 12,

        /// <summary>DLDO2 linear regulator.</summary>
        Dldo2 = 13,

        /// <summary>Backup battery (VBACKUP) channel.</summary>
        VBackup = 14,
    }
}
