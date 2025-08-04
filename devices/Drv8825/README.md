# 4-Wire stepper motor & Drv8825 driver board

A stepper motor is an electromechanical device which converts electrical pulses into discrete mechanical movements.
The shaft or spindle of a stepper motor rotates in discrete step increments when electrical command pulses
are applied to it in the proper sequence. The motors rotation has several direct relationships to these applied
input pulses. The sequence of the applied pulses is directly related to the direction of motor shafts rotation.
The speed of the motor shafts rotation is directly related to the frequency of the input pulses and the length
of rotation is directly related to the number of input pulses applied. One of the most significant advantages of
a stepper motor is its ability to be accurately controlled in an open loop system. Open loop control means
no feedback information about position is needed. This type of control eliminates the need for
expensive sensing and feedback devices such as optical encoders. Your position is known simply by 
keeping track of the input step pulses.

## Documentation

You can find the [Drv8825 documentation here](https://www.ti.com/lit/ds/symlink/drv8825.pdf).

## Connections

VMOT - connect to 8-35V (motor supply voltage)

GND - connect to GND

1A, 1B, 2A, 2B - connect to the 4 coils of motor

DIR - connect to microcontroller pin

STEP - connect to microcontroller pin

RST — join with SLP and connect to microcontroller pin or to 3.3-5V

Logic power supply can be connected to FAULT pin, it will works, but it useless.

If you have long wires (more that few centimeters), it is recommended using a minimum
47uF electrolytic capacitor as close as possible to the VMOT and GND.

![circuit](./Drv8825_circuit_bb.jpeg)

## Usage

```csharp
const byte stepPin = 26;
const byte dirPin = 25;
const byte sleepPin = 27;
const ushort fullStepsPerRotation = 200;
const int fullRotationDegree = 360;
const int sleepDelayInMilliseconds = 100;
const int delayPerRotationsInMilliseconds = 5000;

using (var motor = new Drv8825(stepPin, dirPin, sleepPin, fullStepsPerRotation))
{
    var direction = true;
    motor.WakeUp();
    for(var i = 1; i <= 10; i++)
    {
        var rotationDegree = (direction ? 1 : -1) * (fullRotationDegree * i);
        motor.Rotate(UnitsNet.Angle.FromDegrees(rotationDegree));
        direction = !direction;
        Thread.Sleep(delayPerRotationsInMilliseconds);
    }
    motor.Sleep(sleepDelayInMilliseconds);
}
```

So, you can rotate the motor not only with angle, but with steps too:

```csharp
var steps = 100;
motor.Rotate(steps, Direction.Clockwise);
```

Also you can use microsteps. To do this you need to connect M0, M1 and M2 pins to microcontroller
and pass it to constructor:

```csharp
const byte stepPin = 26;
const byte dirPin = 25;
const byte sleepPin = 27;
const byte m0Pin = 10;
const byte m1Pin = 11;
const byte m2Pin = 12;
const ushort fullStepsPerRotation = 200;
using (var motor = new Drv8825(stepPin, dirPin, sleepPin, fullStepsPerRotation, m0Pin: m0Pin, m1Pin: m1Pin, m2Pin: m2Pin))
{
    var direction = true;
    var steps = 3200;
    motor.WakeUp();
    motor.Rotate(steps, Direction.Clockwise, StepSize.ThirtyTwoStep);
    motor.Sleep(sleepDelayInMilliseconds);
}
```