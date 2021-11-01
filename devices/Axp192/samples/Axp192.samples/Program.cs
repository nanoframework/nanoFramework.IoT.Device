// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Axp192;
using nanoFramework.Hardware.Esp32;
using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using UnitsNet;

Debug.WriteLine("Hello from AXP192!");
Debug.WriteLine("This is the sequence to power on the Axp192 for M5 Stick");

Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);

I2cDevice i2cAxp192 = new(new I2cConnectionSettings(1, Axp192.I2cDefaultAddress));
Axp192 power = new Axp192(i2cAxp192);

// NOTE: the comments include code which was originally used
// to setup the AXP192 and can be found in the M5Stick repository
// This allows to understand the selection dome.
// Set LDO2 & LDO3(TFT_LED & TFT) 3.0V
// I2cWrite(Register.VoltageSettingLdo2_3, 0xcc);
power.SetLDO2Output(0xC);
power.SetLDO3Output(0xC);
// Set ADC sample rate to 200hz
// I2cWrite(Register.AdcFrequency, 0xF2);
power.AdcFrequency = AdcFrequency.Frequency200Hz;
power.AdcPinCurrent = AdcPinCurrent.MicroAmperes80;
power.BatteryTemperatureMonitoring = true;
power.AdcPinCurrentSetting = AdcPinCurrentSetting.AlwaysOn;
// Set ADC to All Enable
// I2cWrite(Register.AdcPin1, 0xff);
power.AdcPinEnabled = AdcPinEnabled.All;
// Bat charge voltage to 4.2, Current 100MA
// I2cWrite(Register.ChargeControl1, 0xc0);
power.SetChargingFunctions(true, ChargingVoltage.V4_2, ChargingCurrent.Current100mA, ChargingStopThreshold.Percent10);
// Depending on configuration enable LDO2, LDO3, DCDC1, DCDC3.
// byte data = I2cRead(Register.SwitchControleDcDC1_3LDO2_3);
// data = (byte)((data & 0xEF) | 0x4D);
// I2cWrite(Register.SwitchControleDcDC1_3LDO2_3, data);
power.LdoDcPinsEnabled = LdoDcPinsEnabled.All;
// 128ms power on, 4s power off
// I2cWrite(Register.ParameterSetting, 0x0C);
power.SetButtonBehavior(LongPressTiming.S1, ShortPressTiming.Ms128, true, SignalDelayAfterPowerUp.Ms64, ShutdownTiming.S10);
// Set RTC voltage to 3.3V
// I2cWrite(Register.VoltageOutputSettingGpio0Ldo, 0xF0);
power.PinOutputVoltage = PinOutputVoltage.V3_3;
// Set GPIO0 to LDO
// I2cWrite(Register.ControlGpio0, 0x02);
power.Gpio0Behavior = Gpio0Behavior.LowNoiseLDO;
// Disable vbus hold limit
// I2cWrite(Register.PathSettingVbus, 0x80);
power.SetVbusSettings(true, false, VholdVoltage.V4_0, false, VbusCurrentLimit.MilliAmper500);
// Set temperature protection
// I2cWrite(Register.HigTemperatureAlarm, 0xfc);
power.SetBatteryHighTemperatureThreshold(ElectricPotential.FromVolts(3.2256));
// Enable RTC BAT charge 
// I2cWrite(Register.BackupBatteryChargingControl, 0xa2);
power.SetBackupBatteryChargingControl(true, BackupBatteryCharingVoltage.V3_0, BackupBatteryChargingCurrent.MicroAmperes200);
// Enable bat detection
// I2cWrite(Register.ShutdownBatteryDetectionControl, 0x46);
// Note 0x46 is not a possible value, most likely 0x4A
power.SetShutdownBatteryDetectionControl(false, true, ShutdownBatteryPinFunction.HighResistance, true, ShutdownBatteryTiming.S2);
// Set Power off voltage 3.0v            
// data = I2cRead(Register.VoltageSettingOff);
// data = (byte)((data & 0xF8) | (1 << 2));
// I2cWrite(Register.VoltageSettingOff, data);
power.VoffVoltage = VoffVoltage.V3_0;

// This part of the code will handle the button behavior
power.EnableButtonPressed(ButtonPressed.LongPressed | ButtonPressed.ShortPressed);
power.SetButtonBehavior(LongPressTiming.S2, ShortPressTiming.Ms128, true, SignalDelayAfterPowerUp.Ms32, ShutdownTiming.S10);

DateTime dt = DateTime.UtcNow.AddSeconds(5);
while (true)
{
    var status = power.GetButtonStatus();
    if ((status & ButtonPressed.ShortPressed) == ButtonPressed.ShortPressed)
    {
        Debug.WriteLine("Short press");
    }
    else if ((status & ButtonPressed.LongPressed) == ButtonPressed.LongPressed)
    {
        Debug.WriteLine("Long press");
    }

    if (DateTime.UtcNow > dt)
    {
        Debug.WriteLine($"Temperature : {power.GetInternalTemperature().DegreesCelsius} �C");
        Debug.WriteLine($"Input:");
        // Note: the current and voltage will show 0 when plugged into USB.
        // To see something else than 0, you should output those data on a serial port for example
        // Or display on the screen.
        Debug.WriteLine($"  Current   : {power.GetInputCurrent().Milliamperes} mA");
        Debug.WriteLine($"  Voltage   : {power.GetInputVoltage().Volts} V");
        Debug.WriteLine($"  Status    : {power.GetInputPowerStatus()}");
        Debug.WriteLine($"  USB volt  : {power.GetUsbVoltageInput().Volts} V");
        Debug.WriteLine($"  USB Curr  : {power.GetUsbCurrentInput().Milliamperes} mA");
        Debug.WriteLine($"Battery:");
        Debug.WriteLine($"  Charge curr  : {power.GetBatteryChargeCurrent().Milliamperes} mA");
        Debug.WriteLine($"  Status       : {power.GetBatteryChargingStatus()}");
        Debug.WriteLine($"  Dicharge curr: {power.GetBatteryDischargeCurrent().Milliamperes} mA");
        Debug.WriteLine($"  Inst Power   : {power.GetBatteryInstantaneousPower().Milliwatts} mW");
        Debug.WriteLine($"  Voltage      : {power.GetBatteryVoltage().Volts} V");
        Debug.WriteLine($"  Is battery   : {power.IsBatteryConnected()} ");
        dt = DateTime.UtcNow.AddSeconds(10);
    }

    Thread.Sleep(100);
}

Thread.Sleep(Timeout.Infinite);
