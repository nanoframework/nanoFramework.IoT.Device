// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Drv8825;
using System.Threading;

const byte StepPin = 26;
const byte DirPin = 25;
const byte SleepPin = 27;
const ushort FullStepsPerRotation = 200;
const int FullRotationDegree = 360;
const int SleepDelayInMilliseconds = 100;
const int DelayPerRotationsInMilliseconds = 5000;
Thread.Sleep(10000);

using (var motor = new Drv8825(StepPin, DirPin, SleepPin, FullStepsPerRotation))
{
    var boolDirection = true;
    for(var i = 1; i <= 10; i++)
    {
        motor.WakeUp();
        var rotationDegree = (boolDirection ? 1 : -1) * (FullRotationDegree * i);
        motor.Rotate(UnitsNet.Angle.FromDegrees(rotationDegree));
        boolDirection = !boolDirection;
        motor.Sleep(SleepDelayInMilliseconds);
        Thread.Sleep(DelayPerRotationsInMilliseconds);
    }

    Thread.Sleep(DelayPerRotationsInMilliseconds);

    var direction = Direction.Clockwise;
    motor.WakeUp();
    for (var steps = 1; steps <= FullStepsPerRotation; steps++)
    {
        motor.Rotate(steps, direction);

        if (direction == Direction.Clockwise)
        {
            direction = Direction.Counterclockwise;
        }
        else
        {
            direction = Direction.Clockwise;
        }
    }
    motor.Sleep();
}

Thread.Sleep(Timeout.Infinite);