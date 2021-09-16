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
    Debug.WriteLine($"Internal temperature: {ag.GetInternalTemperature().DegreesCelsius} C");

    while (true)
    {
        var acc = ag.GetAccelerometer();
        var gyr = ag.GetGyroscope();
        Debug.WriteLine($"Accelerometer data x:{acc.X} y:{acc.Y} z:{acc.Z}");
        Debug.WriteLine($"Gyroscope data x:{gyr.X} y:{gyr.Y} z:{gyr.Z}\n");
        Thread.Sleep(100);
    }
}
```

### Sample output

```text
Internal temperature: 64.81028151 C
Accelerometer data x:11.92480468 y:15.99902343 z:2.27246093
Gyroscope data x:4.94384765 y:3985.9008789 z:271.484375

Accelerometer data x:11.90234375 y:15.984375 z:2.28320312
Gyroscope data x:3.96728515 y:3985.47363281 z:271.24023437

Accelerometer data x:11.87988281 y:15.96386718 z:2.27148437
Gyroscope data x:5.49316406 y:3985.9008789 z:272.82714843

Accelerometer data x:11.9140625 y:15.94726562 z:2.32421875
Gyroscope data x:2.62451171 y:3986.51123046 z:273.31542968

Accelerometer data x:11.87695312 y:15.9765625 z:2.31445312
Gyroscope data x:3.54003906 y:3987.3046875 z:273.80371093
```

See [samples](samples) for a complete sample application.
