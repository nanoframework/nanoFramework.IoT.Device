# HomeAssistant nanoSprinkler sample

This sample is a headless ESP32 sprinkler controller for .NET nanoFramework.
It uses the HomeAssistant binding over MQTT Discovery and exposes runtime controls in Home Assistant.

## What The Sample Does

- Loads runtime configuration from `I:\config.json`
- Saves Wi-Fi credentials to platform storage and reconnects on boot
- Connects to MQTT and publishes Home Assistant discovery/state
- Controls one relay output (sprinkler on/off)
- Supports timer mode with configurable ON/OFF durations
- Re-publishes discovery and state after Home Assistant restarts
- Tries connectivity recovery and triggers deep-sleep reboot after repeated failures

## Exposed Home Assistant Entities

- `Sprinkler` (switch)
- `Timer` (switch)
- `Timer ON Seconds` (number)
- `Timer OFF Seconds` (number)
- `Relay GPIO Pin` (number, 0..33)
- `Relay Active High` (switch)
- `Last Boot Marker` (diagnostic text sensor)

## Hardware Notes

Default relay wiring:

- ESP32 `GPIO15` -> relay `IN`
- ESP32 `GND` -> relay `GND`
- ESP32 `3V3` or `5V` -> relay `VCC`

Relay pin constraints:

- Valid relay output pins are `0..33`
- ESP32 `GPIO34..GPIO39` are input-only and are intentionally rejected

## Configuration

The sample reads config from `I:\config.json` at runtime.
A template file is included in this folder as `config.json`.

Example:

```json
{
  "WifiSsid": "your wifi",
  "WifiPassword": "your password",
  "MqttBroker": "192.168.1.2",
  "MqttPort": 1883,
  "HomeAssistantDeviceName": "nanoSprinkler",
  "HomeAssistantDeviceId": "nano_sprinkler",
  "TimerOnSeconds": 15,
  "TimerOffSeconds": 60,
  "RelayPin": 15,
  "RelayActiveHigh": false
}
```

Notes:

- `WifiSsid` is required.
- `HomeAssistantDeviceId` is sanitized for topic/object-id safety.
- `RelayPin` should remain in the valid output range (`0..33`).

## Run

1. Deploy the sample to your ESP32 using the nanoFramework VS extension.
2. Ensure `config.json` exists and adjust the pins and wifi credentials, it will be deployed automatically on device storage with valid values.
3. Start the app and open Home Assistant.
4. Add/configure MQTT integration in Home Assistant if needed.
5. Confirm entities are discovered and control relay/timer from HA.

## Troubleshooting

- No Wi-Fi at startup: verify `WifiSsid` in `I:\config.json` and station config support on target firmware.
- No HA entities: verify broker address/port and MQTT integration in Home Assistant.
- Relay not switching: verify wiring, polarity (`RelayActiveHigh`), and selected GPIO pin range.
