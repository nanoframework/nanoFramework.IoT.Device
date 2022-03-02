# Ds18b20 - Temperature Sensor

Ds18b20 is an digital Ambient Light Sensor IC for I2C bus interface. This IC is the most suitable to obtain the ambient light data for adjusting LCD and Keypad backlight power of Mobile phone. It is possible to detect wide range at High resolution.

## Documentation

Product datasheet can be found [here](https://datasheets.maximintegrated.com/en/ds/DS18B20.pdf)

## Sensor Image

![sensor](sensor.jpg)

## Usage

**Important**: make sure you properly setup the I2C pins especially for ESP32 before creating the `I2cDevice`, make sure you install the `nanoFramework.Hardware.ESP32 nuget`:

```csharp
I2cConnectionSettings settings = new I2cConnectionSettings(busId: 1, (int)I2cAddress.AddPinLow);
I2cDevice device = I2cDevice.Create(settings);

using (Bh1750fvi sensor = new Bh1750fvi(device))
{
    // read illuminance(Lux)
    double illuminance = sensor.Illuminance;
}

```

## Circuit

![circuit](Ds18b20_Circuit_bb.png)

* SCL - SCL
* SDA - SDA
* VCC - 5V
* GND - GND
* ADDR - GND

Result of the sample:

![running result](RunningResult.jpg)
