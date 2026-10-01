// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using Iot.Device.CoreDiscoveryEngine;
using nanoFramework.Networking;
using nanoFramework.WebServer;
using nanoFramework.WebServer.Mcp;

namespace Iot.Device.CoreDiscoveryEngine.Samples
{
    internal class Program
    {
        private const string WifiSsid = "YourWifiSsid";
        private const string WifiPassword = "YourPassword";

        private static WebServer _server;

        public static void Main()
        {
            Debug.WriteLine("Connecting to Wi-Fi...");
            bool connected = WifiNetworkHelper.ConnectDhcp(
                WifiSsid,
                WifiPassword,
                requiresDateTime: false,
                token: new CancellationTokenSource(60_000).Token);
            if (!connected)
            {
                Debug.WriteLine("Wi-Fi connection failed.");
                Thread.Sleep(Timeout.Infinite);
            }

            AcmeDevice acmeDevice = new AcmeDevice();
            DeviceInterface device = CapabilityDiscovery.Discover(acmeDevice);
            ThermostatDevice thermostat = new ThermostatDevice();
            DeviceInterface thermostatInterface = CapabilityDiscovery.Discover(thermostat);

            Debug.WriteLine(device.Name);
            for (int index = 0; index < device.Capabilities.Length; index++)
            {
                Capability capability = device.Capabilities[index];
                Debug.WriteLine($"{KindToString(capability.Kind)}: {capability.Path}, resource={capability.IsMcpResource}, tool={capability.IsMcpTool}");
            }

            McpDeviceRegistry registry = new McpDeviceRegistry();
            registry.Register(device, acmeDevice, "acme");
            registry.Register(thermostatInterface, thermostat, "thermostat");
            McpServerController.ResourceProvider = registry;
            McpServerController.ToolProvider = registry;
            McpServerController.ServerName = "nanoFramework Device Server";
            McpServerController.Instructions = device.Name + "; " + thermostatInterface.Name;
            McpServerController.MaximumRequestBodySize = 8192;

            _server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(McpServerController) });
            _server.Start();

            string ipAddress = GetIpAddress();
            Debug.WriteLine("Wi-Fi connected: " + ipAddress);
            Debug.WriteLine("MCP server: http://" + ipAddress + "/mcp");

            Thread.Sleep(Timeout.Infinite);
        }

        private static string GetIpAddress()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            if (interfaces == null)
            {
                return "0.0.0.0";
            }

            for (int index = 0; index < interfaces.Length; index++)
            {
                if (interfaces[index].NetworkInterfaceType == NetworkInterfaceType.Wireless80211
                    && !string.IsNullOrEmpty(interfaces[index].IPv4Address)
                    && interfaces[index].IPv4Address != "0.0.0.0")
                {
                    return interfaces[index].IPv4Address;
                }
            }

            return "0.0.0.0";
        }

        private static string KindToString(CapabilityKind kind)
        {
            return kind switch
            {
                CapabilityKind.Property => "Property",
                CapabilityKind.Command => "Command",
                CapabilityKind.Telemetry => "Telemetry",
                _ => "Unknown",
            };
        }
    }
}