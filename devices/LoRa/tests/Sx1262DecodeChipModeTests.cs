// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.LoRa.Drivers.Sx1262;

using nanoFramework.TestFramework;

namespace Iot.Device.LoRa.LoraTests
{
    /// <summary>
    /// Unit tests for <see cref="Sx1262.DecodeChipMode(byte)" /> (no hardware required).
    /// </summary>
    [TestClass]
    public class Sx1262DecodeChipModeTests
    {
        /// <summary>
        /// Verifies decoding of standby-RC mode bits in the status byte.
        /// </summary>
        [TestMethod]
        public void DecodeChipMode_StandbyRc_ReturnsStdbyRc()
        {
            byte status = (byte)(0x02 << 4);
            Assert.Equal("STDBY_RC", Sx1262.DecodeChipMode(status));
        }

        /// <summary>
        /// Verifies decoding of standby-XOSC mode bits in the status byte.
        /// </summary>
        [TestMethod]
        public void DecodeChipMode_StandbyXosc_ReturnsStdbyXosc()
        {
            byte status = (byte)(0x03 << 4);
            Assert.Equal("STDBY_XOSC", Sx1262.DecodeChipMode(status));
        }

        /// <summary>
        /// Verifies decoding of frequency-synthesizer mode bits in the status byte.
        /// </summary>
        [TestMethod]
        public void DecodeChipMode_Fs_ReturnsFs()
        {
            byte status = (byte)(0x04 << 4);
            Assert.Equal("FS", Sx1262.DecodeChipMode(status));
        }

        /// <summary>
        /// Verifies decoding of RX mode bits in the status byte.
        /// </summary>
        [TestMethod]
        public void DecodeChipMode_Rx_ReturnsRx()
        {
            byte status = (byte)(0x05 << 4);
            Assert.Equal("RX", Sx1262.DecodeChipMode(status));
        }

        /// <summary>
        /// Verifies decoding of TX mode bits in the status byte.
        /// </summary>
        [TestMethod]
        public void DecodeChipMode_Tx_ReturnsTx()
        {
            byte status = (byte)(0x06 << 4);
            Assert.Equal("TX", Sx1262.DecodeChipMode(status));
        }

        /// <summary>
        /// Verifies mode code 0 in bits [6:4] maps to unknown.
        /// </summary>
        [TestMethod]
        public void DecodeChipMode_Mode0_ReturnsUnknown()
        {
            byte status = (byte)(0x00 << 4);
            Assert.Equal("UNKNOWN", Sx1262.DecodeChipMode(status));
        }

        /// <summary>
        /// Verifies reserved mode code 7 in bits [6:4] maps to unknown.
        /// </summary>
        [TestMethod]
        public void DecodeChipMode_Mode7_ReturnsUnknown()
        {
            byte status = (byte)(0x07 << 4);
            Assert.Equal("UNKNOWN", Sx1262.DecodeChipMode(status));
        }

        /// <summary>
        /// Verifies unknown mode bits return the unknown label.
        /// </summary>
        [TestMethod]
        public void DecodeChipMode_Unknown_ReturnsUnknown()
        {
            byte status = (byte)(0x01 << 4);
            Assert.Equal("UNKNOWN", Sx1262.DecodeChipMode(status));
        }
    }
}
