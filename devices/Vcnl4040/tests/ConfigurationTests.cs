// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Vcnl4040;
using Iot.Device.Vcnl4040.Definitions;
using nanoFramework.TestFramework;

namespace Iot.Device.Vcnl4040.Tests
{
    [TestClass]
    public class ConfigurationTests
    {
        [TestMethod]
        public void EmitterConfigurationStoresValues()
        {
            EmitterConfiguration configuration = new EmitterConfiguration(
                PsLedCurrent.I200mA,
                PsDuty.Duty40,
                PsIntegrationTime.Time8_0,
                PsMultiPulse.Pulse2);

            Assert.AreEqual((byte)PsLedCurrent.I200mA, (byte)configuration.Current);
            Assert.AreEqual((byte)PsDuty.Duty40, (byte)configuration.DutyRatio);
            Assert.AreEqual((byte)PsIntegrationTime.Time8_0, (byte)configuration.IntegrationTime);
            Assert.AreEqual((byte)PsMultiPulse.Pulse2, (byte)configuration.MultiPulses);
        }

        [TestMethod]
        public void ReceiverConfigurationStoresValues()
        {
            ReceiverConfiguration configuration = new ReceiverConfiguration(true, 123, true, false);

            Assert.IsTrue(configuration.ExtendedOutputRange);
            Assert.AreEqual((ushort)123, configuration.CancellationLevel);
            Assert.IsTrue(configuration.WhiteChannelEnabled);
            Assert.IsFalse(configuration.SunlightCancellationEnabled);
        }

        [TestMethod]
        public void UsesDocumentedDefaultAddress()
        {
            Assert.AreEqual(0x60, Vcnl4040Device.DefaultI2cAddress);
        }
    }
}
