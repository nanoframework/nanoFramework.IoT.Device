// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.At24cxx;
using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using System.Diagnostics;
using System.Text;
using System.Threading;

// Setup ESP32 I2C port.
Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);

// Setup At24c32c device.
I2cConnectionSettings settings = new I2cConnectionSettings(1, At24c32.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(settings);

At24c32 eeprom = new At24c32(i2cDevice);

// Write string to device.
string message = "Hello from nanoFramework!";
byte[] encodedMessage = Encoding.UTF8.GetBytes(message);
int messageAddress = 0x0;
uint writeResult = eeprom.Write(messageAddress, encodedMessage);

Debug.WriteLine($"\"{message}\" written to EEPROM at 0x{messageAddress:X} ({writeResult} bytes)");
Thread.Sleep(100);

// Read back message from device.
byte[] receivedData = eeprom.Read(messageAddress, message.Length);
string decodedMessage = Encoding.UTF8.GetString(receivedData, 0, receivedData.Length);

Debug.WriteLine($"\"{decodedMessage}\" read from EEPROM at 0x{messageAddress:X}");
Thread.Sleep(100);

// Write byte to end of available device memory.
byte value = 0xA9;
int byteAddress = eeprom.Size - 1;
uint writeByteResult = eeprom.WriteByte(byteAddress, value);

Debug.WriteLine($"0x{value:X} written to EEPROM at 0x{byteAddress:X} ({writeByteResult} byte)");
Thread.Sleep(100);

// Read back byte from end of available device memory.
byte receivedByte = eeprom.ReadByte(byteAddress);

Debug.WriteLine($"0x{receivedByte:X} read from EEPROM at 0x{byteAddress:X}");
Thread.Sleep(100);

// Sequentially read back message from device byte-by-byte using the devices internal address counter.
// Since our last read from the device was at its last available byte, the internal address counter has now rolled over to point at the first byte.
byte[] receivedCharacter = new byte[1];
for (int i = 0; i < message.Length; i++)
{
    receivedCharacter[0] = eeprom.ReadByte();
    char[] character = Encoding.UTF8.GetChars(receivedCharacter, 0, 1);

    Debug.WriteLine($"'{character[0]}' read from EEPROM");
}
