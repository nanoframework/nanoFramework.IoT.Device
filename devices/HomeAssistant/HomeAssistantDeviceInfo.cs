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
        private string _id;
        private string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeAssistantDeviceInfo" /> class.
        /// </summary>
        /// <param name="id">Device identifier shared by all entities.</param>
        /// <param name="name">Device display name.</param>
        /// <param name="model">Device model name.</param>
        /// <param name="manufacturer">Device manufacturer.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> or <paramref name="name"/> is null or empty.</exception>
        public HomeAssistantDeviceInfo(string id, string name, string model = null, string manufacturer = null)
        {
            Id = id;
            Name = name;
            Model = model ?? string.Empty;
            Manufacturer = manufacturer ?? string.Empty;
        }

        /// <summary>
        /// Gets or sets the device identifier shared by all entities.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when value is null or empty.</exception>
        public string Id
        {
            get
            {
                return _id;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Device id cannot be null or empty.");
                }

                _id = value;
            }
        }

        /// <summary>
        /// Gets or sets the device display name.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when value is null or empty.</exception>
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Device name cannot be null or empty.");
                }

                _name = value;
            }
        }

        /// <summary>
        /// Gets or sets the device model name.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets the device manufacturer.
        /// </summary>
        public string Manufacturer { get; set; }

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
