# Global Navigation Satellite System Device NMEA 0183 - Including Generic Serial Module with GPS, GNSS, BeiDou

This binding implements a base Global Navigation Satellite System (GNSS) device and a generic serial implementation. The device includes a generic serial module that supports multiple satellite navigation systems such as GPS, GNSS, and BeiDou. The Generic Serial GNSS Device is also extensible. Any serial modules like the NEO6-M from u-blox, ATGM336H,Minewsemi and many many more are directly supported for the main NMEA0183 features.

## Documentation

This bindings implement a large part of [NMEA 0183](https://en.wikipedia.org/wiki/NMEA_0183) and a way to extend the processed elements.

The `Location`class is also aligned with the [MAUI class](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/device/geolocation?view=net-maui-8.0&tabs=windows) `Location`. There is not a one to one match as .NET nanoFramework is using UnitsNet for the Course (with Angle) and Speed (with Speed).

## Connections

There are only 4 wires required:

| GPS/GNSS module | MCU Header |
|----------|------------|
| VCC      | 3.3V / 5V       |
| GND      | GND        |
| TX       | RX         |
| RX       | TX         |

It is important to check which voltage is required for your module.

## Usage of Generic GNSS Serial Module

```csharp
// Some modules like ESP32 requires to setup serial pins
// Configure GPIOs 16 and 17 to be used in UART2 (that's refered as COM3)
Configuration.SetPinFunction(9, DeviceFunction.COM2_RX);
Configuration.SetPinFunction(8, DeviceFunction.COM2_TX);

// By default baud rate is 9600
var gnssDevice = new GenericSerialGnssDevice("COM2");

// Subscribe for events
gnssDevice.FixChanged += FixChanged;
gnssDevice.LocationChanged += LocationChanged;
gnssDevice.OperationModeChanged += OperationModeChanged;
gnssDevice.ParsingError += ParsingError;
gnssDevice.ParsedMessage += ParsedMessage;

// Starts the module
gnssDevice.Start();
```

The various events allows you to subscribe for various behaviors. One of the main one is when the `Fix` change, meaning, your module starts to get proper data.

```csharp
private static void ParsingError(Exception exception)
{
    Console.WriteLine($"Received parsed error: {exception.Message}");
}

private static void OperationModeChanged(GnssOperation mode)
{
    Console.WriteLine($"Received Operation Mode changed: {mode}");
}

private static void LocationChanged(GeoPosition position)
{
    Console.WriteLine($"Received position changed: {position.Latitude},{position.Longitude}");
}

private static void FixChanged(Fix fix)
{
    Console.WriteLine($"Received Fix changed: {fix}");
}
```

## Extensibility of the GenericSerialGnssDevice

### Creating your own NmeaData parsers

This class is extensible by using the `ParsedMessage` event and adding `NmeaData` elements to the `Nmea0183Parser`. The samples show how to to do this.

Here is a simple example of a TXT message processing:

```csharp
using Iot.Device.Common.GnssDevice;
using System;

namespace GnssDevice.Sample
{
    /// <summary>
    /// Implements a simple TXT data from a GNSS device.
    /// </summary>
    internal class TxtData : NmeaData
    {
        /// <inheritdoc/>
        public override string MessageId => "TXT";

        /// <summary>
        /// Gets the decoded text.
        /// </summary>
        public string Text { get; internal set; }

        /// <summary>
        /// Gets the severity of the message.
        /// </summary>
        public MessageSeverity Severity { get; internal set; }

        /// <inheritdoc/>
        public override NmeaData Parse(string inputData)
        {
            if (!IsMatch(inputData))
            {
                throw new ArgumentException();
            }

            try
            {
                var subfields = GetSubFields(inputData);
                if(subfields.Length < 5)
                {
                    return null;
                }

                var txt = subfields[4];
                var sev = (MessageSeverity)int.Parse(subfields[3]);
                return new TxtData(txt, sev);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid NMEA TXT data", ex);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TxtData"/> class.
        /// </summary>
        public TxtData() 
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TxtData"/> class.
        /// </summary>
        /// <param name="txt">The TXT entry.</param>
        /// <param name="sev">The severity of the message.</param>
        public TxtData(string txt, MessageSeverity sev)
        {
            Text = txt;
            Severity = sev;
        }
    }

    public enum MessageSeverity
    {
        Error = 0,
        Warning = 1,
        Notice = 2,
        User = 7
    }
}
```

Once you have your own parser setup, you need to add it to the parsers:

```csharp
// Add the TXT parser in the NMEA Parser
Nmea0183Parser.AddParser(new TxtData());

// on the GNSS device, make sure you add the ParsedMessage event
gnssDevice = new GenericSerialGnssDevice("COM2");
gnssDevice.ParsedMessage += ParsedMessage;
```

And you can then get the parsed element here:

```csharp
private static void ParsedMessage(NmeaData data)
{
    Console.WriteLine($"Received parsed message: {data.GetType()}");
    if (data is TxtData txtData)
    {
        Console.WriteLine($"Received TXT message: {txtData.Text}, severity: {txtData.Severity}");
    }
}
```

If you want to replace one of the existing parser by your own, you can remove it from the Nmea0183Parser and add your own using the same principle.

### Handling yourself unparsed messages

This can be done using the `UnparsedMessage` event. You just need to subscribe to the event and you'll be able to handle the message:

```csharp
// on the GNSS device, make sure you add the UnparsedMessage event
gnssDevice = new GenericSerialGnssDevice("COM2");
gnssDevice.UnparsedMessage += UnparsedMessage;
```

As a result, all the unparsed messages will be delivered for you to handle:

```csharp
private static void UnparsedMessage(string message)
{
    Console.WriteLine($"Received unparsed message: {message}");
}
```

## Extensibility with the GnssDevice

The `GnssDevice` abstract class already incorporate quite some logic. The `GenericSerialGnssDevice` is using it and is a great example of extensibility. [Check the code](GenericSerialGnssDevice.cs).
