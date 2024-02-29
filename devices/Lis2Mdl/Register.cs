// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lis2Mdl
{
    internal enum Register : byte
    {
        /// <summary>
        /// WHO_AM_I register.
        /// </summary>
        WhoAmI = 0x4F,

        /// <summary>
        /// CFG_REG_A register.
        /// </summary>
        ConfigA = 0x60,

        /// <summary>
        /// CFG_REG_B register.
        /// </summary>
        ConfigB = 0x61,

        /// <summary>
        /// CFG_REG_C register.
        /// </summary>
        ConfigC = 0x62,

        /// <summary>
        /// STATUS_REG register.
        /// </summary>
        Satus = 0x67,

        /// <summary>
        /// OUTX_L_REG register.
        /// </summary>
        OutputXLow = 0x68,

        /// <summary>
        /// TEMP_OUT_L_REG register.
        /// </summary>
        TemperatureLow = 0x6E,
    }
}
