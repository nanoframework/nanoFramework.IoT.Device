# PAJ7620U2 gesture sensor

`Iot.Device.Paj7620` is a .NET nanoFramework binding for the PixArt PAJ7620U2 gesture sensor.

## Features

- I2C communication (`0x73` default address)
- Device ID validation during initialization
- Seeed-compatible initialization sequence
- Gesture decoding for:
  - Up, Down, Left, Right
  - Forward, Backward
  - Clockwise, CounterClockwise
  - Wave

## Usage

Create `Paj7620`, call `Initialize()`, then poll `TryReadGesture(...)`.

```csharp
using Iot.Device.Paj7620;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using nanoFramework.Hardware.Esp32;

Configuration.SetPinFunction(Gpio.IO05, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(Gpio.IO06, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new(1, Paj7620.DefaultI2CAddress);

using (Paj7620 sensor = new(I2cDevice.Create(settings)))
{
    sensor.GestureDebounceMilliseconds = 500;

    sensor.Initialize();
    Debug.WriteLine("PAJ7620 initialized.");

    while (true)
    {
        if (sensor.TryReadGesture(out Gesture gesture))
        {
            Debug.WriteLine($"Gesture: {gesture}");
        }

        Thread.Sleep(50);
    }
}
```

See [samples](samples) for a complete sample application.

## Gesture mapping

The driver maps gesture result bitfields to the enum `Gesture` as follows:

- `0x0001` -> `Up`
- `0x0002` -> `Down`
- `0x0004` -> `Left`
- `0x0008` -> `Right`
- `0x0010` -> `Forward`
- `0x0020` -> `Backward`
- `0x0040` -> `Clockwise`
- `0x0080` -> `CounterClockwise`
- `0x0100` -> `Wave`

## Notes

- The sensor requires successful `Initialize()` before reads.
- There is a default 500 ms debounce period after each detected gesture. This can be adjusted at runtime using `sensor.GestureDebounceMilliseconds`.
- Polling intervals between 20 ms and 100 ms are commonly stable in practice.
- Reliable detection depends on distance, hand speed, and ambient conditions.
- The sample uses I2C pins 5 (SDA) and 6 (SCL) on ESP32. Adjust pin configuration as needed for your board and wiring.
