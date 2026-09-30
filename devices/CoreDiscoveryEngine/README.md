# Core Discovery Engine for MCP using System.Device.Model

CoreDiscoveryEngine inspects public methods and property accessors decorated with
`System.Device.Model` attributes and produces a protocol-neutral description of
the device interface. Readable telemetry and properties are MCP resource
candidates; commands and writable properties are MCP tool candidates.

The implementation is designed for .NET nanoFramework constraints: it uses
`ArrayList` and arrays, performs no asynchronous work, and does not depend on
`PropertyInfo`. Property metadata can be applied to the generated
`get_` or `set_` accessor for individual selection or on the main property name:

```csharp
[Property]
public int SamplingRate
{    
    get
    {
        return _samplingRate;
    }

    set
    {
        _samplingRate = value;
    }
}
```

```csharp
DeviceInterface discovered = CapabilityDiscovery.Discover(device);

foreach (Capability capability in discovered.Capabilities)
{
    Debug.WriteLine($"{capability.Path}: resource={capability.IsMcpResource}, tool={capability.IsMcpTool}");
}
```

`McpDeviceRegistry` turns that description into live MCP resources and tools:

```csharp
McpDeviceRegistry registry = new McpDeviceRegistry();
registry.Register(discovered, device, "sensor-a");

McpServerController.ResourceProvider = registry;
McpServerController.ToolProvider = registry;

string resources = registry.GetResourceMetadataJson();
string tools = registry.GetToolMetadataJson();
```

The registry also provides `ReadResource` and `InvokeTool`, including positional
`arg0`, `arg1`, and subsequent arguments for multi-parameter commands. An MCP
controller can delegate its `resources/list`, `resources/read`, `tools/list`,
and `tools/call` handlers directly to these four methods. Resource reads accept
both full `mcp://sensor-a/Reading` URIs and values without the `mcp://` scheme.

This binding is intended to expose existing sensor bindings to MCP from their
`System.Device.Model` metadata. It does not require MCP-specific attributes,
wrapper methods, or additional registration code in the sensor binding.

Dynamic server integration requires `IMcpResourceProvider`,
`IMcpToolProvider`, `McpServerController.ResourceProvider`, and
`McpServerController.ToolProvider` from `nanoFramework.WebServer.Mcp`. These
extension points allow the controller to delegate discovery and invocation to
`McpDeviceRegistry` without adding MCP attributes to the device implementation.
Dynamic providers require `nanoFramework.WebServer.Mcp` 1.2.162 or later.

## ESP32 sample

The sample connects an ESP32 to Wi-Fi and starts the MCP server supplied by the
`nanoFramework.WebServer.Mcp` package. It registers an Acme device and a
synthetic thermostat in the same registry. The thermostat exposes target and
current temperatures, heating state, and tools to start or stop heating.
`McpServerController` provides the MCP endpoint and delegates resources and
tools to the dynamically populated `McpDeviceRegistry`.

1. Set `WifiSsid` and `WifiPassword` in `samples/Program.cs`.
2. Select an ESP32 nanoFramework device and deploy the sample project.
3. Read the Debug output for the assigned address and MCP endpoint, for example
    `http://192.168.1.42/mcp`.

The ESP32 and MCP client must be reachable on the same network. The sample uses
plain, unauthenticated HTTP and is intended for development networks.

## Command argument names

nanoFramework reflection exposes command parameter types but not their source
names. Command arguments are therefore published as `arg0`, `arg1`, and so on,
in declaration order.

Applications that need richer MCP metadata can use the MCP package directly.
Its `McpServerTool`, `McpServerResource`, and `Description` attributes support
explicit names, descriptions, and modeled input details. That approach requires
MCP-specific annotations or input models; CoreDiscoveryEngine favors automatic
exposure without changes to existing sensor code.
