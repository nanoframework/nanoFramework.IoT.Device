// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.DeviceModel.Reflection
{
    /// <summary>
    /// Identifies which System.Device.Model attribute a <see cref="Capability"/> was discovered from.
    /// </summary>
    public enum CapabilityKind
    {
        /// <summary>The capability was declared with a <see cref="System.Device.Model.TelemetryAttribute"/>.</summary>
        Telemetry,

        /// <summary>The capability was declared with a <see cref="System.Device.Model.PropertyAttribute"/>.</summary>
        Property,

        /// <summary>The capability was declared with a <see cref="System.Device.Model.CommandAttribute"/>.</summary>
        Command,
    }
}
