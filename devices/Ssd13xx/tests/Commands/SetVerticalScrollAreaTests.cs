// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using nanoFramework.TestFramework;

namespace Iot.Device.Ssd13xx.Tests
{
    [TestClass]
    public class SetVerticalScrollAreaTests
    {
        [TestMethod]
        public void Get_Bytes_With_Default_ValuesSetVerticalScrollAreaTests()
        {
            SetVerticalScrollArea setVerticalScrollArea = new SetVerticalScrollArea();
            byte[] actualBytes = setVerticalScrollArea.GetBytes();
            Assert.Equal(new byte[] { 0xA3, 0x00, 0x40 }, actualBytes);
        }

        [TestMethod]
        public void Get_BytesSetVerticalScrollAreaTests()
        {
            Get_Bytes(0x00, 0x00, new byte[] { 0xA3, 0x00, 0x00 });
            Get_Bytes(0x3F, 0x00, new byte[] { 0xA3, 0x3F, 0x00 });
            // ScrollAreaRows
            Get_Bytes(0x00, 0x7F, new byte[] { 0xA3, 0x00, 0x7F });
        }

        //[Theory]
        //// TopFixedAreaRows
        //[InlineData(0x00, 0x00, new byte[] { 0xA3, 0x00, 0x00 })]
        //[InlineData(0x3F, 0x00, new byte[] { 0xA3, 0x3F, 0x00 })]
        //// ScrollAreaRows
        //[InlineData(0x00, 0x7F, new byte[] { 0xA3, 0x00, 0x7F })]
        public void Get_Bytes(byte topFixedAreaRows, byte scrollAreaRows, byte[] expectedBytes)
        {
            SetVerticalScrollArea setVerticalScrollArea = new SetVerticalScrollArea(topFixedAreaRows, scrollAreaRows);
            byte[] actualBytes = setVerticalScrollArea.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }

        [TestMethod]
        public void Invalid_HorizontalScrollSetupSetVerticalScrollAreaTests()
        {
            Invalid_HorizontalScrollSetup(0x40, 0x00);
            // ScrollAreaRows
            Invalid_HorizontalScrollSetup(0x00, 0x80);
            // TopFixedAreaRows and ScrollAreaRows
            Invalid_HorizontalScrollSetup(0xAA, 0x99);
        }

        //[Theory]
        //// TopFixedAreaRows
        //[InlineData(0x40, 0x00)]
        //// ScrollAreaRows
        //[InlineData(0x00, 0x80)]
        //// TopFixedAreaRows and ScrollAreaRows
        //[InlineData(0xAA, 0x99)]
        public void Invalid_HorizontalScrollSetup(byte topFixedAreaRows, byte scrollAreaRows)
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () =>
            {
                SetVerticalScrollArea setVerticalScrollArea = new SetVerticalScrollArea(topFixedAreaRows, scrollAreaRows);
            });
        }
    }
}
