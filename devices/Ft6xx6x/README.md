# Ft6xx6x - Touch screen

TBD Touch screen, M5Core2.

## Documentation

- TBD

![sensor](sensor.jpg)

## Usage

**Important**: make sure you properly setup the I2C pins especially for ESP32 before creating the `I2cDevice`, make sure you install the `nanoFramework.Hardware.ESP32 nuget`:

```csharp
//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
```

For other devices like STM32, please make sure you're using the preset pins for the I2C bus you want to use.

```csharp
I2cConnectionSettings settings = new I2cConnectionSettings(1, Ft6xx6x.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using(Ft6xx6x sensor = new Ft6xx6x(device))
{
    // TODO
```
