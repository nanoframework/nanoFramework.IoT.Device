# Radio Receiver

The radio receiver devices supported by the project include TEA5767.

## Documentation

- TEA5767 radio receiver [datasheet](https://cdn.datasheetspdf.com/pdf-down/T/E/A/TEA5767HN-NXP.pdf)

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

- TEA5767
- Male/Female Jumper Wires

```csharp
I2cConnectionSettings settings = new I2cConnectionSettings(1, Tea5767.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using (Tea5767 radio = new Tea5767(device, FrequencyRange.Other, 103.3))
{
    // The radio is running on FM 103.3MHz
}
```
