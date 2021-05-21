// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Pwm
{
    /// <summary>
    /// Values for Mode2 register
    /// </summary>
    internal enum Mode2 : byte
    {
        INVRT = 0b00010000, // Bit 4
        OCH = 0b00001000, // Bit 3
        OUTDRV = 0b00000100 // Bit 2
    }
}
