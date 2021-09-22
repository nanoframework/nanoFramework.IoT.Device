using Iot.Device.Ip5306;
using nanoFramework.Hardware.Esp32;
using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using UnitsNet;

Debug.WriteLine("Hello from IP5306!");

Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);

I2cDevice i2c = new(new I2cConnectionSettings(1, Ip5306.SecondaryI2cAddress));
Ip5306 power = new(i2c);

Debug.WriteLine($"Current configuration:");
DisplayInfo();

// Configuration for M5Stack
power.ButtonOffEnabled = true;
power.BoostOutputEnabled = false;
power.AutoPowerOnEnabled = true;
power.ChargerEnabled = true;
power.BoostEnabled = true;
power.LowPowerOffEnabled = true;
power.FlashLightBehavior = ButtonPress.Doubleclick;
power.SwitchOffBoostBehavior = ButtonPress.LongPress;
power.BoostWhenVinUnpluggedEnabled = true;
power.ChargingUnderVoltage = ChargingUnderVoltage.V4_55;
power.ChargingLoopSelection = ChargingLoopSelection.Vin;
power.ChargingCurrent = ElectricCurrent.FromMilliamperes(2250);
power.ConstantChargingVoltage = ConstantChargingVoltage.Vm28;
power.ChargingCuttOffVoltage = ChargingCutOffVoltage.V4_17;
power.LightDutyShutdownTime = LightDutyShutdownTime.S32;
power.ChargingCutOffCurrent = ChargingCutOffCurrent.C500mA;
power.ChargingCuttOffVoltage = ChargingCutOffVoltage.V4_2;

Debug.WriteLine($"Current for M5Stack:");
DisplayInfo();

Debug.WriteLine("Careful when pressing buttons, the behavior can switch off the board.");
while(true)
{
    var button = power.GetButtonStatus();
    switch (button)
    {
        case ButtonPressed.DoubleClicked:
            Debug.WriteLine("double clicked");
            break;
        case ButtonPressed.LongPressed:
            Debug.WriteLine("Long pressed");
            break;
        case ButtonPressed.ShortPressed:
            Debug.WriteLine("Short pressed");
            break;
        case ButtonPressed.NotPressed:
        default:
            break;
    }

    Thread.Sleep(200);
}

Thread.Sleep(Timeout.Infinite);

void DisplayInfo()
{
    Debug.WriteLine($"  AutoPowerOnEnabled: {power.AutoPowerOnEnabled}");
    Debug.WriteLine($"  BoostOutputEnabled: {power.BoostOutputEnabled}");
    Debug.WriteLine($"  BoostWhenVinUnpluggedEnabled: {power.BoostWhenVinUnpluggedEnabled}");
    Debug.WriteLine($"  BostEnabled: {power.BoostEnabled}");
    Debug.WriteLine($"  ButtonOffEnabled: {power.ButtonOffEnabled}");
    Debug.WriteLine($"  ChargerEnabled: {power.ChargerEnabled}");
    Debug.WriteLine($"  ChargingBatteryVoltage: {power.ChargingBatteryVoltage}");
    Debug.WriteLine($"  ChargingCurrent: {power.ChargingCurrent}");
    Debug.WriteLine($"  ChargingCutOffCurrent: {power.ChargingCutOffCurrent}");
    Debug.WriteLine($"  ChargingCuttOffVoltage{power.ChargingCuttOffVoltage}");
    Debug.WriteLine($"  ChargingLoopSelection: {power.ChargingLoopSelection}");
    Debug.WriteLine($"  ChargingUnderVoltage: {power.ChargingUnderVoltage}");
    Debug.WriteLine($"  ConstantChargingVoltage: {power.ConstantChargingVoltage}");
    Debug.WriteLine($"  FlashLightBehavior {power.FlashLightBehavior}");
    Debug.WriteLine($"  IsBatteryFull: {power.IsBatteryFull}");
    Debug.WriteLine($"  IsCharging: {power.IsCharging}");
    Debug.WriteLine($"  IsOutputLoadHigh: {power.IsOutputLoadHigh}");
    Debug.WriteLine($"  LightDutyShutdownTime: {power.LightDutyShutdownTime}");
    Debug.WriteLine($"  LowPowerOffEnabled: {power.LowPowerOffEnabled}");
    Debug.WriteLine($"  ShortPressToSwitchBosst: {power.ShortPressToSwitchBosst}");
    Debug.WriteLine($"  SwitchOffBoostBehavior: {power.SwitchOffBoostBehavior}");
    Debug.WriteLine($"  GetButtonStatus: {power.GetButtonStatus()}");
}
