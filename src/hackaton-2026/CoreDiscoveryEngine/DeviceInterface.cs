// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace nanoFramework.IoT.Device.CoreDiscoveryEngine
{
    /// <summary>A device interface and its discovered capabilities.</summary>
    public class DeviceInterface
    {
        internal DeviceInterface(string name, Type type, string path, Capability[] capabilities, DeviceInterface[] components)
        {
            Name = name;
            Type = type;
            Path = path;
            Capabilities = capabilities;
            Components = components;
        }

        /// <summary>Gets the interface display name.</summary>
        public string Name { get; }

        /// <summary>Gets the reflected interface type.</summary>
        public Type Type { get; }

        /// <summary>Gets the hierarchical path of the interface.</summary>
        public string Path { get; }

        /// <summary>Gets the capabilities directly exposed by the interface.</summary>
        public Capability[] Capabilities { get; }

        /// <summary>Gets the child interfaces exposed by the interface.</summary>
        public DeviceInterface[] Components { get; }
    }
}