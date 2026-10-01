// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Pmsx003.Shared;
using nanoFramework.TestFramework;

namespace Iot.Device.Pms5003.Tests
{
    [TestClass]
    public class Pmsx003ParserTests
    {
        [TestMethod]
        public void ParseReturnsAllMeasurementValues()
        {
            byte[] frame = CreateFrame();
            Pmsx003Reading reading = Pmsx003Parser.Parse(frame);

            Assert.AreEqual((ushort)1, reading.Pm1Standard);
            Assert.AreEqual((ushort)2, reading.Pm2Point5Standard);
            Assert.AreEqual((ushort)3, reading.Pm10Standard);
            Assert.AreEqual((ushort)4, reading.Pm1Atmospheric);
            Assert.AreEqual((ushort)5, reading.Pm2Point5Atmospheric);
            Assert.AreEqual((ushort)6, reading.Pm10Atmospheric);
            Assert.AreEqual((ushort)7, reading.ParticlesLargerThan0Point3Micrometers);
            Assert.AreEqual((ushort)8, reading.ParticlesLargerThan0Point5Micrometers);
            Assert.AreEqual((ushort)9, reading.ParticlesLargerThan1Micrometer);
            Assert.AreEqual((ushort)10, reading.ParticlesLargerThan2Point5Micrometers);
            Assert.AreEqual((ushort)11, reading.ParticlesLargerThan5Micrometers);
            Assert.AreEqual((ushort)12, reading.ParticlesLargerThan10Micrometers);
            Assert.AreEqual((byte)13, reading.Version);
            Assert.AreEqual((byte)14, reading.ErrorCode);
        }

        [TestMethod]
        public void ParseRejectsInvalidChecksum()
        {
            byte[] frame = CreateFrame();
            frame[30] = 0;
            frame[31] = 0;
            Assert.ThrowsException(typeof(InvalidOperationException), () => Pmsx003Parser.Parse(frame));
        }

        [TestMethod]
        public void ParseRejectsInvalidHeader()
        {
            byte[] frame = CreateFrame();
            frame[0] = 0;
            Assert.ThrowsException(typeof(InvalidOperationException), () => Pmsx003Parser.Parse(frame));
        }

        [TestMethod]
        public void ParseRejectsInvalidFrameLengthField()
        {
            byte[] frame = CreateFrame();
            frame[3] = 27;
            Assert.ThrowsException(typeof(InvalidOperationException), () => Pmsx003Parser.Parse(frame));
        }

        [TestMethod]
        public void ParseRejectsWrongBufferLength()
        {
            Assert.ThrowsException(typeof(ArgumentException), () => Pmsx003Parser.Parse(new byte[31]));
        }

        private static byte[] CreateFrame()
        {
            byte[] frame = new byte[32];
            frame[0] = 0x42;
            frame[1] = 0x4D;
            frame[3] = 28;
            for (int value = 1; value <= 12; value++)
            {
                frame[4 + ((value - 1) * 2) + 1] = (byte)value;
            }

            frame[28] = 13;
            frame[29] = 14;
            ushort checksum = 0;
            for (int index = 0; index < 30; index++)
            {
                checksum += frame[index];
            }

            frame[30] = (byte)(checksum >> 8);
            frame[31] = (byte)checksum;
            return frame;
        }
    }
}