# CHSC6540 - Touch screen controller

The CHSC6540 is the touch screen controllers of the M5Stack [Tough](https://docs.m5stack.com/en/core/tough).

## Documentation

- [More information](https://github.com/m5stack/M5Tough/blob/master/src/M5Touch.cpp) on the touch controller, unfortunately, the datasheet isn't available from the manufacturer. Only an incomplete shadow copy from a content provider [here](https://szzxv.com/static/upload/file/20220301/1646141990530969.pdf).

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

> Note: this sample requires a M5Stack Tough.
> If you want to use another device, just remove all the related NuGet packages.

```csharp
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Chsc6540;
using nanoFramework.Hardware.Esp32;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new(1, Chsc6540.DefaultI2cAddress);
using I2cDevice device = I2cDevice.Create(settings);
using GpioController gpio = new();

using Chsc6540 sensor = new(device);
sensor.SetInterruptMode(true);

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
