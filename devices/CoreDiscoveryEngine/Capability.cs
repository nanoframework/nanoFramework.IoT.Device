// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.CoreDiscoveryEngine
{
    /// <summary>
    /// Describes a discovered device capability.
    /// </summary>
    public class Capability
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Capability"/> class.
        /// </summary>
        /// <param name="kind">The capability kind.</param>
        /// <param name="name">The protocol name.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="valueType">The value or return type.</param>
        /// <param name="path">The path relative to the root device.</param>
        /// <param name="canRead">Whether the capability can be read.</param>
        /// <param name="canWrite">Whether the capability can be written.</param>
        /// <param name="parameters">The command parameters.</param>
        /// <param name="readMethodName">The method used to read or invoke the capability.</param>
        /// <param name="writeMethodName">The method used to write the capability.</param>
        public Capability(
            CapabilityKind kind,
            string name,
            string displayName,
            Type valueType,
            string path,
            bool canRead,
            bool canWrite,
            CapabilityParameter[] parameters,
            string readMethodName,
            string writeMethodName)
        {
            Kind = kind;
            Name = name;
            DisplayName = displayName;
            ValueType = valueType;
            Path = path;
            CanRead = canRead;
            CanWrite = canWrite;
            Parameters = parameters;
            ReadMethodName = readMethodName;
            WriteMethodName = writeMethodName;
        }

        /// <summary>Gets the capability kind.</summary>
        public CapabilityKind Kind { get; }

        /// <summary>Gets the protocol name.</summary>
        public string Name { get; }

        /// <summary>Gets the display name.</summary>
        public string DisplayName { get; }

        /// <summary>Gets the value or return type.</summary>
        public Type ValueType { get; }

        /// <summary>Gets the path relative to the root device.</summary>
        public string Path { get; }

        /// <summary>Gets a value indicating whether the capability can be read.</summary>
        public bool CanRead { get; }

        /// <summary>Gets a value indicating whether the capability can be written.</summary>
        public bool CanWrite { get; }

        /// <summary>Gets the command parameters.</summary>
        public CapabilityParameter[] Parameters { get; }

        /// <summary>Gets the name of the method used to read or invoke the capability.</summary>
        public string ReadMethodName { get; }

        /// <summary>Gets the name of the method used to write the capability.</summary>
        public string WriteMethodName { get; }

        /// <summary>Gets a value indicating whether the capability maps to an MCP resource.</summary>
        public bool IsMcpResource => CanRead;

        /// <summary>Gets a value indicating whether the capability maps to an MCP tool.</summary>
        public bool IsMcpTool => Kind == CapabilityKind.Command || CanWrite;
    }
}