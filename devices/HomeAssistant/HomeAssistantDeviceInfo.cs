// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text;

namespace nanoFramework.HomeAssistant
{
    /// <summary>
    /// Represents Home Assistant MQTT discovery device metadata.
    /// </summary>
    public sealed class HomeAssistantDeviceInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HomeAssistantDeviceInfo" /> class.
        /// </summary>
        /// <param name="id">Device identifier shared by all entities. Device id cannot be null or empty.</param>
        /// <param name="name">Device display name. Device name cannot be null or empty.</param>
        /// <param name="model">Device model name.</param>
        /// <param name="manufacturer">Device manufacturer.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> or <paramref name="name"/> is null or empty.</exception>
        public HomeAssistantDeviceInfo(string id, string name, string model = null, string manufacturer = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException();
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException();
            }

            Id = id;
            Name = name;
            Model = model ?? string.Empty;
            Manufacturer = manufacturer ?? string.Empty;
        }

        /// <summary>
        /// Gets the device identifier shared by all entities.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the device display name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the device model name.
        /// </summary>
        public string Model { get; }

        /// <summary>
        /// Gets the device manufacturer.
        /// </summary>
        public string Manufacturer { get; }

        /// <summary>
        /// Builds a full Home Assistant device JSON object.
        /// </summary>
        /// <returns>JSON object string with full device metadata.</returns>
        public string ToFullJson()
        {
            StringBuilder json = new StringBuilder();
            json.Append('{');
            bool first = true;

            MiniJson.AppendStringProperty(json, ref first, "ids", Id);
            MiniJson.AppendStringProperty(json, ref first, "name", Name);
            if (!string.IsNullOrEmpty(Model))
            {
                MiniJson.AppendStringProperty(json, ref first, "mdl", Model);
            }

            if (!string.IsNullOrEmpty(Manufacturer))
            {
                MiniJson.AppendStringProperty(json, ref first, "mf", Manufacturer);
            }

            json.Append('}');
            return json.ToString();
        }

        /// <summary>
        /// Builds a compact Home Assistant device JSON object containing only the id.
        /// </summary>
        /// <returns>JSON object string with the device identifier.</returns>
        public string ToReferenceJson()
        {
            StringBuilder json = new StringBuilder();
            json.Append('{');
            bool first = true;
            MiniJson.AppendStringProperty(json, ref first, "ids", Id);
            json.Append('}');
            return json.ToString();
        }
    }
}
