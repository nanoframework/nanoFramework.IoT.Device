// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using nanoFramework.TestFramework;

namespace Iot.Device.Max30100.Tests
{
    [TestClass]
    public class Max30100DataConverterTests
    {
        [TestMethod]
        public void ConvertFifoSampleReturnsInfraredAndRedValues()
        {
            Max30100Reading reading = Max30100DataConverter.ConvertFifoSample(new byte[] { 0x12, 0x34, 0xAB, 0xCD });
            Assert.AreEqual((ushort)0x1234, reading.Infrared);
            Assert.AreEqual((ushort)0xABCD, reading.Red);
        }

        [TestMethod]
        public void ConvertFifoSampleRejectsWrongLength()
        {
            Assert.ThrowsException(typeof(ArgumentException), () => Max30100DataConverter.ConvertFifoSample(new byte[3]));
        }

        [TestMethod]
        public void ConvertTemperatureReturnsPositiveValue()
        {
            Assert.AreEqual(25.5, Max30100DataConverter.ConvertTemperature(25, 0x08));
        }

        [TestMethod]
        public void ConvertTemperatureReturnsNegativeValue()
        {
            Assert.AreEqual(-4.75, Max30100DataConverter.ConvertTemperature(0xFB, 0x04));
        }

        [TestMethod]
        public void CalculateUnreadSampleCountRecognizesFullFifo()
        {
            Assert.AreEqual((byte)16, Max30100DataConverter.CalculateUnreadSampleCount(5, 5, 1));
            Assert.AreEqual((byte)0, Max30100DataConverter.CalculateUnreadSampleCount(5, 5, 0));
            Assert.AreEqual((byte)4, Max30100DataConverter.CalculateUnreadSampleCount(9, 5, 0));
        }
    }
}
