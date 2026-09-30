# Plantower PMSA003I particulate-matter sensor

The PMSA003I measures PM1.0, PM2.5, and PM10 mass concentration and particle counts in six size bins. This binding reads the sensor's 32-byte frame over I2C and validates the frame header, length, and checksum.

## Documentation

- [PMSA003 series data manual](https://cdn-shop.adafruit.com/product-files/4632/4505_PMSA003I_series_data_manual_English_V2.6.pdf)

## ESP32 wiring

| PMSA003I breakout | ESP32 | Notes |
|---|---|---|
| VIN | Board supply | Follow the breakout board's supply specification |
| GND | GND | Common ground |
| SDA | GPIO21 | I2C1 data |
| SCL | GPIO22 | I2C1 clock |

The fixed 7-bit I2C address is `0x12`. Configure the ESP32 I2C pin functions before creating `I2cDevice`.

## Usage

```csharp
Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new I2cConnectionSettings(1, Pmsa003i.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(settings);
Pmsa003i sensor = new Pmsa003i(i2cDevice);

PmsReading reading = sensor.Read();
Debug.WriteLine($"PM2.5: {reading.Pm2Point5Atmospheric} ug/m3");
```

Malformed frames throw `InvalidOperationException`. Dispose the sensor to dispose its I2C device, or pass `shouldDispose: false` to retain ownership.