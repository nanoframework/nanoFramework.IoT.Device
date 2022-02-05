﻿# AM2320 - Temperature and Humidity sensor

AM2320 is a temperature and humidity sensor, sensible to 0.1 degree and 0.1 relative humidity.

## Documentation

- [AM2320](https://cdn-shop.adafruit.com/product-files/3721/AM2320.pdf)

## Usage

**Important**: make sure you properly setup the I2C pins especially for ESP32 before creating the `I2cDevice`, make sure you install the `nanoFramework.Hardware.ESP32 nuget`:

```csharp
//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
```

For other devices like STM32, please make sure you're using the pre-set pins for the SPI bus you want to use. The chip select can as well be pre setup.

Here is an example how to use the AM2320:

```csharp
using Iot.Device.Am2320;
using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

Debug.WriteLine("Hello from AM2320!");

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

using Am2320 am2330 = new(new I2cDevice(new I2cConnectionSettings(1, Am2320.DefaultI2cAddress, I2cBusSpeed.StandardMode)));

while(true)
{
    var temp = am2330.Temperature;
    var hum = am2330.Humidity;
    if(am2330.IsLastReadSuccessful)
    {
        Debug.WriteLine($"Temp = {temp.DegreesCelsius} C, Hum = {hum.Percent} %");
    }
    else
    {
        Debug.WriteLine("Not sucessfull read");
    }

    Thread.Sleep(Am2320.MinimumReadPeriod);
}
```

### Device Information

You can read the Device Information.

> Note: on some copies, the device information only returns 0.

```csharp
// On some copies, the device information contains only 0
var deviceInfo = am2330.DeviceInformation;
if (deviceInfo != null)
{
    Debug.WriteLine($"Model: {deviceInfo.Model}");
    Debug.WriteLine($"Version: {deviceInfo.Version}");
    Debug.WriteLine($"Device ID: {deviceInfo.DeviceId}");
}
```

## Limitations

Only the I2C implementation is available, not the 1 wire one.

The user registers and the status register are not implemented. The status register is just a register the user can store data. It is not currently used for any usage according to the documentation.
