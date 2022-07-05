# 4-Wire stepper motor & A4988 driver board

A stepper motor is an electromechanical device which converts electrical pulses into discrete mechanical movements. The shaft or spindle of a stepper motor rotates in discrete step increments when electrical command pulses are applied to it in the proper sequence. The motors rotation has several direct relationships to these applied input pulses. The sequence of the applied pulses is directly related to the direction of motor shafts rotation. The speed of the motor shafts rotation is directly related to the frequency of the input pulses and the length of rotation is directly related to the number of input pulses applied. One of the most significant advantages of a stepper motor is its ability to be accurately controlled in an open loop system. Open loop control means no feedback information about position is needed. This type of control eliminates the need for expensive sensing and feedback devices such as optical encoders. Your position is known simply by keeping track of the input step pulses.

## Documentation

You can find the [A4988 documentation here](https://www.pololu.com/file/0J450/a4988_DMOS_microstepping_driver_with_translator.pdf).

## Connections

VDD - connect to 3-5.5V (driver supply voltage)

VMOT - connect to 8-35V (motor supply voltage)

GND - connect to GND

1A, 1B, 2A, 2B - connect to the 4 coils of motor

DIR - connect to microcontroller pin

STEP - connect to microcontroller pin

## Usage

```csharp
using Iot.Device.A4988;
using System;

// Pinout for MCU please adapt depending on your MCU
// Any regular GPIO will work
const byte stepPin = 10;
const byte dirPin = 11;
const Microsteps microsteps = Microsteps.FullStep;
const ushort fullStepsPerRotation = 200;
TimeSpan sleepTime = TimeSpan.Zero;
using (var motor = new A4988(stepPin, dirPin, microsteps, fullStepsPerRotation, sleepTime))
{
    var direction = true;
    while (true)
    {
        var rotationDegree = (direction ? 1 : -1) * 360;
        motor.Rotate(UnitsNet.Angle.FromDegrees(rotationDegree));
        direction = !direction;
        System.Threading.Thread.Sleep(1000);
    }
}
```