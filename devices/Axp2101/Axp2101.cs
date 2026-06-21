// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using UnitsNet;

namespace Iot.Device.Axp2101
{
    /// <summary>
    /// AXP2101 - Enhanced single Cell Li-Battery and Power System Management IC.
    /// </summary>
    public class Axp2101 : IDisposable
    {
        /// <summary>
        /// AXP2101 default I2C address.
        /// </summary>
        public const int I2cDefaultAddress = 0x34;

        /// <summary>
        /// AXP2101 chip identification value.
        /// </summary>
        public const byte ChipId = 0x4A;

        private const int DataBufferSize = 4;
        private const int ButtonBatteryVoltageMin = 2600;
        private const int ButtonBatteryVoltageMax = 3300;
        private const int ButtonBatteryVoltageStep = 100;
        private const int SysPowerDownVoltageMin = 2600;
        private const int SysPowerDownVoltageMax = 3300;
        private const int SysPowerDownVoltageStep = 100;

        private I2cDevice _i2c;
        private byte[] _writeBuffer = new byte[2];

        /// <summary>
        /// Initializes a new instance of the <see cref="Axp2101"/> class.
        /// </summary>
        /// <param name="i2c">The I2C device.</param>
        /// <exception cref="ArgumentNullException">Thrown when i2c is null.</exception>
        public Axp2101(I2cDevice i2c)
        {
            _i2c = i2c ?? throw new ArgumentNullException();
        }

        #region 4.1 — Power Status & Identification

        /// <summary>
        /// Reads the chip ID from the IC_TYPE register.
        /// </summary>
        /// <returns>Chip ID byte (should be 0x4A for AXP2101).</returns>
        public byte GetChipId()
        {
            return I2cRead(Register.IcType);
        }

        /// <summary>
        /// Gets a value indicating whether VBUS voltage is present and good.
        /// </summary>
        public bool IsVbusGood => GetBit(Register.Status1, 5);

        /// <summary>
        /// Gets a value indicating whether a battery is connected.
        /// </summary>
        public bool IsBatteryConnected => GetBit(Register.Status1, 3);

        /// <summary>
        /// Gets a value indicating whether the device is currently charging.
        /// </summary>
        public bool IsCharging => (I2cRead(Register.Status2) >> 5) == 0x01;

        /// <summary>
        /// Gets a value indicating whether the device is currently discharging.
        /// </summary>
        public bool IsDischarging => (I2cRead(Register.Status2) >> 5) == 0x02;

        /// <summary>
        /// Gets the current charger status.
        /// </summary>
        /// <returns>The charging status.</returns>
        public ChargingStatus GetChargerStatus()
        {
            return (ChargingStatus)(I2cRead(Register.Status2) & 0x07);
        }

        /// <summary>
        /// Gets the power-on source.
        /// </summary>
        /// <returns>The power-on source flags.</returns>
        public PowerOnSource GetPowerOnSource()
        {
            return (PowerOnSource)I2cRead(Register.PwronStatus);
        }

        /// <summary>
        /// Gets the power-off source.
        /// </summary>
        /// <returns>The power-off source flags.</returns>
        public PowerOffSource GetPowerOffSource()
        {
            return (PowerOffSource)I2cRead(Register.PwroffStatus);
        }

        #endregion

        #region 4.2 — DCDC Voltage Regulators

        // ---- DCDC1: 1500-3400 mV, 100 mV steps ----

        /// <summary>
        /// Enables the DCDC1 output.
        /// </summary>
        public void EnableDcDc1() => SetBit(Register.DcOnOffDvmCtrl, 0);

        /// <summary>
        /// Disables the DCDC1 output.
        /// </summary>
        public void DisableDcDc1() => ClearBit(Register.DcOnOffDvmCtrl, 0);

        /// <summary>
        /// Gets a value indicating whether DCDC1 is enabled.
        /// </summary>
        public bool IsDcDc1Enabled => GetBit(Register.DcOnOffDvmCtrl, 0);

        /// <summary>
        /// Gets or sets the DCDC1 output voltage.
        /// </summary>
        /// <remarks>Range is 1500-3400 mV in 100 mV steps.</remarks>
        public ElectricPotential DcDc1Voltage
        {
            get
            {
                int val = I2cRead(Register.DcVol0Ctrl) & 0x1F;
                return ElectricPotential.FromMillivolts((val * 100) + 1500);
            }

            set
            {
                int mv = ClampMillivolts(value, 1500, 3400);
                I2cWrite(Register.DcVol0Ctrl, (byte)((mv - 1500) / 100));
            }
        }

        /// <summary>
        /// Sets whether DCDC1 85% low voltage turns off the PMIC.
        /// </summary>
        /// <param name="enable">True to enable low voltage power down.</param>
        public void SetDcDc1LowVoltagePowerDown(bool enable) => SetOrClearBit(Register.DcOvpUvpCtrl, 0, enable);

        // ---- DCDC2: 500-1200 mV (10 mV steps) + 1220-1540 mV (20 mV steps) ----

        /// <summary>
        /// Enables the DCDC2 output.
        /// </summary>
        public void EnableDcDc2() => SetBit(Register.DcOnOffDvmCtrl, 1);

        /// <summary>
        /// Disables the DCDC2 output.
        /// </summary>
        public void DisableDcDc2() => ClearBit(Register.DcOnOffDvmCtrl, 1);

        /// <summary>
        /// Gets a value indicating whether DCDC2 is enabled.
        /// </summary>
        public bool IsDcDc2Enabled => GetBit(Register.DcOnOffDvmCtrl, 1);

        /// <summary>
        /// Gets or sets the DCDC2 output voltage.
        /// </summary>
        /// <remarks>Range 1: 500-1200 mV in 10 mV steps. Range 2: 1220-1540 mV in 20 mV steps.</remarks>
        public ElectricPotential DcDc2Voltage
        {
            get
            {
                int val = I2cRead(Register.DcVol1Ctrl) & 0x7F;
                if (val < 71)
                {
                    return ElectricPotential.FromMillivolts((val * 10) + 500);
                }
                else
                {
                    return ElectricPotential.FromMillivolts((val * 20) - 200);
                }
            }

            set
            {
                int mv = (int)value.Millivolts;
                byte existing = (byte)(I2cRead(Register.DcVol1Ctrl) & 0x80);

                if (mv <= 1200)
                {
                    mv = Clamp(mv, 500, 1200);
                    I2cWrite(Register.DcVol1Ctrl, (byte)(existing | ((mv - 500) / 10)));
                }
                else
                {
                    mv = Clamp(mv, 1220, 1540);
                    int regVal = ((mv - 1220) / 20) + 71;
                    I2cWrite(Register.DcVol1Ctrl, (byte)(existing | regVal));
                }
            }
        }

        /// <summary>
        /// Sets whether DCDC2 85% low voltage turns off the PMIC.
        /// </summary>
        /// <param name="enable">True to enable low voltage power down.</param>
        public void SetDcDc2LowVoltagePowerDown(bool enable) => SetOrClearBit(Register.DcOvpUvpCtrl, 1, enable);

        // ---- DCDC3: 500-1200 mV (10 mV), 1220-1540 mV (20 mV), 1600-3400 mV (100 mV) ----

        /// <summary>
        /// Enables the DCDC3 output.
        /// </summary>
        public void EnableDcDc3() => SetBit(Register.DcOnOffDvmCtrl, 2);

        /// <summary>
        /// Disables the DCDC3 output.
        /// </summary>
        public void DisableDcDc3() => ClearBit(Register.DcOnOffDvmCtrl, 2);

        /// <summary>
        /// Gets a value indicating whether DCDC3 is enabled.
        /// </summary>
        public bool IsDcDc3Enabled => GetBit(Register.DcOnOffDvmCtrl, 2);

        /// <summary>
        /// Gets or sets the DCDC3 output voltage.
        /// </summary>
        /// <remarks>Range 1: 500-1200 mV (10 mV steps). Range 2: 1220-1540 mV (20 mV steps). Range 3: 1600-3400 mV (100 mV steps).</remarks>
        public ElectricPotential DcDc3Voltage
        {
            get
            {
                int val = I2cRead(Register.DcVol2Ctrl) & 0x7F;
                if (val < 71)
                {
                    return ElectricPotential.FromMillivolts((val * 10) + 500);
                }
                else if (val < 88)
                {
                    return ElectricPotential.FromMillivolts((val * 20) - 200);
                }
                else
                {
                    return ElectricPotential.FromMillivolts((val * 100) - 7200);
                }
            }

            set
            {
                int mv = (int)value.Millivolts;
                byte existing = (byte)(I2cRead(Register.DcVol2Ctrl) & 0x80);

                if (mv <= 1200)
                {
                    mv = Clamp(mv, 500, 1200);
                    I2cWrite(Register.DcVol2Ctrl, (byte)(existing | ((mv - 500) / 10)));
                }
                else if (mv <= 1540)
                {
                    mv = Clamp(mv, 1220, 1540);
                    int regVal = ((mv - 1220) / 20) + 71;
                    I2cWrite(Register.DcVol2Ctrl, (byte)(existing | regVal));
                }
                else
                {
                    mv = Clamp(mv, 1600, 3400);
                    int regVal = ((mv - 1600) / 100) + 88;
                    I2cWrite(Register.DcVol2Ctrl, (byte)(existing | regVal));
                }
            }
        }

        /// <summary>
        /// Sets whether DCDC3 85% low voltage turns off the PMIC.
        /// </summary>
        /// <param name="enable">True to enable low voltage power down.</param>
        public void SetDcDc3LowVoltagePowerDown(bool enable) => SetOrClearBit(Register.DcOvpUvpCtrl, 2, enable);

        // ---- DCDC4: 500-1200 mV (10 mV), 1220-1840 mV (20 mV) ----

        /// <summary>
        /// Enables the DCDC4 output.
        /// </summary>
        public void EnableDcDc4() => SetBit(Register.DcOnOffDvmCtrl, 3);

        /// <summary>
        /// Disables the DCDC4 output.
        /// </summary>
        public void DisableDcDc4() => ClearBit(Register.DcOnOffDvmCtrl, 3);

        /// <summary>
        /// Gets a value indicating whether DCDC4 is enabled.
        /// </summary>
        public bool IsDcDc4Enabled => GetBit(Register.DcOnOffDvmCtrl, 3);

        /// <summary>
        /// Gets or sets the DCDC4 output voltage.
        /// </summary>
        /// <remarks>Range 1: 500-1200 mV (10 mV steps). Range 2: 1220-1840 mV (20 mV steps).</remarks>
        public ElectricPotential DcDc4Voltage
        {
            get
            {
                int val = I2cRead(Register.DcVol3Ctrl) & 0x7F;
                if (val < 71)
                {
                    return ElectricPotential.FromMillivolts((val * 10) + 500);
                }
                else
                {
                    return ElectricPotential.FromMillivolts((val * 20) - 200);
                }
            }

            set
            {
                int mv = (int)value.Millivolts;
                byte existing = (byte)(I2cRead(Register.DcVol3Ctrl) & 0x80);

                if (mv <= 1200)
                {
                    mv = Clamp(mv, 500, 1200);
                    I2cWrite(Register.DcVol3Ctrl, (byte)(existing | ((mv - 500) / 10)));
                }
                else
                {
                    mv = Clamp(mv, 1220, 1840);
                    int regVal = ((mv - 1220) / 20) + 71;
                    I2cWrite(Register.DcVol3Ctrl, (byte)(existing | regVal));
                }
            }
        }

        /// <summary>
        /// Sets whether DCDC4 85% low voltage turns off the PMIC.
        /// </summary>
        /// <param name="enable">True to enable low voltage power down.</param>
        public void SetDcDc4LowVoltagePowerDown(bool enable) => SetOrClearBit(Register.DcOvpUvpCtrl, 3, enable);

        // ---- DCDC5: 1200 mV special + 1400-3700 mV (100 mV steps) ----

        /// <summary>
        /// Enables the DCDC5 output.
        /// </summary>
        public void EnableDcDc5() => SetBit(Register.DcOnOffDvmCtrl, 4);

        /// <summary>
        /// Disables the DCDC5 output.
        /// </summary>
        public void DisableDcDc5() => ClearBit(Register.DcOnOffDvmCtrl, 4);

        /// <summary>
        /// Gets a value indicating whether DCDC5 is enabled.
        /// </summary>
        public bool IsDcDc5Enabled => GetBit(Register.DcOnOffDvmCtrl, 4);

        /// <summary>
        /// Gets or sets the DCDC5 output voltage.
        /// </summary>
        /// <remarks>Special 1200 mV value or range 1400-3700 mV (100 mV steps).</remarks>
        public ElectricPotential DcDc5Voltage
        {
            get
            {
                int val = I2cRead(Register.DcVol4Ctrl) & 0x1F;
                if (val == 0x19)
                {
                    return ElectricPotential.FromMillivolts(1200);
                }

                return ElectricPotential.FromMillivolts((val * 100) + 1400);
            }

            set
            {
                int mv = (int)value.Millivolts;
                byte existing = (byte)(I2cRead(Register.DcVol4Ctrl) & 0xE0);

                if (mv == 1200)
                {
                    I2cWrite(Register.DcVol4Ctrl, (byte)(existing | 0x19));
                }
                else
                {
                    mv = Clamp(mv, 1400, 3700);
                    I2cWrite(Register.DcVol4Ctrl, (byte)(existing | ((mv - 1400) / 100)));
                }
            }
        }

        /// <summary>
        /// Sets whether DCDC5 85% low voltage turns off the PMIC.
        /// </summary>
        /// <param name="enable">True to enable low voltage power down.</param>
        public void SetDcDc5LowVoltagePowerDown(bool enable) => SetOrClearBit(Register.DcOvpUvpCtrl, 4, enable);

        #endregion

        #region 4.3 — LDO Voltage Regulators

        // ---- ALDO1: 500-3500 mV, 100 mV steps (register 0x92, bits [4:0]) ----

        /// <summary>Enables ALDO1 output.</summary>
        public void EnableAldo1() => SetBit(Register.LdoOnOffCtrl0, 0);

        /// <summary>Disables ALDO1 output.</summary>
        public void DisableAldo1() => ClearBit(Register.LdoOnOffCtrl0, 0);

        /// <summary>Gets a value indicating whether ALDO1 is enabled.</summary>
        public bool IsAldo1Enabled => GetBit(Register.LdoOnOffCtrl0, 0);

        /// <summary>
        /// Gets or sets ALDO1 voltage.
        /// </summary>
        /// <remarks>Range 500-3500 mV, 100 mV steps.</remarks>
        public ElectricPotential Aldo1Voltage
        {
            get => ElectricPotential.FromMillivolts(((I2cRead(Register.LdoVol0Ctrl) & 0x1F) * 100) + 500);
            set => I2cWrite(Register.LdoVol0Ctrl, (byte)((I2cRead(Register.LdoVol0Ctrl) & 0xE0) | ((ClampMillivolts(value, 500, 3500) - 500) / 100)));
        }

        // ---- ALDO2: 500-3500 mV, 100 mV steps (register 0x93) ----

        /// <summary>Enables ALDO2 output.</summary>
        public void EnableAldo2() => SetBit(Register.LdoOnOffCtrl0, 1);

        /// <summary>Disables ALDO2 output.</summary>
        public void DisableAldo2() => ClearBit(Register.LdoOnOffCtrl0, 1);

        /// <summary>Gets a value indicating whether ALDO2 is enabled.</summary>
        public bool IsAldo2Enabled => GetBit(Register.LdoOnOffCtrl0, 1);

        /// <summary>
        /// Gets or sets ALDO2 voltage.
        /// </summary>
        /// <remarks>Range 500-3500 mV, 100 mV steps.</remarks>
        public ElectricPotential Aldo2Voltage
        {
            get => ElectricPotential.FromMillivolts(((I2cRead(Register.LdoVol1Ctrl) & 0x1F) * 100) + 500);
            set => I2cWrite(Register.LdoVol1Ctrl, (byte)((I2cRead(Register.LdoVol1Ctrl) & 0xE0) | ((ClampMillivolts(value, 500, 3500) - 500) / 100)));
        }

        // ---- ALDO3: 500-3500 mV, 100 mV steps (register 0x94) ----

        /// <summary>Enables ALDO3 output.</summary>
        public void EnableAldo3() => SetBit(Register.LdoOnOffCtrl0, 2);

        /// <summary>Disables ALDO3 output.</summary>
        public void DisableAldo3() => ClearBit(Register.LdoOnOffCtrl0, 2);

        /// <summary>Gets a value indicating whether ALDO3 is enabled.</summary>
        public bool IsAldo3Enabled => GetBit(Register.LdoOnOffCtrl0, 2);

        /// <summary>
        /// Gets or sets ALDO3 voltage.
        /// </summary>
        /// <remarks>Range 500-3500 mV, 100 mV steps.</remarks>
        public ElectricPotential Aldo3Voltage
        {
            get => ElectricPotential.FromMillivolts(((I2cRead(Register.LdoVol2Ctrl) & 0x1F) * 100) + 500);
            set => I2cWrite(Register.LdoVol2Ctrl, (byte)((I2cRead(Register.LdoVol2Ctrl) & 0xE0) | ((ClampMillivolts(value, 500, 3500) - 500) / 100)));
        }

        // ---- ALDO4: 500-3500 mV, 100 mV steps (register 0x95) ----

        /// <summary>Enables ALDO4 output.</summary>
        public void EnableAldo4() => SetBit(Register.LdoOnOffCtrl0, 3);

        /// <summary>Disables ALDO4 output.</summary>
        public void DisableAldo4() => ClearBit(Register.LdoOnOffCtrl0, 3);

        /// <summary>Gets a value indicating whether ALDO4 is enabled.</summary>
        public bool IsAldo4Enabled => GetBit(Register.LdoOnOffCtrl0, 3);

        /// <summary>
        /// Gets or sets ALDO4 voltage.
        /// </summary>
        /// <remarks>Range 500-3500 mV, 100 mV steps.</remarks>
        public ElectricPotential Aldo4Voltage
        {
            get => ElectricPotential.FromMillivolts(((I2cRead(Register.LdoVol3Ctrl) & 0x1F) * 100) + 500);
            set => I2cWrite(Register.LdoVol3Ctrl, (byte)((I2cRead(Register.LdoVol3Ctrl) & 0xE0) | ((ClampMillivolts(value, 500, 3500) - 500) / 100)));
        }

        // ---- BLDO1: 500-3500 mV, 100 mV steps (register 0x96) ----

        /// <summary>Enables BLDO1 output.</summary>
        public void EnableBldo1() => SetBit(Register.LdoOnOffCtrl0, 4);

        /// <summary>Disables BLDO1 output.</summary>
        public void DisableBldo1() => ClearBit(Register.LdoOnOffCtrl0, 4);

        /// <summary>Gets a value indicating whether BLDO1 is enabled.</summary>
        public bool IsBldo1Enabled => GetBit(Register.LdoOnOffCtrl0, 4);

        /// <summary>
        /// Gets or sets BLDO1 voltage.
        /// </summary>
        /// <remarks>Range 500-3500 mV, 100 mV steps.</remarks>
        public ElectricPotential Bldo1Voltage
        {
            get => ElectricPotential.FromMillivolts(((I2cRead(Register.LdoVol4Ctrl) & 0x1F) * 100) + 500);
            set => I2cWrite(Register.LdoVol4Ctrl, (byte)((I2cRead(Register.LdoVol4Ctrl) & 0xE0) | ((ClampMillivolts(value, 500, 3500) - 500) / 100)));
        }

        // ---- BLDO2: 500-3500 mV, 100 mV steps (register 0x97) ----

        /// <summary>Enables BLDO2 output.</summary>
        public void EnableBldo2() => SetBit(Register.LdoOnOffCtrl0, 5);

        /// <summary>Disables BLDO2 output.</summary>
        public void DisableBldo2() => ClearBit(Register.LdoOnOffCtrl0, 5);

        /// <summary>Gets a value indicating whether BLDO2 is enabled.</summary>
        public bool IsBldo2Enabled => GetBit(Register.LdoOnOffCtrl0, 5);

        /// <summary>
        /// Gets or sets BLDO2 voltage.
        /// </summary>
        /// <remarks>Range 500-3500 mV, 100 mV steps.</remarks>
        public ElectricPotential Bldo2Voltage
        {
            get => ElectricPotential.FromMillivolts(((I2cRead(Register.LdoVol5Ctrl) & 0x1F) * 100) + 500);
            set => I2cWrite(Register.LdoVol5Ctrl, (byte)((I2cRead(Register.LdoVol5Ctrl) & 0xE0) | ((ClampMillivolts(value, 500, 3500) - 500) / 100)));
        }

        // ---- CPUSLDO: 500-1400 mV, 50 mV steps (register 0x98) ----

        /// <summary>Enables CPUSLDO output.</summary>
        public void EnableCpusLdo() => SetBit(Register.LdoOnOffCtrl0, 6);

        /// <summary>Disables CPUSLDO output.</summary>
        public void DisableCpusLdo() => ClearBit(Register.LdoOnOffCtrl0, 6);

        /// <summary>Gets a value indicating whether CPUSLDO is enabled.</summary>
        public bool IsCpusLdoEnabled => GetBit(Register.LdoOnOffCtrl0, 6);

        /// <summary>
        /// Gets or sets CPUSLDO voltage.
        /// </summary>
        /// <remarks>Range 500-1400 mV, 50 mV steps.</remarks>
        public ElectricPotential CpusLdoVoltage
        {
            get => ElectricPotential.FromMillivolts(((I2cRead(Register.LdoVol6Ctrl) & 0x1F) * 50) + 500);
            set => I2cWrite(Register.LdoVol6Ctrl, (byte)((I2cRead(Register.LdoVol6Ctrl) & 0xE0) | ((ClampMillivolts(value, 500, 1400) - 500) / 50)));
        }

        // ---- DLDO1: 500-3400 mV, 100 mV steps (register 0x99) ----

        /// <summary>Enables DLDO1 output.</summary>
        public void EnableDldo1() => SetBit(Register.LdoOnOffCtrl0, 7);

        /// <summary>Disables DLDO1 output.</summary>
        public void DisableDldo1() => ClearBit(Register.LdoOnOffCtrl0, 7);

        /// <summary>Gets a value indicating whether DLDO1 is enabled.</summary>
        public bool IsDldo1Enabled => GetBit(Register.LdoOnOffCtrl0, 7);

        /// <summary>
        /// Gets or sets DLDO1 voltage.
        /// </summary>
        /// <remarks>Range 500-3400 mV, 100 mV steps.</remarks>
        public ElectricPotential Dldo1Voltage
        {
            get => ElectricPotential.FromMillivolts(((I2cRead(Register.LdoVol7Ctrl) & 0x1F) * 100) + 500);
            set => I2cWrite(Register.LdoVol7Ctrl, (byte)((I2cRead(Register.LdoVol7Ctrl) & 0xE0) | ((ClampMillivolts(value, 500, 3400) - 500) / 100)));
        }

        // ---- DLDO2: 500-3400 mV, 100 mV steps (register 0x9A) ----

        /// <summary>Enables DLDO2 output.</summary>
        public void EnableDldo2() => SetBit(Register.LdoOnOffCtrl1, 0);

        /// <summary>Disables DLDO2 output.</summary>
        public void DisableDldo2() => ClearBit(Register.LdoOnOffCtrl1, 0);

        /// <summary>Gets a value indicating whether DLDO2 is enabled.</summary>
        public bool IsDldo2Enabled => GetBit(Register.LdoOnOffCtrl1, 0);

        /// <summary>
        /// Gets or sets DLDO2 voltage.
        /// </summary>
        /// <remarks>Range 500-3400 mV, 100 mV steps.</remarks>
        public ElectricPotential Dldo2Voltage
        {
            get => ElectricPotential.FromMillivolts(((I2cRead(Register.LdoVol8Ctrl) & 0x1F) * 100) + 500);
            set => I2cWrite(Register.LdoVol8Ctrl, (byte)((I2cRead(Register.LdoVol8Ctrl) & 0xE0) | ((ClampMillivolts(value, 500, 3400) - 500) / 100)));
        }

        #endregion

        #region 4.4 — Battery Charging Control

        /// <summary>
        /// Enables cell (main) battery charging.
        /// </summary>
        public void EnableCellBatteryCharge() => SetBit(Register.ChargeGaugeWdtCtrl, 1);

        /// <summary>
        /// Disables cell (main) battery charging.
        /// </summary>
        public void DisableCellBatteryCharge() => ClearBit(Register.ChargeGaugeWdtCtrl, 1);

        /// <summary>
        /// Sets the constant charge current.
        /// </summary>
        /// <param name="current">The charging current setting.</param>
        public void SetChargeConstantCurrent(ChargingCurrent current)
        {
            byte val = I2cRead(Register.IccChgSet);
            val = (byte)((val & 0xE0) | ((byte)current & 0x1F));
            I2cWrite(Register.IccChgSet, val);
        }

        /// <summary>
        /// Gets the constant charge current setting.
        /// </summary>
        /// <returns>The charging current setting.</returns>
        public ChargingCurrent GetChargeConstantCurrent()
        {
            return (ChargingCurrent)(I2cRead(Register.IccChgSet) & 0x1F);
        }

        /// <summary>
        /// Sets the pre-charge current.
        /// </summary>
        /// <param name="current">The pre-charge current setting.</param>
        public void SetPrechargeCurrent(PrechargeCurrent current)
        {
            byte val = I2cRead(Register.IprechgSet);
            val = (byte)((val & 0xF0) | ((byte)current & 0x0F));
            I2cWrite(Register.IprechgSet, val);
        }

        /// <summary>
        /// Gets the pre-charge current setting.
        /// </summary>
        /// <returns>The pre-charge current setting.</returns>
        public PrechargeCurrent GetPrechargeCurrent()
        {
            return (PrechargeCurrent)(I2cRead(Register.IprechgSet) & 0x0F);
        }

        /// <summary>
        /// Sets the charge termination current.
        /// </summary>
        /// <param name="current">The termination current setting.</param>
        public void SetChargeTerminationCurrent(ChargeTerminationCurrent current)
        {
            byte val = I2cRead(Register.ItermChgSetCtrl);
            val = (byte)((val & 0xF0) | ((byte)current & 0x0F));
            I2cWrite(Register.ItermChgSetCtrl, val);
        }

        /// <summary>
        /// Enables the charge termination current limit.
        /// </summary>
        public void EnableChargeTerminationLimit()
        {
            byte val = I2cRead(Register.ItermChgSetCtrl);
            I2cWrite(Register.ItermChgSetCtrl, (byte)(val | 0x10));
        }

        /// <summary>
        /// Disables the charge termination current limit.
        /// </summary>
        public void DisableChargeTerminationLimit()
        {
            byte val = I2cRead(Register.ItermChgSetCtrl);
            I2cWrite(Register.ItermChgSetCtrl, (byte)(val & 0xEF));
        }

        /// <summary>
        /// Sets the charge target voltage.
        /// </summary>
        /// <param name="voltage">The target voltage setting.</param>
        public void SetChargeTargetVoltage(ChargeTargetVoltage voltage)
        {
            byte val = I2cRead(Register.CvChgVolSet);
            val = (byte)((val & 0xF8) | ((byte)voltage & 0x07));
            I2cWrite(Register.CvChgVolSet, val);
        }

        /// <summary>
        /// Gets the charge target voltage setting.
        /// </summary>
        /// <returns>The charge target voltage.</returns>
        public ChargeTargetVoltage GetChargeTargetVoltage()
        {
            return (ChargeTargetVoltage)(I2cRead(Register.CvChgVolSet) & 0x07);
        }

        /// <summary>
        /// Sets the thermal regulation threshold.
        /// </summary>
        /// <param name="threshold">The thermal threshold setting.</param>
        public void SetThermalThreshold(ThermalThreshold threshold)
        {
            byte val = I2cRead(Register.TheReguThresSet);
            val = (byte)((val & 0xFC) | ((byte)threshold & 0x03));
            I2cWrite(Register.TheReguThresSet, val);
        }

        #endregion

        #region 4.5 — Button/Backup Battery

        /// <summary>
        /// Enables button (backup) battery charging.
        /// </summary>
        public void EnableButtonBatteryCharge() => SetBit(Register.ChargeGaugeWdtCtrl, 2);

        /// <summary>
        /// Disables button (backup) battery charging.
        /// </summary>
        public void DisableButtonBatteryCharge() => ClearBit(Register.ChargeGaugeWdtCtrl, 2);

        /// <summary>
        /// Sets the button battery charge voltage.
        /// </summary>
        /// <param name="voltage">Target voltage (2600-3300 mV, 100 mV steps).</param>
        public void SetButtonBatteryChargeVoltage(ElectricPotential voltage)
        {
            int mv = ClampMillivolts(voltage, ButtonBatteryVoltageMin, ButtonBatteryVoltageMax);
            byte val = I2cRead(Register.BtnBatChgVolSet);
            val = (byte)((val & 0xF8) | ((mv - ButtonBatteryVoltageMin) / ButtonBatteryVoltageStep));
            I2cWrite(Register.BtnBatChgVolSet, val);
        }

        /// <summary>
        /// Gets the button battery charge voltage.
        /// </summary>
        /// <returns>The charge voltage.</returns>
        public ElectricPotential GetButtonBatteryChargeVoltage()
        {
            int val = I2cRead(Register.BtnBatChgVolSet) & 0x07;
            return ElectricPotential.FromMillivolts((val * ButtonBatteryVoltageStep) + ButtonBatteryVoltageMin);
        }

        #endregion

        #region 4.6 — ADC Measurements

        /// <summary>
        /// Gets the battery voltage.
        /// </summary>
        /// <returns>Battery voltage, or 0 if no battery connected.</returns>
        public ElectricPotential GetBatteryVoltage()
        {
            if (!IsBatteryConnected)
            {
                return ElectricPotential.FromMillivolts(0);
            }

            return ElectricPotential.FromMillivolts(ReadAdcH5L8(Register.AdcDataBatteryVoltageHigh, Register.AdcDataBatteryVoltageLow));
        }

        /// <summary>
        /// Gets the VBUS voltage.
        /// </summary>
        /// <returns>VBUS voltage, or 0 if VBUS not present.</returns>
        public ElectricPotential GetVbusVoltage()
        {
            if (!IsVbusGood)
            {
                return ElectricPotential.FromMillivolts(0);
            }

            return ElectricPotential.FromMillivolts(ReadAdcH6L8(Register.AdcDataVbusVoltageHigh, Register.AdcDataVbusVoltageLow));
        }

        /// <summary>
        /// Gets the system voltage.
        /// </summary>
        /// <returns>System rail voltage.</returns>
        public ElectricPotential GetSystemVoltage()
        {
            return ElectricPotential.FromMillivolts(ReadAdcH6L8(Register.AdcDataVsysVoltageHigh, Register.AdcDataVsysVoltageLow));
        }

        /// <summary>
        /// Gets the internal die temperature.
        /// </summary>
        /// <returns>Die temperature.</returns>
        public Temperature GetInternalTemperature()
        {
            int raw = ReadAdcH6L8(Register.AdcDataDieTempHigh, Register.AdcDataDieTempLow);

            // AXP2101 formula: T = 22.0 + (7274 - raw) / 20.0
            double tempC = 22.0 + ((7274 - raw) / 20.0);
            return Temperature.FromDegreesCelsius(tempC);
        }

        /// <summary>
        /// Gets the battery percentage from the fuel gauge.
        /// </summary>
        /// <returns>Battery percentage (0-100), or -1 if no battery connected.</returns>
        public int GetBatteryPercentage()
        {
            if (!IsBatteryConnected)
            {
                return -1;
            }

            return I2cRead(Register.BatPercentData);
        }

        /// <summary>Enables battery voltage ADC measurement.</summary>
        public void EnableBatteryVoltageMeasure() => SetBit(Register.AdcChannelCtrl, 0);

        /// <summary>Disables battery voltage ADC measurement.</summary>
        public void DisableBatteryVoltageMeasure() => ClearBit(Register.AdcChannelCtrl, 0);

        /// <summary>Enables TS pin ADC measurement.</summary>
        public void EnableTsPinMeasure()
        {
            // Set TS pin to battery temperature sensor mode
            byte val = I2cRead(Register.TsPinCtrl);
            val = (byte)((val & 0xE0) | 0x07);
            I2cWrite(Register.TsPinCtrl, val);
            SetBit(Register.AdcChannelCtrl, 1);
        }

        /// <summary>Disables TS pin ADC measurement.</summary>
        public void DisableTsPinMeasure()
        {
            // Set TS pin to external fixed input (doesn't affect charger)
            byte val = I2cRead(Register.TsPinCtrl);
            val = (byte)((val & 0xF0) | 0x10);
            I2cWrite(Register.TsPinCtrl, val);
            ClearBit(Register.AdcChannelCtrl, 1);
        }

        /// <summary>Enables VBUS voltage ADC measurement.</summary>
        public void EnableVbusVoltageMeasure() => SetBit(Register.AdcChannelCtrl, 2);

        /// <summary>Disables VBUS voltage ADC measurement.</summary>
        public void DisableVbusVoltageMeasure() => ClearBit(Register.AdcChannelCtrl, 2);

        /// <summary>Enables system voltage ADC measurement.</summary>
        public void EnableSystemVoltageMeasure() => SetBit(Register.AdcChannelCtrl, 3);

        /// <summary>Disables system voltage ADC measurement.</summary>
        public void DisableSystemVoltageMeasure() => ClearBit(Register.AdcChannelCtrl, 3);

        /// <summary>Enables die temperature ADC measurement.</summary>
        public void EnableTemperatureMeasure() => SetBit(Register.AdcChannelCtrl, 4);

        /// <summary>Disables die temperature ADC measurement.</summary>
        public void DisableTemperatureMeasure() => ClearBit(Register.AdcChannelCtrl, 4);

        #endregion

        #region 4.7 — VBUS Input Limits

        /// <summary>
        /// Sets the VBUS voltage input limit.
        /// </summary>
        /// <param name="limit">The voltage limit setting.</param>
        public void SetVbusVoltageLimit(VbusVoltageLimit limit)
        {
            byte val = I2cRead(Register.InputVolLimitCtrl);
            val = (byte)((val & 0xF0) | ((byte)limit & 0x0F));
            I2cWrite(Register.InputVolLimitCtrl, val);
        }

        /// <summary>
        /// Gets the VBUS voltage input limit setting.
        /// </summary>
        /// <returns>The voltage limit setting.</returns>
        public VbusVoltageLimit GetVbusVoltageLimit()
        {
            return (VbusVoltageLimit)(I2cRead(Register.InputVolLimitCtrl) & 0x0F);
        }

        /// <summary>
        /// Sets the VBUS current input limit.
        /// </summary>
        /// <param name="limit">The current limit setting.</param>
        public void SetVbusCurrentLimit(VbusCurrentLimit limit)
        {
            byte val = I2cRead(Register.InputCurLimitCtrl);
            val = (byte)((val & 0xF8) | ((byte)limit & 0x07));
            I2cWrite(Register.InputCurLimitCtrl, val);
        }

        /// <summary>
        /// Gets the VBUS current input limit setting.
        /// </summary>
        /// <returns>The current limit setting.</returns>
        public VbusCurrentLimit GetVbusCurrentLimit()
        {
            return (VbusCurrentLimit)(I2cRead(Register.InputCurLimitCtrl) & 0x07);
        }

        #endregion

        #region 4.8 — System Power Configuration

        /// <summary>
        /// Sets the minimum system operating voltage (VSYS power-down threshold).
        /// </summary>
        /// <param name="voltage">Voltage (2600-3300 mV, 100 mV steps).</param>
        public void SetSysPowerDownVoltage(ElectricPotential voltage)
        {
            int mv = ClampMillivolts(voltage, SysPowerDownVoltageMin, SysPowerDownVoltageMax);
            byte val = I2cRead(Register.VoffSet);
            val = (byte)((val & 0xF8) | ((mv - SysPowerDownVoltageMin) / SysPowerDownVoltageStep));
            I2cWrite(Register.VoffSet, val);
        }

        /// <summary>
        /// Gets the minimum system operating voltage (VSYS power-down threshold).
        /// </summary>
        /// <returns>The power-down voltage.</returns>
        public ElectricPotential GetSysPowerDownVoltage()
        {
            int val = I2cRead(Register.VoffSet) & 0x07;
            return ElectricPotential.FromMillivolts((val * SysPowerDownVoltageStep) + SysPowerDownVoltageMin);
        }

        /// <summary>
        /// Sets the linear charger Vsys DPM threshold.
        /// </summary>
        /// <param name="dpm">The DPM threshold setting.</param>
        public void SetLinearChargerVsysDpm(LinearChargerVsysDpm dpm)
        {
            byte val = I2cRead(Register.MinSysVolCtrl);
            val = (byte)((val & 0x8F) | (((byte)dpm & 0x07) << 4));
            I2cWrite(Register.MinSysVolCtrl, val);
        }

        /// <summary>
        /// Shuts down the PMIC, turning off all power channels (except VRTC).
        /// </summary>
        public void Shutdown() => SetBit(Register.CommonConfig, 0);

        /// <summary>
        /// Resets the SoC system (POWOFF/POWON cycle and register reset).
        /// </summary>
        public void Reset() => SetBit(Register.CommonConfig, 1);

        /// <summary>
        /// Enables the BATFET (battery output path).
        /// </summary>
        public void EnableBatfet() => SetBit(Register.BatfetCtrl, 3);

        /// <summary>
        /// Disables the BATFET (battery output path).
        /// </summary>
        public void DisableBatfet() => ClearBit(Register.BatfetCtrl, 3);

        #endregion

        #region 4.9 — Low Battery Warning

        /// <summary>
        /// Sets the low battery warning threshold.
        /// </summary>
        /// <param name="percentage">Warning percentage (5-20).</param>
        public void SetLowBatteryWarnThreshold(int percentage)
        {
            percentage = Clamp(percentage, 5, 20);
            byte val = I2cRead(Register.LowBatWarnSet);
            val = (byte)((val & 0x0F) | ((percentage - 5) << 4));
            I2cWrite(Register.LowBatWarnSet, val);
        }

        /// <summary>
        /// Gets the low battery warning threshold.
        /// </summary>
        /// <returns>Warning percentage (5-20).</returns>
        public int GetLowBatteryWarnThreshold()
        {
            return ((I2cRead(Register.LowBatWarnSet) & 0xF0) >> 4) + 5;
        }

        /// <summary>
        /// Sets the low battery shutdown threshold.
        /// </summary>
        /// <param name="percentage">Shutdown percentage (0-15).</param>
        public void SetLowBatteryShutdownThreshold(int percentage)
        {
            percentage = Clamp(percentage, 0, 15);
            byte val = I2cRead(Register.LowBatWarnSet);
            val = (byte)((val & 0xF0) | (percentage & 0x0F));
            I2cWrite(Register.LowBatWarnSet, val);
        }

        /// <summary>
        /// Gets the low battery shutdown threshold.
        /// </summary>
        /// <returns>Shutdown percentage (0-15).</returns>
        public int GetLowBatteryShutdownThreshold()
        {
            return I2cRead(Register.LowBatWarnSet) & 0x0F;
        }

        #endregion

        #region 4.10 — Interrupt Handling

        /// <summary>
        /// Enables the specified interrupt sources.
        /// </summary>
        /// <param name="irq">The interrupt sources to enable.</param>
        public void EnableIrq(Axp2101Irq irq)
        {
            SetInterrupt((int)irq, true);
        }

        /// <summary>
        /// Disables the specified interrupt sources.
        /// </summary>
        /// <param name="irq">The interrupt sources to disable.</param>
        public void DisableIrq(Axp2101Irq irq)
        {
            SetInterrupt((int)irq, false);
        }

        /// <summary>
        /// Gets the current interrupt status.
        /// </summary>
        /// <returns>The interrupt status flags.</returns>
        public Axp2101Irq GetIrqStatus()
        {
            int sts0 = I2cRead(Register.IrqStatus0);
            int sts1 = I2cRead(Register.IrqStatus1);
            int sts2 = I2cRead(Register.IrqStatus2);
            return (Axp2101Irq)((sts2 << 16) | (sts1 << 8) | sts0);
        }

        /// <summary>
        /// Clears all interrupt status flags.
        /// </summary>
        public void ClearIrqStatus()
        {
            I2cWrite(Register.IrqStatus0, 0xFF);
            I2cWrite(Register.IrqStatus1, 0xFF);
            I2cWrite(Register.IrqStatus2, 0xFF);
        }

        /// <summary>Checks if VBUS insert IRQ is active.</summary>
        /// <returns>True if the IRQ fired.</returns>
        public bool IsVbusInsertIrq() => (GetIrqStatus() & Axp2101Irq.VbusInsert) != 0;

        /// <summary>Checks if VBUS remove IRQ is active.</summary>
        /// <returns>True if the IRQ fired.</returns>
        public bool IsVbusRemoveIrq() => (GetIrqStatus() & Axp2101Irq.VbusRemove) != 0;

        /// <summary>Checks if battery insert IRQ is active.</summary>
        /// <returns>True if the IRQ fired.</returns>
        public bool IsBatInsertIrq() => (GetIrqStatus() & Axp2101Irq.BatteryInsert) != 0;

        /// <summary>Checks if battery remove IRQ is active.</summary>
        /// <returns>True if the IRQ fired.</returns>
        public bool IsBatRemoveIrq() => (GetIrqStatus() & Axp2101Irq.BatteryRemove) != 0;

        /// <summary>Checks if battery charge done IRQ is active.</summary>
        /// <returns>True if the IRQ fired.</returns>
        public bool IsBatChargeDoneIrq() => (GetIrqStatus() & Axp2101Irq.BatteryChargeDone) != 0;

        /// <summary>Checks if battery charge start IRQ is active.</summary>
        /// <returns>True if the IRQ fired.</returns>
        public bool IsBatChargeStartIrq() => (GetIrqStatus() & Axp2101Irq.BatteryChargeStart) != 0;

        /// <summary>Checks if power key short press IRQ is active.</summary>
        /// <returns>True if the IRQ fired.</returns>
        public bool IsPowerKeyShortPressIrq() => (GetIrqStatus() & Axp2101Irq.PowerKeyShortPress) != 0;

        /// <summary>Checks if power key long press IRQ is active.</summary>
        /// <returns>True if the IRQ fired.</returns>
        public bool IsPowerKeyLongPressIrq() => (GetIrqStatus() & Axp2101Irq.PowerKeyLongPress) != 0;

        /// <summary>Checks if watchdog expire IRQ is active.</summary>
        /// <returns>True if the IRQ fired.</returns>
        public bool IsWatchdogExpireIrq() => (GetIrqStatus() & Axp2101Irq.WatchdogExpire) != 0;

        #endregion

        #region 4.11 — Watchdog

        /// <summary>
        /// Enables the watchdog timer.
        /// </summary>
        public void EnableWatchdog()
        {
            SetBit(Register.ChargeGaugeWdtCtrl, 0);
            EnableIrq(Axp2101Irq.WatchdogExpire);
        }

        /// <summary>
        /// Disables the watchdog timer.
        /// </summary>
        public void DisableWatchdog()
        {
            DisableIrq(Axp2101Irq.WatchdogExpire);
            ClearBit(Register.ChargeGaugeWdtCtrl, 0);
        }

        /// <summary>
        /// Sets the watchdog configuration (behavior on timeout).
        /// </summary>
        /// <param name="config">The watchdog behavior.</param>
        public void SetWatchdogConfig(WatchdogConfig config)
        {
            byte val = I2cRead(Register.WdtCtrl);
            val = (byte)((val & 0xCF) | (((byte)config & 0x03) << 4));
            I2cWrite(Register.WdtCtrl, val);
        }

        /// <summary>
        /// Sets the watchdog timeout period.
        /// </summary>
        /// <param name="timeout">The timeout period.</param>
        public void SetWatchdogTimeout(WatchdogTimeout timeout)
        {
            byte val = I2cRead(Register.WdtCtrl);
            val = (byte)((val & 0xF8) | ((byte)timeout & 0x07));
            I2cWrite(Register.WdtCtrl, val);
        }

        /// <summary>
        /// Clears (feeds) the watchdog timer.
        /// </summary>
        public void ClearWatchdog() => SetBit(Register.WdtCtrl, 3);

        #endregion

        #region 4.12 — Power Key / Button Timing

        /// <summary>
        /// Sets the power key press-on time (ONLEVEL).
        /// </summary>
        /// <param name="time">Press-on time: 128ms, 512ms, 1s, 2s.</param>
        public void SetPowerKeyPressOnTime(byte time)
        {
            byte val = I2cRead(Register.IrqOffOnLevelCtrl);
            val = (byte)((val & 0xFC) | (time & 0x03));
            I2cWrite(Register.IrqOffOnLevelCtrl, val);
        }

        /// <summary>
        /// Gets the power key press-on time (ONLEVEL).
        /// </summary>
        /// <returns>Press-on time value (0=128ms, 1=512ms, 2=1s, 3=2s).</returns>
        public byte GetPowerKeyPressOnTime()
        {
            return (byte)(I2cRead(Register.IrqOffOnLevelCtrl) & 0x03);
        }

        /// <summary>
        /// Sets the power key press-off time (OFFLEVEL).
        /// </summary>
        /// <param name="time">Press-off time: 0=4s, 1=6s, 2=8s, 3=10s.</param>
        public void SetPowerKeyPressOffTime(byte time)
        {
            byte val = I2cRead(Register.IrqOffOnLevelCtrl);
            val = (byte)((val & 0xF3) | ((time & 0x03) << 2));
            I2cWrite(Register.IrqOffOnLevelCtrl, val);
        }

        /// <summary>
        /// Gets the power key press-off time (OFFLEVEL).
        /// </summary>
        /// <returns>Press-off time value (0=4s, 1=6s, 2=8s, 3=10s).</returns>
        public byte GetPowerKeyPressOffTime()
        {
            return (byte)((I2cRead(Register.IrqOffOnLevelCtrl) & 0x0C) >> 2);
        }

        /// <summary>
        /// Enables long press shutdown.
        /// </summary>
        public void EnableLongPressShutdown() => SetBit(Register.PwroffEn, 1);

        /// <summary>
        /// Disables long press shutdown.
        /// </summary>
        public void DisableLongPressShutdown() => ClearBit(Register.PwroffEn, 1);

        /// <summary>
        /// Sets long press behavior to restart system.
        /// </summary>
        public void SetLongPressRestart() => SetBit(Register.PwroffEn, 0);

        /// <summary>
        /// Sets long press behavior to power off.
        /// </summary>
        public void SetLongPressPowerOff() => ClearBit(Register.PwroffEn, 0);

        #endregion

        #region 4.13 — Sleep & Wakeup

        /// <summary>
        /// Enables sleep mode.
        /// </summary>
        public void EnableSleep() => SetBit(Register.SleepWakeupCtrl, 0);

        /// <summary>
        /// Disables sleep mode.
        /// </summary>
        public void DisableSleep() => ClearBit(Register.SleepWakeupCtrl, 0);

        /// <summary>
        /// Enables wakeup.
        /// </summary>
        public void EnableWakeup() => SetBit(Register.SleepWakeupCtrl, 1);

        /// <summary>
        /// Disables wakeup.
        /// </summary>
        public void DisableWakeup() => ClearBit(Register.SleepWakeupCtrl, 1);

        #endregion

        #region 4.14 — Charge LED

        /// <summary>
        /// Sets the charge LED mode.
        /// </summary>
        /// <param name="mode">The LED mode.</param>
        public void SetChargeLedMode(ChargeLedMode mode)
        {
            byte val = I2cRead(Register.ChgledSetCtrl);

            if (mode == ChargeLedMode.ControlledByCharger)
            {
                // Use charger-controlled mode (Type A)
                val = (byte)((val & 0xF9) | 0x01);
                I2cWrite(Register.ChgledSetCtrl, val);
            }
            else
            {
                // Manual control mode
                val = (byte)(val & 0xC8);
                val |= 0x05; // Manual control enable
                val |= (byte)((byte)mode << 4);
                I2cWrite(Register.ChgledSetCtrl, val);
            }
        }

        /// <summary>
        /// Gets the charge LED mode.
        /// </summary>
        /// <returns>The current LED mode.</returns>
        public ChargeLedMode GetChargeLedMode()
        {
            byte val = I2cRead(Register.ChgledSetCtrl);

            // Check if manual control mode is enabled (bit 2, consistent with SetChargeLedMode)
            if ((val & 0x04) == 0x04)
            {
                // Manual mode: decode mode from bits 5:4 (encoded as mode << 4)
                return (ChargeLedMode)((val >> 4) & 0x03);
            }

            return ChargeLedMode.ControlledByCharger;
        }

        #endregion

        #region 4.15 — DCDC Advanced Settings

        /// <summary>
        /// Sets whether DCDC1 operates in forced PWM mode.
        /// </summary>
        /// <param name="enable">True to force PWM mode.</param>
        public void SetDcDc1WorkModePwm(bool enable) => SetOrClearBit(Register.DcForcePwmCtrl, 2, enable);

        /// <summary>
        /// Sets whether DCDC2 operates in forced PWM mode.
        /// </summary>
        /// <param name="enable">True to force PWM mode.</param>
        public void SetDcDc2WorkModePwm(bool enable) => SetOrClearBit(Register.DcForcePwmCtrl, 3, enable);

        /// <summary>
        /// Sets whether DCDC3 operates in forced PWM mode.
        /// </summary>
        /// <param name="enable">True to force PWM mode.</param>
        public void SetDcDc3WorkModePwm(bool enable) => SetOrClearBit(Register.DcForcePwmCtrl, 4, enable);

        /// <summary>
        /// Sets whether DCDC4 operates in forced PWM mode.
        /// </summary>
        /// <param name="enable">True to force PWM mode.</param>
        public void SetDcDc4WorkModePwm(bool enable) => SetOrClearBit(Register.DcForcePwmCtrl, 5, enable);

        /// <summary>
        /// Enables DCDC 120%/130% high voltage turn off PMIC protection.
        /// </summary>
        public void EnableDcHighVoltageTurnOff() => SetBit(Register.DcOvpUvpCtrl, 5);

        /// <summary>
        /// Disables DCDC 120%/130% high voltage turn off PMIC protection.
        /// </summary>
        public void DisableDcHighVoltageTurnOff() => ClearBit(Register.DcOvpUvpCtrl, 5);

        /// <summary>
        /// Enables continuous conduction mode (CCM).
        /// </summary>
        public void EnableCcm() => SetBit(Register.DcOnOffDvmCtrl, 6);

        /// <summary>
        /// Disables continuous conduction mode (CCM).
        /// </summary>
        public void DisableCcm() => ClearBit(Register.DcOnOffDvmCtrl, 6);

        /// <summary>
        /// Sets the DCDC frequency spread range.
        /// </summary>
        /// <param name="use100kHz">True for 100 kHz range, false for 50 kHz range.</param>
        public void SetDcFreqSpreadRange(bool use100kHz) => SetOrClearBit(Register.DcForcePwmCtrl, 6, use100kHz);

        #endregion

        #region 4.16 — Fast Power-On Sequence

        /// <summary>
        /// Sets the fast power-on startup sequence level for a power channel.
        /// </summary>
        /// <param name="channel">The power channel.</param>
        /// <param name="level">The startup sequence level.</param>
        public void SetFastPowerOnLevel(PowerChannel channel, StartSequenceLevel level)
        {
            byte seqVal = (byte)((byte)level & 0x03);

            switch (channel)
            {
                case PowerChannel.DcDc1:
                    WriteFastPwronBits(Register.FastPwronSet0, 0, seqVal);
                    break;
                case PowerChannel.DcDc2:
                    WriteFastPwronBits(Register.FastPwronSet0, 2, seqVal);
                    break;
                case PowerChannel.DcDc3:
                    WriteFastPwronBits(Register.FastPwronSet0, 4, seqVal);
                    break;
                case PowerChannel.DcDc4:
                    WriteFastPwronBits(Register.FastPwronSet0, 6, seqVal);
                    break;
                case PowerChannel.DcDc5:
                    WriteFastPwronBits(Register.FastPwronSet1, 0, seqVal);
                    break;
                case PowerChannel.Aldo1:
                    WriteFastPwronBits(Register.FastPwronSet1, 2, seqVal);
                    break;
                case PowerChannel.Aldo2:
                    WriteFastPwronBits(Register.FastPwronSet1, 4, seqVal);
                    break;
                case PowerChannel.Aldo3:
                    WriteFastPwronBits(Register.FastPwronSet1, 6, seqVal);
                    break;
                case PowerChannel.Aldo4:
                    WriteFastPwronBits(Register.FastPwronSet2, 0, seqVal);
                    break;
                case PowerChannel.Bldo1:
                    WriteFastPwronBits(Register.FastPwronSet2, 2, seqVal);
                    break;
                case PowerChannel.Bldo2:
                    WriteFastPwronBits(Register.FastPwronSet2, 4, seqVal);
                    break;
                case PowerChannel.CpusLdo:
                    WriteFastPwronBits(Register.FastPwronSet2, 6, seqVal);
                    break;
                case PowerChannel.Dldo1:
                    WriteFastPwronBits(Register.FastPwronCtrl, 0, seqVal);
                    break;
                case PowerChannel.Dldo2:
                    WriteFastPwronBits(Register.FastPwronCtrl, 2, seqVal);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Enables the global fast power-on feature.
        /// </summary>
        public void EnableFastPowerOn() => SetBit(Register.FastPwronCtrl, 7);

        /// <summary>
        /// Disables the global fast power-on feature.
        /// </summary>
        public void DisableFastPowerOn() => ClearBit(Register.FastPwronCtrl, 7);

        /// <summary>
        /// Enables fast wakeup from sleep.
        /// </summary>
        public void EnableFastWakeup() => SetBit(Register.FastPwronCtrl, 6);

        /// <summary>
        /// Disables fast wakeup from sleep.
        /// </summary>
        public void DisableFastWakeup() => ClearBit(Register.FastPwronCtrl, 6);

        #endregion

        #region 4.17 — Data Buffer

        /// <summary>
        /// Writes up to 4 bytes to the AXP2101 data buffer.
        /// </summary>
        /// <param name="data">Data to write (max 4 bytes).</param>
        public void WriteDataBuffer(byte[] data)
        {
            if (data == null || data.Length > DataBufferSize)
            {
                throw new ArgumentException();
            }

            for (int i = 0; i < data.Length; i++)
            {
                I2cWrite((Register)((byte)Register.DataBuffer0 + i), data[i]);
            }
        }

        /// <summary>
        /// Reads up to 4 bytes from the AXP2101 data buffer.
        /// </summary>
        /// <param name="data">Buffer to receive data (max 4 bytes).</param>
        public void ReadDataBuffer(byte[] data)
        {
            if (data == null || data.Length > DataBufferSize)
            {
                throw new ArgumentException();
            }

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = I2cRead((Register)((byte)Register.DataBuffer0 + i));
            }
        }

        #endregion

        #region 4.18 — Fuel Gauge (Advanced)

        /// <summary>
        /// Writes a 128-byte battery profile to the gauge ROM.
        /// </summary>
        /// <param name="data">128-byte battery profile data.</param>
        /// <returns>True if the data was written and verified successfully.</returns>
        public bool WriteGaugeData(byte[] data)
        {
            if (data == null || data.Length != 128)
            {
                return false;
            }

            // Reset gauge first
            SetBit(Register.ResetFuelGauge, 2);
            ClearBit(Register.ResetFuelGauge, 2);

            // Enable ROM register
            ClearBit(Register.FuelGaugeCtrl, 0);
            SetBit(Register.FuelGaugeCtrl, 0);

            // Write data to buffer (serial write to same register)
            for (int i = 0; i < 128; i++)
            {
                I2cWrite(Register.BatParams, data[i]);
            }

            // Re-enable ROM register
            ClearBit(Register.FuelGaugeCtrl, 0);
            SetBit(Register.FuelGaugeCtrl, 0);

            return CompareGaugeData(data);
        }

        /// <summary>
        /// Compares gauge ROM data with the provided data.
        /// </summary>
        /// <param name="data">128-byte battery profile data to compare.</param>
        /// <returns>True if the data matches.</returns>
        public bool CompareGaugeData(byte[] data)
        {
            if (data == null || data.Length != 128)
            {
                return false;
            }

            // Re-enable ROM register
            ClearBit(Register.FuelGaugeCtrl, 0);
            SetBit(Register.FuelGaugeCtrl, 0);

            // Read 128 bytes from gauge
            byte[] buffer = new byte[128];
            for (int i = 0; i < 128; i++)
            {
                buffer[i] = I2cRead(Register.BatParams);
            }

            // Disable ROM register
            ClearBit(Register.FuelGaugeCtrl, 0);

            // Set data interface
            SetBit(Register.FuelGaugeCtrl, 4);

            // Reset gauge
            SetBit(Register.ResetFuelGauge, 2);
            ClearBit(Register.ResetFuelGauge, 2);

            // Compare
            for (int i = 0; i < 128; i++)
            {
                if (buffer[i] != data[i])
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region IDisposable

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2c?.Dispose();
            _i2c = null;
        }

        #endregion

        #region Private I2C Helpers

        private static int ClampMillivolts(ElectricPotential value, int min, int max)
        {
            int mv = (int)value.Millivolts;
            return Clamp(mv, min, max);
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        private void I2cWrite(Register command, byte data)
        {
            _writeBuffer[0] = (byte)command;
            _writeBuffer[1] = data;
            _i2c.Write(_writeBuffer);
        }

        private byte I2cRead(Register command)
        {
            _i2c.WriteByte((byte)command);
            return _i2c.ReadByte();
        }

        private void SetBit(Register register, int bit)
        {
            byte val = I2cRead(register);
            val |= (byte)(1 << bit);
            I2cWrite(register, val);
        }

        private void ClearBit(Register register, int bit)
        {
            byte val = I2cRead(register);
            val &= (byte)~(1 << bit);
            I2cWrite(register, val);
        }

        private bool GetBit(Register register, int bit)
        {
            return (I2cRead(register) & (1 << bit)) != 0;
        }

        private void SetOrClearBit(Register register, int bit, bool set)
        {
            if (set)
            {
                SetBit(register, bit);
            }
            else
            {
                ClearBit(register, bit);
            }
        }

        /// <summary>
        /// Reads a 14-bit ADC value: high byte bits [5:0] as upper 6 bits, low byte as lower 8 bits.
        /// </summary>
        /// <param name="highReg">The register containing the high byte.</param>
        /// <param name="lowReg">The register containing the low byte.</param>
        /// <returns>The combined ADC value.</returns>
        private int ReadAdcH6L8(Register highReg, Register lowReg)
        {
            int high = I2cRead(highReg) & 0x3F;
            int low = I2cRead(lowReg);
            return (high << 8) | low;
        }

        /// <summary>
        /// Reads a 13-bit ADC value: high byte bits [4:0] as upper 5 bits, low byte as lower 8 bits.
        /// </summary>
        /// <param name="highReg">The register containing the high byte.</param>
        /// <param name="lowReg">The register containing the low byte.</param>
        /// <returns>The combined ADC value.</returns>
        private int ReadAdcH5L8(Register highReg, Register lowReg)
        {
            int high = I2cRead(highReg) & 0x1F;
            int low = I2cRead(lowReg);
            return (high << 8) | low;
        }

        private void SetInterrupt(int opts, bool enable)
        {
            if ((opts & 0x0000FF) != 0)
            {
                byte mask = (byte)(opts & 0xFF);
                byte data = I2cRead(Register.IrqEnable0);
                data = enable ? (byte)(data | mask) : (byte)(data & ~mask);
                I2cWrite(Register.IrqEnable0, data);
            }

            if ((opts & 0x00FF00) != 0)
            {
                byte mask = (byte)((opts >> 8) & 0xFF);
                byte data = I2cRead(Register.IrqEnable1);
                data = enable ? (byte)(data | mask) : (byte)(data & ~mask);
                I2cWrite(Register.IrqEnable1, data);
            }

            if ((opts & 0xFF0000) != 0)
            {
                byte mask = (byte)((opts >> 16) & 0xFF);
                byte data = I2cRead(Register.IrqEnable2);
                data = enable ? (byte)(data | mask) : (byte)(data & ~mask);
                I2cWrite(Register.IrqEnable2, data);
            }
        }

        private void WriteFastPwronBits(Register register, int bitShift, byte value)
        {
            byte val = I2cRead(register);
            byte mask = (byte)(0x03 << bitShift);
            val = (byte)((val & ~mask) | ((value & 0x03) << bitShift));
            I2cWrite(register, val);
        }

        #endregion
    }
}
