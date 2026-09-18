// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.DeviceModel.Reflection
{
    /// <summary>
    /// A single telemetry, property, or command discovered on a <see cref="DeviceInterface"/>.
    /// </summary>
    public class Capability
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Capability" /> class.
        /// </summary>
        /// <param name="kind">Which attribute the capability was discovered from.</param>
        /// <param name="name">The capability name.</param>
        /// <param name="valueType">The type of value the capability reads or writes, or <see langword="null"/> for a command with no return value.</param>
        /// <param name="canRead">Whether the capability can be read (telemetry and readable properties).</param>
        /// <param name="canWrite">Whether the capability can be written (writable properties).</param>
        /// <param name="parameters">The command's parameters, or an empty array for telemetry and properties.</param>
        public Capability(CapabilityKind kind, string name, Type valueType, bool canRead, bool canWrite, CapabilityParameter[] parameters)
        {
            Kind = kind;
            Name = name;
            ValueType = valueType;
            CanRead = canRead;
            CanWrite = canWrite;
            Parameters = parameters;
        }

        /// <summary>Gets which attribute the capability was discovered from.</summary>
        public CapabilityKind Kind { get; }

        /// <summary>Gets the capability name.</summary>
        public string Name { get; }

        /// <summary>Gets the type of value the capability reads or writes, or <see langword="null"/> for a command with no return value.</summary>
        public Type ValueType { get; }

        /// <summary>Gets a value indicating whether the capability can be read.</summary>
        public bool CanRead { get; }

        /// <summary>Gets a value indicating whether the capability can be written.</summary>
        public bool CanWrite { get; }

        /// <summary>Gets the command's parameters, or an empty array for telemetry and properties.</summary>
        public CapabilityParameter[] Parameters { get; }
    }
}
