// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Model
{
    /// <summary>
    /// Property of the interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class PropertyAttribute : Attribute
    {
        /// <summary>
        /// Gets name of the property in the interface.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets display name of the property.
        /// </summary>
        public string? DisplayName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAttribute" /> class.
        /// </summary>
        /// <param name="name">Optional name of the property in the interface. If not provided property name will be used.</param>
        /// <param name="displayName">Optional name of the property in the interface. If not provided it may be infered from the type.</param>
        public PropertyAttribute(string? name, string? displayName)
        {
            Name = name;
            DisplayName = displayName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAttribute" /> class.
        /// </summary>
        /// <param name="name">Optional name of the property in the interface. If not provided property name will be used.</param>
        public PropertyAttribute(string? name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAttribute" /> class.
        /// </summary>
        public PropertyAttribute()
        {
        }
    }
}
