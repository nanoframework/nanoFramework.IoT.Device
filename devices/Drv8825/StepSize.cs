// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Drv8825
{
    /// <summary>
    /// Enum for microsteps settings.
    /// </summary>
    public enum StepSize : byte
    {
        /// <summary>
        /// No microsteps (full step).
        /// </summary>
        FullStep = 1,

        /// <summary>
        /// 1/2 step.
        /// </summary>
        HalfStep = 2,

        /// <summary>
        /// 1/4 step.
        /// </summary>
        QuaterStep = 4,

        /// <summary>
        /// 1/8 step.
        /// </summary>
        EightStep = 8,

        /// <summary>
        /// 1/16 step.
        /// </summary>
        SixteenStep = 16,

        /// <summary>
        /// 1/32 step.
        /// </summary>
        ThirtyTwoStep = 32,
    }
}
