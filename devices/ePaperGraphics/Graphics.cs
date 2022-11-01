using System;

using Iot.Device.ePaper;

namespace Iot.Device.ePaperGraphics
{
    public sealed class Graphics : IDisposable
    {
        private bool disposedValue;

        public IePaperDisplay ePaperDisplay { get; }

        public Graphics(IePaperDisplay ePaperDisplay)
        {
            this.ePaperDisplay = ePaperDisplay;
        }

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
                this.ePaperDisplay.DrawPixel(startX, startY, color);

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

        public void DrawCircle(int centerX, int centerY, 
            int radius, Color color, bool fill)
        {
            if (fill)
                this.DrawCircleFilled(centerX, centerY, radius, color);
            else
                this.DrawCircleOutline(centerX, centerY, radius, color);
        }

        public void DrawRectangle(int x, int y, int width, 
            int height, Color color, bool fill)
        {
            // This will draw points
            int endX = x + width;
            int endY = y + height;

            if (fill)
                DrawRectangleFilled(x, y, endX, endY, color);
            else
                DrawRectangleOutline(x, y, endX, endY, color);
        }

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
                    this.ePaperDisplay.DrawBuffer(x + col, y + line + i, (byte)~Reverse(cb[i], font.Width));
                }

                col += font.Width;
            }
        }




        private void DrawRectangleOutline(int startX, int startY,
            int endX, int endY, Color color)
        {
            endX -= 1;
            endY -= 1;

            for (int currentX = startX; currentX != endX; currentX++)
            {
                this.ePaperDisplay.DrawPixel(currentX, startY, color);
            }

            for (int currentX = startX; currentX <= endX; currentX++)
            {
                this.ePaperDisplay.DrawPixel(currentX, endY, color);
            }

            for (int currentY = startY; currentY != endY; currentY++)
            {
                this.ePaperDisplay.DrawPixel(startX, currentY, color);
            }

            for (int currentY = startY; currentY <= endY; currentY++)
            {
                this.ePaperDisplay.DrawPixel(endX, currentY, color);
            }
        }

        private void DrawRectangleFilled(int startX, int startY,
            int endX, int endY, Color color)
        {
            for (int currentY = startY; currentY != endY; currentY++)
            {
                for (int xx = startX; xx != endX; xx++)
                {
                    this.ePaperDisplay.DrawPixel(xx, currentY, color);
                }
            }
        }

        private void DrawCircleOutline(int centerX, int centerY, 
            int radius, Color color)
        {
            // Midpoint Circle Algorithm: https://en.wikipedia.org/wiki/Midpoint_circle_algorithm

            void drawCircle(int xc, int yc, int x, int y, Color color)
            {
                this.ePaperDisplay.DrawPixel(xc + x, yc + y, color);
                this.ePaperDisplay.DrawPixel(xc - x, yc + y, color);
                this.ePaperDisplay.DrawPixel(xc + x, yc - y, color);
                this.ePaperDisplay.DrawPixel(xc - x, yc - y, color);
                this.ePaperDisplay.DrawPixel(xc + y, yc + x, color);
                this.ePaperDisplay.DrawPixel(xc - y, yc + x, color);
                this.ePaperDisplay.DrawPixel(xc + y, yc - x, color);
                this.ePaperDisplay.DrawPixel(xc - y, yc - x, color);
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
