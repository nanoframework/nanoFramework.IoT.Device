// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

using nanoFramework.TestFramework;

namespace Iot.Device.Vl53L1X.Tests
{
    [TestClass]
    public class RoiTests
    {
        [TestMethod]
        public void ConstructorStoresDimensions()
        {
            Roi roi = new(8, 12);

            Assert.AreEqual((ushort)8, roi.Width);
            Assert.AreEqual((ushort)12, roi.Height);
        }

        [TestMethod]
        public void ConstructorRejectsDimensionsOutsideSensorLimits()
        {
            Assert.ThrowsException(typeof(ArgumentOutOfRangeException), () => new Roi(3, 4));
            Assert.ThrowsException(typeof(ArgumentOutOfRangeException), () => new Roi(4, 17));
        }

        [TestMethod]
        public void DefaultAddressMatchesDatasheet()
        {
            Assert.AreEqual((byte)0x29, Vl53L1X.DefaultI2cAddress);
        }
    }
}
