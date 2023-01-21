// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ds1621
{
    internal enum Register : byte
    {
        /// <summary>
        /// Stores the last temperature conversion result.
        /// </summary>
        Temperature = 0xAA,

        /// <summary>
        /// Reads or writes to the high temperature register.
        /// </summary>
        HighTemperature = 0xA1,

        /// <summary>
        /// Reads or writes to the low temperature register.
        /// </summary>
        LowTemperature = 0xA2,

        /// <summary>
        /// Reads or writes to the configuration register.
        /// </summary>
        Configuration = 0xAC,

        /// <summary>
        /// Reads counts remaining register.
        /// </summary>
        CountsRemaining = 0xA8,

        /// <summary>
        /// Reads counts per degree register.
        /// </summary>
        CountsPerDegree = 0xA9
    }
}
