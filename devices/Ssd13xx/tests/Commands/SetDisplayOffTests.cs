// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ssd13xx.Commands;
using nanoFramework.TestFramework;

namespace Iot.Device.Ssd13xx.Tests
{
    [TestClass]
    public class SetDisplayOffTests
    {
        [TestMethod]
        public void Get_BytesSetDisplayOffTests()
        {
            SetDisplayOff setDisplayOff = new SetDisplayOff();
            byte[] actualBytes = setDisplayOff.GetBytes();
            Assert.Equal(new byte[] { 0xAE }, actualBytes);
        }
    }
}
