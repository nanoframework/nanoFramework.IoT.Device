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

        public void DrawText(string text, IFont font, int x, int y)
        {
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

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    this.ePaperDisplay?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Graphics()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
