# Radio Transmitter

The radio transmitter devices supported by the project include KT0803.

## Documentation

- KT0803 radio transmitter [datasheet](https://cdn.datasheetspdf.com/pdf-down/K/T/0/KT0803L-KTMicro.pdf)

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

### Hardware Required

- KT0803
- Male/Female Jumper Wires

```csharp
I2cConnectionSettings settings = new I2cConnectionSettings(1, Kt0803.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

// The radio is running on FM 106.6MHz
using (Kt0803 radio = new Kt0803(device, 106.6, Region.China))
{
    // Connect a sound sources to the 3.5mm earphone jack of the module
}
```
