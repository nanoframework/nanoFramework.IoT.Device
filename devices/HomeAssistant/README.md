# Home Assistant MQTT Integration

This binding provides a Home Assistant MQTT integration for .NET nanoFramework. It implements the [Home Assistant MQTT Discovery protocol](https://www.home-assistant.io/integrations/mqtt/#mqtt-discovery), automatically registering device entities in Home Assistant and keeping their state in sync over MQTT.

## Features

- **MQTT Discovery** — entities are automatically registered in Home Assistant on connect
- **Auto-generated topics** — state and command topics derived from the device and entity names
- **Built-in entity types** — Switch, Number, Sensor, Binary Sensor, Select, Text, and more
- **Availability tracking** — Last-Will-Testament and online/offline publishing
- **HA restart detection** — re-publishes discovery and state when Home Assistant comes back online
- **Sensor presets** — ready-made configurations for temperature, humidity, pressure, energy, and more

## Usage

### 1. Create device info

```csharp
var device = new HomeAssistantDeviceInfo(
    id:           "my_device",
    name:         "My Device",
    model:        "ESP32",
    manufacturer: "nanoFramework");
```

### 2. Create the client

```csharp
var client = new HomeAssistantClient(
    device:               device,
    brokerAddress:        "192.168.1.2",
    brokerPort:           1883,
    mqttClientIdPrefix:   "my-device-",
    mqttUsername:         null,    // optional
    mqttPassword:         null);   // optional
```

The device's `Name` (from `HomeAssistantDeviceInfo`) is normalized to lowercase with dashes and becomes the root MQTT topic prefix:
`My Device` → `nanoframework/my-device/…`

### 3. Add entities

```csharp
// Switch (binary on/off)
HomeAssistantSwitch lightSwitch = client.AddSwitch("my_device_light", "Light");

// Number (integer input)
HomeAssistantNumber brightness = client.AddNumber(
    objectId:          "my_device_brightness",
    name:              "Brightness",
    min:               "0",
    max:               "100",
    step:              "1",
    unitOfMeasurement: "%");

// Sensor (read-only numeric value)
HomeAssistantNumber temperature = client.AddSensor(
    objectId:          "my_device_temperature",
    name:              "Temperature",
    unitOfMeasurement: "°C",
    deviceClass:       HomeAssistantDeviceClass.Temperature);

// Select (enumerated options)
HomeAssistantSelect mode = client.AddSelect(
    objectId: "my_device_mode",
    name:     "Mode",
    options:  new[] { "Off", "Low", "High" });

// Diagnostic text sensor (read-only, shown under Diagnostics in HA)
HomeAssistantTextItem status = client.AddDiagnosticStringSensor(
    objectId:     "my_device_status",
    name:         "Status",
    initialValue: "OK");
```

### 4. Subscribe to state changes

```csharp
lightSwitch.OnStateChange += (sender, oldState, newState) =>
{
    if (newState == "ON")
    {
        // turn on hardware
    }
    else
    {
        // turn off hardware
    }
};

brightness.OnStateChange += (sender, oldState, newState) =>
{
    int value;
    if (int.TryParse(newState, out value))
    {
        // apply brightness
    }
};
```

### 5. Connect and publish

```csharp
bool connected = client.Connect();

if (connected)
{
    // Publish initial entity states
    lightSwitch.PublishState("OFF");
    brightness.PublishState("50");
    temperature.PublishState("21.5");
}
```

`Connect()` automatically sets a Last-Will-Testament on `client.AvailabilityTopic` (payload `"offline"`), so Home Assistant marks the device unavailable if it disconnects ungracefully. Pass a different topic to `willTopic` to override it, or `string.Empty` to connect without an LWT at all.

### 6. Publish state updates

Use `PublishState` for application-originated changes (does not trigger `OnStateChange`):

```csharp
temperature.PublishState("22.3");
```

Use `SetState` when receiving an external command and you need to run actuation logic (does trigger `OnStateChange`, does not publish):

```csharp
lightSwitch.SetState("ON");
```

After successful actuation, publish the confirmed state:

```csharp
lightSwitch.PublishState("ON");
```

### 7. Handle Home Assistant restarts

Subscribe to incoming MQTT messages and re-publish discovery and state when HA comes back online:

```csharp
client.MqttMessageReceived += (sender, e) =>
{
    string topic = e.Topic;
    string payload = System.Text.Encoding.UTF8.GetString(e.Message, 0, e.Message.Length).Trim();

    if (client.IsHomeAssistantOnlineEvent(topic, payload))
    {
        client.PublishOnline();
        client.PublishDiscovery();
        // re-publish all entity states here
    }
};
```

## MQTT Topic Convention

Topics are auto-generated from the device name and entity name:

| Element | Example value | Resulting topic segment |
| --- | --- | --- |
| Device name | `MyDevice` | `nanoframework/mydevice` |
| Entity name | `Timer ON Seconds` | `timer_on_seconds` |
| State topic | | `nanoframework/mydevice/timer_on_seconds/state` |
| Command topic | | `nanoframework/mydevice/timer_on_seconds/set` |
| Availability | | `nanoframework/mydevice/availability` |

## Sensor Presets

`HomeAssistantSensorPresets` applies common device class, state class, and unit-of-measurement settings to a `HomeAssistantDiscoveryEntity`:

```csharp
HomeAssistantNumber tempSensor = client.AddSensor("my_device_temp", "Temperature");
HomeAssistantSensorPresets.ApplyTemperaturePreset(tempSensor.Discovery);

HomeAssistantNumber humiditySensor = client.AddSensor("my_device_humidity", "Humidity");
HomeAssistantSensorPresets.ApplyHumidityPreset(humiditySensor.Discovery);
```

Available presets:

| Method | Device class | Unit |
| --- | --- | --- |
| `ApplyTemperaturePreset` | `temperature` | °C |
| `ApplyHumidityPreset` | `humidity` | % |
| `ApplyPressurePreset` | `atmospheric_pressure` | hPa |
| `ApplyPowerPreset` | `power` | W |
| `ApplyEnergyPreset` | `energy` | Wh |
| `ApplyWaterPreset` | `water` | L |
| `ApplyVoltagePreset` | `voltage` | V |
| `ApplyCurrentPreset` | `current` | A |
| `ApplyIlluminancePreset` | `illuminance` | lx |
| `ApplyBatteryPreset` | `battery` | % |
| `ApplyDurationPreset` | `duration` | s |
| `ApplyCarbonDioxidePreset` | `carbon_dioxide` | ppm |

## Supported Component Types

`HomeAssistantComponentType` covers:

`Switch`, `Number`, `Sensor`, `BinarySensor`, `Button`, `Select`, `Light`, `Cover`, `Climate`, `Text`

## Entity Categories

Use `HomeAssistantEntityCategory` to control where entities appear in the HA UI:

- `HomeAssistantEntityCategory.Config` — shown under *Configuration*
- `HomeAssistantEntityCategory.Diagnostic` — shown under *Diagnostics*

## Sample

See the [repository sample](https://github.com/nanoframework/nanoFramework.IoT.Device/tree/main/devices/HomeAssistant/samples/HomeAssistant.sample) for a complete ESP32 sprinkler controller example that demonstrates:

- Loading Wi-Fi and MQTT credentials from `config.json` stored on the device filesystem
- Connecting to Wi-Fi and an MQTT broker with auto-reconnect
- Registering Switch, Number, and diagnostic Sensor entities
- Controlling a GPIO relay from HA switch commands
- Configurable timer mode with ON/OFF duration entities
- Detecting Home Assistant restarts and re-publishing state

### Wiring (sample)

| ESP32 pin | Connected to |
| --- | --- |
| GPIO15 (default) | Relay IN |
| GND | Relay GND |
| 3.3V / 5V | Relay VCC |

The relay GPIO pin and active-high/active-low polarity are configurable from Home Assistant at runtime.

## References

- [Home Assistant MQTT Integration](https://www.home-assistant.io/integrations/mqtt/)
- [MQTT Discovery Protocol](https://www.home-assistant.io/integrations/mqtt/#mqtt-discovery)
- [nanoFramework M2Mqtt](https://github.com/nanoframework/nanoFramework.m2mqtt)
- Credits to [WilliamBZA](https://github.com/WilliamBZA/nanoframework.homeassistant.mqttdiscovery) for the great start to create this binding!
