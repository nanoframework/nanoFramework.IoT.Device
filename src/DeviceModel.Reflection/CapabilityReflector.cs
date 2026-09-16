// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Model;
using System.Runtime.InteropServices;

namespace Iot.Device.DeviceModel.Reflection
{
    /// <summary>
    /// Reflects oveer device binding types to discover their System.Device.Model metadata.
    /// </summary>
    public static class CapabilityReflector
    {
        /// <summary>
        /// Reads the display name declared by the <see cref="cref="InterfaceAttribute"/> on the given device type 
        /// </summary>
        /// <param name="deviceType">The device type to inspect, e.g. <c>typeof(AcmeDevice)</c>.</param>
        /// <returns> The interface display name, or <see langword="null"/> if the type has no <see cref="InterfaceAttribute"/>.</returns>
        public static string GetInterfaceDisplayName(Type deviceType)
        {
            var attributes = deviceType.GetCustomAttributes(true);

            foreach (var attribute in attributes)
            {
                if (attribute.GetType() == typeof(InterfaceAttribute))
                {
                    return ((InterfaceAttribute)attribute).DisplayName;
                }
            }

            return null;
        }
    }
}