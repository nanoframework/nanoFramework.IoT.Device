# AT24C128C - I2C EEPROM read/write

This binding is used used to read and write data via I2C from the external EEPROM memory.

## Documentation

[Datasheet](https://ww1.microchip.com/downloads/en/DeviceDoc/AT24C128C-I%C2%B2C-Compatible-(Two-Wire)-Serial-EEPROM-128-Kbit-(16,384x8)-20006110B.pdf)

Original code was written for MIMXRT1060 Evaluation Board by BusKetZz <busketz2k@gmail.com>


## Usage

```csharp
using System.Diagnostics;
using System.Text;
using System.Threading;
using Iot.Device.At24C128C;

At24C128C eeprom = new(0x50, 1);

string message = "Hello from MIMXRT1060!";
byte[] messageToSent = Encoding.UTF8.GetBytes(message);
ushort memoryAddress = 0x0;
eeprom.Write(memoryAddress, messageToSent);

Thread.Sleep(100);

byte[] receivedData = eeprom.Read(memoryAddress, message.Length);
string dataConvertedToString = System.Text.Encoding.UTF8.GetString(receivedData, 0, receivedData.Length);
Debug.WriteLine($"Message read from EEPROM: {dataConvertedToString}");
```
