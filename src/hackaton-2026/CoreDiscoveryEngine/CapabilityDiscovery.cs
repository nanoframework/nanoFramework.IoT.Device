// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Device.Model;
using System.Reflection;

namespace nanoFramework.IoT.Device.CoreDiscoveryEngine
{
    /// <summary>Discovers capabilities from System.Device.Model metadata.</summary>
    public static class CapabilityDiscovery
    {
        /// <summary>Discovers an interface from a device instance.</summary>
        /// <param name="device">The device instance to inspect.</param>
        /// <returns>The discovered device interface.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="device" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">Thrown when the device metadata is invalid.</exception>
        public static DeviceInterface Discover(object device)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            return Discover(device.GetType());
        }

        /// <summary>Discovers an interface from a device type.</summary>
        /// <param name="deviceType">The device type to inspect.</param>
        /// <returns>The discovered device interface.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="deviceType" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">Thrown when the device metadata is invalid.</exception>
        public static DeviceInterface Discover(Type deviceType)
        {
            if (deviceType == null)
            {
                throw new ArgumentNullException(nameof(deviceType));
            }

            return DiscoverInterface(deviceType, string.Empty, new ArrayList());
        }

        private static DeviceInterface DiscoverInterface(Type type, string path, ArrayList stack)
        {
            InterfaceAttribute interfaceAttribute = GetTypeAttribute<InterfaceAttribute>(type);
            if (interfaceAttribute == null)
            {
                throw Invalid(type, "the type must have an InterfaceAttribute");
            }

            string interfaceName = interfaceAttribute.DisplayName;
            if (string.IsNullOrEmpty(interfaceName))
            {
                interfaceName = type.Name;
            }

            if (stack.Contains(type))
            {
                throw Invalid(type, "component cycle detected");
            }

            stack.Add(type);
            ArrayList capabilities = new ArrayList();
            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            for (int index = 0; index < methods.Length; index++)
            {
                DiscoverMethod(methods[index], path, capabilities);
            }

            stack.Remove(type);
            Capability[] discoveredCapabilities = ToCapabilityArray(capabilities);
            SortCapabilities(discoveredCapabilities);
            return new DeviceInterface(interfaceName, type, path, discoveredCapabilities, new DeviceInterface[0]);
        }

        private static void DiscoverMethod(MethodInfo method, string path, ArrayList capabilities)
        {
            TelemetryAttribute telemetry = GetAttribute<TelemetryAttribute>(method);
            PropertyAttribute property = GetAttribute<PropertyAttribute>(method);
            ComponentAttribute component = GetAttribute<ComponentAttribute>(method);
            CommandAttribute command = GetAttribute<CommandAttribute>(method);
            int attributeCount = (telemetry == null ? 0 : 1) + (property == null ? 0 : 1) + (component == null ? 0 : 1) + (command == null ? 0 : 1);
            if (attributeCount == 0) return;
            if (attributeCount > 1) throw Invalid(method, "a method can have only one capability attribute");

            ParameterInfo[] parameters = method.GetParameters();
            string memberName = PropertyAccessorName(method.Name);
            if (component != null)
            {
                if (parameters.Length != 0 || method.ReturnType == typeof(void))
                {
                    throw Invalid(method, "a component getter must have no parameters and return a value");
                }

                // The component is represented as a method by the nanoFramework reflection API.
                return;
            }
            if (telemetry != null)
            {
                Type valueType = TelemetryValueType(method, parameters);
                string name = NameOrDefault(telemetry.Name, memberName);
                AddCapability(capabilities, new Capability(CapabilityKind.Telemetry, name, DisplayName(telemetry.DisplayName, memberName), valueType, JoinPath(path, name), true, false, new CapabilityParameter[0]), method);
            }
            else if (property != null)
            {
                if (method.ReturnType == typeof(void) && parameters.Length != 1)
                {
                    throw Invalid(method, "a property setter must take exactly one parameter");
                }

                if (method.ReturnType != typeof(void) && parameters.Length != 0)
                {
                    throw Invalid(method, "a property getter method cannot take parameters");
                }

                Type valueType = method.ReturnType == typeof(void) ? parameters[0].ParameterType : method.ReturnType;
                string name = NameOrDefault(property.Name, memberName);
                AddCapability(capabilities, new Capability(CapabilityKind.Property, name, DisplayName(property.DisplayName, memberName), valueType, JoinPath(path, name), method.ReturnType != typeof(void), method.ReturnType == typeof(void), new CapabilityParameter[0]), method);
            }
            else
            {
                string name = NameOrDefault(command.Name, memberName);
                AddCapability(capabilities, new Capability(CapabilityKind.Command, name, DisplayName(command.DisplayName, memberName), method.ReturnType == typeof(void) ? null : method.ReturnType, JoinPath(path, name), false, false, Parameters(parameters)), method);
            }
        }

        private static Type TelemetryValueType(MethodInfo method, ParameterInfo[] parameters)
        {
            if (parameters.Length == 0 && method.ReturnType != typeof(void))
            {
                return method.ReturnType;
            }

            if (method.ReturnType == typeof(bool) && parameters.Length == 1 && parameters[0].ParameterType.GetElementType() != null)
            {
                return parameters[0].ParameterType.GetElementType();
            }

            throw Invalid(method, "telemetry must be a value-returning method or a bool method with one out parameter");
        }

        private static CapabilityParameter[] Parameters(ParameterInfo[] parameters)
        {
            CapabilityParameter[] result = new CapabilityParameter[parameters.Length];
            for (int index = 0; index < parameters.Length; index++)
            {
                result[index] = new CapabilityParameter("arg" + index, parameters[index].ParameterType, parameters[index].ParameterType.GetElementType() != null);
            }

            return result;
        }

        private static void AddCapability(ArrayList capabilities, Capability capability, MethodInfo member)
        {
            for (int index = 0; index < capabilities.Count; index++)
            {
                Capability existing = (Capability)capabilities[index];
                if (existing.Name != capability.Name)
                {
                    continue;
                }

                if (existing.Kind == CapabilityKind.Property && capability.Kind == CapabilityKind.Property && existing.Path == capability.Path && (existing.CanRead != capability.CanRead || existing.CanWrite != capability.CanWrite))
                {
                    capabilities[index] = MergeProperty(existing, capability);
                    return;
                }

                throw Invalid(member, "duplicate capability name '" + capability.Name + "'");
            }

            capabilities.Add(capability);
        }

        private static Capability MergeProperty(Capability first, Capability second)
        {
            Type valueType = first.ValueType ?? second.ValueType;
            bool canRead = first.CanRead || second.CanRead;
            bool canWrite = first.CanWrite || second.CanWrite;
            return new Capability(CapabilityKind.Property, first.Name, first.DisplayName, valueType, first.Path, canRead, canWrite, new CapabilityParameter[0]);
        }

        private static string NameOrDefault(string name, string fallback)
        {
            return string.IsNullOrEmpty(name) ? fallback : name;
        }

        private static string PropertyAccessorName(string name)
        {
            if (name.StartsWith("get_"))
            {
                return name.Substring(4);
            }

            if (name.StartsWith("set_"))
            {
                return name.Substring(4);
            }

            return name;
        }

        private static string DisplayName(string displayName, string fallback)
        {
            return string.IsNullOrEmpty(displayName) ? fallback : displayName;
        }

        private static string JoinPath(string parent, string name)
        {
            return string.IsNullOrEmpty(parent) ? name : parent + "." + name;
        }
        private static T GetAttribute<T>(MethodInfo method) where T : Attribute
        {
            object[] attributes = method.GetCustomAttributes(true);
            for (int index = 0; index < attributes.Length; index++)
            {
                T attribute = attributes[index] as T;
                if (attribute != null)
                {
                    return attribute;
                }
            }

            return null;
        }

        private static T GetTypeAttribute<T>(Type type) where T : Attribute
        {
            object[] attributes = type.GetCustomAttributes(true);
            for (int index = 0; index < attributes.Length; index++)
            {
                T attribute = attributes[index] as T;
                if (attribute != null)
                {
                    return attribute;
                }
            }

            return null;
        }

        private static ArgumentException Invalid(MethodInfo member, string message)
        {
            return new ArgumentException(member.DeclaringType.FullName + "." + member.Name + ": " + message);
        }

        private static ArgumentException Invalid(Type type, string message)
        {
            return new ArgumentException(type.FullName + ": " + message);
        }

        private static Capability[] ToCapabilityArray(ArrayList values)
        {
            Capability[] result = new Capability[values.Count];
            values.CopyTo(result);
            return result;
        }

        private static void SortCapabilities(Capability[] values)
        {
            for (int index = 1; index < values.Length; index++)
            {
                Capability value = values[index];
                int position = index - 1;
                while (position >= 0 && string.Compare(values[position].Path, value.Path) > 0)
                {
                    values[position + 1] = values[position];
                    position--;
                }

                values[position + 1] = value;
            }
        }

    }
}