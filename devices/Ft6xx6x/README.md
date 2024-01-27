# Ft6xx6x/Ft6336GU - Touch screen controller

The FT6xx6x are touch screens controllers. [M5Core2](https://github.com/nanoframework/nanoFramework.M5Stack) has a FT6336U. This driver supports the variants FT6236G, FT6336G , FT6336U and FT6426. The sample has been built and tested with a [M5Core2](https://github.com/nanoframework/nanoFramework.M5Stack).

## Documentation

- You can find [the registers here](https://m5stack.oss-cn-shenzhen.aliyuncs.com/resource/docs/datasheet/core/Ft6336GU_Firmware%20%E5%A4%96%E9%83%A8%E5%AF%84%E5%AD%98%E5%99%A8_20151112-%20EN.xlsx).
- [More information](https://www.buydisplay.com/download/ic/FT6236-FT6336-FT6436L-FT6436_Datasheet.pdf) on the touch controller, events, gesture.

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

> Note: this sample requires a M5Core2.
> If you want to use another device, just remove all the related nugets.

```csharp
M5Core2.InitializeScreen();
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
                // Some controllers supports as well events, you can get access to them as well thru point.Event
        Debug.WriteLine($"ID: {point.TouchId}, X: {point.X}, Y: {point.Y}, Weight: {point.Weigth}, Misc: {point.Miscelaneous}");
    }
    else if (points == 2)
    {
        var dp = sensor.GetDoublePoints();
        Debug.WriteLine($"ID: {dp.Point1.TouchId}, X: {dp.Point1.X}, Y: {dp.Point1.Y}, Weight: {dp.Point1.Weigth}, Misc: {dp.Point1.Miscelaneous}");
        Debug.WriteLine($"ID: {dp.Point2.TouchId}, X: {dp.Point2.X}, Y: {dp.Point2.Y}, Weight: {dp.Point2.Weigth}, Misc: {dp.Point2.Miscelaneous}");
    }
}
```
