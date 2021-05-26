// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using nanoFramework.TestFramework;

namespace Iot.Device.Ssd13xx.Tests
{
    [TestClass]
    public class EntireDisplayOnTests
    {
        [TestMethod]
        public void Get_BytesEntireDisplayOnTests()
        {
            Get_Bytes(false, new byte[] { 0xA4 });
            Get_Bytes(true, new byte[] { 0xA5 });
        }

        //[Theory]
        //[InlineData(false, new byte[] { 0xA4 })]
        //[InlineData(true, new byte[] { 0xA5 })]
        public void Get_Bytes(bool entireDisplay, byte[] expectedBytes)
        {
            EntireDisplayOn entireDisplayOn = new EntireDisplayOn(entireDisplay);
            byte[] actualBytes = entireDisplayOn.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}
