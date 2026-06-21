// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Axp2101
{
    /// <summary>
    /// LDO channel enable flags from LDO_ONOFF_CTRL0 (0x90) and LDO_ONOFF_CTRL1 (0x91) registers.
    /// Bits [7:0] of register 0x90 map to ALDO1-4, BLDO1-2, CPUSLDO, DLDO1.
    /// Bit 0 of register 0x91 maps to DLDO2.
    /// </summary>
    [Flags]
    public enum LdoPinsEnabled
    {
        /// <summary>ALDO1 enabled (register 0x90, bit 0).</summary>
        Aldo1 = 0b0_0000_0001,

        /// <summary>ALDO2 enabled (register 0x90, bit 1).</summary>
        Aldo2 = 0b0_0000_0010,

        /// <summary>ALDO3 enabled (register 0x90, bit 2).</summary>
        Aldo3 = 0b0_0000_0100,

        /// <summary>ALDO4 enabled (register 0x90, bit 3).</summary>
        Aldo4 = 0b0_0000_1000,

        /// <summary>BLDO1 enabled (register 0x90, bit 4).</summary>
        Bldo1 = 0b0_0001_0000,

        /// <summary>BLDO2 enabled (register 0x90, bit 5).</summary>
        Bldo2 = 0b0_0010_0000,

        /// <summary>CPUSLDO enabled (register 0x90, bit 6).</summary>
        CpusLdo = 0b0_0100_0000,

        /// <summary>DLDO1 enabled (register 0x90, bit 7).</summary>
        Dldo1 = 0b0_1000_0000,

        /// <summary>DLDO2 enabled (register 0x91, bit 0 — mapped to bit 8 in this enum).</summary>
        Dldo2 = 0b1_0000_0000,
    }
}
