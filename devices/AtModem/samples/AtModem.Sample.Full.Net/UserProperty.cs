using System;

namespace nanoFramework.M2Mqtt.Messages
{
    /// <summary>
    /// A user property is a key value string pair, v5.0 only.
    /// </summary>
    public class UserProperty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserProperty"/> class with the specified name and value.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        public UserProperty(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value { get; set; }
    }
}
