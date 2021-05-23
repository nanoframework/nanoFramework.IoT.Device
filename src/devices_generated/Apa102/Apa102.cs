// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using System.Drawing;

namespace Iot.Device.Apa102
{
    /// <summary>
    /// Driver for APA102. A double line transmission integrated control LED
    /// </summary>
    public class Apa102 : IDisposable
    {
        /// <summary>
        /// Colors of LEDs
        /// </summary>
        public SpanColor Pixels => _pixels;

        private SpiDevice _spiDevice;
        private Color[] _pixels;
        private byte[] _buffer;

        /// <summary>
        /// Initializes a new instance of the APA102 device.
        /// </summary>
        /// <param name="spiDevice">The SPI device used for communication.</param>
        /// <param name="length">Number of LEDs</param>
        public Apa102(SpiDevice spiDevice, int length)
        {
            _spiDevice = spiDevice ?? throw new ArgumentNullException(nameof(spiDevice));
            _pixels = new Color[length];
            _buffer = new byte[(length + 2) * 4];

            _buffer.AsSpan(0, 4).Fill(0x00); // start frame
            _buffer.AsSpan((length + 1) * 4, 4).Fill(0xFF); // end frame
        }

        /// <summary>
        /// Update color data to LEDs
        /// </summary>
        public void Flush()
        {
            for (var i = 0; i < _pixels.Length; i++)
            {
                var pixel = _buffer.AsSpan((i + 1) * 4);
                pixel[0] = (byte)((_pixels[i].A >> 3) | 0b11100000); // global brightness (alpha)
                pixel[1] = _pixels[i].B; // blue
                pixel[2] = _pixels[i].G; // green
                pixel[3] = _pixels[i].R; // red
            }

            _spiDevice.Write(_buffer);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _spiDevice?.Dispose();
            _spiDevice = null!;
            _pixels = null!;
            _buffer = null!;
        }
    }
}
