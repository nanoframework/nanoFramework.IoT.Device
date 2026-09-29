// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Rdm6300;
using nanoFramework.Hardware.Esp32;
using System.Diagnostics;
using System.Threading;

Configuration.SetPinFunction(Gpio.IO17, DeviceFunction.COM2_TX);
Configuration.SetPinFunction(Gpio.IO16, DeviceFunction.COM2_RX);

using Rdm6300 reader = new Rdm6300("COM2");
reader.TagDetected += (sender, tag) => Debug.WriteLine($"RFID tag: {tag}");

Thread.Sleep(Timeout.Infinite);
