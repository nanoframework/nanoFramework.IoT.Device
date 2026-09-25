// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lis3Mdl
{
    /// <summary>
    /// Magnetic field conversion mode.
    /// </summary>
    public enum OperationMode : byte
    {
        /// <summary>Continuous conversion.</summary>
        Continuous = 0b00,

        /// <summary>Single conversion.</summary>
        Single = 0b01,

        /// <summary>Power-down mode.</summary>
        PowerDown = 0b11,
    }
}