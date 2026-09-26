# LIS3MDL - three-axis digital magnetometer

The LIS3MDL is a low-power, high-performance magnetic sensor with selectable full-scale ranges from plus or minus 4 gauss to plus or minus 16 gauss. It is included on boards such as the ST B-L475E-IOT01A Discovery kit.

## Documentation

- [LIS3MDL datasheet](https://www.st.com/resource/en/datasheet/lis3mdl.pdf)
- [B-L475E-IOT01A Discovery kit](https://www.st.com/en/evaluation-tools/b-l475e-iot01a.html)

## Usage

The on-board LIS3MDL on the B-L475E-IOT01A uses address `0x1E`, exposed as `SecondaryI2cAddress`. It is connected to the board's sensor I2C bus. The sample uses bus 2; adjust the bus number if your firmware maps that peripheral differently.

External LIS3MDL modules commonly use address `0x1C`, exposed as `DefaultI2cAddress`. For ESP32 targets, configure the selected I2C pins before creating the device.

```csharp
using Iot.Device.Lis3Mdl;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

I2cConnectionSettings settings = new(2, Lis3Mdl.SecondaryI2cAddress);
using Lis3Mdl sensor = new(I2cDevice.Create(settings));

sensor.MagneticInductionScale = MagneticInductionScale.Scale04G;
sensor.DataRate = DataRate.Rate10Hz;

while (true)
{
    var field = sensor.MagneticField;
    Debug.WriteLine($"X: {field.X:F2} mG, Y: {field.Y:F2} mG, Z: {field.Z:F2} mG");
    Debug.WriteLine($"Temperature: {sensor.Temperature.DegreesCelsius:F1} C");
    Thread.Sleep(1000);
}
```

`MagneticField` returns a `Vector3` whose components are measured in milligauss. `MagneticFieldX`, `MagneticFieldY`, and `MagneticFieldZ` return UnitsNet values. The driver also exposes continuous, single-shot, and power-down operation modes, four performance modes, and output rates from 0.625 Hz to 1000 Hz.
