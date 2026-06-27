// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

            // Common fields for all entity types
            MiniJson.AppendStringProperty(json, ref first, "name", Name);
            MiniJson.AppendStringProperty(json, ref first, "uniq_id", UniqueId);
            MiniJson.AppendStringProperty(json, ref first, "stat_t", StateTopic);

            // Writable entity types (Switch, Number, Select, Text) include command topic
            // Read-only types (Sensor) do not include command topic
            bool isWritable = ComponentType == HomeAssistantComponentType.Switch
                || ComponentType == HomeAssistantComponentType.Number
                || ComponentType == HomeAssistantComponentType.Select
                || ComponentType == HomeAssistantComponentType.Text;

            if (isWritable)
            {
                MiniJson.AppendStringProperty(json, ref first, "cmd_t", CommandTopic);
            }

            // Payload on/off only for switches and binary sensors
            if (ComponentType == HomeAssistantComponentType.Switch
                || ComponentType == HomeAssistantComponentType.BinarySensor)
            {
                MiniJson.AppendStringProperty(json, ref first, "pl_on", PayloadOn);
                MiniJson.AppendStringProperty(json, ref first, "pl_off", PayloadOff);
            }

            // Min/max/step only for number entities
            if (ComponentType == HomeAssistantComponentType.Number)
            {
                MiniJson.AppendStringProperty(json, ref first, "min", Min, true);
                MiniJson.AppendStringProperty(json, ref first, "max", Max, true);
                MiniJson.AppendStringProperty(json, ref first, "step", Step, true);
            }

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

        private static string ToComponentName(HomeAssistantComponentType componentType)
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
