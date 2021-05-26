// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using nanoFramework.TestFramework;

namespace Iot.Device.Ssd13xx.Tests
{
    [TestClass]
    public class SetLowerColumnStartAddressForPageAddressingModeTests
    {
        [TestMethod]
        public void Get_Bytes_With_Default_ValuesSetLowerColumnStartAddressForPageAddressingModeTests()
        {
            SetLowerColumnStartAddressForPageAddressingMode setLowerColumnStartAddressForPageAddressingMode =
                new SetLowerColumnStartAddressForPageAddressingMode();
            byte[] actualBytes = setLowerColumnStartAddressForPageAddressingMode.GetBytes();
            Assert.Equal(new byte[] { 0x00 }, actualBytes);
        }

        [TestMethod]
        public void Get_BytesSetLowerColumnStartAddressForPageAddressingModeTests()
        {
            Get_Bytes(0x00, new byte[] { 0x00 });
            Get_Bytes(0x0F, new byte[] { 0x0F });
        }

        //[Theory]
        //[InlineData(0x00, new byte[] { 0x00 })]
        //[InlineData(0x0F, new byte[] { 0x0F })]
        public void Get_Bytes(byte lowerColumnStartAddress, byte[] expectedBytes)
        {
            SetLowerColumnStartAddressForPageAddressingMode setLowerColumnStartAddressForPageAddressingMode =
                new SetLowerColumnStartAddressForPageAddressingMode(lowerColumnStartAddress);
            byte[] actualBytes = setLowerColumnStartAddressForPageAddressingMode.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }

        [TestMethod]
        public void Invalid_LowerColumnStartAddressSetLowerColumnStartAddressForPageAddressingModeTests()
        {
            Invalid_LowerColumnStartAddress(0x10);
            Invalid_LowerColumnStartAddress(0xFF);
        }

        //[Theory]
        //[InlineData(0x10)]
        //[InlineData(0xFF)]
        public void Invalid_LowerColumnStartAddress(byte lowerColumnStartAddress)
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () =>
            {
                SetLowerColumnStartAddressForPageAddressingMode setLowerColumnStartAddressForPageAddressingMode =
                new SetLowerColumnStartAddressForPageAddressingMode(lowerColumnStartAddress);
            });
        }
    }
}
