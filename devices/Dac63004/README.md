# DAC63004/DAC63004W - Ultra-low-power quad-channel 12-bit smart DAC with I²C, SPI and PWM

This library supports DAC63004 and DAC63004W devices.
Currently the implementation allows I2C connection to the device. SPI will be added in the future.

## Documentation

- DAC63004 [datasheet](https://www.ti.com/lit/gpn/dac63004)
- DAC63004W [datasheet](https://www.ti.com/lit/gpn/dac63004w)

## Device & EVM

| ![Device diagram](./DAC63004-diagram.png) |
|:--:|
| Device diagram |

| ![Evaluation Module](./DAC63004WCSP-EVM.png) |
|:--:|
| Evaluation Module |

## Usage

>**Warning**: If using an ESP32, make sure to properly setup the I2C pins before creating the `I2cDevice`. Add a reference to  `nanoFramework.Hardware.ESP32` NuGet package and add the following code lines:

```csharp
//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
```

For other devices like STM32, please make sure you're using the preset pins for the I2C/SPI bus you want to use.

### Hardware Required

- DAC63004W EVM
- Male/Female Jumper Wires

### Circuit

- SCL - SCL
- SDA - SDA
- VCC - 5V
- GND - GND

### Code

The following code creates an I2C configuration and instantiates a Dac63004 object. Then it prints the Minimum System Voltage detected by the device at boot. Last it enters a loop where it prints the current Vbus voltage each second.

```csharp
I2cConnectionSettings settings = new I2cConnectionSettings(1, Bq25798.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using (Bq25798 charger = new Bq25798(device))
{
    Debug.WriteLine("");
    Debug.WriteLine($"DAC63004 connected to I2C{device.ConnectionSettings.BusId}");
    Debug.WriteLine("");

    Debug.WriteLine($"Minimum System Voltage is config @ {charger.MinimalSystemVoltage.VoltsDc:N3}V");

    while (true)
    {
        Debug.WriteLine($"Current Vbus: {charger.Vbus.VoltsDc:N3}V");

        Debug.WriteLine("");

        Thread.Sleep(1000);
    }
}
```

## Acknowledgments

The development of this library was kindly sponsored by [OrgPal.IoT](https://www.orgpal.com/)!

![orgpallogo.png](./orgpallogo.png)
