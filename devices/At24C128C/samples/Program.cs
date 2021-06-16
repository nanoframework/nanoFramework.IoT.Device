// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
