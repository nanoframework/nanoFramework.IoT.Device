# MPR121 - Proximity Capacitive Touch Sensor Controller

The 12-channels I2C proximity capacitive touch sensor controller.

## Documentation

* MPR121 [datasheet](https://www.sparkfun.com/datasheets/Components/MPR121.pdf)

## Usage

**Important**: make sure you properly setup the I2C pins especially for ESP32 before creating the `I2cDevice`, make sure you install the `nanoFramework.Hardware.ESP32 nuget`:

```csharp
//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
```

For other devices like STM32, please make sure you're using the preset pins for the I2C bus you want to use.

### Default configuration with manually updating of channel statuses

```csharp
var i2cDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, deviceAddress: Mpr121.DefaultI2cAddress));
var mpr121 = new Mpr121(device: i2cDevice);

var statuses = mpr121.ReadChannelStatuses();
var status = statuses[Channels.Channel01]
    ? "pressed"
    : "released";

Debug.WriteLine($"The 1st channel is {status}");
```

### Channel statuses auto refresh

```csharp
var i2cDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, deviceAddress: Mpr121.DefaultI2cAddress));

// Initialize controller with default configuration and auto-refresh the channel statuses every 100 ms.
var mpr121 = new Mpr121(device: i2cDevice, periodRefresh: 100);

// Subscribe to channel statuses updates.
mpr121.ChannelStatusesChanged += (object sender, ChannelStatusesChangedEventArgs e) =>
    {
        var channelStatuses = e.ChannelStatuses;
        // do something.
    };
```

### Custom MPR121 registers configuration

```csharp
var i2cDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, deviceAddress: Mpr121.DefaultI2cAddress));
var config = new Mpr121Configuration
{
    MaxHalfDeltaRising = 0x01,
    NoiseHalfDeltaRising = 0x01,
    NoiseCountLimitRising = 0x00,
    FilterDelayCountLimitRising = 0x00,
    MaxHalfDeltaFalling = 0x01,
    NoiseHalfDeltaFalling = 0x01,
    NoiseCountLimitFalling = 0xFF,
    FilterDelayCountLimitFalling = 0x01,
    ElectrodeTouchThreshold = 0x0F,
    ElectrodeReleaseThreshold = 0x0A,
    ChargeDischargeTimeConfiguration = 0x04,
    ElectrodeConfiguration = 0x0C
};

var mpr121 = new Mpr121(device: i2cDevice, configuration: config);
```

This sample demonstrates how to read channel statuses using auto-refresh configuration.

### Handling the channel statuses changes

```csharp
mpr121.ChannelStatusesChanged += (object sender, ChannelStatusesChangedEventArgs e) =>
    {
        var channelStatuses = e.ChannelStatuses;
        // do something.
    };
```

## Binding Notes

The binding provides different options of device configuration. The device can be configured to update the channel statuses periodically. Also it supports custom configuration of controller registers.
