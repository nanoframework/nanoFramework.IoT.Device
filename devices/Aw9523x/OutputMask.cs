// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Aw9523x
{
    /// <summary>
    /// AW9523X output-port bit mask.
    /// </summary>
    [Flags]
    public enum OutputMask : byte
    {
        /// <summary>
        /// No bits selected.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Selects port bit 0.
        /// </summary>
        PortBit0 = 0x01,

        /// <summary>
        /// Selects port bit 1.
        /// </summary>
        PortBit1 = 0x02,

        /// <summary>
        /// Selects port bit 2.
        /// </summary>
        PortBit2 = 0x04,

        /// <summary>
        /// Selects port bit 3.
        /// </summary>
        PortBit3 = 0x08,

        /// <summary>
        /// Selects port bit 4.
        /// </summary>
        PortBit4 = 0x10,

        /// <summary>
        /// Selects port bit 5.
        /// </summary>
        PortBit5 = 0x20,

        /// <summary>
        /// Selects port bit 6.
        /// </summary>
        PortBit6 = 0x40,

        /// <summary>
        /// Selects port bit 7.
        /// </summary>
        PortBit7 = 0x80,

        /// <summary>
        /// Selects all bits.
        /// </summary>
        All = 0xFF,
    }
}