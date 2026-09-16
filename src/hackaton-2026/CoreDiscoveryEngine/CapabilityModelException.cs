// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace nanoFramework.IoT.Device.CoreDiscoveryEngine
{
    /// <summary>Indicates that metadata cannot be represented by the capability model.</summary>
    public sealed class CapabilityModelException : Exception
    {
        public CapabilityModelException(string message)
            : base(message)
        {
        }
    }
}