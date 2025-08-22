// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Adc128D818
{
    /// <summary>
    /// ADC128D818 voltage reference options.
    /// </summary>
    public enum VoltageReference : byte
    {
        /// <summary>
        /// Internal 2.56V reference.
        /// </summary>
        Internal2_56 = 0,

        /// <summary>
        /// External reference voltage.
        /// </summary>
        External = 1,
    }
}
