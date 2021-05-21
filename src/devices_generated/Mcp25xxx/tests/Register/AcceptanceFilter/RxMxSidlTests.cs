// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.AcceptanceFilter;
using Xunit;

namespace Iot.Device.Mcp25xxx.Tests.Register.AcceptanceFilter
{
    public class RxMxSidlTests
    {
        [Theory]
        [InlineData(0, Address.RxM0Sidl)]
        [InlineData(1, Address.RxM1Sidl)]
        public void Get_RxMaskNumber_Address(byte rxMaskNumber, Address address)
        {
            Assert.Equal(rxMaskNumber, RxMxSidl.GetRxMaskNumber(address));
            Assert.Equal(address, new RxMxSidl(rxMaskNumber, 0x00, 0x00).Address);
        }

        [Theory]
        [InlineData(0b00, 0b000, 0b0000_0000)]
        [InlineData(0b11, 0b000, 0b0000_0011)]
        [InlineData(0b00, 0b111, 0b1110_0000)]
        public void From_To_Byte(byte extendedIdentifierMask, byte standardIdentifierMask, byte expectedByte)
        {
            var rxMxSidl = new RxMxSidl(0, extendedIdentifierMask, standardIdentifierMask);
            Assert.Equal(extendedIdentifierMask, rxMxSidl.ExtendedIdentifierMask);
            Assert.Equal(standardIdentifierMask, rxMxSidl.StandardIdentifierMask);
            Assert.Equal(expectedByte, rxMxSidl.ToByte());
            Assert.Equal(expectedByte, new RxMxSidl(0, expectedByte).ToByte());
        }

        [Theory]
        [InlineData(2, 0b000, 0b000)]
        [InlineData(0, 0b100, 0b000)]
        [InlineData(0, 0b00, 0b1000)]
        public void Invalid_Arguments(byte rxMaskNumber, byte extendedIdentifierMask, byte standardIdentifierMask)
        {
            Assert.Throws<ArgumentException>(() =>
             new RxMxSidl(rxMaskNumber, extendedIdentifierMask, standardIdentifierMask).ToByte());
        }
    }
}
