// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ltr553AlsWa
{
    /// <summary>
    /// PS LED pulse frequency setting for the LTR-553ALS-WA.
    /// </summary>
    public enum LedPulseFrequency : byte
    {
        /// <summary>30 kHz pulse frequency.</summary>
        Frequency30kHz = 0x00,

        /// <summary>40 kHz pulse frequency.</summary>
        Frequency40kHz = 0x01,

        /// <summary>50 kHz pulse frequency.</summary>
        Frequency50kHz = 0x02,

        /// <summary>60 kHz pulse frequency (default).</summary>
        Frequency60kHz = 0x03,

        /// <summary>70 kHz pulse frequency.</summary>
        Frequency70kHz = 0x04,

        /// <summary>80 kHz pulse frequency.</summary>
        Frequency80kHz = 0x05,

        /// <summary>90 kHz pulse frequency.</summary>
        Frequency90kHz = 0x06,

        /// <summary>100 kHz pulse frequency.</summary>
        Frequency100kHz = 0x07,
    }
}
