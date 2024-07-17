# GPS Module
ATGM336H is a highly sensitive GPS module that provides high accuracy navigation and positioning features.

## Documentation

`ATGM336H` GPS module relies on the `NMEA0183` standard for communication. This standard is widely used for the interfacing and decoding of navigational data from the GPS module.

For more understanding and better implementation, please refer to the official [NMEA0183 documentation](https://www.tronico.fi/OH6NT/docs/NMEA0183.pdf).

## Connections

There are only 4 wires required:

| ATGM336H | MCU Header |
|----------|------------|
| VCC      | 3.3V       |
| GND      | GND        |
| TX       | RX         |
| RX       | TX         |

## Usage

Takes up to 30s to get fix. There is an internal diode on module that indicates if the fix is acquired.

```csharp
using Iot.Device.Atgm336h;
using System;
using System.Threading;
using nanoFramework.Hardware.Esp32;

var rxPin = 21;
var txPin = 19;
var comPort = "COM2";
Configuration.SetPinFunction(rxPin, DeviceFunction.COM2_RX);
Configuration.SetPinFunction(txPin, DeviceFunction.COM2_TX);
var gpsModule = new Atgm336h(comPort);
gpsModule.LocationChanged += GpsModuleOnLocationChanged;
gpsModule.FixChanged += GpsModuleOnFixChanged;
gpsModule.ModeChanged += GpsModuleOnModeChanged;
gpsModule.Start();

Thread.Sleep(Timeout.Infinite);

void GpsModuleOnFixChanged(Fix fix)
{
    Console.WriteLine($"Fix changed to: {fix}");
}

void GpsModuleOnModeChanged(Mode mode)
{
    Console.WriteLine($"Mode changed to: {mode}");
}

void GpsModuleOnLocationChanged(double latitude, double longitude)
{
    Console.WriteLine($"Position: {latitude},{longitude}");
}
```