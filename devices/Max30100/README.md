# MAX30100 pulse oximeter and heart-rate sensor

The MAX30100 integrates red and infrared LEDs, a photodetector, optical elements, and low-noise electronics for pulse-oximetry and heart-rate signal acquisition. This binding configures the sensor and reads its raw red and infrared FIFO samples.

## Documentation

- [MAX30100 datasheet](https://www.analog.com/media/en/technical-documentation/data-sheets/MAX30100.pdf)

## Hardware required

- ESP32 development board
- MAX30100 breakout module
- Jumper wires

## ESP32 wiring

| MAX30100 | ESP32 |
|---|---|
| VIN | 3.3 V |
| GND | GND |
| SDA | GPIO21 |
| SCL | GPIO22 |
| INT | Not required by this sample |

Use 3.3 V logic. Some breakout boards include voltage regulators and I2C pull-ups; check the specifications of your module before connecting it.

## Usage

Configure the ESP32 I2C pins before creating the I2C device:

```csharp
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new I2cConnectionSettings(1, Max30100.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(settings);
Max30100 sensor = new Max30100(i2cDevice);

sensor.Reset();
sensor.Configure(
    Max30100Mode.SpO2,
    Max30100SampleRate.SamplesPerSecond100,
    Max30100PulseWidth.Microseconds1600,
    Max30100LedCurrent.Milliamps27Point1,
    Max30100LedCurrent.Milliamps27Point1);

while (true)
{
    if (sensor.TryReadSample(out Max30100Reading reading))
    {
        Debug.WriteLine($"IR: {reading.Infrared}, red: {reading.Red}");
    }
    else
    {
        Thread.Sleep(5);
    }
}
```

The MAX30100 supplies optical ADC samples; it does not directly return beats per minute or oxygen saturation. Calculating those values requires filtering, beat detection, calibration, and validation appropriate to the application. This binding is not intended for medical diagnosis.
