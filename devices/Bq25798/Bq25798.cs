////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

using System;
using System.Device.I2c;
using System.Device.Model;
using UnitsNet;

namespace Iot.Device.Bq25798
{
    /// <summary>
    /// Buck-Boost Battery Charger Bq25798.
    /// </summary>
    [Interface("Buck-Boost Battery Charger Bq25798")]
    public class Bq25798 : IDisposable
    {
        /// <summary>
        /// Bq25798 Default I2C Address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x6B;

        private const int DevicePartNumberMask = 0b00111000;
        private const int DeviceRevisionMask = 0b00000111;

        private const int FixedOffsetMinimalSystemVoltage = 2500;
        private const int MaxValueMinimalSystemVoltage = 16000;
        private const int StepMinimalSystemVoltage = 250;

        private const int PrechargeCurrentLimitMinValue = 40;
        private const int StepPrechargeCurrentLimit = 40;
        private const int PrechargeCurrentLimitMaxValue = 2000;

        private const int Bq25798PartNumber = 0b00011000;
        private const int DeviceRevision = 0b00000001;
        private const int PrechargeCurrentLimitMask = 0b00111111;
        private const int VbusAdcStep = 1;

        private I2cDevice _i2cDevice;

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
        /// <remarks>Range : 0mV-30000mV.</remarks>
        public ElectricPotentialDc Vbus => GetVbus();

        /// <summary>
        /// Initializes a new instance of the <see cref="Bq25798" /> class.
        /// </summary>
        /// <param name="i2cDevice"><see cref="I2cDevice"/> to communicate with Si7021 device.</param>
        /// <exception cref="InvalidOperationException">When failing to read part information.</exception>
        /// <exception cref="NotSupportedException">If the part information returned is invalid, thus the connected part is not a BQ25798.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="i2cDevice"/> is null.</exception>
        public Bq25798(I2cDevice i2cDevice)
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

            if ((buffer[0] & DevicePartNumberMask) != Bq25798PartNumber
                || (buffer[0] & DeviceRevisionMask) != DeviceRevision)
            {
                throw new NotSupportedException();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }

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

            throw new NotImplementedException();
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

        #region REG35_VBUS_ADC

        // REG35_VBUS_ADC Register
        // | 15 14 13 12 11 10 9 8 | 7 6 5 4 3 2 1 0 |
        // |                VBUS_ADC_15:0            |
        ////

        private ElectricPotentialDc GetVbus()
        {
            byte[] buffer = ReadFromRegister(Register.REG35_VBUS_ADC_Register, 2);

            var vbus = (buffer[0] << 8) | buffer[1];

            return new ElectricPotentialDc(
                vbus * VbusAdcStep,
                UnitsNet.Units.ElectricPotentialDcUnit.MillivoltDc);
        }

        #endregion

        #region Helper Methods to Read and Write to Registers

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
