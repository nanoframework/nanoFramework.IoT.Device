// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ltr553AlsWa
{
    /// <summary>
    /// PS LED duty cycle setting for the LTR-553ALS-WA.
    /// </summary>
    public enum LedDutyCycle : byte
    {
        /// <summary>25% duty cycle.</summary>
        DutyCycle25Percent = 0x00,

        /// <summary>50% duty cycle.</summary>
        DutyCycle50Percent = 0x01,

        /// <summary>75% duty cycle.</summary>
        DutyCycle75Percent = 0x02,

        /// <summary>100% duty cycle (default).</summary>
        DutyCycle100Percent = 0x03,
    }
}
