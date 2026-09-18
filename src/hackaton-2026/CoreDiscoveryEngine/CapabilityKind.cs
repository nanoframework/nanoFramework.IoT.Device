// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace nanoFramework.IoT.Device.CoreDiscoveryEngine
{
    /// <summary>Protocol-neutral kind of a discovered capability.</summary>
    public enum CapabilityKind
    {
        /// <summary>A value reported by a device.</summary>
        Telemetry,

        /// <summary>A readable or writable device value.</summary>
        Property,

        /// <summary>An operation that can be invoked on a device.</summary>
        Command,
    }
}