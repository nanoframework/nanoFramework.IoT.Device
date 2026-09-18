// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.DeviceModel.Reflection
{
    /// <summary>
    /// A single parameter accepted by a <see cref="CapabilityKind.Command"/> capability.
    /// </summary>
    public class CapabilityParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CapabilityParameter" /> class.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="type">The parameter's declared type.</param>
        public CapabilityParameter(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        /// <summary>
        /// Gets the parameter name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the parameter's declared type.
        /// </summary>
        public Type Type { get; }
    }
}
