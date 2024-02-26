# LIS2MDL - Ultra-low-power, high-performance 3-axis digital magnetic sensor

Some of the applications mentioned by the datasheet:

- Tilt-compensated compasses
- Map rotation
- Intelligent power saving for handheld devices
- Gaming and virtual reality input devices

## Documentation

- [Datasheet](https://www.st.com/resource/en/datasheet/lis2mdl.pdf)

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
using Iot.Device.Lis2Mdl;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

using Lis2Mdl lis2mdlDevice = new(CreateI2cDevice());

while (true)
{
    var tempValue = lis2mdlDevice.Temperature;
    var magFieldValue = lis2mdlDevice.MagneticField;

    Debug.WriteLine($"Temperature: {tempValue.DegreesCelsius:F1}\u00B0C");
    Debug.WriteLine($"Mag. field X: {magFieldValue[0].Milligausses:F3}mG");
    Debug.WriteLine($"Mag. field Y: {magFieldValue[1].Milligausses:F3}mG");
    Debug.WriteLine($"Mag. field Z: {magFieldValue[2].Milligausses:F3}mG");

    Thread.Sleep(1000);
}

I2cDevice CreateI2cDevice()
{
    I2cConnectionSettings settings = new(1, Lis2Mdl.I2cAddress);
    return I2cDevice.Create(settings);
}

```
