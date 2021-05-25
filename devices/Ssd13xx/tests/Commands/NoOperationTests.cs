// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using nanoFramework.TestFramework;

namespace Iot.Device.Ssd13xx.Tests
{
    [TestClass]
    public class NoOperationTests
    {
        [TestMethod]
        public void Get_BytesNoOperationTests()
        {
            NoOperation noOperation = new NoOperation();
            byte[] actualBytes = noOperation.GetBytes();
            Assert.Equal(new byte[] { 0xE3 }, actualBytes);
        }
    }
}
