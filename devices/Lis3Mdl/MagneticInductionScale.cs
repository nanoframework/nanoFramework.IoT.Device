// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lis3Mdl
{
    /// <summary>
    /// Magnetic induction full-scale range.
    /// </summary>
    public enum MagneticInductionScale : byte
    {
        /// <summary>Plus or minus 4 gauss.</summary>
        Scale04G = 0b00,

        /// <summary>Plus or minus 8 gauss.</summary>
        Scale08G = 0b01,

        /// <summary>Plus or minus 12 gauss.</summary>
        Scale12G = 0b10,

        /// <summary>Plus or minus 16 gauss.</summary>
        Scale16G = 0b11,
    }
}