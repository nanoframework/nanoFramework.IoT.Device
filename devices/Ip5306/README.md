# IP5306 - Power management

This chip is used mainly in power bank and embedded devices as a power management device. It is connected using I2C and allows to adjust the charging current,voltage, cutoff voltage. It has the capacity to light up 4 leds displaying the charge of the battery. This device is used in the M5Stack.

## Documentation

The datasheet in Chinese can be found [here](https://github.com/m5stack/M5-Schematic/blob/master/Core/IIC_IP5306_REG_V1.4.pdf).

## Usage

As always when using I2C, you need to make sure you're using the right pins. En ESP32, you need the register the pins if not using the default one.

```csharp
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);

I2cDevice i2c = new(new I2cConnectionSettings(1, Ip5306.SecondaryI2cAddress));
Ip5306 power = new(i2c);
```

Note the default address of the IP5306 is 0xEA, the address used in M5Stack is 0x75 (here setup as `SecondaryI2cAddress`).

### Getting access to the properties

All properties can be adjusted and changed. He is an example how to display all of them:

```csharp
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
```

### Button status

You can get the button status:

```csharp
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
```

> **Important**: depending on the behavior you setup for the button, the device may switch off what's connected to it. So be careful on what you setup. Once the I2C connection is broken, you can't adjust the behavior bck.

### configuration for M5Stack

The configuration for M5Stack is the following:

```csharp
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
 ```
 