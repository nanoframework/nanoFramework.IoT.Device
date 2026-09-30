# VCNL4040 - Proximity and Ambient Light Sensor

The VCNL4040 combines a proximity sensor, infrared emitter, ambient light sensor, and white-light channel. The binding supports sensor power control, emitter and receiver configuration, measurements, threshold interrupts, and interrupt flags. Its fixed I2C address is `0x60`.

## Documentation

- [VCNL4040 datasheet](https://www.vishay.com/docs/84274/vcnl4040.pdf)
- [VCNL4040 application note](https://www.vishay.com/docs/84307/designingvcnl4040.pdf)

## Wiring

| VCNL4040 | ESP32 |
|---|---|
| VIN | 3.3 V |
| GND | GND |
| SDA | GPIO21 |
| SCL | GPIO22 |
| INT | Any input GPIO (optional) |

Configure ESP32 I2C pins before creating the device. Other targets use their board-specific I2C pins.

## Usage

```csharp
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new I2cConnectionSettings(1, Vcnl4040Device.DefaultI2cAddress);
using I2cDevice i2cDevice = I2cDevice.Create(settings);
using Vcnl4040Device sensor = new Vcnl4040Device(i2cDevice);

sensor.AmbientLightSensor.IntegrationTime = AlsIntegrationTime.Time160ms;
sensor.AmbientLightSensor.PowerOn = true;
sensor.ProximitySensor.PowerOn = true;

while (true)
{
    Debug.WriteLine($"Illuminance: {sensor.AmbientLightSensor.Illuminance.Lux:F2} lux");
    Debug.WriteLine($"Proximity reading: {sensor.ProximitySensor.Distance} counts");
    Thread.Sleep(1000);
}
```

The proximity reading is an uncalibrated reflection count, not a physical distance. Its relationship to distance depends on the target surface, emitter settings, optical layout, and receiver configuration.
