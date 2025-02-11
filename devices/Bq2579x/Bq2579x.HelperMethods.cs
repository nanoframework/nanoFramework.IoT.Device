////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

using System;
using System.Device.I2c;
using UnitsNet;

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// Buck-Boost Battery Charger Bq2579x.
    /// </summary>
    internal class Bq2579xHelpers
    {
        private const byte PrechargeCurrentLimitMask = 0b00111111;
        private const byte TerminationCurrentMask = 0b0001_1111;
        private const byte ChargerStatus0PowerGoodStatusMask = 0b0000_1000;
        private const byte ChargerStatus1ChargeStatusMask = 0b1110_0000;
        private const byte ChargerStatus1VbusStatusMask = 0b0001_1110;
        private const byte ChargerStatus1Bc12DetectionMask = 0b0000_0001;
        private const byte ChargerControl1OvpThresholdMask = 0b0011_0000;
        private const byte ChargerControl1I2cResetyMask = 0b0000_1000;
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
        private const float TcAdcStep = 0.0976563f;

        private readonly I2cDevice _i2cDevice;

        public Bq2579xHelpers(I2cDevice device)
        {
            _i2cDevice = device;
        }

        #region REG1B_Charger_Status_0

        // REG1B_Charger_Status_0 Register
        // | 7 6 5 4 3 2 1 0 |
        // | IINDPM_STAT | VINDPM_STAT | WD_STAT | RESERVED | PG_STAT | AC2_PRESENT_STAT | AC1_PRESENT_STAT | VBUS_PRESENT_STAT |
        ////

        public ChargerStatus0 GetChargerStatus0()
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

        public ChargeStatus GetChargeStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG1C_Charger_Status_1, 1);

            return (ChargeStatus)(buffer[0] & ChargerStatus1ChargeStatusMask);
        }

        public VbusStatus GetVbusStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG1C_Charger_Status_1, 1);

            return (VbusStatus)(buffer[0] & ChargerStatus1VbusStatusMask);
        }

        public bool GetBc12Detection()
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

        public ChargerStatus2 GetChargerStatus2()
        {
            byte[] buffer = ReadFromRegister(Register.REG1D_Charger_Status_2, 1);

            return (ChargerStatus2)buffer[0];
        }

        public IcoStatus GetIcoStatus()
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

        public ChargeVoltage GetChargeVoltage()
        {
            byte[] buffer = ReadFromRegister(Register.REG17_NTC_Control_0, 1);

            return (ChargeVoltage)(buffer[0] & NtcControl0ChargerVoltageMask);
        }

        public void SetChargeVoltage(ChargeVoltage value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG17_NTC_Control_0, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~NtcControl0ChargerVoltageMask);

            // set value
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG17_NTC_Control_0, buffer[0]);
        }

        public ChargeCurrent GetChargeCurrentHighTempRange()
        {
            byte[] buffer = ReadFromRegister(Register.REG17_NTC_Control_0, 1);

            // need to shift 3 positions to the right to get the value
            return (ChargeCurrent)((buffer[0] & NtcControl0ChargeCurrentMask) >> 3);
        }

        public void SetChargeCurrentHighTempRange(ChargeCurrent value)
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

        public ChargeCurrent GetChargeCurrentLowTempRange()
        {
            byte[] buffer = ReadFromRegister(Register.REG17_NTC_Control_0, 1);

            // need to shift 1 position to the right to get the value
            return (ChargeCurrent)((buffer[0] & NtcControl0ChargeCurrentMask) >> 1);
        }

        public void SetChargeCurrentLowTempRange(ChargeCurrent value)
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

        public CompVoltageRisingThreshold GetVt2ComparatorVoltage()
        {
            byte[] buffer = ReadFromRegister(Register.REG18_NTC_Control_1, 1);

            return (CompVoltageRisingThreshold)(buffer[0] & NtcControl1Vt2ComparatorVoltageMask);
        }

        public void SetVt2ComparatorVoltage(CompVoltageRisingThreshold value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG18_NTC_Control_1, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~NtcControl1Vt2ComparatorVoltageMask);

            // set value
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG18_NTC_Control_1, buffer[0]);
        }

        public CompVoltageFallingThreshold GetVt3ComparatorVoltage()
        {
            byte[] buffer = ReadFromRegister(Register.REG18_NTC_Control_1, 1);

            return (CompVoltageFallingThreshold)(buffer[0] & NtcControl1Vt3ComparatorVoltageMask);
        }

        public void SetVt3ComparatorVoltage(CompVoltageFallingThreshold value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG18_NTC_Control_1, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~NtcControl1Vt3ComparatorVoltageMask);

            // set value
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG18_NTC_Control_1, buffer[0]);
        }

        public OtgHotTempThreshold GetOtgHotTempThreshold()
        {
            byte[] buffer = ReadFromRegister(Register.REG18_NTC_Control_1, 1);

            return (OtgHotTempThreshold)(buffer[0] & NtcControl1OtgHotTemperatureMask);
        }

        public void SetOtgHotTempThreshold(OtgHotTempThreshold value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG18_NTC_Control_1, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~NtcControl1OtgHotTemperatureMask);

            // set value
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG18_NTC_Control_1, buffer[0]);
        }

        public OtgColdTempThreshold GetOtgColdTempThreshold()
        {
            byte[] buffer = ReadFromRegister(Register.REG18_NTC_Control_1, 1);

            return (OtgColdTempThreshold)(buffer[0] & NtcControl1OtgColdTemperatureMask);
        }

        public void SetOtgColdTempThreshold(OtgColdTempThreshold value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG18_NTC_Control_1, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~NtcControl1OtgColdTemperatureMask);

            // set value
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG18_NTC_Control_1, buffer[0]);
        }

        public bool GetIgnoreTempSensor()
        {
            byte[] buffer = ReadFromRegister(Register.REG18_NTC_Control_1, 1);

            return (buffer[0] & NtcControl1IgnoreTSMask) == 1;
        }

        public void SetIgnoreTempSensor(bool value)
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

        public ElectricPotentialDc GetMinimalSystemVoltage()
        {
            byte[] buffer = ReadFromRegister(Register.REG00_Minimal_System_Voltage, 1);

            return new ElectricPotentialDc(
                (buffer[0] * StepMinimalSystemVoltage) + FixedOffsetMinimalSystemVoltage,
                UnitsNet.Units.ElectricPotentialDcUnit.MillivoltDc);
        }

        public void SetMinimalSystemVoltage(ElectricPotentialDc value)
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

        public ElectricPotentialDc GetChargeVoltageLimit()
        {
            byte[] buffer = ReadFromRegister(Register.REG01_Charge_Voltage_Limit, 2);

            var vbus = (buffer[0] << 8) | buffer[1];

            return new ElectricPotentialDc(
                vbus * ChargeVoltageStep,
                UnitsNet.Units.ElectricPotentialDcUnit.MillivoltDc);
        }

        public void SetChargeVoltageLimit(ElectricPotentialDc value)
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

        public ElectricCurrent GetChargeCurrentLimit()
        {
            byte[] buffer = ReadFromRegister(Register.REG03_Charge_Current_Limit, 2);

            var vbus = (buffer[0] << 8) | buffer[1];

            return new ElectricCurrent(
                vbus * ChargeInputCurrentStep,
                UnitsNet.Units.ElectricCurrentUnit.Milliampere);
        }

        public void SetChargeCurrentLimit(ElectricCurrent value)
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

        public ElectricPotentialDc GetInputVoltageLimit()
        {
            byte[] buffer = ReadFromRegister(Register.REG05_Input_Voltage_Limit, 1);

            return new ElectricPotentialDc(
                buffer[0] * InputVoltageStep,
                UnitsNet.Units.ElectricPotentialDcUnit.MillivoltDc);
        }

        public void SetInputVoltageLimit(ElectricPotentialDc value)
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

        public ElectricCurrent GetInputCurrentLimit()
        {
            byte[] buffer = ReadFromRegister(Register.REG06_Input_Current_Limit, 2);

            var vbus = (buffer[0] << 8) | buffer[1];

            return new ElectricCurrent(
                vbus * ChargeInputCurrentStep,
                UnitsNet.Units.ElectricCurrentUnit.Milliampere);
        }

        public void SetInputCurrentLimit(ElectricCurrent value)
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

        public ThresholdFastCharge GetThresholdFastCharge()
        {
            byte[] buffer = ReadFromRegister(Register.REG08_Precharge_Control, 1);

            return (ThresholdFastCharge)(buffer[0] >> 6);
        }

        public void SetThresholdFastCharge(ThresholdFastCharge value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG08_Precharge_Control, 1);

            // process value to replace VBAT_LOWV_1:0
            buffer[0] = (byte)(((byte)value << 6) | (byte)(buffer[0] & 0b0011_1111));
        }

        public ElectricCurrent GetPrechargeCurrentLimit()
        {
            byte[] buffer = ReadFromRegister(Register.REG08_Precharge_Control, 1);

            return new ElectricCurrent(
                buffer[0] & PrechargeCurrentLimitMask * StepPrechargeCurrentLimit,
                UnitsNet.Units.ElectricCurrentUnit.Milliampere);
        }

        public void SetPrechargeCurrentLimit(ElectricCurrent value)
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

        public ElectricCurrent GetTerminationCurrent()
        {
            byte[] buffer = ReadFromRegister(Register.REG09_Charge_Termination_Current, 1);

            return new ElectricCurrent(
                buffer[0] & TerminationCurrentMask * TerminationCurrentMinValue,
                UnitsNet.Units.ElectricCurrentUnit.Milliampere);
        }

        public void SetTerminationCurrent(ElectricCurrent value)
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

        public TopOffTimerControl GetTopOffTimerControl()
        {
            byte[] buffer = ReadFromRegister(Register.REG0E_Timer_Control, 1);

            return (TopOffTimerControl)(buffer[0] & TimerControlTopOffMask);
        }

        public void SetTopOffTimerControl(TopOffTimerControl value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG0E_Timer_Control, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~TimerControlTopOffMask);
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG0E_Timer_Control, buffer[0]);
        }

        public bool GetEnableTrickleChargeTimer()
        {
            byte[] buffer = ReadFromRegister(Register.REG0E_Timer_Control, 1);

            return (buffer[0] & TimerControlEnableTrickleChargeMask) == 1;
        }

        public void SetEnableTrickleChargeTimer(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG0E_Timer_Control, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~TimerControlEnableTrickleChargeMask);

            // set value
            buffer[0] |= (byte)(value ? TimerControlEnableTrickleChargeMask : 0b0000_0000);

            WriteToRegister(Register.REG0E_Timer_Control, buffer[0]);
        }

        public bool GetEnablePrechargeTimer()
        {
            byte[] buffer = ReadFromRegister(Register.REG0E_Timer_Control, 1);

            return (buffer[0] & TimerControlEnablePrechargeTimerMask) == 1;
        }

        public void SetEnablePrechargeTimer(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG0E_Timer_Control, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~TimerControlEnablePrechargeTimerMask);

            // set value
            buffer[0] |= (byte)(value ? TimerControlEnablePrechargeTimerMask : 0b0000_0000);

            WriteToRegister(Register.REG0E_Timer_Control, buffer[0]);
        }

        public bool GetEnableFastChargeTimer()
        {
            byte[] buffer = ReadFromRegister(Register.REG0E_Timer_Control, 1);

            return (buffer[0] & TimerControlEnableFastChargeMask) == 1;
        }

        public void SetEnableFastChargeTimer(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG0E_Timer_Control, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~TimerControlEnableFastChargeMask);

            // set value
            buffer[0] |= (byte)(value ? TimerControlEnableFastChargeMask : 0b0000_0000);

            WriteToRegister(Register.REG0E_Timer_Control, buffer[0]);
        }

        public FastChargeTimerSetting GetFastChargeTimer()
        {
            byte[] buffer = ReadFromRegister(Register.REG0E_Timer_Control, 1);

            return (FastChargeTimerSetting)(buffer[0] & TimerControlFastChargeTimerMask);
        }

        public void SetFastChargeTimer(FastChargeTimerSetting value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG0E_Timer_Control, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~TimerControlFastChargeTimerMask);
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG0E_Timer_Control, buffer[0]);
        }

        public bool GetEnable2xTimerSlowdown()
        {
            byte[] buffer = ReadFromRegister(Register.REG0E_Timer_Control, 1);

            return (buffer[0] & TimerControl2xTimerMask) == 1;
        }

        public void SetEnable2xTimerSlowdown(bool value)
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

        public bool GetAutoBatteryDischarge()
        {
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            return (buffer[0] & ChargerControl0AutoBatteryDischargeMask) == 1;
        }

        public void SetAutoBatteryDischarge(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl0AutoBatteryDischargeMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl0AutoBatteryDischargeMask : 0b0000_0000);

            WriteToRegister(Register.REG0F_Charger_Control_0, buffer[0]);
        }

        public bool GetForceBatteryDischarge()
        {
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            return (buffer[0] & ChargerControl0ForceBatteryDischargeMask) == 1;
        }

        public void SetForceBatteryDischarge(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl0ForceBatteryDischargeMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl0ForceBatteryDischargeMask : 0b0000_0000);

            WriteToRegister(Register.REG0F_Charger_Control_0, buffer[0]);
        }

        public bool GetEnableCharger()
        {
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            return (buffer[0] & ChargerControl0ChargerEnableMask) == 1;
        }

        public void SetEnableCharger(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl0ChargerEnableMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl0ChargerEnableMask : 0b0000_0000);

            WriteToRegister(Register.REG0F_Charger_Control_0, buffer[0]);
        }

        public bool GetEnableInputCurrentOptimizer()
        {
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            return (buffer[0] & ChargerControl0InputCurrentOptimizerMask) == 1;
        }

        public void SetEnableInputCurrentOptimizer(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl0InputCurrentOptimizerMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl0InputCurrentOptimizerMask : 0b0000_0000);

            WriteToRegister(Register.REG0F_Charger_Control_0, buffer[0]);
        }

        public bool GetForceStartInputCurrentOptimizer()
        {
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            return (buffer[0] & ChargerControl0ForceStartInputMask) == 1;
        }

        public void SetForceStartInputCurrentOptimizer(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl0ForceStartInputMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl0ForceStartInputMask : 0b0000_0000);

            WriteToRegister(Register.REG0F_Charger_Control_0, buffer[0]);
        }

        public bool GetEnableHighImpedanceMode()
        {
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            return (buffer[0] & ChargerControl0EnableHizMask) == 1;
        }

        public void SetEnableHighImpedanceMode(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl0EnableHizMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl0EnableHizMask : 0b0000_0000);

            WriteToRegister(Register.REG0F_Charger_Control_0, buffer[0]);
        }

        public bool GetEnableTermination()
        {
            byte[] buffer = ReadFromRegister(Register.REG0F_Charger_Control_0, 1);

            return (buffer[0] & ChargerControl0EnableTerminationMask) == 1;
        }

        public void SetEnableTermination(bool value)
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

        // REG10_Charger_Control_1 Register
        // |   7 6    |     5 4     |    3   |     2 1 0    |
        // | RESERVED | VAC_OVP_1:0 | WD_RST | WATCHDOG_2:0 |
        ////

        public VacOpvThreshold GetVacOpvThreshold()
        {
            byte[] buffer = ReadFromRegister(Register.REG10_Charger_Control_1, 1);

            return (VacOpvThreshold)(buffer[0] & ChargerControl1OvpThresholdMask);
        }

        public void SetVacOpvThreshold(VacOpvThreshold value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG10_Charger_Control_1, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl1OvpThresholdMask);

            // set value
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG10_Charger_Control_1, buffer[0]);
        }

        public bool GetI2CWatchdogReset()
        {
            byte[] buffer = ReadFromRegister(Register.REG10_Charger_Control_1, 1);

            return (buffer[0] & ChargerControl1I2cResetyMask) == 1;
        }

        public void SetI2CWatchdogReset(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG10_Charger_Control_1, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl1I2cResetyMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl1I2cResetyMask : 0b0000_0000);

            WriteToRegister(Register.REG10_Charger_Control_1, buffer[0]);
        }

        public WatchdogSetting GetWatchdogTimerSetting()
        {
            byte[] buffer = ReadFromRegister(Register.REG10_Charger_Control_1, 1);

            return (WatchdogSetting)(buffer[0] & ChargerControl1WatchdogMask);
        }

        public void SetWatchdogTimerSetting(WatchdogSetting value)
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

        public bool GetForceDDDetection()
        {
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            return (buffer[0] & ChargerControl2ForceDDDetectionMask) == 1;
        }

        public void SetForceDDDetection(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl2ForceDDDetectionMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl2ForceDDDetectionMask : 0b0000_0000);

            WriteToRegister(Register.REG11_Charger_Control_2, buffer[0]);
        }

        public bool GetAutoDDDetection()
        {
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            return (buffer[0] & ChargerControl2AutomaticDDDetectionMask) == 1;
        }

        public void SetAutoDDDetection(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl2AutomaticDDDetectionMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl2AutomaticDDDetectionMask : 0b0000_0000);

            WriteToRegister(Register.REG11_Charger_Control_2, buffer[0]);
        }

        public bool GetEnable12VInput()
        {
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            return (buffer[0] & ChargerControl2Enable12VMask) == 1;
        }

        public void SetEnable12VInput(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl2Enable12VMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl2Enable12VMask : 0b0000_0000);

            WriteToRegister(Register.REG11_Charger_Control_2, buffer[0]);
        }

        public bool GetEnable9VInput()
        {
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            return (buffer[0] & ChargerControl2Enable9VMask) == 1;
        }

        public void SetEnable9VInput(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl2Enable9VMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl2Enable9VMask : 0b0000_0000);

            WriteToRegister(Register.REG11_Charger_Control_2, buffer[0]);
        }

        public bool GetEnableHighVoltageDdcp()
        {
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            return (buffer[0] & ChargerControl2HighVoltageDcpMask) == 1;
        }

        public void SetEnableHighVoltageDdcp(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl2HighVoltageDcpMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl2HighVoltageDcpMask : 0b0000_0000);

            WriteToRegister(Register.REG11_Charger_Control_2, buffer[0]);
        }

        public SFETControl GetSdrvControl()
        {
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            return (SFETControl)(buffer[0] & ChargerControl2SFETControlMask);
        }

        public void SetSdrvControl(SFETControl value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl2SFETControlMask);
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG11_Charger_Control_2, buffer[0]);
        }

        public bool GetSdrvDelayEnable()
        {
            byte[] buffer = ReadFromRegister(Register.REG11_Charger_Control_2, 1);

            return (buffer[0] & ChargerControl2SdrvDelayMask) == 1;
        }

        public void SetSdrvDelayEnable(bool value)
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

        public bool GetDisableAcDrive()
        {
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            return (buffer[0] & ChargerControl3DisableAcDriveMask) == 1;
        }

        public void SetDisableAcDrive(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl3DisableAcDriveMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl3DisableAcDriveMask : 0b0000_0000);

            WriteToRegister(Register.REG12_Charger_Control_3, buffer[0]);
        }

        public bool GetOTGModeControl()
        {
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            return (buffer[0] & ChargerControl3OTGModeContolMask) == 1;
        }

        public void SetOTGModeControl(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl3OTGModeContolMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl3OTGModeContolMask : 0b0000_0000);

            WriteToRegister(Register.REG12_Charger_Control_3, buffer[0]);
        }

        public bool GetDisablePFMInOTG()
        {
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            return (buffer[0] & ChargerControl3DisablePFMInOTGMask) == 1;
        }

        public void SetDisablePFMInOTG(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl3DisablePFMInOTGMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl3DisablePFMInOTGMask : 0b0000_0000);

            WriteToRegister(Register.REG12_Charger_Control_3, buffer[0]);
        }

        public bool GetDisablePFMInForwardMode()
        {
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            return (buffer[0] & ChargerControl3DisablePFMInForwardMask) == 1;
        }

        public void SetDisablePFMInForwardMode(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl3DisablePFMInForwardMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl3DisablePFMInForwardMask : 0b0000_0000);

            WriteToRegister(Register.REG12_Charger_Control_3, buffer[0]);
        }

        public bool GetWakeUpDelay()
        {
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            return (buffer[0] & ChargerControl3WakeupDelayMask) == 1;
        }

        public void SetWakeUpDelay(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl3WakeupDelayMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl3WakeupDelayMask : 0b0000_0000);

            WriteToRegister(Register.REG12_Charger_Control_3, buffer[0]);
        }

        public bool GetDisableLDOInPreCharge()
        {
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            return (buffer[0] & ChargerControl3DisableLdoMask) == 1;
        }

        public void SetDisableLDOInPreCharge(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl3DisableLdoMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl3DisableLdoMask : 0b0000_0000);

            WriteToRegister(Register.REG12_Charger_Control_3, buffer[0]);
        }

        public bool GetDisableOOAInOTGMode()
        {
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            return (buffer[0] & ChargerControl3DisableOOAInOTGMask) == 1;
        }

        public void SetDisableOOAInOTGMode(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl3DisableOOAInOTGMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl3DisableOOAInOTGMask : 0b0000_0000);

            WriteToRegister(Register.REG12_Charger_Control_3, buffer[0]);
        }

        public bool GetDisableOOAInForwardMode()
        {
            byte[] buffer = ReadFromRegister(Register.REG12_Charger_Control_3, 1);

            return (buffer[0] & ChargerControl3DisableOOAInForwardMask) == 1;
        }

        public void SetDisableOOAInForwardMode(bool value)
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

        public bool GetEnableAcDrive2()
        {
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            return (buffer[0] & ChargerControl4EnableACDrive1Mask) == 1;
        }

        public void SetEnableAcDrive2(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl4EnableACDrive1Mask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl4EnableACDrive1Mask : 0b0000_0000);

            WriteToRegister(Register.REG13_Charger_Control_4, buffer[0]);
        }

        public bool GetEnableAcDrive1()
        {
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            return (buffer[0] & ChargerControl4EnableACDrive2Mask) == 1;
        }

        public void SetEnableAcDrive1(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl4EnableACDrive2Mask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl4EnableACDrive2Mask : 0b0000_0000);

            WriteToRegister(Register.REG13_Charger_Control_4, buffer[0]);
        }

        public SwitchingFrequency GetPwmFrequency()
        {
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            return (SwitchingFrequency)(buffer[0] & ChargerControl4PwmFrequencyMask);
        }

        public void SetPwmFrequency(SwitchingFrequency value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl4PwmFrequencyMask);
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG13_Charger_Control_4, buffer[0]);
        }

        public bool GetDisableStatPin()
        {
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            return (buffer[0] & ChargerControl4DisableStatPinMask) == 1;
        }

        public void SetDisableStatPin(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl4DisableStatPinMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl4DisableStatPinMask : 0b0000_0000);

            WriteToRegister(Register.REG13_Charger_Control_4, buffer[0]);
        }

        public bool GetDisableForwardModeVsys()
        {
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            return (buffer[0] & ChargerControl4DisableVSysForwardMask) == 1;
        }

        public void SetDisableForwardModeVsys(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl4DisableVSysForwardMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl4DisableVSysForwardMask : 0b0000_0000);

            WriteToRegister(Register.REG13_Charger_Control_4, buffer[0]);
        }

        public bool GetDisableOtgModeUvp()
        {
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            return (buffer[0] & ChargerControl4DisableVOTGUvpMask) == 1;
        }

        public void SetDisableOtgModeUvp(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl4DisableVOTGUvpMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl4DisableVOTGUvpMask : 0b0000_0000);

            WriteToRegister(Register.REG13_Charger_Control_4, buffer[0]);
        }

        public bool GetForceVinDpmDetection()
        {
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            return (buffer[0] & ChargerControl4ForceVinDMPMask) == 1;
        }

        public void SetForceVinDpmDetection(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl4ForceVinDMPMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl4ForceVinDMPMask : 0b0000_0000);

            WriteToRegister(Register.REG13_Charger_Control_4, buffer[0]);
        }

        public bool GetEnableIbusOcp()
        {
            byte[] buffer = ReadFromRegister(Register.REG13_Charger_Control_4, 1);

            return (buffer[0] & ChargerControl4EnableIbusOcpMask) == 1;
        }

        public void SetEnableIbusOcp(bool value)
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

        public bool GetSFetPresent()
        {
            byte[] buffer = ReadFromRegister(Register.REG14_Charger_Control_5, 1);

            return (buffer[0] & ChargerControl5SFETIsPresentMask) == 1;
        }

        public void SetSFetPresent(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG14_Charger_Control_5, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl5SFETIsPresentMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl5SFETIsPresentMask : 0b0000_0000);

            WriteToRegister(Register.REG14_Charger_Control_5, buffer[0]);
        }

        public bool GetEnableBatteryDischargeSensing()
        {
            byte[] buffer = ReadFromRegister(Register.REG14_Charger_Control_5, 1);

            return (buffer[0] & ChargerControl5IBatCurrentSensingMask) == 1;
        }

        public void SetEnableBatteryDischargeSensing(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG14_Charger_Control_5, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl5IBatCurrentSensingMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl5IBatCurrentSensingMask : 0b0000_0000);

            WriteToRegister(Register.REG14_Charger_Control_5, buffer[0]);
        }

        public BatteryDischargeCurrent GetBatteryDischargeCurrent()
        {
            byte[] buffer = ReadFromRegister(Register.REG14_Charger_Control_5, 1);

            return (BatteryDischargeCurrent)(buffer[0] & ChargerControl5BatteryDischargeCurrentMask);
        }

        public void SetBatteryDischargeCurrent(BatteryDischargeCurrent value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG14_Charger_Control_5, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl5BatteryDischargeCurrentMask);
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG14_Charger_Control_5, buffer[0]);
        }

        public bool GetEnableInputDpm()
        {
            byte[] buffer = ReadFromRegister(Register.REG14_Charger_Control_5, 1);

            return (buffer[0] & ChargerControl5EnableIINDPMMask) == 1;
        }

        public void SetEnableInputDpm(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG14_Charger_Control_5, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl5EnableIINDPMMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl5EnableIINDPMMask : 0b0000_0000);

            WriteToRegister(Register.REG14_Charger_Control_5, buffer[0]);
        }

        public bool GetEnableExternalILIM()
        {
            byte[] buffer = ReadFromRegister(Register.REG14_Charger_Control_5, 1);

            return (buffer[0] & ChargerControl5EnableExtILIMMask) == 1;
        }

        public void SetEnableExternalILIM(bool value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG14_Charger_Control_5, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ChargerControl5EnableExtILIMMask);

            // set value
            buffer[0] |= (byte)(value ? ChargerControl5EnableExtILIMMask : 0b0000_0000);

            WriteToRegister(Register.REG14_Charger_Control_5, buffer[0]);
        }

        public bool GetEnableBatteryOCP()
        {
            byte[] buffer = ReadFromRegister(Register.REG14_Charger_Control_5, 1);

            return (buffer[0] & ChargerControl5EnableDischargeOCPMask) == 1;
        }

        public void SetEnableBatteryOCP(bool value)
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

        public bool GetAcrb2Status()
        {
            byte[] buffer = ReadFromRegister(Register.REG1E_Charger_Status_3, 1);

            return (buffer[0] & ChargerStatus3ACRB2StatusMask) == 1;
        }

        public bool GetAcrb1Status()
        {
            byte[] buffer = ReadFromRegister(Register.REG1E_Charger_Status_3, 1);

            return (buffer[0] & ChargerStatus3ACRB1StatusMask) == 1;
        }

        public bool GetAdcDoneStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG1E_Charger_Status_3, 1);

            return (buffer[0] & ChargerStatus3ADCConversionStatusMask) == 1;
        }

        public bool GetVsysStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG1E_Charger_Status_3, 1);

            return (buffer[0] & ChargerStatus3VSysSatusMask) == 1;
        }

        public bool GetFastChargeTimerStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG1E_Charger_Status_3, 1);

            return (buffer[0] & ChargerStatus3FastChargerTimerStatusMask) == 1;
        }

        public bool GetTrickleChargeTimerStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG1E_Charger_Status_3, 1);

            return (buffer[0] & ChargerStatus3TrickleChargeStatusMask) == 1;
        }

        public bool GetPreChargeTimerStatus()
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

        public bool GetVBatOTGLowStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG1F_Charger_Status_4, 1);

            return (buffer[0] & ChargerStatus4VbatOTGLowStatusMask) == 1;
        }

        public bool GetTsColdStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG1F_Charger_Status_4, 1);

            return (buffer[0] & ChargerStatus4TsColdStatusMask) == 1;
        }

        public bool GetTsCoolStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG1F_Charger_Status_4, 1);

            return (buffer[0] & ChargerStatus4TsCoolStatusMask) == 1;
        }

        public bool GetTsWarmStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG1F_Charger_Status_4, 1);

            return (buffer[0] & ChargerStatus4TsWarmStatusMask) == 1;
        }

        public bool GetTsHotStatus()
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

        public bool GetIBatRegulationStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG20_FAULT_Status_0, 1);

            return (buffer[0] & FaultStatus0IBatRegulationStatusMask) == 1;
        }

        public bool GetVbusOverVoltageStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG20_FAULT_Status_0, 1);

            return (buffer[0] & FaultStatus0VBusOverVoltageStatusMask) == 1;
        }

        public bool GetVBatOverVoltageStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG20_FAULT_Status_0, 1);

            return (buffer[0] & FaultStatus0VBatOvervoltageStatusMask) == 1;
        }

        public bool GetIBusOverCurrentStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG20_FAULT_Status_0, 1);

            return (buffer[0] & FaultStatus0IBusOverCurrentStatusMask) == 1;
        }

        public bool GetIBatOverCurrentStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG20_FAULT_Status_0, 1);

            return (buffer[0] & FaultStatus0IBatOverCurrentStatusMask) == 1;
        }

        public bool GetConverterOverCurrentStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG20_FAULT_Status_0, 1);

            return (buffer[0] & FaultStatus0ConverterOverCurrentStatusMask) == 1;
        }

        public bool GetVac2OverVoltageStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG20_FAULT_Status_0, 1);

            return (buffer[0] & FaultStatus0VAc2OverVoltageStatusMask) == 1;
        }

        public bool GetVac1OverVoltageStatus()
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

        public bool GetVsysShortCircuitStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG21_FAULT_Status_1, 1);

            return (buffer[0] & FaultStatus1VsysShortCircuitStatusMask) == 1;
        }

        public bool GetVsysOverVoltageStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG21_FAULT_Status_1, 1);

            return (buffer[0] & FaultStatus1VsysOverVoltageStatusMask) == 1;
        }

        public bool GetOTGOverVoltageStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG21_FAULT_Status_1, 1);

            return (buffer[0] & FaultStatus1OTGOverVoltageStatusMask) == 1;
        }

        public bool GetOTGUnderVoltageStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG21_FAULT_Status_1, 1);

            return (buffer[0] & FaultStatus1OTGUnderVoltageStatusMask) == 1;
        }

        public bool GetTemperatureShutdownStatus()
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

        public bool GetIIndpmFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG22_Charger_Flag_0, 1);

            return (buffer[0] & ChargerFlag0IINDPMFlagMask) == 1;
        }

        public bool GetVIndpmFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG22_Charger_Flag_0, 1);

            return (buffer[0] & ChargerFlag0VINDPMFlagMask) == 1;
        }

        public bool GetI2cWatchdogFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG22_Charger_Flag_0, 1);

            return (buffer[0] & ChargerFlag0I2cWatchdogFlagMask) == 1;
        }

        public bool GetPoorSourceFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG22_Charger_Flag_0, 1);

            return (buffer[0] & ChargerFlag0PoorSourceFlagMask) == 1;
        }

        public bool GetPowerGoodFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG22_Charger_Flag_0, 1);

            return (buffer[0] & ChargerFlag0PowerGoodFlagMask) == 1;
        }

        public bool GetAc2PresentFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG22_Charger_Flag_0, 1);

            return (buffer[0] & ChargerFlag0Vac2PresentFlagMask) == 1;
        }

        public bool GetAc1PresentFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG22_Charger_Flag_0, 1);

            return (buffer[0] & ChargerFlag0Vac1PresentFlagMask) == 1;
        }

        public bool GetVbusPresentFlag()
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

        public bool GetChargeStatusFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG23_Charger_Flag_1, 1);

            return (buffer[0] & ChargerFlag1ChargeFlagMask) == 1;
        }

        public bool GetIcoStatusFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG23_Charger_Flag_1, 1);

            return (buffer[0] & ChargerFlag1IcoFlagMask) == 1;
        }

        public bool GetVbusStatusFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG23_Charger_Flag_1, 1);

            return (buffer[0] & ChargerFlag1VbusFlagMask) == 1;
        }

        public bool GetThermalRegulationFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG23_Charger_Flag_1, 1);

            return (buffer[0] & ChargerFlag1ThermallagMask) == 1;
        }

        public bool GetVbatPresentFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG23_Charger_Flag_1, 1);

            return (buffer[0] & ChargerFlag1VbatPresentFlagMask) == 1;
        }

        public bool GetBc12DetectionFlag()
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

        public bool GetDpdmDoneFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG24_Charger_Flag_2, 1);

            return (buffer[0] & ChargerFlag2DPDMDoneFlagMask) == 1;
        }

        public bool GetAdcDoneFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG24_Charger_Flag_2, 1);

            return (buffer[0] & ChargerFlag2AdcConversionFlagMask) == 1;
        }

        public bool GetVsysFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG24_Charger_Flag_2, 1);

            return (buffer[0] & ChargerFlag2VsysFlagMask) == 1;
        }

        public bool GetFastChargeTimerFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG24_Charger_Flag_2, 1);

            return (buffer[0] & ChargerFlag2FastChargeTimerFlagMask) == 1;
        }

        public bool GetTrickleChargeTimerFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG24_Charger_Flag_2, 1);

            return (buffer[0] & ChargerFlag2TrickleChargeTimerFlagMask) == 1;
        }

        public bool GetPreChargeTimerFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG24_Charger_Flag_2, 1);

            return (buffer[0] & ChargerFlag2PrechargeTimerFlagMask) == 1;
        }

        public bool GetTopOffTimerFlag()
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

        public bool GetVBatOTGLowFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG25_Charger_Flag_3, 1);

            return (buffer[0] & ChargerFlag3VbatOTGLowFlagMask) == 1;
        }

        public bool GetTsColdFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG25_Charger_Flag_3, 1);

            return (buffer[0] & ChargerFlag3TsColdFlagMask) == 1;
        }

        public bool GetTsCoolFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG25_Charger_Flag_3, 1);

            return (buffer[0] & ChargerFlag3TsCoolFlagMask) == 1;
        }

        public bool GetTsWarmFlag()
        {
            byte[] buffer = ReadFromRegister(Register.REG25_Charger_Flag_3, 1);

            return (buffer[0] & ChargerFlag3TsWarmFlagMask) == 1;
        }

        public bool GetTsHotFlag()
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

        public bool GetAdcEnable()
        {
            byte[] buffer = ReadFromRegister(Register.REG2E_ADC_Control, 1);

            return (buffer[0] & AdcControlEnableMask) != 0;
        }

        public void SetAdcEnable(bool value)
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

        public AdcConversioRate GetAdcConversionRate()
        {
            byte[] buffer = ReadFromRegister(Register.REG2E_ADC_Control, 1);

            return (AdcConversioRate)(buffer[0] & AdcControlConversionRateMask);
        }

        public void SetAdcConversionRate(AdcConversioRate value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG2E_ADC_Control, 1);

            // clear bit
            buffer[0] = (byte)(buffer[0] & ~AdcControlConversionRateMask);
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG2E_ADC_Control, buffer[0]);
        }

        public AdcResolution GetAdcResolution()
        {
            byte[] buffer = ReadFromRegister(Register.REG2E_ADC_Control, 1);

            return (AdcResolution)(buffer[0] & AdcControlResolutionMask);
        }

        public void SetAdcResolution(AdcResolution value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG2E_ADC_Control, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~AdcControlResolutionMask);
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG2E_ADC_Control, buffer[0]);
        }

        public AdcAveraging GetAdcAveraging()
        {
            byte[] buffer = ReadFromRegister(Register.REG2E_ADC_Control, 1);

            return (AdcAveraging)(buffer[0] & AdcControlAverageControlMask);
        }

        public void SetAdcAveraging(AdcAveraging value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG2E_ADC_Control, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~AdcControlAverageControlMask);
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG2E_ADC_Control, buffer[0]);
        }

        public AdcInitialAverage GetAdcInitialAverage()
        {
            byte[] buffer = ReadFromRegister(Register.REG2E_ADC_Control, 1);

            return (AdcInitialAverage)(buffer[0] & AdcControlInitialAverageMask);
        }

        public void SetAdcInitialAverage(AdcInitialAverage value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG2E_ADC_Control, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~AdcControlInitialAverageMask);
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG2E_ADC_Control, buffer[0]);
        }

        public ThermalRegulationThreshold GetThermalRegulationThreshold()
        {
            byte[] buffer = ReadFromRegister(Register.REG16_Temperature_Control, 1);

            return (ThermalRegulationThreshold)(buffer[0] & ThermalRegulationThresholdMask);
        }

        public void SetThermalRegulationThreshold(ThermalRegulationThreshold value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG16_Temperature_Control, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ThermalRegulationThresholdMask);
            buffer[0] |= (byte)value;

            WriteToRegister(Register.REG16_Temperature_Control, buffer[0]);
        }

        public ThermalShutdownThreshold GetThermalShutdownThreshold()
        {
            byte[] buffer = ReadFromRegister(Register.REG16_Temperature_Control, 1);

            return (ThermalShutdownThreshold)((buffer[0] & ThermalShutdownThresholdMask) >> 2);
        }

        public void SetThermalShutdownThreshold(ThermalShutdownThreshold value)
        {
            // read existing content
            byte[] buffer = ReadFromRegister(Register.REG16_Temperature_Control, 1);

            // clear bits
            buffer[0] = (byte)(buffer[0] & ~ThermalShutdownThresholdMask);
            buffer[0] |= (byte)((byte)value << 2);

            WriteToRegister(Register.REG16_Temperature_Control, buffer[0]);
        }

        #endregion

        #region REG3F_TS_ADC

        // REG3F_TS_ADC Register
        // | 15 14 13 12 11 10 9 8 7 6 5 4 3 2 1 0 |
        // |             TS_ADC_15:0               |
        ////

        public float GetTCAdcReading()
        {
            byte[] buffer = ReadFromRegister(Register.REG3F_TS_ADC, 2);

            var adcReading = (buffer[0] << 8) | buffer[1];

            return adcReading * TcAdcStep;
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

        public ElectricPotentialDc GetAdcVoltage(Register register)
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

        public Temperature GetDieTemperature()
        {
            byte[] buffer = ReadFromRegister(Register.REG41_TDIE_ADC, 2);

            var ascReading = (buffer[0] << 8) | buffer[1];

            return new Temperature(
                ascReading * TdieStemp,
                UnitsNet.Units.TemperatureUnit.DegreeCelsius);
        }

        #endregion

        #region Helper Methods to Read and Write to Registers

        public void WriteToRegister(Register register, byte content)
        {
            WriteToRegister(register, new byte[] { content });
        }

        public void WriteToRegister(Register register, byte[] contents)
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

        public byte[] ReadFromRegister(Register register, int readByteCount)
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
