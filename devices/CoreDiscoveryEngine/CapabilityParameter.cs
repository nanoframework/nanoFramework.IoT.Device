// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.CoreDiscoveryEngine
{
    /// <summary>
    /// Describes a parameter of a discovered command.
    /// </summary>
    public class CapabilityParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CapabilityParameter"/> class.
        /// </summary>
        /// <param name="name">The positional parameter name.</param>
        /// <param name="parameterType">The parameter type.</param>
        /// <param name="isByReference">Whether the parameter is passed by reference.</param>
        public CapabilityParameter(string name, Type parameterType, bool isByReference)
        {
            Name = name;
            ParameterType = parameterType;
            IsByReference = isByReference;
        }

        /// <summary>Gets the positional parameter name.</summary>
        public string Name { get; }

        /// <summary>Gets the parameter type.</summary>
        public Type ParameterType { get; }

        /// <summary>Gets a value indicating whether this parameter is passed by reference.</summary>
        public bool IsByReference { get; }
    }
}