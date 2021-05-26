// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Ssd13xx.Commands;
using nanoFramework.TestFramework;

namespace Iot.Device.Ssd13xx.Tests
{
    [TestClass]
    public class SetMultiplexRatioTests
    {
        [TestMethod]
        public void Get_Bytes_With_Default_ValuesSetMultiplexRatioTests()
        {
            SetMultiplexRatio setMultiplexRatio = new SetMultiplexRatio();
            byte[] actualBytes = setMultiplexRatio.GetBytes();
            Assert.Equal(new byte[] { 0xA8, 0x3F }, actualBytes);
        }

        [TestMethod]
        public void Get_BytesSetMultiplexRatioTests()
        {
            Get_Bytes(0x0F, new byte[] { 0xA8, 0x0F });
            Get_Bytes(0x3F, new byte[] { 0xA8, 0x3F });
        }

        //[Theory]
        //[InlineData(0x0F, new byte[] { 0xA8, 0x0F })]
        //[InlineData(0x3F, new byte[] { 0xA8, 0x3F })]
        public void Get_Bytes(byte multiplexRatio, byte[] expectedBytes)
        {
            SetMultiplexRatio setMultiplexRatio = new SetMultiplexRatio(multiplexRatio);
            byte[] actualBytes = setMultiplexRatio.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }

        //[Theory]
        //[InlineData(0x0E)]
        [TestMethod]
        public void Invalid_MultiplexRatio()
        {
            Assert.Throws(typeof(ArgumentException), () =>
            {
                SetMultiplexRatio setMultiplexRatio = new SetMultiplexRatio(0x0E);
            });
        }
    }
}
