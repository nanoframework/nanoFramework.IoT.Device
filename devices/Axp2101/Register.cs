// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp2101
{
    /// <summary>
    /// AXP2101 register map.
    /// </summary>
    internal enum Register
    {
        // ---- Status & Chip ID ----

        /// <summary>PMU status 1: VBUS good, battery present, thermal regulation, current limit.</summary>
        Status1 = 0x00,

        /// <summary>PMU status 2: charging/discharging state, power on/off status.</summary>
        Status2 = 0x01,

        /// <summary>Chip ID register (should read 0x4A for AXP2101).</summary>
        IcType = 0x03,

        // ---- Data Buffer (4 bytes of user storage) ----

        /// <summary>User data buffer byte 0.</summary>
        DataBuffer0 = 0x04,

        /// <summary>User data buffer byte 1.</summary>
        DataBuffer1 = 0x05,

        /// <summary>User data buffer byte 2.</summary>
        DataBuffer2 = 0x06,

        /// <summary>User data buffer byte 3.</summary>
        DataBuffer3 = 0x07,

        // ---- Common Config & Power Control ----

        /// <summary>Common config: internal discharge, PWROK pull-low, PWRON shutdown, reset, shutdown.</summary>
        CommonConfig = 0x10,

        /// <summary>BATFET enable/disable control.</summary>
        BatfetCtrl = 0x12,

        /// <summary>Die over-temperature detection level and enable.</summary>
        DieTempCtrl = 0x13,

        /// <summary>Linear charger Vsys DPM voltage (minimum system voltage).</summary>
        MinSysVolCtrl = 0x14,

        /// <summary>VBUS voltage input limit.</summary>
        InputVolLimitCtrl = 0x15,

        /// <summary>VBUS current input limit.</summary>
        InputCurLimitCtrl = 0x16,

        /// <summary>Fuel gauge reset control.</summary>
        ResetFuelGauge = 0x17,

        /// <summary>Button battery charge, cell battery charge enable, watchdog enable.</summary>
        ChargeGaugeWdtCtrl = 0x18,

        /// <summary>Watchdog config, timeout, and clear.</summary>
        WdtCtrl = 0x19,

        /// <summary>Low battery warning and shutdown voltage thresholds.</summary>
        LowBatWarnSet = 0x1A,

        // ---- Power On/Off Status ----

        /// <summary>Power-on source detection.</summary>
        PwronStatus = 0x20,

        /// <summary>Power-off source detection.</summary>
        PwroffStatus = 0x21,

        /// <summary>Over-temperature power-off and long-press behavior enable.</summary>
        PwroffEn = 0x22,

        /// <summary>DCDC over-voltage / under-voltage protection control.</summary>
        DcOvpUvpCtrl = 0x23,

        /// <summary>System power-down voltage (VSYS threshold).</summary>
        VoffSet = 0x24,

        /// <summary>PWROK pin and power sequence control.</summary>
        PwrokSequCtrl = 0x25,

        /// <summary>Sleep and wakeup control.</summary>
        SleepWakeupCtrl = 0x26,

        /// <summary>IRQ/OFF/ON press timing control.</summary>
        IrqOffOnLevelCtrl = 0x27,

        // ---- Fast Power-On Sequence ----

        /// <summary>DCDC1-4 fast power-on startup sequence.</summary>
        FastPwronSet0 = 0x28,

        /// <summary>DCDC5, ALDO1-3 fast power-on startup sequence.</summary>
        FastPwronSet1 = 0x29,

        /// <summary>ALDO4, BLDO1-2, CPUSLDO fast power-on startup sequence.</summary>
        FastPwronSet2 = 0x2A,

        /// <summary>DLDO1-2, fast power-on and wakeup enable.</summary>
        FastPwronCtrl = 0x2B,

        // ---- ADC ----

        /// <summary>ADC channel enable (battery, TS, VBUS, system, die temperature, general).</summary>
        AdcChannelCtrl = 0x30,

        /// <summary>Battery voltage ADC result high byte.</summary>
        AdcDataBatteryVoltageHigh = 0x34,

        /// <summary>Battery voltage ADC result low byte.</summary>
        AdcDataBatteryVoltageLow = 0x35,

        /// <summary>TS pin voltage ADC result high byte.</summary>
        AdcDataTsVoltageHigh = 0x36,

        /// <summary>TS pin voltage ADC result low byte.</summary>
        AdcDataTsVoltageLow = 0x37,

        /// <summary>VBUS voltage ADC result high byte.</summary>
        AdcDataVbusVoltageHigh = 0x38,

        /// <summary>VBUS voltage ADC result low byte.</summary>
        AdcDataVbusVoltageLow = 0x39,

        /// <summary>VSYS voltage ADC result high byte.</summary>
        AdcDataVsysVoltageHigh = 0x3A,

        /// <summary>VSYS voltage ADC result low byte.</summary>
        AdcDataVsysVoltageLow = 0x3B,

        /// <summary>Die temperature ADC result high byte.</summary>
        AdcDataDieTempHigh = 0x3C,

        /// <summary>Die temperature ADC result low byte.</summary>
        AdcDataDieTempLow = 0x3D,

        // ---- Interrupts ----

        /// <summary>Interrupt enable register 0.</summary>
        IrqEnable0 = 0x40,

        /// <summary>Interrupt enable register 1.</summary>
        IrqEnable1 = 0x41,

        /// <summary>Interrupt enable register 2.</summary>
        IrqEnable2 = 0x42,

        /// <summary>Interrupt status register 0 (write 1 to clear).</summary>
        IrqStatus0 = 0x48,

        /// <summary>Interrupt status register 1 (write 1 to clear).</summary>
        IrqStatus1 = 0x49,

        /// <summary>Interrupt status register 2 (write 1 to clear).</summary>
        IrqStatus2 = 0x4A,

        // ---- TS Pin & JEITA ----

        /// <summary>TS pin function control.</summary>
        TsPinCtrl = 0x50,

        /// <summary>TS hysteresis low-to-high threshold setting.</summary>
        TsHysL2hSet = 0x52,

        /// <summary>TS hysteresis high-to-low threshold setting.</summary>
        TsHysH2lSet = 0x53,

        /// <summary>Charge temperature threshold VLTF setting.</summary>
        VltfChgSet = 0x54,

        /// <summary>Charge temperature threshold VHTF setting.</summary>
        VhltfChgSet = 0x55,

        /// <summary>Work temperature threshold VLTF setting.</summary>
        VltfWorkSet = 0x56,

        /// <summary>Work temperature threshold VHTF setting.</summary>
        VhltfWorkSet = 0x57,

        /// <summary>JEITA enable control.</summary>
        JeitaEnCtrl = 0x58,

        /// <summary>JEITA configuration register 0.</summary>
        JeitaSet0 = 0x59,

        /// <summary>JEITA configuration register 1.</summary>
        JeitaSet1 = 0x5A,

        /// <summary>JEITA configuration register 2.</summary>
        JeitaSet2 = 0x5B,

        // ---- Charging ----

        /// <summary>Pre-charge current setting (0-200 mA in 25 mA steps).</summary>
        IprechgSet = 0x61,

        /// <summary>Constant charge current setting (0-1000 mA).</summary>
        IccChgSet = 0x62,

        /// <summary>Charge termination current setting and enable.</summary>
        ItermChgSetCtrl = 0x63,

        /// <summary>Charge target voltage setting.</summary>
        CvChgVolSet = 0x64,

        /// <summary>Thermal regulation threshold setting.</summary>
        TheReguThresSet = 0x65,

        /// <summary>Charge timeout control.</summary>
        ChgTimeoutSetCtrl = 0x67,

        /// <summary>Battery detection enable.</summary>
        BatDetCtrl = 0x68,

        /// <summary>Charge LED mode control.</summary>
        ChgledSetCtrl = 0x69,

        /// <summary>Button battery charge voltage setting.</summary>
        BtnBatChgVolSet = 0x6A,

        // ---- DCDC Control ----

        /// <summary>DCDC1-5 on/off, DVM ramp control, CCM enable.</summary>
        DcOnOffDvmCtrl = 0x80,

        /// <summary>DCDC force PWM mode and frequency spread control.</summary>
        DcForcePwmCtrl = 0x81,

        /// <summary>DCDC1 voltage setting.</summary>
        DcVol0Ctrl = 0x82,

        /// <summary>DCDC2 voltage setting.</summary>
        DcVol1Ctrl = 0x83,

        /// <summary>DCDC3 voltage setting.</summary>
        DcVol2Ctrl = 0x84,

        /// <summary>DCDC4 voltage setting.</summary>
        DcVol3Ctrl = 0x85,

        /// <summary>DCDC5 voltage setting.</summary>
        DcVol4Ctrl = 0x86,

        // ---- LDO Control ----

        /// <summary>ALDO1-4, BLDO1-2, CPUSLDO, DLDO1 on/off control.</summary>
        LdoOnOffCtrl0 = 0x90,

        /// <summary>DLDO2 on/off control.</summary>
        LdoOnOffCtrl1 = 0x91,

        /// <summary>ALDO1 voltage setting.</summary>
        LdoVol0Ctrl = 0x92,

        /// <summary>ALDO2 voltage setting.</summary>
        LdoVol1Ctrl = 0x93,

        /// <summary>ALDO3 voltage setting.</summary>
        LdoVol2Ctrl = 0x94,

        /// <summary>ALDO4 voltage setting.</summary>
        LdoVol3Ctrl = 0x95,

        /// <summary>BLDO1 voltage setting.</summary>
        LdoVol4Ctrl = 0x96,

        /// <summary>BLDO2 voltage setting.</summary>
        LdoVol5Ctrl = 0x97,

        /// <summary>CPUSLDO voltage setting.</summary>
        LdoVol6Ctrl = 0x98,

        /// <summary>DLDO1 voltage setting.</summary>
        LdoVol7Ctrl = 0x99,

        /// <summary>DLDO2 voltage setting.</summary>
        LdoVol8Ctrl = 0x9A,

        // ---- Fuel Gauge ----

        /// <summary>Battery parameters data (128-byte serial write).</summary>
        BatParams = 0xA1,

        /// <summary>Fuel gauge control register.</summary>
        FuelGaugeCtrl = 0xA2,

        /// <summary>Battery percentage readout (0-100%).</summary>
        BatPercentData = 0xA4,
    }
}
