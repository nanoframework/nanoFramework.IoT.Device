// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.AcmeBinding;
using Iot.Device.DeviceModel.Reflection;
using nanoFramework.TestFramework;
using System;

namespace Iot.Device.NFUnitTest
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
        }

        [TestMethod]
        public void Discover_FindsNamedTelemetry()
        {
            DeviceInterface result = CapabilityDiscovery.Discover(typeof(AcmeDevice));

            Capability capability = Find(result, "Uptime");

            Assert.AreEqual((int)CapabilityKind.Telemetry, (int)capability.Kind);
            Assert.AreEqual(typeof(int), capability.ValueType);
            Assert.IsTrue(capability.CanRead);
            Assert.IsFalse(capability.CanWrite);
        }

        [TestMethod]
        public void Discover_MergesSingleGetSetProperty()
        {
            DeviceInterface result = CapabilityDiscovery.Discover(typeof(AcmeDevice));

            Capability capability = Find(result, "SamplingRateHz");

            Assert.AreEqual((int)CapabilityKind.Property, (int)capability.Kind);
            Assert.AreEqual(typeof(int), capability.ValueType);
            Assert.IsTrue(capability.CanRead);
            Assert.IsTrue(capability.CanWrite);
        }

        [TestMethod]
        public void Discover_MergesPropertyDeclaredByPairedGetterAndSetterMethods()
        {
            DeviceInterface result = CapabilityDiscovery.Discover(typeof(AcmeDevice));

            Capability capability = Find(result, "Threshold");

            Assert.AreEqual((int)CapabilityKind.Property, (int)capability.Kind);
            Assert.AreEqual(typeof(double), capability.ValueType);
            Assert.IsTrue(capability.CanRead);
            Assert.IsTrue(capability.CanWrite);
        }

        [TestMethod]
        public void Discover_ReadsReadOnlyProperty()
        {
            DeviceInterface result = CapabilityDiscovery.Discover(typeof(AcmeDevice));

            Capability capability = Find(result, "FirmwareVersion");

            Assert.IsTrue(capability.CanRead);
            Assert.IsFalse(capability.CanWrite);
        }

        [TestMethod]
        public void Discover_PreservesAllCommandParameters()
        {
            DeviceInterface result = CapabilityDiscovery.Discover(typeof(AcmeDevice));

            Capability capability = Find(result, "Calibrate");

            Assert.AreEqual((int)CapabilityKind.Command, (int)capability.Kind);
            Assert.AreEqual(3, capability.Parameters.Length);
            Assert.AreEqual(typeof(double), capability.Parameters[0].Type);
            Assert.AreEqual(typeof(double), capability.Parameters[1].Type);
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
        public void Discover_FallsBackToTypeNameWhenInterfaceAttributeIsMissing()
        {
            DeviceInterface result = CapabilityDiscovery.Discover(typeof(UnannotatedDevice));

            Assert.AreEqual(nameof(UnannotatedDevice), result.Name);
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

        private class UnannotatedDevice
        {
        }
    }
}
