// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

using Iot.Device.LoRa;

using nanoFramework.TestFramework;

namespace Iot.Device.LoRa.LoraTests
{
    /// <summary>
    /// Regression tests for <see cref="LoRaMessage" /> (null payload, defensive copy, immutability).
    /// </summary>
    [TestClass]
    public class LoRaMessageTests
    {
        /// <summary>
        /// Verifies a null payload throws <see cref="ArgumentNullException" /> (avoids NRE in handlers).
        /// </summary>
        [TestMethod]
        public void Constructor_NullPayload_ThrowsArgumentNullException()
        {
            Assert.Throws(typeof(ArgumentNullException), () => new LoRaMessage(null, 0, 0f));
        }

        /// <summary>
        /// Verifies the constructor copies payload bytes so later caller mutations do not change the message.
        /// </summary>
        [TestMethod]
        public void Constructor_CopiesPayload_DefensiveCopy()
        {
            byte[] source = new byte[] { 0x01, 0x02, 0x03 };
            LoRaMessage message = new LoRaMessage(source, -90, 1.25f);

            source[0] = 0xFF;

            Assert.Equal((byte)0x01, message.Payload[0]);
            Assert.Equal((byte)0x02, message.Payload[1]);
            Assert.Equal((byte)0x03, message.Payload[2]);
        }

        /// <summary>
        /// Verifies empty payloads are allowed (length zero frame).
        /// </summary>
        [TestMethod]
        public void Constructor_EmptyPayload_LengthZero()
        {
            LoRaMessage message = new LoRaMessage(new byte[0], -100, 0f);

            Assert.Equal(0, message.Payload.Length);
        }

        /// <summary>
        /// Verifies RSSI and SNR round-trip unchanged.
        /// </summary>
        [TestMethod]
        public void Constructor_PreservesRssiAndSnr()
        {
            LoRaMessage message = new LoRaMessage(new byte[] { 0x00 }, -72, 3.5f);

            Assert.Equal(-72, message.Rssi);
            Assert.True(message.Snr > 3.49f && message.Snr < 3.51f);
        }
    }
}
