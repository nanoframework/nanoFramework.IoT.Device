// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;

namespace nanoFramework.HomeAssistant
{
    /// <summary>
    /// Unified Home Assistant MQTT discovery component combining entity management, MQTT connection lifecycle, and publishing.
    /// Manages the complete MQTT connection including discovery publishing, availability tracking, and message subscription.
    /// Auto-generates topics based on entity object IDs and implements entity-based event routing with push-on-change model.
    /// </summary>
    public sealed class HomeAssistantClient
    {
        /// <summary>
        /// Delegate invoked when an MQTT publish message is received.
        /// </summary>
        /// <param name="sender">The sender instance.</param>
        /// <param name="e">MQTT publish event arguments.</param>
        public delegate void MqttMessageReceivedHandler(object sender, MqttMsgPublishEventArgs e);

        /// <summary>
        /// Delegate invoked when an MQTT publish is acknowledged.
        /// </summary>
        /// <param name="sender">The sender instance.</param>
        /// <param name="e">MQTT publish acknowledgment arguments.</param>
        public delegate void MqttMessagePublishedHandler(object sender, MqttMsgPublishedEventArgs e);

        private readonly HomeAssistantDeviceInfo _device;
        private readonly string _brokerAddress;
        private readonly int _brokerPort;
        private readonly string _mqttUsername;
        private readonly string _mqttPassword;
        private readonly string _mqttClientIdPrefix;
        private readonly string _deviceTopicRoot;
        private readonly ArrayList _discoveryEntities;
        private readonly ArrayList _runtimeEntities;
        private string _availabilityTopic;
        private MqttClient _mqttClient;
        private object _mqttLock = new object();

        /// <summary>
        /// Event raised when MQTT connection is established.
        /// </summary>
        public event EventHandler MqttConnected;

        /// <summary>
        /// Event raised when MQTT connection is closed.
        /// </summary>
        public event EventHandler MqttConnectionClosed;

        /// <summary>
        /// Event raised when an MQTT message is received.
        /// </summary>
        public event MqttMessageReceivedHandler MqttMessageReceived;

        /// <summary>
        /// Event raised when an MQTT message publish is acknowledged.
        /// </summary>
        public event MqttMessagePublishedHandler MqttMessagePublished;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeAssistantClient" /> class.
        /// Configures MQTT broker details and device metadata with auto-generated topics.
        /// </summary>
        /// <param name="device">Device metadata shared by all entities. Its <see cref="HomeAssistantDeviceInfo.Id"/> is also used to auto-generate MQTT topics (e.g., 'nanoSprinkler-01'). <see cref="HomeAssistantDeviceInfo.Id"/> must contain at least one alphanumeric character (a-z, 0-9) to generate MQTT topics.</param>
        /// <param name="brokerAddress">MQTT broker IP address or hostname.</param>
        /// <param name="brokerPort">MQTT broker port (usually 1883).</param>
        /// <param name="mqttClientIdPrefix">Prefix for generating unique MQTT client ID.</param>
        /// <param name="mqttUsername">Optional MQTT broker username.</param>
        /// <param name="mqttPassword">Optional MQTT broker password.</param>
        /// <param name="onMqttMessageReceived">Optional event handler for received MQTT messages.</param>
        /// <param name="onMqttConnectionClosed">Optional event handler for MQTT connection closed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="device"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="device"/>'s <see cref="HomeAssistantDeviceInfo.Id"/> contains no alphanumeric characters (e.g. it is entirely emoji or punctuation), or contains a character that cannot be represented in an MQTT topic segment (e.g. emoji, punctuation, non-ASCII, or MQTT wildcards such as '+' and '#'), and cannot be used to generate MQTT topics.</exception>
        public HomeAssistantClient(
            HomeAssistantDeviceInfo device,
            string brokerAddress,
            int brokerPort,
            string mqttClientIdPrefix = "ha-client-",
            string mqttUsername = null,
            string mqttPassword = null,
            MqttMessageReceivedHandler onMqttMessageReceived = null,
            EventHandler onMqttConnectionClosed = null)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (SanitizeTopicSegment(device.Id, '-').Length == 0)
            {
                throw new ArgumentException();
            }

            _device = device;
            _brokerAddress = brokerAddress;
            _brokerPort = brokerPort;
            _mqttClientIdPrefix = mqttClientIdPrefix;
            _mqttUsername = mqttUsername;
            _mqttPassword = mqttPassword;
            _deviceTopicRoot = GenerateDeviceTopicRoot(_device.Id);
            _availabilityTopic = GenerateAvailabilityTopic();
            _discoveryEntities = new ArrayList();
            _runtimeEntities = new ArrayList();

            // Wire up event handlers if provided
            if (onMqttMessageReceived != null)
            {
                MqttMessageReceived += onMqttMessageReceived;
            }

            if (onMqttConnectionClosed != null)
            {
                MqttConnectionClosed += onMqttConnectionClosed;
            }
        }

        /// <summary>
        /// Gets the display name of the device. MQTT topics are generated from
        /// <see cref="HomeAssistantDeviceInfo.Id"/>, not from this name.
        /// </summary>
        public string DeviceName
        {
            get { return _device.Name; }
        }

        /// <summary>
        /// Gets the Home Assistant status topic (standard "homeassistant/status").
        /// </summary>
        public string StatusTopic
        {
            get { return HomeAssistantTopics.StatusTopic; }
        }

        /// <summary>
        /// Gets the MQTT availability topic used for online/offline publishing and discovery payloads.
        /// </summary>
        public string AvailabilityTopic
        {
            get { return _availabilityTopic; }
        }

        /// <summary>
        /// Gets the optional MQTT username.
        /// </summary>
        public string MqttUsername
        {
            get { return _mqttUsername; }
        }

        /// <summary>
        /// Gets the optional MQTT password.
        /// </summary>
        public string MqttPassword
        {
            get { return _mqttPassword; }
        }

        /// <summary>
        /// Gets a value indicating whether the MQTT client is currently connected.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                lock (_mqttLock)
                {
                    return _mqttClient != null && _mqttClient.IsConnected;
                }
            }
        }

        /// <summary>
        /// Sanitizes a name into an MQTT-topic-safe segment: lowercase ASCII letters and digits only,
        /// with whitespace/dashes/underscores collapsed to a single <paramref name="separator"/>.
        /// Any other character (emoji, punctuation, non-ASCII, MQTT wildcards such as '+' and '#') is rejected
        /// rather than dropped, so that distinct inputs (e.g. "dev/1" and "dev1") cannot silently collapse
        /// onto the same topic segment.
        /// </summary>
        /// <param name="value">The raw name to sanitize.</param>
        /// <param name="separator">The character used to join word segments (e.g. '-' or '_').</param>
        /// <returns>The sanitized, topic-safe segment.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> contains a character that
        /// cannot be represented in a topic segment (e.g. emoji, punctuation, non-ASCII, or MQTT wildcards such
        /// as '+' and '#').</exception>
        private static string SanitizeTopicSegment(string value, char separator)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            StringBuilder sanitized = new StringBuilder(value.Length);
            bool lastWasSeparator = false;

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c >= 'a' && c <= 'z')
                {
                    sanitized.Append(c);
                    lastWasSeparator = false;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    sanitized.Append((char)(c + 32));
                    lastWasSeparator = false;
                }
                else if (c >= '0' && c <= '9')
                {
                    sanitized.Append(c);
                    lastWasSeparator = false;
                }
                else if (IsAsciiWhitespace(c) || c == '-' || c == '_')
                {
                    if (!lastWasSeparator && sanitized.Length > 0)
                    {
                        sanitized.Append(separator);
                        lastWasSeparator = true;
                    }
                }
                else
                {
                    throw new ArgumentException();
                }
            }

            if (sanitized.Length > 0 && sanitized[sanitized.Length - 1] == separator)
            {
                sanitized.Length--;
            }

            return sanitized.ToString();
        }

        /// <summary>
        /// Determines whether <paramref name="c"/> is an ASCII whitespace character (space, tab, newline,
        /// carriage return, form feed, or vertical tab). nanoFramework's mscorlib does not provide
        /// <c>Char.IsWhiteSpace</c>, so this enumerates the characters explicitly.
        /// </summary>
        /// <param name="c">The character to test.</param>
        /// <returns><see langword="true"/> if <paramref name="c"/> is ASCII whitespace; otherwise <see langword="false"/>.</returns>
        private static bool IsAsciiWhitespace(char c)
        {
            return c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\f' || c == '\v';
        }

        /// <summary>
        /// Generates the availability topic from the device ID.
        /// Example: device.Id='nanoSprinkler' → 'nanoframework/nanosprinkler/availability'.
        /// </summary>
        /// <returns>The availability topic for the configured device.</returns>
        private string GenerateAvailabilityTopic()
        {
            return _deviceTopicRoot + "/availability";
        }

        /// <summary>
        /// Generates the root topic prefix from a device ID.
        /// Example: 'nanoSprinkler-01' → 'nanoframework/nanosprinkler-01'.
        /// </summary>
        /// <returns>The normalized device topic root.</returns>
        private string GenerateDeviceTopicRoot(string deviceId)
        {
            string normalized = SanitizeTopicSegment(deviceId, '-');
            return $"nanoframework/{normalized}";
        }

        /// <summary>
        /// Generates the command topic for the root device command.
        /// </summary>
        /// <returns>The root device command topic.</returns>
        private string GenerateDeviceCommandTopic()
        {
            return _deviceTopicRoot + "/command";
        }

        /// <summary>
        /// Generates the state topic for the root device state.
        /// </summary>
        /// <returns>The root device state topic.</returns>
        private string GenerateDeviceStateTopic()
        {
            return _deviceTopicRoot + "/state";
        }

        /// <summary>
        /// Generates command topic for an entity based on its name and the device ID.
        /// The device topic root is normalized with '-' as the word separator; entity name
        /// segments are normalized with '_'.
        /// Example: device.Id='nanoSprinkler', entityName='Timer ON Seconds'.
        /// → 'nanoframework/nanosprinkler/timer_on_seconds/set'.
        /// </summary>
        /// <returns>The generated command topic for the entity, or the root command topic when name is empty.</returns>
        public string GenerateCommandTopic(string entityName)
        {
            if (string.IsNullOrEmpty(entityName))
            {
                return GenerateDeviceCommandTopic();
            }

            string entityNormalized = NormalizeTopicName(entityName);
            return _deviceTopicRoot + "/" + entityNormalized + "/set";
        }

        /// <summary>
        /// Generates command topic for an entity with a command channel suffix.
        /// Example: device.Id='nanoSprinkler', entityName='Thermostat', commandSuffix='mode'.
        /// → 'nanoframework/nanosprinkler/thermostat/mode/set'.
        /// </summary>
        /// <returns>The generated command topic with suffix channel.</returns>
        public string GenerateCommandTopic(string entityName, string commandSuffix)
        {
            if (string.IsNullOrEmpty(commandSuffix))
            {
                return GenerateCommandTopic(entityName);
            }

            if (string.IsNullOrEmpty(entityName))
            {
                string suffixNormalized = NormalizeTopicName(commandSuffix);
                return _deviceTopicRoot + "/" + suffixNormalized + "/set";
            }

            string entityNormalized = NormalizeTopicName(entityName);
            string commandNormalized = NormalizeTopicName(commandSuffix);
            return _deviceTopicRoot + "/" + entityNormalized + "/" + commandNormalized + "/set";
        }

        /// <summary>
        /// Generates state topic for an entity based on its name and the device ID.
        /// Example: device.Id='nanoSprinkler', entityName='Timer'.
        /// → 'nanoframework/nanosprinkler/timer/state'.
        /// </summary>
        /// <returns>The generated state topic for the entity, or the root state topic when name is empty.</returns>
        public string GenerateStateTopic(string entityName)
        {
            if (string.IsNullOrEmpty(entityName))
            {
                return GenerateDeviceStateTopic();
            }

            string entityNormalized = NormalizeTopicName(entityName);
            return _deviceTopicRoot + "/" + entityNormalized + "/state";
        }

        /// <summary>
        /// Generates state topic for an entity with a state channel suffix.
        /// Example: device.Id='nanoSprinkler', entityName='Thermostat', stateSuffix='temperature'.
        /// → 'nanoframework/nanosprinkler/thermostat/temperature/state'.
        /// </summary>
        /// <returns>The generated state topic with suffix channel.</returns>
        public string GenerateStateTopic(string entityName, string stateSuffix)
        {
            if (string.IsNullOrEmpty(stateSuffix))
            {
                return GenerateStateTopic(entityName);
            }

            if (string.IsNullOrEmpty(entityName))
            {
                string suffixNormalized = NormalizeTopicName(stateSuffix);
                return _deviceTopicRoot + "/" + suffixNormalized + "/state";
            }

            string entityNormalized = NormalizeTopicName(entityName);
            string stateNormalized = NormalizeTopicName(stateSuffix);
            return _deviceTopicRoot + "/" + entityNormalized + "/" + stateNormalized + "/state";
        }

        /// <summary>
        /// Normalizes an entity name into a topic-safe segment: lowercase ASCII, with
        /// whitespace/dashes/underscores collapsed to a single underscore.
        /// </summary>
        /// <returns>The normalized topic segment.</returns>
        private string NormalizeTopicName(string name)
        {
            string normalized = SanitizeTopicSegment(name, '_');
            if (normalized.Length == 0)
            {
                throw new ArgumentException("Topic name segment must contain at least one alphanumeric character.", nameof(name));
            }

            return normalized;
        }

        /// <summary>
        /// Validates and normalizes an entity object ID used for topic and unique ID generation.
        /// </summary>
        /// <param name="objectId">The object ID to validate.</param>
        /// <returns>The trimmed object ID.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="objectId"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="objectId"/> is empty or whitespace.</exception>
        private string RequireObjectId(string objectId)
        {
            if (objectId == null)
            {
                throw new ArgumentNullException(nameof(objectId));
            }

            string normalized = objectId.Trim();
            if (normalized.Length == 0)
            {
                throw new ArgumentException(nameof(objectId));
            }

            return normalized;
        }

        /// <summary>
        /// Adds a switch entity with auto-generated topics based on object ID.
        /// Topics are generated from the object ID normalized to lowercase with underscores.
        /// </summary>
        /// <param name="objectId">Unique object ID for the entity.</param>
        /// <param name="name">Display name for the entity.</param>
        /// <param name="payloadOn">Payload for ON state (default "ON").</param>
        /// <param name="payloadOff">Payload for OFF state (default "OFF").</param>
        /// <returns>The created switch runtime entity.</returns>
        public HomeAssistantSwitch AddSwitch(
            string objectId,
            string name,
            string payloadOn = "ON",
            string payloadOff = "OFF")
        {
            objectId = RequireObjectId(objectId);

            string stateTopic = GenerateStateTopic(objectId);
            string commandTopic = GenerateCommandTopic(objectId);

            var discovery = new HomeAssistantDiscoveryEntity
            {
                ComponentType = HomeAssistantComponentType.Switch,
                ObjectId = objectId,
                Name = name,
                UniqueId = objectId,
                StateTopic = stateTopic,
                CommandTopic = commandTopic,
                PayloadOn = payloadOn,
                PayloadOff = payloadOff
            };
            RegisterDiscoveryEntity(discovery);

            var runtime = new HomeAssistantSwitch(discovery, payloadOff, PublishMessage);
            RegisterRuntimeEntity(runtime);
            return runtime;
        }

        /// <summary>
        /// Adds a number entity with auto-generated topics based on object ID.
        /// Topics are generated from the object ID normalized to lowercase with underscores.
        /// </summary>
        /// <returns>The created number runtime entity.</returns>
        public HomeAssistantNumber AddNumber(
            string objectId,
            string name,
            string min = "0",
            string max = "100",
            string step = "1",
            string unitOfMeasurement = null)
        {
            objectId = RequireObjectId(objectId);
            unitOfMeasurement = unitOfMeasurement ?? string.Empty;

            string stateTopic = GenerateStateTopic(objectId);
            string commandTopic = GenerateCommandTopic(objectId);

            var discovery = new HomeAssistantDiscoveryEntity
            {
                ComponentType = HomeAssistantComponentType.Number,
                ObjectId = objectId,
                Name = name,
                UniqueId = objectId,
                StateTopic = stateTopic,
                CommandTopic = commandTopic,
                Min = min,
                Max = max,
                Step = step,
                UnitOfMeasurement = unitOfMeasurement,
                Mode = "box"
            };
            RegisterDiscoveryEntity(discovery);

            var runtime = new HomeAssistantNumber(discovery, min, PublishMessage);
            RegisterRuntimeEntity(runtime);
            return runtime;
        }

        /// <summary>
        /// Adds a sensor entity with an auto-generated topic based on object ID.
        /// </summary>
        /// <returns>The created sensor runtime entity.</returns>
        public HomeAssistantNumber AddSensor(
            string objectId,
            string name,
            string unitOfMeasurement = null,
            string deviceClass = null)
        {
            objectId = RequireObjectId(objectId);
            unitOfMeasurement = unitOfMeasurement ?? string.Empty;
            deviceClass = deviceClass ?? string.Empty;

            string stateTopic = GenerateStateTopic(objectId);

            var discovery = new HomeAssistantDiscoveryEntity
            {
                ComponentType = HomeAssistantComponentType.Sensor,
                ObjectId = objectId,
                Name = name,
                UniqueId = objectId,
                StateTopic = stateTopic,
                UnitOfMeasurement = unitOfMeasurement,
                DeviceClass = deviceClass,
                IncludeAvailability = true
            };
            RegisterDiscoveryEntity(discovery);

            var runtime = new HomeAssistantNumber(discovery, "0", PublishMessage);
            RegisterRuntimeEntity(runtime);
            return runtime;
        }

        /// <summary>
        /// Adds a select (enumeration) entity with auto-generated topics based on object ID.
        /// </summary>
        /// <returns>The created select runtime entity.</returns>
        public HomeAssistantSelect AddSelect(
            string objectId,
            string name,
            string[] options)
        {
            objectId = RequireObjectId(objectId);

            if (options == null || options.Length == 0)
            {
                throw new ArgumentException("Select options cannot be null or empty.", nameof(options));
            }

            string stateTopic = GenerateStateTopic(objectId);
            string commandTopic = GenerateCommandTopic(objectId);
            string optionsJson = BuildOptionsJson(options);

            var discovery = new HomeAssistantDiscoveryEntity
            {
                ComponentType = HomeAssistantComponentType.Select,
                ObjectId = objectId,
                Name = name,
                UniqueId = objectId,
                StateTopic = stateTopic,
                CommandTopic = commandTopic,
                ExtraProperties = new[] { optionsJson }
            };
            RegisterDiscoveryEntity(discovery);

            var runtime = new HomeAssistantSelect(discovery, options[0], PublishMessage);
            RegisterRuntimeEntity(runtime);
            return runtime;
        }

        /// <summary>
        /// Adds a text input entity with auto-generated topics based on object ID.
        /// </summary>
        /// <returns>The created text runtime entity.</returns>
        public HomeAssistantTextItem AddTextItem(
            string objectId,
            string name,
            string initialValue = null)
        {
            objectId = RequireObjectId(objectId);
            initialValue = initialValue ?? string.Empty;

            string stateTopic = GenerateStateTopic(objectId);
            string commandTopic = GenerateCommandTopic(objectId);

            var discovery = new HomeAssistantDiscoveryEntity
            {
                ComponentType = HomeAssistantComponentType.Text,
                ObjectId = objectId,
                Name = name,
                UniqueId = objectId,
                StateTopic = stateTopic,
                CommandTopic = commandTopic
            };
            RegisterDiscoveryEntity(discovery);

            var runtime = new HomeAssistantTextItem(discovery, initialValue, PublishMessage);
            RegisterRuntimeEntity(runtime);
            return runtime;
        }

        /// <summary>
        /// Adds a read-only diagnostic sensor that publishes string state without accepting commands.
        /// This is the correct Home Assistant pattern for diagnostic/status text.
        /// </summary>
        /// <returns>The created diagnostic text runtime entity.</returns>
        public HomeAssistantTextItem AddDiagnosticStringSensor(
            string objectId,
            string name,
            string initialValue = null)
        {
            objectId = RequireObjectId(objectId);
            initialValue = initialValue ?? string.Empty;

            string stateTopic = GenerateStateTopic(objectId);

            var discovery = new HomeAssistantDiscoveryEntity
            {
                ComponentType = HomeAssistantComponentType.Sensor,
                ObjectId = objectId,
                Name = name,
                UniqueId = objectId,
                StateTopic = stateTopic,
                EntityCategory = HomeAssistantEntityCategory.Diagnostic
            };
            RegisterDiscoveryEntity(discovery);

            var runtime = new HomeAssistantTextItem(discovery, initialValue, PublishMessage);
            RegisterRuntimeEntity(runtime);
            return runtime;
        }

        /// <summary>
        /// Adds a button entity with an auto-generated command topic based on object ID.
        /// </summary>
        /// <param name="objectId">Unique object ID for the entity.</param>
        /// <param name="name">Display name for the entity.</param>
        /// <param name="payloadPress">Payload sent by Home Assistant when button is pressed.</param>
        /// <returns>The created button runtime entity.</returns>
        public HomeAssistantButton AddButton(
            string objectId,
            string name,
            string payloadPress = "PRESS")
        {
            objectId = RequireObjectId(objectId);
            string commandTopic = GenerateCommandTopic(objectId);

            var discovery = new HomeAssistantDiscoveryEntity
            {
                ComponentType = HomeAssistantComponentType.Button,
                ObjectId = objectId,
                Name = name,
                UniqueId = objectId,
                CommandTopic = commandTopic,
                PayloadPress = payloadPress
            };
            RegisterDiscoveryEntity(discovery);

            var runtime = new HomeAssistantButton(discovery, PublishMessage);
            RegisterRuntimeEntity(runtime);
            return runtime;
        }

        /// <summary>
        /// Adds a light entity using basic on/off MQTT light discovery fields.
        /// </summary>
        /// <param name="objectId">Unique object ID for the entity.</param>
        /// <param name="name">Display name for the entity.</param>
        /// <param name="payloadOn">Payload for ON commands and state.</param>
        /// <param name="payloadOff">Payload for OFF commands and state.</param>
        /// <returns>The created light runtime entity.</returns>
        public HomeAssistantLight AddLight(
            string objectId,
            string name,
            string payloadOn = "ON",
            string payloadOff = "OFF")
        {
            objectId = RequireObjectId(objectId);
            string stateTopic = GenerateStateTopic(objectId);
            string commandTopic = GenerateCommandTopic(objectId);

            var discovery = new HomeAssistantDiscoveryEntity
            {
                ComponentType = HomeAssistantComponentType.Light,
                ObjectId = objectId,
                Name = name,
                UniqueId = objectId,
                StateTopic = stateTopic,
                CommandTopic = commandTopic,
                PayloadOn = payloadOn,
                PayloadOff = payloadOff
            };
            RegisterDiscoveryEntity(discovery);

            var runtime = new HomeAssistantLight(discovery, payloadOff, PublishMessage);
            RegisterRuntimeEntity(runtime);
            return runtime;
        }

        /// <summary>
        /// Adds a cover entity with open/close/stop command support and state topic.
        /// </summary>
        /// <param name="objectId">Unique object ID for the entity.</param>
        /// <param name="name">Display name for the entity.</param>
        /// <param name="payloadOpen">Payload used to open the cover.</param>
        /// <param name="payloadClose">Payload used to close the cover.</param>
        /// <param name="payloadStop">Payload used to stop the cover.</param>
        /// <returns>The created cover runtime entity.</returns>
        public HomeAssistantCover AddCover(
            string objectId,
            string name,
            string payloadOpen = "OPEN",
            string payloadClose = "CLOSE",
            string payloadStop = "STOP")
        {
            objectId = RequireObjectId(objectId);
            string stateTopic = GenerateStateTopic(objectId);
            string commandTopic = GenerateCommandTopic(objectId);
            string setPositionTopic = GenerateCommandTopic(objectId, "position");
            string positionTopic = GenerateStateTopic(objectId, "position");

            var discovery = new HomeAssistantDiscoveryEntity
            {
                ComponentType = HomeAssistantComponentType.Cover,
                ObjectId = objectId,
                Name = name,
                UniqueId = objectId,
                StateTopic = stateTopic,
                CommandTopic = commandTopic,
                SetPositionTopic = setPositionTopic,
                PositionTopic = positionTopic,
                PayloadOpen = payloadOpen,
                PayloadClose = payloadClose,
                PayloadStop = payloadStop,
                StateOpen = "open",
                StateOpening = "opening",
                StateClosed = "closed",
                StateClosing = "closing",
                StateStopped = "stopped"
            };
            RegisterDiscoveryEntity(discovery);

            var runtime = new HomeAssistantCover(discovery, "closed", PublishMessage);
            RegisterRuntimeEntity(runtime);
            return runtime;
        }

        /// <summary>
        /// Adds a climate entity with mode and temperature command/state topics.
        /// </summary>
        /// <param name="objectId">Unique object ID for the entity.</param>
        /// <param name="name">Display name for the entity.</param>
        /// <param name="modes">Supported climate modes (defaults to off/heat/cool).</param>
        /// <param name="minTemp">Minimum setpoint temperature.</param>
        /// <param name="maxTemp">Maximum setpoint temperature.</param>
        /// <param name="tempStep">Setpoint temperature step.</param>
        /// <param name="temperatureUnit">Temperature unit (C or F).</param>
        /// <returns>The created climate runtime entity.</returns>
        public HomeAssistantClimate AddClimate(
            string objectId,
            string name,
            string[] modes = null,
            string minTemp = null,
            string maxTemp = null,
            string tempStep = null,
            string temperatureUnit = "C")
        {
            objectId = RequireObjectId(objectId);
            modes = modes ?? new[] { "off", "heat", "cool" };

            string modeCommandTopic = GenerateCommandTopic(objectId, "mode");
            string modeStateTopic = GenerateStateTopic(objectId, "mode");
            string temperatureCommandTopic = GenerateCommandTopic(objectId, "temperature");
            string temperatureStateTopic = GenerateStateTopic(objectId, "temperature");
            string currentTemperatureTopic = GenerateStateTopic(objectId, "current_temperature");

            var discovery = new HomeAssistantDiscoveryEntity
            {
                ComponentType = HomeAssistantComponentType.Climate,
                ObjectId = objectId,
                Name = name,
                UniqueId = objectId,
                CommandTopic = modeCommandTopic,
                StateTopic = modeStateTopic,
                ModeCommandTopic = modeCommandTopic,
                ModeStateTopic = modeStateTopic,
                TemperatureCommandTopic = temperatureCommandTopic,
                TemperatureStateTopic = temperatureStateTopic,
                CurrentTemperatureTopic = currentTemperatureTopic,
                Modes = modes,
                MinTemp = minTemp,
                MaxTemp = maxTemp,
                TempStep = tempStep,
                TemperatureUnit = temperatureUnit
            };
            RegisterDiscoveryEntity(discovery);

            string initialMode = modes.Length > 0 ? modes[0] : "off";
            var runtime = new HomeAssistantClimate(discovery, initialMode, PublishMessage);
            RegisterRuntimeEntity(runtime);
            return runtime;
        }

        /// <summary>
        /// Connects to the MQTT broker and publishes discovery configuration.
        /// </summary>
        /// <param name="willTopic">Topic for the last-will-testament message. When <c>null</c> (the default), auto-generated from the device ID. Pass <see cref="string.Empty"/> to connect without an LWT (availability publishing still uses the default topic). A non-empty custom value also becomes <see cref="AvailabilityTopic"/> for this session, so <see cref="PublishOnline"/>, <see cref="Disconnect"/>, and discovery payloads stay consistent with the broker's LWT topic.</param>
        /// <param name="willMessage">Payload for LWT (usually "offline").</param>
        /// <returns>True if connection successful, false otherwise.</returns>
        public bool Connect(string willTopic = null, string willMessage = "offline")
        {
            lock (_mqttLock)
            {
                string clientId = null;
                MqttClient client = null;
                try
                {
                    Disconnect();

                    client = new MqttClient(_brokerAddress, _brokerPort, false, null, null, MqttSslProtocols.None);
                    client.ConnectionClosed += OnInternalMqttConnectionClosed;
                    client.MqttMsgPublishReceived += OnInternalMqttMessageReceived;
                    client.MqttMsgPublished += OnInternalMqttMessagePublished;

                    clientId = _mqttClientIdPrefix + Guid.NewGuid().ToString();

                    // Auto-generate the will topic from the device ID unless the caller overrode it
                    // (or explicitly opted out of an LWT by passing string.Empty).
                    string effectiveWillTopic = willTopic ?? GenerateAvailabilityTopic();

                    _availabilityTopic = string.IsNullOrEmpty(effectiveWillTopic) ? GenerateAvailabilityTopic() : effectiveWillTopic;

                    // Connect with LWT if a will topic is in effect
                    if (!string.IsNullOrEmpty(effectiveWillTopic))
                    {
                        client.Connect(
                            clientId,
                            _mqttUsername,
                            _mqttPassword,
                            willRetain: true,
                            willQosLevel: MqttQoSLevel.AtLeastOnce,
                            willFlag: true,
                            willTopic: effectiveWillTopic,
                            willMessage: willMessage,
                            cleanSession: true,
                            keepAlivePeriod: 60);
                    }
                    else
                    {
                        client.Connect(
                            clientId,
                            _mqttUsername,
                            _mqttPassword,
                            cleanSession: true,
                            keepAlivePeriod: 60);
                    }

                    if (!client.IsConnected)
                    {
                        client.ConnectionClosed -= OnInternalMqttConnectionClosed;
                        client.MqttMsgPublishReceived -= OnInternalMqttMessageReceived;
                        client.MqttMsgPublished -= OnInternalMqttMessagePublished;
                        return false;
                    }

                    _mqttClient = client;

                    // Auto-subscribe to all entity command topics
                    AutoSubscribeToEntityCommandTopics();

                    // Publish online and discovery
                    PublishOnline();
                    PublishDiscovery();

                    MqttConnected?.Invoke(this, EventArgs.Empty);
                    return true;
                }
                catch (Exception ex)
                {
                    try
                    {
                        if (client != null)
                        {
                            client.ConnectionClosed -= OnInternalMqttConnectionClosed;
                            client.MqttMsgPublishReceived -= OnInternalMqttMessageReceived;
                            client.MqttMsgPublished -= OnInternalMqttMessagePublished;

                            if (client.IsConnected)
                            {
                                client.Disconnect();
                            }
                        }
                    }
                    catch (Exception cleanupEx)
                    {
                        Debug.WriteLine("MQTT connect rollback failed. clientId=" + clientId + ", error=" + cleanupEx.Message);
                    }
                    finally
                    {
                        if (_mqttClient == client)
                        {
                            _mqttClient = null;
                        }
                    }

                    Debug.WriteLine("MQTT connect failed. broker=" + _brokerAddress + ":" + _brokerPort + ", clientId=" + clientId + ", error=" + ex.Message);
                    return false;
                }
            }
        }

        /// <summary>
        /// Disconnects from the MQTT broker.
        /// </summary>
        public void Disconnect()
        {
            lock (_mqttLock)
            {
                if (_mqttClient == null)
                {
                    return;
                }

                try
                {
                    _mqttClient.ConnectionClosed -= OnInternalMqttConnectionClosed;
                    _mqttClient.MqttMsgPublishReceived -= OnInternalMqttMessageReceived;
                    _mqttClient.MqttMsgPublished -= OnInternalMqttMessagePublished;

                    if (_mqttClient.IsConnected)
                    {
                        try
                        {
                            // Publish offline explicitly before disconnect
                            PublishRetained(_availabilityTopic, "offline");
                            _mqttClient.Disconnect();
                        }
                        catch (Exception ex)
                        {
                            bool isConnected = _mqttClient != null && _mqttClient.IsConnected;
                            Debug.WriteLine("MQTT disconnect publish/disconnect failed. connected=" + isConnected + ", error=" + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("MQTT disconnect cleanup failed. connected=" + _mqttClient.IsConnected + ", error=" + ex.Message);
                }
                finally
                {
                    _mqttClient = null;
                }
            }
        }

        /// <summary>
        /// Subscribes to MQTT topics with specified QoS levels.
        /// </summary>
        /// <returns><c>true</c> when subscription succeeds; otherwise, <c>false</c>.</returns>
        public bool Subscribe(string[] topics, MqttQoSLevel[] qosLevels)
        {
            lock (_mqttLock)
            {
                if (_mqttClient == null || !_mqttClient.IsConnected)
                {
                    return false;
                }

                try
                {
                    _mqttClient.Subscribe(topics, qosLevels);
                    return true;
                }
                catch (Exception ex)
                {
                    int topicCount = topics == null ? 0 : topics.Length;
                    bool isConnected = _mqttClient != null && _mqttClient.IsConnected;
                    Debug.WriteLine("MQTT subscribe failed. topics=" + topicCount + ", connected=" + isConnected + ", error=" + ex.Message);
                    return false;
                }
            }
        }

        /// <summary>
        /// Auto-subscribes to all entity command topics.
        /// Called during Connect() to enable automatic message routing to entities.
        /// </summary>
        private void AutoSubscribeToEntityCommandTopics()
        {
            if (_mqttClient == null || !_mqttClient.IsConnected)
            {
                return;
            }

            var topics = new ArrayList();
            var qosLevels = new ArrayList();

            // Collect all entity command topics plus Home Assistant status topic
            for (int i = 0; i < _runtimeEntities.Count; i++)
            {
                HomeAssistantRuntimeEntity entity = (HomeAssistantRuntimeEntity)_runtimeEntities[i];
                string[] commandTopics = entity.Discovery.GetCommandTopics();
                for (int j = 0; j < commandTopics.Length; j++)
                {
                    string commandTopic = commandTopics[j];
                    if (string.IsNullOrEmpty(commandTopic))
                    {
                        continue;
                    }

                    bool exists = false;
                    for (int k = 0; k < topics.Count; k++)
                    {
                        if ((string)topics[k] == commandTopic)
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (!exists)
                    {
                        topics.Add(commandTopic);
                        qosLevels.Add(MqttQoSLevel.AtLeastOnce);
                    }
                }
            }

            // Always subscribe to Home Assistant status topic for restart detection
            topics.Add(HomeAssistantTopics.StatusTopic);
            qosLevels.Add(MqttQoSLevel.AtLeastOnce);

            // Subscribe to all collected topics
            if (topics.Count > 0)
            {
                try
                {
                    string[] topicsArray = new string[topics.Count];
                    MqttQoSLevel[] qosArray = new MqttQoSLevel[qosLevels.Count];
                    for (int i = 0; i < topics.Count; i++)
                    {
                        topicsArray[i] = (string)topics[i];
                        qosArray[i] = (MqttQoSLevel)qosLevels[i];
                    }

                    _mqttClient.Subscribe(topicsArray, qosArray);
                }
                catch (Exception ex)
                {
                    bool isConnected = _mqttClient != null && _mqttClient.IsConnected;
                    Debug.WriteLine("MQTT auto-subscribe failed. topics=" + topics.Count + ", connected=" + isConnected + ", error=" + ex.Message);
                }
            }
        }

        /// <summary>
        /// Publishes retained online availability status.
        /// </summary>
        public void PublishOnline()
        {
            lock (_mqttLock)
            {
                PublishRetained(_availabilityTopic, "online");
            }
        }

        /// <summary>
        /// Publishes retained offline availability status.
        /// </summary>
        public void PublishOffline()
        {
            lock (_mqttLock)
            {
                PublishRetained(_availabilityTopic, "offline");
            }
        }

        /// <summary>
        /// Publishes retained discovery payloads for all registered entities.
        /// </summary>
        public void PublishDiscovery()
        {
            if (_discoveryEntities.Count == 0)
            {
                return;
            }

            lock (_mqttLock)
            {
                string deviceFull = _device == null ? null : _device.ToFullJson();
                string deviceRef = _device == null ? null : _device.ToReferenceJson();
                bool fullDevicePublished = false;

                for (int i = 0; i < _discoveryEntities.Count; i++)
                {
                    HomeAssistantDiscoveryEntity entity = (HomeAssistantDiscoveryEntity)_discoveryEntities[i];
                    if (entity == null)
                    {
                        continue;
                    }

                    string deviceJson = null;
                    if (entity.IncludeDevice && _device != null)
                    {
                        if (!fullDevicePublished && entity.PreferFullDevice)
                        {
                            deviceJson = deviceFull;
                            fullDevicePublished = true;
                        }
                        else
                        {
                            deviceJson = deviceRef;
                        }
                    }

                    string topic = entity.BuildDiscoveryTopic(HomeAssistantTopics.DiscoveryPrefix);
                    string payload = entity.BuildConfigPayload(_availabilityTopic, deviceJson);
                    PublishRetained(topic, payload);
                }
            }
        }

        /// <summary>
        /// Checks whether an incoming message is the Home Assistant online event.
        /// </summary>
        /// <returns><c>true</c> when the message indicates Home Assistant is online; otherwise, <c>false</c>.</returns>
        public bool IsHomeAssistantOnlineEvent(string topic, string payload)
        {
            if (topic != HomeAssistantTopics.StatusTopic || payload == null)
            {
                return false;
            }

            return payload.Trim().ToUpper() == "ONLINE";
        }

        /// <summary>
        /// Publishes a message to an MQTT topic.
        /// </summary>
        /// <param name="topic">The MQTT topic.</param>
        /// <param name="payload">The message payload.</param>
        /// <param name="retain">Whether to retain the message on the broker.</param>
        public void Publish(string topic, string payload, bool retain = true)
        {
            PublishMessage(topic, payload, retain);
        }

        /// <summary>
        /// Finds a runtime entity by its command topic.
        /// </summary>
        /// <param name="commandTopic">The command topic to find.</param>
        /// <returns>The matching runtime entity, or null if not found.</returns>
        public HomeAssistantRuntimeEntity FindByCommandTopic(string commandTopic)
        {
            if (string.IsNullOrEmpty(commandTopic))
            {
                return null;
            }

            for (int i = 0; i < _runtimeEntities.Count; i++)
            {
                HomeAssistantRuntimeEntity entity = (HomeAssistantRuntimeEntity)_runtimeEntities[i];
                string[] commandTopics = entity.Discovery.GetCommandTopics();
                for (int j = 0; j < commandTopics.Length; j++)
                {
                    if (commandTopics[j] == commandTopic)
                    {
                        return entity;
                    }
                }
            }

            return null;
        }

        private void PublishRetained(string topic, string payload)
        {
            if (string.IsNullOrEmpty(topic) || payload == null)
            {
                return;
            }

            PublishMessage(topic, payload, true);
        }

        private void PublishMessage(string topic, string payload, bool retain)
        {
            lock (_mqttLock)
            {
                if (_mqttClient == null || !_mqttClient.IsConnected)
                {
                    return;
                }

                try
                {
                    _mqttClient.Publish(
                        topic,
                        Encoding.UTF8.GetBytes(payload),
                        null,
                        null,
                        MqttQoSLevel.AtLeastOnce,
                        retain);
                }
                catch (Exception ex)
                {
                    bool isConnected = _mqttClient != null && _mqttClient.IsConnected;
                    Debug.WriteLine("MQTT publish failed. topic=" + topic + ", retain=" + retain + ", connected=" + isConnected + ", error=" + ex.Message);
                }
            }
        }

        private void OnInternalMqttConnectionClosed(object sender, EventArgs e)
        {
            // Single-owner reconnect pattern:
            // this component reports connection loss, and the application owns retry policy.
            lock (_mqttLock)
            {
                _mqttClient = null;
            }

            MqttConnectionClosed?.Invoke(this, e);
        }

        private void OnInternalMqttMessageReceived(object sender, MqttMsgPublishEventArgs e)
        {
            // Global message handler: loop through all entities to find command topic match
            lock (_mqttLock)
            {
                for (int i = 0; i < _runtimeEntities.Count; i++)
                {
                    HomeAssistantRuntimeEntity entity = (HomeAssistantRuntimeEntity)_runtimeEntities[i];
                    string[] commandTopics = entity.Discovery.GetCommandTopics();
                    bool matched = false;
                    for (int j = 0; j < commandTopics.Length; j++)
                    {
                        if (commandTopics[j] == e.Topic)
                        {
                            matched = true;
                            break;
                        }
                    }

                    if (matched)
                    {
                        // Route message to this entity via its SetState method
                        // SetState triggers OnStateChange only.
                        // The application should publish confirmed state after successful actuation.
                        string payload = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length).Trim();
                        entity.SetState(payload);
                        break;
                    }
                }
            }

            // Also invoke external event handler for any additional processing
            MqttMessageReceived?.Invoke(this, e);
        }

        private void OnInternalMqttMessagePublished(object sender, MqttMsgPublishedEventArgs e)
        {
            MqttMessagePublished?.Invoke(this, e);
        }

        private string BuildOptionsJson(string[] options)
        {
            StringBuilder json = new StringBuilder();
            json.Append("\"options\": [");

            for (int i = 0; i < options.Length; i++)
            {
                if (i > 0)
                {
                    json.Append(", ");
                }

                json.Append('"');
                json.Append(MiniJson.Escape(options[i]));
                json.Append('"');
            }

            json.Append("]");
            return json.ToString();
        }

        private void RegisterDiscoveryEntity(HomeAssistantDiscoveryEntity discovery)
        {
            if (discovery == null)
            {
                return;
            }

            if (_discoveryEntities.Count == 0)
            {
                discovery.PreferFullDevice = true;
            }

            _discoveryEntities.Add(discovery);

            if (IsConnected)
            {
                PublishDiscoveryEntity(discovery);
            }
        }

        private void RegisterRuntimeEntity(HomeAssistantRuntimeEntity runtime)
        {
            if (runtime == null)
            {
                return;
            }

            _runtimeEntities.Add(runtime);

            if (IsConnected)
            {
                SubscribeEntityCommandTopics(runtime);
            }
        }

        private void PublishDiscoveryEntity(HomeAssistantDiscoveryEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            lock (_mqttLock)
            {
                string deviceJson = null;
                if (entity.IncludeDevice && _device != null)
                {
                    deviceJson = entity.PreferFullDevice ? _device.ToFullJson() : _device.ToReferenceJson();
                }

                string topic = entity.BuildDiscoveryTopic(HomeAssistantTopics.DiscoveryPrefix);
                string payload = entity.BuildConfigPayload(_availabilityTopic, deviceJson);
                PublishRetained(topic, payload);
            }
        }

        private void SubscribeEntityCommandTopics(HomeAssistantRuntimeEntity entity)
        {
            if (entity == null || entity.Discovery == null)
            {
                return;
            }

            string[] commandTopics = entity.Discovery.GetCommandTopics();
            if (commandTopics == null || commandTopics.Length == 0)
            {
                return;
            }

            var filteredTopics = new ArrayList();
            for (int i = 0; i < commandTopics.Length; i++)
            {
                string commandTopic = commandTopics[i];
                if (string.IsNullOrEmpty(commandTopic))
                {
                    continue;
                }

                bool exists = false;
                for (int j = 0; j < filteredTopics.Count; j++)
                {
                    if ((string)filteredTopics[j] == commandTopic)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    filteredTopics.Add(commandTopic);
                }
            }

            if (filteredTopics.Count == 0)
            {
                return;
            }

            string[] topics = new string[filteredTopics.Count];
            MqttQoSLevel[] qos = new MqttQoSLevel[filteredTopics.Count];
            for (int i = 0; i < filteredTopics.Count; i++)
            {
                topics[i] = (string)filteredTopics[i];
                qos[i] = MqttQoSLevel.AtLeastOnce;
            }

            Subscribe(topics, qos);
        }
    }
}
