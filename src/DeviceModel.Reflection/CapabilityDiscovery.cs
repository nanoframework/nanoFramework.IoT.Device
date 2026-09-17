// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Reflection;

namespace Iot.Device.DeviceModel.Reflection
{
    /// <summary>
    /// Walks a device type's public members and builds the <see cref="DeviceInterface"/> capabilities
    /// described by its System.Device.Model attributes.
    /// </summary>
    public static class CapabilityDiscovery
    {
        private static readonly CapabilityParameter[] NoParameters = new CapabilityParameter[0];

        /// <summary>
        /// Discovers the capabilities of the given device instance's type.
        /// </summary>
        /// <param name="device">The device instance to inspect.</param>
        /// <returns>The discovered <see cref="DeviceInterface"/>.</returns>
        public static DeviceInterface Discover(object device)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            return Discover(device.GetType());
        }

        /// <summary>
        /// Discovers the capabilities of the given device type.
        /// </summary>
        /// <param name="deviceType">The device type to inspect, e.g. <c>typeof(AcmeDevice)</c>.</param>
        /// <returns>The discovered <see cref="DeviceInterface"/>.</returns>
        public static DeviceInterface Discover(Type deviceType)
        {
            if (deviceType == null)
            {
                throw new ArgumentNullException(nameof(deviceType));
            }

            // No InterfaceAttribute is not fatal: fall back to the type name so discovery keeps going.
            string name = CapabilityReflector.GetInterfaceDisplayName(deviceType) ?? deviceType.Name;

            ArrayList capabilities = new ArrayList();
            MethodInfo[] methods = deviceType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            for (int index = 0; index < methods.Length; index++)
            {
                DiscoverMember(methods[index], capabilities);
            }

            Capability[] capabilityArray = ToCapabilityArray(capabilities);
            SortByName(capabilityArray);

            return new DeviceInterface(name, deviceType, capabilityArray);
        }

        private static void DiscoverMember(MethodInfo method, ArrayList capabilities)
        {
            string memberName = StrippedMemberName(method.Name);

            string telemetryName = CapabilityReflector.GetTelemetryName(method, memberName);
            if (telemetryName != null)
            {
                AddTelemetry(capabilities, method, telemetryName);
                return;
            }

            string propertyName = CapabilityReflector.GetPropertyName(method, memberName);
            if (propertyName != null)
            {
                AddProperty(capabilities, method, propertyName);
                return;
            }

            string commandName = CapabilityReflector.GetCommandName(method);
            if (commandName != null)
            {
                AddCommand(capabilities, method, commandName);
            }
        }

        private static void AddTelemetry(ArrayList capabilities, MethodInfo method, string name)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != 0 || method.ReturnType == typeof(void))
            {
                // Skip rather than throw: an unsupported telemetry shape (e.g. a try-pattern "out" method)
                // should not stop discovery of everything else on the device.
                return;
            }

            Capability capability = new Capability(CapabilityKind.Telemetry, name, method.ReturnType, true, false, NoParameters);
            AddCapability(capabilities, capability);
        }

        private static void AddProperty(ArrayList capabilities, MethodInfo method, string name)
        {
            ParameterInfo[] parameters = method.GetParameters();
            bool isGetter = method.ReturnType != typeof(void) && parameters.Length == 0;
            bool isSetter = method.ReturnType == typeof(void) && parameters.Length == 1;

            if (!isGetter && !isSetter)
            {
                throw new InvalidOperationException(method.DeclaringType.FullName + "." + method.Name + ": a property accessor must either return a value with no parameters, or return void with exactly one parameter");
            }

            Type valueType = isGetter ? method.ReturnType : parameters[0].ParameterType;
            Capability capability = new Capability(CapabilityKind.Property, name, valueType, isGetter, isSetter, NoParameters);
            AddCapability(capabilities, capability);
        }

        private static void AddCommand(ArrayList capabilities, MethodInfo method, string name)
        {
            ParameterInfo[] parameters = method.GetParameters();

            // Parameter names are not reflected here: positional "argN" naming is used instead, matching the
            // convention MCP tools already need for methods with more than one parameter.
            CapabilityParameter[] commandParameters = new CapabilityParameter[parameters.Length];
            for (int index = 0; index < parameters.Length; index++)
            {
                commandParameters[index] = new CapabilityParameter("arg" + index, parameters[index].ParameterType);
            }

            Type returnType = method.ReturnType == typeof(void) ? null : method.ReturnType;
            Capability capability = new Capability(CapabilityKind.Command, name, returnType, false, false, commandParameters);
            AddCapability(capabilities, capability);
        }

        private static void AddCapability(ArrayList capabilities, Capability capability)
        {
            for (int index = 0; index < capabilities.Count; index++)
            {
                Capability existing = (Capability)capabilities[index];
                if (existing.Name != capability.Name)
                {
                    continue;
                }

                if (existing.Kind == CapabilityKind.Property && capability.Kind == CapabilityKind.Property)
                {
                    capabilities[index] = MergeProperty(existing, capability);
                    return;
                }

                throw new InvalidOperationException("Duplicate capability name '" + capability.Name + "'");
            }

            capabilities.Add(capability);
        }

        private static Capability MergeProperty(Capability first, Capability second)
        {
            Type valueType = first.ValueType ?? second.ValueType;
            return new Capability(CapabilityKind.Property, first.Name, valueType, first.CanRead || second.CanRead, first.CanWrite || second.CanWrite, NoParameters);
        }

        private static string StrippedMemberName(string methodName)
        {
            if (methodName.StartsWith("get_") || methodName.StartsWith("set_"))
            {
                return methodName.Substring(4);
            }

            return methodName;
        }

        private static Capability[] ToCapabilityArray(ArrayList values)
        {
            Capability[] result = new Capability[values.Count];
            values.CopyTo(result);
            return result;
        }

        private static void SortByName(Capability[] values)
        {
            for (int index = 1; index < values.Length; index++)
            {
                Capability value = values[index];
                int position = index - 1;
                while (position >= 0 && string.Compare(values[position].Name, value.Name) > 0)
                {
                    values[position + 1] = values[position];
                    position--;
                }

                values[position + 1] = value;
            }
        }
    }
}
