using System;

using Iot.Device.ePaper;

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
                DrawRectangleFilled(startX, startY, endX, endY, color);
            else
                DrawRectangleOutline(startX, startY, endX, endY, color);
        }

        /// <summary>
        /// Writes text to the display.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="font">The font to use.</param>
        /// <param name="x">Starting X point.</param>
        /// <param name="y">Starting Y point.</param>
        public void DrawText(string text, IFont font, int x, int y)
        {
            //TODO: get rid of this method by updating the font byte array so it is not reversed.
            uint Reverse(uint a, int length)
            {
                uint b = 0b_0;
                for (int i = 0; i < length; i++)
                {
                    b = (b << 1) | (a & 0b_1);
                    a = a >> 1;
                }
                return b;
            }

            var col = 0;
            var line = 0;

            foreach (char c in text)
            {
                if (col == this.ePaperDisplay.Width)
                {
                    col = 0;
                    line += font.Height + 1;
                }

                var cb = font[c];
                for (var i = 0; i < font.Height; i++)
                {
                    var xPos = x + col;
                    var yPos = y + line + i;

                    this.ePaperDisplay.DrawBuffer(xPos, yPos,
                        (byte)~Reverse(cb[i], font.Width));
                }

                col += font.Width;
            }
        }

        /// <summary>
        /// Gets the real X Position of a given point after considering the current <see cref="Rotation"/> of the display.
        /// </summary>
        /// <param name="x">The X Position in the current rotation.</param>
        /// <param name="y">The Y Position in the current rotation.</param>
        /// <param name="displayWidth"></param>
        /// <returns>The real X position on the display.</returns>
        public int GetRealXPosition(int x, int y)
            => this.DisplayRotation switch
            {
                Rotation.NinetyDegreesClockwise => this.ePaperDisplay.Width - y,
                Rotation.OneEightyDegreesClockwise => this.ePaperDisplay.Width - x,
                Rotation.TwoSeventyDegreesClockwise => y,
                _ => x
            };

        /// <summary>
        /// Gets the real Y Position of a given point after considering the current <see cref="Rotation"/> of the display.
        /// </summary>
        /// <param name="x">The X Position in the current rotation.</param>
        /// <param name="y">The Y Position in the current rotation.</param>
        /// <param name="displayWidth"></param>
        /// <returns>The real Y position on the display.</returns>
        public int GetRealYPosition(int x, int y)
            => this.DisplayRotation switch
            {
                Rotation.NinetyDegreesClockwise => x,
                Rotation.OneEightyDegreesClockwise => this.ePaperDisplay.Height - y,
                Rotation.TwoSeventyDegreesClockwise => this.ePaperDisplay.Height - x,
                _ => y
            };


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

        private void DrawPixel(int x, int y, Color color)
        {
            this.ePaperDisplay.DrawPixel(this.GetRealXPosition(x, y),
                this.GetRealYPosition(x, y),
               color);
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
