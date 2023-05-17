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
        /// <summary>
        /// Bq25792/8 Default I2C Address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x6B;

        private const byte DevicePartNumberMask = 0b00111000;
        private const byte DeviceRevisionMask = 0b00000111;
        private const byte PrechargeCurrentLimitMask = 0b00111111;
        private const byte ChargerStatus1ChargeStatusMask = 0b1110_0000;
        private const byte ChargerStatus1VbusStatusMask = 0b0001_1110;
        private const byte ChargerStatus1Bc12DetectionMask = 0b0000_0001;
        private const byte ChargerControl1WatchdogMask = 0b0000_0111;

        private const int FixedOffsetMinimalSystemVoltage = 2500;
        private const int MaxValueMinimalSystemVoltage = 16000;
        private const int StepMinimalSystemVoltage = 250;

        private const int PrechargeCurrentLimitMinValue = 40;
        private const int StepPrechargeCurrentLimit = 40;
        private const int PrechargeCurrentLimitMaxValue = 2000;

        private const int DeviceRevision = 0b00000001;
        private const int VbusAdcStep = 1;
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
        /// Gets charger status 0.
        /// </summary>
        [Property]
        public ChargerStatus0 ChargerStatus0 => GetChargerStatus0();

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

        private ChargeStatus GetChargeStatus()
        {
            byte[] buffer = ReadFromRegister(Register.REG1C_Charger_Status_1, 1);

            return (ChargeStatus)(buffer[0] & ChargerStatus1ChargeStatusMask);
        }

        #endregion

        #region REG1C_Charger_Status_1

        // REG1C_Charger_Status_1 Register
        // |    7 6 5     |     4 3 2 1   |         0       |
        // | CHG_STAT_2:0 | VBUS_STAT_3:0 | BC1.2_DONE_STAT |
        ////

        private ChargerStatus0 GetChargerStatus0()
        {
            byte[] buffer = ReadFromRegister(Register.REG1B_Charger_Status_0, 1);

            return (ChargerStatus0)buffer[0];
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
