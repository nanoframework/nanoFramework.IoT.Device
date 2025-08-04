// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Drv8825;
using System.Threading;

const byte stepPin = 26;
const byte dirPin = 25;
const byte sleepPin = 27;
const ushort fullStepsPerRotation = 200;
const int fullRotationDegree = 360;
const int sleepDelayInMilliseconds = 100;
const int delayPerRotationsInMilliseconds = 5000;
Thread.Sleep(10000);

using (var motor = new Drv8825(stepPin, dirPin, sleepPin, fullStepsPerRotation, m0Pin: 10))
{
    var boolDirection = true;
    for(var i = 1; i <= 10; i++)
    {
        motor.WakeUp();
        var rotationDegree = (boolDirection ? 1 : -1) * (fullRotationDegree * i);
        motor.Rotate(UnitsNet.Angle.FromDegrees(rotationDegree));
        boolDirection = !boolDirection;
        motor.Sleep(sleepDelayInMilliseconds);
        Thread.Sleep(delayPerRotationsInMilliseconds);
    }

    Thread.Sleep(delayPerRotationsInMilliseconds);

    var direction = Direction.Clockwise;
    motor.WakeUp();
    for (var steps = 1; steps <= fullStepsPerRotation; steps++)
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