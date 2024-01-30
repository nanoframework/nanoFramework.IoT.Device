# RGB diode - PWM

Library designed for controlling RGB diodes using PWM.

## Functions

The binding supports the following sensor functions:

* Control of RGB diodes
* PWM-based color intensity management

## Classes

* RgbDiode: Main class for RGB LED control

## Usage

```csharp
using Iot.Device.RgbDiode;
using System.Drawing;

const byte redGpioPin = 25;
const byte greenGpioPin = 26;
const byte blueGpioPin = 27;

// Uncomment for ESP32
// nanoFramework.Hardware.Esp32.Configuration.SetPinFunction(redGpioPin, nanoFramework.Hardware.Esp32.DeviceFunction.PWM1);
// nanoFramework.Hardware.Esp32.Configuration.SetPinFunction(greenGpioPin, nanoFramework.Hardware.Esp32.DeviceFunction.PWM2);
// nanoFramework.Hardware.Esp32.Configuration.SetPinFunction(blueGpioPin, nanoFramework.Hardware.Esp32.DeviceFunction.PWM3);

var pwm = new RgbDiode(redGpioPin, greenGpioPin, blueGpioPin, true);
// Currently there is a bug, you need to call set color twice to get desired outcome
pwm.SetColor(255, 0, 0); // Should display red
pwm.SetColor(255, 0, 0); // Should display red
pwm.SetColor(0, 255, 0); // Should display green
pwm.SetColor(0, 255, 0); // Should display green
pwm.SetColor(0, 0, 255); // Should display blue
pwm.SetColor(0, 0, 255); // Should display blue

pwm.SetColor(Color.LawnGreen);
pwm.SetColor(Color.LawnGreen);
pwm.SetColor(Color.OrangeRed);
pwm.SetColor(Color.OrangeRed);
pwm.SetColor(Color.Teal);
pwm.SetColor(Color.Teal);

pwm.Transition(Color.FromArgb(0, 255, 0)); // Will fade blue to green
pwm.Transition(Color.FromArgb(255, 0, 0)); // Will fade green to red
```

**Important**: make sure you properly setup the PWM pins especially for ESP32 before creating the `RgbDiode`.