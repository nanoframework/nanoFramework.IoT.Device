# M5PM1 - M5Stack power-management IC

The M5PM1 is M5Stack's ESP32-S3-era power-management IC, used on boards such as the **M5StickS3**. It handles the battery and USB power paths, charging control and status, board power gates and external 5V switching.

This binding covers the **I2C control plane** and exposes the telemetry and control features that are most commonly used: battery / VBUS / 5V-out voltage, external 5V enable, charge enable, charging status and the active power source. The register behaviour is ported from the M5Stack `M5Unified` power implementation, which is the most authoritative reference for the M5PM1 (M5Stack's public product docs do not expose a full register map).

## Documentation

- [M5StickS3 product page](https://docs.m5stack.com/en/core/M5StickS3)
- [M5Stack I2C device address table](https://docs.m5stack.com/en/product_i2c_addr)
- [M5Unified power implementation (`Power_Class.cpp`)](https://github.com/m5stack/M5Unified/blob/master/src/utility/Power_Class.cpp)
- [M5Unified power API (`Power_Class.hpp`)](https://github.com/m5stack/M5Unified/blob/master/src/utility/Power_Class.hpp)
- [M5GFX board autodetect / PM1 checks (`M5GFX.cpp`)](https://github.com/m5stack/M5GFX/blob/master/src/M5GFX.cpp)

## Device family

| Property | Value |
| --- | --- |
| I2C address | `0x6E` |
| Battery voltage | registers `0x22` / `0x23` (mV) |
| VBUS voltage | registers `0x24` / `0x25` (mV) |
| 5V out voltage | registers `0x26` / `0x27` (mV) |
| External 5V enable | register `0x06`, bit 3 |
| Charge enable | register `0x06`, bit 0 |
| Charging status | register `0x12`, bit 0 (low = charging) |
| Power source | register `0x04`, bits 2:0 |
| Reliability init | I2C sleep off (`0x09` = `0x00`), watchdog off (`0x0A` = `0x00`) |

## Usage

**Important**: on the M5StickS3 the M5PM1 is on the internal I2C bus (SDA = GPIO47, SCL = GPIO48). Configure those pins before creating the `I2cDevice`.

```csharp
using Iot.Device.M5Pm1;
using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using System.Diagnostics;
using UnitsNet;

// M5StickS3 internal I2C bus.
Configuration.SetPinFunction(47, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(48, DeviceFunction.I2C1_CLOCK);

// The M5PM1 is a 100 kHz (standard-mode) device; the ESP32 default of 400 kHz is NAK'd.
I2cConnectionSettings settings = new I2cConnectionSettings(1, M5Pm1.I2cDefaultAddress, I2cBusSpeed.StandardMode);
I2cDevice i2cDevice = new I2cDevice(settings);

M5Pm1 power = new M5Pm1(i2cDevice);

// The constructor wakes the PMIC and applies the M5Stack reliability init (disable the I2C idle-sleep
// and the watchdog); the M5PM1 sleeps on an idle I2C bus. Optionally confirm the device ID.
int deviceId = power.GetDeviceId(); // should equal M5Pm1.DeviceId (0x2050)
Debug.WriteLine($"M5PM1 device ID: 0x{deviceId:X4} (expected 0x{M5Pm1.DeviceId:X4}).");

// Control.
power.BatteryChargeEnabled = true;
power.ExternalOutputEnabled = true;

// Telemetry (UnitsNet ElectricPotential).
Debug.WriteLine($"Battery : {power.GetBatteryVoltage().Millivolts} mV");
Debug.WriteLine($"VBUS    : {power.GetVbusVoltage().Millivolts} mV");
Debug.WriteLine($"5V out  : {power.GetOutputVoltage().Millivolts} mV");
Debug.WriteLine($"Source  : {power.GetPowerSource()}");
Debug.WriteLine($"Charging: {power.IsCharging}");
```

### GPIO

The M5PM1 exposes five general-purpose I/O pins (`Pin.Gpio0` to `Pin.Gpio4`). Each pin has a mux function (`GpioFunction`), a direction (the standard `PinMode` input or output only), an output driver type (`GpioDrive`), an output latch and an input reading. Boards wire these pins to different loads (for example the M5StickS3 uses `Gpio2` for the L3B / LCD power gate).

```csharp
// Drive GPIO2 high as a push-pull output (e.g. the M5StickS3 L3B / LCD power gate).
power.SetGpioFunction(Pin.Gpio2, GpioFunction.Gpio);
power.SetGpioDrive(Pin.Gpio2, GpioDrive.PushPull);
power.SetGpioMode(Pin.Gpio2, PinMode.Output);
power.WriteGpio(Pin.Gpio2, PinValue.High);

// Read GPIO0 as an input (e.g. the M5StickS3 charge-status line).
power.SetGpioMode(Pin.Gpio0, PinMode.Input);
PinValue level = power.ReadGpio(Pin.Gpio0);
```
