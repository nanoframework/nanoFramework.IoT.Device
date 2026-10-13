// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Apds9960
{
    /// <summary>
    /// APDS9960 proximity receiver gain.
    /// </summary>
    public enum ProximityGain : byte
    {
        /// <summary>One-times gain.</summary>
        One = 0,

        /// <summary>Two-times gain.</summary>
        Two = 1,

        /// <summary>Four-times gain.</summary>
        Four = 2,

        /// <summary>Eight-times gain.</summary>
        Eight = 3,
    }
}
