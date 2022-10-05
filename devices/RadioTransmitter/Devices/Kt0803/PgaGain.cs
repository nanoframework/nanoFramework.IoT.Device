// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.RadioTransmitter
{
    /// <summary>
    /// PGA (Programmable Gain Amplifier) Gain.
    /// </summary>
    public enum PgaGain : byte
    {
        /// <summary>
        /// PgaGain 0 dB.
        /// </summary>
        Pga00dB = 4,

        /// <summary>
        /// PgaGain 4 dB.
        /// </summary>
        Pga04dB = 5,

        /// <summary>
        /// PgaGain 8 dB.
        /// </summary>
        Pga08dB = 6,

        /// <summary>
        /// PgaGain 12 dB.
        /// </summary>
        Pga12dB = 7,

        /// <summary>
        /// PgaGain -4 dB.
        /// </summary>
        PgaN04dB = 1,

        /// <summary>
        /// PgaGain -8 dB.
        /// </summary>
        PgaN08dB = 2,

        /// <summary>
        /// PgaGain -12 dB.
        /// </summary>
        PgaN12dB = 3,
    }
}
