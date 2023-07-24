// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace nanoFramework.M2Mqtt.Messages
{
    /// <summary>
    /// A user property is a key value string pair, v5.0 only.
    /// </summary>
    public class UserProperty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserProperty"/> class.
        /// </summary>
        /// <param name="name">Name of the property.</param>
        /// <param name="value">Value of the property.</param>
        public UserProperty(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        public string Value { get; set; }
    }
}