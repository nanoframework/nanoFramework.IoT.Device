////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

using System;
using System.Device.I2c;
using System.Device.Model;
using System.Runtime.InteropServices;
using UnitsNet;

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// Buck-Boost Battery Charger Bq2579x.
    /// </summary>
    [Interface("Buck-Boost Battery Charger Bq2579x")]
    public class Bq2579x : IDisposable
    {
        /// <summary>
        /// Bq25792/8 Default I2C Address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x6B;

        private const byte DevicePartNumberMask = 0b00111000;
        private const byte PrechargeCurrentLimitMask = 0b00111111;
        private const byte TerminationCurrentMask = 0b0001_1111;
        private const byte ChargerStatus0PowerGoodStatusMask = 0b0000_1000;
        private const byte ChargerStatus1ChargeStatusMask = 0b1110_0000;
        private const byte ChargerStatus1VbusStatusMask = 0b0001_1110;
        private const byte ChargerStatus1Bc12DetectionMask = 0b0000_0001;
        private const byte ChargerControl1WatchdogMask = 0b0000_0111;
        private const byte AdcControlEnableMask = 0b1000_0000;
        private const byte AdcControlConversionRateMask = 0b0100_0000;
        private const byte AdcControlResolutionMask = 0b0011_0000;
        private const byte AdcControlAverageControlMask = 0b0000_1000;
        private const byte AdcControlInitialAverageMask = 0b0000_0100;
        private const byte ThermalRegulationThresholdMask = 0b0000_0011;
        private const byte ThermalShutdownThresholdMask = 0b0000_1100;
        private const byte ChargerStatus3IcoStatusMask = 0b1100_0000;
        private const byte NtcControl0ChargerVoltageMask = 0b1110_0000;
        private const byte NtcControl0ChargeCurrentMask = 0b0001_1000;
        private const byte NtcControl1Vt2ComparatorVoltageMask = 0b1100_0000;
        private const byte NtcControl1Vt3ComparatorVoltageMask = 0b0011_0000;
        private const byte NtcControl1OtgHotTemperatureMask = 0b0000_1100;
        private const byte NtcControl1OtgColdTemperatureMask = 0b0000_0010;
        private const byte NtcControl1IgnoreTSMask = 0b0000_0001;
        private const byte TimerControlTopOffMask = 0b1100_0000;
        private const byte TimerControlEnableTrickleChargeMask = 0b0010_0000;
        private const byte TimerControlEnablePrechargeTimerMask = 0b0001_0000;
        private const byte TimerControlEnableFastChargeMask = 0b0000_1000;
        private const byte TimerControlFastChargeTimerMask = 0b0000_0110;
        private const byte TimerControl2xTimerMask = 0b0000_0001;
        private const byte ChargerControl0AutoBatteryDischargeMask = 0b1000_0000;
        private const byte ChargerControl0ForceBatteryDischargeMask = 0b0100_0000;
        private const byte ChargerControl0ChargerEnableMask = 0b0010_0000;
        private const byte ChargerControl0InputCurrentOptimizerMask = 0b0001_0000;
        private const byte ChargerControl0ForceStartInputMask = 0b0000_1000;
        private const byte ChargerControl0EnableHizMask = 0b0000_0100;
        private const byte ChargerControl0EnableTerminationMask = 0b0000_0010;
        private const byte ChargerControl2ForceDDDetectionMask = 0b1000_0000;
        private const byte ChargerControl2AutomaticDDDetectionMask = 0b0100_0000;
        private const byte ChargerControl2Enable12VMask = 0b0010_0000;
        private const byte ChargerControl2Enable9VMask = 0b0001_0000;
        private const byte ChargerControl2HighVoltageDcpMask = 0b0000_1000;
        private const byte ChargerControl2SFETControlMask = 0b0000_0110;
        private const byte ChargerControl2SdrvDelayMask = 0b0000_0001;
        private const byte ChargerControl3DisableAcDriveMask = 0b1000_0000;
        private const byte ChargerControl3OTGModeContolMask = 0b0100_0000;
        private const byte ChargerControl3DisablePFMInOTGMask = 0b0010_0000;
        private const byte ChargerControl3DisablePFMInForwardMask = 0b0001_0000;
        private const byte ChargerControl3WakeupDelayMask = 0b0000_1000;
        private const byte ChargerControl3DisableLdoMask = 0b0000_0100;
        private const byte ChargerControl3DisableOOAInOTGMask = 0b0000_0010;
        private const byte ChargerControl3DisableOOAInForwardMask = 0b0000_0001;
        private const byte ChargerControl4EnableACDrive2Mask = 0b1000_0000;
        private const byte ChargerControl4EnableACDrive1Mask = 0b0100_0000;
        private const byte ChargerControl4PwmFrequencyMask = 0b0010_0000;
        private const byte ChargerControl4DisableStatPinMask = 0b0001_0000;
        private const byte ChargerControl4DisableVSysForwardMask = 0b0000_1000;
        private const byte ChargerControl4DisableVOTGUvpMask = 0b0000_0100;
        private const byte ChargerControl4ForceVinDMPMask = 0b0000_0010;
        private const byte ChargerControl4EnableIbusOcpMask = 0b0000_0001;
        private const byte ChargerControl5SFETIsPresentMask = 0b1000_0000;
        private const byte ChargerControl5IBatCurrentSensingMask = 0b0010_0000;
        private const byte ChargerControl5BatteryDischargeCurrentMask = 0b0001_1000;
        private const byte ChargerControl5EnableIINDPMMask = 0b0000_0100;
        private const byte ChargerControl5EnableExtILIMMask = 0b0000_0010;
        private const byte ChargerControl5EnableDischargeOCPMask = 0b0000_0001;
        private const byte ChargerStatus3ACRB2StatusMask = 0b1000_0000;
        private const byte ChargerStatus3ACRB1StatusMask = 0b0100_0000;
        private const byte ChargerStatus3ADCConversionStatusMask = 0b0010_0000;
        private const byte ChargerStatus3VSysSatusMask = 0b0001_0000;
        private const byte ChargerStatus3FastChargerTimerStatusMask = 0b0000_1000;
        private const byte ChargerStatus3TrickleChargeStatusMask = 0b0000_0100;
        private const byte ChargerStatus3PrechargeStatusMask = 0b0000_0010;
        private const byte ChargerStatus4VbatOTGLowStatusMask = 0b0001_0000;
        private const byte ChargerStatus4TsColdStatusMask = 0b0000_1000;
        private const byte ChargerStatus4TsCoolStatusMask = 0b0000_0100;
        private const byte ChargerStatus4TsWarmStatusMask = 0b0000_0010;
        private const byte ChargerStatus4TsHotStatusMask = 0b0000_0001;
        private const byte FaultStatus0IBatRegulationStatusMask = 0b1000_0000;
        private const byte FaultStatus0VBusOverVoltageStatusMask = 0b0100_0000;
        private const byte FaultStatus0VBatOvervoltageStatusMask = 0b0010_0000;
        private const byte FaultStatus0IBusOverCurrentStatusMask = 0b0001_0000;
        private const byte FaultStatus0IBatOverCurrentStatusMask = 0b0000_1000;
        private const byte FaultStatus0ConverterOverCurrentStatusMask = 0b0000_0100;
        private const byte FaultStatus0VAc2OverVoltageStatusMask = 0b0000_0010;
        private const byte FaultStatus0VAc1OverVoltageStatusMask = 0b0000_0001;
        private const byte FaultStatus1VsysShortCircuitStatusMask = 0b1000_0000;
        private const byte FaultStatus1VsysOverVoltageStatusMask = 0b0100_0000;
        private const byte FaultStatus1OTGOverVoltageStatusMask = 0b0010_0000;
        private const byte FaultStatus1OTGUnderVoltageStatusMask = 0b0001_0000;
        private const byte FaultStatus1TemperatureShutdowntatusMask = 0b0000_0100;
        private const byte ChargerFlag0IINDPMFlagMask = 0b1000_0000;
        private const byte ChargerFlag0VINDPMFlagMask = 0b0100_0000;
        private const byte ChargerFlag0I2cWatchdogFlagMask = 0b0010_0000;
        private const byte ChargerFlag0PoorSourceFlagMask = 0b0001_0000;
        private const byte ChargerFlag0PowerGoodFlagMask = 0b0000_1000;
        private const byte ChargerFlag0Vac2PresentFlagMask = 0b0000_0100;
        private const byte ChargerFlag0Vac1PresentFlagMask = 0b0000_0010;
        private const byte ChargerFlag0VbusPresentFlagMask = 0b0000_0001;
        private const byte ChargerFlag1ChargeFlagMask = 0b1000_0000;
        private const byte ChargerFlag1IcoFlagMask = 0b0100_0000;
        private const byte ChargerFlag1VbusFlagMask = 0b0001_0000;
        private const byte ChargerFlag1ThermallagMask = 0b0000_0100;
        private const byte ChargerFlag1VbatPresentFlagMask = 0b0000_0010;
        private const byte ChargerFlag1Bc12FlagMask = 0b0000_0001;
        private const byte ChargerFlag2DPDMDoneFlagMask = 0b0100_0000;
        private const byte ChargerFlag2AdcConversionFlagMask = 0b0010_0000;
        private const byte ChargerFlag2VsysFlagMask = 0b0001_0000;
        private const byte ChargerFlag2FastChargeTimerFlagMask = 0b0000_1000;
        private const byte ChargerFlag2TrickleChargeTimerFlagMask = 0b0000_0100;
        private const byte ChargerFlag2PrechargeTimerFlagMask = 0b0000_0010;
        private const byte ChargerFlag2TopOffTimerFlagMask = 0b0000_0001;
        private const byte ChargerFlag3VbatOTGLowFlagMask = 0b0001_0000;
        private const byte ChargerFlag3TsColdFlagMask = 0b0000_1000;
        private const byte ChargerFlag3TsCoolFlagMask = 0b0000_0100;
        private const byte ChargerFlag3TsWarmFlagMask = 0b0000_0010;
        private const byte ChargerFlag3TsHotFlagMask = 0b0000_0001;

        private const int FixedOffsetMinimalSystemVoltage = 2500;
        private const int MaxValueMinimalSystemVoltage = 16000;
        private const int StepMinimalSystemVoltage = 250;
        private const int PrechargeCurrentLimitMinValue = 40;
        private const int StepPrechargeCurrentLimit = 40;
        private const int TerminationCurrentMinValue = 40;
        private const int TerminationCurrentMaxValue = 1000;
        private const int PrechargeCurrentLimitMaxValue = 2000;
        private const int FixedOffsetMinimalChargeVoltageLimit = 3000;
        private const int MaxValueChargeVoltageLimit = 18800;
        private const int FixedOffsetMinimalChargeCurrentLimit = 50;
        private const int MaxValueChargeCurrentLimit = 5000;
        private const int MaxValueInputVoltageLimit = 22000;
        private const int MinValueInputVoltageLimit = 3600;
        private const int FixedOffsetMinimalInputCurrentLimit = 100;
        private const int MaxValueInputCurrentLimit = 3300;

        private const int VbusAdcStep = 1;
        private const int ChargeVoltageStep = 10;
        private const int ChargeInputCurrentStep = 10;
        private const int InputVoltageStep = 100;
        private const float TdieStemp = 0.5f;

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
        public ChargeStatus ChargeStatus => GetChargeStatus();

        /// <summary>
        /// Gets the VBUS status.
        /// </summary>
        [Property]
        public VbusStatus VbusStatus => GetVbusStatus();

        /// <summary>
        /// Gets BC1.2 or non-standard detection.
        /// </summary>
        /// <value><see langword="true"/> if BC1.2 or non-standard detection is complete, otherwise <see langword="false"/>.</value>
        [Property]
        public bool Bc12Detection => GetBc12Detection();

        /// <summary>
        /// Gets or sets a value indicating whether the ADC is enable.
        /// </summary>
        /// <value><see langword="true"/> to enable ADC, otherwise <see langword="false"/>.</value>
        [Property]
        public bool AdcEnable { get => GetAdcEnable(); set => SetAdcEnable(value); }

        /// <summary>
        /// Gets or sets ADC conversion rate.
        /// </summary>
        [Property]
        public AdcConversioRate AdcConversionRate { get => GetAdcConversionRate(); set => SetAdcConversionRate(value); }

        /// <summary>
        /// Gets or sets ADC resolution.
        /// </summary>
        [Property]
        public AdcResolution AdcResolution { get => GetAdcResolution(); set => SetAdcResolution(value); }

        /// <summary>
        /// Gets or sets ADC resolution.
        /// </summary>
        [Property]
        public AdcAveraging AdcAveraging { get => GetAdcAveraging(); set => SetAdcAveraging(value); }

        /// <summary>
        /// Gets or sets ADC resolution.
        /// </summary>
        [Property]
        public AdcInitialAverage AdcInitialAverage { get => GetAdcInitialAverage(); set => SetAdcInitialAverage(value); }

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
        public ElectricPotentialDc MinimalSystemVoltage { get => GetMinimalSystemVoltage(); set => SetMinimalSystemVoltage(value); }

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
        public ElectricPotentialDc ChargeVoltageLimit { get => GetChargeVoltageLimit(); set => SetChargeVoltageLimit(value); }

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
        public ElectricCurrent ChargeCurrentLimit { get => GetChargeCurrentLimit(); set => SetChargeCurrentLimit(value); }

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
        public ElectricPotentialDc InputVoltageLimit { get => GetInputVoltageLimit(); set => SetInputVoltageLimit(value); }

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
        public ElectricCurrent InputCurrentLimit { get => GetInputCurrentLimit(); set => SetInputCurrentLimit(value); }

        /// <summary>
        /// Gets or sets battery voltage thresholds for the transition from precharge to fast charge.
        /// </summary>
        /// <remarks>Defined as a ratio of battery regulation limit(VREG).</remarks>
        [Property]
        public ThresholdFastCharge FastChargeTransitionVoltage { get => GetThresholdFastCharge(); set => SetThresholdFastCharge(value); }

        /// <summary>
        /// Gets or sets precharge current limit (in steps of 40mA).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If value is out of range (40mA-2000mA).</exception>
        /// <remarks>Equivalent range: 40mA-2000mA.</remarks>
        [Property]
        public ElectricCurrent PrechargeCurrentLimit { get => GetPrechargeCurrentLimit(); set => SetPrechargeCurrentLimit(value); }

        /// <summary>
        /// Gets or sets termination current (in steps of 40mA).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If value is out of range (40mA-1000mA).</exception>"
        /// <remarks>Equivalent range: 40mA-1000mA.</remarks>
        public ElectricCurrent TerminationCurrent { get => GetTerminationCurrent(); set => SetTerminationCurrent(value); }

        /// <summary>
        /// Gets VBUS.
        /// </summary>
        /// <remarks>Range: 0mV-30000mV.</remarks>
        public ElectricPotentialDc Vbus => GetAdcVoltage(Register.REG35_VBUS_ADC);

        /// <summary>
        /// Gets VAC1.
        /// </summary>
        /// <remarks>Range: 0mV-30000mV.</remarks>
        public ElectricPotentialDc Vac1 => GetAdcVoltage(Register.REG37_VAC1_ADC);

        /// <summary>
        /// Gets VAC2.
        /// </summary>
        /// <remarks>Range: 0mV-30000mV.</remarks>
        public ElectricPotentialDc Vac2 => GetAdcVoltage(Register.REG39_VAC2_ADC);

        /// <summary>
        /// Gets VBAT.
        /// </summary>
        /// <remarks>Range: 0mV-20000mV.</remarks>
        public ElectricPotentialDc Vbat => GetAdcVoltage(Register.REG3B_VBAT_ADC);

        /// <summary>
        /// Gets VSYS.
        /// </summary>
        /// <remarks>Range: 0mV-24000mV.</remarks>
        public ElectricPotentialDc Vsys => GetAdcVoltage(Register.REG3D_VSYS_ADC);

        /// <summary>
        /// Gets die temperature.
        /// </summary>
        /// <remarks>Range: -40°C -150°C.</remarks>
        public Temperature DieTemperature => GetDieTemperature();

        /// <summary>
        /// Gets or sets the watchdog timer setting (in milliseconds).
        /// </summary>
        public WatchdogSetting WatchdogTimerSetting { get => GetWatchdogTimerSetting(); set => SetWatchdogTimerSetting(value); }

        /// <summary>
        /// Gets or sets Thermal regulation threshold.
        /// </summary>
        [Property]
        public ThermalRegulationThreshold ThermalRegulationThreshold { get => GetThermalRegulationThreshold(); set => SetThermalRegulationThreshold(value); }

        /// <summary>
        /// Gets or sets Thermal shutdown threshold.
        /// </summary>
        [Property]
        public ThermalShutdownThreshold ThermalShutdownThreshold { get => GetThermalShutdownThreshold(); set => SetThermalShutdownThreshold(value); }

        /// <summary>
        /// Gets or sets charge voltage for JEITA high temperature range (TWARN - THOT). 
        /// </summary>
        [Property]
        public ChargeVoltage ChargeVoltageHighTempRange { get => GetChargeVoltage(); set => SetChargeVoltage(value); }

        /// <summary>
        /// Gets or sets charge current for JEITA high temperature range (TWARN - THOT). 
        /// </summary>
        [Property]
        public ChargeCurrent ChargeCurrentHighTempRange { get => GetChargeCurrentHighTempRange(); set => SetChargeCurrentHighTempRange(value); }

        /// <summary>
        /// Gets or sets charge current for JEITA low temperature range (TCOLD - TCOOL). 
        /// </summary>
        [Property]
        public ChargeCurrent ChargeCurrentLowTempRange { get => GetChargeCurrentLowTempRange(); set => SetChargeCurrentLowTempRange(value); }

        /// <summary>
        /// Gets or sets VT2 comparator voltage rising thresholds as a percentage of REGN.
        /// </summary>
        [Property]
        public CompVoltageRisingThreshold Vt2ComparatorRisingThreshold { get => GetVt2ComparatorVoltage(); set => SetVt2ComparatorVoltage(value); }

        /// <summary>
        /// Gets or sets VT3 comparator voltage falling thresholds as a percentage of REGN.
        /// </summary>
        [Property]
        public CompVoltageFallingThreshold Vt3ComparatorFallingThreshold { get => GetVt3ComparatorVoltage(); set => SetVt3ComparatorVoltage(value); }

        /// <summary>
        /// Gets or sets OTG mode TS HOT temperature threshold.
        /// </summary>
        [Property]
        public OtgHotTempThreshold OtgHotTempThreshold { get => GetOtgHotTempThreshold(); set => SetOtgHotTempThreshold(value); }

        /// <summary>
        /// Gets or sets OTG mode TS COLD temperature threshold.
        /// </summary>
        [Property]
        public OtgColdTempThreshold OtgColdTempThreshold { get => GetOtgColdTempThreshold(); set => SetOtgColdTempThreshold(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the temperature sensor feedback should be ignored.
        /// </summary>
        /// <value><see langword="true"/> if ignoring temperature sensor feedback, otherwise <see langword="false"/>.</value>
        /// <remarks>
        /// Ignore the temperature sensor (TS) feedback, the charger considers the TS is always good to allow the charging and OTG modes. <see cref="Vt2ComparatorRisingThreshold"/> and <see cref="Vt3ComparatorFallingThreshold"/> always read 0 to report the normal condition.
        /// </remarks>
        [Property]
        public bool IgnoreTempSensor { get => GetIgnoreTempSensor(); set => SetIgnoreTempSensor(value); }

        /// <summary>
        /// Gets Power Good Status.
        /// </summary>
        /// <value><see langword="true"/> if power is good, otherwise <see langword="false"/>.</value>
        [Property]
        public bool IsPowerGood => GetChargerStatus0().HasFlag(ChargerStatus0.PowerGood);

        /// <summary>
        /// Gets state of VAC2 inserted.
        /// </summary>
        /// <value><see langword="true"/> if VAC2 is inserted, otherwise <see langword="false"/>.</value>
        [Property]
        public bool IsVac2Present => GetChargerStatus0().HasFlag(ChargerStatus0.Vac2Inserted);

        /// <summary>
        /// Gets state of VBUS inserted.
        /// </summary>
        /// <value><see langword="true"/> if VBUS is inserted, otherwise <see langword="false"/>.</value>
        [Property]
        public bool IsVbusPresent => GetChargerStatus0().HasFlag(ChargerStatus0.VbusPresent);

        /// <summary>
        /// Gets state of VAC1 inserted.
        /// </summary>
        /// <value><see langword="true"/> if VAC1 is inserted, otherwise <see langword="false"/>.</value>
        [Property]
        public bool IsVac1Present => GetChargerStatus0().HasFlag(ChargerStatus0.Vac1Inserted);

        /// <summary>
        /// Gets state IC thermal regulation.
        /// </summary>
        /// <value><see langword="true"/> if device is in thermal regulation, <see langword="false"/> for normal operation.</value>
        [Property]
        public bool IsInThermalRegulation => GetChargerStatus2().HasFlag(ChargerStatus2.ThermalRegulation);

        /// <summary>
        /// Gets D+/D- detection status bits.
        /// </summary>
        /// <value><see langword="true"/> if D+/D- detection is ongoing, <see langword="false"/>  D+/D- detection is NOT started yet, or the detection is done.</value>
        [Property]
        public bool DpDmDetectionOngoing => GetChargerStatus2().HasFlag(ChargerStatus2.DpDmStatus);

        /// <summary>
        /// Gets battery present status.
        /// </summary>
        /// <value><see langword="true"/> if VBAT present, otherwise <see langword="false"/>.</value>
        [Property]
        public bool IsVbatPresent => GetChargerStatus2().HasFlag(ChargerStatus2.VbatPresent);

        /// <summary>
        /// Gets Input Current Optimizer (ICO) status.
        /// </summary>
        [Property]
        public IcoStatus IcoStatus => GetIcoStatus();

        /// <summary>
        /// Gets or sets the top-off timer control.
        /// </summary>
        [Property]
        public TopOffTimerControl TopOffTimer { get => GetTopOffTimerControl(); set => SetTopOffTimerControl(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the trickle charge timer is set.
        /// Fixed at 1 hour.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="true"/>.
        /// </remarks>
        [Property]

        public bool EnableTrickleCharge { get => GetEnableTrickleChargeTimer(); set => SetEnableTrickleChargeTimer(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the precharge timer is set.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="true"/>.
        /// </remarks>
        [Property]
        public bool EnablePrechargeTimer { get => GetEnablePrechargeTimer(); set => SetEnablePrechargeTimer(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the fast charge timer is set.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="true"/>.
        /// </remarks>
        [Property]
        public bool EnableFastChargeTimer { get => GetEnableFastChargeTimer(); set => SetEnableFastChargeTimer(value); }

        /// <summary>
        /// Gets or sets the fast charge timer setting.
        /// </summary>
        /// <remarks>
        /// Default value is <see cref="FastChargeTimerSetting.Time_12H"/>.
        /// </remarks>
        [Property]
        public FastChargeTimerSetting FastChargeTimer { get => GetFastChargeTimer(); set => SetFastChargeTimer(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the trickle charge, pre-charge and fast charge timer are slowed by 2x during input DPM or thermal regulation.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="true"/>, the timer is slowed down.
        /// </remarks>
        [Property]
        public bool TimerControlSlowDown { get => GetEnable2xTimerSlowdown(); set => SetEnable2xTimerSlowdown(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the the auto battery discharging is enabled during the battery OVP fault.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="true"/>, The charger will apply a discharging current on BAT during battery OVP.
        /// </remarks>
        [Property]
        public bool AutoBatteryDischarge { get => GetAutoBatteryDischarge(); set => SetAutoBatteryDischarge(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the charger will apply a discharging current on BAT regardless the battery OVP status.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/> (idle), the charger won't apply a discharging current.
        /// </remarks>
        [Property]
        public bool ForceBatteryDischarge { get => GetForceBatteryDischarge(); set => SetForceBatteryDischarge(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the charge is enabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="true"/>, charging is enabled.
        /// </remarks>
        [Property]
        public bool EnableCharge { get => GetEnableCharger(); set => SetEnableCharger(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the input current optimizer is enabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, the input current optimizer is disabled.
        /// </remarks>
        [Property]
        public bool EnableInputCurrentOptimizer { get => GetEnableInputCurrentOptimizer(); set => SetEnableInputCurrentOptimizer(value); }

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
        public bool ForceStartInputCurrentOptimizer { get => GetForceStartInputCurrentOptimizer(); set => SetForceStartInputCurrentOptimizer(value); }

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
        public bool EnableHighImpedance { get => GetEnableHighImpedanceMode(); set => SetEnableHighImpedanceMode(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the charge termination is enabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="true"/>, the charge termination is enabled.
        /// </remarks>
        [Property]
        public bool EnableTermination { get => GetEnableTermination(); set => SetEnableTermination(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the D+/D- detection is forced.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Set to <see langword="true"/> to force the D+/D- detection. When D+/D- detection is done, this bit will be reset to <see langword="false"/>
        /// </para>
        /// </remarks>
        [Property]
        public bool ForceDDDetection { get => GetForceDDDetection(); set => SetForceDDDetection(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the automatic D+/D- detection is enabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="true"/>, D+/D- detection when VBUS is plugged-in is enabled.
        /// </remarks>
        [Property]
        public bool AutomaticDDDetection { get => GetAutoDDDetection(); set => SetAutoDDDetection(value); }

        /// <summary>
        /// Gets or sets a value indicating whether 12V mode in HVDCP is enabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, 12V mode in HVDCP is disabled.
        /// </remarks>
        [Property]
        public bool Enable12V { get => GetEnable12VInput(); set => SetEnable12VInput(value); }

        /// <summary>
        /// Gets or sets a value indicating whether 9V mode in HVDCP is enabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, 9V mode in HVDCP is disabled.
        /// </remarks>
        [Property]
        public bool Enable9V { get => GetEnable9VInput(); set => SetEnable9VInput(value); }

        /// <summary>
        /// Gets or sets a value indicating whether high voltage DCP handshake is enabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, high voltage DCP handshake is disabled.
        /// </remarks>
        [Property]
        public bool HighVoltageDcp { get => GetEnableHighVoltageDdcp(); set => SetEnableHighVoltageDdcp(value); }

        /// <summary>
        /// Gets or sets the external ship FET control logic to force the device enter different modes.
        /// </summary>
        [Property]
        public SFETControl SFETControl { get => GetSdrvControl(); set => SetSdrvControl(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the delay time is added to the taking action in <see cref="SFETControl"/>.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, the 10s delay time is added.
        /// </remarks>
        [Property]
        public bool SFETDelayDisabled { get => GetSdrvDelayEnable(); set => SetSdrvDelayEnable(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the AC drive is disabled.
        /// </summary>
        /// <remarks>
        /// When this is set to <see langword="true"/>, both AC drives are disabled.
        /// </remarks>
        [Property]
        public bool DisableAcDrive { get => GetDisableAcDrive(); set => SetDisableAcDrive(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the OTG mode is enabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, the OTG mode is disabled.
        /// </remarks>
        [Property]
        public bool OTGModeControl { get => GetOTGModeControl(); set => SetOTGModeControl(value); }

        /// <summary>
        /// Gets or sets a value indicating whether PFM is disabled in OTG mode.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, PFM is enabled in OTG mode.
        /// </remarks>
        [Property]
        public bool DisablePFMInOTG { get => GetDisablePFMInOTG(); set => SetDisablePFMInOTG(value); }

        /// <summary>
        /// Gets or sets a value indicating whether there will be a 15ms delay When waking up the device from ship mode, to pull low the QON pin.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, so there is a 1ms delay.
        /// </remarks>
        [Property]
        public bool WakeupDelay { get => GetWakeUpDelay(); set => SetWakeUpDelay(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the BATFET LDO  is disabled in pre-charge stage.
        /// </summary>
        /// <remarks>
        /// Default is <see langword="false"/>, the BATFET LDO mode is enabled.
        /// </remarks>
        [Property]
        public bool DisableLDO { get => GetDisableLDOInPreCharge(); set => SetDisableLDOInPreCharge(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the OOA is disabled in OTG mode.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, the OOA is enabled in OTG mode.
        /// </remarks>
        [Property]
        public bool DisableOOAInOTG { get => GetDisableOOAInOTGMode(); set => SetDisableOOAInOTGMode(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the OOA is disabled in Forward mode.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, the OOA is enabled in Forward mode.
        /// </remarks>
        [Property]
        public bool DisableOOAInForward { get => GetDisableOOAInForwardMode(); set => SetDisableOOAInForwardMode(value); }

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
        public bool EnableACDrive2 { get => GetEnableAcDrive1(); set => SetEnableAcDrive1(value); }

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
        public bool EnableACDrive1 { get => GetEnableAcDrive2(); set => SetEnableAcDrive2(value); }

        /// <summary>
        /// Gets or sets the <see cref="SwitchingFrequency"/> selection for the charget.
        /// </summary>
        /// <remarks>
        /// This bit POR default value is based on the PROG pin strapping indicating whether the external BATFET gate driver control is enabled.
        /// </remarks>
        [Property]
        public SwitchingFrequency SwitchingFrequency { get => GetPwmFrequency(); set => SetPwmFrequency(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the STAT pin output will be disabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, the STAT pin output is enabled.
        /// </remarks>
        [Property]
        public bool DisableStatPin { get => GetDisableStatPin(); set => SetDisableStatPin(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the forward mode VSYS short hiccup protection will be disabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, the forward mode VSYS short hiccup protection is enabled.
        /// </remarks> 
        [Property]
        public bool DisableForwardModeVSys { get => GetDisableForwardModeVsys(); set => SetDisableForwardModeVsys(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the OTG mode VOTG UVP hiccup protection will be disabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, the OTG mode VOTG UVP hiccup protection is enabled.
        /// </remarks>
        [Property]
        public bool DisableOTGModeUVP { get => GetDisableOtgModeUvp(); set => SetDisableOtgModeUvp(value); }

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
        public bool ForceVinDPMDetection { get => GetForceVinDpmDetection(); set => SetForceVinDpmDetection(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the IBUS OCP in OTG mode will be disabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="true"/>, the IBUS OCP in OTG mode is enabled.
        /// </remarks>
        [Property]
        public bool EnableIBusOcpInForwardMode { get => GetEnableIbusOcp(); set => SetEnableIbusOcp(value); }

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
        public bool SFETIsPresent { get => GetSFetPresent(); set => SetSFetPresent(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the battery discharge current sensing is enabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, the battery discharge current sensing is disabled.
        /// </remarks>
        [Property]
        public bool BatteryDischargeCurrentSensing { get => GetEnableBatteryDischargeSensing(); set => SetEnableBatteryDischargeSensing(value); }

        /// <summary>
        /// Gets or sets the battery discharge current.
        /// </summary>
        /// <remarks>
        /// Default value is <see cref="BatteryDischargeCurrent.Current_5A"/>.
        /// </remarks>
        [Property]
        public BatteryDischargeCurrent BatteryDischargeCurrent { get => GetBatteryDischargeCurrent(); set => SetBatteryDischargeCurrent(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the the internal IINDPM register input current regulation is enabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="true"/>, the internal IINDPM is enabled.
        /// </remarks>
        [Property]
        public bool EnableInternalDPM { get => GetEnableInputDpm(); set => SetEnableInputDpm(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the the external ILIM_HIZ pin input current regulation is enabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="true"/>, the external ILIM_HIZ pin is enabled.
        /// </remarks>
        [Property]
        public bool EnableExternalInpuCurrent { get => GetEnableExternalILIM(); set => SetEnableExternalILIM(value); }

        /// <summary>
        /// Gets or sets a value indicating whether the the battery discharging current OCP is enabled.
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="false"/>, the battery discharging current OCP is disabled.
        /// </remarks>
        [Property]
        public bool EnableBatteryDischargingCurrentOCP { get => GetEnableBatteryOCP(); set => SetEnableBatteryOCP(value); }

        /// <summary>
        /// Gets a value indicating whether the the ACFET2-RBFET2 is placed.
        /// </summary>
        [Property]
        public bool ACFET2IsPlaced => GetAcrb2Status();

        /// <summary>
        /// Gets a value indicating whether the the ACFET1-RBFET1 is placed.
        /// </summary>
        [Property]
        public bool ACFET1IsPlaced => GetAcrb1Status();

        /// <summary>
        /// Gets a value indicating whether the ADC converstion is complete. This is valid for only one-shot mode.
        /// </summary>
        [Property]
        public bool AdcConversionComplete => GetAdcDoneStatus();

        /// <summary>
        /// Gets a value indicating whether the charger is in VSYS regulation. This is valid for forward mode.
        /// </summary>
        [Property]
        public bool InVsysRegulation => GetVsysStatus();

        /// <summary>
        /// Gets a value indicating whether the fast charge safety timer has expired.
        /// </summary>
        /// <value><see langword="true"/> if the fast charge safety timer has expired, otherwise <see langword="false"/> meaning in normal operation.</value>
        [Property]
        public bool FastChargeSafetyTimerExpired => GetFastChargeTimerStatus();

        /// <summary>
        /// Gets a value indicating whether the trickle charge safety timer has expired.
        /// </summary>
        /// <value><see langword="true"/> if the trickle charge safety timer has expired, otherwise <see langword="false"/> meaning in normal operation.</value>
        [Property]
        public bool TrickleChargeSafetyTimerExpired => GetTrickleChargeTimerStatus();

        /// <summary>
        /// Gets a value indicating whether the pre-charge safety timer has expired.
        /// </summary>
        /// <value><see langword="true"/> if the pre-charge safety timer has expired, otherwise <see langword="false"/> meaning in normal operation.</value>
        [Property]
        public bool PreChargeSafetyTimerExpired => GetPreChargeTimerStatus();

        /// <summary>
        /// Gets a value indicating whether the the battery voltage is too low to enable OTG mode.
        /// </summary>
        /// <value><see langword="true"/> if the battery voltage is too low to enable OTG mode, otherwise <see langword="false"/> meaning that the battery voltage is high enough to enable the OTG operation.</value>
        [Property]
        public bool VBatLowForOTG => GetVBatOTGLowStatus();

        /// <summary>
        /// Gets a value indicating whether the TS temperature is in the cold range, lower than T1.
        /// </summary>
        /// <value><see langword="true"/> if the TS status is in cold range, otherwise <see langword="false"/>.</value>
        [Property]
        public bool TsTemperatureIsCold => GetTsColdStatus();

        /// <summary>
        /// Gets a value indicating whether the TS temperature is in the cool range, between T1 and T2.
        /// </summary>
        /// <value><see langword="true"/> if the TS status is in cool range, otherwise <see langword="false"/>.</value>
        [Property]
        public bool TsTemperatureIsCool => GetTsCoolStatus();

        /// <summary>
        /// Gets a value indicating whether the TS temperature is in the warm range, between T3 and T5.
        /// </summary>
        /// <value><see langword="true"/> if the TS status is in warm range, otherwise <see langword="false"/>.</value>
        [Property]
        public bool TsTemperatureIsWarm => GetTsWarmStatus();

        /// <summary>
        /// Gets a value indicating whether the TS temperature is in the hot range, higher than T5.
        /// </summary>
        /// <value><see langword="true"/> if the TS status is in hot range, otherwise <see langword="false"/>.</value>
        [Property]
        public bool TsTemperatureIsHot => GetTsHotStatus();

        /// <summary>
        /// Gets a value indicating whether the device is in battery discharging current regulation.
        /// </summary>
        /// <value><see langword="true"/> if the device is in battery discharging current regulation, otherwise <see langword="false"/> meaning in normal operation.</value>
        [Property]
        public bool DeviceInBatteryDischargingRegulation => GetIBatRegulationStatus();

        /// <summary>
        /// Gets a value indicating whether the VBUS is in over voltage protection.
        /// </summary>
        /// <value><see langword="true"/> if the VBUS is in over voltage protection, otherwise <see langword="false"/> meaning in normal operation.</value>
        [Property]
        public bool VbusInOverVoltageProtection => GetVbusOverVoltageStatus();

        /// <summary>
        /// Gets a value indicating whether the VBAT is in over voltage protection.
        /// </summary>
        /// <value><see langword="true"/> if the VBAT is in over voltage protection, otherwise <see langword="false"/> meaning in normal operation.</value>
        [Property]
        public bool VbatInOverVoltageProtection => GetVBatOverVoltageStatus();

        /// <summary>
        /// Gets a value indicating whether the IBUS is in over current protection.
        /// </summary>
        /// <value><see langword="true"/> if the IBUS is in over current protection, otherwise <see langword="false"/> meaning in normal operation.</value>
        [Property]
        public bool IbusInOverCurrentProtection => GetIBusOverCurrentStatus();

        /// <summary>
        /// Gets a value indicating whether the IBAT is in over current protection.
        /// </summary>
        /// <value><see langword="true"/> if the IBAT is in over current protection, otherwise <see langword="false"/> meaning in normal operation.</value>
        [Property]
        public bool IbatInOverCurrentProtection => GetIBatOverCurrentStatus();

        /// <summary>
        /// Gets a value indicating whether the converter is in over current protection.
        /// </summary>
        /// <value><see langword="true"/> if the converter is in over current protection, otherwise <see langword="false"/> meaning in normal operation.</value>
        [Property]
        public bool ConverterInOverCurrentProtection => GetConverterOverCurrentStatus();

        /// <summary>
        /// Gets a value indicating whether the VAC2 is in over voltage protection.
        /// </summary>
        /// <value><see langword="true"/> if the VAC2 is in over voltage protection, otherwise <see langword="false"/> meaning in normal operation.</value>
        [Property]
        public bool Vac2InOverVoltageProtection => GetVac2OverVoltageStatus();

        /// <summary>
        /// Gets a value indicating whether the VAC1 is in over voltage protection.
        /// </summary>
        /// <value><see langword="true"/> if the VAC1 is in over voltage protection, otherwise <see langword="false"/> meaning in normal operation.</value>
        [Property]
        public bool Vac1InOverVoltageProtection => GetVac1OverVoltageStatus();

        /// <summary>
        /// Gets a value indicating whether the device is in SYS short circuit protection.
        /// </summary>
        /// <value><see langword="true"/> if the device is in SYS short circuit protection, otherwise <see langword="false"/> meaning in normal operation.</value>
        [Property]
        public bool DeviceInSysShortCircuitProtection => GetVsysShortCircuitStatus();

        /// <summary>
        /// Gets a value indicating whether a IINDPM/IOTG signal rising edge was detected.
        /// </summary>
        /// <value><see langword="true"/> if a IINDPM/IOTG signal rising edge was detected, otherwise <see langword="false"/>.</value>
        [Property]
        public bool IINDPMSignalDetected => GetIIndpmFlag();

        /// <summary>
        /// Gets a value indicating whether a VINDPM signal rising edge was detected.
        /// </summary>
        /// <value><see langword="true"/> if a VINDPM signal rising edge was detected, otherwise <see langword="false"/>.</value>
        [Property]
        public bool VINDPMSignalDetected => GetVIndpmFlag();

        /// <summary>
        /// Gets a value indicating whether a I2C watchdog timer event occurred.
        /// </summary>
        /// <value><see langword="true"/> if a I2C watchdog timer event occurred, otherwise <see langword="false"/>.</value>
        [Property]
        public bool I2cWatchdogSignaled => GetI2cWatchdogFlag();

        /// <summary>
        /// Gets a value indicating whether a poor source was detected.
        /// </summary>
        /// <value><see langword="true"/> if a poor source was detected, otherwise <see langword="false"/>.</value>
        [Property]
        public bool PoorSourceDetected => GetPoorSourceFlag();

        /// <summary>
        /// Gets a value indicating whether a power good flag change has been set.
        /// </summary>
        /// <value><see langword="true"/> on any change in PG_STAT even (adapter good qualification or adapter good going away), otherwise <see langword="false"/>.</value>
        [Property]
        public bool PowerGoodHasChanged => GetPowerGoodFlag();

        /// <summary>
        /// Gets a value indicating whether a AC2 present flag change has been set.
        /// </summary>
        /// <value><see langword="true"/> on any change in VAC2 present status, otherwise <see langword="false"/>.</value>
        [Property]
        public bool Ac1PresentHasChanged => GetAc1PresentFlag();

        /// <summary>
        /// Gets a value indicating whether a AC1 present flag change has been set.
        /// </summary>
        /// <value><see langword="true"/> on any change in VAC1 present status, otherwise <see langword="false"/>.</value>
        [Property]
        public bool Ac2PresentHasChanged => GetAc2PresentFlag();

        /// <summary>
        /// Gets a value indicating whether a VBUS present flag change has been set.
        /// </summary>
        /// <value><see langword="true"/> on any change in VBUS present status, otherwise <see langword="false"/>.</value>
        [Property]
        public bool VbusPresentHasChanged => GetVbusPresentFlag();

        /// <summary>
        /// Gets a value indicating whether a charge status flag has changed.
        /// </summary>
        /// <value><see langword="true"/> on any change in charge status, otherwise <see langword="false"/>.</value>
        [Property]
        public bool ChargeStatusChanged => GetChargeStatusFlag();

        /// <summary>
        /// Gets a value indicating whether a ICO status flag has changed.
        /// </summary>
        /// <value><see langword="true"/> on any change in ICO status, otherwise <see langword="false"/>.</value>
        [Property]
        public bool IcoStatusChanged => GetIcoStatusFlag();

        /// <summary>
        /// Gets a value indicating whether a Vbus status flag has changed.
        /// </summary>
        /// <value><see langword="true"/> on any change in Vbus status, otherwise <see langword="false"/>.</value>
        [Property]
        public bool VbusStatusChanged => GetVbusStatusFlag();

        /// <summary>
        /// Gets a value indicating whether a TREG signal rising threshold was detected.
        /// </summary>
        /// <value><see langword="true"/> if a TREG signal rising threshold was detected, otherwise <see langword="false"/>.</value>
        [Property]
        public bool ThermalRegulationThresholdReached => GetThermalRegulationFlag();

        /// <summary>
        /// Gets a value indicating whether a VBAT present flag change has been set.
        /// </summary>
        /// <value><see langword="true"/> on any change in VBAT present status, otherwise <see langword="false"/>.</value>
        [Property]
        public bool VbatPresentChanged => GetVbatPresentFlag();

        /// <summary>
        /// Gets a value indicating whether a change in BC1.2 detection has occurred.
        /// </summary>
        [Property]
        public bool BC12DetectionChanged => GetBc12DetectionFlag();

        /// <summary>
        /// Gets a value indicating whether a D+/D- detection is completed.
        /// </summary>
        /// <value><see langword="true"/> if the D+/D- detection is completed, otherwise <see langword="false"/> if it hasn't started or is ongoing.</value>
        [Property]
        public bool DpDmDetectionCompleted => GetDpdmDoneFlag();

        /// <summary>
        /// Gets a value indicating whether an ADC conversion completed.
        /// </summary>
        /// <value><see langword="true"/> if the ADC conversion is completed, otherwise <see langword="false"/> if it hasn't started or is ongoing.</value>
        [Property]
        public bool AdcConversionDone => GetAdcDoneFlag();

        /// <summary>
        /// Gets a value indicating whether the charger has entered or existed VSYSMIN regulation.
        /// </summary>
        /// <value><see langword="true"/> if the charger has entered or existed VSYSMIN regulation, otherwise <see langword="false"/>.</value>
        [Property]
        public bool VsysMinChanged => GetVsysFlag();

        /// <summary>
        /// Gets a value indicating whether a fast charge timer expired rising edge detected.
        /// </summary>
        /// <value><see langword="true"/> if a fast charge timer expired rising edge detected, otherwise <see langword="false"/>.</value>
        [Property]
        public bool FastChargeTimerExpired => GetFastChargeTimerFlag();

        /// <summary>
        /// Gets a value indicating whether a trickle charge timer expired rising edge detected.
        /// </summary>
        /// <value><see langword="true"/> if a trickle charge timer expired rising edge detected, otherwise <see langword="false"/>.</value>
        [Property]
        public bool TrickleChargeTimerExpired => GetTrickleChargeTimerFlag();

        /// <summary>
        /// Gets a value indicating whether a pre-charge timer expired rising edge detected.
        /// </summary>
        /// <value><see langword="true"/> if a pre-charge timer expired rising edge detected, otherwise <see langword="false"/>.</value>
        [Property]
        public bool PreChargeTimerExpired => GetPreChargeTimerFlag();

        /// <summary>
        /// Gets a value indicating whether the top off timer expired rising edge detected.
        /// </summary>
        /// <value><see langword="true"/> if the top off timer expired rising edge detected, otherwise <see langword="false"/>.</value>
        [Property]
        public bool TopOffTimerExpired => GetTopOffTimerFlag();

        /// <summary>
        /// Gets a value indicating whether the VBAT falls below the threshold to enable the OTG mode.
        /// </summary>
        /// <value><see langword="true"/> if the VBAT falls below the threshold to enable the OTG mode, otherwise <see langword="false"/>.</value>
        [Property]
        public bool VBatTooLowForOTG => GetVBatOTGLowFlag();

        /// <summary>
        /// Gets a value indicating whether the TS across cold temperature (T1) is detected.
        /// </summary>
        /// <value><see langword="true"/> if the TS cold temperature (T1) is detected, otherwise <see langword="false"/>.</value>
        [Property]
        public bool TsColdDetected => GetTsColdFlag();

        /// <summary>
        /// Gets a value indicating whether the TS across cool temperature (T2) is detected.
        /// </summary>
        /// <value><see langword="true"/> if the TS cool temperature (T2) is detected, otherwise <see langword="false"/>.</value>
        [Property]
        public bool TsCoolDetected => GetTsCoolFlag();

        /// <summary>
        /// Gets a value indicating whether the TS across warm temperature (T3) is detected.
        /// </summary>
        /// <value><see langword="true"/> if the TS warm temperature (T3) is detected, otherwise <see langword="false"/>.</value>
        [Property]
        public bool TsWarmDetected => GetTsWarmFlag();

        /// <summary>
        /// Gets a value indicating whether the TS across hot temperature (T5) is detected.
        /// </summary>
        /// <value><see langword="true"/> if the TS hot temperature (T5) is detected, otherwise <see langword="false"/>.</value>
        [Property]
        public bool TsHotDetected => GetTsHotFlag();

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

            // read part information
            ReadPartInformation();
        }

        /// <summary>
        /// Read part information to perform a sanity check for the correct part.
        /// </summary>
        private void ReadPartInformation()
        {
            byte[] buffer = ReadFromRegister(Register.REG48_Part_Information, 1);

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
            WriteToRegister(Register.REG10_Charger_Control_1, 0b0000_1000);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }

        #region REG1B_Charger_Status_0

        // REG1B_Charger_Status_0 Register
        // | 7 6 5 4 3 2 1 0 |
        // | IINDPM_STAT | VINDPM_STAT | WD_STAT | RESERVED | PG_STAT | AC2_PRESENT_STAT | AC1_PRESENT_STAT | VBUS_PRESENT_STAT |
        ////

        private ChargerStatus0 GetChargerStatus0()
        {
            byte[] buffer = ReadFromRegister(Register.REG1C_Charger_Status_1, 1);

            return (ChargerStatus0)buffer[0];
        }

        #endregion

        #region REG1C_Charger_Status_1

        // REG1C_Charger_Status_1 Register
        // |    7 6 5     |     4 3 2 1   |         0       |
        // | CHG_STAT_2:0 | VBUS_STAT_3:0 | BC1.2_DONE_STAT |
        ////

        private ChargeStatus GetChargeStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG1C_Charger_Status_1, 1);

            return (ChargeStatus)(buffer[0] & ChargerStatus1ChargeStatusMask);
        }

        private VbusStatus GetVbusStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG1C_Charger_Status_1, 1);

            return (VbusStatus)(buffer[0] & ChargerStatus1VbusStatusMask);
        }

        private bool GetBc12Detection()
        {
            byte[] buffer = ReadFromRegister(Register.REG1C_Charger_Status_1, 1);

            return (buffer[0] & ChargerStatus1Bc12DetectionMask) == 1;
        }

        #endregion

        #region REG1D_Charger_Status_2

        // REG1D_Charger_Status_2 Register
        // |      7 6     |   5 4 3  |     2     |    1      |         0         |
        // | ICO_STAT_1:0 | RESERVED | TREG_STAT | DPDM_STAT | VBAT_PRESENT_STAT |
        ////

        private ChargerStatus2 GetChargerStatus2()
        {
            byte[] buffer = ReadFromRegister(Register.REG1D_Charger_Status_2, 1);

            return (ChargerStatus2)buffer[0];
        }

        private IcoStatus GetIcoStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG1D_Charger_Status_2, 1);

            return (IcoStatus)(buffer[0] & ChargerStatus3IcoStatusMask);
        }

        #endregion

        #region REG17_NTC_Control_0

        // REG17_NTC_Control_0 Register
        // |    7   6   5   |     4    3      |     2    1      |     0    |
        // | JEITA_VSET_2:0 | JEITA_ISETH_1:0 | JEITA_ISETC_1:0 | RESERVED |
        ////

        private ChargeVoltage GetChargeVoltage()
        {
            byte[] buffer = ReadFromRegister(Register.REG17_NTC_Control_0, 1);

            return (ChargeVoltage)(buffer[0] & NtcControl0ChargerVoltageMask);
        }

        private void SetChargeVoltage(ChargeVoltage value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG17_NTC_Control_0, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~NtcControl0ChargerVoltageMask);

            // set value
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG17_NTC_Control_0, buffer[0]);
        }

        private ChargeCurrent GetChargeCurrentHighTempRange()
        {
            byte[] buffer = ReadFromRegister(Register.REG17_NTC_Control_0, 1);

            // need to shift 3 positions to the right to get the value
            return (ChargeCurrent)((buffer[0] & NtcControl0ChargeCurrentMask) >> 3);
        }

        private void SetChargeCurrentHighTempRange(ChargeCurrent value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG17_NTC_Control_0, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~NtcControl0ChargeCurrentMask);

            // set value
            // need to shift 3 positions to the left to get the value into the correct position
            buffer[0] |= (byte)((byte)value << 3);

            WriteToRegister(Register.REG17_NTC_Control_0, buffer[0]);
        }

        private ChargeCurrent GetChargeCurrentLowTempRange()
        {
            byte[] buffer = ReadFromRegister(Register.REG17_NTC_Control_0, 1);

            // need to shift 1 position to the right to get the value
            return (ChargeCurrent)((buffer[0] & NtcControl0ChargeCurrentMask) >> 1);
        }

        private void SetChargeCurrentLowTempRange(ChargeCurrent value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG17_NTC_Control_0, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~NtcControl0ChargeCurrentMask);

            // set value
            // need to shift 1 position to the left to get the value into the correct position
            buffer[0] |= (byte)((byte)value << 1);

            WriteToRegister(Register.REG17_NTC_Control_0, buffer[0]);
        }

        #endregion

        #region REG18_NTC_Control_1

        // REG18_NTC_Control_1 Register
        // |    7   6    |    5  4     |   3  2   |   1   |     0     |
        // | TS_COOL_1:0 | TS_WARM_1:0 | BHOT_1:0 | BCOLD | TS_IGNORE |
        ////

        private CompVoltageRisingThreshold GetVt2ComparatorVoltage()
        {
            byte[] buffer = ReadFromRegister(Register.REG18_NTC_Control_1, 1);

            return (CompVoltageRisingThreshold)(buffer[0] & NtcControl1Vt2ComparatorVoltageMask);
        }

        private void SetVt2ComparatorVoltage(CompVoltageRisingThreshold value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG18_NTC_Control_1, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~NtcControl1Vt2ComparatorVoltageMask);

            // set value
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG18_NTC_Control_1, buffer[0]);
        }

        private CompVoltageFallingThreshold GetVt3ComparatorVoltage()
        {
            byte[] buffer = ReadFromRegister(Register.REG18_NTC_Control_1, 1);

            return (CompVoltageFallingThreshold)(buffer[0] & NtcControl1Vt3ComparatorVoltageMask);
        }

        private void SetVt3ComparatorVoltage(CompVoltageFallingThreshold value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG18_NTC_Control_1, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~NtcControl1Vt3ComparatorVoltageMask);

            // set value
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG18_NTC_Control_1, buffer[0]);
        }

        private OtgHotTempThreshold GetOtgHotTempThreshold()
        {
            byte[] buffer = ReadFromRegister(Register.REG18_NTC_Control_1, 1);

            return (OtgHotTempThreshold)(buffer[0] & NtcControl1OtgHotTemperatureMask);
        }

        private void SetOtgHotTempThreshold(OtgHotTempThreshold value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG18_NTC_Control_1, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~NtcControl1OtgHotTemperatureMask);

            // set value
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG18_NTC_Control_1, buffer[0]);
        }

        private OtgColdTempThreshold GetOtgColdTempThreshold()
        {
            byte[] buffer = ReadFromRegister(Register.REG18_NTC_Control_1, 1);

            return (OtgColdTempThreshold)(buffer[0] & NtcControl1OtgColdTemperatureMask);
        }

        private void SetOtgColdTempThreshold(OtgColdTempThreshold value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG18_NTC_Control_1, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~NtcControl1OtgColdTemperatureMask);

            // set value
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG18_NTC_Control_1, buffer[0]);
        }

        private bool GetIgnoreTempSensor()
        {
            byte[] buffer = ReadFromRegister(Register.REG18_NTC_Control_1, 1);

            return (buffer[0] & NtcControl1IgnoreTSMask) == 1;
        }

        private void SetIgnoreTempSensor(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG18_NTC_Control_1, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~NtcControl1IgnoreTSMask);

            // set value
            buffer[0] |= (byte)(value ? 0b0001 : 0b0000);

            WriteToRegister(Register.REG18_NTC_Control_1, buffer[0]);
        }

        #endregion

        #region REG00_Minimal_System_Voltage

        // REG00_Minimal_System_Voltage
        // | 7 6      | 5 4 3 2 1 0 |
        // | RESERVED | VSYSMIN_5:0 |
        ////

        private ElectricPotentialDc GetMinimalSystemVoltage()
        {
            byte[] buffer = ReadFromRegister(Register.REG00_Minimal_System_Voltage, 1);

            return new ElectricPotentialDc(
                (buffer[0] * StepMinimalSystemVoltage) + FixedOffsetMinimalSystemVoltage,
                UnitsNet.Units.ElectricPotentialDcUnit.MillivoltDc);
        }

        private void SetMinimalSystemVoltage(ElectricPotentialDc value)
        {
            // sanity check
            if (value.MillivoltsDc < FixedOffsetMinimalSystemVoltage
                || value.MillivoltsDc > MaxValueMinimalSystemVoltage)
            {
                throw new ArgumentOutOfRangeException();
            }

            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG00_Minimal_System_Voltage, 1);

            // divide by step value, as the register takes the value as 240mV steps
            var newValue = value.MillivoltsDc / StepMinimalSystemVoltage;

            // process value to replace VSYSMIN_5:0
            // no need to mask as the value has to be already 6 bits wide
            buffer[0] |= (byte)newValue;

            WriteToRegister(Register.REG00_Minimal_System_Voltage, buffer);
        }

        #endregion

        #region REG01_Charge_Voltage_Limit

        // REG01_Charge_Voltage_Limit
        // | 15 14 13 12 11 | 10 9 8 | 7 6 5 4 3 2 1 0 |
        // |    RESERVED    |        VREG_10:0         |
        ////

        private ElectricPotentialDc GetChargeVoltageLimit()
        {
            byte[] buffer = ReadFromRegister(Register.REG01_Charge_Voltage_Limit, 2);

            var vbus = (buffer[0] << 8) | buffer[1];

            return new ElectricPotentialDc(
                vbus * ChargeVoltageStep,
                UnitsNet.Units.ElectricPotentialDcUnit.MillivoltDc);
        }

        private void SetChargeVoltageLimit(ElectricPotentialDc value)
        {
            // sanity check
            if (value.MillivoltsDc < FixedOffsetMinimalChargeVoltageLimit
                || value.MillivoltsDc > MaxValueChargeVoltageLimit)
            {
                throw new ArgumentOutOfRangeException();
            }

            // divide by step value, as the register takes the value as 10mV steps
            var newValue = value.MillivoltsDc / ChargeVoltageStep;

            byte[] buffer = new byte[2];

            // process value 
            buffer[0] |= (byte)newValue;
            buffer[1] |= (byte)((int)newValue >> 8);

            WriteToRegister(Register.REG01_Charge_Voltage_Limit, buffer);
        }

        #endregion

        #region REG03_Charge_Current_Limit

        // REG03_Charge_Current_Limit
        // | 15 14 13 12 11 10 9 8 | 7 6 5 4 3 2 1 0 |
        // |       RESERVED        |     ICHG_8:0    |
        ////

        private ElectricCurrent GetChargeCurrentLimit()
        {
            byte[] buffer = ReadFromRegister(Register.REG03_Charge_Current_Limit, 2);

            var vbus = (buffer[0] << 8) | buffer[1];

            return new ElectricCurrent(
                vbus * ChargeInputCurrentStep,
                UnitsNet.Units.ElectricCurrentUnit.Milliampere);
        }

        private void SetChargeCurrentLimit(ElectricCurrent value)
        {
            // sanity check
            if (value.Milliamperes < FixedOffsetMinimalChargeCurrentLimit
                || value.Milliamperes > MaxValueChargeCurrentLimit)
            {
                throw new ArgumentOutOfRangeException();
            }

            // divide by step value, as the register takes the value as 10mA steps
            var newValue = value.Milliamperes / ChargeInputCurrentStep;

            byte[] buffer = new byte[2];

            // process value 
            buffer[0] |= (byte)newValue;
            buffer[1] |= (byte)((int)newValue >> 8);

            WriteToRegister(Register.REG03_Charge_Current_Limit, buffer);
        }

        #endregion

        #region REG05_Input_Voltage_Limit

        // REG05_INput_Voltage_Limit
        // | 7 6 5 4 3 2 1 0 |
        // |   VINDPM_7:0    |
        ////

        private ElectricPotentialDc GetInputVoltageLimit()
        {
            byte[] buffer = ReadFromRegister(Register.REG05_Input_Voltage_Limit, 1);

            return new ElectricPotentialDc(
                buffer[0] * InputVoltageStep,
                UnitsNet.Units.ElectricPotentialDcUnit.MillivoltDc);
        }

        private void SetInputVoltageLimit(ElectricPotentialDc value)
        {
            // sanity check
            if (value.MillivoltsDc < MinValueInputVoltageLimit
                || value.MillivoltsDc > MaxValueInputVoltageLimit)
            {
                throw new ArgumentOutOfRangeException();
            }

            // divide by step value, as the register takes the value as 100mV steps
            var newValue = value.MillivoltsDc / InputVoltageStep;

            WriteToRegister(Register.REG05_Input_Voltage_Limit, (byte)newValue);
        }

        #endregion

        #region REG06_Input_Current_Limit

        // REG06_Charge_Current_Limit
        // | 15 14 13 12 11 10 9 | 8 7 6 5 4 3 2 1 0 |
        // |      RESERVED       |    IINDPM_8:0     |
        ////

        private ElectricCurrent GetInputCurrentLimit()
        {
            byte[] buffer = ReadFromRegister(Register.REG06_Input_Current_Limit, 2);

            var vbus = (buffer[0] << 8) | buffer[1];

            return new ElectricCurrent(
                vbus * ChargeInputCurrentStep,
                UnitsNet.Units.ElectricCurrentUnit.Milliampere);
        }

        private void SetInputCurrentLimit(ElectricCurrent value)
        {
            // sanity check
            if (value.Milliamperes < FixedOffsetMinimalInputCurrentLimit
                || value.Milliamperes > MaxValueInputCurrentLimit)
            {
                throw new ArgumentOutOfRangeException();
            }

            // divide by step value, as the register takes the value as 10mA steps
            var newValue = value.Milliamperes / ChargeInputCurrentStep;

            byte[] buffer = new byte[2];

            // process value 
            buffer[0] |= (byte)newValue;
            buffer[1] |= (byte)((int)newValue >> 8);

            WriteToRegister(Register.REG06_Input_Current_Limit, buffer);
        }

        #endregion

        #region REG08_Precharge_Control

        // REG08_Precharge_Control Register
        // | 7 6           | 5 4 3 2 1 0 |
        // | VBAT_LOWV_1:0 | IPRECHG_5:0 |
        ////

        private ThresholdFastCharge GetThresholdFastCharge()
        {
            byte[] buffer = ReadFromRegister(Register.REG08_Precharge_Control, 1);

            return (ThresholdFastCharge)(buffer[0] >> 6);
        }

        private void SetThresholdFastCharge(ThresholdFastCharge value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG08_Precharge_Control, 1);

            // process value to replace VBAT_LOWV_1:0
            buffer[0] = (byte)(((byte)value << 6) | (byte)(buffer[0] & 0b0011_1111));
        }

        private ElectricCurrent GetPrechargeCurrentLimit()
        {
            byte[] buffer = ReadFromRegister(Register.REG08_Precharge_Control, 1);

            return new ElectricCurrent(
                buffer[0] & PrechargeCurrentLimitMask * StepPrechargeCurrentLimit,
                UnitsNet.Units.ElectricCurrentUnit.Milliampere);
        }

        private void SetPrechargeCurrentLimit(ElectricCurrent value)
        {
            // sanity check
            if (value.Milliamperes < PrechargeCurrentLimitMinValue
                || value.Milliamperes > PrechargeCurrentLimitMaxValue)
            {
                throw new ArgumentOutOfRangeException();
            }

            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG08_Precharge_Control, 1);

            // divide by 40 as the register takes the value as 40mA steps
            var newValue = value.Milliamperes / StepPrechargeCurrentLimit;

            // process value to replace IPRECHG_5:0
            // no need to mask as the value has to be already 6 bits wide
            buffer[0] |= (byte)newValue;
        }

        #endregion

        #region REG09_Termination_Control Register

        // REG09_Termination_Control Register Register
        // |     7    |    6    |     5    | 4 3 2 1 0 |
        // | RESERVED | REG_RST | RESERVED | ITERM_4:0 |
        ////

        private ElectricCurrent GetTerminationCurrent()
        {
            byte[] buffer = ReadFromRegister(Register.REG09_Charge_Termination_Current, 1);

            return new ElectricCurrent(
                buffer[0] & TerminationCurrentMask * TerminationCurrentMinValue,
                UnitsNet.Units.ElectricCurrentUnit.Milliampere);
        }

        private void SetTerminationCurrent(ElectricCurrent value)
        {
            // sanity check
            if (value.Milliamperes < TerminationCurrentMinValue
                || value.Milliamperes > TerminationCurrentMaxValue)
            {
                throw new ArgumentOutOfRangeException();
            }

            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG09_Charge_Termination_Current, 1);

            // divide by 40 as the register takes the value as 40mA steps
            var newValue = value.Milliamperes / TerminationCurrentMinValue;

            // process value to replace ITERM_4:0
            // no need to mask as the value has to be already 5 bits wide
            buffer[0] |= (byte)newValue;
        }

        #endregion

        #region REG0E_Timer_Control

        // REG0E_Timer_Control Register
        // |       7 6      |       5       |        4       |      3     |    2 1      |     0    |
        // | TOPOFF_TMR_1:0 | EN_TRICHG_TMR | EN_PRECHG__TMR | EN_CHG_TMR | CHG_TMR_1:0 | TMR2X_EN |
        ////

        private TopOffTimerControl GetTopOffTimerControl()
        {
            byte[] buffer = ReadFromRegister(Register.REG0E_Timer_Control, 1);

            return (TopOffTimerControl)(buffer[0] & TimerControlTopOffMask);
        }

        private void SetTopOffTimerControl(TopOffTimerControl value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG0E_Timer_Control, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~TimerControlTopOffMask);
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG0E_Timer_Control, buffer[0]);
        }

        private bool GetEnableTrickleChargeTimer()
        {
            byte[] buffer = ReadFromRegister(Register.REG0E_Timer_Control, 1);

            return (buffer[0] & TimerControlEnableTrickleChargeMask) == 1;
        }

        private void SetEnableTrickleChargeTimer(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG0E_Timer_Control, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~TimerControlEnableTrickleChargeMask);

            // set value
            buffer[0] |= (byte)(value ? TimerControlEnableTrickleChargeMask : 0b0000_0000);

            WriteToRegister(Register.REG0E_Timer_Control, buffer[0]);
        }

        private bool GetEnablePrechargeTimer()
        {
            byte[] buffer = ReadFromRegister(Register.REG0E_Timer_Control, 1);

            return (buffer[0] & TimerControlEnablePrechargeTimerMask) == 1;
        }

        private void SetEnablePrechargeTimer(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG0E_Timer_Control, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~TimerControlEnablePrechargeTimerMask);

            // set value
            buffer[0] |= (byte)(value ? TimerControlEnablePrechargeTimerMask : 0b0000_0000);

            WriteToRegister(Register.REG0E_Timer_Control, buffer[0]);
        }

        private bool GetEnableFastChargeTimer()
        {
            byte[] buffer = ReadFromRegister(Register.REG0E_Timer_Control, 1);

            return (buffer[0] & TimerControlEnableFastChargeMask) == 1;
        }

        private void SetEnableFastChargeTimer(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG0E_Timer_Control, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~TimerControlEnableFastChargeMask);

            // set value
            buffer[0] |= (byte)(value ? TimerControlEnableFastChargeMask : 0b0000_0000);

            WriteToRegister(Register.REG0E_Timer_Control, buffer[0]);
        }

        private FastChargeTimerSetting GetFastChargeTimer()
        {
            byte[] buffer = ReadFromRegister(Register.REG0E_Timer_Control, 1);

            return (FastChargeTimerSetting)(buffer[0] & TimerControlFastChargeTimerMask);
        }

        private void SetFastChargeTimer(FastChargeTimerSetting value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG0E_Timer_Control, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~TimerControlFastChargeTimerMask);
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG0E_Timer_Control, buffer[0]);
        }

        private bool GetEnable2xTimerSlowdown()
        {
            byte[] buffer = ReadFromRegister(Register.REG0E_Timer_Control, 1);

            return (buffer[0] & TimerControl2xTimerMask) == 1;
        }

        private void SetEnable2xTimerSlowdown(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG0E_Timer_Control, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~TimerControl2xTimerMask);

            // set value
            buffer[0] |= (byte)(value ? TimerControl2xTimerMask : 0b0000_0000);

            WriteToRegister(Register.REG0E_Timer_Control, buffer[0]);
        }

        #endregion

        #region REG0F_Charger_Control_0

        // REG0F_Charger_Control_0 Register
        // |        7        |       6       |    5   |    4   |     3     |    2   |    1    |     0    |
        // | EN_AUTO_IBATDIS | FORCE_IBATDIS | EN_CHG | EN_ICO | FORCE_ICO | EN_HIZ | EN_TERM | RESERVED |
        ////

        private bool GetAutoBatteryDischarge()
        {
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            return (buffer[0] & ChargerControl0AutoBatteryDischargeMask) == 1;
        }

        private void SetAutoBatteryDischarge(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl0AutoBatteryDischargeMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl0AutoBatteryDischargeMask : 0b0000_0000);

            WriteToRegister(Register.REG0F_Charger_Control_0, buffer[0]);
        }

        private bool GetForceBatteryDischarge()
        {
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            return (buffer[0] & ChargerControl0ForceBatteryDischargeMask) == 1;
        }

        private void SetForceBatteryDischarge(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl0ForceBatteryDischargeMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl0ForceBatteryDischargeMask : 0b0000_0000);

            WriteToRegister(Register.REG0F_Charger_Control_0, buffer[0]);
        }

        private bool GetEnableCharger()
        {
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            return (buffer[0] & ChargerControl0ChargerEnableMask) == 1;
        }

        private void SetEnableCharger(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl0ChargerEnableMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl0ChargerEnableMask : 0b0000_0000);

            WriteToRegister(Register.REG0F_Charger_Control_0, buffer[0]);
        }

        private bool GetEnableInputCurrentOptimizer()
        {
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            return (buffer[0] & ChargerControl0InputCurrentOptimizerMask) == 1;
        }

        private void SetEnableInputCurrentOptimizer(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl0InputCurrentOptimizerMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl0InputCurrentOptimizerMask : 0b0000_0000);

            WriteToRegister(Register.REG0F_Charger_Control_0, buffer[0]);
        }

        private bool GetForceStartInputCurrentOptimizer()
        {
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            return (buffer[0] & ChargerControl0ForceStartInputMask) == 1;
        }

        private void SetForceStartInputCurrentOptimizer(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl0ForceStartInputMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl0ForceStartInputMask : 0b0000_0000);

            WriteToRegister(Register.REG0F_Charger_Control_0, buffer[0]);
        }

        private bool GetEnableHighImpedanceMode()
        {
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            return (buffer[0] & ChargerControl0EnableHizMask) == 1;
        }

        private void SetEnableHighImpedanceMode(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl0EnableHizMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl0EnableHizMask : 0b0000_0000);

            WriteToRegister(Register.REG0F_Charger_Control_0, buffer[0]);
        }

        private bool GetEnableTermination()
        {
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            return (buffer[0] & ChargerControl0EnableTerminationMask) == 1;
        }

        private void SetEnableTermination(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl0EnableTerminationMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl0EnableTerminationMask : 0b0000_0000);

            WriteToRegister(Register.REG0F_Charger_Control_0, buffer[0]);
        }

        #endregion

        #region REG10_Charger_Control_1

        private WatchdogSetting GetWatchdogTimerSetting()
        {
            byte[] buffer = ReadFromRegister(Register.REG10_Charger_Control_1, 1);

            return (WatchdogSetting)(buffer[0] & ChargerControl1WatchdogMask);
        }

        private void SetWatchdogTimerSetting(WatchdogSetting value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG10_Charger_Control_1, 1);

            buffer[0] = (byte)(buffer[0] & ~ChargerControl1WatchdogMask);
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG10_Charger_Control_1, buffer[0]);
        }

        #endregion

        #region REG11_Charger_Control_2

        // REG11_Charger_Control_2 Register
        // |      7      |        6      |    5   |   4   |     3    |      2 1      |     0    |
        // | FORCE_INDET | AUTO_INDET_EN | EN_12V | EN_9V | HVDCP_EN | SDRV_CTRL_1:0 | SDRV_DLY |
        ////

        private bool GetForceDDDetection()
        {
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            return (buffer[0] & ChargerControl2ForceDDDetectionMask) == 1;
        }

        private void SetForceDDDetection(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl2ForceDDDetectionMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl2ForceDDDetectionMask : 0b0000_0000);

            WriteToRegister(Register.REG11_Charger_Control_2, buffer[0]);
        }

        private bool GetAutoDDDetection()
        {
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            return (buffer[0] & ChargerControl2AutomaticDDDetectionMask) == 1;
        }

        private void SetAutoDDDetection(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl2AutomaticDDDetectionMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl2AutomaticDDDetectionMask : 0b0000_0000);

            WriteToRegister(Register.REG11_Charger_Control_2, buffer[0]);
        }

        private bool GetEnable12VInput()
        {
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            return (buffer[0] & ChargerControl2Enable12VMask) == 1;
        }

        private void SetEnable12VInput(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl2Enable12VMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl2Enable12VMask : 0b0000_0000);

            WriteToRegister(Register.REG11_Charger_Control_2, buffer[0]);
        }

        private bool GetEnable9VInput()
        {
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            return (buffer[0] & ChargerControl2Enable9VMask) == 1;
        }

        private void SetEnable9VInput(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl2Enable9VMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl2Enable9VMask : 0b0000_0000);

            WriteToRegister(Register.REG11_Charger_Control_2, buffer[0]);
        }

        private bool GetEnableHighVoltageDdcp()
        {
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            return (buffer[0] & ChargerControl2HighVoltageDcpMask) == 1;
        }

        private void SetEnableHighVoltageDdcp(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl2HighVoltageDcpMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl2HighVoltageDcpMask : 0b0000_0000);

            WriteToRegister(Register.REG11_Charger_Control_2, buffer[0]);
        }

        private SFETControl GetSdrvControl()
        {
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            return (SFETControl)(buffer[0] & ChargerControl2SFETControlMask);
        }

        private void SetSdrvControl(SFETControl value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl2SFETControlMask);
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG11_Charger_Control_2, buffer[0]);
        }

        private bool GetSdrvDelayEnable()
        {
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            return (buffer[0] & ChargerControl2SdrvDelayMask) == 1;
        }

        private void SetSdrvDelayEnable(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl2SdrvDelayMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl2SdrvDelayMask : 0b0000_0000);

            WriteToRegister(Register.REG11_Charger_Control_2, buffer[0]);
        }

        #endregion

        #region REG12_Charger_Control_3

        // REG12_Charger_Control_3 Register
        // |     7     |   6    |      5      |      4      |    3     |    2    |      1      |      0      |
        // | DIS_ACDRV | EN_OTG | PFM_OTG_DIS | PFM_FWD_DIS | WKUP_DLY | DIS_LDO | DIS_OTG_OOA | DIS_FWD_OOA |
        ////

        private bool GetDisableAcDrive()
        {
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            return (buffer[0] & ChargerControl3DisableAcDriveMask) == 1;
        }

        private void SetDisableAcDrive(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl3DisableAcDriveMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl3DisableAcDriveMask : 0b0000_0000);

            WriteToRegister(Register.REG12_Charger_Control_3, buffer[0]);
        }

        private bool GetOTGModeControl()
        {
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            return (buffer[0] & ChargerControl3OTGModeContolMask) == 1;
        }

        private void SetOTGModeControl(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl3OTGModeContolMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl3OTGModeContolMask : 0b0000_0000);

            WriteToRegister(Register.REG12_Charger_Control_3, buffer[0]);
        }

        private bool GetDisablePFMInOTG()
        {
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            return (buffer[0] & ChargerControl3DisablePFMInOTGMask) == 1;
        }

        private void SetDisablePFMInOTG(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl3DisablePFMInOTGMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl3DisablePFMInOTGMask : 0b0000_0000);

            WriteToRegister(Register.REG12_Charger_Control_3, buffer[0]);
        }

        private bool GetDisablePFMInForwardMode()
        {
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            return (buffer[0] & ChargerControl3DisablePFMInForwardMask) == 1;
        }

        private void SetDisablePFMInForwardMode(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl3DisablePFMInForwardMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl3DisablePFMInForwardMask : 0b0000_0000);

            WriteToRegister(Register.REG12_Charger_Control_3, buffer[0]);
        }

        private bool GetWakeUpDelay()
        {
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            return (buffer[0] & ChargerControl3WakeupDelayMask) == 1;
        }

        private void SetWakeUpDelay(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl3WakeupDelayMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl3WakeupDelayMask : 0b0000_0000);

            WriteToRegister(Register.REG12_Charger_Control_3, buffer[0]);
        }

        private bool GetDisableLDOInPreCharge()
        {
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            return (buffer[0] & ChargerControl3DisableLdoMask) == 1;
        }

        private void SetDisableLDOInPreCharge(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl3DisableLdoMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl3DisableLdoMask : 0b0000_0000);

            WriteToRegister(Register.REG12_Charger_Control_3, buffer[0]);
        }

        private bool GetDisableOOAInOTGMode()
        {
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            return (buffer[0] & ChargerControl3DisableOOAInOTGMask) == 1;
        }

        private void SetDisableOOAInOTGMode(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl3DisableOOAInOTGMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl3DisableOOAInOTGMask : 0b0000_0000);

            WriteToRegister(Register.REG12_Charger_Control_3, buffer[0]);
        }

        private bool GetDisableOOAInForwardMode()
        {
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            return (buffer[0] & ChargerControl3DisableOOAInForwardMask) == 1;
        }

        private void SetDisableOOAInForwardMode(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl3DisableOOAInForwardMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl3DisableOOAInForwardMask : 0b0000_0000);

            WriteToRegister(Register.REG12_Charger_Control_3, buffer[0]);
        }

        #endregion

        #region REG13_Charger_Control_4

        // REG13_Charger_Control_4 Register
        // |      7    |     6     |     5    |     4    |        3       |       2      |        1         |      0      |
        // | EN_ACDRV2 | EN_ACDRV1 | PWM_FREQ | DIS_STAT | DIS_VSYS_SHORT | DIS_VOTG_UVP | FORCE_VINDPM_DET | EN_IBUS_OCP |
        ////

        private bool GetEnableAcDrive2()
        {
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            return (buffer[0] & ChargerControl4EnableACDrive1Mask) == 1;
        }

        private void SetEnableAcDrive2(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl4EnableACDrive1Mask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl4EnableACDrive1Mask : 0b0000_0000);

            WriteToRegister(Register.REG13_Charger_Control_4, buffer[0]);
        }

        private bool GetEnableAcDrive1()
        {
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            return (buffer[0] & ChargerControl4EnableACDrive2Mask) == 1;
        }

        private void SetEnableAcDrive1(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl4EnableACDrive2Mask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl4EnableACDrive2Mask : 0b0000_0000);

            WriteToRegister(Register.REG13_Charger_Control_4, buffer[0]);
        }

        private SwitchingFrequency GetPwmFrequency()
        {
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            return (SwitchingFrequency)(buffer[0] & ChargerControl4PwmFrequencyMask);
        }

        private void SetPwmFrequency(SwitchingFrequency value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl4PwmFrequencyMask);
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG13_Charger_Control_4, buffer[0]);
        }

        private bool GetDisableStatPin()
        {
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            return (buffer[0] & ChargerControl4DisableStatPinMask) == 1;
        }

        private void SetDisableStatPin(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl4DisableStatPinMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl4DisableStatPinMask : 0b0000_0000);

            WriteToRegister(Register.REG13_Charger_Control_4, buffer[0]);
        }

        private bool GetDisableForwardModeVsys()
        {
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            return (buffer[0] & ChargerControl4DisableVSysForwardMask) == 1;
        }

        private void SetDisableForwardModeVsys(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl4DisableVSysForwardMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl4DisableVSysForwardMask : 0b0000_0000);

            WriteToRegister(Register.REG13_Charger_Control_4, buffer[0]);
        }

        private bool GetDisableOtgModeUvp()
        {
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            return (buffer[0] & ChargerControl4DisableVOTGUvpMask) == 1;
        }

        private void SetDisableOtgModeUvp(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl4DisableVOTGUvpMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl4DisableVOTGUvpMask : 0b0000_0000);

            WriteToRegister(Register.REG13_Charger_Control_4, buffer[0]);
        }

        private bool GetForceVinDpmDetection()
        {
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            return (buffer[0] & ChargerControl4ForceVinDMPMask) == 1;
        }

        private void SetForceVinDpmDetection(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl4ForceVinDMPMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl4ForceVinDMPMask : 0b0000_0000);

            WriteToRegister(Register.REG13_Charger_Control_4, buffer[0]);
        }

        private bool GetEnableIbusOcp()
        {
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            return (buffer[0] & ChargerControl4EnableIbusOcpMask) == 1;
        }

        private void SetEnableIbusOcp(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl4EnableIbusOcpMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl4EnableIbusOcpMask : 0b0000_0000);

            WriteToRegister(Register.REG13_Charger_Control_4, buffer[0]);
        }

        #endregion

        #region REG14_Charger_Control_5

        // REG14_Charger_Control_5 Register
        // |       7      |     6    |    5    |     4 3      |      2    |      1     |     0    |
        // | SFET_PRESENT | RESERVED | EN_IBAT | IBAT_REG_1:0 | EN_IINDPM | EN_EXTILIM | EN_BATOC |
        ////

        private bool GetSFetPresent()
        {
            byte[] buffer = ReadFromRegister(Register.REG14_Charger_Control_5, 1);

            return (buffer[0] & ChargerControl5SFETIsPresentMask) == 1;
        }

        private void SetSFetPresent(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG14_Charger_Control_5, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl5SFETIsPresentMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl5SFETIsPresentMask : 0b0000_0000);

            WriteToRegister(Register.REG14_Charger_Control_5, buffer[0]);
        }

        private bool GetEnableBatteryDischargeSensing()
        {
            byte[] buffer = ReadFromRegister(Register.REG14_Charger_Control_5, 1);

            return (buffer[0] & ChargerControl5IBatCurrentSensingMask) == 1;
        }

        private void SetEnableBatteryDischargeSensing(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG14_Charger_Control_5, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl5IBatCurrentSensingMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl5IBatCurrentSensingMask : 0b0000_0000);

            WriteToRegister(Register.REG14_Charger_Control_5, buffer[0]);
        }

        private BatteryDischargeCurrent GetBatteryDischargeCurrent()
        {
            byte[] buffer = ReadFromRegister(Register.REG14_Charger_Control_5, 1);

            return (BatteryDischargeCurrent)(buffer[0] & ChargerControl5BatteryDischargeCurrentMask);
        }

        private void SetBatteryDischargeCurrent(BatteryDischargeCurrent value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG14_Charger_Control_5, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl5BatteryDischargeCurrentMask);
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG14_Charger_Control_5, buffer[0]);
        }

        private bool GetEnableInputDpm()
        {
            byte[] buffer = ReadFromRegister(Register.REG14_Charger_Control_5, 1);

            return (buffer[0] & ChargerControl5EnableIINDPMMask) == 1;
        }

        private void SetEnableInputDpm(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG14_Charger_Control_5, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl5EnableIINDPMMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl5EnableIINDPMMask : 0b0000_0000);

            WriteToRegister(Register.REG14_Charger_Control_5, buffer[0]);
        }

        private bool GetEnableExternalILIM()
        {
            byte[] buffer = ReadFromRegister(Register.REG14_Charger_Control_5, 1);

            return (buffer[0] & ChargerControl5EnableExtILIMMask) == 1;
        }

        private void SetEnableExternalILIM(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG14_Charger_Control_5, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl5EnableExtILIMMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl5EnableExtILIMMask : 0b0000_0000);

            WriteToRegister(Register.REG14_Charger_Control_5, buffer[0]);
        }

        private bool GetEnableBatteryOCP()
        {
            byte[] buffer = ReadFromRegister(Register.REG14_Charger_Control_5, 1);

            return (buffer[0] & ChargerControl5EnableDischargeOCPMask) == 1;
        }

        private void SetEnableBatteryOCP(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG14_Charger_Control_5, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl5EnableDischargeOCPMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl5EnableDischargeOCPMask : 0b0000_0000);

            WriteToRegister(Register.REG14_Charger_Control_5, buffer[0]);
        }

        #endregion

        #region REG1E_Charger_Status_3

        // REG1E_Charger_Status_3 Register
        // |      7     |      6     |       5       |     4     |       3      |        2        |        1        |    0     |
        // | ACRB2_STAT | ACRB1_STAT | ADC_DONE_STAT | VSYS_STAT | CHG_TMR_STAT | TRICHG_TMR_STAT | PRECHG_TMR_STAT | RESERVED |
        ////

        private bool GetAcrb2Status()
        {
            byte[] buffer = ReadFromRegister(Register.REG1E_Charger_Status_3, 1);

            return (buffer[0] & ChargerStatus3ACRB2StatusMask) == 1;
        }

        private bool GetAcrb1Status()
        {
            byte[] buffer = ReadFromRegister(Register.REG1E_Charger_Status_3, 1);

            return (buffer[0] & ChargerStatus3ACRB1StatusMask) == 1;
        }

        private bool GetAdcDoneStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG1E_Charger_Status_3, 1);

            return (buffer[0] & ChargerStatus3ADCConversionStatusMask) == 1;
        }

        private bool GetVsysStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG1E_Charger_Status_3, 1);

            return (buffer[0] & ChargerStatus3VSysSatusMask) == 1;
        }

        private bool GetFastChargeTimerStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG1E_Charger_Status_3, 1);

            return (buffer[0] & ChargerStatus3FastChargerTimerStatusMask) == 1;
        }

        private bool GetTrickleChargeTimerStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG1E_Charger_Status_3, 1);

            return (buffer[0] & ChargerStatus3TrickleChargeStatusMask) == 1;
        }

        private bool GetPreChargeTimerStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG1E_Charger_Status_3, 1);

            return (buffer[0] & ChargerStatus3PrechargeStatusMask) == 1;
        }

        #endregion

        #region REG1F_Charger_Status_4

        // REG1F_Charger_Status_4 Register
        // |  7  6 5  |         4        |       3      |       2      |       1      |       0     |
        // | RESERVED | VBATOTG_LOW_STAT | TS_COLD_STAT | TS_COOL_STAT | TS_WARM_STAT | TS_HOT_STAT |
        ////

        private bool GetVBatOTGLowStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG1F_Charger_Status_4, 1);

            return (buffer[0] & ChargerStatus4VbatOTGLowStatusMask) == 1;
        }

        private bool GetTsColdStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG1F_Charger_Status_4, 1);

            return (buffer[0] & ChargerStatus4TsColdStatusMask) == 1;
        }

        private bool GetTsCoolStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG1F_Charger_Status_4, 1);

            return (buffer[0] & ChargerStatus4TsCoolStatusMask) == 1;
        }

        private bool GetTsWarmStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG1F_Charger_Status_4, 1);

            return (buffer[0] & ChargerStatus4TsWarmStatusMask) == 1;
        }

        private bool GetTsHotStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG1F_Charger_Status_4, 1);

            return (buffer[0] & ChargerStatus4TsHotStatusMask) == 1;
        }

        #endregion

        #region REG20_FAULT_Status_0

        // REG20_FAULT_Status_0 Register
        // |       7       |       6       |       5       |       4       |       3       |       2       |       1       |       0       | 
        // | IBAT_REG_STAT | VBUS_OVP_STAT | VBAT_OVP_STAT | IBUS_OCP_STAT | IBAT_OCP_STAT | CONV_OCP_STAT | VAC2_OVP_STAT | VAC1_OVP_STAT |
        ////

        private bool GetIBatRegulationStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG20_FAULT_Status_0, 1);

            return (buffer[0] & FaultStatus0IBatRegulationStatusMask) == 1;
        }

        private bool GetVbusOverVoltageStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG20_FAULT_Status_0, 1);

            return (buffer[0] & FaultStatus0VBusOverVoltageStatusMask) == 1;
        }

        private bool GetVBatOverVoltageStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG20_FAULT_Status_0, 1);

            return (buffer[0] & FaultStatus0VBatOvervoltageStatusMask) == 1;
        }

        private bool GetIBusOverCurrentStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG20_FAULT_Status_0, 1);

            return (buffer[0] & FaultStatus0IBusOverCurrentStatusMask) == 1;
        }

        private bool GetIBatOverCurrentStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG20_FAULT_Status_0, 1);

            return (buffer[0] & FaultStatus0IBatOverCurrentStatusMask) == 1;
        }

        private bool GetConverterOverCurrentStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG20_FAULT_Status_0, 1);

            return (buffer[0] & FaultStatus0ConverterOverCurrentStatusMask) == 1;
        }

        private bool GetVac2OverVoltageStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG20_FAULT_Status_0, 1);

            return (buffer[0] & FaultStatus0VAc2OverVoltageStatusMask) == 1;
        }

        private bool GetVac1OverVoltageStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG20_FAULT_Status_0, 1);

            return (buffer[0] & FaultStatus0VAc1OverVoltageStatusMask) == 1;
        }

        #endregion

        #region REG21_FAULT_Status_1

        // REG21_FAULT_Status_1 Register
        // |        7        |       6       |       5      |      4       |    3     |     2      |   1 0    | 
        // | VSYS_SHORT_STAT | VSYS_OVP_STAT | OTG_OVP_STAT | OTG_UVP_STAT | RESERVED | TSHUT_STAT | RESERVED |
        ////

        private bool GetVsysShortCircuitStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG21_FAULT_Status_1, 1);

            return (buffer[0] & FaultStatus1VsysShortCircuitStatusMask) == 1;
        }

        private bool GetVsysOverVoltageStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG21_FAULT_Status_1, 1);

            return (buffer[0] & FaultStatus1VsysOverVoltageStatusMask) == 1;
        }

        private bool GetOTGOverVoltageStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG21_FAULT_Status_1, 1);

            return (buffer[0] & FaultStatus1OTGOverVoltageStatusMask) == 1;
        }

        private bool GetOTGUnderVoltageStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG21_FAULT_Status_1, 1);

            return (buffer[0] & FaultStatus1OTGUnderVoltageStatusMask) == 1;
        }

        private bool GetTemperatureShutdownStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG21_FAULT_Status_1, 1);

            return (buffer[0] & FaultStatus1TemperatureShutdowntatusMask) == 1;
        }

        #endregion

        #region REG22_Charger_Flag_0

        // REG22_Charger_Flag_0 Register
        // |      7      |      6      |    5     |      4        |    3     |         2         |         1         |          0         |
        // | IINDPM_FLAG | VINDPM_FLAG | WD_FLAG  | POORSRC_FLAG  | PG_FLAG  | AC2_PRESENT_FLAG  | AC1_PRESENT_FLAG  | VBUS_PRESENT_FLAG  |
        ////

        private bool GetIIndpmFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG22_Charger_Flag_0, 1);

            return (buffer[0] & ChargerFlag0IINDPMFlagMask) == 1;
        }

        private bool GetVIndpmFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG22_Charger_Flag_0, 1);

            return (buffer[0] & ChargerFlag0VINDPMFlagMask) == 1;
        }

        private bool GetI2cWatchdogFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG22_Charger_Flag_0, 1);

            return (buffer[0] & ChargerFlag0I2cWatchdogFlagMask) == 1;
        }

        private bool GetPoorSourceFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG22_Charger_Flag_0, 1);

            return (buffer[0] & ChargerFlag0PoorSourceFlagMask) == 1;
        }

        private bool GetPowerGoodFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG22_Charger_Flag_0, 1);

            return (buffer[0] & ChargerFlag0PowerGoodFlagMask) == 1;
        }

        private bool GetAc2PresentFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG22_Charger_Flag_0, 1);

            return (buffer[0] & ChargerFlag0Vac2PresentFlagMask) == 1;
        }

        private bool GetAc1PresentFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG22_Charger_Flag_0, 1);

            return (buffer[0] & ChargerFlag0Vac1PresentFlagMask) == 1;
        }

        private bool GetVbusPresentFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG22_Charger_Flag_0, 1);

            return (buffer[0] & ChargerFlag0VbusPresentFlagMask) == 1;
        }

        #endregion

        #region REG23_Charger_Flag_1

        // REG23_Charger_Flag_1 Register
        // |     7    |     6    |    5     |      4     |    3     |     2     |         1        |        0        |
        // | CHG_FLAG | ICO_FLAG | RESERVED | VBUS_FLAG  | RESERVED | TREG_FLAG | VBAT_PRESENT_FLAG| BC1.2_DONE_FLAG |
        ////

        private bool GetChargeStatusFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG23_Charger_Flag_1, 1);

            return (buffer[0] & ChargerFlag1ChargeFlagMask) == 1;
        }

        private bool GetIcoStatusFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG23_Charger_Flag_1, 1);

            return (buffer[0] & ChargerFlag1IcoFlagMask) == 1;
        }

        private bool GetVbusStatusFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG23_Charger_Flag_1, 1);

            return (buffer[0] & ChargerFlag1VbusFlagMask) == 1;
        }

        private bool GetThermalRegulationFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG23_Charger_Flag_1, 1);

            return (buffer[0] & ChargerFlag1ThermallagMask) == 1;
        }

        private bool GetVbatPresentFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG23_Charger_Flag_1, 1);

            return (buffer[0] & ChargerFlag1VbatPresentFlagMask) == 1;
        }

        private bool GetBc12DetectionFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG23_Charger_Flag_1, 1);

            return (buffer[0] & ChargerFlag1Bc12FlagMask) == 1;
        }

        #endregion

        #region REG24_Charger_Flag_2

        // REG24_Charger_Flag_2 Register
        // |    7     |        6       |       5       |      4    |      3       |         2       |        1        |        0        |
        // | RESERVED | DPDM_DONE_FLAG | ADC_DONE_FLAG | VSYS_FLAG | CHG_TMR_FLAG | TRICHG_TMR_FLAG | PRECHG_TMR_FLAG | TOPOFF_TMR_FLAG |
        ////

        private bool GetDpdmDoneFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG24_Charger_Flag_2, 1);

            return (buffer[0] & ChargerFlag2DPDMDoneFlagMask) == 1;
        }

        private bool GetAdcDoneFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG24_Charger_Flag_2, 1);

            return (buffer[0] & ChargerFlag2AdcConversionFlagMask) == 1;
        }

        private bool GetVsysFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG24_Charger_Flag_2, 1);

            return (buffer[0] & ChargerFlag2VsysFlagMask) == 1;
        }

        private bool GetFastChargeTimerFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG24_Charger_Flag_2, 1);

            return (buffer[0] & ChargerFlag2FastChargeTimerFlagMask) == 1;
        }

        private bool GetTrickleChargeTimerFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG24_Charger_Flag_2, 1);

            return (buffer[0] & ChargerFlag2TrickleChargeTimerFlagMask) == 1;
        }

        private bool GetPreChargeTimerFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG24_Charger_Flag_2, 1);

            return (buffer[0] & ChargerFlag2PrechargeTimerFlagMask) == 1;
        }

        private bool GetTopOffTimerFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG24_Charger_Flag_2, 1);

            return (buffer[0] & ChargerFlag2TopOffTimerFlagMask) == 1;
        }

        #endregion

        #region REG25_Charger_Flag_3

        // REG25_Charger_Flag_3 Register
        // |  7 6 5   |        4         |      3       |       2      |       1      |      0      |
        // | RESERVED | VBATOTG_LOW_FLAG | TS_COLD_FLAG | TS_COOL_FLAG | TS_WARM_FLAG | TS_HOT_FLAG |
        ////

        private bool GetVBatOTGLowFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG25_Charger_Flag_3, 1);

            return (buffer[0] & ChargerFlag3VbatOTGLowFlagMask) == 1;
        }

        private bool GetTsColdFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG25_Charger_Flag_3, 1);

            return (buffer[0] & ChargerFlag3TsColdFlagMask) == 1;
        }

        private bool GetTsCoolFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG25_Charger_Flag_3, 1);

            return (buffer[0] & ChargerFlag3TsCoolFlagMask) == 1;
        }

        private bool GetTsWarmFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG25_Charger_Flag_3, 1);

            return (buffer[0] & ChargerFlag3TsWarmFlagMask) == 1;
        }

        private bool GetTsHotFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG25_Charger_Flag_3, 1);

            return (buffer[0] & ChargerFlag3TsHotFlagMask) == 1;
        }

        #endregion

        #region REG2E_ADC_Control

        // REG08_Precharge_Control Register
        // |    7   |    6     |       5 4      |    3    |      2       |    1 0   |
        // | ADC_EN | ADC_RATE | ADC_SAMPLE_1:0 | ADC_AVG | ADC_AVG_INIT | RESERVED |
        ////

        private bool GetAdcEnable()
        {
            byte[] buffer = ReadFromRegister(Register.REG2E_ADC_Control, 1);

            return (buffer[0] & AdcControlEnableMask) != 0;
        }

        private void SetAdcEnable(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG2E_ADC_Control, 1);

            // clear bit
            buffer[0] = (byte)(buffer[0] & ~AdcControlEnableMask);

            // set bit if needed
            if (value)
            {
                buffer[0] |= AdcControlEnableMask;
            }

            WriteToRegister(Register.REG2E_ADC_Control, buffer[0]);
        }

        private AdcConversioRate GetAdcConversionRate()
        {
            byte[] buffer = ReadFromRegister(Register.REG2E_ADC_Control, 1);

            return (AdcConversioRate)(buffer[0] & AdcControlConversionRateMask);
        }

        private void SetAdcConversionRate(AdcConversioRate value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG2E_ADC_Control, 1);

            // clear bit
            buffer[0] = (byte)(buffer[0] & ~AdcControlConversionRateMask);
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG2E_ADC_Control, buffer[0]);
        }

        private AdcResolution GetAdcResolution()
        {
            byte[] buffer = ReadFromRegister(Register.REG2E_ADC_Control, 1);

            return (AdcResolution)(buffer[0] & AdcControlResolutionMask);
        }

        private void SetAdcResolution(AdcResolution value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG2E_ADC_Control, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~AdcControlResolutionMask);
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG2E_ADC_Control, buffer[0]);
        }

        private AdcAveraging GetAdcAveraging()
        {
            byte[] buffer = ReadFromRegister(Register.REG2E_ADC_Control, 1);

            return (AdcAveraging)(buffer[0] & AdcControlAverageControlMask);
        }

        private void SetAdcAveraging(AdcAveraging value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG2E_ADC_Control, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~AdcControlAverageControlMask);
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG2E_ADC_Control, buffer[0]);
        }

        private AdcInitialAverage GetAdcInitialAverage()
        {
            byte[] buffer = ReadFromRegister(Register.REG2E_ADC_Control, 1);

            return (AdcInitialAverage)(buffer[0] & AdcControlInitialAverageMask);
        }

        private void SetAdcInitialAverage(AdcInitialAverage value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG2E_ADC_Control, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~AdcControlInitialAverageMask);
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG2E_ADC_Control, buffer[0]);
        }

        private ThermalRegulationThreshold GetThermalRegulationThreshold()
        {
            byte[] buffer = ReadFromRegister(Register.REG16_Temperature_Control, 1);

            return (ThermalRegulationThreshold)(buffer[0] & ThermalRegulationThresholdMask);
        }

        private void SetThermalRegulationThreshold(ThermalRegulationThreshold value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG16_Temperature_Control, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ThermalRegulationThresholdMask);
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG16_Temperature_Control, buffer[0]);
        }

        private ThermalShutdownThreshold GetThermalShutdownThreshold()
        {
            byte[] buffer = ReadFromRegister(Register.REG16_Temperature_Control, 1);

            return (ThermalShutdownThreshold)((buffer[0] & ThermalShutdownThresholdMask) >> 2);
        }

        private void SetThermalShutdownThreshold(ThermalShutdownThreshold value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG16_Temperature_Control, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ThermalShutdownThresholdMask);
            buffer[0] |= (byte)((byte)value << 2);

            WriteToRegister(Register.REG16_Temperature_Control, buffer[0]);
        }

        #endregion

        #region REGxx_NN_ADCs

        // REG35_VBUS_ADC Register
        // REG37_VAC1_ADC Register
        // REG39_VAC2_ADC Register
        // REG3B_VBAT_ADC Register
        // REG3D_VSYS_ADC Register
        // | 15 14 13 12 11 10 9 8 | 7 6 5 4 3 2 1 0 |
        // |                Vnnn_ADC_15:0            |
        ////

        private ElectricPotentialDc GetAdcVoltage(Register register)
        {
            byte[] buffer = ReadFromRegister(register, 2);

            var vbus = (buffer[0] << 8) | buffer[1];

            return new ElectricPotentialDc(
                vbus * VbusAdcStep,
                UnitsNet.Units.ElectricPotentialDcUnit.MillivoltDc);
        }

        #endregion

        #region REG41_TDIE_ADC

        // REG41_TDIE_ADC Register
        // | 15 14 13 12 11 10 9 8 | 7 6 5 4 3 2 1 0 |
        // |                TDIE_ADC_15:0            |
        ////

        private Temperature GetDieTemperature()
        {
            byte[] buffer = ReadFromRegister(Register.REG41_TDIE_ADC, 2);

            var ascReading = (buffer[0] << 8) | buffer[1];

            return new Temperature(
                ascReading * TdieStemp,
                UnitsNet.Units.TemperatureUnit.DegreeCelsius);
        }

        #endregion

        #region Helper Methods to Read and Write to Registers

        private void WriteToRegister(Register register, byte content)
        {
            WriteToRegister(register, new byte[] { content });
        }

        private void WriteToRegister(Register register, byte[] contents)
        {
            byte[] writeBuffer = new byte[contents.Length + 1];
            writeBuffer[0] = (byte)register;

            contents.CopyTo(writeBuffer, 1);

            I2cTransferResult result = _i2cDevice.Write(writeBuffer);

            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new InvalidOperationException();
            }
        }

        private byte[] ReadFromRegister(Register register, int readByteCount)
        {
            SpanByte writeBuff = new byte[1] { (byte)register };

            byte[] readBuffer = new byte[readByteCount];

            I2cTransferResult result = _i2cDevice.WriteRead(writeBuff, readBuffer);

            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new InvalidOperationException();
            }

            return readBuffer;
        }

        #endregion
    }
}
