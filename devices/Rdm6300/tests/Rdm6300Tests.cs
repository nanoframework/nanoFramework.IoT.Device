// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.TestFramework;

namespace Iot.Device.Rdm6300.Tests
{
    [TestClass]
    public class Rdm6300Tests
    {
        [TestMethod]
        public void TryParseFrame_ValidFrame_ReturnsTag()
        {
            byte[] frame = new byte[] { 0x02, (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'8', (byte)'9', (byte)'0', (byte)'9', (byte)'8', 0x03 };

            bool result = Rdm6300.TryParseFrame(frame, out string tag, out byte tagVersion, out uint tagId);

            Assert.IsTrue(result);
            Assert.AreEqual("1234567890", tag);
            Assert.AreEqual((byte)0x12, tagVersion);
            Assert.AreEqual(0x34567890U, tagId);
        }

        [TestMethod]
        public void TryParseFrame_LowercaseHex_ReturnsTag()
        {
            byte[] frame = new byte[] { 0x02, (byte)'0', (byte)'a', (byte)'1', (byte)'b', (byte)'2', (byte)'c', (byte)'3', (byte)'d', (byte)'4', (byte)'e', (byte)'4', (byte)'e', 0x03 };

            bool result = Rdm6300.TryParseFrame(frame, out string tag, out byte tagVersion, out uint tagId);

            Assert.IsTrue(result);
            Assert.AreEqual("0a1b2c3d4e", tag);
            Assert.AreEqual((byte)0x0A, tagVersion);
            Assert.AreEqual(0x1B2C3D4EU, tagId);
        }

        [TestMethod]
        public void TryParseFrame_InvalidChecksum_ReturnsFalse()
        {
            byte[] frame = new byte[] { 0x02, (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'8', (byte)'9', (byte)'0', (byte)'0', (byte)'0', 0x03 };

            bool result = Rdm6300.TryParseFrame(frame, out string tag);

            Assert.IsFalse(result);
            Assert.IsNull(tag);
        }

        [TestMethod]
        public void TryParseFrame_InvalidHex_ReturnsFalse()
        {
            byte[] frame = new byte[] { 0x02, (byte)'G', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'8', (byte)'9', (byte)'0', (byte)'9', (byte)'8', 0x03 };

            bool result = Rdm6300.TryParseFrame(frame, out string tag);

            Assert.IsFalse(result);
            Assert.IsNull(tag);
        }

        [TestMethod]
        public void TryParseFrame_IncorrectLength_ReturnsFalse()
        {
            bool result = Rdm6300.TryParseFrame(new byte[13], out string tag);

            Assert.IsFalse(result);
            Assert.IsNull(tag);
        }
    }
}
