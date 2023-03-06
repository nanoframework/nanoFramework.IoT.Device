// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ssd13xx;
using Iot.Device.Ssd13xx.Samples;
using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

Debug.WriteLine("Hello Ssd1306 Sample!");

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus, for example
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
//////////////////////////////////////////////////////////////////////

//Tested with 128x64 and 128x32 OLEDs
using Ssd1306 device = new Ssd1306(I2cDevice.Create(new I2cConnectionSettings(1, Ssd1306.DefaultI2cAddress)), Ssd13xx.DisplayResolution.OLED128x64);
//with reset. Pin number 18. Assembly needs to be added a nanoFramework.System.Device.Gpio
//using Ssd1306 device = new Ssd1306(I2cDevice.Create(new I2cConnectionSettings(1, Ssd1306.SecondaryI2cAddress)), Ssd13xx.DisplayResolution.OLED128x64, 18);
device.Rotation = RotationOptions.NoRotation;

device.ClearScreen();
device.Font = new BasicFont();
device.DrawString(2, 2, "nF IOT!", 2);//large size 2 font
device.DrawString(2, 32, "nanoFramework", 1, true);//centered text
device.Display();

Thread.Sleep(2000);
device.ClearScreen();
device.Font = new DoubleByteFont();
device.DrawString(2, 2, "功夫＄", 2, false);
device.DrawString(2, 34, "８９ＡＢ功夫＄", 1, true);
device.Display();

Thread.Sleep(Timeout.Infinite);
