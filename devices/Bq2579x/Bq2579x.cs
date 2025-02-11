////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

using System;
using System.Device.I2c;
using System.Device.Model;
using UnitsNet;

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// Buck-Boost Battery Charger Bq2579x.
    /// </summary>
    [Interface("Buck-Boost Battery Charger Bq2579x")]
    public class Bq2579x : IDisposable
    {
        private const byte DevicePartNumberMask = 0b00111000;

        /// <summary>
        /// Bq25792/8 Default I2C Address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x6B;

        private Bq2579xHelpers _helpers;
        private I2cDevice _i2cDevice;
        private Model _deviceModel;

        /// <summary>
        /// Gets de model of the Bq2579x device connected.
        /// </summary>
        public Model Model => _deviceModel;

        /// <summary>
        /// Gets the charge status.
        /// </summary>
        [Property]
        public ChargeStatus ChargeStatus => _helpers.GetChargeStatus();

        /// <summary>
        /// Gets the VBUS status.
        /// </summary>
        [Property]
        public VbusStatus VbusStatus => _helpers.GetVbusStatus();

        /// <summary>
        /// Gets BC1.2 or non-standard detection.
        /// </summary>
        /// <value><see langword="true"/> if BC1.2 or non-standard detection is complete, otherwise <see langword="false"/>.</value>
        [Property]
        public bool Bc12Detection => _helpers.GetBc12Detection();

        /// <summary>
        /// Gets or sets a value indicating whether the ADC is enable.
        /// </summary>
        /// <value><see langword="true"/> to enable ADC, otherwise <see langword="false"/>.</value>
        [Property]
        public bool AdcEnable { get => _helpers.GetAdcEnable(); set => _helpers.SetAdcEnable(value); }

        /// <summary>
        /// Gets or sets ADC conversion rate.
        /// </summary>
        [Property]
        public AdcConversioRate AdcConversionRate { get => _helpers.GetAdcConversionRate(); set => _helpers.SetAdcConversionRate(value); }

        /// <summary>
        /// Gets or sets ADC resolution.
        /// </summary>
        [Property]
        public AdcResolution AdcResolution { get => _helpers.GetAdcResolution(); set => _helpers.SetAdcResolution(value); }

        /// <summary>
        /// Gets or sets ADC resolution.
        /// </summary>
        [Property]
        public AdcAveraging AdcAveraging { get => _helpers.GetAdcAveraging(); set => _helpers.SetAdcAveraging(value); }

        /// <summary>
        /// Gets or sets ADC resolution.
        /// </summary>
        [Property]
        public AdcInitialAverage AdcInitialAverage { get => _helpers.GetAdcInitialAverage(); set => _helpers.SetAdcInitialAverage(value); }

        /// <summary>
        /// Gets or sets minimal system voltage.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If value is out of range (2500mV-16000mV).</exception>
        /// <remarks>
        /// <para>
        /// During POR, the device reads the resistance tie to PROG pin, to identify the default battery cell count and determine the default power.
        /// </para>
        /// <para>
        /// Range : 2500mV-16000mV
        /// </para>
        /// </remarks>
        [Property]
        public ElectricPotentialDc MinimalSystemVoltage { get => _helpers.GetMinimalSystemVoltage(); set => _helpers.SetMinimalSystemVoltage(value); }

        /// <summary>
        /// Gets or sets Battery Voltage Limit.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If value is out of range (3000mV-18800mV).</exception>
        /// <remarks>
        /// <para>
        /// During POR, the device reads the resistance tie to PROG pin, to identify the default battery cell count and determine the default power-on battery voltage regulation limit.
        /// </para>
        /// <para>
        /// Range: 3000mV-18800mV.
        /// </para>
        /// </remarks>
        public ElectricPotentialDc ChargeVoltageLimit { get => _helpers.GetChargeVoltageLimit(); set => _helpers.SetChargeVoltageLimit(value); }

        /// <summary>
        /// Gets or sets Charge Current Limit.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If value is out of range (50mA-5000mA).</exception>
        /// <remarks>
        /// <para>
        /// During POR, the device reads the resistance tie to PROG pin, to identify the default battery cell count and determine the default power-on battery charging current: 1A.
        /// </para>
        /// <para>
        /// Range: 50mA-5000mA.
        /// </para>
        /// </remarks>
        public ElectricCurrent ChargeCurrentLimit { get => _helpers.GetChargeCurrentLimit(); set => _helpers.SetChargeCurrentLimit(value); }

        /// <summary>
        /// Gets or sets Input Voltage Limit.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If value is out of range (3600mV-22000mV).</exception>
        /// <remarks>
        /// <para>
        /// VINDPM register is reset to 3600mV upon adapter unplugged and  it is set to the value based on the VBUS measurement when the adapter plugs in. It is not reset by the REG_RST and the WATCHDOG.
        /// </para>
        /// <para>
        /// Range: 3600mV-22000mV.
        /// </para>
        /// </remarks>
        public ElectricPotentialDc InputVoltageLimit { get => _helpers.GetInputVoltageLimit(); set => _helpers.SetInputVoltageLimit(value); }

        /// <summary>
        /// Gets or sets Input Current Limit.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If value is out of range (100mA-3300mA).</exception>
        /// <remarks>
        /// <para>
        /// Based on D+/D- detection results:
        /// USB SDP = 500mA
        /// USB CDP = 1.5A
        /// USB DCP = 3.25A
        /// Adjustable High Voltage DCP = 1.5A
        /// Unknown Adapter = 3A
        /// Non-Standard Adapter = 1A/2A/2.1A/2.4A
        /// </para>
        /// <para>
        /// Range: 100mA-3300mA.
        /// </para>
        /// </remarks>
        public ElectricCurrent InputCurrentLimit { get => _helpers.GetInputCurrentLimit(); set => _helpers.SetInputCurrentLimit(value); }

        /// <summary>
        /// Gets or sets battery voltage thresholds for the transition from precharge to fast charge.
        /// </summary>
        /// <remarks>Defined as a ratio of battery regulation limit(VREG).</remarks>
        [Property]
        public ThresholdFastCharge FastChargeTransitionVoltage { get => _helpers.GetThresholdFastCharge(); set => _helpers.SetThresholdFastCharge(value); }

        /// <summary>
        /// Gets or sets precharge current limit (in steps of 40mA).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If value is out of range (40mA-2000mA).</exception>
        /// <remarks>Equivalent range: 40mA-2000mA.</remarks>
        [Property]
        public ElectricCurrent PrechargeCurrentLimit { get => _helpers.GetPrechargeCurrentLimit(); set => _helpers.SetPrechargeCurrentLimit(value); }

        /// <summary>
        /// Gets or sets termination current (in steps of 40mA).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If value is out of range (40mA-1000mA).</exception>"
        /// <remarks>Equivalent range: 40mA-1000mA.</remarks>
        public ElectricCurrent TerminationCurrent { get => _helpers.GetTerminationCurrent(); set => _helpers.SetTerminationCurrent(value); }

        /// <summary>
        /// Gets VBUS.
        /// </summary>
        /// <remarks>Range: 0mV-30000mV.</remarks>
        public ElectricPotentialDc Vbus => _helpers.GetAdcVoltage(Register.REG35_VBUS_ADC);

        /// <summary>
        /// Gets VAC1.
        /// </summary>
        /// <remarks>Range: 0mV-30000mV.</remarks>
        public ElectricPotentialDc Vac1 => _helpers.GetAdcVoltage(Register.REG37_VAC1_ADC);

        /// <summary>
        /// Gets VAC2.
        /// </summary>
        /// <remarks>Range: 0mV-30000mV.</remarks>
        public ElectricPotentialDc Vac2 => _helpers.GetAdcVoltage(Register.REG39_VAC2_ADC);

        /// <summary>
        /// Gets VBAT.
        /// </summary>
        /// <remarks>Range: 0mV-20000mV.</remarks>
        public ElectricPotentialDc Vbat => _helpers.GetAdcVoltage(Register.REG3B_VBAT_ADC);

        /// <summary>
        /// Gets VSYS.
        /// </summary>
        /// <remarks>Range: 0mV-24000mV.</remarks>
        public ElectricPotentialDc Vsys => _helpers.GetAdcVoltage(Register.REG3D_VSYS_ADC);

        /// <summary>
        /// Gets die temperature.
        /// </summary>
        /// <remarks>Range: -40°C -150°C.</remarks>
        public Temperature DieTemperature => _helpers.GetDieTemperature();

        /// <summary>
        /// Gets or sets the watchdog timer setting (in milliseconds).
        /// </summary>
        public WatchdogSetting WatchdogTimerSetting { get => _helpers.GetWatchdogTimerSetting(); set => _helpers.SetWatchdogTimerSetting(value); }

        /// <summary>
        /// Gets or sets the VAC OPV threshold.
        /// </summary>
        /// <remarks>
        /// Default value is 26V <see cref="VacOpvThreshold.Opv_26V"/>.
        /// </remarks>
        [Property]
        public VacOpvThreshold OverVoltageProtectionThreshold { get => _helpers.GetVacOpvThreshold(); set => _helpers.SetVacOpvThreshold(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the I2C watchdog is reset.
        /// </summary>
        /// <remarks>
        /// This goes back to <see langword="false"/> after the timer resets.
        /// </remarks>
        [Property]
        public bool ResetI2cWatchdog { get => _helpers.GetI2CWatchdogReset(); set => _helpers.SetI2CWatchdogReset(value); }

        /// <summary>
        /// Gets or sets Thermal regulation threshold.
        /// </summary>
        [Property]
        public ThermalRegulationThreshold ThermalRegulationThreshold { get => _helpers.GetThermalRegulationThreshold(); set => _helpers.SetThermalRegulationThreshold(value); }

        /// <summary>
        /// Gets or sets Thermal shutdown threshold.
        /// </summary>
        [Property]
        public ThermalShutdownThreshold ThermalShutdownThreshold { get => _helpers.GetThermalShutdownThreshold(); set => _helpers.SetThermalShutdownThreshold(value); }

        /// <summary>
        /// Gets or sets charge voltage for JEITA high temperature range (TWARN - THOT). 
        /// </summary>
        [Property]
        public ChargeVoltage ChargeVoltageHighTempRange { get => _helpers.GetChargeVoltage(); set => _helpers.SetChargeVoltage(value); }

        /// <summary>
        /// Gets or sets charge current for JEITA high temperature range (TWARN - THOT). 
        /// </summary>
        [Property]
        public ChargeCurrent ChargeCurrentHighTempRange { get => _helpers.GetChargeCurrentHighTempRange(); set => _helpers.SetChargeCurrentHighTempRange(value); }

        /// <summary>
        /// Gets or sets charge current for JEITA low temperature range (TCOLD - TCOOL). 
        /// </summary>
        [Property]
        public ChargeCurrent ChargeCurrentLowTempRange { get => _helpers.GetChargeCurrentLowTempRange(); set => _helpers.SetChargeCurrentLowTempRange(value); }

        /// <summary>
        /// Gets or sets VT2 comparator voltage rising thresholds as a percentage of REGN.
        /// </summary>
        [Property]
        public CompVoltageRisingThreshold Vt2ComparatorRisingThreshold { get => _helpers.GetVt2ComparatorVoltage(); set => _helpers.SetVt2ComparatorVoltage(value); }

        /// <summary>
        /// Gets or sets VT3 comparator voltage falling thresholds as a percentage of REGN.
        /// </summary>
        [Property]
        public CompVoltageFallingThreshold Vt3ComparatorFallingThreshold { get => _helpers.GetVt3ComparatorVoltage(); set => _helpers.SetVt3ComparatorVoltage(value); }

        /// <summary>
        /// Gets or sets OTG mode TS HOT temperature threshold.
        /// </summary>
        [Property]
        public OtgHotTempThreshold OtgHotTempThreshold { get => _helpers.GetOtgHotTempThreshold(); set => _helpers.SetOtgHotTempThreshold(value); }

        /// <summary>
        /// Gets or sets OTG mode TS COLD temperature threshold.
        /// </summary>
        [Property]
        public OtgColdTempThreshold OtgColdTempThreshold { get => _helpers.GetOtgColdTempThreshold(); set => _helpers.SetOtgColdTempThreshold(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the temperature sensor feedback should be ignored.
        /// </summary>
        /// <value><see langword="true"/> if ignoring temperature sensor feedback, otherwise <see langword="false"/>.</value>
        /// <remarks>
        /// Ignore the temperature sensor (TS) feedback, the charger considers the TS is always good to allow the charging and OTG modes. <see cref="Vt2ComparatorRisingThreshold"/> and <see cref="Vt3ComparatorFallingThreshold"/> always read 0 to report the normal condition.
        /// </remarks>
        [Property]
        public bool IgnoreTempSensor { get => _helpers.GetIgnoreTempSensor(); set => _helpers.SetIgnoreTempSensor(value); }

        /// <summary>
        /// Gets Power Good Status.
        /// </summary>
        /// <value><see langword="true"/> if power is good, otherwise <see langword="false"/>.</value>
        [Property]
        public bool IsPowerGood => _helpers.GetChargerStatus0().HasFlag(ChargerStatus0.PowerGood);

        /// <summary>
        /// Gets state of VAC2 inserted.
        /// </summary>
        /// <value><see langword="true"/> if VAC2 is inserted, otherwise <see langword="false"/>.</value>
        [Property]
        public bool IsVac2Present => _helpers.GetChargerStatus0().HasFlag(ChargerStatus0.Vac2Inserted);

        /// <summary>
        /// Gets state of VBUS inserted.
        /// </summary>
        /// <value><see langword="true"/> if VBUS is inserted, otherwise <see langword="false"/>.</value>
        [Property]
        public bool IsVbusPresent => _helpers.GetChargerStatus0().HasFlag(ChargerStatus0.VbusPresent);

        /// <summary>
        /// Gets state of VAC1 inserted.
        /// </summary>
        /// <value><see langword="true"/> if VAC1 is inserted, otherwise <see langword="false"/>.</value>
        [Property]
        public bool IsVac1Present => _helpers.GetChargerStatus0().HasFlag(ChargerStatus0.Vac1Inserted);

        /// <summary>
        /// Gets state IC thermal regulation.
        /// </summary>
        /// <value><see langword="true"/> if device is in thermal regulation, <see langword="false"/> for normal operation.</value>
        [Property]
        public bool IsInThermalRegulation => _helpers.GetChargerStatus2().HasFlag(ChargerStatus2.ThermalRegulation);

        /// <summary>
        /// Gets D+/D- detection status bits.
        /// </summary>
        /// <value><see langword="true"/> if D+/D- detection is ongoing, <see langword="false"/>  D+/D- detection is NOT started yet, or the detection is done.</value>
        [Property]
        public bool DpDmDetectionOngoing => _helpers.GetChargerStatus2().HasFlag(ChargerStatus2.DpDmStatus);

        /// <summary>
        /// Gets battery present status.
        /// </summary>
        /// <value><see langword="true"/> if VBAT present, otherwise <see langword="false"/>.</value>
        [Property]
        public bool IsVbatPresent => _helpers.GetChargerStatus2().HasFlag(ChargerStatus2.VbatPresent);

        /// <summary>
        /// Gets Input Current Optimizer (ICO) status.
        /// </summary>
        [Property]
        public IcoStatus IcoStatus => _helpers.GetIcoStatus();

        /// <summary>
        /// Gets or sets the top-off timer control.
        /// </summary>
        [Property]
        public TopOffTimerControl TopOffTimer { get => _helpers.GetTopOffTimerControl(); set => _helpers.SetTopOffTimerControl(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the trickle charge timer is set.
        /// Fixed at 1 hour.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="true"/>.
        /// </remarks>
        [Property]

        public bool EnableTrickleCharge { get => _helpers.GetEnableTrickleChargeTimer(); set => _helpers.SetEnableTrickleChargeTimer(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the precharge timer is set.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="true"/>.
        /// </remarks>
        [Property]
        public bool EnablePrechargeTimer { get => _helpers.GetEnablePrechargeTimer(); set => _helpers.SetEnablePrechargeTimer(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the fast charge timer is set.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="true"/>.
        /// </remarks>
        [Property]
        public bool EnableFastChargeTimer { get => _helpers.GetEnableFastChargeTimer(); set => _helpers.SetEnableFastChargeTimer(value); }

        /// <summary>
        /// Gets or sets the fast charge timer setting.
        /// </summary>
        /// <remarks>
        /// Default value is <see cref="FastChargeTimerSetting.Time_12H"/>.
        /// </remarks>
        [Property]
        public FastChargeTimerSetting FastChargeTimer { get => _helpers.GetFastChargeTimer(); set => _helpers.SetFastChargeTimer(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the trickle charge, pre-charge and fast charge timer are slowed by 2x during input DPM or thermal regulation.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="true"/>, the timer is slowed down.
        /// </remarks>
        [Property]
        public bool TimerControlSlowDown { get => _helpers.GetEnable2xTimerSlowdown(); set => _helpers.SetEnable2xTimerSlowdown(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the the auto battery discharging is enabled during the battery OVP fault.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="true"/>, The charger will apply a discharging current on BAT during battery OVP.
        /// </remarks>
        [Property]
        public bool AutoBatteryDischarge { get => _helpers.GetAutoBatteryDischarge(); set => _helpers.SetAutoBatteryDischarge(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the charger will apply a discharging current on BAT regardless the battery OVP status.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/> (idle), the charger won't apply a discharging current.
        /// </remarks>
        [Property]
        public bool ForceBatteryDischarge { get => _helpers.GetForceBatteryDischarge(); set => _helpers.SetForceBatteryDischarge(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the charge is enabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="true"/>, charging is enabled.
        /// </remarks>
        [Property]
        public bool EnableCharge { get => _helpers.GetEnableCharger(); set => _helpers.SetEnableCharger(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the input current optimizer is enabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, the input current optimizer is disabled.
        /// </remarks>
        [Property]
        public bool EnableInputCurrentOptimizer { get => _helpers.GetEnableInputCurrentOptimizer(); set => _helpers.SetEnableInputCurrentOptimizer(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the start input current optimizer (ICO) will be forced.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This bit can only be set and returns <see langword="false"/> after ICO starts.
        /// </para>
        /// <para>
        /// This bit only valid when <see cref="EnableInputCurrentOptimizer"/> is set to <see langword="true"/>.
        /// </para>
        /// <para>
        /// Default value is <see langword="false"/>, the input current optimizer is not forced.
        /// </para>
        /// </remarks>
        [Property]
        public bool ForceStartInputCurrentOptimizer { get => _helpers.GetForceStartInputCurrentOptimizer(); set => _helpers.SetForceStartInputCurrentOptimizer(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the high impedance mode is enabled.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This bit will be also reset to 0, when the adapter is plugged in at VBUS.
        /// </para>
        /// <para>
        /// Default value is <see langword="false"/>, the high impedance mode is disabled.
        /// </para>
        /// </remarks>
        [Property]
        public bool EnableHighImpedance { get => _helpers.GetEnableHighImpedanceMode(); set => _helpers.SetEnableHighImpedanceMode(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the charge termination is enabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="true"/>, the charge termination is enabled.
        /// </remarks>
        [Property]
        public bool EnableTermination { get => _helpers.GetEnableTermination(); set => _helpers.SetEnableTermination(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the D+/D- detection is forced.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Set to <see langword="true"/> to force the D+/D- detection. When D+/D- detection is done, this bit will be reset to <see langword="false"/>
        /// </para>
        /// </remarks>
        [Property]
        public bool ForceDDDetection { get => _helpers.GetForceDDDetection(); set => _helpers.SetForceDDDetection(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the automatic D+/D- detection is enabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="true"/>, D+/D- detection when VBUS is plugged-in is enabled.
        /// </remarks>
        [Property]
        public bool AutomaticDDDetection { get => _helpers.GetAutoDDDetection(); set => _helpers.SetAutoDDDetection(value); }

        /// <summary>
        /// Gets or sets a value indicating whether 12V mode in HVDCP is enabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, 12V mode in HVDCP is disabled.
        /// </remarks>
        [Property]
        public bool Enable12V { get => _helpers.GetEnable12VInput(); set => _helpers.SetEnable12VInput(value); }

        /// <summary>
        /// Gets or sets a value indicating whether 9V mode in HVDCP is enabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, 9V mode in HVDCP is disabled.
        /// </remarks>
        [Property]
        public bool Enable9V { get => _helpers.GetEnable9VInput(); set => _helpers.SetEnable9VInput(value); }

        /// <summary>
        /// Gets or sets a value indicating whether high voltage DCP handshake is enabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, high voltage DCP handshake is disabled.
        /// </remarks>
        [Property]
        public bool HighVoltageDcp { get => _helpers.GetEnableHighVoltageDdcp(); set => _helpers.SetEnableHighVoltageDdcp(value); }

        /// <summary>
        /// Gets or sets the external ship FET control logic to force the device enter different modes.
        /// </summary>
        [Property]
        public SFETControl SFETControl { get => _helpers.GetSdrvControl(); set => _helpers.SetSdrvControl(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the delay time is added to the taking action in <see cref="SFETControl"/>.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, the 10s delay time is added.
        /// </remarks>
        [Property]
        public bool SFETDelayDisabled { get => _helpers.GetSdrvDelayEnable(); set => _helpers.SetSdrvDelayEnable(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the AC drive is disabled.
        /// </summary>
        /// <remarks>
        /// When this is set to <see langword="true"/>, both AC drives are disabled.
        /// </remarks>
        [Property]
        public bool DisableAcDrive { get => _helpers.GetDisableAcDrive(); set => _helpers.SetDisableAcDrive(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the OTG mode is enabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, the OTG mode is disabled.
        /// </remarks>
        [Property]
        public bool OTGModeControl { get => _helpers.GetOTGModeControl(); set => _helpers.SetOTGModeControl(value); }

        /// <summary>
        /// Gets or sets a value indicating whether PFM is disabled in OTG mode.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, PFM is enabled in OTG mode.
        /// </remarks>
        [Property]
        public bool DisablePFMInOTG { get => _helpers.GetDisablePFMInOTG(); set => _helpers.SetDisablePFMInOTG(value); }

        /// <summary>
        /// Gets or sets a value indicating whether there will be a 15ms delay When waking up the device from ship mode, to pull low the QON pin.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, so there is a 1ms delay.
        /// </remarks>
        [Property]
        public bool WakeupDelay { get => _helpers.GetWakeUpDelay(); set => _helpers.SetWakeUpDelay(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the BATFET LDO  is disabled in pre-charge stage.
        /// </summary>
        /// <remarks>
        /// Default is <see langword="false"/>, the BATFET LDO mode is enabled.
        /// </remarks>
        [Property]
        public bool DisableLDO { get => _helpers.GetDisableLDOInPreCharge(); set => _helpers.SetDisableLDOInPreCharge(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the OOA is disabled in OTG mode.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, the OOA is enabled in OTG mode.
        /// </remarks>
        [Property]
        public bool DisableOOAInOTG { get => _helpers.GetDisableOOAInOTGMode(); set => _helpers.SetDisableOOAInOTGMode(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the OOA is disabled in Forward mode.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, the OOA is enabled in Forward mode.
        /// </remarks>
        [Property]
        public bool DisableOOAInForward { get => _helpers.GetDisableOOAInForwardMode(); set => _helpers.SetDisableOOAInForwardMode(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the external ACFET2-RBFET2 gate driver control is enabled.
        /// </summary>
        /// <remarks>
        /// <para>
        /// At POR, if the charger detects that there is no ACFET2-RBFET2 populated, this bit will be locked at <see langword="false"/>
        /// </para>
        /// <para>
        /// Default value is <see langword="false"/>, the external ACFET2-RBFET2 gate driver control is disabled.
        /// </para>
        /// </remarks>
        [Property]
        public bool EnableACDrive2 { get => _helpers.GetEnableAcDrive1(); set => _helpers.SetEnableAcDrive1(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the external ACFET1-RBFET1 gate driver control is enabled.
        /// </summary>
        /// <remarks>
        /// <para>
        /// At POR, if the charger detects that there is no ACFET1-RBFET1 populated, this bit will be locked at <see langword="false"/>
        /// </para>
        /// <para>
        /// Default value is <see langword="false"/>, the external ACFET1-RBFET1 gate driver control is disabled.
        /// </para>
        /// </remarks>
        [Property]
        public bool EnableACDrive1 { get => _helpers.GetEnableAcDrive2(); set => _helpers.SetEnableAcDrive2(value); }

        /// <summary>
        /// Gets or sets the <see cref="SwitchingFrequency"/> selection for the charget.
        /// </summary>
        /// <remarks>
        /// This bit POR default value is based on the PROG pin strapping indicating whether the external BATFET gate driver control is enabled.
        /// </remarks>
        [Property]
        public SwitchingFrequency SwitchingFrequency { get => _helpers.GetPwmFrequency(); set => _helpers.SetPwmFrequency(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the STAT pin output will be disabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, the STAT pin output is enabled.
        /// </remarks>
        [Property]
        public bool DisableStatPin { get => _helpers.GetDisableStatPin(); set => _helpers.SetDisableStatPin(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the forward mode VSYS short hiccup protection will be disabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, the forward mode VSYS short hiccup protection is enabled.
        /// </remarks> 
        [Property]
        public bool DisableForwardModeVSys { get => _helpers.GetDisableForwardModeVsys(); set => _helpers.SetDisableForwardModeVsys(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the OTG mode VOTG UVP hiccup protection will be disabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, the OTG mode VOTG UVP hiccup protection is enabled.
        /// </remarks>
        [Property]
        public bool DisableOTGModeUVP { get => _helpers.GetDisableOtgModeUvp(); set => _helpers.SetDisableOtgModeUvp(value); }

        /// <summary>
        /// Gets or sets a value indicating whether VINDPM detection will be forced.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only when VBAT>VSYSMIN, this bit can be set to <see langword="true"/>. Once the VINDPM auto detection is done, this bit returns to <see langword="false"/>.
        /// </para>
        /// <para>
        /// When set to <see langword="true"/> this will force the converter stop switching, and ADC measures the VBUS voltage without input current, then the charger updates the VINDPM register accordingly.
        /// </para>
        /// <para>
        /// Default value is <see langword="false"/>, the VINDPM detection is not forced.
        /// </para>
        /// </remarks>
        [Property]
        public bool ForceVinDPMDetection { get => _helpers.GetForceVinDpmDetection(); set => _helpers.SetForceVinDpmDetection(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the IBUS OCP in OTG mode will be disabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="true"/>, the IBUS OCP in OTG mode is enabled.
        /// </remarks>
        [Property]
        public bool EnableIBusOcpInForwardMode { get => _helpers.GetEnableIbusOcp(); set => _helpers.SetEnableIbusOcp(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the ship FET is populated or not.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The user has to set this bit based on whether a ship FET is populated or not.
        /// </para>
        /// <para>
        /// The POR default value is <see langword="false"/>, which means the charger does not support all the features associated with the ship FET.
        /// </para>
        /// <para>
        /// Default value is <see langword="false"/>, the ship FET is not populated.
        /// </para>
        /// </remarks>
        [Property]
        public bool SFETIsPresent { get => _helpers.GetSFetPresent(); set => _helpers.SetSFetPresent(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the battery discharge current sensing is enabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, the battery discharge current sensing is disabled.
        /// </remarks>
        [Property]
        public bool BatteryDischargeCurrentSensing { get => _helpers.GetEnableBatteryDischargeSensing(); set => _helpers.SetEnableBatteryDischargeSensing(value); }

        /// <summary>
        /// Gets or sets the battery discharge current.
        /// </summary>
        /// <remarks>
        /// Default value is <see cref="BatteryDischargeCurrent.Current_5A"/>.
        /// </remarks>
        [Property]
        public BatteryDischargeCurrent BatteryDischargeCurrent { get => _helpers.GetBatteryDischargeCurrent(); set => _helpers.SetBatteryDischargeCurrent(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the the internal IINDPM register input current regulation is enabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="true"/>, the internal IINDPM is enabled.
        /// </remarks>
        [Property]
        public bool EnableInternalDPM { get => _helpers.GetEnableInputDpm(); set => _helpers.SetEnableInputDpm(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the the external ILIM_HIZ pin input current regulation is enabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="true"/>, the external ILIM_HIZ pin is enabled.
        /// </remarks>
        [Property]
        public bool EnableExternalInpuCurrent { get => _helpers.GetEnableExternalILIM(); set => _helpers.SetEnableExternalILIM(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the the battery discharging current OCP is enabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, the battery discharging current OCP is disabled.
        /// </remarks>
        [Property]
        public bool EnableBatteryDischargingCurrentOCP { get => _helpers.GetEnableBatteryOCP(); set => _helpers.SetEnableBatteryOCP(value); }

        /// <summary>
        /// Gets a value indicating whether the the ACFET2-RBFET2 is placed.
        /// </summary>
        [Property]
        public bool ACFET2IsPlaced => _helpers.GetAcrb2Status();

        /// <summary>
        /// Gets a value indicating whether the the ACFET1-RBFET1 is placed.
        /// </summary>
        [Property]
        public bool ACFET1IsPlaced => _helpers.GetAcrb1Status();

        /// <summary>
        /// Gets a value indicating whether the ADC converstion is complete. This is valid for only one-shot mode.
        /// </summary>
        [Property]
        public bool AdcConversionComplete => _helpers.GetAdcDoneStatus();

        /// <summary>
        /// Gets a value indicating whether the charger is in VSYS regulation. This is valid for forward mode.
        /// </summary>
        [Property]
        public bool InVsysRegulation => _helpers.GetVsysStatus();

        /// <summary>
        /// Gets a value indicating whether the fast charge safety timer has expired.
        /// </summary>
        /// <value><see langword="true"/> if the fast charge safety timer has expired, otherwise <see langword="false"/> meaning in normal operation.</value>
        [Property]
        public bool FastChargeSafetyTimerExpired => _helpers.GetFastChargeTimerStatus();

        /// <summary>
        /// Gets a value indicating whether the trickle charge safety timer has expired.
        /// </summary>
        /// <value><see langword="true"/> if the trickle charge safety timer has expired, otherwise <see langword="false"/> meaning in normal operation.</value>
        [Property]
        public bool TrickleChargeSafetyTimerExpired => _helpers.GetTrickleChargeTimerStatus();

        /// <summary>
        /// Gets a value indicating whether the pre-charge safety timer has expired.
        /// </summary>
        /// <value><see langword="true"/> if the pre-charge safety timer has expired, otherwise <see langword="false"/> meaning in normal operation.</value>
        [Property]
        public bool PreChargeSafetyTimerExpired => _helpers.GetPreChargeTimerStatus();

        /// <summary>
        /// Gets a value indicating whether the the battery voltage is too low to enable OTG mode.
        /// </summary>
        /// <value><see langword="true"/> if the battery voltage is too low to enable OTG mode, otherwise <see langword="false"/> meaning that the battery voltage is high enough to enable the OTG operation.</value>
        [Property]
        public bool VBatLowForOTG => _helpers.GetVBatOTGLowStatus();

        /// <summary>
        /// Gets a value indicating whether the TS temperature is in the cold range, lower than T1.
        /// </summary>
        /// <value><see langword="true"/> if the TS status is in cold range, otherwise <see langword="false"/>.</value>
        [Property]
        public bool TsTemperatureIsCold => _helpers.GetTsColdStatus();

        /// <summary>
        /// Gets a value indicating whether the TS temperature is in the cool range, between T1 and T2.
        /// </summary>
        /// <value><see langword="true"/> if the TS status is in cool range, otherwise <see langword="false"/>.</value>
        [Property]
        public bool TsTemperatureIsCool => _helpers.GetTsCoolStatus();

        /// <summary>
        /// Gets a value indicating whether the TS temperature is in the warm range, between T3 and T5.
        /// </summary>
        /// <value><see langword="true"/> if the TS status is in warm range, otherwise <see langword="false"/>.</value>
        [Property]
        public bool TsTemperatureIsWarm => _helpers.GetTsWarmStatus();

        /// <summary>
        /// Gets a value indicating whether the TS temperature is in the hot range, higher than T5.
        /// </summary>
        /// <value><see langword="true"/> if the TS status is in hot range, otherwise <see langword="false"/>.</value>
        [Property]
        public bool TsTemperatureIsHot => _helpers.GetTsHotStatus();

        /// <summary>
        /// Gets a value indicating whether the device is in battery discharging current regulation.
        /// </summary>
        /// <value><see langword="true"/> if the device is in battery discharging current regulation, otherwise <see langword="false"/> meaning in normal operation.</value>
        [Property]
        public bool DeviceInBatteryDischargingRegulation => _helpers.GetIBatRegulationStatus();

        /// <summary>
        /// Gets a value indicating whether the VBUS is in over voltage protection.
        /// </summary>
        /// <value><see langword="true"/> if the VBUS is in over voltage protection, otherwise <see langword="false"/> meaning in normal operation.</value>
        [Property]
        public bool VbusInOverVoltageProtection => _helpers.GetVbusOverVoltageStatus();

        /// <summary>
        /// Gets a value indicating whether the VBAT is in over voltage protection.
        /// </summary>
        /// <value><see langword="true"/> if the VBAT is in over voltage protection, otherwise <see langword="false"/> meaning in normal operation.</value>
        [Property]
        public bool VbatInOverVoltageProtection => _helpers.GetVBatOverVoltageStatus();

        /// <summary>
        /// Gets a value indicating whether the IBUS is in over current protection.
        /// </summary>
        /// <value><see langword="true"/> if the IBUS is in over current protection, otherwise <see langword="false"/> meaning in normal operation.</value>
        [Property]
        public bool IbusInOverCurrentProtection => _helpers.GetIBusOverCurrentStatus();

        /// <summary>
        /// Gets a value indicating whether the IBAT is in over current protection.
        /// </summary>
        /// <value><see langword="true"/> if the IBAT is in over current protection, otherwise <see langword="false"/> meaning in normal operation.</value>
        [Property]
        public bool IbatInOverCurrentProtection => _helpers.GetIBatOverCurrentStatus();

        /// <summary>
        /// Gets a value indicating whether the converter is in over current protection.
        /// </summary>
        /// <value><see langword="true"/> if the converter is in over current protection, otherwise <see langword="false"/> meaning in normal operation.</value>
        [Property]
        public bool ConverterInOverCurrentProtection => _helpers.GetConverterOverCurrentStatus();

        /// <summary>
        /// Gets a value indicating whether the VAC2 is in over voltage protection.
        /// </summary>
        /// <value><see langword="true"/> if the VAC2 is in over voltage protection, otherwise <see langword="false"/> meaning in normal operation.</value>
        [Property]
        public bool Vac2InOverVoltageProtection => _helpers.GetVac2OverVoltageStatus();

        /// <summary>
        /// Gets a value indicating whether the VAC1 is in over voltage protection.
        /// </summary>
        /// <value><see langword="true"/> if the VAC1 is in over voltage protection, otherwise <see langword="false"/> meaning in normal operation.</value>
        [Property]
        public bool Vac1InOverVoltageProtection => _helpers.GetVac1OverVoltageStatus();

        /// <summary>
        /// Gets a value indicating whether the device is in SYS short circuit protection.
        /// </summary>
        /// <value><see langword="true"/> if the device is in SYS short circuit protection, otherwise <see langword="false"/> meaning in normal operation.</value>
        [Property]
        public bool DeviceInSysShortCircuitProtection => _helpers.GetVsysShortCircuitStatus();

        /// <summary>
        /// Gets a value indicating whether a IINDPM/IOTG signal rising edge was detected.
        /// </summary>
        /// <value><see langword="true"/> if a IINDPM/IOTG signal rising edge was detected, otherwise <see langword="false"/>.</value>
        [Property]
        public bool IINDPMSignalDetected => _helpers.GetIIndpmFlag();

        /// <summary>
        /// Gets a value indicating whether a VINDPM signal rising edge was detected.
        /// </summary>
        /// <value><see langword="true"/> if a VINDPM signal rising edge was detected, otherwise <see langword="false"/>.</value>
        [Property]
        public bool VINDPMSignalDetected => _helpers.GetVIndpmFlag();

        /// <summary>
        /// Gets a value indicating whether a I2C watchdog timer event occurred.
        /// </summary>
        /// <value><see langword="true"/> if a I2C watchdog timer event occurred, otherwise <see langword="false"/>.</value>
        [Property]
        public bool I2cWatchdogSignaled => _helpers.GetI2cWatchdogFlag();

        /// <summary>
        /// Gets a value indicating whether a poor source was detected.
        /// </summary>
        /// <value><see langword="true"/> if a poor source was detected, otherwise <see langword="false"/>.</value>
        [Property]
        public bool PoorSourceDetected => _helpers.GetPoorSourceFlag();

        /// <summary>
        /// Gets a value indicating whether a power good flag change has been set.
        /// </summary>
        /// <value><see langword="true"/> on any change in PG_STAT even (adapter good qualification or adapter good going away), otherwise <see langword="false"/>.</value>
        [Property]
        public bool PowerGoodHasChanged => _helpers.GetPowerGoodFlag();

        /// <summary>
        /// Gets a value indicating whether a AC2 present flag change has been set.
        /// </summary>
        /// <value><see langword="true"/> on any change in VAC2 present status, otherwise <see langword="false"/>.</value>
        [Property]
        public bool Ac1PresentHasChanged => _helpers.GetAc1PresentFlag();

        /// <summary>
        /// Gets a value indicating whether a AC1 present flag change has been set.
        /// </summary>
        /// <value><see langword="true"/> on any change in VAC1 present status, otherwise <see langword="false"/>.</value>
        [Property]
        public bool Ac2PresentHasChanged => _helpers.GetAc2PresentFlag();

        /// <summary>
        /// Gets a value indicating whether a VBUS present flag change has been set.
        /// </summary>
        /// <value><see langword="true"/> on any change in VBUS present status, otherwise <see langword="false"/>.</value>
        [Property]
        public bool VbusPresentHasChanged => _helpers.GetVbusPresentFlag();

        /// <summary>
        /// Gets a value indicating whether a charge status flag has changed.
        /// </summary>
        /// <value><see langword="true"/> on any change in charge status, otherwise <see langword="false"/>.</value>
        [Property]
        public bool ChargeStatusChanged => _helpers.GetChargeStatusFlag();

        /// <summary>
        /// Gets a value indicating whether a ICO status flag has changed.
        /// </summary>
        /// <value><see langword="true"/> on any change in ICO status, otherwise <see langword="false"/>.</value>
        [Property]
        public bool IcoStatusChanged => _helpers.GetIcoStatusFlag();

        /// <summary>
        /// Gets a value indicating whether a Vbus status flag has changed.
        /// </summary>
        /// <value><see langword="true"/> on any change in Vbus status, otherwise <see langword="false"/>.</value>
        [Property]
        public bool VbusStatusChanged => _helpers.GetVbusStatusFlag();

        /// <summary>
        /// Gets a value indicating whether a TREG signal rising threshold was detected.
        /// </summary>
        /// <value><see langword="true"/> if a TREG signal rising threshold was detected, otherwise <see langword="false"/>.</value>
        [Property]
        public bool ThermalRegulationThresholdReached => _helpers.GetThermalRegulationFlag();

        /// <summary>
        /// Gets a value indicating whether a VBAT present flag change has been set.
        /// </summary>
        /// <value><see langword="true"/> on any change in VBAT present status, otherwise <see langword="false"/>.</value>
        [Property]
        public bool VbatPresentChanged => _helpers.GetVbatPresentFlag();

        /// <summary>
        /// Gets a value indicating whether a change in BC1.2 detection has occurred.
        /// </summary>
        [Property]
        public bool BC12DetectionChanged => _helpers.GetBc12DetectionFlag();

        /// <summary>
        /// Gets a value indicating whether a D+/D- detection is completed.
        /// </summary>
        /// <value><see langword="true"/> if the D+/D- detection is completed, otherwise <see langword="false"/> if it hasn't started or is ongoing.</value>
        [Property]
        public bool DpDmDetectionCompleted => _helpers.GetDpdmDoneFlag();

        /// <summary>
        /// Gets a value indicating whether an ADC conversion completed.
        /// </summary>
        /// <value><see langword="true"/> if the ADC conversion is completed, otherwise <see langword="false"/> if it hasn't started or is ongoing.</value>
        [Property]
        public bool AdcConversionDone => _helpers.GetAdcDoneFlag();

        /// <summary>
        /// Gets a value indicating whether the charger has entered or existed VSYSMIN regulation.
        /// </summary>
        /// <value><see langword="true"/> if the charger has entered or existed VSYSMIN regulation, otherwise <see langword="false"/>.</value>
        [Property]
        public bool VsysMinChanged => _helpers.GetVsysFlag();

        /// <summary>
        /// Gets a value indicating whether a fast charge timer expired rising edge detected.
        /// </summary>
        /// <value><see langword="true"/> if a fast charge timer expired rising edge detected, otherwise <see langword="false"/>.</value>
        [Property]
        public bool FastChargeTimerExpired => _helpers.GetFastChargeTimerFlag();

        /// <summary>
        /// Gets a value indicating whether a trickle charge timer expired rising edge detected.
        /// </summary>
        /// <value><see langword="true"/> if a trickle charge timer expired rising edge detected, otherwise <see langword="false"/>.</value>
        [Property]
        public bool TrickleChargeTimerExpired => _helpers.GetTrickleChargeTimerFlag();

        /// <summary>
        /// Gets a value indicating whether a pre-charge timer expired rising edge detected.
        /// </summary>
        /// <value><see langword="true"/> if a pre-charge timer expired rising edge detected, otherwise <see langword="false"/>.</value>
        [Property]
        public bool PreChargeTimerExpired => _helpers.GetPreChargeTimerFlag();

        /// <summary>
        /// Gets a value indicating whether the top off timer expired rising edge detected.
        /// </summary>
        /// <value><see langword="true"/> if the top off timer expired rising edge detected, otherwise <see langword="false"/>.</value>
        [Property]
        public bool TopOffTimerExpired => _helpers.GetTopOffTimerFlag();

        /// <summary>
        /// Gets a value indicating whether the VBAT falls below the threshold to enable the OTG mode.
        /// </summary>
        /// <value><see langword="true"/> if the VBAT falls below the threshold to enable the OTG mode, otherwise <see langword="false"/>.</value>
        [Property]
        public bool VBatTooLowForOTG => _helpers.GetVBatOTGLowFlag();

        /// <summary>
        /// Gets a value indicating whether the TS across cold temperature (T1) is detected.
        /// </summary>
        /// <value><see langword="true"/> if the TS cold temperature (T1) is detected, otherwise <see langword="false"/>.</value>
        [Property]
        public bool TsColdDetected => _helpers.GetTsColdFlag();

        /// <summary>
        /// Gets a value indicating whether the TS across cool temperature (T2) is detected.
        /// </summary>
        /// <value><see langword="true"/> if the TS cool temperature (T2) is detected, otherwise <see langword="false"/>.</value>
        [Property]
        public bool TsCoolDetected => _helpers.GetTsCoolFlag();

        /// <summary>
        /// Gets a value indicating whether the TS across warm temperature (T3) is detected.
        /// </summary>
        /// <value><see langword="true"/> if the TS warm temperature (T3) is detected, otherwise <see langword="false"/>.</value>
        [Property]
        public bool TsWarmDetected => _helpers.GetTsWarmFlag();

        /// <summary>
        /// Gets a value indicating whether the TS across hot temperature (T5) is detected.
        /// </summary>
        /// <value><see langword="true"/> if the TS hot temperature (T5) is detected, otherwise <see langword="false"/>.</value>
        [Property]
        public bool TsHotDetected => _helpers.GetTsHotFlag();

        /// <summary>
        /// Gets a value of the TS ADC reading (in percentage).
        /// </summary>
        /// <remarks>
        /// Range is from 0% to 99.9023%.
        /// </remarks>
        [Property]
        public float TcAdcReading => _helpers.GetTCAdcReading();

        /// <summary>
        /// Initializes a new instance of the <see cref="Bq2579x" /> class.
        /// </summary>
        /// <param name="i2cDevice"><see cref="I2cDevice"/> to communicate with Si7021 device.</param>
        /// <exception cref="InvalidOperationException">When failing to read part information.</exception>
        /// <exception cref="NotSupportedException">If the part information returned is invalid, thus the connected part is not one of the supported BQ2579x devices.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="i2cDevice"/> is null.</exception>
        public Bq2579x(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException();

            _helpers = new Bq2579xHelpers(_i2cDevice);

            // read part information
            ReadPartInformation();
        }

        /// <summary>
        /// Read part information to perform a sanity check for the correct part.
        /// </summary>
        private void ReadPartInformation()
        {
            byte[] buffer = _helpers.ReadFromRegister(Register.REG48_Part_Information, 1);

            _deviceModel = (Model)(buffer[0] & DevicePartNumberMask);

            // sanity check for valid part
            if (_deviceModel != Model.Bq25792
                && _deviceModel != Model.Bq25798)
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Reset I2C device watchdog.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the I2C operation to write to the device to reset the watchdog fails.</exception>
        public void ResetWatchdog()
        {
            _helpers.WriteToRegister(Register.REG10_Charger_Control_1, 0b0000_1000);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
            _helpers = null;
        }
    }
}
