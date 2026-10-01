// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Reflection;
using System.Text;
using nanoFramework.Json;
using nanoFramework.WebServer.Mcp;

namespace Iot.Device.CoreDiscoveryEngine
{
    /// <summary>
    /// Generates MCP resources and tools from discovered device capabilities.
    /// </summary>
    public class McpDeviceRegistry : IMcpResourceProvider, IMcpToolProvider
    {
        private readonly Hashtable _resources = new Hashtable();
        private readonly Hashtable _tools = new Hashtable();

        /// <summary>
        /// Discovers and registers the capabilities exposed by a live device.
        /// </summary>
        /// <param name="device">The device instance to register.</param>
        /// <param name="prefix">An optional prefix used to distinguish device instances.</param>
        /// <exception cref="ArgumentNullException"><paramref name="device"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Discovery fails, a reflected member cannot be resolved, or the generated MCP name or URI is already registered.</exception>
        public void Register(object device, string prefix = null)
        {
            if (device == null)
            {
                throw new ArgumentNullException();
            }

            DeviceInterface deviceInterface = CapabilityDiscovery.Discover(device);
            Register(deviceInterface, device, prefix);
        }

        /// <summary>
        /// Registers capabilities from an existing discovery result for a live device.
        /// </summary>
        /// <param name="deviceInterface">The discovered device interface.</param>
        /// <param name="device">The live device instance to register.</param>
        /// <param name="prefix">An optional prefix used to distinguish device instances.</param>
        /// <exception cref="ArgumentNullException"><paramref name="deviceInterface"/> or <paramref name="device"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">A reflected member cannot be resolved, or the generated MCP name or URI is already registered.</exception>
        public void Register(DeviceInterface deviceInterface, object device, string prefix = null)
        {
            if (deviceInterface == null)
            {
                throw new ArgumentNullException();
            }

            if (device == null)
            {
                throw new ArgumentNullException();
            }

            RegisterInterface(deviceInterface, device, NormalizePrefix(prefix));
        }

        /// <summary>
        /// Gets MCP resource-list metadata.
        /// </summary>
        /// <returns>The JSON member containing the resource list.</returns>
        public string GetResourceMetadataJson()
        {
            return GetMetadataJson("resources", _resources);
        }

        /// <summary>
        /// Gets MCP tool-list metadata.
        /// </summary>
        /// <returns>The JSON member containing the tool list.</returns>
        public string GetToolMetadataJson()
        {
            return GetMetadataJson("tools", _tools);
        }

        /// <summary>
        /// Reads a generated MCP resource.
        /// </summary>
        /// <param name="uri">The absolute MCP resource URI.</param>
        /// <returns>The MCP resource-read result.</returns>
        /// <exception cref="McpResourceRegistry.ResourceNotFoundException">No registered resource matches <paramref name="uri"/>.</exception>
        public string ReadResource(string uri)
        {
            McpInvocation invocation = (McpInvocation)_resources[NormalizeResourceUri(uri)];
            if (invocation == null)
            {
                throw new McpResourceRegistry.ResourceNotFoundException(uri);
            }

            object result = invocation.Method.Invoke(invocation.Target, null);
            McpResourceMetadata metadata = (McpResourceMetadata)invocation.Metadata;
            string resultText = result == null ? string.Empty : IsSimpleType(invocation.Capability.ValueType)
                ? result.ToString()
                : JsonConvert.SerializeObject(result);
            string text = JsonString(resultText);
            return "{\"contents\":[{\"uri\":" + JsonString(metadata.Uri)
                + ",\"mimeType\":" + JsonString(metadata.MimeType) + ",\"text\":" + text + "}]}";
        }

        /// <summary>
        /// Invokes a generated MCP tool.
        /// </summary>
        /// <param name="name">The generated tool name.</param>
        /// <param name="arguments">The MCP tool arguments.</param>
        /// <returns>The JSON value returned by the tool.</returns>
        /// <exception cref="ArgumentException">The tool is not registered, a required argument is missing, or an argument type is unsupported.</exception>
        /// <exception cref="InvalidCastException">An argument cannot be converted to the required type.</exception>
        public string InvokeTool(string name, Hashtable arguments)
        {
            McpInvocation invocation = (McpInvocation)_tools[name.ToLower()];
            if (invocation == null)
            {
                throw new ArgumentException("Tool not found", nameof(name));
            }

            Capability capability = invocation.Capability;
            object[] parameters;
            if (capability.Kind == CapabilityKind.Property)
            {
                parameters = new object[] { ConvertArgument(GetArgument(arguments, "value"), capability.ValueType) };
            }
            else
            {
                parameters = new object[capability.Parameters.Length];
                for (int index = 0; index < parameters.Length; index++)
                {
                    CapabilityParameter parameter = capability.Parameters[index];
                    parameters[index] = ConvertArgument(GetArgument(arguments, parameter.Name), parameter.ParameterType);
                }
            }

            object result = invocation.Method.Invoke(invocation.Target, parameters.Length == 0 ? null : parameters);
            if (result == null)
            {
                return "null";
            }

            string resultText;
            if (!IsSimpleType(result.GetType()))
            {
                resultText = JsonConvert.SerializeObject(result);
            }
            else
            {
                resultText = result.GetType() == typeof(bool) ? result.ToString().ToLower() : result.ToString();
            }

            return JsonString(resultText);
        }

        private void RegisterInterface(DeviceInterface deviceInterface, object target, string prefix)
        {
            for (int index = 0; index < deviceInterface.Capabilities.Length; index++)
            {
                RegisterCapability(deviceInterface.Capabilities[index], target, prefix);
            }

            for (int index = 0; index < deviceInterface.Components.Length; index++)
            {
                DeviceInterface component = deviceInterface.Components[index];
                object componentTarget = GetComponentTarget(target, component.Path);
                RegisterInterface(component, componentTarget, prefix);
            }
        }

        private void RegisterCapability(Capability capability, object target, string prefix)
        {
            Type targetType = target.GetType();
            if (capability.IsMcpResource)
            {
                MethodInfo method = FindMethod(targetType, capability.ReadMethodName);
                string relativeUri = prefix + ReplaceCharacter(capability.Path, '.', '/');
                McpResourceMetadata metadata = new McpResourceMetadata
                {
                    Registry = this,
                    Uri = "mcp://" + relativeUri,
                    Name = capability.DisplayName,
                    Description = capability.Kind == CapabilityKind.Property ? GetMethodDisplayName(capability.ReadMethodName) : capability.DisplayName,
                    MimeType = IsSimpleType(capability.ValueType) ? "text/plain" : "application/json",
                };
                Add(_resources, NormalizeResourceUri(metadata.Uri), new McpInvocation(metadata, capability, target, method));
            }

            if (capability.IsMcpTool)
            {
                string methodName = capability.Kind == CapabilityKind.Property ? capability.WriteMethodName : capability.ReadMethodName;
                MethodInfo method = FindMethod(targetType, methodName);
                string name = ReplaceCharacter(ReplaceCharacter(prefix + capability.Path, '.', '_'), '/', '_');
                McpToolMetadata metadata = new McpToolMetadata
                {
                    Registry = this,
                    Name = name,
                    Description = capability.Kind == CapabilityKind.Property ? GetMethodDisplayName(capability.WriteMethodName) : capability.DisplayName,
                    InputSchema = GetInputSchema(capability),
                };
                Add(_tools, name.ToLower(), new McpInvocation(metadata, capability, target, method));
            }
        }

        private string GetMethodDisplayName(string methodName)
        {
            return methodName.StartsWith("get_") || methodName.StartsWith("set_") ? methodName.Substring(4) : methodName;
        }

        private object GetComponentTarget(object target, string path)
        {
            int separator = path.LastIndexOf('.');
            string componentName = separator < 0 ? path : path.Substring(separator + 1);
            MethodInfo[] methods = target.GetType().GetMethods();
            for (int index = 0; index < methods.Length; index++)
            {
                MethodInfo method = methods[index];
                if (!method.Name.StartsWith("get_") || method.GetParameters().Length != 0)
                {
                    continue;
                }

                string name = method.Name.Substring(4);
                object[] attributes = method.GetCustomAttributes(true);
                for (int attributeIndex = 0; attributeIndex < attributes.Length; attributeIndex++)
                {
                    if (attributes[attributeIndex] is System.Device.Model.ComponentAttribute attribute)
                    {
                        name = string.IsNullOrEmpty(attribute.Name) ? name : attribute.Name;
                        break;
                    }
                }

                if (name == componentName)
                {
                    return method.Invoke(target, null);
                }
            }

            throw new ArgumentException();
        }

        private MethodInfo FindMethod(Type type, string methodName)
        {
            MethodInfo[] methods = type.GetMethods();
            for (int index = 0; index < methods.Length; index++)
            {
                if (methods[index].Name == methodName)
                {
                    return methods[index];
                }
            }

            throw new ArgumentException();
        }

        private string GetInputSchema(Capability capability)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{\"type\":\"object\",\"properties\":{");
            if (capability.Kind == CapabilityKind.Property)
            {
                AppendSchemaProperty(builder, "value", capability.ValueType, true);
            }
            else
            {
                for (int index = 0; index < capability.Parameters.Length; index++)
                {
                    CapabilityParameter parameter = capability.Parameters[index];
                    AppendSchemaProperty(builder, parameter.Name, parameter.ParameterType, index == 0);
                }
            }

            builder.Append("},\"required\":[");
            if (capability.Kind == CapabilityKind.Property)
            {
                builder.Append("\"value\"");
            }
            else
            {
                for (int index = 0; index < capability.Parameters.Length; index++)
                {
                    if (index > 0)
                    {
                        builder.Append(',');
                    }

                    builder.Append('"').Append(capability.Parameters[index].Name).Append('"');
                }
            }

            builder.Append("]}");
            return builder.ToString();
        }

        private void AppendSchemaProperty(StringBuilder builder, string name, Type type, bool first)
        {
            if (!first)
            {
                builder.Append(',');
            }

            builder.Append('"').Append(name).Append("\":{\"type\":\"").Append(GetJsonType(type)).Append("\"}");
        }

        private string GetJsonType(Type type)
        {
            if (type == typeof(bool))
            {
                return "boolean";
            }

            if (type == typeof(string) || type == typeof(char))
            {
                return "string";
            }

            return IsIntegralType(type) ? "integer" : IsSimpleType(type) ? "number" : "object";
        }

        private bool IsIntegralType(Type type)
        {
            return type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort)
                || type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong);
        }

        private bool IsSimpleType(Type type)
        {
            return type == typeof(string) || type == typeof(char) || type == typeof(bool)
                || type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort)
                || type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong)
                || type == typeof(float) || type == typeof(double);
        }

        private object GetArgument(Hashtable arguments, string name)
        {
            if (arguments == null || !arguments.Contains(name))
            {
                throw new ArgumentException("Missing tool argument: " + name);
            }

            return arguments[name];
        }

        private object ConvertArgument(object value, Type type)
        {
            if (value == null)
            {
                throw new ArgumentException("Null tool argument");
            }

            string text = value.ToString();
            if (type == typeof(string))
            {
                return text;
            }

            if (type == typeof(bool))
            {
                string lower = text.ToLower();
                if (lower == "true")
                {
                    return true;
                }

                if (lower == "false")
                {
                    return false;
                }

                throw new InvalidCastException("Invalid Boolean argument");
            }

            if (type == typeof(int))
            {
                return Convert.ToInt32(text);
            }

            if (type == typeof(double))
            {
                return Convert.ToDouble(text);
            }

            if (type == typeof(float))
            {
                return Convert.ToSingle(text);
            }

            if (type == typeof(long))
            {
                return Convert.ToInt64(text);
            }

            if (type == typeof(byte))
            {
                return Convert.ToByte(text);
            }

            if (type == typeof(short))
            {
                return Convert.ToInt16(text);
            }

            if (type == typeof(char))
            {
                return string.IsNullOrEmpty(text) ? '\0' : text[0];
            }

            if (type == typeof(uint))
            {
                return Convert.ToUInt32(text);
            }

            if (type == typeof(ulong))
            {
                return Convert.ToUInt64(text);
            }

            if (type == typeof(ushort))
            {
                return Convert.ToUInt16(text);
            }

            if (type == typeof(sbyte))
            {
                return Convert.ToSByte(text);
            }

            throw new ArgumentException("Unsupported MCP argument type: " + type.FullName);
        }

        private string GetMetadataJson(string memberName, Hashtable values)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('"').Append(memberName).Append("\":[");
            bool first = true;
            foreach (McpInvocation invocation in values.Values)
            {
                if (!first)
                {
                    builder.Append(',');
                }

                first = false;
                builder.Append(invocation.Metadata.ToString());
            }

            return builder.Append(']').ToString();
        }

        private string NormalizePrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return string.Empty;
            }

            return prefix.EndsWith("/") ? prefix : prefix + "/";
        }

        private string NormalizeResourceUri(string uri)
        {
            string normalized = uri.ToLower();
            return normalized.StartsWith("mcp://") ? normalized : "mcp://" + normalized;
        }

        private void Add(Hashtable values, string key, McpInvocation invocation)
        {
            if (values.Contains(key))
            {
                throw new ArgumentException();
            }

            values.Add(key, invocation);
        }

        private string JsonString(string value)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('"');
            for (int index = 0; index < value.Length; index++)
            {
                char character = value[index];
                if (character == '\\' || character == '"')
                {
                    builder.Append('\\').Append(character);
                }
                else if (character == '\r')
                {
                    builder.Append("\\r");
                }
                else if (character == '\n')
                {
                    builder.Append("\\n");
                }
                else if (character == '\t')
                {
                    builder.Append("\\t");
                }
                else if (character < ' ')
                {
                    builder.Append("\\u").Append(((int)character).ToString("x4"));
                }
                else
                {
                    builder.Append(character);
                }
            }

            return builder.Append('"').ToString();
        }

        private string ReplaceCharacter(string value, char oldCharacter, char newCharacter)
        {
            StringBuilder builder = new StringBuilder(value.Length);
            for (int index = 0; index < value.Length; index++)
            {
                char character = value[index];
                builder.Append(character == oldCharacter ? newCharacter : character);
            }

            return builder.ToString();
        }

        private sealed class McpInvocation
        {
            public McpInvocation(object metadata, Capability capability, object target, MethodInfo method)
            {
                Metadata = metadata;
                Capability = capability;
                Target = target;
                Method = method;
            }

            public object Metadata { get; }

            public Capability Capability { get; }

            public object Target { get; }

            public MethodInfo Method { get; }
        }

        private sealed class McpResourceMetadata
        {
            public McpDeviceRegistry Registry { get; set; }

            public string Uri { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public string MimeType { get; set; }

            public override string ToString()
            {
                return "{\"uri\":" + Registry.JsonString(Uri) + ",\"name\":" + Registry.JsonString(Name)
                    + ",\"description\":" + Registry.JsonString(Description) + ",\"mimeType\":" + Registry.JsonString(MimeType) + "}";
            }
        }

        private sealed class McpToolMetadata
        {
            public McpDeviceRegistry Registry { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public string InputSchema { get; set; }

            public override string ToString()
            {
                return "{\"name\":" + Registry.JsonString(Name) + ",\"description\":" + Registry.JsonString(Description)
                    + ",\"inputSchema\":" + InputSchema + "}";
            }
        }
    }
}