# RGB diode - PWM
Library designed for controlling RGB diodes using PWM.

### Functions
The binding supports the following sensor functions:

* Control of RGB diodes
* PWM-based color intensity management

### Classes
* RgbDiode: Main class for RGB LED control

## Usage

```csharp
using Iot.Device.RgbDiode;

const byte redGpioPin = 25;
const byte greenGpioPin = 27;
const byte blueGpioPin = 26;

var pwm = new RgbDiode(redGpioPin, greenGpioPin, blueGpioPin);
pwm.SetColor(255, 0, 0); // Should display red
pwm.SetColor(0, 255, 0); // Should display green
pwm.SetColor(0, 0, 255); // Should display blue
```

**Important**: make sure you properly setup the PWM pins especially for ESP32 before creating the `RgbDiode`.