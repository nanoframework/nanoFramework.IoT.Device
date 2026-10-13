// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;
using nanoFramework.TestFramework;

namespace Iot.Device.Apds9960.Tests
{
    [TestClass]
    public class Apds9960Tests
    {
        [TestMethod]
        public void ColorDataConvertsAndScalesChannels()
        {
            Color color = Apds9960.ConvertColorData(
                new byte[] { 0x00, 0x94, 0x00, 0x4A, 0x00, 0x25, 0x00, 0x4A },
                1.0,
                1.0,
                1.0);
            Assert.AreEqual((byte)255, color.A);
            Assert.AreEqual((byte)255, color.R);
            Assert.AreEqual((byte)128, color.G);
            Assert.AreEqual((byte)255, color.B);
        }

        [TestMethod]
        public void ColorDataAppliesWhiteBalance()
        {
            Color color = Apds9960.ConvertColorData(
                new byte[] { 0xE8, 0x03, 103, 0, 92, 0, 83, 0 },
                1.0,
                1.119565217391304,
                1.240963855421687);
            Assert.AreEqual((byte)6, color.A);
            Assert.AreEqual((byte)255, color.R);
            Assert.AreEqual((byte)255, color.G);
            Assert.AreEqual((byte)255, color.B);
        }

        [TestMethod]
        public void ColorDataHandlesZeroClearChannel()
        {
            Color color = Apds9960.ConvertColorData(new byte[] { 0, 0, 100, 0, 100, 0, 100, 0 });
            Assert.AreEqual((byte)0, color.A);
            Assert.AreEqual((byte)0, color.R);
            Assert.AreEqual((byte)0, color.G);
            Assert.AreEqual((byte)0, color.B);
        }

        [TestMethod]
        public void GestureDecoderRecognizesDirections()
        {
            Assert.AreEqual(Gesture.Up, GestureDecoder.Decode(new byte[] { 40, 80, 60, 60, 50, 70, 60, 60, 60, 60, 60, 60, 70, 50, 60, 60, 80, 40, 60, 60 }, 5));
            Assert.AreEqual(Gesture.Down, GestureDecoder.Decode(new byte[] { 80, 40, 60, 60, 70, 50, 60, 60, 60, 60, 60, 60, 50, 70, 60, 60, 40, 80, 60, 60 }, 5));
            Assert.AreEqual(Gesture.Left, GestureDecoder.Decode(new byte[] { 60, 60, 40, 80, 60, 60, 50, 70, 60, 60, 60, 60, 60, 60, 70, 50, 60, 60, 80, 40 }, 5));
            Assert.AreEqual(Gesture.Right, GestureDecoder.Decode(new byte[] { 60, 60, 80, 40, 60, 60, 70, 50, 60, 60, 60, 60, 60, 60, 50, 70, 60, 60, 40, 80 }, 5));
        }

        [TestMethod]
        public void GestureDecoderIgnoresWeakAndUnchangedData()
        {
            Assert.AreEqual(Gesture.None, GestureDecoder.Decode(new byte[] { 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 80, 80, 80, 80 }, 5));
            Assert.AreEqual(Gesture.None, GestureDecoder.Decode(new byte[] { 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 65, 55, 65, 55 }, 5));
        }

        [TestMethod]
        public void GestureDecoderIgnoresSaturatedData()
        {
            Assert.AreEqual(
                Gesture.Up,
                GestureDecoder.Decode(
                    new byte[]
                    {
                        80, 40, 60, 60,
                        255, 255, 255, 255,
                        255, 255, 255, 255,
                        40, 80, 60, 60,
                    },
                    4));
        }

        [TestMethod]
        public void GestureDecoderRecognizesCloneScaleMovement()
        {
            Assert.AreEqual(Gesture.Down, GestureDecoder.DecodeDeltas(36, 7));
            Assert.AreEqual(Gesture.Down, GestureDecoder.DecodeDeltas(14, 8));
            Assert.AreEqual(Gesture.Up, GestureDecoder.DecodeDeltas(-15, 8));
            Assert.AreEqual(Gesture.None, GestureDecoder.DecodeDeltas(11, 7));
        }

        [TestMethod]
        public void GestureDecoderUsesSessionEndpointsAcrossSmallBatches()
        {
            byte[] firstBatch = new byte[] { 80, 40, 60, 60 };
            byte[] finalBatch = new byte[] { 40, 80, 60, 60 };
            Assert.AreEqual(Gesture.None, GestureDecoder.Decode(firstBatch, 1));
            Assert.AreEqual(Gesture.None, GestureDecoder.Decode(finalBatch, 1));
            GestureDecoder.TryGetRatios(
                firstBatch,
                1,
                out int initialUpDown,
                out int initialLeftRight,
                out _,
                out _);
            GestureDecoder.TryGetRatios(
                finalBatch,
                1,
                out _,
                out _,
                out int finalUpDown,
                out int finalLeftRight);
            Assert.AreEqual(
                Gesture.Up,
                GestureDecoder.DecodeDeltas(
                    finalUpDown - initialUpDown,
                    finalLeftRight - initialLeftRight));
        }

        [TestMethod]
        public void GestureDecoderRejectsInvalidLength()
        {
            Assert.ThrowsException(typeof(ArgumentException), () => GestureDecoder.Decode(new byte[4], 2));
        }
    }
}
