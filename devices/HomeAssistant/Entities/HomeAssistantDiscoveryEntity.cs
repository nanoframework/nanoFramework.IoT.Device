// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Text;

namespace nanoFramework.HomeAssistant
{
    /// <summary>
    /// Defines a Home Assistant MQTT discovery entity configuration.
    /// </summary>
    public sealed class HomeAssistantDiscoveryEntity
    {
        /// <summary>
        /// Gets or sets the Home Assistant component type.
        /// </summary>
        public HomeAssistantComponentType ComponentType { get; set; }

        /// <summary>
        /// Gets or sets the entity object id used in the discovery topic.
        /// </summary>
        public string ObjectId { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the unique id.
        /// </summary>
        public string UniqueId { get; set; }

        /// <summary>
        /// Gets or sets the state topic.
        /// </summary>
        public string StateTopic { get; set; }

        /// <summary>
        /// Gets or sets the command topic.
        /// </summary>
        public string CommandTopic { get; set; }

        /// <summary>
        /// Gets or sets the payload used for ON commands/states.
        /// </summary>
        public string PayloadOn { get; set; }

        /// <summary>
        /// Gets or sets the payload used for OFF commands/states.
        /// </summary>
        public string PayloadOff { get; set; }

        /// <summary>
        /// Gets or sets the minimum numeric value.
        /// </summary>
        public string Min { get; set; }

        /// <summary>
        /// Gets or sets the maximum numeric value.
        /// </summary>
        public string Max { get; set; }

        /// <summary>
        /// Gets or sets the numeric step value.
        /// </summary>
        public string Step { get; set; }

        /// <summary>
        /// Gets or sets the unit of measurement.
        /// </summary>
        public string UnitOfMeasurement { get; set; }

        /// <summary>
        /// Gets or sets the Home Assistant input mode.
        /// </summary>
        public string Mode { get; set; }

        /// <summary>
        /// Gets or sets the Home Assistant device class.
        /// </summary>
        public string DeviceClass { get; set; }

        /// <summary>
        /// Gets or sets the Home Assistant state class.
        /// </summary>
        public string StateClass { get; set; }

        /// <summary>
        /// Gets or sets the value template.
        /// </summary>
        public string ValueTemplate { get; set; }

        /// <summary>
        /// Gets or sets the Home Assistant entity category.
        /// </summary>
        public string EntityCategory { get; set; }

        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets additional raw JSON properties appended to the discovery payload.
        /// </summary>
        public string[] ExtraProperties { get; set; }

        /// <summary>
        /// Gets or sets the payload sent when a button entity is pressed.
        /// </summary>
        public string PayloadPress { get; set; }

        /// <summary>
        /// Gets or sets the light schema (for example, basic, json, template).
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Gets or sets the brightness command topic for light entities.
        /// </summary>
        public string BrightnessCommandTopic { get; set; }

        /// <summary>
        /// Gets or sets the brightness state topic for light entities.
        /// </summary>
        public string BrightnessStateTopic { get; set; }

        /// <summary>
        /// Gets or sets the brightness scale for light entities.
        /// </summary>
        public string BrightnessScale { get; set; }

        /// <summary>
        /// Gets or sets the color temperature command topic for light entities.
        /// </summary>
        public string ColorTempCommandTopic { get; set; }

        /// <summary>
        /// Gets or sets the color temperature state topic for light entities.
        /// </summary>
        public string ColorTempStateTopic { get; set; }

        /// <summary>
        /// Gets or sets the RGB command topic for light entities.
        /// </summary>
        public string RgbCommandTopic { get; set; }

        /// <summary>
        /// Gets or sets the RGB state topic for light entities.
        /// </summary>
        public string RgbStateTopic { get; set; }

        /// <summary>
        /// Gets or sets the effect command topic for light entities.
        /// </summary>
        public string EffectCommandTopic { get; set; }

        /// <summary>
        /// Gets or sets the effect state topic for light entities.
        /// </summary>
        public string EffectStateTopic { get; set; }

        /// <summary>
        /// Gets or sets the supported effects for light entities.
        /// </summary>
        public string[] EffectList { get; set; }

        /// <summary>
        /// Gets or sets the supported color modes for light entities.
        /// </summary>
        public string[] SupportedColorModes { get; set; }

        /// <summary>
        /// Gets or sets the position topic for cover entities.
        /// </summary>
        public string PositionTopic { get; set; }

        /// <summary>
        /// Gets or sets the set-position command topic for cover entities.
        /// </summary>
        public string SetPositionTopic { get; set; }

        /// <summary>
        /// Gets or sets the payload used to open a cover.
        /// </summary>
        public string PayloadOpen { get; set; }

        /// <summary>
        /// Gets or sets the payload used to close a cover.
        /// </summary>
        public string PayloadClose { get; set; }

        /// <summary>
        /// Gets or sets the payload used to stop a cover.
        /// </summary>
        public string PayloadStop { get; set; }

        /// <summary>
        /// Gets or sets the payload value representing open state for cover entities.
        /// </summary>
        public string StateOpen { get; set; }

        /// <summary>
        /// Gets or sets the payload value representing opening state for cover entities.
        /// </summary>
        public string StateOpening { get; set; }

        /// <summary>
        /// Gets or sets the payload value representing closed state for cover entities.
        /// </summary>
        public string StateClosed { get; set; }

        /// <summary>
        /// Gets or sets the payload value representing closing state for cover entities.
        /// </summary>
        public string StateClosing { get; set; }

        /// <summary>
        /// Gets or sets the payload value representing stopped state for cover entities.
        /// </summary>
        public string StateStopped { get; set; }

        /// <summary>
        /// Gets or sets the mode command topic for climate entities.
        /// </summary>
        public string ModeCommandTopic { get; set; }

        /// <summary>
        /// Gets or sets the mode state topic for climate entities.
        /// </summary>
        public string ModeStateTopic { get; set; }

        /// <summary>
        /// Gets or sets the supported operation modes for climate entities.
        /// </summary>
        public string[] Modes { get; set; }

        /// <summary>
        /// Gets or sets the temperature command topic for climate entities.
        /// </summary>
        public string TemperatureCommandTopic { get; set; }

        /// <summary>
        /// Gets or sets the temperature state topic for climate entities.
        /// </summary>
        public string TemperatureStateTopic { get; set; }

        /// <summary>
        /// Gets or sets the current temperature topic for climate entities.
        /// </summary>
        public string CurrentTemperatureTopic { get; set; }

        /// <summary>
        /// Gets or sets the action topic for climate entities.
        /// </summary>
        public string ActionTopic { get; set; }

        /// <summary>
        /// Gets or sets the action template for climate entities.
        /// </summary>
        public string ActionTemplate { get; set; }

        /// <summary>
        /// Gets or sets the fan mode command topic for climate entities.
        /// </summary>
        public string FanModeCommandTopic { get; set; }

        /// <summary>
        /// Gets or sets the fan mode state topic for climate entities.
        /// </summary>
        public string FanModeStateTopic { get; set; }

        /// <summary>
        /// Gets or sets the supported fan modes for climate entities.
        /// </summary>
        public string[] FanModes { get; set; }

        /// <summary>
        /// Gets or sets the preset mode command topic for climate entities.
        /// </summary>
        public string PresetModeCommandTopic { get; set; }

        /// <summary>
        /// Gets or sets the preset mode state topic for climate entities.
        /// </summary>
        public string PresetModeStateTopic { get; set; }

        /// <summary>
        /// Gets or sets the supported preset modes for climate entities.
        /// </summary>
        public string[] PresetModes { get; set; }

        /// <summary>
        /// Gets or sets the swing mode command topic for climate entities.
        /// </summary>
        public string SwingModeCommandTopic { get; set; }

        /// <summary>
        /// Gets or sets the swing mode state topic for climate entities.
        /// </summary>
        public string SwingModeStateTopic { get; set; }

        /// <summary>
        /// Gets or sets the supported swing modes for climate entities.
        /// </summary>
        public string[] SwingModes { get; set; }

        /// <summary>
        /// Gets or sets the power command topic for climate entities.
        /// </summary>
        public string PowerCommandTopic { get; set; }

        /// <summary>
        /// Gets or sets the minimum temperature for climate entities.
        /// </summary>
        public string MinTemp { get; set; }

        /// <summary>
        /// Gets or sets the maximum temperature for climate entities.
        /// </summary>
        public string MaxTemp { get; set; }

        /// <summary>
        /// Gets or sets the temperature step for climate entities.
        /// </summary>
        public string TempStep { get; set; }

        /// <summary>
        /// Gets or sets the precision for climate entities.
        /// </summary>
        public string Precision { get; set; }

        /// <summary>
        /// Gets or sets the temperature unit for climate entities (C or F).
        /// </summary>
        public string TemperatureUnit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether availability topic is included.
        /// </summary>
        public bool IncludeAvailability { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether device metadata is included.
        /// </summary>
        public bool IncludeDevice { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this entity should publish the full device block.
        /// </summary>
        public bool PreferFullDevice { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeAssistantDiscoveryEntity" /> class.
        /// </summary>
        public HomeAssistantDiscoveryEntity()
        {
            IncludeAvailability = true;
            IncludeDevice = true;
        }

        /// <summary>
        /// Builds the Home Assistant MQTT discovery topic for this entity.
        /// </summary>
        /// <param name="discoveryPrefix">Discovery prefix, typically homeassistant.</param>
        /// <returns>Full MQTT discovery topic for this entity.</returns>
        public string BuildDiscoveryTopic(string discoveryPrefix)
        {
            return discoveryPrefix + "/" + ToComponentName(ComponentType) + "/" + ObjectId + "/config";
        }

        /// <summary>
        /// Builds the discovery configuration payload JSON.
        /// </summary>
        /// <param name="availabilityTopic">Availability topic to include.</param>
        /// <param name="deviceJson">Device JSON object to include in dev field.</param>
        /// <returns>Discovery payload as a JSON object string.</returns>
        public string BuildConfigPayload(string availabilityTopic, string deviceJson)
        {
            StringBuilder json = new StringBuilder();
            json.Append('{');
            bool first = true;

            // Common identity fields
            MiniJson.AppendStringProperty(json, ref first, "name", Name);
            MiniJson.AppendStringProperty(json, ref first, "uniq_id", UniqueId);

            AppendComponentSpecificFields(json, ref first);

            // Optional fields (included only if populated)
            MiniJson.AppendStringProperty(json, ref first, "unit_of_meas", UnitOfMeasurement);
            MiniJson.AppendStringProperty(json, ref first, "mode", Mode);
            MiniJson.AppendStringProperty(json, ref first, "dev_cla", DeviceClass);
            MiniJson.AppendStringProperty(json, ref first, "stat_cla", StateClass);
            MiniJson.AppendStringProperty(json, ref first, "val_tpl", ValueTemplate);
            MiniJson.AppendStringProperty(json, ref first, "entity_category", EntityCategory);
            MiniJson.AppendStringProperty(json, ref first, "icon", Icon);

            if (IncludeAvailability)
            {
                MiniJson.AppendStringProperty(json, ref first, "avty_t", availabilityTopic);
            }

            if (!string.IsNullOrEmpty(deviceJson))
            {
                MiniJson.AppendRawProperty(json, ref first, "dev", deviceJson);
            }

            if (ExtraProperties != null)
            {
                for (int i = 0; i < ExtraProperties.Length; i++)
                {
                    string property = ExtraProperties[i];
                    if (string.IsNullOrEmpty(property))
                    {
                        continue;
                    }

                    if (!first)
                    {
                        json.Append(',');
                    }

                    json.Append(property);
                    first = false;
                }
            }

            json.Append('}');
            return json.ToString();
        }

        /// <summary>
        /// Gets command topics used by this entity.
        /// </summary>
        /// <returns>Array of command topics for subscription and routing.</returns>
        public string[] GetCommandTopics()
        {
            var topics = new ArrayList();

            AddTopicIfMissing(topics, CommandTopic);

            if (ComponentType == HomeAssistantComponentType.Light)
            {
                AddTopicIfMissing(topics, BrightnessCommandTopic);
                AddTopicIfMissing(topics, ColorTempCommandTopic);
                AddTopicIfMissing(topics, RgbCommandTopic);
                AddTopicIfMissing(topics, EffectCommandTopic);
            }

            if (ComponentType == HomeAssistantComponentType.Cover)
            {
                AddTopicIfMissing(topics, SetPositionTopic);
            }

            if (ComponentType == HomeAssistantComponentType.Climate)
            {
                AddTopicIfMissing(topics, ModeCommandTopic);
                AddTopicIfMissing(topics, TemperatureCommandTopic);
                AddTopicIfMissing(topics, FanModeCommandTopic);
                AddTopicIfMissing(topics, PresetModeCommandTopic);
                AddTopicIfMissing(topics, SwingModeCommandTopic);
                AddTopicIfMissing(topics, PowerCommandTopic);
            }

            string[] output = new string[topics.Count];
            for (int i = 0; i < topics.Count; i++)
            {
                output[i] = (string)topics[i];
            }

            return output;
        }

        private void AppendComponentSpecificFields(StringBuilder json, ref bool first)
        {
            if (ComponentType == HomeAssistantComponentType.Button)
            {
                if (string.IsNullOrEmpty(CommandTopic))
                {
                    throw new InvalidOperationException("Button discovery requires CommandTopic.");
                }

                MiniJson.AppendStringProperty(json, ref first, "command_topic", CommandTopic);
                MiniJson.AppendStringProperty(json, ref first, "payload_press", PayloadPress);
                return;
            }

            if (ComponentType == HomeAssistantComponentType.Light)
            {
                if (string.IsNullOrEmpty(CommandTopic))
                {
                    throw new InvalidOperationException("Light discovery requires CommandTopic.");
                }

                MiniJson.AppendStringProperty(json, ref first, "command_topic", CommandTopic);
                MiniJson.AppendStringProperty(json, ref first, "state_topic", StateTopic);
                MiniJson.AppendStringProperty(json, ref first, "payload_on", PayloadOn);
                MiniJson.AppendStringProperty(json, ref first, "payload_off", PayloadOff);
                MiniJson.AppendStringProperty(json, ref first, "schema", Schema);
                MiniJson.AppendStringProperty(json, ref first, "brightness_command_topic", BrightnessCommandTopic);
                MiniJson.AppendStringProperty(json, ref first, "brightness_state_topic", BrightnessStateTopic);
                MiniJson.AppendStringProperty(json, ref first, "brightness_scale", BrightnessScale, true);
                MiniJson.AppendStringProperty(json, ref first, "color_temp_command_topic", ColorTempCommandTopic);
                MiniJson.AppendStringProperty(json, ref first, "color_temp_state_topic", ColorTempStateTopic);
                MiniJson.AppendStringProperty(json, ref first, "rgb_command_topic", RgbCommandTopic);
                MiniJson.AppendStringProperty(json, ref first, "rgb_state_topic", RgbStateTopic);
                MiniJson.AppendStringProperty(json, ref first, "effect_command_topic", EffectCommandTopic);
                MiniJson.AppendStringProperty(json, ref first, "effect_state_topic", EffectStateTopic);
                MiniJson.AppendStringArrayProperty(json, ref first, "effect_list", EffectList);
                MiniJson.AppendStringArrayProperty(json, ref first, "supported_color_modes", SupportedColorModes);
                return;
            }

            if (ComponentType == HomeAssistantComponentType.Cover)
            {
                MiniJson.AppendStringProperty(json, ref first, "command_topic", CommandTopic);
                MiniJson.AppendStringProperty(json, ref first, "state_topic", StateTopic);
                MiniJson.AppendStringProperty(json, ref first, "position_topic", PositionTopic);
                MiniJson.AppendStringProperty(json, ref first, "set_position_topic", SetPositionTopic);
                MiniJson.AppendStringProperty(json, ref first, "payload_open", PayloadOpen);
                MiniJson.AppendStringProperty(json, ref first, "payload_close", PayloadClose);
                MiniJson.AppendStringProperty(json, ref first, "payload_stop", PayloadStop);
                MiniJson.AppendStringProperty(json, ref first, "state_open", StateOpen);
                MiniJson.AppendStringProperty(json, ref first, "state_opening", StateOpening);
                MiniJson.AppendStringProperty(json, ref first, "state_closed", StateClosed);
                MiniJson.AppendStringProperty(json, ref first, "state_closing", StateClosing);
                MiniJson.AppendStringProperty(json, ref first, "state_stopped", StateStopped);
                return;
            }

            if (ComponentType == HomeAssistantComponentType.Climate)
            {
                bool hasClimateCommand = !string.IsNullOrEmpty(ModeCommandTopic)
                    || !string.IsNullOrEmpty(TemperatureCommandTopic)
                    || !string.IsNullOrEmpty(FanModeCommandTopic)
                    || !string.IsNullOrEmpty(PresetModeCommandTopic)
                    || !string.IsNullOrEmpty(SwingModeCommandTopic)
                    || !string.IsNullOrEmpty(PowerCommandTopic);

                if (!hasClimateCommand)
                {
                    throw new InvalidOperationException("Climate discovery requires at least one command topic (for example ModeCommandTopic).");
                }

                MiniJson.AppendStringProperty(json, ref first, "mode_command_topic", ModeCommandTopic);
                MiniJson.AppendStringProperty(json, ref first, "mode_state_topic", ModeStateTopic);
                MiniJson.AppendStringArrayProperty(json, ref first, "modes", Modes);

                MiniJson.AppendStringProperty(json, ref first, "temperature_command_topic", TemperatureCommandTopic);
                MiniJson.AppendStringProperty(json, ref first, "temperature_state_topic", TemperatureStateTopic);
                MiniJson.AppendStringProperty(json, ref first, "current_temperature_topic", CurrentTemperatureTopic);

                MiniJson.AppendStringProperty(json, ref first, "action_topic", ActionTopic);
                MiniJson.AppendStringProperty(json, ref first, "action_template", ActionTemplate);

                MiniJson.AppendStringProperty(json, ref first, "fan_mode_command_topic", FanModeCommandTopic);
                MiniJson.AppendStringProperty(json, ref first, "fan_mode_state_topic", FanModeStateTopic);
                MiniJson.AppendStringArrayProperty(json, ref first, "fan_modes", FanModes);

                MiniJson.AppendStringProperty(json, ref first, "preset_mode_command_topic", PresetModeCommandTopic);
                MiniJson.AppendStringProperty(json, ref first, "preset_mode_state_topic", PresetModeStateTopic);
                MiniJson.AppendStringArrayProperty(json, ref first, "preset_modes", PresetModes);

                MiniJson.AppendStringProperty(json, ref first, "swing_mode_command_topic", SwingModeCommandTopic);
                MiniJson.AppendStringProperty(json, ref first, "swing_mode_state_topic", SwingModeStateTopic);
                MiniJson.AppendStringArrayProperty(json, ref first, "swing_modes", SwingModes);

                MiniJson.AppendStringProperty(json, ref first, "power_command_topic", PowerCommandTopic);
                MiniJson.AppendStringProperty(json, ref first, "payload_on", PayloadOn);
                MiniJson.AppendStringProperty(json, ref first, "payload_off", PayloadOff);

                MiniJson.AppendStringProperty(json, ref first, "min_temp", MinTemp, true);
                MiniJson.AppendStringProperty(json, ref first, "max_temp", MaxTemp, true);
                MiniJson.AppendStringProperty(json, ref first, "temp_step", TempStep, true);
                MiniJson.AppendStringProperty(json, ref first, "precision", Precision, true);
                MiniJson.AppendStringProperty(json, ref first, "temperature_unit", TemperatureUnit);
                return;
            }

            MiniJson.AppendStringProperty(json, ref first, "stat_t", StateTopic);

            bool isWritable = ComponentType == HomeAssistantComponentType.Switch
                || ComponentType == HomeAssistantComponentType.Number
                || ComponentType == HomeAssistantComponentType.Select
                || ComponentType == HomeAssistantComponentType.Text;

            if (isWritable)
            {
                MiniJson.AppendStringProperty(json, ref first, "cmd_t", CommandTopic);
            }

            if (ComponentType == HomeAssistantComponentType.Switch
                || ComponentType == HomeAssistantComponentType.BinarySensor)
            {
                MiniJson.AppendStringProperty(json, ref first, "pl_on", PayloadOn);
                MiniJson.AppendStringProperty(json, ref first, "pl_off", PayloadOff);
            }

            if (ComponentType == HomeAssistantComponentType.Number)
            {
                MiniJson.AppendStringProperty(json, ref first, "min", Min, true);
                MiniJson.AppendStringProperty(json, ref first, "max", Max, true);
                MiniJson.AppendStringProperty(json, ref first, "step", Step, true);
            }
        }

        private void AddTopicIfMissing(ArrayList topics, string topic)
        {
            if (string.IsNullOrEmpty(topic))
            {
                return;
            }

            for (int i = 0; i < topics.Count; i++)
            {
                if ((string)topics[i] == topic)
                {
                    return;
                }
            }

            topics.Add(topic);
        }

        private string ToComponentName(HomeAssistantComponentType componentType)
        {
            if (componentType == HomeAssistantComponentType.Switch)
            {
                return "switch";
            }

            if (componentType == HomeAssistantComponentType.Number)
            {
                return "number";
            }

            if (componentType == HomeAssistantComponentType.Sensor)
            {
                return "sensor";
            }

            if (componentType == HomeAssistantComponentType.BinarySensor)
            {
                return "binary_sensor";
            }

            if (componentType == HomeAssistantComponentType.Button)
            {
                return "button";
            }

            if (componentType == HomeAssistantComponentType.Select)
            {
                return "select";
            }

            if (componentType == HomeAssistantComponentType.Light)
            {
                return "light";
            }

            if (componentType == HomeAssistantComponentType.Cover)
            {
                return "cover";
            }

            if (componentType == HomeAssistantComponentType.Climate)
            {
                return "climate";
            }

            if (componentType == HomeAssistantComponentType.Text)
            {
                return "text";
            }

            return "sensor";
        }
    }
}
