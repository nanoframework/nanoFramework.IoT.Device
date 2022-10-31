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

        public void DrawLine(int x1, int y1, int x2, int y2, Color color)
        {
            // This is a common line drawing algorithm. Read about it here:
            // http://en.wikipedia.org/wiki/Bresenham's_line_algorithm
            int sx = (x1 < x2) ? 1 : -1;
            int sy = (y1 < y2) ? 1 : -1;

            int dx = x2 > x1 ? x2 - x1 : x1 - x2;
            int dy = y2 > x1 ? y2 - y1 : y1 - y2;

            float err = dx - dy, e2;

            // if there is an error with drawing a point or the line is finished get out of the loop!
            while (!(x1 == x2 && y1 == y2))
            {
                this.ePaperDisplay.DrawPixel(x1, y1, color);

                e2 = 2 * err;

                if (e2 > -dy)
                {
                    err -= dy;
                    x1 += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    y1 += sy;
                }
            }
        }

        public void DrawCircle(int xc, int yc, int r, Color color)
        {
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

            int x = 0, y = r;
            int d = 3 - 2 * r;
            drawCircle(xc, yc, x, y, color);
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

                drawCircle(xc, yc, x, y, color);
            }
        }

        public void DrawRectangle(int x, int y, int width, int height, Color color, bool isFilled)
        {
            // This will draw points
            int xe = x + width;
            int ye = y + height;

            if (isFilled)
            {
                for (int yy = y; yy != ye; yy++)
                {
                    for (int xx = x; xx != xe; xx++)
                    {
                        this.ePaperDisplay.DrawPixel(xx, yy, color);
                    }
                }
            }
            else
            {
                xe -= 1;
                ye -= 1;

                for (int xx = x; xx != xe; xx++)
                {
                    this.ePaperDisplay.DrawPixel(xx, y, color);
                }

                for (int xx = x; xx <= xe; xx++)
                {
                    this.ePaperDisplay.DrawPixel(xx, ye, color);
                }

                for (int yy = y; yy != ye; yy++)
                {
                    this.ePaperDisplay.DrawPixel(x, yy, color);
                }

                for (int yy = y; yy <= ye; yy++)
                {
                    this.ePaperDisplay.DrawPixel(xe, yy, color);
                }
            }
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
