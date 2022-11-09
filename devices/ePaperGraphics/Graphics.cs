using System;

using Iot.Device.ePaper.Shared.Buffers;
using Iot.Device.ePaper.Shared.Drivers;
using Iot.Device.ePaper.Shared.Fonts;
using Iot.Device.ePaper.Shared.Primitives;

namespace Iot.Device.ePaperGraphics
{
    public sealed class Graphics : IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// Gets the E-Paper display being controlled by this <see cref="Graphics"/> class instance.
        /// </summary>
        public IePaperDisplay ePaperDisplay { get; }

        /// <summary>
        /// Gets or sets the current display orientation.
        /// </summary>
        /// <see cref="Rotation"/>
        public Rotation DisplayRotation { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="Graphics"/> class.
        /// </summary>
        /// <param name="ePaperDisplay">The E-Paper display device to draw to.</param>
        public Graphics(IePaperDisplay ePaperDisplay)
        {
            this.ePaperDisplay = ePaperDisplay;
            this.DisplayRotation = Rotation.Default;
        }

        /// <summary>
        /// Draws a line from the a starting point to an end point.
        /// </summary>
        /// <param name="startX">X position of the start point.</param>
        /// <param name="startY">Y position of the start point.</param>
        /// <param name="endX">X position of the end point.</param>
        /// <param name="endY">Y position of the end point.</param>
        /// <param name="color">The color of the line.</param>
        public void DrawLine(int startX, int startY,
            int endX, int endY, Color color)
        {
            // This is a common line drawing algorithm. Read about it here:
            // http://en.wikipedia.org/wiki/Bresenham's_line_algorithm
            int sx = (startX < endX) ? 1 : -1;
            int sy = (startY < endY) ? 1 : -1;

            int dx = endX > startX ? endX - startX : startX - endX;
            int dy = endY > startX ? endY - startY : startY - endY;

            float err = dx - dy, e2;

            // if there is an error with drawing a point or the line is finished get out of the loop!
            while (!(startX == endX && startY == endY))
            {
                this.DrawPixel(startX, startY, color);

                e2 = 2 * err;

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
        /// <param name="centerX">X position of the center point.</param>
        /// <param name="centerY">Y position of the center point.</param>
        /// <param name="radius">The circle's radius.</param>
        /// <param name="color">The color to use when drawing the circle.</param>
        /// <param name="fill">True to fill the circle, otherwise; draws only the outline.</param>
        public void DrawCircle(int centerX, int centerY,
            int radius, Color color, bool fill)
        {
            if (fill)
                this.DrawCircleFilled(centerX, centerY, radius, color);
            else
                this.DrawCircleOutline(centerX, centerY, radius, color);
        }

        /// <summary>
        /// Draws a rectangle defined by a starting point, width, and height.
        /// </summary>
        /// <param name="startX">Top left point X position.</param>
        /// <param name="startY">Top left point Y position.</param>
        /// <param name="width">The width of the rectangle in pixels.</param>
        /// <param name="height">The height of the rectangle in pixels.</param>
        /// <param name="color">The color to use when drawing the rectangle.</param>
        /// <param name="fill">True to fill the rectangle, otherwise; draws only the outline.</param>
        public void DrawRectangle(int startX, int startY, int width,
            int height, Color color, bool fill)
        {
            // This will draw points
            int endX = startX + width;
            int endY = startY + height;

            if (fill)
                this.DrawRectangleFilled(startX, startY, endX, endY, color);
            else
                this.DrawRectangleOutline(startX, startY, endX, endY, color);
        }

        /// <summary>
        /// Writes text to the display.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="font">The font to use.</param>
        /// <param name="x">Starting X point.</param>
        /// <param name="y">Starting Y point.</param>
        /// <param name="color">The font color.</param>
        public void DrawText(string text, IFont font, int x, int y, Color color)
        {
            var col = 0;
            var line = 0;

            foreach (char character in text)
            {
                if (col == this.ePaperDisplay.Width)
                {
                    col = 0;
                    line += font.Height + 1;
                }

                var characterBitmap = font[character];
                for (var i = 0; i < font.Height; i++)
                {
                    var xPos = x + col;
                    var yPos = y + line + i;
                    var bitMask = 0x01;
                    var b = characterBitmap[i];

                    for (var pixel = 0; pixel < 8; pixel++)
                    {
                        if ((b & bitMask) > 0)
                        {
                            this.DrawPixel(xPos + pixel, yPos, color);
                        }

                        bitMask <<= 1;
                    }
                }

                col += font.Width;
            }
        }

        public void DrawBitmap(IFrameBuffer bitmap, Point start, bool rotate = false)
        {
            if (this.DisplayRotation == Rotation.Default)
            {
                this.ePaperDisplay.FrameBuffer.WriteBuffer(bitmap, destinationStart: start);
                return;
            }

            if (!rotate)
            {
                this.ePaperDisplay.FrameBuffer.WriteBuffer(bitmap, destinationStart: start);
            }
            else // caller opted in to rotate (slow)
            {
                for (var y = 0; y < bitmap.Height; y++)
                {
                    for (var x = 0; x < bitmap.Width; x++)
                    {
                        var currentPoint = new Point(x, y);

                        this.ePaperDisplay
                            .FrameBuffer
                            .SetPixel(start + currentPoint, bitmap.GetPixel(currentPoint));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the real X Position of a given point after considering the current <see cref="Rotation"/> of the display.
        /// </summary>
        /// <param name="x">The X Position in the current rotation.</param>
        /// <param name="y">The Y Position in the current rotation.</param>
        /// <param name="displayWidth"></param>
        /// <returns>The real X position on the display.</returns>
        private int GetRealXPosition(int x, int y)
            => this.DisplayRotation switch
            {
                Rotation.NinetyDegreesClockwise => this.ePaperDisplay.Width - y - 1,
                Rotation.OneEightyDegreesClockwise => this.ePaperDisplay.Width - x - 1,
                Rotation.TwoSeventyDegreesClockwise => y,
                _ => x
            };

        /// <summary>
        /// Gets the real Y Position of a given point after considering the current <see cref="Rotation"/> of the display.
        /// </summary>
        /// <param name="x">The X Position in the current rotation.</param>
        /// <param name="y">The Y Position in the current rotation.</param>
        /// <returns>The real Y position on the display.</returns>
        private int GetRealYPosition(int x, int y)
            => this.DisplayRotation switch
            {
                Rotation.NinetyDegreesClockwise => x,
                Rotation.OneEightyDegreesClockwise => this.ePaperDisplay.Height - y - 1,
                Rotation.TwoSeventyDegreesClockwise => this.ePaperDisplay.Height - x - 1,
                _ => y
            };

        /// <summary>
        /// Gets the real Position of a given point after considering the current <see cref="Rotation"/> of the display.
        /// </summary>
        /// <param name="x">The X Position in the current rotation.</param>
        /// <param name="y">The Y Position in the current rotation.</param>
        /// <returns>The real position on the display.</returns>
        public Point GetRealPosition(int x, int y)
            => new Point(this.GetRealXPosition(x, y), this.GetRealYPosition(x, y));

        private void DrawRectangleOutline(int startX, int startY,
            int endX, int endY, Color color)
        {
            endX -= 1;
            endY -= 1;

            for (int currentX = startX; currentX != endX; currentX++)
            {
                this.DrawPixel(currentX, startY, color);
            }

            for (int currentX = startX; currentX <= endX; currentX++)
            {
                this.DrawPixel(currentX, endY, color);
            }

            for (int currentY = startY; currentY != endY; currentY++)
            {
                this.DrawPixel(startX, currentY, color);
            }

            for (int currentY = startY; currentY <= endY; currentY++)
            {
                this.DrawPixel(endX, currentY, color);
            }
        }

        private void DrawRectangleFilled(int startX, int startY,
            int endX, int endY, Color color)
        {
            for (int currentY = startY; currentY != endY; currentY++)
            {
                for (int xx = startX; xx != endX; xx++)
                {
                    this.DrawPixel(xx, currentY, color);
                }
            }
        }

        private void DrawCircleOutline(int centerX, int centerY,
            int radius, Color color)
        {
            // Midpoint Circle Algorithm: https://en.wikipedia.org/wiki/Midpoint_circle_algorithm

            void drawCircle(int xc, int yc, int x, int y, Color color)
            {
                this.DrawPixel(xc + x, yc + y, color);
                this.DrawPixel(xc - x, yc + y, color);
                this.DrawPixel(xc + x, yc - y, color);
                this.DrawPixel(xc - x, yc - y, color);
                this.DrawPixel(xc + y, yc + x, color);
                this.DrawPixel(xc - y, yc + x, color);
                this.DrawPixel(xc + y, yc - x, color);
                this.DrawPixel(xc - y, yc - x, color);
            }

            int x = 0, y = radius;
            int d = 3 - 2 * radius;
            drawCircle(centerX, centerY, x, y, color);
            while (y >= x)
            {
                // for each pixel we will
                // draw all eight pixels

                x++;

                // check for decision parameter
                // and correspondingly
                // update d, x, y
                if (d > 0)
                {
                    y--;
                    d = d + 4 * (x - y) + 10;
                }
                else
                {
                    d = d + 4 * x + 6;
                }

                drawCircle(centerX, centerY, x, y, color);
            }
        }

        private void DrawCircleFilled(int centerX, int centerY,
            int radius, Color color)
        {
            // Midpoint Circle Algorithm: https://en.wikipedia.org/wiki/Midpoint_circle_algorithm
            // C# Implementation: https://rosettacode.org/wiki/Bitmap/Midpoint_circle_algorithm#C.23

            var d = 3 - 2 * radius;
            var x = 0;
            var y = radius;

            while (x <= y)
            {
                this.DrawLine(centerX + x, centerY + y, centerX - x, centerY + y, color);
                this.DrawLine(centerX + x, centerY - y, centerX - x, centerY - y, color);
                this.DrawLine(centerX - y, centerY + x, centerX + y, centerY + x, color);
                this.DrawLine(centerX - y, centerY - x, centerX + y, centerY - x, color);

                if (d < 0)
                {
                    d += (2 * x) + 1;
                }
                else
                {
                    d += (2 * (x - y)) + 1;
                    y--;
                }
                x++;
            }
        }

        /// <summary>
        /// Draws a pixel on the display with respect to the current <see cref="DisplayRotation"/>.
        /// </summary>
        /// <param name="x">The X position.</param>
        /// <param name="y">The Y position.</param>
        /// <param name="color">The color to use when drawing the pixel.</param>
        public void DrawPixel(int x, int y, Color color)
        {
            this.ePaperDisplay.FrameBuffer.SetPixel(this.GetRealPosition(x, y), color);
        }

        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.ePaperDisplay?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
