# Realtime Clock
The RTC devices supported by the project include DS1307, DS3231, PCF8563 / BM8563.

## Usage
```C#
using System.Device.I2c;
using Iot.Device.Rtc;
using nanoFramework.Hardware.Esp32;

Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);

I2cConnectionSettings settings = new I2cConnectionSettings(1, Pcf8563.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using (Pcf8563 rtc = new Pcf8563(device))
{
    // set Pcf8563 time
    rtc.DateTime = new DateTime(2021, 9, 8, 10, 11, 12);
    
    while (true)
    {
        // read time
        DateTime dt = rtc.DateTime;
        
        // output the current time
        Debug.WriteLine($"Time: {dt.ToString("yyyy/MM/dd HH:mm:ss")}");
        
        // wait for a second
        Thread.Sleep(1000);
    }
}
```

## References
DS1307: https://cdn.datasheetspdf.com/pdf-down/D/S/1/DS1307-Maxim.pdf

DS3231: https://datasheets.maximintegrated.com/en/ds/DS3231.pdf

PCF8563 / BM8563: https://cdn.datasheetspdf.com/pdf-down/P/C/F/PCF-856.pdf
