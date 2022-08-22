# Mcp7940xx - I2C Real-Time Clock/Calendar with SRAM

The MCP7940M Real-Time Clock/Calendar (RTCC) tracks time using internal counters for hours, minutes, seconds, days, months, years, and day of week. Alarms can be configured on all counters up to and including months.

## Documentation

[Datasheet](https://ww1.microchip.com/downloads/en/DeviceDoc/MCP7940M-Low-Cost%20I2C-RTCC-with-SRAM-20002292C.pdf)

Original code was written for ESP32

## Usage

**Important**: Make sure you properly setup the I2C pins for ESP32 before creating the `I2cDevice`. For this, make sure you install the `nanoFramework.Hardware.Esp32` NuGet and use the `Configuration` class to configure the pins:

```csharp
//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs used for the bus.
Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);
```

For other devices like STM32, please make sure you're using the preset pins for the I2C bus you want to use.

The following example demonstrates using the clock and alarm functions of the Mcp7940xx family.

```csharp
using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Mcp7940xx;
using nanoFramework.Hardware.Esp32;

// Setup ESP32 I2C port.
Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);

// Setup Mcp7940m device. 
I2cConnectionSettings i2cSettings = new I2cConnectionSettings(1, Mcp7940m.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(i2cSettings);

Mcp7940m clock = new Mcp7940m(i2cDevice, ClockSource.ExternalCrystal);
clock.SetTime(DateTime.UtcNow);
clock.StartClock(true);

// Set Alarm 1 to trigger on the 4th minute of every hour.
Mcp7940m.Alarm alarm1 = new Mcp7940m.Alarm(AlarmMatchMode.Minute, minute: 4);
clock.SetAlarm1(alarm1);
clock.EnableAlarm1();
Debug.WriteLine($"Alarm 1 : {clock.GetAlarm1()}");

// Set Alarm 2 to trigger every Wednesday.
Mcp7940m.Alarm alarm2 = new Mcp7940m.Alarm(AlarmMatchMode.DayOfWeek, dayOfWeek: DayOfWeek.Wednesday);
clock.SetAlarm2(alarm2);
clock.EnableAlarm2();
Debug.WriteLine($"Alarm 2 : {clock.GetAlarm2()}");

while (true)
{
    // Get current time.
    DateTime currentTime = clock.GetTime();
    Debug.WriteLine($"Time: {currentTime.ToString("yyyy/MM/dd HH:mm:ss")}");

    // Check if alarm 1 has triggered.
    if (clock.IsTriggeredAlarm1)
    {
        Debug.WriteLine("[ALARM 1]");

        // Clear alarm 1 flag.
        clock.ResetAlarm1();
    }

    // Check if alarm 2 has triggered.
    if (clock.IsTriggeredAlarm2)
    {
        Debug.WriteLine("[ALARM 2]");

        // Clear alarm 2 flag.
        clock.ResetAlarm2();

        // Turn off alarm 2.
        clock.DisableAlarm2();
    }

    // Wait for one second.
    Thread.Sleep(1000);
}
```

The following example demonstrates accessing the protected EEPROM and EUI of the Mcp79401.

```csharp
using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Mcp7940xx;
using nanoFramework.Hardware.Esp32;

// Setup ESP32 I2C port.
Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);

// Setup Mcp79401 eeprom device. 
I2cConnectionSettings i2cSettings = new I2cConnectionSettings(1, Mcp79401.Eeprom.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(i2cSettings);

Mcp79401.Eeprom eeprom = new Mcp79401.Eeprom(i2cDevice);

// Read and write to EEPROM.
byte byteAddress = 0;
eeprom.WriteByte(byteAddress, 0xA6);
byte value = eeprom.ReadByte(byteAddress);

// Read EUI from EEPROM.
byte[] eui = eeprom.ReadEui();
```
