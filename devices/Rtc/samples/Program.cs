using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Rtc;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

// This project contains DS1307, DS3231, PCF8563
I2cConnectionSettings settings = new(1, Ds3231.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using Ds3231 rtc = new(device);
// set time
rtc.DateTime = DateTime.UtcNow;

// loop
while (true)
{
    // read time
    DateTime dt = rtc.DateTime;

    Debug.WriteLine($"Time: {dt.ToString("yyyy/MM/dd HH:mm:ss")}");
    Debug.WriteLine($"Temperature: {rtc.Temperature.DegreesCelsius} ℃");
    Debug.WriteLine("");

    // wait for a second
    Thread.Sleep(1000);
}
