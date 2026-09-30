// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Pms5003;
using nanoFramework.Hardware.Esp32;
using System.Device.Gpio;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

const int Reset = 2;
const int Set = 3;

GpioController gpioController = new GpioController();
GpioPin resetPin = gpioController.OpenPin(Reset, PinMode.Output);
GpioPin setPin = gpioController.OpenPin(Set, PinMode.Output);
resetPin.Write(PinValue.High);
setPin.Write(PinValue.High);

// PMS5003 TX -> ESP32-C3 Super Mini GPIO20 (RX), PMS5003 RX -> GPIO21 (TX).
Configuration.SetPinFunction(Gpio.IO20, DeviceFunction.COM2_RX);
Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.COM2_TX);

SerialPort serialPort = new SerialPort("COM2", Pms5003.DefaultBaudRate, Parity.None, 8, StopBits.One);
serialPort.ReadTimeout = 5000;
serialPort.Open();
Pms5003 sensor = new Pms5003(serialPort);

while (true)
{
    PmsReading reading = sensor.Read();
    Debug.WriteLine($"PM1.0: {reading.Pm1Atmospheric} ug/m3");
    Debug.WriteLine($"PM2.5: {reading.Pm2Point5Atmospheric} ug/m3");
    Debug.WriteLine($"PM10: {reading.Pm10Atmospheric} ug/m3");
    Thread.Sleep(1000);
}