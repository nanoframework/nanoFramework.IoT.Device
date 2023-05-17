////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

using System;

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// Charger status 0.
    /// </summary>
    [Flags]
    public enum ChargerStatus0 : byte
    {
        /// <summary>
        /// IINDPM status (forward mode) or IOTG status (OTG mode).
        /// </summary>
        /// <remarks><see langword="true"/> if in INDPM regulation or IOTG regulation, <see langword="false"/> for normal regulation.</remarks>
        InDpm = 0b1000_0000,

        /// <summary>
        /// VINDPM status (forward mode) or VOTG status (OTG mode).
        /// </summary>
        /// <remarks><see langword="true"/> if in VINDPM regulation or VOTG regualtion, <see langword="false"/> for normal mode.</remarks>
        VinDpm = 0b0100_0000,

        /// <summary>
        /// I2C watch dog timer status.
        /// </summary>
        /// <remarks><see langword="true"/> if I2C watchdog timer expired, <see langword="false"/> for normal status.</remarks>
        I2cWatchdogExpired = 0b0010_0000,

        /// <summary>
        /// Power Good Status.
        /// </summary>
        /// <remarks><see langword="true"/> if power good, <see langword="false"/> for power NOT good.</remarks>
        PowerGood = 0b0000_1000,

        /// <summary>
        /// VAC2 insert status.
        /// </summary>
        /// <remarks><see langword="true"/>  VAC2 present (above present threshold), <see langword="false"/> for VAC2 NOT present.</remarks>
        Vac2Inserted = 0b0000_0100,

        /// <summary>
        /// VAC1 insert status.
        /// </summary>
        /// <remarks><see langword="true"/>  VAC1 present (above present threshold), <see langword="false"/> for VAC1 NOT present.</remarks>
        Vac1Inserted = 0b0000_0010,

        /// <summary>
        /// VBUS present status.
        /// </summary>
        /// <remarks><see langword="true"/>  VBUS present (above present threshold), <see langword="false"/> for VBUS NOT present.</remarks>
        VbusPresent = 0b0000_0001,
    }
}
