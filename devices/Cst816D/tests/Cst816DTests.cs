// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using nanoFramework.TestFramework;
using nanoFramework.UI;

namespace Iot.Device.Cst816D.Tests
{
    [TestClass]
    public class Cst816DTests
    {
        [TestMethod]
        public void NullDeviceIsRejected()
        {
            Assert.ThrowsException(typeof(ArgumentNullException), () => new Cst816D(null));
        }

        [TestMethod]
        public void DecodesFullTwelveBitCoordinates()
        {
            Point point = new Point(new byte[] { 1, 0x0A, 0xBC, 0x0D, 0xEF });
            Assert.IsTrue(point.IsTouched);
            Assert.AreEqual(0xABC, point.X);
            Assert.AreEqual(0xDEF, point.Y);
            Assert.AreEqual((int)TouchEvent.PressDown, (int)point.Event);
        }

        [TestMethod]
        public void MasksStatusAndCoordinateFlags()
        {
            Point point = new Point(new byte[] { 0xF1, 0xB1, 0x23, 0xF4, 0x56 });
            Assert.IsTrue(point.IsTouched);
            Assert.AreEqual(0x123, point.X);
            Assert.AreEqual(0x456, point.Y);
            Assert.AreEqual((int)TouchEvent.Contact, (int)point.Event);
        }

        [TestMethod]
        public void ReleasePreservesLastPositionWithoutReportingContact()
        {
            Point point = new Point(new byte[] { 0, 0x40, 239, 0, 100 });
            Assert.IsFalse(point.IsTouched);
            Assert.AreEqual(239, point.X);
            Assert.AreEqual(100, point.Y);
            Assert.AreEqual((int)TouchEvent.LiftUp, (int)point.Event);
        }

        [TestMethod]
        public void DecodesCoordinateLimits()
        {
            Point origin = new Point(new byte[] { 1, 0, 0, 0, 0 });
            Assert.AreEqual(0, origin.X);
            Assert.AreEqual(0, origin.Y);
            Point maximum = new Point(new byte[] { 1, 0x0F, 0xFF, 0x0F, 0xFF });
            Assert.AreEqual(4095, maximum.X);
            Assert.AreEqual(4095, maximum.Y);
        }

        [TestMethod]
        public void NoTouchAndNoEventAreDecoded()
        {
            Point point = new Point(new byte[] { 0, 0xC0, 0, 0, 0 });
            Assert.IsFalse(point.IsTouched);
            Assert.AreEqual((int)TouchEvent.None, (int)point.Event);
        }

        [TestMethod]
        public void InvalidTouchCountIsRejected()
        {
            Assert.ThrowsException(typeof(InvalidOperationException), () => new Point(new byte[] { 2, 0, 0, 0, 0 }));
            Assert.ThrowsException(typeof(InvalidOperationException), () => new Point(new byte[] { 15, 0, 0, 0, 0 }));
        }

        [TestMethod]
        public void RotatesAsymmetricPointInAllOrientations()
        {
            byte[] report = new byte[] { 1, 0x80, 17, 0, 43 };
            int[] expectedX = new int[] { 17, 222, 276, 43 };
            int[] expectedY = new int[] { 43, 276, 17, 222 };
            for (int i = 0; i < 4; i++)
            {
                Point point = new Point(report, 240, 320, (DisplayOrientation)i);
                Assert.AreEqual(expectedX[i], point.X);
                Assert.AreEqual(expectedY[i], point.Y);
                Assert.IsTrue(point.IsTouched);
                Assert.AreEqual((int)TouchEvent.Contact, (int)point.Event);
            }
        }

        [TestMethod]
        public void RotatesEveryCornerWithinZeroBasedDisplayBounds()
        {
            int[] rawX = new int[] { 0, 239, 239, 0 };
            int[] rawY = new int[] { 0, 0, 319, 319 };
            int[][] expectedX = new int[][]
            {
                new int[] { 0, 239, 239, 0 },
                new int[] { 239, 0, 0, 239 },
                new int[] { 319, 319, 0, 0 },
                new int[] { 0, 0, 319, 319 },
            };
            int[][] expectedY = new int[][]
            {
                new int[] { 0, 0, 319, 319 },
                new int[] { 319, 319, 0, 0 },
                new int[] { 0, 239, 239, 0 },
                new int[] { 239, 0, 0, 239 },
            };
            for (int orientation = 0; orientation < 4; orientation++)
            {
                for (int corner = 0; corner < 4; corner++)
                {
                    byte[] report = new byte[] { 1, (byte)(rawX[corner] >> 8), (byte)rawX[corner], (byte)(rawY[corner] >> 8), (byte)rawY[corner] };
                    Point point = new Point(report, 240, 320, (DisplayOrientation)orientation);
                    Assert.AreEqual(expectedX[orientation][corner], point.X);
                    Assert.AreEqual(expectedY[orientation][corner], point.Y);
                }
            }
        }

        [TestMethod]
        public void RotationPreservesReleaseStateAndEvent()
        {
            Point point = new Point(new byte[] { 0, 0x40, 17, 0, 43 }, 240, 320, DisplayOrientation.Landscape);
            Assert.IsFalse(point.IsTouched);
            Assert.AreEqual((int)TouchEvent.LiftUp, (int)point.Event);
            Assert.AreEqual(276, point.X);
            Assert.AreEqual(17, point.Y);
        }

        [TestMethod]
        public void PortraitWithoutDimensionsPreservesRawCoordinates()
        {
            Point point = new Point(new byte[] { 1, 0x0F, 0xFF, 0x0F, 0xFF }, 0, 0, DisplayOrientation.Portrait);
            Assert.AreEqual(4095, point.X);
            Assert.AreEqual(4095, point.Y);
        }

        [TestMethod]
        public void RotationRequiresDimensions()
        {
            for (int i = 1; i < 4; i++)
            {
                DisplayOrientation orientation = (DisplayOrientation)i;
                Assert.ThrowsException(typeof(InvalidOperationException), () => new Point(new byte[] { 1, 0, 0, 0, 0 }, 0, 320, orientation));
                Assert.ThrowsException(typeof(InvalidOperationException), () => new Point(new byte[] { 1, 0, 0, 0, 0 }, 240, 0, orientation));
            }
        }

        [TestMethod]
        public void InvalidOrientationIsRejected()
        {
            Assert.ThrowsException(typeof(ArgumentOutOfRangeException), () => new Point(new byte[] { 1, 0, 0, 0, 0 }, 240, 320, (DisplayOrientation)4));
            Assert.ThrowsException(typeof(ArgumentOutOfRangeException), () => new Point(new byte[] { 1, 0, 0, 0, 0 }, 0, 0, (DisplayOrientation)(-1)));
        }

        [TestMethod]
        public void SinglePixelPanelRotatesWithoutOffByOneErrors()
        {
            for (int i = 0; i < 4; i++)
            {
                Point point = new Point(new byte[] { 1, 0, 0, 0, 0 }, 1, 1, (DisplayOrientation)i);
                Assert.AreEqual(0, point.X);
                Assert.AreEqual(0, point.Y);
            }
        }
    }
}
