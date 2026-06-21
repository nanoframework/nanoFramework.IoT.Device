// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Axp2101
{
    /// <summary>
    /// AXP2101 interrupt source flags across 3 interrupt registers (0x40-0x42 enable, 0x48-0x4A status).
    /// </summary>
    [Flags]
    public enum Axp2101Irq
    {
        // ---- IRQ Register 0 (0x40 / 0x48) ----

        /// <summary>Battery under temperature in work mode IRQ.</summary>
        BatteryNormalUnderTemperature = 0x00000001,

        /// <summary>Battery over temperature in work mode IRQ.</summary>
        BatteryNormalOverTemperature = 0x00000002,

        /// <summary>Battery under temperature in charge mode IRQ.</summary>
        BatteryChargeUnderTemperature = 0x00000004,

        /// <summary>Battery over temperature in charge mode IRQ.</summary>
        BatteryChargeOverTemperature = 0x00000008,

        /// <summary>Gauge new SOC (state of charge) IRQ.</summary>
        GaugeNewSoc = 0x00000010,

        /// <summary>Gauge watchdog timeout IRQ.</summary>
        WatchdogTimeout = 0x00000020,

        /// <summary>SOC drop to warning level 1 IRQ.</summary>
        WarningLevel1 = 0x00000040,

        /// <summary>SOC drop to warning level 2 IRQ.</summary>
        WarningLevel2 = 0x00000080,

        // ---- IRQ Register 1 (0x41 / 0x49) ----

        /// <summary>POWERON positive edge IRQ.</summary>
        PowerKeyPositive = 0x00000100,

        /// <summary>POWERON negative edge IRQ.</summary>
        PowerKeyNegative = 0x00000200,

        /// <summary>POWERON long press IRQ.</summary>
        PowerKeyLongPress = 0x00000400,

        /// <summary>POWERON short press IRQ.</summary>
        PowerKeyShortPress = 0x00000800,

        /// <summary>Battery removed IRQ.</summary>
        BatteryRemove = 0x00001000,

        /// <summary>Battery inserted IRQ.</summary>
        BatteryInsert = 0x00002000,

        /// <summary>VBUS removed IRQ.</summary>
        VbusRemove = 0x00004000,

        /// <summary>VBUS inserted IRQ.</summary>
        VbusInsert = 0x00008000,

        // ---- IRQ Register 2 (0x42 / 0x4A) ----

        /// <summary>Battery over voltage protection IRQ.</summary>
        BatteryOverVoltage = 0x00010000,

        /// <summary>Charger safety timer expire IRQ.</summary>
        ChargerTimer = 0x00020000,

        /// <summary>Die over temperature level 1 IRQ.</summary>
        DieOverTemperature = 0x00040000,

        /// <summary>Charger start IRQ.</summary>
        BatteryChargeStart = 0x00080000,

        /// <summary>Battery charge done IRQ.</summary>
        BatteryChargeDone = 0x00100000,

        /// <summary>BATFET over current protection IRQ.</summary>
        BatfetOverCurrent = 0x00200000,

        /// <summary>LDO over current IRQ.</summary>
        LdoOverCurrent = 0x00400000,

        /// <summary>Watchdog expire IRQ.</summary>
        WatchdogExpire = 0x00800000,

        /// <summary>All IRQ sources.</summary>
        All = 0x00FFFFFF,
    }
}
