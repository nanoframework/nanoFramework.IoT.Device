using Iot.Device.A4988;
using System;

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
        motor.Rotate(360, direction);
        direction = !direction;
    }
}