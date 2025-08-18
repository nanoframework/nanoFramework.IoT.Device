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

You can find the [DRV8825 chip](https://www.ti.com/lit/ds/symlink/drv8825.pdf) or
[DRV8825 module](https://www.tme.eu/Document/1dd18faf1196df48619105e397146fdf/POLOLU-2133.pdf) datasheet here.

## Connections

VMOT - connect to 8-35V (motor supply voltage).

GND - connect to GND.

1A, 1B, 2A, 2B - connect to the 4 coils of motor.

DIR - connect to microcontroller output pin to control direction of stepping. Internal pulldown.

STEP - connect to microcontroller output pin to be able to perform the steps. Internal pulldown.

RST — connect to microcontroller output pin or to 3.3-5V, active-low, reinitializes the indexer logic
and disables H-bridge outputs. Internal pulldown.

SLP - can be joined with RST, connect it to microcontroller output pin or to 3.3-5V.
Logic high to enable driver, logic low to enter low-power sleep mode. Internal pulldown.

FAULT — can be connected to microcontroller input pin with pullup.
Logic low when driver in fault condition(overtemp, overcurrent). Not necessary connection.

M0, M1, M2 — can be connected to microcontroller output pins to use microsteps,
it takes smoother movement of motor, but it not necessary connections.

![circuit](./Drv8825_circuit_bb.jpeg)

### Advices

If you have DRV8825 module (not only chip), it is pin-compatible A4988 carrier.
It means for example that you can connect logic power supply to FAULT pin.
It will work for DRV8825 module (not just the chip!), but it is useless. If possible, try to avoid it.

If you have long wires (more that few centimeters), it is recommended using a minimum
47uF electrolytic capacitor as close as possible to the VMOT and GND.

Use a heat sink. The chip can get very hot before the temperature limiter kicks in.

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