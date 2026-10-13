# APDS9960 proximity, ambient light, RGB, and gesture sensor

`Iot.Device.Apds9960` is a .NET nanoFramework binding for the Broadcom APDS9960 digital proximity, ambient light, RGB, and gesture sensor.

## Features

- I2C communication at the fixed `0x39` address
- Device ID access for diagnostics
- Clear, red, green, and blue channel readings
- Proximity readings
- Up, down, left, and right gesture recognition
- Configurable ambient light gain, proximity gain, and infrared LED drive strength

## Documentation

- [APDS9960 product page](https://www.broadcom.com/products/optical-sensors/integrated-ambient-light-and-proximity-sensors/apds-9960)
- [APDS9960 datasheet](https://docs.broadcom.com/doc/AV02-4191EN)

## Wiring

| APDS9960 | ESP32 |
| --- | --- |
| VL | 3.3 V |
| GND | GND |
| VCC | 3.3 V |
| SDA | GPIO21 |
| SCL | GPIO22 |
| INT | Optional GPIO input |

`VCC` powers the sensor and its I2C logic. `VL` powers the infrared LED used for proximity and gesture detection. Connect both pins to 3.3 V when using an ESP32. Do not connect `VCC`, `SDA`, or `SCL` to 5 V. Some breakout boards permit a different LED supply voltage on `VL`; consult the documentation for the specific board before using anything other than 3.3 V.

## Usage

```csharp
Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new I2cConnectionSettings(1, Apds9960.DefaultI2cAddress);
using I2cDevice i2cDevice = new I2cDevice(settings);
using Apds9960 sensor = new Apds9960(i2cDevice);

sensor.ColorSensorEnabled = true;
sensor.ProximitySensorEnabled = true;
sensor.GestureSensorEnabled = true;

while (true)
{
    if (sensor.IsGestureAvailable)
    {
        Debug.WriteLine("Gesture mode active; color and proximity output paused.");
        Gesture gesture = sensor.ReadGesture();
        Debug.WriteLine("Gesture mode ended.");

        switch (gesture)
        {
            case Gesture.Up:
                Debug.WriteLine("Gesture=Up");
                break;
            case Gesture.Down:
                Debug.WriteLine("Gesture=Down");
                break;
            case Gesture.Left:
                Debug.WriteLine("Gesture=Left");
                break;
            case Gesture.Right:
                Debug.WriteLine("Gesture=Right");
                break;
        }

        continue;
    }

    if (sensor.IsColorDataReady)
    {
        Color color = sensor.ReadColorData();
        Debug.WriteLine($"Clear={color.A}, R={color.R}, G={color.G}, B={color.B}");
    }

    Debug.WriteLine($"Proximity={sensor.ReadProximity()}");
    Thread.Sleep(200);
}
```

The interrupt pin is not required when polling `IsColorDataReady` and `IsGestureAvailable`.

## Gesture mode and other measurements

When proximity exceeds the gesture-entry threshold, the APDS9960 gesture engine takes control of the ADC. Ambient-light and RGB acquisition pauses, and the normal proximity register retains its last value. These measurements resume after the object moves away and the gesture engine exits.

The sample polls gesture state every 50 ms, prints one message when gesture mode starts, and suppresses color and proximity output while that mode remains active. `ReadGesture()` immediately drains the available FIFO data, continues draining every 10 ms until the gesture ends, and compares the first and last valid directional samples from the complete gesture session. It then returns the recognized direction and normal output resumes.

In Debug builds, set `GestureDiagnosticsEnabled` to `true` to write the FIFO level, every raw up/down/left/right sample, the batch endpoint ratios, and the final session deltas to the debugger. The diagnostics property, code, and strings are excluded from Release builds.

The gesture engine uses 1-times receiver gain and 25 mA LED drive to preserve directional detail when a hand is close to the sensor. Its entry and exit thresholds are reduced to 10 and 7 to match that lower sensitivity, and a directional ratio change of at least 12 is required to recognize a gesture. FIFO samples containing a channel clipped at 255 are excluded from direction calculations because their directional ratio is no longer reliable.

`ReadColorData()` returns an ARGB color. The alpha component contains clear-channel intensity scaled against the configured ADC range. Red, green, and blue are white-balanced and scaled so that the strongest compensated channel is 255.

The `RedChannelCompensation`, `GreenChannelCompensation`, and `BlueChannelCompensation` values default to neutral multipliers of `1.0`. Adjust these properties when the sensor, protective window, or lighting requires white-balance correction.

The constructor does not reject devices based on the ID register because compatible modules can report identifiers other than the documented APDS9960 value. Use `DeviceId` when the identifier is needed for diagnostics.
