// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using nanoFramework.TestFramework;

namespace Iot
{
    [TestClass]
    public class SetDisplayStartLineTests
    {
        [TestMethod]
        public void Get_Bytes_With_Default_ValuesSetDisplayStartLineTests()
        {
            SetDisplayStartLine setDisplayStartLine = new SetDisplayStartLine();
            byte[] actualBytes = setDisplayStartLine.GetBytes();
            Assert.Equal(new byte[] { 0x40 }, actualBytes);
        }

        [TestMethod]
        public void Get_BytesSetDisplayStartLineTests()
        {
            Get_Bytes(0x00, new byte[] { 0x40 });
            Get_Bytes(0x3F, new byte[] { 0x7F });
    }

        //[Theory]
        //[InlineData(0x00, new byte[] { 0x40 })]
        //[InlineData(0x3F, new byte[] { 0x7F })]
        public void Get_Bytes(byte displayStartLine, byte[] expectedBytes)
        {
            SetDisplayStartLine setDisplayStartLine = new SetDisplayStartLine(displayStartLine);
            byte[] actualBytes = setDisplayStartLine.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }

        public void Invalid_DisplayStartLine()
        {
            Invalid_DisplayStartLine(0x40);
            Invalid_DisplayStartLine(0xFF);
        }

        //[Theory]
        //[InlineData(0x40)]
        //[InlineData(0xFF)]
        public void Invalid_DisplayStartLine(byte displayStartLine)
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () =>
             {
                 SetDisplayStartLine setDisplayStartLine = new SetDisplayStartLine(displayStartLine);
             });
        }
    }
}
