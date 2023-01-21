# DS3231 - Realtime Clock
DS3231 is a low-cost, extremely accurate I2C realtime clock (RTC) with an integrated temperature compensated crystal oscillator (TCXO) and crystal.

## Sensor Image

![sensor](./sensor.jpg)

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
I2cConnectionSettings settings = new I2cConnectionSettings(1, Ds3231.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using (Ds3231 rtc = new Ds3231(device))
{
    // set DS3231 time
    rtc.DateTime = DateTime.Now;
    // read time
    DateTime dt = rtc.DateTime;
    // read temperature
    double temp = rtc.Temperature.Celsius;
}
```
