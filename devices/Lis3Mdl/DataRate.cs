// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lis3Mdl
{
    /// <summary>
    /// Magnetic field output data rate.
    /// </summary>
    public enum DataRate : byte
    {
        /// <summary>A rate of 0.625 Hz.</summary>
        Rate0Point625Hz = 0b0000,

        /// <summary>A rate of 1.25 Hz.</summary>
        Rate1Point25Hz = 0b0010,

        /// <summary>A rate of 2.5 Hz.</summary>
        Rate2Point5Hz = 0b0100,

        /// <summary>A rate of 5 Hz.</summary>
        Rate5Hz = 0b0110,

        /// <summary>A rate of 10 Hz.</summary>
        Rate10Hz = 0b1000,

        /// <summary>A rate of 20 Hz.</summary>
        Rate20Hz = 0b1010,

        /// <summary>A rate of 40 Hz.</summary>
        Rate40Hz = 0b1100,

        /// <summary>A rate of 80 Hz.</summary>
        Rate80Hz = 0b1110,

        /// <summary>A rate of 155 Hz.</summary>
        Rate155Hz = 0b0001,

        /// <summary>A rate of 300 Hz.</summary>
        Rate300Hz = 0b0011,

        /// <summary>A rate of 560 Hz.</summary>
        Rate560Hz = 0b0101,

        /// <summary>A rate of 1000 Hz.</summary>
        Rate1000Hz = 0b0111,
    }
}