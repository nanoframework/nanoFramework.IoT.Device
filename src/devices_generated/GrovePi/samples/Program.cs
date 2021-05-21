// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Device.I2c;
using System.Device.Gpio;
using Iot.Device.GrovePiDevice.Models;
using Iot.Device.GrovePiDevice;
using Iot.Device.GrovePiDevice.Sensors;

Debug.WriteLine("Hello GrovePi!");
PinValue relay = PinValue.Low;
I2cConnectionSettings i2CConnectionSettings = new(1, GrovePi.DefaultI2cAddress);
using GrovePi grovePi = new GrovePi(I2cDevice.Create(i2CConnectionSettings));
Debug.WriteLine($"Manufacturer :{grovePi.GrovePiInfo.Manufacturer}");
Debug.WriteLine($"Board: {grovePi.GrovePiInfo.Board}");
Debug.WriteLine($"Firmware version: {grovePi.GrovePiInfo.SoftwareVersion}");
// Specific example to show how to read directly a pin without a high level class
grovePi.PinMode(GrovePort.AnalogPin0, PinMode.Input);
grovePi.PinMode(GrovePort.DigitalPin2, PinMode.Output);
grovePi.PinMode(GrovePort.DigitalPin3, PinMode.Output);
grovePi.PinMode(GrovePort.DigitalPin4, PinMode.Input);
// 2 high level classes
UltrasonicSensor ultrasonic = new(grovePi, GrovePort.DigitalPin6);
DhtSensor dhtSensor = new(grovePi, GrovePort.DigitalPin7, DhtType.Dht11);
int poten = 0;
while (!Console.KeyAvailable)
{
    Console.Clear();
    poten = grovePi.AnalogRead(GrovePort.AnalogPin0);
    Debug.WriteLine($"Potentiometer: {poten}");
    relay = (relay == PinValue.Low) ? PinValue.High : PinValue.Low;
    grovePi.DigitalWrite(GrovePort.DigitalPin2, relay);
    Debug.WriteLine($"Relay: {relay}");
    grovePi.AnalogWrite(GrovePort.DigitalPin3, (byte)(poten * 100 / 1023));
    Debug.WriteLine($"Button: {grovePi.DigitalRead(GrovePort.DigitalPin4)}");
    Debug.WriteLine($"Ultrasonic: {ultrasonic}");
    dhtSensor.Read();
    Debug.WriteLine($"{dhtSensor.DhtType}: {dhtSensor}");
    Thread.Sleep(2000);
}
