// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Device.Model;
using System.Reflection;

namespace Iot.Device.CoreDiscoveryEngine
{
    /// <summary>
    /// Discovers device capabilities from <see cref="System.Device.Model"/> metadata.
    /// </summary>
    public static class CapabilityDiscovery
    {
        private static readonly CapabilityParameter[] NoParameters = new CapabilityParameter[0];

        /// <summary>
        /// Discovers the capabilities exposed by a device instance.
        /// </summary>
        /// <param name="device">The device instance to inspect.</param>
        /// <returns>The discovered device interface.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="device"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The device metadata contains a component cycle, invalid capability signature, conflicting attributes, duplicate capability, or mismatched property accessors.</exception>
        public static DeviceInterface Discover(object device)
        {
            if (device == null)
            {
                throw new ArgumentNullException();
            }

            return Discover(device.GetType());
        }

        /// <summary>
        /// Discovers the capabilities exposed by a device type.
        /// </summary>
        /// <param name="deviceType">The device type to inspect.</param>
        /// <returns>The discovered device interface.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="deviceType"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The device metadata contains a component cycle, invalid capability signature, conflicting attributes, duplicate capability, or mismatched property accessors.</exception>
        public static DeviceInterface Discover(Type deviceType)
        {
            if (deviceType == null)
            {
                throw new ArgumentNullException();
            }

            return DiscoverInterface(deviceType, string.Empty, new ArrayList());
        }

        private static DeviceInterface DiscoverInterface(Type deviceType, string path, ArrayList stack)
        {
            if (stack.Contains(deviceType))
            {
                throw new ArgumentException("cycle");
            }

            InterfaceAttribute interfaceAttribute = (InterfaceAttribute)GetAttribute(deviceType, typeof(InterfaceAttribute));
            string interfaceName = interfaceAttribute == null || string.IsNullOrEmpty(interfaceAttribute.DisplayName) ? deviceType.Name : interfaceAttribute.DisplayName;

            stack.Add(deviceType);
            ArrayList capabilities = new ArrayList();
            ArrayList components = new ArrayList();
            MethodInfo[] methods = deviceType.GetMethods();
            for (int index = 0; index < methods.Length; index++)
            {
                DiscoverMethod(methods[index], path, capabilities, components, stack);
            }

            stack.Remove(deviceType);
            Capability[] capabilityArray = ToCapabilityArray(capabilities);
            DeviceInterface[] componentArray = ToDeviceInterfaceArray(components);

            return new DeviceInterface(interfaceName, deviceType, path, capabilityArray, componentArray);
        }

        private static void DiscoverMethod(MethodInfo method, string path, ArrayList capabilities, ArrayList components, ArrayList stack)
        {
            TelemetryAttribute telemetry = (TelemetryAttribute)GetAttribute(method, typeof(TelemetryAttribute));
            PropertyAttribute property = (PropertyAttribute)GetAttribute(method, typeof(PropertyAttribute));
            CommandAttribute command = (CommandAttribute)GetAttribute(method, typeof(CommandAttribute));
            ComponentAttribute component = (ComponentAttribute)GetAttribute(method, typeof(ComponentAttribute));

            int attributeCount = (telemetry == null ? 0 : 1) + (property == null ? 0 : 1) + (command == null ? 0 : 1) + (component == null ? 0 : 1);
            if (attributeCount == 0)
            {
                return;
            }

            if (attributeCount > 1)
            {
                throw new ArgumentException("attributes");
            }

            string memberName = GetMemberName(method.Name);
            if (telemetry != null)
            {
                AddTelemetry(capabilities, method, telemetry, memberName, path);
            }
            else if (property != null)
            {
                AddProperty(capabilities, method, property, memberName, path);
            }
            else if (command != null)
            {
                AddCommand(capabilities, method, command, memberName, path);
            }
            else
            {
                AddComponent(components, method, component, memberName, path, stack);
            }
        }

        private static void AddTelemetry(ArrayList capabilities, MethodInfo method, TelemetryAttribute attribute, string memberName, string path)
        {
            ParameterInfo[] parameters = method.GetParameters();
            Type valueType;
            if (parameters.Length == 0 && method.ReturnType != typeof(void))
            {
                valueType = method.ReturnType;
            }
            else
            {
                throw new ArgumentException("telemetry");
            }

            string name = NameOrDefault(attribute.Name, memberName);
            Capability capability = new Capability(
                CapabilityKind.Telemetry,
                name,
                memberName,
                valueType,
                JoinPath(path, name),
                true,
                false,
                NoParameters,
                method.Name,
                null);
            AddCapability(capabilities, capability, method);
        }

        private static void AddProperty(ArrayList capabilities, MethodInfo method, PropertyAttribute attribute, string memberName, string path)
        {
            ParameterInfo[] parameters = method.GetParameters();
            bool isGetter = method.Name.StartsWith("get_") && method.ReturnType != typeof(void) && parameters.Length == 0;
            bool isSetter = method.Name.StartsWith("set_") && method.ReturnType == typeof(void) && parameters.Length == 1;
            bool isGetterMethod = !method.Name.StartsWith("set_") && method.ReturnType != typeof(void) && parameters.Length == 0;
            bool isSetterMethod = !method.Name.StartsWith("get_") && method.ReturnType == typeof(void) && parameters.Length == 1;

            if (!isGetter && !isSetter && !isGetterMethod && !isSetterMethod)
            {
                throw new ArgumentException("property");
            }

            bool canRead = isGetter || isGetterMethod;
            bool canWrite = isSetter || isSetterMethod;
            Type valueType = canRead ? method.ReturnType : parameters[0].ParameterType;
            string name = NameOrDefault(attribute.Name, memberName);
            Capability capability = new Capability(
                CapabilityKind.Property,
                name,
                memberName,
                valueType,
                JoinPath(path, name),
                canRead,
                canWrite,
                NoParameters,
                canRead ? method.Name : null,
                canWrite ? method.Name : null);
            AddCapability(capabilities, capability, method);
        }

        private static void AddCommand(ArrayList capabilities, MethodInfo method, CommandAttribute attribute, string memberName, string path)
        {
            ParameterInfo[] parameters = method.GetParameters();
            CapabilityParameter[] capabilityParameters = new CapabilityParameter[parameters.Length];
            for (int index = 0; index < parameters.Length; index++)
            {
                capabilityParameters[index] = new CapabilityParameter("arg" + index, parameters[index].ParameterType, false);
            }

            string name = NameOrDefault(attribute.Name, memberName);
            Capability capability = new Capability(
                CapabilityKind.Command,
                name,
                memberName,
                method.ReturnType == typeof(void) ? null : method.ReturnType,
                JoinPath(path, name),
                false,
                false,
                capabilityParameters,
                method.Name,
                null);
            AddCapability(capabilities, capability, method);
        }

        private static void AddComponent(ArrayList components, MethodInfo method, ComponentAttribute attribute, string memberName, string path, ArrayList stack)
        {
            if (!method.Name.StartsWith("get_") || method.ReturnType == typeof(void) || method.GetParameters().Length != 0)
            {
                throw new ArgumentException("component");
            }

            string name = NameOrDefault(attribute.Name, memberName);
            components.Add(DiscoverInterface(method.ReturnType, JoinPath(path, name), stack));
        }

        private static void AddCapability(ArrayList capabilities, Capability capability, MethodInfo method)
        {
            for (int index = 0; index < capabilities.Count; index++)
            {
                Capability existing = (Capability)capabilities[index];
                if (existing.Name != capability.Name)
                {
                    continue;
                }

                if (existing.Kind == CapabilityKind.Property && capability.Kind == CapabilityKind.Property && existing.Path == capability.Path)
                {
                    if (existing.ValueType != capability.ValueType)
                    {
                        throw new ArgumentException("type");
                    }

                    if ((existing.CanRead && capability.CanRead) || (existing.CanWrite && capability.CanWrite))
                    {
                        throw new ArgumentException("accessor");
                    }

                    capabilities[index] = MergeProperty(existing, capability);
                    return;
                }

                throw new ArgumentException("capability");
            }

            capabilities.Add(capability);
        }

        private static Capability MergeProperty(Capability first, Capability second)
        {
            return new Capability(
                CapabilityKind.Property,
                first.Name,
                first.DisplayName,
                first.ValueType,
                first.Path,
                first.CanRead || second.CanRead,
                first.CanWrite || second.CanWrite,
                NoParameters,
                first.ReadMethodName ?? second.ReadMethodName,
                first.WriteMethodName ?? second.WriteMethodName);
        }

        private static object GetAttribute(Type type, Type attributeType)
        {
            object[] attributes = type.GetCustomAttributes(true);
            return FindAttribute(attributes, attributeType);
        }

        private static object GetAttribute(MethodInfo method, Type attributeType)
        {
            object[] attributes = method.GetCustomAttributes(true);
            return FindAttribute(attributes, attributeType);
        }

        private static object FindAttribute(object[] attributes, Type attributeType)
        {
            for (int index = 0; index < attributes.Length; index++)
            {
                if (attributes[index].GetType() == attributeType)
                {
                    return attributes[index];
                }
            }

            return null;
        }

        private static string GetMemberName(string methodName)
        {
            if (methodName.StartsWith("get_") || methodName.StartsWith("set_"))
            {
                return methodName.Substring(4);
            }

            return methodName;
        }

        private static string NameOrDefault(string name, string fallback)
        {
            return string.IsNullOrEmpty(name) ? fallback : name;
        }

        private static string JoinPath(string parent, string name)
        {
            return string.IsNullOrEmpty(parent) ? name : parent + "." + name;
        }

        private static Capability[] ToCapabilityArray(ArrayList values)
        {
            Capability[] result = new Capability[values.Count];
            values.CopyTo(result);
            return result;
        }

        private static DeviceInterface[] ToDeviceInterfaceArray(ArrayList values)
        {
            DeviceInterface[] result = new DeviceInterface[values.Count];
            values.CopyTo(result);
            return result;
        }
    }
}