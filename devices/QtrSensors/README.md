# QTR Sensors - Pololu QTR Reflectance Sensors

Those sensors are popular on small robots to have scenarios like line following. They provide a way to calibrate, read, and provide high level functions to help in line following.

While this implementation has been tested with reflectance sensors, this will work with any type of analogic sensor. The original design have the sensors lining up but the shape of the sensors can be anything. Just keep in mind that the ReadPosition function will not be relevant if you don't have the sensors aligned.

## Documentation

Official documentation can be fond [here](https://www.pololu.com/category/123/pololu-qtr-reflectance-sensors).

## QTR Sensors

You will find different implementation of those. All implementations from Pololu have aligned sensors. Some others may have different implementation. The present implementation is only for analogic sensors. It will not only work with Pololu sensors but with any analogic sensors. They have to have 1 or 2 common emitters. 

## Usage

You can create q QtrAnalog sensor with an array of GPIO for the individual sensors and an array of 0 or 1 or 2 GPIO for the emitter.

```csharp
// In this case, we will use 1 emitter and 5 sensors
QtrAnalogic qtr = new(new int[] { 33, 14, 26, 4, 13 }, new int[] { 15 });
```

### Calibration

It is recommended to calibrate the sensors on the surface you are going to use without any line or perturbation. This will increase the accuracy of the line detection if you're using this scenario.

```csharp
var calibOn = qtr.Calibrate(10, true);
var calibOff = qtr.Calibrate(10, false);
```

### Samples per sensor, emitters and emitter values

You can adjust the number or reads you'll do per sensor. The read value will be averaged. Keep in mind that reading the sensor is taking time, so the more you read, the more time it takes. If you are using a robot scenario, try to limit the sapling if it is moving fast.

```csharp
// This will read each sensor 5 times
qtr.SamplesPerSensor = 5;
```

You can as well adjust the sensor emitter pins to either on or off and choose to have all of them, none, even or odds one. The combination will allow you to find a flexible way depending on your scenario. And you can as well do not create the QTR Sensors with emitter pins and manage them yourself.

```csharp
qtr.EmitterSelection = EmitterSelection.All;
qtr.EmitterValue = PinValue.High;
```

Note this has impact on the measurement. The calibration can be preformed for the 2 scenarios: with the emitter on and off.

### Read raw values

You can get the raw values without calibration by using the `ReadRaw` function. This will give the values as they are without using any calibration.

```csharp
// Raws is an array of double
var raws = qtr.ReadRaw();
```

### Read ratio value

You can get normalized reading of the data using the calibration if you've done it. The results will be between 0 and 1.0

```csharp
var ratios = qtr.ReadRatio();
```

### Read position values

This is useful in line following scenarios. You can get a position between -1.0 and +1.0 where 0.0 is the middle, meaning it has detected in the middle of the sensors.

```csharp
// By default it will read black/dark lines out of a white/clear background
var pos = qtr.ReadPosition();
```

Note: the sensor will keep the last position, so if you were close to -1 and the line is not detected, it will continue to give you -1. Same for +1. Arbitrary, -1 is used for the first GPIO number you gave in the constructor and +1 for the last one.

This is using normalized and pondered data to provide this value.

```csharp
// This will read the position of a white/clear line on a black/dark background
var pos = qtr.ReadPosition(false);
```

## Current implementation

QTR Sensors type:
- [x] Analog QTR sensors
- [ ] Digital QTR sensors
