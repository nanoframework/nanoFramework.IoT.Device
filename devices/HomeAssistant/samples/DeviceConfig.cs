// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace nanoSprinkler
{
    /// <summary>
    /// All persisted device settings.  Serialised to / from I:\config.json by ConfigManager.
    /// </summary>
    public class DeviceConfig
    {
        /// <summary>
        /// Initializes a new instance with default values.
        /// </summary>
        public DeviceConfig()
        {
            WifiSsid = string.Empty;
            WifiPassword = string.Empty;
            MqttBroker = "192.168.1.2";
            MqttPort = 1883;
            HomeAssistantDeviceName = "nanoSprinkler";
            HomeAssistantDeviceId = "nano_sprinkler";
            TimerOnSeconds = 30;
            TimerOffSeconds = 60;
            TimerModeEnabled = false;
            RelayPin = 15;
            RelayActiveHigh = false;
        }

        /// <summary>
        /// Gets or sets the Wi-Fi SSID used for station mode.
        /// </summary>
        public string WifiSsid { get; set; }

        /// <summary>
        /// Gets or sets the Wi-Fi password used for station mode.
        /// </summary>
        public string WifiPassword { get; set; }

        /// <summary>
        /// Gets or sets the MQTT broker host or IP address.
        /// </summary>
        public string MqttBroker { get; set; }

        /// <summary>
        /// Gets or sets the MQTT broker port.
        /// </summary>
        public int MqttPort { get; set; }

        /// <summary>
        /// Gets or sets the Home Assistant device name used for topics and metadata.
        /// </summary>
        public string HomeAssistantDeviceName { get; set; }

        /// <summary>
        /// Gets or sets the Home Assistant device id (object id prefix).
        /// </summary>
        public string HomeAssistantDeviceId { get; set; }

        /// <summary>
        /// Gets or sets timer ON duration in seconds.
        /// </summary>
        public int TimerOnSeconds { get; set; }

        /// <summary>
        /// Gets or sets timer OFF duration in seconds.
        /// </summary>
        public int TimerOffSeconds { get; set; }

        /// <summary>
        /// Gets or sets whether timer mode should be enabled after reboot.
        /// </summary>
        public bool TimerModeEnabled { get; set; }

        /// <summary>
        /// Gets or sets the GPIO pin used for relay output.
        /// </summary>
        public int RelayPin { get; set; }

        /// <summary>
        /// Gets or sets whether relay uses active-high logic.
        /// </summary>
        public bool RelayActiveHigh { get; set; }
    }
}
