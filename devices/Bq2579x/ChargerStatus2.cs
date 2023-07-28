////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

using System;

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// Charger status 2.
    /// </summary>
    [Flags]
    public enum ChargerStatus2 : byte
    {
        /// <summary>
        /// ICO msb.
        /// </summary>
        IcoMsb = 0b1000_0000,

        /// <summary>
        /// ICO lsb.
        /// </summary>
        IcoLsb = 0b0100_0000,

        /// <summary>
        /// C thermal regulation status.
        /// </summary>
        /// <remarks><see langword="true"/> Device in thermal regulation, <see langword="false"/> for normal operation.</remarks>
        ThermalRegulation = 0b0000_0100,

        /// <summary>
        /// VAC1 insert status.
        /// </summary>
        /// <remarks><see langword="true"/> The D+/D- detection is ongoing, <see langword="false"/> for D+/D- detection NOT started yet, or the detection is done.</remarks>
        DpDmStatus = 0b0000_0010,

        /// <summary>
        /// VBUS present status.
        /// </summary>
        /// <remarks><see langword="true"/> VBAT present, <see langword="false"/> VBAT NOT present.</remarks>
        VbatPresent = 0b0000_0001,
    }
}
