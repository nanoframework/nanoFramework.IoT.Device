// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Aw9523x
{
    /// <summary>
    /// AW9523X register map.
    /// </summary>
    public enum Register : byte
    {
        /// <summary>
        /// Input Port 0 register.
        /// </summary>
        InputPort0 = 0x00,

        /// <summary>
        /// Input Port 1 register.
        /// </summary>
        InputPort1 = 0x01,

        /// <summary>
        /// Output Port 0 register.
        /// </summary>
        OutputPort0 = 0x02,

        /// <summary>
        /// Output Port 1 register.
        /// </summary>
        OutputPort1 = 0x03,

        /// <summary>
        /// Direction Port 0 register.
        /// </summary>
        DirectionPort0 = 0x04,

        /// <summary>
        /// Direction Port 1 register.
        /// </summary>
        DirectionPort1 = 0x05,

        /// <summary>
        /// Interrupt enable Port 0 register.
        /// </summary>
        InterruptEnablePort0 = 0x06,

        /// <summary>
        /// Interrupt enable Port 1 register.
        /// </summary>
        InterruptEnablePort1 = 0x07,

        /// <summary>
        /// Global control register.
        /// </summary>
        GlobalControl = 0x11,

        /// <summary>
        /// Chip identification register.
        /// </summary>
        ChipId = 0x10,
    }
}
