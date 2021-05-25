// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using nanoFramework.TestFramework;

namespace Iot.Device.Ssd13xx.Tests
{
    [TestClass]
    public class SetPageAddressTests
    {
        [TestMethod]
        public void Get_Bytes_With_Default_ValuesSetPageAddressTests()
        {
            SetPageAddress setPageAddress = new SetPageAddress();
            byte[] actualBytes = setPageAddress.GetBytes();
            Assert.Equal(new byte[] { 0x22, 0x00, 0x07 }, actualBytes);
        }

        [TestMethod]
        public void Get_BytesSetPageAddressTests()
        {
            Get_Bytes(PageAddress.Page0, PageAddress.Page0, new byte[] { 0x22, 0x00, 0x00 });
            Get_Bytes(PageAddress.Page1, PageAddress.Page0, new byte[] { 0x22, 0x01, 0x00 });
            Get_Bytes(PageAddress.Page2, PageAddress.Page0, new byte[] { 0x22, 0x02, 0x00 });
            Get_Bytes(PageAddress.Page3, PageAddress.Page0, new byte[] { 0x22, 0x03, 0x00 });
            Get_Bytes(PageAddress.Page4, PageAddress.Page0, new byte[] { 0x22, 0x04, 0x00 });
            Get_Bytes(PageAddress.Page5, PageAddress.Page0, new byte[] { 0x22, 0x05, 0x00 });
            Get_Bytes(PageAddress.Page6, PageAddress.Page0, new byte[] { 0x22, 0x06, 0x00 });
            Get_Bytes(PageAddress.Page7, PageAddress.Page0, new byte[] { 0x22, 0x07, 0x00 });
            // EndAddress                                                                   
            Get_Bytes(PageAddress.Page0, PageAddress.Page1, new byte[] { 0x22, 0x00, 0x01 });
            Get_Bytes(PageAddress.Page0, PageAddress.Page2, new byte[] { 0x22, 0x00, 0x02 });
            Get_Bytes(PageAddress.Page0, PageAddress.Page3, new byte[] { 0x22, 0x00, 0x03 });
            Get_Bytes(PageAddress.Page0, PageAddress.Page4, new byte[] { 0x22, 0x00, 0x04 });
            Get_Bytes(PageAddress.Page0, PageAddress.Page5, new byte[] { 0x22, 0x00, 0x05 });
            Get_Bytes(PageAddress.Page0, PageAddress.Page6, new byte[] { 0x22, 0x00, 0x06 });
            Get_Bytes(PageAddress.Page0, PageAddress.Page7, new byte[] { 0x22, 0x00, 0x07 });
        }

        //[Theory]
        //// StartAddress
        //[InlineData(PageAddress.Page0, PageAddress.Page0, new byte[] { 0x22, 0x00, 0x00 })]
        //[InlineData(PageAddress.Page1, PageAddress.Page0, new byte[] { 0x22, 0x01, 0x00 })]
        //[InlineData(PageAddress.Page2, PageAddress.Page0, new byte[] { 0x22, 0x02, 0x00 })]
        //[InlineData(PageAddress.Page3, PageAddress.Page0, new byte[] { 0x22, 0x03, 0x00 })]
        //[InlineData(PageAddress.Page4, PageAddress.Page0, new byte[] { 0x22, 0x04, 0x00 })]
        //[InlineData(PageAddress.Page5, PageAddress.Page0, new byte[] { 0x22, 0x05, 0x00 })]
        //[InlineData(PageAddress.Page6, PageAddress.Page0, new byte[] { 0x22, 0x06, 0x00 })]
        //[InlineData(PageAddress.Page7, PageAddress.Page0, new byte[] { 0x22, 0x07, 0x00 })]
        //// EndAddress
        //[InlineData(PageAddress.Page0, PageAddress.Page1, new byte[] { 0x22, 0x00, 0x01 })]
        //[InlineData(PageAddress.Page0, PageAddress.Page2, new byte[] { 0x22, 0x00, 0x02 })]
        //[InlineData(PageAddress.Page0, PageAddress.Page3, new byte[] { 0x22, 0x00, 0x03 })]
        //[InlineData(PageAddress.Page0, PageAddress.Page4, new byte[] { 0x22, 0x00, 0x04 })]
        //[InlineData(PageAddress.Page0, PageAddress.Page5, new byte[] { 0x22, 0x00, 0x05 })]
        //[InlineData(PageAddress.Page0, PageAddress.Page6, new byte[] { 0x22, 0x00, 0x06 })]
        //[InlineData(PageAddress.Page0, PageAddress.Page7, new byte[] { 0x22, 0x00, 0x07 })]
        public void Get_Bytes(PageAddress startAddress, PageAddress endAddress, byte[] expectedBytes)
        {
            SetPageAddress setPageAddress = new SetPageAddress(startAddress, endAddress);
            byte[] actualBytes = setPageAddress.GetBytes();
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}
