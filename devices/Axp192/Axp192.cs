// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using UnitsNet;
using UnitsNet.Units;

namespace Iot.Device.Axp192
{
    /// <summary>
    /// Class for handling Axp192 device.
    /// </summary>
    public class Axp192
    {
        private const byte DcD3SetBit = 1 << 1;
        private const byte Ldo2SetBit = 1 << 2;
        private const byte Ldo3SetBit = 1 << 3;
        private const byte ExtEnSetBit = 1 << 6;

        /// <summary>
        /// Default address of I2C Axp192 device.
        /// </summary>
        public const int I2cDefaultAddress = 0x34;

        private I2cDevice _i2c;
        private byte[] _writeBuffer = new byte[2];

        /// <summary>
        /// Initializes a new instance of the <see cref="Axp192" /> class.
        /// </summary>
        /// <param name="i2c">I2C device.</param>
        /// <exception cref="ArgumentNullException">When i2c device is null.</exception>
        public Axp192(I2cDevice i2c)
        {
            _i2c = i2c ?? throw new ArgumentNullException();
        }

        /// <summary>
        /// Gets or sets LDO2 output voltage.
        /// </summary>
        /// <remarks>Range is from 1.8 to 3.3V, steps of 100 mV.</remarks>
        public ElectricPotential LDO2OutputVoltage
        {
            get
            {
                byte buf = I2cRead(Register.VoltageSettingLdo2_3);
                return ElectricPotential.FromVolts(((buf >> 4) / 10.0) + 1.8);
            }

            set
            {
                ElectricPotential volt = GetProperVoltValue(value, 1.8, 3.3);
                byte output = (byte)((volt.Volts - 1.8) * 10.0);
                byte buf = I2cRead(Register.VoltageSettingLdo2_3);
                I2cWrite(Register.VoltageSettingLdo2_3, (byte)((buf & 0x0f) | (output << 4)));
            }
        }

        /// <summary>
        /// Gets or sets LDO3 output voltage.
        /// </summary>
        /// <remarks>Range is from 1.8 to 3.3V, steps of 100 mV.</remarks>
        public ElectricPotential LDO3OutputVoltage
        {
            get
            {
                byte buf = I2cRead(Register.VoltageSettingLdo2_3);
                return ElectricPotential.FromVolts(((buf & 0x0f) / 10.0) + 1.8);
            }

            set
            {
                ElectricPotential volt = GetProperVoltValue(value, 1.8, 3.3);
                byte output = (byte)((volt.Volts - 1.8) * 10.0);
                byte buf = I2cRead(Register.VoltageSettingLdo2_3);
                I2cWrite(Register.VoltageSettingLdo2_3, (byte)((buf & 0xF0) | output));
            }
        }

        /// <summary>
        /// Gets or sets DC-DC2 voltage.
        /// </summary>
        /// <remarks>Range is from 0.7 to 2.275V, steps of 25 mV.</remarks>
        public ElectricPotential DcDc2Voltage
        {
            get
            {
                byte buf = I2cRead(Register.VoltageSettingDcDc2);
                return ElectricPotential.FromVolts((buf * 0.025) + 0.7);
            }

            set
            {
                ElectricPotential volt = GetProperVoltValue(value, 0.7, 2.275);
                byte output = (byte)((volt.Volts - 0.7) / 0.025);
                I2cWrite(Register.VoltageSettingDcDc2, output);
            }
        }

        /// <summary>
        /// Gets or sets DC-DC1 voltage.
        /// </summary>
        /// <remarks>Range is from 0.7 to 3.5V, steps of 25 mV.</remarks>
        public ElectricPotential DcDc1Voltage
        {
            get
            {
                byte buf = I2cRead(Register.VoltageSettingDcDc1);
                return ElectricPotential.FromVolts((buf * 0.025) + 0.7);
            }

            set
            {
                ElectricPotential volt = GetProperVoltValue(value, 0.7, 3.5);
                byte output = (byte)((volt.Volts - 0.7) / 0.025);
                I2cWrite(Register.VoltageSettingDcDc1, output);
            }
        }

        /// <summary>
        /// Gets or sets DC-DC3 voltage.
        /// </summary>
        /// <remarks>Range is from 0.7 to 2.275V, steps of 25 mV.</remarks>
        public ElectricPotential DcDc3Voltage
        {
            get
            {
                byte buf = I2cRead(Register.VoltageSettingDcDc3);
                return ElectricPotential.FromVolts((buf * 0.025) + 0.7);
            }

            set
            {
                ElectricPotential volt = GetProperVoltValue(value, 0.7, 3.5);
                byte output = (byte)((volt.Volts - 0.7) / 0.025);
                I2cWrite(Register.VoltageSettingDcDc3, output);
            }
        }

        private ElectricPotential GetProperVoltValue(ElectricPotential value, double minVal, double maxVal)
        {
            ElectricPotential volt;
            if (value.Volts > maxVal)
            {
                volt = ElectricPotential.FromVolts(maxVal);
            }
            else if (value.Volts < minVal)
            {
                volt = ElectricPotential.FromVolts(minVal);
            }
            else
            {
                volt = value;
            }

            return volt;
        }

        /*
         * Coulomb calculation method: C - 65536 - current LSB ( charging coulomb meter value - discharge coulomb meter value) / 3600 / ADC sample rate.
         * Where: the ADC sample rate refers to the setting of REG84H, the current LSB is 0.5mA, and the calculation is in mAh.
         */

        /// <summary>
        /// Enable Coulomb counter.
        /// </summary>
        public void EnableCoulombCounter() => I2cWrite(Register.CoulombCounter, 0x80);

        /// <summary>
        /// Disable Coulomb counter.
        /// </summary>
        public void DisableCoulombCounter() => I2cWrite(Register.CoulombCounter, 0x00);

        /// <summary>
        /// Stops Coulomb counter.
        /// </summary>            
        public void StopCoulombCounter() => I2cWrite(Register.CoulombCounter, 0xC0);

        /// <summary>
        /// Clear Coulomb counter.
        /// </summary>
        public void ClearCoulombCounter() => I2cWrite(Register.CoulombCounter, 0xA0);

        /// <summary>
        /// Checks if the battery is connected.
        /// </summary>
        /// <returns>True if connected.</returns>
        public bool IsBatteryConnected() => (GetBatteryChargingStatus() & BatteryStatus.BatteryConnected) == BatteryStatus.BatteryConnected;

        /// <summary>
        /// Gets the power status.
        /// </summary>
        /// <returns>The power status.</returns>
        public PowerStatus GetInputPowerStatus() => (PowerStatus)I2cRead(Register.PowerStatus);

        /// <summary>
        ///  Gets battery charging status.
        /// </summary>
        /// <returns>The battery status.</returns>
        public BatteryStatus GetBatteryChargingStatus() => (BatteryStatus)I2cRead(Register.PowerModeChargingStatus);

        private uint GetCoulombCharge() => I2cRead32(Register.CoulombCounterChargingData1);

        private uint GetCoulombDischarge() => I2cRead32(Register.CoulombCounterDischargingData1);

        /// <summary>
        /// Gets Coulomb.
        /// </summary>
        /// <returns>The mA per hour.</returns>
        public double GetCoulomb()
        {
            uint coin = GetCoulombCharge();
            uint coout = GetCoulombDischarge();
            uint valueDifferent = 0;
            bool bIsNegative = false;

            if (coin > coout)
            {   
                // Expected, in always more then out
                valueDifferent = coin - coout;
            }
            else
            {    
                // Warning: Out is more than In, the battery is not started at 0% 
                // just Flip the output sign later
                bIsNegative = true;
                valueDifferent = coout - coin;
            }

            // c = 65536 * current_LSB * (coin - coout) / 3600 / ADC rate
            // Adc rate can be read from 84H, change this variable if you change the ADC reate
            double ccc = (65536 * 0.5 * valueDifferent) / 3600.0 / 200.0;  // Note the ADC has defaulted to be 200 Hz

            if (bIsNegative)
            {
                ccc = 0.0 - ccc;    // Flip it back to negative
            }

            // TODO: migrate the Energy UnitsNet to nanoFramework
            return ccc;
        }

        /*
         * For all the Voltage and Current:
         * Channel 000H STEP FFFH
         * Battery Voltage 0mV 1.1mV 4.5045V
         * Bat discharge current 0mA 0.5mA 4.095A
         * Bat charge current 0mA 0.5mA 4.095A
         * ACIN volatge 0mV 1.7mV 6.9615V
         * ACIN current 0mA 0.625mA 2.5594A
         * VBUS voltage 0mV 1.7mV 6.9615V
         * VBUS current 0mA 0.375mA 1.5356A
         * Internal temperature -144.7℃ 0.1℃ 264.8℃
         * APS voltage 0mV 1.4mV 5.733V
         * TS pin input 0mV 0.8mV 3.276V
         * GPIO0 0/0.7V 0.5mV 2.0475/2.7475V
         * GPIO1 0/0.7V 0.5mV 2.0475/2.7475V
         * GPIO2 0/0.7V 0.5mV 2.0475/2.7475V
         * GPIO3 0/0.7V 0.5mV 2.0475/2.7475V
         */

        /// <summary>
        /// Gets the battery voltage.
        /// </summary>
        /// <returns>The battery voltage.</returns>
        public ElectricPotential GetBatteryVoltage()
        {
            byte[] buf = new byte[2];
            I2cRead(Register.BatteryVoltage8bitsHigh, buf);
            ushort volt = (ushort)((buf[0] << 4) + buf[1]);
            return new ElectricPotential(volt * 1.1, ElectricPotentialUnit.Millivolt);
        }

        /// <summary>
        /// Gets the input voltage.
        /// </summary>
        /// <returns>The input voltage.</returns>
        public ElectricPotential GetInputVoltage()
        {
            byte[] buf = new byte[2];
            I2cRead(Register.InputVoltageAdc8bitsHigh, buf);
            ushort vin = (ushort)((buf[0] << 4) + buf[1]);
            return new ElectricPotential(vin * 1.7, ElectricPotentialUnit.Millivolt);
        }

        /// <summary>
        /// Gets the input current.
        /// </summary>
        /// <returns>The input current.</returns>
        public ElectricCurrent GetInputCurrent()
        {
            ushort iin = 0;
            byte[] buf = new byte[2];
            I2cRead(Register.InputCurrentAdc8bitsHigh, buf);
            iin = (ushort)((buf[0] << 4) + buf[1]);
            return new ElectricCurrent(iin * 0.625, ElectricCurrentUnit.Milliampere);
        }

        /// <summary>
        /// Gets the USB voltage input.
        /// </summary>
        /// <returns>The USB voltage input.</returns>
        public ElectricPotential GetUsbVoltageInput()
        {
            ushort vin = 0;
            byte[] buf = new byte[2];
            I2cRead(Register.UsbVoltageAdc8bitsHigh, buf);
            vin = (ushort)((buf[0] << 4) + buf[1]);
            return new ElectricPotential(vin * 1.7, ElectricPotentialUnit.Millivolt);
        }

        /// <summary>
        /// Gets the USB current.
        /// </summary>
        /// <returns>The USB current.</returns>
        public ElectricCurrent GetUsbCurrentInput()
        {
            byte[] buf = new byte[2];
            I2cRead(Register.UsbCurrentAdc8bitsHigh, buf);
            ushort iin = (ushort)((buf[0] << 4) + buf[1]);
            return new ElectricCurrent(iin * 0.375, ElectricCurrentUnit.Milliampere);
        }

        /// <summary>
        /// Gets the battery charge Current.
        /// </summary>
        /// <returns>The battery charge Current.</returns>
        public ElectricCurrent GetBatteryChargeCurrent()
        {
            ushort icharge = 0;
            byte[] buf = new byte[2];
            I2cRead(Register.BatteryChargeCurrent8bitsHigh, buf);
            icharge = (ushort)((buf[0] << 5) + buf[1]);
            return new ElectricCurrent(icharge * 0.5, ElectricCurrentUnit.Milliampere);
        }

        /// <summary>
        /// Gets the battery discharge current.
        /// </summary>
        /// <returns>The battery discharge current.</returns>
        public ElectricCurrent GetBatteryDischargeCurrent()
        {
            ushort idischarge = 0;
            byte[] buf = new byte[2];
            I2cRead(Register.BatteryDischargeCurrent8bitsHigh, buf);
            idischarge = (ushort)((buf[0] << 5) + buf[1]);
            return new ElectricCurrent(idischarge * 0.5, ElectricCurrentUnit.Milliampere);
        }

        /// <summary>
        /// Gets internal temperature.
        /// </summary>
        /// <returns>The temperature.</returns>
        public Temperature GetInternalTemperature()
        {
            byte[] buf = new byte[2];
            I2cRead(Register.Axp192InternalTemperatureAdc8bitsHigh, buf);
            ushort temp = (ushort)((buf[0] << 4) + buf[1]);
            return new Temperature((temp * 0.1) - 144.7, TemperatureUnit.DegreeCelsius);
        }

        /// <summary>
        /// Gets the battery instantaneous consumption.
        /// </summary>
        /// <returns>The power consumption.</returns>
        public Power GetBatteryInstantaneousPower()
        {
            uint power = 0;
            byte[] buf = new byte[3];
            I2cRead(Register.BatteryInstantaneousPower1, buf);
            power = (ushort)((buf[0] << 16) | (buf[1] << 8) | buf[2]);
            return new Power(power, PowerUnit.Milliwatt);
        }

        /// <summary>
        /// Gets the APS voltage.
        /// </summary>
        /// <returns>The APS voltage.</returns>
        public ElectricPotential GetApsVoltage()
        {
            byte[] buf = new byte[2];
            I2cRead(Register.ApsVoltage8bitsHigh, buf);
            ushort vaps = (ushort)((buf[0] << 4) + buf[1]);
            return new ElectricPotential(vaps * 1.4, ElectricPotentialUnit.Millivolt);
        }

        /// <summary>
        /// Sets the sleep mode.
        /// </summary>
        public void SetSleep()
        {
            I2cWrite(Register.VoltageSettingOff, (byte)(I2cRead(Register.VoltageSettingOff) | (1 << 3))); // Turn on short press to wake up
            I2cWrite(Register.ControlGpio0, (byte)(I2cRead(Register.ControlGpio0) | 0x07)); // GPIO0 floating
            I2cWrite(Register.AdcPin1, 0x00); // Disable ADCs
            I2cWrite(Register.SwitchControleDcDC1_3LDO2_3, (byte)(I2cRead(Register.SwitchControleDcDC1_3LDO2_3) & 0xA1)); // Disable all outputs but DCDC1
        }

        /// <summary>
        /// Is the temperature in warning.
        /// </summary>
        /// <returns>True if internal temperature too high.</returns>
        public bool IsTemperatureWarning() => (I2cRead(Register.IrqStatus4) & 0x01) == 0x01;

        /// <summary>
        /// Get button status.
        /// </summary>
        /// <returns>The state of a button.</returns>
        public ButtonPressed GetButtonStatus()
        {
            // IRQ 3 status.  
            byte state = I2cRead(Register.IrqStatus3);

            if (state != 0)
            {
                // Write 1 back to clear IRQ
                I2cWrite(Register.IrqStatus3, 0x03);
            }

            return (ButtonPressed)(state & 0x03);
        }

        /// <summary>
        /// Sets the button default behavior.
        /// </summary>
        /// <param name="longPress">The long press timing.</param>
        /// <param name="shortPress">The short press timing.</param>
        /// <param name="automaticShutdownAtOvertime">True if automatic shutdown should be processed when over shutdown time.</param>
        /// <param name="signalDelay">The PWROK signal delay after power start-up.</param>
        /// <param name="shutdownTiming">The shutdown timing.</param>
        public void SetButtonBehavior(LongPressTiming longPress, ShortPressTiming shortPress, bool automaticShutdownAtOvertime, SignalDelayAfterPowerUp signalDelay, ShutdownTiming shutdownTiming)
        {
            byte buf = (byte)(automaticShutdownAtOvertime ? 0b0000_1000 : 0b0000_0000);
            buf |= (byte)((byte)longPress | (byte)shortPress | (byte)signalDelay | (byte)shutdownTiming);
            I2cWrite(Register.ParameterSetting, buf);
        }

        /// <summary>
        /// Sets the state of LDO2.
        /// </summary>
        /// <remarks>On M5Stack, can turn LCD Backlight OFF for power saving.</remarks>
        /// <param name="state">True for on/high/1, false for off/low/O.</param>
        public void EnableLDO2(bool state)
        {
            byte buf = I2cRead(Register.SwitchControleDcDC1_3LDO2_3);

            if (state == true)
            {
                buf = (byte)(Ldo2SetBit | buf);
            }
            else
            {
                buf = (byte)(~Ldo2SetBit & buf);
            }

            I2cWrite(Register.SwitchControleDcDC1_3LDO2_3, buf);
        }

        /// <summary>
        /// Sets the state of LDO3.
        /// </summary>
        /// <param name="state">True to enable LDO3.</param>
        public void EnableLDO3(bool state)
        {
            byte buf = I2cRead(Register.SwitchControleDcDC1_3LDO2_3);

            if (state == true)
            {
                buf = (byte)(Ldo3SetBit | buf);
            }
            else
            {
                buf = (byte)(~Ldo3SetBit & buf);
            }

            I2cWrite(Register.SwitchControleDcDC1_3LDO2_3, buf);
        }

        /// <summary>
        /// Sets the state of DC-DC3.
        /// </summary>
        /// <param name="state">True to enable DC-DC3.</param>
        public void EnableDCDC3(bool state)
        {
            byte buf = I2cRead(Register.SwitchControleDcDC1_3LDO2_3);

            if (state == true)
            {
                buf = (byte)(DcD3SetBit | buf);
            }
            else
            {
                buf = (byte)(~DcD3SetBit & buf);
            }

            I2cWrite(Register.SwitchControleDcDC1_3LDO2_3, buf);
        }

        /// <summary>
        /// Sets the state of DC-DC1.
        /// </summary>
        /// <param name="state">True to enable DC-DC1.</param>
        public void EnableDCDC1(bool state)
        {
            byte buf = I2cRead(Register.SwitchControleDcDC1_3LDO2_3);

            if (state == true)
            {
                buf = (byte)(1 | buf);
            }
            else
            {
                buf = (byte)(~1 & buf);
            }

            I2cWrite(Register.SwitchControleDcDC1_3LDO2_3, buf);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the state of EXTEN switch control.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if EXTEN switch is enabled, <see langword="false"/> otherwise.
        /// </value>
        public bool EXTENEnable
        {
            get => (I2cRead(Register.SwitchControleDcDC1_3LDO2_3) & ExtEnSetBit) == ExtEnSetBit;

            set
            {
                byte buf = I2cRead(Register.SwitchControleDcDC1_3LDO2_3);

                if (value)
                {
                    buf |= ExtEnSetBit;
                }
                else
                {
                    buf &= unchecked((byte)~ExtEnSetBit);
                }

                I2cWrite(Register.SwitchControleDcDC1_3LDO2_3, buf);
            }
        }

        /// <summary>
        /// Gets or sets LDO and DC pins.
        /// </summary>
        public LdoDcPinsEnabled LdoDcPinsEnabled
        {
            get => (LdoDcPinsEnabled)(I2cRead(Register.SwitchControleDcDC1_3LDO2_3) & 0b0000_1111);

            set
            {
                byte buf = I2cRead(Register.SwitchControleDcDC1_3LDO2_3);
                buf &= 0b1111_0000;
                buf |= (byte)value;
                I2cWrite(Register.SwitchControleDcDC1_3LDO2_3, buf);
            }
        }

        /// <summary>
        /// Gets or sets the GPIO0 behavior.
        /// </summary>
        public Gpio0Behavior Gpio0Behavior
        {
            get => (Gpio0Behavior)I2cRead(Register.ControlGpio0);
            set => I2cWrite(Register.ControlGpio0, (byte)value);
        }

        /// <summary>
        /// Gets or sets the GPIO1 behavior.
        /// </summary>
        public Gpio12Behavior Gpio1Behavior
        {
            get => (Gpio12Behavior)I2cRead(Register.ControlGpio1);
            set => I2cWrite(Register.ControlGpio1, (byte)value);
        }

        /// <summary>
        /// Gets or sets the GPIO2 behavior.
        /// </summary>
        public Gpio12Behavior Gpio2Behavior
        {
            get => (Gpio12Behavior)I2cRead(Register.ControlGpio2);
            set => I2cWrite(Register.ControlGpio2, (byte)value);
        }

        /// <summary>
        /// Gets or sets the GPIO3 behavior.
        /// </summary>
        public Gpio3Behavior Gpio3Behavior
        {
            get => (Gpio3Behavior)(I2cRead(Register.ControlGpio34) & 0b0000_0011);
            set => I2cWrite(Register.ControlGpio34, (byte)((I2cRead(Register.ControlGpio34) & 0b1111_1100) | (byte)value | 0b1000_0000));
        }

        /// <summary>
        /// Gets or sets the GPIO4 behavior.
        /// </summary>
        public Gpio4Behavior Gpio4Behavior
        {
            get => (Gpio4Behavior)(I2cRead(Register.ControlGpio34) & 0b0000_1100);
            set => I2cWrite(Register.ControlGpio34, (byte)((I2cRead(Register.ControlGpio34) & 0b1111_0011) | (byte)value | 0b1000_0000));
        }

        /// <summary>
        /// Gets or sets the pin value for GPIO0.
        /// </summary>
        public PinValue Gpio0Value
        {
            get => (I2cRead(Register.GpioState012) & 0b0001_0000) == 0b0001_0000 ? PinValue.High : PinValue.Low;
            set => I2cWrite(Register.GpioState012, (byte)(I2cRead(Register.GpioState012) & 0b0000_0110 | (value == PinValue.High ? 0b0000_0001 : 0)));
        }

        /// <summary>
        /// Gets or sets the pin value for GPIO1.
        /// </summary>
        public PinValue Gpio1Value
        {
            get => (I2cRead(Register.GpioState012) & 0b0010_0000) == 0b0010_0000 ? PinValue.High : PinValue.Low;
            set => I2cWrite(Register.GpioState012, (byte)(I2cRead(Register.GpioState012) & 0b0000_0101 | (value == PinValue.High ? 0b0000_0010 : 0)));
        }

        /// <summary>
        /// Gets or sets the pin value for GPIO2.
        /// </summary>
        public PinValue Gpio2Value
        {
            get => (I2cRead(Register.GpioState012) & 0b0100_0000) == 0b0100_0000 ? PinValue.High : PinValue.Low;
            set => I2cWrite(Register.GpioState012, (byte)(I2cRead(Register.GpioState012) & 0b0000_0011 | (value == PinValue.High ? 0b0000_0100 : 0)));
        }

        /// <summary>
        /// Gets or sets the pin value for GPIO3.
        /// </summary>
        public PinValue Gpio3Value
        {
            get => (I2cRead(Register.GpioState34) & 0b0001_0000) == 0b0001_0000 ? PinValue.High : PinValue.Low;
            set => I2cWrite(Register.GpioState34, (byte)(I2cRead(Register.GpioState34) & 0b0000_0010 | (value == PinValue.High ? 0b0000_0001 : 0)));
        }

        /// <summary>
        /// Gets or sets the pin value for GPIO4.
        /// </summary>
        public PinValue Gpio4Value
        {
            get => (I2cRead(Register.GpioState34) & 0b0010_0000) == 0b0010_0000 ? PinValue.High : PinValue.Low;
            set => I2cWrite(Register.GpioState34, (byte)(I2cRead(Register.GpioState34) & 0b0000_0001 | (value == PinValue.High ? 0b0000_0010 : 0)));
        }

        /// <summary>
        /// Sets the high temperature threshold for the battery.
        /// </summary>
        /// <param name="potential">From 0 to 3.264V. Anything higher will be caped to the maximum.</param>
        public void SetBatteryHighTemperatureThreshold(ElectricPotential potential)
        {
            // Docs says Battery high temperature threshold setting when charging, N
            // N * 10H
            // When N-1FH,
            // corresponding to 0.397V;
            // The voltage can be 0V to 3.264V
            byte voltage = (byte)(potential.Volts / 0.0128);
            I2cWrite(Register.HigTemperatureAlarm, voltage);
        }

        /// <summary>
        /// Sets the backup battery charging control.
        /// </summary>
        /// <param name="enabled">Is enabled.</param>
        /// <param name="voltage">Battery charging voltage.</param>
        /// <param name="current">Battery charging current.</param>
        public void SetBackupBatteryChargingControl(bool enabled, BackupBatteryCharingVoltage voltage, BackupBatteryChargingCurrent current)
        {
            byte buf = (byte)(enabled ? 0b1000_0000 : 0);
            buf |= (byte)voltage;
            buf |= (byte)current;
            I2cWrite(Register.BackupBatteryChargingControl, buf);
        }

        /// <summary>
        /// Sets shutdown battery detection control.
        /// </summary>
        /// <param name="turnOffAxp192">True to shutdown the AXP192.</param>
        /// <param name="enabled">True to enable the control.</param>
        /// <param name="function">The pin function.</param>
        /// <param name="pinControl">True to enable the pin function.</param>
        /// <param name="timing">Delay after AXP192 lowered to higher.</param>
        public void SetShutdownBatteryDetectionControl(bool turnOffAxp192, bool enabled, ShutdownBatteryPinFunction function, bool pinControl, ShutdownBatteryTiming timing)
        {
            byte buf = (byte)(turnOffAxp192 ? 0b1000_0000 : 0);
            buf |= (byte)(enabled ? 0b0100_0000 : 0);
            buf |= (byte)function;
            buf |= (byte)(pinControl ? 0b0000_1000 : 0);
            buf |= (byte)timing;
            I2cWrite(Register.ShutdownBatteryDetectionControl, buf);
        }

        /// <summary>
        /// Gets or sets the charging voltage.
        /// </summary>
        public ChargingVoltage ChargingVoltage
        {
            get => (ChargingVoltage)(I2cRead(Register.ChargeControl1) & 0x60);

            set
            {
                byte buf = I2cRead(Register.ChargeControl1);
                buf = (byte)((buf & ~0x60) | ((byte)value & 0x60));
                I2cWrite(Register.ChargeControl1, buf);
            }
        }

        /// <summary>
        /// Gets or sets the charging current.
        /// </summary>
        /// <remarks>Not recommend to set charge current > 100mA, since Battery is only 80mAh.
        /// more then 1C charge-rate may shorten battery life-span.</remarks>
        public ChargingCurrent ChargingCurrent
        {
            get => (ChargingCurrent)(I2cRead(Register.ChargeControl1) & 0x07);

            set
            {
                byte buf = I2cRead(Register.ChargeControl1);
                buf = (byte)((buf & 0xf0) | (byte)value);
                I2cWrite(Register.ChargeControl1, buf);
            }
        }

        /// <summary>
        /// Gets or sets charging threshold when battery should stop charging.
        /// </summary>
        public ChargingStopThreshold ChargingStopThreshold
        {
            get => (ChargingStopThreshold)(I2cRead(Register.ChargeControl1) & 0b0001_0000);

            set
            {
                byte buf = I2cRead(Register.ChargeControl1);
                buf = (byte)((buf & 0b0001_0000) | (byte)((byte)value & 0b0001_0000));
                I2cWrite(Register.ChargeControl1, buf);
            }
        }

        /// <summary>
        /// Set the charging functions.
        /// </summary>
        /// <param name="includeExternal">True to include the external.</param>
        /// <param name="chargingVoltage">Charging voltage.</param>
        /// <param name="chargingCurrent">Charging current.</param>
        /// <param name="stopThreshold">Stop threshold.</param>
        public void SetChargingFunctions(bool includeExternal, ChargingVoltage chargingVoltage, ChargingCurrent chargingCurrent, ChargingStopThreshold stopThreshold)
        {
            byte buf = (byte)(includeExternal ? 0b1000_0000 : 0);
            buf |= (byte)chargingVoltage;
            buf |= (byte)chargingCurrent;
            buf |= (byte)stopThreshold;
            I2cWrite(Register.ChargeControl1, buf);
        }

        /// <summary>
        /// Gets or sets the global pin output voltage.
        /// </summary>
        public PinOutputVoltage PinOutputVoltage
        {
            get => (PinOutputVoltage)(I2cRead(Register.VoltageOutputSettingGpio0Ldo) & 0b1111_0000);

            set
            {
                byte buf = I2cRead(Register.VoltageOutputSettingGpio0Ldo);
                buf = (byte)((buf & 0b1111_0000) | (byte)((byte)value & 0b1111_0000));
                I2cWrite(Register.VoltageOutputSettingGpio0Ldo, buf);
            }
        }

        /// <summary>
        /// Sets the VBUS settings.
        /// </summary>
        /// <param name="vbusIpsOut">The VBUS-IPSOUT path selects the control signal when VBUS is available.</param>
        /// <param name="vbusLimit">True to limit VBUS VHOLD control.</param>
        /// <param name="vholdVoltage">VHOLD Voltage.</param>
        /// <param name="currentLimitEnable">True to limit VBUS current.</param>
        /// <param name="vbusCurrent">VBUS Current limit.</param>
        public void SetVbusSettings(bool vbusIpsOut, bool vbusLimit, VholdVoltage vholdVoltage, bool currentLimitEnable, VbusCurrentLimit vbusCurrent)
        {
            byte buf = (byte)(vbusIpsOut ? 0b1000_0000 : 0);
            buf |= (byte)(vbusLimit ? 0b0100_0000 : 0);
            buf |= (byte)vholdVoltage;
            buf |= (byte)(currentLimitEnable ? 0b0000_0010 : 0);
            buf |= (byte)vbusCurrent;
            I2cWrite(Register.PathSettingVbus, buf);
        }

        /// <summary>
        /// Gets or sets the ADC pin enabled.
        /// </summary>
        public AdcPinEnabled AdcPinEnabled
        {
            get => (AdcPinEnabled)I2cRead(Register.AdcPin1);

            set
            {
                I2cWrite(Register.AdcPin1, (byte)value);
            }
        }

        /// <summary>
        /// Gets or sets power off voltage.
        /// </summary>
        public VoffVoltage VoffVoltage
        {
            get => (VoffVoltage)(I2cRead(Register.VoltageSettingOff) & 0xf8);

            set => I2cWrite(Register.VoltageSettingOff, (byte)((I2cRead(Register.VoltageSettingOff) & 0xf8) | (byte)value));
        }

        /// <summary>
        /// Cut all power, except for LDO1 (RTC).
        /// </summary>
        public void PowerOff()
        {
            I2cWrite(Register.ShutdownBatteryDetectionControl, (byte)(I2cRead(Register.ShutdownBatteryDetectionControl) | 0x80));     // MSB for Power Off
        }

        /// <summary>
        /// Sets the ADC state.
        /// </summary>
        /// <param name="state">True to enable, false to disable.</param>
        public void SetAdcState(bool state)
        {
            I2cWrite(Register.AdcPin1, (byte)(state ? 0xff : 0x00));  // Enable / Disable all ADCs
        }

        /// <summary>
        /// Disable all Irq.
        /// </summary>
        public void DisableAllIRQ()
        {
            I2cWrite(Register.IrqEnable1, 0x00);
            I2cWrite(Register.IrqEnable2, 0x00);
            I2cWrite(Register.IrqEnable3, 0x00);
            I2cWrite(Register.IrqEnable4, 0x00);
            I2cWrite(Register.IrqEnable5, 0x00);
        }

        /// <summary>
        /// Enable the button to be pressed and raise IRQ events.
        /// </summary>
        /// <param name="button">Type of button press event.</param>
        public void EnableButtonPressed(ButtonPressed button)
        {
            byte value = I2cRead(Register.IrqEnable2);
            value &= 0xfc;
            value |= (byte)button;
            I2cWrite(Register.IrqEnable2, value);
        }

        /// <summary>
        /// Clears all Irq.
        /// </summary>
        public void ClearAllIrq()
        {
            I2cWrite(Register.IrqStatus1, 0xff);
            I2cWrite(Register.IrqStatus2, 0xff);
            I2cWrite(Register.IrqStatus3, 0xff);
            I2cWrite(Register.IrqStatus4, 0xff);
            I2cWrite(Register.IrqStatus5, 0xff);
        }

        /// <summary>
        /// Gets or sets the ADC frequency.
        /// </summary>
        public AdcFrequency AdcFrequency
        {
            get => (AdcFrequency)(I2cRead(Register.AdcFrequency) & 0xc0);

            set
            {
                byte buf = I2cRead(Register.AdcFrequency);
                buf = (byte)((buf & ~0xc0) | ((byte)value & 0xc0));
                I2cWrite(Register.AdcFrequency, buf);
            }
        }

        /// <summary>
        /// Gets or sets the ADC Pin output Current.
        /// </summary>
        public AdcPinCurrent AdcPinCurrent
        {
            get => (AdcPinCurrent)(I2cRead(Register.AdcFrequency) & 0b0011_0000);

            set
            {
                byte buf = I2cRead(Register.AdcFrequency);
                buf = (byte)((buf & ~0b0011_0000) | ((byte)value & 0b0011_0000));
                I2cWrite(Register.AdcFrequency, buf);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether ADC battery temperature monitoring function is enabled.
        /// </summary>
        public bool BatteryTemperatureMonitoring
        {
            get => (I2cRead(Register.AdcFrequency) & 0b0000_0100) == 0;

            set
            {
                byte buf = I2cRead(Register.AdcFrequency);
                buf = (byte)((buf & ~0b0000_0100) | (value ? 0 : 0b0000_0100));
                I2cWrite(Register.AdcFrequency, buf);
            }
        }

        /// <summary>
        /// Gets or sets ADC pin current settings.
        /// </summary>
        public AdcPinCurrentSetting AdcPinCurrentSetting
        {
            get => (AdcPinCurrentSetting)(I2cRead(Register.AdcFrequency) & 0b0000_0011);

            set
            {
                byte buf = I2cRead(Register.AdcFrequency);
                buf = (byte)((buf & ~0b0000_0011) | ((byte)value & 0b0000_0011));
                I2cWrite(Register.AdcFrequency, buf);
            }
        }

        /// <summary>
        /// Gets or sets PWM1 output frequency.
        /// </summary>
        /// <remarks>
        /// Default is 0x00.
        /// </remarks>
        public byte Pwm1OutputFrequencySetting
        {
            get => I2cRead(Register.Pwm1OutputFrequencySetting);

            set
            {
                I2cWrite(Register.Pwm1OutputFrequencySetting, value);
            }
        }

        /// <summary>
        /// Gets or sets PWM1 duty cycle setting 1.
        /// </summary>
        /// <remarks>
        /// Default is 0x16.
        /// </remarks>
        public byte Pwm1DutyCycleSetting1
        {
            get => I2cRead(Register.Pwm1DutyCycleSetting1);

            set
            {
                I2cWrite(Register.Pwm1DutyCycleSetting1, value);
            }
        }

        /// <summary>
        /// Gets or sets PWM1 duty cycle setting 2.
        /// </summary>
        /// <remarks>
        /// Default is 0x0B.
        /// </remarks>
        public byte Pwm1DutyCycleSetting2
        {
            get => I2cRead(Register.Pwm1DutyCycleSetting2);

            set
            {
                I2cWrite(Register.Pwm1DutyCycleSetting2, value);
            }
        }

        /// <summary>
        /// Reads the 6 bytes from the storage.
        /// AXP192 have a 6 byte storage, when the power is still valid, the data will not be lost.
        /// </summary>
        /// <param name="buffer">A 6 bytes buffer.</param>
        public void Read6BytesStorage(SpanByte buffer)
        {
            if (buffer.Length != 6)
            {
                throw new ArgumentException();
            }

            // Address from 0x06 - 0x0B
            I2cRead(Register.Storage1, buffer);
        }

        /// <summary>
        /// Stores data in the storage. 6 bytes are available.
        /// AXP192 have a 6 byte storage, when the power is still valid, the data will not be lost.
        /// </summary>
        /// <param name="buffer">A 6 bytes buffer.</param>
        public void Write6BytesStorage(SpanByte buffer)
        {
            if (buffer.Length != 6)
            {
                throw new ArgumentException();
            }

            // Address from 0x06 - 0x0B
            for (byte i = 0; i < buffer.Length; i++)
            {
                _writeBuffer[0] = (byte)((byte)Register.Storage1 + i);
                _writeBuffer[1] = buffer[i];
                _i2c.Write(_writeBuffer);
            }
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

        private void I2cRead(Register command, SpanByte buffer)
        {
            _i2c.WriteByte((byte)command);
            _i2c.Read(buffer);
        }

        private uint I2cRead32(Register command)
        {
            SpanByte buffer = new byte[4];
            _i2c.WriteByte((byte)command);
            _i2c.Read(buffer);
            return (uint)(buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[1]);
        }
    }
}
