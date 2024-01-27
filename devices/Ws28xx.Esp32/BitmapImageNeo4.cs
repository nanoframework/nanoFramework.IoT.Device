// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;

namespace Iot.Device.Ws28xx.Esp32
{
    /// <summary>
    /// BitmapImage Neo4.
    /// </summary>
    public class BitmapImageNeo4 : BitmapImage
    {
        /// <summary>
        /// The number of bytes per component.
        /// </summary>
        private const int BytesPerComponent = 3;

        /// <summary>
        /// The number of bytes per pixel.
        /// </summary>
        private const int BytesPerPixel = BytesPerComponent * 4;

        // This field defines the count within the lookup table. The length correlates to the possible values of a single byte.
        private const int LookupCount = 256;

        private static readonly byte[] Lookup = new byte[LookupCount * BytesPerComponent];

        static BitmapImageNeo4()
        {
            ClearInternal();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BitmapImageNeo4"/> class.
        /// </summary>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        public BitmapImageNeo4(int width, int height)
                            : base(new byte[width * height * BytesPerPixel], width, height, width * BytesPerPixel)
        {
        }

        /// <inheritdoc />
        public override void Clear() => ClearInternal();

        private static void ClearInternal()
        {
            for (int i = 0; i < LookupCount; i++)
            {
                int data = 0;
                for (int j = 7; j >= 0; j--)
                {
                    data = (data << 3) | 0b100 | ((i >> j) << 1) & 2;
                }

                Lookup[(i * BytesPerComponent) + 0] = unchecked((byte)(data >> 16));
                Lookup[(i * BytesPerComponent) + 1] = unchecked((byte)(data >> 8));
                Lookup[(i * BytesPerComponent) + 2] = unchecked((byte)(data >> 0));
            }
        }

        /// <inheritdoc />
        public override void Clear(int x, int y) => SetPixel(x, y, Color.Black);

        /// <inheritdoc />
        public override void SetPixel(int x, int y, Color c)
        {
            // Alpha is used as white.
            var offset = (y * Stride) + (x * BytesPerPixel);
            Data[offset++] = Lookup[(c.G * BytesPerComponent) + 0];
            Data[offset++] = Lookup[(c.G * BytesPerComponent) + 1];
            Data[offset++] = Lookup[(c.G * BytesPerComponent) + 2];
            Data[offset++] = Lookup[(c.R * BytesPerComponent) + 0];
            Data[offset++] = Lookup[(c.R * BytesPerComponent) + 1];
            Data[offset++] = Lookup[(c.R * BytesPerComponent) + 2];
            Data[offset++] = Lookup[(c.B * BytesPerComponent) + 0];
            Data[offset++] = Lookup[(c.B * BytesPerComponent) + 1];
            Data[offset++] = Lookup[(c.B * BytesPerComponent) + 2];
            Data[offset++] = Lookup[(c.A * BytesPerComponent) + 0];
            Data[offset++] = Lookup[(c.A * BytesPerComponent) + 1];
            Data[offset++] = Lookup[(c.A * BytesPerComponent) + 2];
        }

        /// <inheritdoc />
        public override void SetPixel(int x, int y, byte r, byte g, byte b) => SetPixel(x, y, Color.FromArgb(r, g, b));
    }
}
