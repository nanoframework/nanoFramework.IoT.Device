// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ssd13xx;
using Iot.Device.Ssd13xx.Samples;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;


Debug.WriteLine("Hello Ssd1306 Sample!");

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus, for example
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
//////////////////////////////////////////////////////////////////////

//Tested with 128x64 and 128x32 OLEDs
using Ssd1306 device = new Ssd1306(I2cDevice.Create(new I2cConnectionSettings(2, Ssd1306.DefaultI2cAddress)), Ssd13xx.DisplayResolution.OLED128x64);
device.ClearScreen();
device.Font = new BasicFont();
device.DrawString(2, 2, "Hello nF IOT!");
device.Display();

Thread.Sleep(-1);
