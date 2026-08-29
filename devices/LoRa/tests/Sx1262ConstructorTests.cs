// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

using Iot.Device.LoRa.Drivers.Sx1262;

using nanoFramework.TestFramework;

namespace Iot.Device.LoRa.LoraTests
{
    /// <summary>
    /// Guards constructor validation on <see cref="Sx1262" /> (no SPI hardware required for null check).
    /// </summary>
    [TestClass]
    public class Sx1262ConstructorTests
    {
        /// <summary>
        /// Verifies a null <see cref="System.Device.Spi.SpiDevice" /> throws before GPIO/SPI setup.
        /// </summary>
        [TestMethod]
        public void Constructor_NullSpiDevice_ThrowsArgumentNullException()
        {
            Assert.Throws(
                typeof(ArgumentNullException),
                () => new Sx1262(null, resetPin: 0, busyPin: 1, dio1Pin: 2));
        }
    }
}
