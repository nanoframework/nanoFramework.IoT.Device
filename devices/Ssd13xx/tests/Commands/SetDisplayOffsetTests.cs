// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using nanoFramework.TestFramework;

namespace Iot.Device.Ssd13xx.Tests
{
    [TestClass]
    public class SetDisplayOffsetTests
    {
        [TestMethod]
        public void Get_Bytes_With_Default_ValuesSetDisplayOffsetTests()
        {
            SetDisplayOffset setDisplayOffset = new SetDisplayOffset();
            byte[] actualBytes = setDisplayOffset.GetBytes();
            Assert.Equal(new byte[] { 0xD3, 0x00 }, actualBytes);
        }

        [TestMethod]
        public void Get_BytesSetDisplayOffsetTests()
        {
            Get_Bytes(0x00, new byte[] { 0xD3, 0x00 });
            Get_Bytes(0x10, new byte[] { 0xD3, 0x10 });
        }

        //[Theory]
        //[InlineData(0x00, new byte[] { 0xD3, 0x00 })]
        //[InlineData(0x10, new byte[] { 0xD3, 0x10 })]
        public void Get_Bytes(byte displayOffset, byte[] expectedBytes)
        {
            SetDisplayOffset setDisplayOffset = new SetDisplayOffset(displayOffset);
            byte[] actualBytes = setDisplayOffset.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }

        [TestMethod]
        public void Invalid_DisplayOffset()
        {
            Invalid_DisplayOffset(0x40);
            Invalid_DisplayOffset(0xFF);
        }

        //[Theory]
        //[InlineData(0x40)]
        //[InlineData(0xFF)]
        public void Invalid_DisplayOffset(byte displayOffset)
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () =>
             {
                 SetDisplayOffset setDisplayOffset = new SetDisplayOffset(displayOffset);
             });
        }
    }
}
