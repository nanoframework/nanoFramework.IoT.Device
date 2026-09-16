// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace nanoFramework.IoT.Device.CoreDiscoveryEngine
{
    /// <summary>Protocol-neutral kind of a discovered capability.</summary>
    public enum CapabilityKind
    {
        Telemetry,
        Property,
        Command,
    }

    /// <summary>A method parameter exposed by a capability.</summary>
    public sealed class CapabilityParameter
    {
        public CapabilityParameter(string name, Type type, bool isByReference)
        {
            Name = name;
            Type = type;
            IsByReference = isByReference;
        }

        public string Name { get; }

        public Type Type { get; }

        public bool IsByReference { get; }
    }

    /// <summary>A discovered resource or operation on a device interface.</summary>
    public sealed class Capability
    {
        internal Capability(CapabilityKind kind, string name, string displayName, Type valueType, string path, bool canRead, bool canWrite, CapabilityParameter[] parameters)
        {
            Kind = kind;
            Name = name;
            DisplayName = displayName;
            ValueType = valueType;
            Path = path;
            CanRead = canRead;
            CanWrite = canWrite;
            Parameters = parameters;
        }

        public CapabilityKind Kind { get; }
        public string Name { get; }
        public string DisplayName { get; }
        public Type ValueType { get; }
        public string Path { get; }
        public bool CanRead { get; }
        public bool CanWrite { get; }
        public CapabilityParameter[] Parameters { get; }
    }

    /// <summary>A device interface and its discovered capabilities.</summary>
    public sealed class DeviceInterface
    {
        internal DeviceInterface(string name, Type type, string path, Capability[] capabilities, DeviceInterface[] components)
        {
            Name = name;
            Type = type;
            Path = path;
            Capabilities = capabilities;
            Components = components;
        }

        public string Name { get; }
        public Type Type { get; }
        public string Path { get; }
        public Capability[] Capabilities { get; }
        public DeviceInterface[] Components { get; }
    }
}