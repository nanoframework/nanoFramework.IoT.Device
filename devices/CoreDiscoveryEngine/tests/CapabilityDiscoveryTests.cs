// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using Iot.Device.CoreDiscoveryEngine.Samples;
using nanoFramework.TestFramework;
using nanoFramework.WebServer.Mcp;

namespace Iot.Device.CoreDiscoveryEngine.Tests
{
    [TestClass]
    public class CapabilityDiscoveryTests
    {
        [TestMethod]
        public void DiscoverAcmeDeviceFindsAccessorMetadata()
        {
            DeviceInterface device = CapabilityDiscovery.Discover(typeof(AcmeDevice));

            Assert.AreEqual("Acme synthetic sensor/actuator", device.Name);
            Assert.AreEqual(7, device.Capabilities.Length);

            Capability temperature = Find(device, "Temperature");
            Assert.AreEqual((int)CapabilityKind.Telemetry, (int)temperature.Kind);
            Assert.IsTrue(temperature.ValueType == typeof(double));
            Assert.IsTrue(temperature.CanRead);
            Assert.IsTrue(temperature.IsMcpResource);
            Assert.IsFalse(temperature.IsMcpTool);
            Assert.AreEqual("get_Temperature", temperature.ReadMethodName);
        }

        [TestMethod]
        public void DiscoverAcmeDeviceMergesPropertyAccessors()
        {
            DeviceInterface device = CapabilityDiscovery.Discover(new AcmeDevice());
            Capability samplingRate = Find(device, "SamplingRate");
            Capability threshold = Find(device, "Threshold");

            Assert.IsTrue(samplingRate.CanRead);
            Assert.IsTrue(samplingRate.CanWrite);
            Assert.IsTrue(samplingRate.IsMcpResource);
            Assert.IsTrue(samplingRate.IsMcpTool);
            Assert.AreEqual("get_SamplingRate", samplingRate.ReadMethodName);
            Assert.AreEqual("set_SamplingRate", samplingRate.WriteMethodName);

            Assert.IsTrue(threshold.CanRead);
            Assert.IsTrue(threshold.CanWrite);
            Assert.IsTrue(threshold.ValueType == typeof(double));
        }

        [TestMethod]
        public void DiscoverAcmeDevicePreservesCommandParameters()
        {
            DeviceInterface device = CapabilityDiscovery.Discover(typeof(AcmeDevice));
            Capability calibrate = Find(device, "Calibrate");

            Assert.AreEqual((int)CapabilityKind.Command, (int)calibrate.Kind);
            Assert.IsTrue(calibrate.IsMcpTool);
            Assert.AreEqual(3, calibrate.Parameters.Length);
            Assert.AreEqual("arg0", calibrate.Parameters[0].Name);
            Assert.IsTrue(calibrate.Parameters[0].ParameterType == typeof(double));
            Assert.AreEqual("arg2", calibrate.Parameters[2].Name);
            Assert.IsTrue(calibrate.Parameters[2].ParameterType == typeof(int));
        }

        [TestMethod]
        public void DiscoverRejectsNull()
        {
            Assert.ThrowsException(typeof(ArgumentNullException), () => CapabilityDiscovery.Discover((object)null));
            Assert.ThrowsException(typeof(ArgumentNullException), () => CapabilityDiscovery.Discover((Type)null));
        }

        [TestMethod]
        public void McpRegistryGeneratesAcmeElements()
        {
            AcmeDevice acmeDevice = new AcmeDevice();
            DeviceInterface device = CapabilityDiscovery.Discover(acmeDevice);
            McpDeviceRegistry registry = new McpDeviceRegistry();
            registry.Register(device, acmeDevice, "acme");

            string resources = registry.GetResourceMetadataJson();
            string tools = registry.GetToolMetadataJson();

            Assert.IsTrue(resources.Contains("mcp://acme/Temperature"));
            Assert.IsTrue(resources.Contains("mcp://acme/SamplingRate"));
            Assert.IsTrue(tools.Contains("acme_SamplingRate"));
            Assert.IsTrue(tools.Contains("acme_Calibrate"));
            Assert.IsTrue(tools.Contains("arg0"));
            Assert.IsTrue(tools.Contains("arg2"));
            Assert.IsTrue(resources.Contains("GetThreshold"));
            Assert.IsTrue(tools.Contains("SetThreshold"));
            Assert.IsTrue(tools.Contains("\"required\":[\"value\"]"));
            Assert.IsTrue(tools.Contains("\"required\":[\"arg0\",\"arg1\",\"arg2\"]"));
        }

        [TestMethod]
        public void McpRegistryInvokesLiveAcmeDevice()
        {
            AcmeDevice device = new AcmeDevice();
            McpDeviceRegistry registry = new McpDeviceRegistry();
            registry.Register(device, "live");
            Hashtable setterArguments = new Hashtable();
            setterArguments.Add("value", "25");

            registry.InvokeTool("live_SamplingRate", setterArguments);
            string resource = registry.ReadResource("mcp://live/SamplingRate");

            Assert.AreEqual(25, device.SamplingRate);
            Assert.IsTrue(resource.Contains("25"));

            Hashtable commandArguments = new Hashtable();
            commandArguments.Add("arg0", "1");
            commandArguments.Add("arg1", "2");
            commandArguments.Add("arg2", "3");
            string commandResult = registry.InvokeTool("live_Calibrate", commandArguments);
            Assert.AreEqual("\"true\"", commandResult);
        }

        [TestMethod]
        public void McpRegistryReadsResourceCaseInsensitively()
        {
            McpDeviceRegistry registry = new McpDeviceRegistry();
            registry.Register(new AcmeDevice(), "live");

            string resource = registry.ReadResource("MCP://LIVE/sAMPLINGrATE");

            Assert.IsTrue(resource.Contains("mcp://live/SamplingRate"));
            Assert.IsTrue(resource.Contains("10"));
        }

        [TestMethod]
        public void McpRegistryReadsResourceWithoutScheme()
        {
            McpDeviceRegistry registry = new McpDeviceRegistry();
            registry.Register(new AcmeDevice(), "acme");

            string resource = registry.ReadResource("acme/threshold");

            Assert.IsTrue(resource.Contains("mcp://acme/Threshold"));
            Assert.IsTrue(resource.Contains("1.5"));
        }

        [TestMethod]
        public void McpRegistryInvokesToolCaseInsensitively()
        {
            AcmeDevice device = new AcmeDevice();
            McpDeviceRegistry registry = new McpDeviceRegistry();
            registry.Register(device, "live");
            Hashtable arguments = new Hashtable();
            arguments.Add("value", "25");

            registry.InvokeTool("LIVE_sAMPLINGrATE", arguments);

            Assert.AreEqual(25, device.SamplingRate);
        }

        [TestMethod]
        public void McpRegistryControlsThermostat()
        {
            ThermostatDevice thermostat = new ThermostatDevice();
            DeviceInterface thermostatInterface = CapabilityDiscovery.Discover(thermostat);
            McpDeviceRegistry registry = new McpDeviceRegistry();
            registry.Register(thermostatInterface, thermostat, "thermostat");
            Hashtable arguments = new Hashtable();
            arguments.Add("value", "24.5");

            registry.InvokeTool("thermostat_TargetTemperature", arguments);
            registry.InvokeTool("thermostat_StartHeating", new Hashtable());

            Assert.AreEqual(24.5, thermostat.TargetTemperature);
            Assert.IsTrue(thermostat.IsHeating);
            Assert.IsTrue(thermostat.CurrentTemperature < thermostat.TargetTemperature);
            Assert.IsTrue(registry.GetResourceMetadataJson().Contains("mcp://thermostat/CurrentTemperature"));
            Assert.IsTrue(registry.GetToolMetadataJson().Contains("thermostat_StopHeating"));

            registry.InvokeTool("thermostat_StopHeating", new Hashtable());
            Assert.IsFalse(thermostat.IsHeating);
        }

        [TestMethod]
        public void McpRegistryImplementsProviderContracts()
        {
            McpDeviceRegistry registry = new McpDeviceRegistry();

            Assert.IsTrue(registry is IMcpResourceProvider);
            Assert.IsTrue(registry is IMcpToolProvider);
            Assert.ThrowsException(typeof(McpResourceRegistry.ResourceNotFoundException), () =>
            {
                registry.ReadResource("mcp://missing");
            });
        }

        private static Capability Find(DeviceInterface device, string name)
        {
            for (int index = 0; index < device.Capabilities.Length; index++)
            {
                if (device.Capabilities[index].Name == name)
                {
                    return device.Capabilities[index];
                }
            }

            throw new Exception("Capability not found: " + name);
        }
    }
}