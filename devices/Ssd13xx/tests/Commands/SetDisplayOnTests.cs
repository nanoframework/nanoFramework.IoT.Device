// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ssd13xx.Commands;
using nanoFramework.TestFramework;

namespace Iot.Device.Ssd13xx.Tests
{
    [TestClass]
    public class SetDisplayOnTests
    {
        [TestMethod]
        public void Get_BytesSetDisplayOnTests()
        {
            SetDisplayOn setDisplayOn = new SetDisplayOn();
            byte[] actualBytes = setDisplayOn.GetBytes();
            Assert.Equal(new byte[] { 0xAF }, actualBytes);
        }
    }
}
