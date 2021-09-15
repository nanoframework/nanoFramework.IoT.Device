# Mpu6886 - accelerometer and gyroscope

## Documentation

- [Datasheet](https://m5stack.oss-cn-shenzhen.aliyuncs.com/resource/docs/datasheet/core/MPU-6886-000193%2Bv1.1_GHIC_en.pdf), register descriptions start at page 32.
- This sensor is used in for example the [M5StickC Plus development kit](https://shop.m5stack.com/products/m5stickc-plus-esp32-pico-mini-iot-development-kit), initialization code can be found [here](https://github.com/m5stack/M5StickC/blob/3e00ecfa0897a432995a3f80874715cbe6ad60ee/src/utility/MPU6886.cpp#L32).

## Usage

```csharp
// I2C pins need to be configured, for example for pin 22 & 21 for 
// the M5StickC Plus. These pins might be different for other boards.

Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);

I2cConnectionSettings settings = new(1, 0x68);

using (Mpu6886AccelerometerGyroscope ag = new(I2cDevice.Create(settings)))
{
    Debug.WriteLine($"Internal temperature: {ag.GetTemperature()} C");

    while (true)
    {
        var acc = ag.GetAccelerometer();
        var gyr = ag.GetGyroscope();
        Debug.WriteLine($"Accelerometer data x:{acc.X} y:{acc.Y} z:{acc.Z}");
        Debug.WriteLine($"Gyroscope data x:{gyr.X} y:{gyr.Y} z:{gyr.Z}");
        Thread.Sleep(100);
    }
}
```

See [samples](samples/README.md) for a complete sample application.
