// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Apds9960
{
    /// <summary>
    /// APDS9960 ambient light ADC gain.
    /// </summary>
    public enum AmbientLightGain : byte
    {
        /// <summary>One-times gain.</summary>
        One = 0,

        /// <summary>Four-times gain.</summary>
        Four = 1,

        /// <summary>Sixteen-times gain.</summary>
        Sixteen = 2,

        /// <summary>Sixty-four-times gain.</summary>
        SixtyFour = 3,
    }
}
