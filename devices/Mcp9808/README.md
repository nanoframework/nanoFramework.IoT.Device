# MCP9808 - Digital Temperature Sensor

Microchip Technology Inc.’s MCP9808 digital temperature sensor converts temperatures between -20°C and +100°C to a digital word with ±0.25°C/±0.5°C (typical/maximum) accuracy

## Documentation

- You can find the datasheet [here](http://ww1.microchip.com/downloads/en/DeviceDoc/25095A.pdf)

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

- MCP9808
- Male/Female Jumper Wires

### Circuit

- SCL - SCL
- SDA - SDA
- VCC - 5V
- GND - GND

### Code

```csharp
I2cConnectionSettings settings = new I2cConnectionSettings(1, Mcp9808.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using(Mcp9808 sensor = new Mcp9808(device))
{
    while (true)
    {
        Debug.WriteLine($"Temperature: {sensor.Temperature.Celsius} ℃");
        Debug.WriteLine();

        Thread.Sleep(1000);
    }
}
```
