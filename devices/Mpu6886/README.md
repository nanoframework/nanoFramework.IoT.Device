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
    Debug.WriteLine("Start calibration ...");
    var offset = ag.Calibrate(1000);
    Debug.WriteLine($"Calibration done, calculated offsets {offset.X} {offset.Y} {offset.Y}");

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
Start calibration ...
Calibration done, calculated offsets 49.189 -86.21099999 -86.21099999
Internal temperature: 64.21664626 C
Accelerometer data x:-0.041503906 y:0 z:1.056884765
Gyroscope data x:4.94384765 y:-8.60595703 z:-15.68603515

Accelerometer data x:-0.040771484 y:-0.0051269531 z:1.062988281
Gyroscope data x:4.94384765 y:-7.56835937 z:-15.014648437

Accelerometer data x:-0.046630859 y:-0.0068359375 z:1.055175781
Gyroscope data x:3.60107421 y:-7.62939453 z:-15.1977539

Accelerometer data x:-0.049560546 y:-0 z:1.061279296
Gyroscope data x:4.39453125 y:-7.32421875 z:-14.28222656
```

See [samples](samples) for a complete sample application.
