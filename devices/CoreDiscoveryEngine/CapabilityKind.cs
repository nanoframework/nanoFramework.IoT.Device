// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.CoreDiscoveryEngine
{
    /// <summary>
    /// Identifies the kind of a discovered device capability.
    /// </summary>
    public enum CapabilityKind
    {
        /// <summary>A read-only telemetry value.</summary>
        Telemetry,

        /// <summary>A readable or writable device property.</summary>
        Property,

        /// <summary>An operation exposed by the device.</summary>
        Command,
    }
}