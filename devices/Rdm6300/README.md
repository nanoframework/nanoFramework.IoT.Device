# RDM6300 RFID reader

The RDM6300 is a 125 kHz RFID reader module that reports tag identifiers over a fixed 9600 baud, 8-N-1 UART connection. It is a passive reader: the host receives tag frames from TX, but there is no documented command or configuration protocol to implement over RX. It is typically compatible with EM4100/EM4102 read-only tags.

## Documentation

- [RDM6300 datasheet](https://www.handsontec.com/dataspecs/module/RDM6300.pdf)
- [Original nanoFramework community contribution](https://github.com/nanoframework/nf-Community-Contributions/tree/main/drivers/RFID_RDM6300_UART)

## Protocol

Each read is a 14-byte ASCII hexadecimal frame. The checksum is the XOR of the five decoded data bytes: the version/customer byte followed by the four tag ID bytes.

| Byte | Content |
| --- | --- |
| 0 | Start of text (`0x02`) |
| 1-2 | Version or customer code, two hexadecimal characters |
| 3-10 | Four-byte tag ID, eight hexadecimal characters |
| 11-12 | XOR checksum, two hexadecimal characters |
| 13 | End of text (`0x03`) |

The reader repeats the frame while a tag remains in the field, typically about every 65 ms. Consequently, `TagDetected` is raised for each valid frame. Applications that need one action per presentation should ignore consecutive events with the same `TagId` until the tag has been absent for an application-defined interval.

## Wiring

Connect the reader's TX output to the microcontroller UART RX input. The reader does not require a connection to the microcontroller UART TX output.

| RDM6300 | ESP32 example | Description |
| --- | --- | --- |
| TX | GPIO16 (COM2 RX) | Tag data from the reader |
| RX | Not connected | Reserved; no host commands are required |
| GND | GND | Common ground |
| +5V | 5V | Module power |
| ANT1, ANT2 | Antenna | External 125 kHz antenna |
| LED | Optional GPIO input | Status output; normally high and briefly active low when a tag is read |

Check the logic voltage of your module variant before connecting TX. Use a level shifter or voltage divider when its output exceeds the microcontroller input rating.

## Usage

```csharp
using Iot.Device.Rdm6300;
using nanoFramework.Hardware.Esp32;
using System.Diagnostics;
using System.Threading;

Configuration.SetPinFunction(Gpio.IO17, DeviceFunction.COM2_TX);
Configuration.SetPinFunction(Gpio.IO16, DeviceFunction.COM2_RX);

using Rdm6300 reader = new Rdm6300("COM2");
reader.TagDetected += (sender, tag) =>
{
    Debug.WriteLine($"Raw tag: {tag}");
    Debug.WriteLine($"Version: 0x{reader.TagVersion:X2}");
    Debug.WriteLine($"Tag ID: 0x{reader.TagId:X8}");
};

Thread.Sleep(Timeout.Infinite);
```

The latest complete value is available through `Tag`, with its decoded fields exposed through `TagVersion` and `TagId`. `TryParseFrame` can validate and decode a complete frame received by another transport or stored for later processing.
