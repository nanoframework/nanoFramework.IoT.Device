// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Multiplexing.Utility;
using nanoFramework.TestFramework;

namespace Iot.Device.Multiplexing
{
    [TestClass]
    public class OutputSegmentTests
    {
        [TestMethod]
        public void SegmentLength()
        {
            VirtualOutputSegment segment = new(2);
            Assert.IsTrue(segment.Length == 2);
        }

        [TestMethod]
        public void SegmentValuesWritePinValues()
        {
            VirtualOutputSegment segment = new(4);
            for (int i = 0; i < 4; i++)
            {
                segment.Write(i, i % 2);
            }

            Assert.IsTrue(
                segment[0] == 0 &&
                segment[1] == 1 &&
                segment[2] == 0 &&
                segment[3] == 1);
        }

        [TestMethod]
        public void SegmentValuesWriteByte()
        {
            VirtualOutputSegment segment = new(8);
            segment.Write(0b_1001_0110);

            Assert.IsTrue(
                segment[0] == 0 &&
                segment[1] == 1 &&
                segment[2] == 1 &&
                segment[3] == 0 &&
                segment[4] == 1 &&
                segment[5] == 0 &&
                segment[6] == 0 &&
                segment[7] == 1);
        }

        [TestMethod]
        public void SegmentValuesWriteLongByte()
        {
            VirtualOutputSegment segment = new(16);
            segment.Write(new byte[] { 0b_1101_0110, 0b_1111_0010 });
            for(int i=0; i<16;i++)
            {
                Debug.WriteLine($"{segment[i]}");
            }


            Assert.IsTrue(
                segment[0] == 0 &&
                segment[1] == 1 &&
                segment[2] == 0 &&
                segment[3] == 0 &&
                segment[4] == 1 &&
                segment[5] == 1 &&
                segment[6] == 1 &&
                segment[7] == 1 &&
                segment[8] == 0 &&
                segment[9] == 1 &&
                segment[10] == 1 &&
                segment[11] == 0 &&
                segment[12] == 1 &&
                segment[13] == 0 &&
                segment[14] == 1 &&
                segment[15] == 1);
        }

        [TestMethod]
        public void SegmentValuesClear()
        {
            VirtualOutputSegment segment = new(8);
            segment.Write(255);
            Assert.IsTrue(segment[3] == 1);
            segment.TurnOffAll();

            for (int i = 0; i < segment.Length; i++)
            {
                Assert.IsTrue(segment[i] == 0);
            }
        }
    }
}
