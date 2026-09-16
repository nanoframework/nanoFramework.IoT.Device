// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.AcmeBinding;
using nanoFramework.IoT.Device.CoreDiscoveryEngine;
using nanoFramework.TestFramework;
using System;

namespace nanoFramework.IoT.Device.CoreDiscoveryEngine.Tests
{
    [TestClass]
    public class CapabilityDiscoveryTests
    {
        [TestMethod]
        public void Discover_UsesInterfaceMetadata()
        {
            DeviceInterface result = CapabilityDiscovery.Discover(typeof(AcmeDevice));

            Assert.AreEqual("Acme synthetic sensor/actuator", result.Name);
            Assert.AreEqual(typeof(AcmeDevice), result.Type);
            Assert.AreEqual(string.Empty, result.Path);
        }

        [TestMethod]
        public void Discover_FindsNamedTelemetry()
        {
            DeviceInterface result = CapabilityDiscovery.Discover(typeof(AcmeDevice));

            Capability capability = Find(result, "Uptime");

            Assert.AreEqual(CapabilityKind.Telemetry, capability.Kind);
            Assert.AreEqual(typeof(int), capability.ValueType);
            Assert.AreEqual("Uptime", capability.Path);
            Assert.IsTrue(capability.CanRead);
            Assert.IsFalse(capability.CanWrite);
        }

        [TestMethod]
        public void Discover_MergesPropertyGetterAndSetter()
        {
            DeviceInterface result = CapabilityDiscovery.Discover(typeof(AcmeDevice));

            Capability capability = Find(result, "Threshold");

            Assert.AreEqual(CapabilityKind.Property, capability.Kind);
            Assert.AreEqual(typeof(double), capability.ValueType);
            Assert.IsTrue(capability.CanRead);
            Assert.IsTrue(capability.CanWrite);
        }

        [TestMethod]
        public void Discover_PreservesAllCommandParameters()
        {
            DeviceInterface result = CapabilityDiscovery.Discover(typeof(AcmeDevice));

            Capability capability = Find(result, "Calibrate");

            Assert.AreEqual(CapabilityKind.Command, capability.Kind);
            Assert.AreEqual(3, capability.Parameters.Length);
            Assert.AreEqual("arg0", capability.Parameters[0].Name);
            Assert.AreEqual(typeof(double), capability.Parameters[0].Type);
            Assert.AreEqual("arg1", capability.Parameters[1].Name);
            Assert.AreEqual(typeof(double), capability.Parameters[1].Type);
            Assert.AreEqual("arg2", capability.Parameters[2].Name);
            Assert.AreEqual(typeof(int), capability.Parameters[2].Type);
        }

        [TestMethod]
        public void Discover_AcceptsDeviceInstance()
        {
            AcmeDevice device = new AcmeDevice();

            DeviceInterface result = CapabilityDiscovery.Discover(device);

            Assert.AreEqual(typeof(AcmeDevice), result.Type);
        }

        [TestMethod]
        public void Discover_RejectsNullDevice()
        {
            bool rejected = false;
            try
            {
                CapabilityDiscovery.Discover((object)null);
            }
            catch (ArgumentNullException)
            {
                rejected = true;
            }

            Assert.IsTrue(rejected);
        }

        [TestMethod]
        public void Discover_RejectsUnannotatedType()
        {
            bool rejected = false;
            try
            {
                CapabilityDiscovery.Discover(typeof(UnannotatedDevice));
            }
            catch (CapabilityModelException)
            {
                rejected = true;
            }

            Assert.IsTrue(rejected);
        }

        private static Capability Find(DeviceInterface deviceInterface, string name)
        {
            for (int index = 0; index < deviceInterface.Capabilities.Length; index++)
            {
                if (deviceInterface.Capabilities[index].Name == name)
                {
                    return deviceInterface.Capabilities[index];
                }
            }

            Assert.IsTrue(false, "Capability was not found: " + name);
            return null;
        }

        private sealed class UnannotatedDevice
        {
        }
    }
}