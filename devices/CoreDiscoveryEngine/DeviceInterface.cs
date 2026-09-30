// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.CoreDiscoveryEngine
{
    /// <summary>
    /// Describes a device interface and its nested components.
    /// </summary>
    public class DeviceInterface
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceInterface"/> class.
        /// </summary>
        /// <param name="name">The interface display name.</param>
        /// <param name="deviceType">The reflected device type.</param>
        /// <param name="path">The path relative to the root device.</param>
        /// <param name="capabilities">The declared capabilities.</param>
        /// <param name="components">The nested component interfaces.</param>
        public DeviceInterface(string name, Type deviceType, string path, Capability[] capabilities, DeviceInterface[] components)
        {
            Name = name;
            DeviceType = deviceType;
            Path = path;
            Capabilities = capabilities;
            Components = components;
        }

        /// <summary>Gets the interface display name.</summary>
        public string Name { get; }

        /// <summary>Gets the reflected device type.</summary>
        public Type DeviceType { get; }

        /// <summary>Gets the path relative to the root device.</summary>
        public string Path { get; }

        /// <summary>Gets the capabilities declared by this interface.</summary>
        public Capability[] Capabilities { get; }

        /// <summary>Gets nested component interfaces.</summary>
        public DeviceInterface[] Components { get; }
    }
}