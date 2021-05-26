// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ssd13xx.Commands;
using nanoFramework.TestFramework;

namespace Iot.Device.Ssd13xx.Tests
{
    [TestClass]
    public class SetContrastControlForBank0Tests
    {
        [TestMethod]
        public void Get_Bytes_With_Default_ValuesSetContrastControlForBank0Tests()
        {
            SetContrastControlForBank0 setContrastControlForBank0 = new SetContrastControlForBank0();
            byte[] actualBytes = setContrastControlForBank0.GetBytes();
            Assert.Equal(new byte[] { 0x81, 0x7F }, actualBytes);
        }

        [TestMethod]
        public void Get_BytesSetContrastControlForBank0Tests()
        {
            Get_Bytes(0x00, new byte[] { 0x81, 0x00 });
            Get_Bytes(0xFF, new byte[] { 0x81, 0xFF });

        }

        //[Theory]
        //[InlineData(0x00, new byte[] { 0x81, 0x00 })]
        //[InlineData(0xFF, new byte[] { 0x81, 0xFF })]
        public void Get_Bytes(byte contrastSetting, byte[] expectedBytes)
        {
            SetContrastControlForBank0 setContrastControlForBank0 = new SetContrastControlForBank0(contrastSetting);
            byte[] actualBytes = setContrastControlForBank0.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}
