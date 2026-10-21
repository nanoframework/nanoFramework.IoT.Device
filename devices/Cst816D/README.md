# CST816D - Capacitive touch controller

The CST816D is a single-point I2C capacitive touch controller, used on the
[Waveshare ESP32-S3-Touch-LCD-2](https://www.waveshare.com/wiki/ESP32-S3-Touch-LCD-2).
Its 7-bit I2C address is **0x15**, and its chip identifier is **0xB6**.

## Documentation

- [Board documentation and vendor examples](https://www.waveshare.com/wiki/ESP32-S3-Touch-LCD-2)
- [Board schematic](https://files.waveshare.com/wiki/ESP32-S3-Touch-LCD-2/ESP32-S3-Touch-LCD-2-SchDoc.pdf)
- [Vendor driver and register definitions (demo archive)](https://files.waveshare.com/wiki/ESP32-S3-Touch-LCD-2/ESP32-S3-Touch-LCD-2-Demo.zip)

## Wiring

The Waveshare ESP32-S3-Touch-LCD-2 already connects the controller to these pins:

| Signal | ESP32-S3 GPIO |
| --- | --- |
| SDA | GPIO48 |
| SCL | GPIO47 |

These are board-specific pins. Always set them up on ESP32.
The vendor's example polls the controller and does not configure a touch reset GPIO
(`EXAMPLE_PIN_NUM_TP_RST = -1`). This sample follows that approach and does not require
an interrupt pin. For a separate module, use its documented reset/power-up sequence,
3.3 V power, common ground and I2C pull-ups. Configure the actual SDA/SCL pins before
creating the I2C device. On STM32, use the target's preset I2C pins instead.

## Usage

Install the binding (which depends on `nanoFramework.Graphics.Core`) and, for ESP32
pin configuration, `nanoFramework.Hardware.Esp32`.

```csharp
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Cst816D;
using nanoFramework.Hardware.Esp32;
using nanoFramework.UI;

Configuration.SetPinFunction(48, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(47, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new I2cConnectionSettings(1, Cst816D.DefaultI2cAddress);
using Cst816D touch = new Cst816D(new I2cDevice(settings), 240, 320, DisplayOrientation.Portrait);

while (true)
{
    Point point = touch.GetPoint();
    if (point.IsTouched)
    {
        Debug.WriteLine($"X: {point.X}, Y: {point.Y}, Event: {point.Event}");
    }

    Thread.Sleep(20);
}
```

`GetPoint()` reads touch count, X, Y and event in one contiguous I2C transaction.
`IsTouched` indicates current contact; release reports can contain the last position,
so do not infer contact from nonzero coordinates or the event alone. Polling may
miss brief transitions between reads.

## Display orientation

Use `nanoFramework.UI.DisplayOrientation` from `nanoFramework.Graphics.Core` to
match your display. Supply the touch area's **portrait** width and height to the
constructor (240 x 320 for this board), even when using a landscape orientation.
You can change `touch.Orientation` at runtime.

| Orientation | Returned X | Returned Y | Output size |
| --- | --- | --- | --- |
| `Portrait` | x | y | width x height |
| `Portrait180` | width - 1 - x | height - 1 - y | width x height |
| `Landscape` | height - 1 - y | x | height x width |
| `Landscape180` | y | width - 1 - x | height x width |

Coordinates are zero-based; landscape is a 90-degree clockwise coordinate rotation
and landscape180 is a 90-degree counterclockwise coordinate rotation. Touch state
and event are unchanged. No clipping or scaling is applied, so dimensions must
match the panel's native coordinate space. Release coordinates are rotated too.

The original constructor without dimensions preserves raw portrait coordinates.
Selecting a non-portrait orientation without dimensions makes `GetPoint()` throw
`InvalidOperationException`. Unsupported orientation values throw
`ArgumentOutOfRangeException` when reading; width and height must each be 1 to 4096.

The driver reuses its I2C buffers and is not thread-safe. Serialize access if reading
from multiple threads or GPIO callbacks. Failed I2C transfers throw
`InvalidOperationException` rather than returning a false no-touch report. If your
module uses controller sleep modes, wake/reset it according to its documentation
before accessing I2C; the binding does not change power or interrupt settings.

Disposing the controller also disposes the supplied I2C device by default. Pass
`shouldDispose: false` to retain ownership of that device. The board's I2C bus is
shared with other peripherals; create separate I2C devices with their own addresses.

## Sample and tests

[The sample](samples/Program.cs) polls the Waveshare board and logs contact and release.
Build `Cst816D.sln` with NuGet restore and nanoFramework MSBuild support.
[The tests](tests/Cst816DTests.cs) cover report decoding, all four orientations, coordinate boundaries and null-device validation
without physical I2C hardware. Run them using the nanoFramework Test Adapter and
[emulator runsettings](tests/nano.runsettings). Real-board validation is still needed
for electrical communication, power-up and display orientation.
