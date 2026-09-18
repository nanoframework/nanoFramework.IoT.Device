// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace nanoFramework.IoT.Device.CoreDiscoveryEngine
{
    /// <summary>A method parameter exposed by a capability.</summary>
    public class CapabilityParameter
    {
        /// <summary>Initializes a new instance of the <see cref="CapabilityParameter" /> class.</summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="type">The parameter type.</param>
        /// <param name="isByReference">Indicates whether the parameter is passed by reference.</param>
        public CapabilityParameter(string name, Type type, bool isByReference)
        {
            Name = name;
            Type = type;
            IsByReference = isByReference;
        }

        /// <summary>Gets the parameter name.</summary>
        public string Name { get; }

        /// <summary>Gets the parameter type.</summary>
        public Type Type { get; }

        /// <summary>Gets a value indicating whether the parameter is passed by reference.</summary>
        public bool IsByReference { get; }
    }
}