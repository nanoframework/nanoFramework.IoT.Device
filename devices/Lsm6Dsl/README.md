# LSM6DSL - three-axis accelerometer and gyroscope

The LSM6DSL is a low-power six-axis inertial measurement unit combining a three-axis accelerometer and a three-axis gyroscope. It is included on boards such as the ST B-L475E-IOT01A Discovery kit.

## Documentation

- [LSM6DSL datasheet](https://www.st.com/resource/en/datasheet/lsm6dsl.pdf)
- [B-L475E-IOT01A Discovery kit](https://www.st.com/en/evaluation-tools/b-l475e-iot01a.html)

## Usage

**Important**: make sure you properly setup the I2C pins especially for ESP32 before creating the `I2cDevice`, make sure you install the `nanoFramework.Hardware.ESP32 nuget`:

```csharp
//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
```

The on-board LSM6DSL on the B-L475E-IOT01A uses address `0x6A` and the board's sensor I2C bus. The sample uses bus 2; adjust the bus number if your firmware maps that peripheral differently.

```csharp
using Iot.Device.Lsm6Dsl;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

I2cConnectionSettings settings = new(2, Lsm6Dsl.DefaultI2cAddress);
using Lsm6Dsl sensor = new(I2cDevice.Create(settings));

sensor.AccelerationScale = AccelerationScale.Scale02G;
sensor.AngularRateScale = AngularRateScale.Scale0250Dps;

while (true)
{
    var acceleration = sensor.Acceleration;
    var angularRate = sensor.AngularRate;
    Debug.WriteLine($"Acceleration: X={acceleration.X:F3} g, Y={acceleration.Y:F3} g, Z={acceleration.Z:F3} g");
    Debug.WriteLine($"Angular rate: X={angularRate.X:F2} dps, Y={angularRate.Y:F2} dps, Z={angularRate.Z:F2} dps");
    Debug.WriteLine($"Temperature: {sensor.Temperature.DegreesCelsius:F1} C");
    Thread.Sleep(1000);
}
```

The driver supports accelerometer ranges from plus or minus 2 g to plus or minus 16 g, gyroscope ranges from plus or minus 125 dps to plus or minus 2000 dps, independent output data rates, data-ready status, and internal temperature.
