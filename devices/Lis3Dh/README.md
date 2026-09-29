# LIS3DH - ultra-low-power high-performance 3-axis accelerometer

The LIS3DH is an ultra-low-power, high-performance three-axis linear accelerometer from STMicroelectronics. It supports selectable plus or minus 2 g, 4 g, 8 g, and 16 g full-scale ranges. The sensor supports I2C and SPI; this binding currently supports I2C.

## Documentation

- [LIS3DH datasheet](https://www.st.com/resource/en/datasheet/lis3dh.pdf)

## Wiring

Connect VIN and GND according to the breakout board requirements, then connect SDA and SCL to the selected I2C bus. Tie SDO/SA0 low to use `DefaultI2cAddress` (`0x18`) or high to use `SecondaryI2cAddress` (`0x19`).

On ESP32, configure the I2C pins before creating the device:

```csharp
Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);
```

For STM32 and other targets, use the hardware I2C pins configured by the firmware.

## Usage

```csharp
using Iot.Device.Lis3DhAccelerometer;
using System.Device.I2c;
using System.Diagnostics;
using System.Numerics;
using System.Threading;

I2cConnectionSettings settings = new(1, Lis3Dh.DefaultI2cAddress);
using I2cDevice i2cDevice = I2cDevice.Create(settings);
using Lis3Dh accelerometer = Lis3Dh.Create(
    i2cDevice,
    DataRate.DataRate10Hz,
    OperatingMode.HighResolutionMode,
    AccelerationScale.Scale04G);

while (true)
{
    Vector3 acceleration = accelerometer.Acceleration;
    Debug.WriteLine($"X: {acceleration.X:F4} g, Y: {acceleration.Y:F4} g, Z: {acceleration.Z:F4} g");
    Thread.Sleep(100);
}
```

`Acceleration` returns a `Vector3` whose components are measured in gravitational force (g).