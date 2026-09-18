// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.DeviceModel.Reflection
{
    /// <summary>
    /// A device type together with the capabilities discovered on it by <see cref="CapabilityDiscovery"/>.
    /// </summary>
    public class DeviceInterface
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceInterface" /> class.
        /// </summary>
        /// <param name="name">The interface's display name.</param>
        /// <param name="type">The reflected device type.</param>
        /// <param name="capabilities">The telemetry, property, and command capabilities discovered on this interface.</param>
        public DeviceInterface(string name, Type type, Capability[] capabilities)
        {
            Name = name;
            Type = type;
            Capabilities = capabilities;
        }

        /// <summary>Gets the interface's display name.</summary>
        public string Name { get; }

        /// <summary>Gets the reflected device type.</summary>
        public Type Type { get; }

        /// <summary>Gets the telemetry, property, and command capabilities discovered on this interface.</summary>
        public Capability[] Capabilities { get; }
    }
}
