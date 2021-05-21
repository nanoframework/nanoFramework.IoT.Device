// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Amg88xx
{
    /// <summary>
    /// Defines the reset types of the reset register (addr: 0x01)
    /// </summary>
    internal enum ResetType : byte
    {
        /// <summary>
        /// flag reset (clear all flags and interrupt flag registers)
        /// </summary>
        Flag = 0x30,

        /// <summary>
        /// initial reset (set all registers to defaults)
        /// </summary>
        Initial = 0x3f
    }
}
