# Lp3943 LED driver

The LP3943 is an integrated device capable of independently driving 16 LEDs. It uses I2C.

## Documentation

- LM75 [datasheet](https://www.ti.com/lit/ds/symlink/lp3943.pdf)

![Lp3943 eval board example back side](https://raw.githubusercontent.com/nanoframework/nanoFramework.IoT.Device/develop/devices/Lp3943/Lp3943_eval_back.png)
![Lp3943 eval board example front side](https://raw.githubusercontent.com/nanoframework/nanoFramework.IoT.Device/develop/devices/Lp3943/Lp3943_eval_front.png)

## Usage

**Important**: make sure you properly setup the I2C pins especially for ESP32 before creating the `I2cDevice`, make sure you install the `nanoFramework.Hardware.ESP32 nuget`:

```cs
//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
```

For other devices like STM32, please make sure you're using the preset pins for the I2C bus you want to use.

```cs
I2cConnectionSettings settings = new I2cConnectionSettings(1, Lp3943.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using(var ledDriver = new Lp3943(device))
{
    ledDriver.SetLed(0, LedState.On);
}
```
