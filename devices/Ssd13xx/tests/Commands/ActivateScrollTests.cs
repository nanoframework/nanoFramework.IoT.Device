// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ssd13xx.Commands;
using nanoFramework.TestFramework;

namespace Iot.Device.Ssd13xx.Tests
{
    [TestClass]
    public class ActivateScrollTests
    {
        [TestMethod]
        public void Get_BytesActivateScrollTests()
        {
            ActivateScroll activateScroll = new ActivateScroll();
            byte[] actualBytes = activateScroll.GetBytes();
            Assert.Equal(new byte[] { 0x2F }, actualBytes);
        }
    }
}
