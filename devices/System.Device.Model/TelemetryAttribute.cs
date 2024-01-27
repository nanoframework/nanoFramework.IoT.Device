// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Model
{
    /// <summary>
    /// Telemetry of the interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TelemetryAttribute : Attribute
    {
        /// <summary>
        /// Gets name of the telemetry in the interface.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets display name of the telemetry.
        /// </summary>
        public string? DisplayName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryAttribute" /> class.
        /// </summary>
        /// <param name="name">Name of the telemetry. If not provided property name will be used.</param>
        /// <param name="displayName">Optional display name of the telemetry. If not provided it may be infered from the type.</param>
        /// <remarks>When put on methods name should always be provided.</remarks>
        public TelemetryAttribute(string? name, string? displayName)
        {
            Name = name;
            DisplayName = displayName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryAttribute" /> class.
        /// </summary>
        /// <param name="name">Name of the telemetry. If not provided property name will be used.</param>
        /// <remarks>When put on methods name should always be provided.</remarks>
        public TelemetryAttribute(string? name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryAttribute" /> class.
        /// </summary>
        /// <remarks>When put on methods name should always be provided.</remarks>
        public TelemetryAttribute()
        {
        }
    }
}
