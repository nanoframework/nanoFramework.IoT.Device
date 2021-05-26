// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using nanoFramework.TestFramework;

namespace Iot.Device.Ssd13xx.Tests
{
    [TestClass]
    public class SetComOutputScanDirectionTests
    {
        [TestMethod]
        public void Get_Bytes_With_Default_ValuesSetComOutputScanDirectionTests()
        {
            SetComOutputScanDirection setComOutputScanDirection = new SetComOutputScanDirection();
            byte[] actualBytes = setComOutputScanDirection.GetBytes();
            Assert.Equal(new byte[] { 0xC0 }, actualBytes);
        }

        [TestMethod]
        public void Get_BytesSetComOutputScanDirectionTests()
        {
            Get_Bytes(false, new byte[] { 0xC8 });
            Get_Bytes(true, new byte[] { 0xC0 });
        }

        //[Theory]
        //[InlineData(false, new byte[] { 0xC8 })]
        //[InlineData(true, new byte[] { 0xC0 })]
        public void Get_Bytes(bool normalMode, byte[] expectedBytes)
        {
            SetComOutputScanDirection setComOutputScanDirection = new SetComOutputScanDirection(normalMode);
            byte[] actualBytes = setComOutputScanDirection.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}
