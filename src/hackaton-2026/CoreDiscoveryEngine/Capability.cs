// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace nanoFramework.IoT.Device.CoreDiscoveryEngine
{
    /// <summary>A discovered resource or operation on a device interface.</summary>
    public class Capability
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

        /// <summary>Gets the capability kind.</summary>
        public CapabilityKind Kind { get; }

        /// <summary>Gets the capability name.</summary>
        public string Name { get; }

        /// <summary>Gets the display name.</summary>
        public string DisplayName { get; }

        /// <summary>Gets the capability value type, or <see langword="null" /> for commands without a return value.</summary>
        public Type ValueType { get; }

        /// <summary>Gets the hierarchical path of the capability.</summary>
        public string Path { get; }

        /// <summary>Gets a value indicating whether the capability can be read.</summary>
        public bool CanRead { get; }

        /// <summary>Gets a value indicating whether the capability can be written.</summary>
        public bool CanWrite { get; }

        /// <summary>Gets the parameters exposed by the capability.</summary>
        public CapabilityParameter[] Parameters { get; }
    }
}