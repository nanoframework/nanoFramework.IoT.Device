// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using nanoFramework.TestFramework;

namespace Iot.Device.Ssd13xx.Tests
{
    [TestClass]
    public class SetPreChargePeriodTests
    {
        [TestMethod]
        public void Get_Bytes_With_Default_ValuesSetPreChargePeriodTests()
        {
            SetPreChargePeriod setPreChargePeriod = new SetPreChargePeriod();
            byte[] actualBytes = setPreChargePeriod.GetBytes();
            Assert.Equal(new byte[] { 0xD9, 0x22 }, actualBytes);
        }

        [TestMethod]
        public void Get_BytesSetPreChargePeriodTests()
        {
            Get_Bytes(0x01, 0x01, new byte[] { 0xD9, 0x11 });
            Get_Bytes(0x0F, 0x01, new byte[] { 0xD9, 0x1F });
            // Phase2Period
            Get_Bytes(0x01, 0x0F, new byte[] { 0xD9, 0xF1 });
            Get_Bytes(0x0B, 0x06, new byte[] { 0xD9, 0x6B });
        }

        //[Theory]
        //// Phase1Period
        //[InlineData(0x01, 0x01, new byte[] { 0xD9, 0x11 })]
        //[InlineData(0x0F, 0x01, new byte[] { 0xD9, 0x1F })]
        //// Phase2Period
        //[InlineData(0x01, 0x0F, new byte[] { 0xD9, 0xF1 })]
        //[InlineData(0x0B, 0x06, new byte[] { 0xD9, 0x6B })]
        public void Get_Bytes(byte phase1Period, byte phase2Period, byte[] expectedBytes)
        {
            SetPreChargePeriod setPreChargePeriod = new SetPreChargePeriod(phase1Period, phase2Period);
            byte[] actualBytes = setPreChargePeriod.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }

        [TestMethod]
        public void Invalid_LowerColumnStartAddressSetPreChargePeriodTests()
        {
            Invalid_LowerColumnStartAddress(0x00, 0x01);
            Invalid_LowerColumnStartAddress(0xFF, 0x01);
            // Phase2Period
            Invalid_LowerColumnStartAddress(0x01, 0x00);
            Invalid_LowerColumnStartAddress(0x01, 0xFF);
            // Phase1Period and Phase2Period
            Invalid_LowerColumnStartAddress(0x00, 0x00);
            Invalid_LowerColumnStartAddress(0x10, 0x10);
        }

        //[Theory]
        //// Phase1Period
        //[InlineData(0x00, 0x01)]
        //[InlineData(0xFF, 0x01)]
        //// Phase2Period
        //[InlineData(0x01, 0x00)]
        //[InlineData(0x01, 0xFF)]
        //// Phase1Period and Phase2Period
        //[InlineData(0x00, 0x00)]
        //[InlineData(0x10, 0x10)]
        public void Invalid_LowerColumnStartAddress(byte phase1Period, byte phase2Period)
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () =>
            {
                SetPreChargePeriod setPreChargePeriod = new SetPreChargePeriod(phase1Period, phase2Period);
            });
        }
    }
}
