# Module and Unit 4 Relay - I2C relay

Module and Unit 4 Relay are a realys that can be piloted thru I2C. They are available with M5Stack.

## Documentation

* The 4 Relay Module documentation can be found [here](https://docs.m5stack.com/en/module/4relay)
* The 4 Relay Unit documentation can be found [here](https://docs.m5stack.com/en/unit/4relay)

## Usage

**Important**: make sure you properly setup the I2C pins especially for ESP32 before creating the `I2cDevice`, make sure you install the `nanoFramework.Hardware.ESP32 nuget`:

```csharp
//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
```

For other devices like STM32, please make sure you're using the preset pins for the SPI bus you want to use. The chip select can as well be pre setup.

```csharp
using Iot.Device.Relay;
using System.Device.I2c;
using System.Threading;

Unit4Relay unit4Relay = new(new I2cDevice(new I2cConnectionSettings(1, Base4Relay.DefaultI2cAddress)));
// This will synchronize the led with the relay
unit4Relay.SynchronizedMode = true;
// Set relay 2, the led 2 should be on
unit4Relay.SetRelay(2, State.On);
// Set back the asyn modo
unit4Relay.SynchronizedMode = false;
// Set relay 1, the led 1 should be off while the relay on
unit4Relay.SetRelay(1, State.On);
// Set led 0 to on, the relay should be off
unit4Relay.SetLed(0, State.On);
```
