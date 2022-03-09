// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Axp192;
using Iot.Device.Ft6xx6x;
using nanoFramework.Hardware.Esp32;
using UnitsNet;

// Note: this sample requires a M5Core2.
// If you want to use another device, just remove all the related nugets
// And comments the following line
// You will need as well to set the pins if needed.
// Setup first the I2C bus
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);

// Create the energy management device
I2cDevice i2c = new(new I2cConnectionSettings(1, Axp192.I2cDefaultAddress));
Axp192 _power = new(i2c);

// Configuration for M5Core2
// AXP Vbus limit off
_power.SetVbusSettings(false, false, VholdVoltage.V4_0, true, VbusCurrentLimit.MilliAmper100);
// AXP192 GPIO1 and 2:OD OUTPUT
_power.Gpio1Behavior = Gpio12Behavior.MnosLeakOpenOutput;
_power.Gpio2Behavior = Gpio12Behavior.MnosLeakOpenOutput;
// Enable RTC BAT charge 
_power.SetBackupBatteryChargingControl(true, BackupBatteryCharingVoltage.V3_0, BackupBatteryChargingCurrent.MicroAmperes200);
// Sets the ESP voltage
_power.DcDc1Volvate = ElectricPotential.FromVolts(3.35);
// Sets the LCD Voltage to 2.8V
_power.DcDc3Volvate = ElectricPotential.FromVolts(2.8);
// Sets the SD Card voltage
_power.LDO2OutputVoltage = ElectricPotential.FromVolts(3.3);
_power.EnableLDO2(true);
// Sets the Vibrator voltage
_power.LDO3OutputVoltage = ElectricPotential.FromVolts(2.0);
// Bat charge voltage to 4.2, Current 100MA
_power.SetChargingFunctions(true, ChargingVoltage.V4_2, ChargingCurrent.Current100mA, ChargingStopThreshold.Percent10);
// Set ADC sample rate to 200hz
_power.AdcFrequency = AdcFrequency.Frequency200Hz;
_power.AdcPinCurrent = AdcPinCurrent.MicroAmperes80;
_power.BatteryTemperatureMonitoring = true;
_power.AdcPinCurrentSetting = AdcPinCurrentSetting.AlwaysOn;
// Set ADC1 Enable
_power.AdcPinEnabled = AdcPinEnabled.All;

// Set GPIO4 as output (rest LCD)
_power.Gpio4Behavior = Gpio4Behavior.MnosLeakOpenOutput;
// 128ms power on, 4s power off
_power.SetButtonBehavior(LongPressTiming.S1, ShortPressTiming.Ms128, true, SignalDelayAfterPowerUp.Ms64, ShutdownTiming.S10);
// Set temperature protection
_power.SetBatteryHighTemperatureThreshold(ElectricPotential.FromVolts(3.2256));
// Enable bat detection
_power.SetShutdownBatteryDetectionControl(false, true, ShutdownBatteryPinFunction.HighResistance, true, ShutdownBatteryTiming.S2);
// Set Power off voltage 3.0v            
_power.VoffVoltage = VoffVoltage.V3_0;
// This part of the code will handle the button behavior
_power.EnableButtonPressed(ButtonPressed.LongPressed | ButtonPressed.ShortPressed);

// Reset
_power.Gpio4Value = PinValue.Low;
Thread.Sleep(100);
_power.Gpio4Value = PinValue.High;
Thread.Sleep(100);
// Switch on the screen
_power.EnableDCDC3(true);
_power.EnableLDO2(true);
_power.LDO3OutputVoltage = ElectricPotential.FromVolts(3);

I2cConnectionSettings settings = new(1, Ft6xx6x.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);
using GpioController gpio = new();

using Ft6xx6x sensor = new(device);
var ver = sensor.GetVersion();
Debug.WriteLine($"version: {ver}");
sensor.SetInterruptMode(false);
Debug.WriteLine($"Period active: {sensor.PeriodActive}");
Debug.WriteLine($"Period active in monitor mode: {sensor.MonitorModePeriodActive}");
Debug.WriteLine($"Time to enter monitor: {sensor.MonitorModeDelaySeconds} seconds");
Debug.WriteLine($"Monitor mode: {sensor.MonitorModeEnabled}");
Debug.WriteLine($"Proximity sensing: {sensor.ProximitySensingEnabled}");

gpio.OpenPin(39, PinMode.Input);
// This will enable an event on GPIO39 on falling edge when the screen if touched
gpio.RegisterCallbackForPinValueChangedEvent(39, PinEventTypes.Falling, TouchInterrupCallback);

while (true)
{
    Thread.Sleep(20);
}

void TouchInterrupCallback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
{
    Debug.WriteLine("Touch interrupt");
    var points = sensor.GetNumberPoints();
    if (points == 1)
    {
        var point = sensor.GetPoint(true);
        Debug.WriteLine($"ID: {point.TouchId}, X: {point.X}, Y: {point.Y}, Weight: {point.Weigth}, Misc: {point.Miscelaneous}, Event: {point.Event}");
    }
    else if (points == 2)
    {
        var dp = sensor.GetDoublePoints();
        Debug.WriteLine($"ID: {dp.Point1.TouchId}, X: {dp.Point1.X}, Y: {dp.Point1.Y}, Weight: {dp.Point1.Weigth}, Misc: {dp.Point1.Miscelaneous}, Event: {dp.Point1.Event}");
        Debug.WriteLine($"ID: {dp.Point2.TouchId}, X: {dp.Point2.X}, Y: {dp.Point2.Y}, Weight: {dp.Point2.Weigth}, Misc: {dp.Point2.Miscelaneous}, Event: {dp.Point1.Event}");
    }
}