// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ds18b20
{
    /// <summary>
    /// ROM commands, see data sheet, page 10, section DS18B20 ROM Commands.
    /// </summary>
    public enum RomCommands : byte
    {
        /// <summary>
        /// Reference family code.
        /// </summary>
        FamilyCode = 0x28,

        /// <summary>
        /// Command to address specific device on network.
        /// </summary>
        Match = 0x55,

        /// <summary>
        /// Command to address all devices on the bus simultaneously.
        /// </summary>
        Skip = 0xcc,
    }
}