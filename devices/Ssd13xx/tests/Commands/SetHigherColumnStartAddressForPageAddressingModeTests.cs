// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using nanoFramework.TestFramework;

namespace Iot.Device.Ssd13xx.Tests
{
    [TestClass]
    public class SetHigherColumnStartAddressForPageAddressingModeTests
    {
        [TestMethod]
        public void Get_Bytes_With_Default_ValuesSetHigherColumnStartAddressForPageAddressingModeTests()
        {
            SetHigherColumnStartAddressForPageAddressingMode setHigherColumnStartAddressForPageAddressingMode =
                new SetHigherColumnStartAddressForPageAddressingMode();
            byte[] actualBytes = setHigherColumnStartAddressForPageAddressingMode.GetBytes();
            Assert.Equal(new byte[] { 0x10 }, actualBytes);
        }

        [TestMethod]
        public void Get_BytesSetHigherColumnStartAddressForPageAddressingModeTests()
        {
            Get_Bytes(0x00, new byte[] { 0x10 });
            Get_Bytes(0x0F, new byte[] { 0x1F });

        }

        //[Theory]
        //[InlineData(0x00, new byte[] { 0x10 })]
        //[InlineData(0x0F, new byte[] { 0x1F })]
        public void Get_Bytes(byte higherColumnStartAddress, byte[] expectedBytes)
        {
            SetHigherColumnStartAddressForPageAddressingMode setHigherColumnStartAddressForPageAddressingMode =
                new SetHigherColumnStartAddressForPageAddressingMode(higherColumnStartAddress);
            byte[] actualBytes = setHigherColumnStartAddressForPageAddressingMode.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }

        [TestMethod]
        public void Invalid_HigherColumnStartAddress()
        {
            Invalid_HigherColumnStartAddress(0x10);
            Invalid_HigherColumnStartAddress(0xFF);
        }

        //[Theory]
        //[InlineData(0x10)]
        //[InlineData(0xFF)]
        public void Invalid_HigherColumnStartAddress(byte higherColumnStartAddress)
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () =>
             {
                 SetHigherColumnStartAddressForPageAddressingMode setHigherColumnStartAddressForPageAddressingMode =
                 new SetHigherColumnStartAddressForPageAddressingMode(higherColumnStartAddress);
             });
        }
    }
}
