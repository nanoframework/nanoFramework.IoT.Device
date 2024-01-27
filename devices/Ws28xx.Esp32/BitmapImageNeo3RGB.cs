// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;

namespace Iot.Device.Ws28xx.Esp32
{
    /// <summary>
    /// Special 24bit RGB format for Neo pixel LEDs where each bit is converted to 3 bits.
    /// A one is converted to 110, a zero is converted to 100.
    /// </summary>
    /// <seealso cref="Iot.Device.Ws28xx.BitmapImageNeo3" />
    public class BitmapImageNeo3Rgb : BitmapImageNeo3
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BitmapImageNeo3Rgb"/> class.
        /// </summary>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        public BitmapImageNeo3Rgb(int width, int height)
            : base(width, height)
        {
        }

        /// <inheritdoc />
        public override void SetPixel(int x, int y, Color c)
        {
            var offset = (y * Stride) + (x * BytesPerPixel);
            Data[offset++] = Lookup[(c.R * BytesPerComponent) + 0];
            Data[offset++] = Lookup[(c.R * BytesPerComponent) + 1];
            Data[offset++] = Lookup[(c.R * BytesPerComponent) + 2];
            Data[offset++] = Lookup[(c.G * BytesPerComponent) + 0];
            Data[offset++] = Lookup[(c.G * BytesPerComponent) + 1];
            Data[offset++] = Lookup[(c.G * BytesPerComponent) + 2];
            Data[offset++] = Lookup[(c.B * BytesPerComponent) + 0];
            Data[offset++] = Lookup[(c.B * BytesPerComponent) + 1];
            Data[offset++] = Lookup[(c.B * BytesPerComponent) + 2];
        }
    }
}
