// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using nanoFramework.UI;

namespace Iot.Device.Cst816D
{
    /// <summary>
    /// A single-point touch report from the CST816D.
    /// </summary>
    public struct Point
    {
        internal Point(byte[] report)
        {
            int count = report[0] & 0x0F;
            if (count > 1)
            {
                throw new InvalidOperationException();
            }

            IsTouched = count == 1;
            X = ((report[1] & 0x0F) << 8) | report[2];
            Y = ((report[3] & 0x0F) << 8) | report[4];
            Event = (TouchEvent)((report[1] >> 6) & 0x03);
        }

        internal Point(byte[] report, int width, int height, DisplayOrientation orientation)
            : this(report)
        {
            if (orientation == DisplayOrientation.Portrait)
            {
                return;
            }

            if (orientation != DisplayOrientation.Portrait180 &&
                orientation != DisplayOrientation.Landscape &&
                orientation != DisplayOrientation.Landscape180)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (width <= 0 || height <= 0)
            {
                throw new InvalidOperationException();
            }

            int rawX = X;
            int rawY = Y;
            switch (orientation)
            {
                case DisplayOrientation.Portrait180:
                    X = width - 1 - rawX;
                    Y = height - 1 - rawY;
                    break;
                case DisplayOrientation.Landscape:
                    X = height - 1 - rawY;
                    Y = rawX;
                    break;
                case DisplayOrientation.Landscape180:
                    X = rawY;
                    Y = width - 1 - rawX;
                    break;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a finger is currently touching the panel.
        /// </summary>
        public bool IsTouched { get; }

        /// <summary>
        /// Gets the X coordinate in the configured display orientation. When not touched, this may be the last reported position.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Gets the Y coordinate in the configured display orientation. When not touched, this may be the last reported position.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Gets the event encoded in the coordinate report.
        /// </summary>
        public TouchEvent Event { get; }
    }
}
