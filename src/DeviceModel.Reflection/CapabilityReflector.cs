// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Model;
using System.Reflection;

namespace Iot.Device.DeviceModel.Reflection
{
    /// <summary>
    /// Reflects oveer device binding types to discover their System.Device.Model metadata.
    /// </summary>
    public static class CapabilityReflector
    {
        /// <summary>
        /// Reads the display name declared by the <see cref="InterfaceAttribute"/> on the given device type
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

        /// <summary>
        /// Reads the telemetry name declared by the <see cref="TelemetryAttribute"/> on the given method,
        /// falling back to the given name when no explicit name was provided.
        /// </summary>
        /// <remarks>
        /// nanoFramework does not support <c>PropertyInfo</c>/<c>Type.GetProperties()</c>. When telemetry is declared
        /// on a property, pass its compiler-generated "get_" accessor method here instead, following the same
        /// pattern used by nanoFramework.WebServer.Mcp's JsonHelper.
        /// </remarks>
        /// <param name="method">The method or property getter to inspect, expected to carry a <see cref="TelemetryAttribute"/>.</param>
        /// <param name="memberName">The name to fall back to when the attribute does not specify one.</param>
        /// <returns>The telemetry name, or <see langword="null"/> if the method has no <see cref="TelemetryAttribute"/>.</returns>
        public static string GetTelemetryName(MethodInfo method, string memberName)
        {
            var attributes = method.GetCustomAttributes(true);

            foreach (var attribute in attributes)
            {
                if (attribute.GetType() == typeof(TelemetryAttribute))
                {
                    var telemetryAttribute = (TelemetryAttribute)attribute;
                    return telemetryAttribute.Name ?? memberName;
                }
            }

            return null;
        }

        /// <summary>
        /// Reads the property name declared by the <see cref="PropertyAttribute"/> on the given property accessor,
        /// falling back to the given name when no explicit name was provided.
        /// </summary>
        /// <remarks>Same "get_"/"set_" accessor workaround as <see cref="GetTelemetryName"/>.</remarks>
        /// <param name="method">The property accessor method to inspect, expected to carry a <see cref="PropertyAttribute"/>.</param>
        /// <param name="memberName">The name to fall back to when the attribute does not specify one.</param>
        /// <returns>The property name, or <see langword="null"/> if the method has no <see cref="PropertyAttribute"/>.</returns>
        public static string GetPropertyName(MethodInfo method, string memberName)
        {
            var attributes = method.GetCustomAttributes(true);

            foreach (var attribute in attributes)
            {
                if (attribute.GetType() == typeof(PropertyAttribute))
                {
                    var propertyAttribute = (PropertyAttribute)attribute;
                    return propertyAttribute.Name ?? memberName;
                }
            }

            return null;
        }

        /// <summary>
        /// Reads the command name declared by the <see cref="CommandAttribute"/> on the given method,
        /// falling back to the method's own name when no explicit name was provided.
        /// </summary>
        /// <remarks>
        /// Unlike Telemetry/Property/Component, <see cref="CommandAttribute"/> only targets ordinary methods
        /// (e.g. <c>Reset()</c>), so no "get_"/"set_" accessor workaround is needed here.
        /// </remarks>
        /// <param name="method">The method to inspect, expected to carry a <see cref="CommandAttribute"/>.</param>
        /// <returns>The command name, or <see langword="null"/> if the method has no <see cref="CommandAttribute"/>.</returns>
        public static string GetCommandName(MethodInfo method)
        {
            var attributes = method.GetCustomAttributes(true);

            foreach (var attribute in attributes)
            {
                if (attribute.GetType() == typeof(CommandAttribute))
                {
                    var commandAttribute = (CommandAttribute)attribute;
                    return commandAttribute.Name ?? method.Name;
                }
            }

            return null;
        }
    }
}