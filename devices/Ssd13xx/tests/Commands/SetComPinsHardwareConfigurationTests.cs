// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using nanoFramework.TestFramework;

namespace Iot.Device.Ssd13xx.Tests
{
    [TestClass]
    public class SetComPinsHardwareConfigurationTests
    {
        [TestMethod]
        public void Get_Bytes_With_Default_ValuesSetComPinsHardwareConfigurationTests()
        {
            SetComPinsHardwareConfiguration setComPinsHardwareConfiguration = new SetComPinsHardwareConfiguration();
            byte[] actualBytes = setComPinsHardwareConfiguration.GetBytes();
            Assert.Equal(new byte[] { 0xDA, 0x12 }, actualBytes);
        }

        [TestMethod]
        public void Get_BytesSetComPinsHardwareConfigurationTests()
        {
            Get_Bytes(false, false, new byte[] { 0xDA, 0x02 });
            Get_Bytes(true, false, new byte[] { 0xDA, 0x12 });
            // EnableLeftRightRemap
            Get_Bytes(false, true, new byte[] { 0xDA, 0x22 });
        }

        //[Theory]
        //// AlternativeComPinConfiguration
        //[InlineData(false, false, new byte[] { 0xDA, 0x02 })]
        //[InlineData(true, false, new byte[] { 0xDA, 0x12 })]
        //// EnableLeftRightRemap
        //[InlineData(false, true, new byte[] { 0xDA, 0x22 })]
        public void Get_Bytes(bool alternativeComPinConfiguration, bool enableLeftRightRemap, byte[] expectedBytes)
        {
            SetComPinsHardwareConfiguration setComPinsHardwareConfiguration = new SetComPinsHardwareConfiguration(alternativeComPinConfiguration, enableLeftRightRemap);
            byte[] actualBytes = setComPinsHardwareConfiguration.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}
