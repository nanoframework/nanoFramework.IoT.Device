// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System;
using System.Drawing;
using Iot.Device.EPaper.Buffers;
using Iot.Device.EPaper.Drivers;
using Iot.Device.EPaper.Enums;
using Iot.Device.EPaper.Fonts;
using nanoFramework.UI;

namespace Iot.Device.EPaper
{
    /// <summary>
    /// A graphics class for ePaper displays with basic graphic APIs support.
    /// </summary>
    public sealed class Graphics : IDisposable
    {
        private readonly bool _disposeDisplay;
        private bool _disposedValue;

        /// <summary>
        /// Gets the E-Paper display being controlled by this <see cref="Graphics"/> class instance.
        /// </summary>
        public IEPaperDisplay EPaperDisplay { get; }

        /// <summary>
        /// Gets or sets the current display orientation.
        /// </summary>
        /// <see cref="Rotation"/>
        public Rotation DisplayRotation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether glyph rows are rendered mirrored horizontally.
        /// </summary>
        public bool FlipGlyphsHorizontally { get; set; }

        /// <summary>
        /// Gets the logical drawing width for the current rotation.
        /// </summary>
        public int Width =>
            DisplayRotation == Rotation.Degrees90Clockwise || DisplayRotation == Rotation.Degrees270Clockwise
                ? EPaperDisplay.Height
                : EPaperDisplay.Width;

        /// <summary>
        /// Gets the logical drawing height for the current rotation.
        /// </summary>
        public int Height =>
            DisplayRotation == Rotation.Degrees90Clockwise || DisplayRotation == Rotation.Degrees270Clockwise
                ? EPaperDisplay.Width
                : EPaperDisplay.Height;

        /// <summary>
        /// Initializes a new instance of the <see cref="Graphics"/> class.
        /// </summary>
        /// <param name="ePaperDisplay">The E-Paper display device to draw to.</param>
        /// <param name="disposeDisplay">
        /// When <see langword="true"/>, <see cref="Dispose"/> also disposes <paramref name="ePaperDisplay"/>.
        /// Pass <see langword="false"/> when the display lifetime is managed elsewhere (for example a <c>using</c> on the driver).
        /// </param>
        public Graphics(IEPaperDisplay ePaperDisplay, bool disposeDisplay = true)
        {
            EPaperDisplay = ePaperDisplay ?? throw new ArgumentNullException(nameof(ePaperDisplay));
            _disposeDisplay = disposeDisplay;
            DisplayRotation = Rotation.Default;
            FlipGlyphsHorizontally = false;
        }

        /// <summary>
        /// Draws a line from the a starting point to an end point.
        /// </summary>
        /// <param name="startX">X position of the start point.</param>
        /// <param name="startY">Y position of the start point.</param>
        /// <param name="endX">X position of the end point.</param>
        /// <param name="endY">Y position of the end point.</param>
        /// <param name="color">The color of the line.</param>
        public void DrawLine(int startX, int startY, int endX, int endY, Color color)
        {
            int sx = (startX < endX) ? 1 : -1;
            int sy = (startY < endY) ? 1 : -1;

            int dx = endX > startX ? endX - startX : startX - endX;
            int dy = endY > startY ? endY - startY : startY - endY;

            int err = dx - dy;

            while (true)
            {
                DrawPixel(startX, startY, color);

                if (startX == endX && startY == endY)
                {
                    break;
                }

                int e2 = 2 * err;

                if (e2 > -dy)
                {
                    err -= dy;
                    startX += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    startY += sy;
                }
            }
        }

        /// <summary>
        /// Draws a circle defined by the specified center point and radius.
        /// </summary>
        /// <param name="centerX">The X coordinate of the circle center.</param>
        /// <param name="centerY">The Y coordinate of the circle center.</param>
        /// <param name="radius">The radius of the circle in pixels.</param>
        /// <param name="color">The color used to draw the circle.</param>
        /// <param name="fill"><see langword="true"/> to fill the circle; otherwise, only the outline is drawn.</param>
        public void DrawCircle(int centerX, int centerY, int radius, Color color, bool fill)
        {
            if (fill)
            {
                DrawCircleFilled(centerX, centerY, radius, color);
            }
            else
            {
                DrawCircleOutline(centerX, centerY, radius, color);
            }
        }

        /// <summary>
        /// Draws a rectangle defined by a starting point, width, and height.
        /// </summary>
        /// <param name="startX">The X coordinate of the rectangle's top-left corner.</param>
        /// <param name="startY">The Y coordinate of the rectangle's top-left corner.</param>
        /// <param name="width">The width of the rectangle in pixels.</param>
        /// <param name="height">The height of the rectangle in pixels.</param>
        /// <param name="color">The color used to draw the rectangle.</param>
        /// <param name="fill"><see langword="true"/> to fill the rectangle; otherwise, only the outline is drawn.</param>
        public void DrawRectangle(int startX, int startY, int width, int height, Color color, bool fill)
        {
            int endX = startX + width;
            int endY = startY + height;

            if (fill)
            {
                DrawRectangleFilled(startX, startY, endX, endY, color);
            }
            else
            {
                DrawRectangleOutline(startX, startY, endX, endY, color);
            }
        }

        /// <summary>
        /// Writes text to the display.
        /// </summary>
        /// <param name="text">The text to draw.</param>
        /// <param name="font">The font used to render the text.</param>
        /// <param name="x">The X coordinate of the text starting position.</param>
        /// <param name="y">The Y coordinate of the text starting position.</param>
        /// <param name="color">The color used to draw the text.</param>
        public void DrawText(string text, IFont font, int x, int y, Color color)
        {
            int col = 0;
            int line = 0;

            foreach (char character in text)
            {
                if (y + line + font.Height > Height)
                {
                    break;
                }

                if (x + col + font.Width > Width)
                {
                    col = 0;
                    line += font.Height + 1;

                    if (y + line + font.Height > Height)
                    {
                        break;
                    }
                }

                var characterBitmap = font[character];
                for (int i = 0; i < font.Height; i++)
                {
                    int xPos = x + col;
                    int yPos = y + line + i;
                    int bitMask = 0x80;
                    byte b = characterBitmap[i];

                    for (int pixel = 0; pixel < 8; pixel++)
                    {
                        if ((b & bitMask) != 0)
                        {
                            int drawX = FlipGlyphsHorizontally ? xPos + (7 - pixel) : xPos + pixel;
                            DrawPixel(drawX, yPos, color);
                        }

                        bitMask >>= 1;
                    }
                }

                col += font.Width;
            }
        }

        /// <summary>
        /// Draws the specified bitmap buffer to the display using the specified starting point.
        /// </summary>
        /// <param name="bitmap">The bitmap buffer to draw.</param>
        /// <param name="start">The starting position on the display where the bitmap will be drawn.</param>
        /// <param name="rotate"><see langword="true"/> to rotate the bitmap using the current <see cref="DisplayRotation"/>; otherwise, the bitmap is drawn without additional rotation.</param>
        public void DrawBitmap(IFrameBuffer bitmap, System.Drawing.Point start, bool rotate = false)
        {
            if (!rotate || DisplayRotation == Rotation.Default)
            {
                EPaperDisplay.FrameBuffer.WriteBuffer(bitmap, destinationStart: start);
                return;
            }

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    DrawPixel(start.X + x, start.Y + y, bitmap.GetPixel(new System.Drawing.Point(x, y)));
                }
            }
        }

        /// <summary>
        /// Gets the real position of a point after applying the current <see cref="Rotation"/>.
        /// </summary>
        /// <param name="x">The logical X coordinate.</param>
        /// <param name="y">The logical Y coordinate.</param>
        /// <returns>The transformed position on the display.</returns>
        public System.Drawing.Point GetRealPosition(int x, int y)
        {
            switch (DisplayRotation)
            {
                case Rotation.Degrees90Clockwise:
                    return new System.Drawing.Point(EPaperDisplay.Width - y - 1, x);

                case Rotation.Degrees180Clockwise:
                    return new System.Drawing.Point(EPaperDisplay.Width - x - 1, EPaperDisplay.Height - y - 1);

                case Rotation.Degrees270Clockwise:
                    return new System.Drawing.Point(y, EPaperDisplay.Height - x - 1);

                default:
                    return new System.Drawing.Point(x, y);
            }
        }

        private void DrawRectangleOutline(int startX, int startY, int endX, int endY, Color color)
        {
            endX -= 1;
            endY -= 1;

            for (int currentX = startX; currentX != endX; currentX++)
            {
                DrawPixel(currentX, startY, color);
            }

            for (int currentX = startX; currentX <= endX; currentX++)
            {
                DrawPixel(currentX, endY, color);
            }

            for (int currentY = startY; currentY != endY; currentY++)
            {
                DrawPixel(startX, currentY, color);
            }

            for (int currentY = startY; currentY <= endY; currentY++)
            {
                DrawPixel(endX, currentY, color);
            }
        }

        private void DrawRectangleFilled(int startX, int startY, int endX, int endY, Color color)
        {
            for (int currentY = startY; currentY != endY; currentY++)
            {
                for (int xx = startX; xx != endX; xx++)
                {
                    DrawPixel(xx, currentY, color);
                }
            }
        }

        private void DrawCircleOutline(int centerX, int centerY, int radius, Color color)
        {
            void DrawCirclePoints(int xc, int yc, int x1, int y1, Color pixelColor)
            {
                DrawPixel(xc + x1, yc + y1, pixelColor);
                DrawPixel(xc - x1, yc + y1, pixelColor);
                DrawPixel(xc + x1, yc - y1, pixelColor);
                DrawPixel(xc - x1, yc - y1, pixelColor);
                DrawPixel(xc + y1, yc + x1, pixelColor);
                DrawPixel(xc - y1, yc + x1, pixelColor);
                DrawPixel(xc + y1, yc - x1, pixelColor);
                DrawPixel(xc - y1, yc - x1, pixelColor);
            }

            int x1 = 0;
            int y1 = radius;
            int determinant = 3 - (2 * radius);

            DrawCirclePoints(centerX, centerY, x1, y1, color);

            while (y1 >= x1)
            {
                x1++;

                if (determinant > 0)
                {
                    y1--;
                    determinant = determinant + (4 * (x1 - y1)) + 10;
                }
                else
                {
                    determinant = determinant + (4 * x1) + 6;
                }

                DrawCirclePoints(centerX, centerY, x1, y1, color);
            }
        }

        private void DrawCircleFilled(int centerX, int centerY, int radius, Color color)
        {
            int x1 = 0;
            int y1 = radius;
            int determinant = 3 - (2 * radius);

            while (x1 <= y1)
            {
                DrawLine(centerX + x1, centerY + y1, centerX - x1, centerY + y1, color);
                DrawLine(centerX + x1, centerY - y1, centerX - x1, centerY - y1, color);
                DrawLine(centerX - y1, centerY + x1, centerX + y1, centerY + x1, color);
                DrawLine(centerX - y1, centerY - x1, centerX + y1, centerY - x1, color);

                if (determinant < 0)
                {
                    determinant += (2 * x1) + 1;
                }
                else
                {
                    determinant += (2 * (x1 - y1)) + 1;
                    y1--;
                }

                x1++;
            }
        }

        /// <summary>
        /// Draws a pixel on the display with respect to the current <see cref="DisplayRotation"/>.
        /// </summary>
        /// <param name="x">The logical X coordinate of the pixel.</param>
        /// <param name="y">The logical Y coordinate of the pixel.</param>
        /// <param name="color">The color used to draw the pixel.</param>
        public void DrawPixel(int x, int y, Color color)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return;
            }

            System.Drawing.Point point = GetRealPosition(x, y);

            if (point.X < 0 || point.X >= EPaperDisplay.Width || point.Y < 0 || point.Y >= EPaperDisplay.Height)
            {
                return;
            }

            EPaperDisplay.DrawPixel(point.X, point.Y, color);
        }

        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing && _disposeDisplay)
                {
                    EPaperDisplay?.Dispose();
                }

                _disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
