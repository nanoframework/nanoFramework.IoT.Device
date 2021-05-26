// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using nanoFramework.TestFramework;

namespace Iot.Device.Ssd13xx.Tests
{
    [TestClass]
    public class SetSegmentReMapTests
    {
        [TestMethod]
        public void Get_Bytes_With_Default_ValuesSetSegmentReMapTests()
        {
            SetSegmentReMap setSegmentReMap = new SetSegmentReMap();
            byte[] actualBytes = setSegmentReMap.GetBytes();
            Assert.Equal(new byte[] { 0xA0 }, actualBytes);
        }

        [TestMethod]
        public void Get_BytesSetSegmentReMapTests()
        {
            Get_Bytes(false, new byte[] { 0xA0 });
            Get_Bytes(true, new byte[] { 0xA1 });
        }

        //[Theory]
        //[InlineData(false, new byte[] { 0xA0 })]
        //[InlineData(true, new byte[] { 0xA1 })]
        public void Get_Bytes(bool columnAddress127, byte[] expectedBytes)
        {
            SetSegmentReMap setSegmentReMap = new SetSegmentReMap(columnAddress127);
            byte[] actualBytes = setSegmentReMap.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}
