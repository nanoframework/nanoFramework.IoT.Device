// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.AcmeBinding
{
    /// <summary>
    /// Operating mode of the synthetic Acme device.
    /// </summary>
    public enum AcmeMode
    {
        /// <summary>
        /// Device is idle.
        /// </summary>
        Idle,

        /// <summary>
        /// Device is actively sampling.
        /// </summary>
        Active,

        /// <summary>
        /// Device is running its calibration routine.
        /// </summary>
        Calibrating
    }
}
