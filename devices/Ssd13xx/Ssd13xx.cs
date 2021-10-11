// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;

namespace Iot.Device.Ssd13xx
{
    /// <summary>
    /// Represents base class for SSD13xx OLED displays
    /// </summary>
    public abstract class Ssd13xx : IDisposable
    {
        // Multiply of screen resolution plus single command byte.
        private byte[] _genericBuffer;
        private byte[] _pageData;

        /// <summary>
        /// Underlying I2C device
        /// </summary>
        protected I2cDevice _i2cDevice;


        /// <summary>
        /// Screen Resolution Width in Pixels
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Screen Resolution Height in Pixels
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Screen data pages.
        /// </summary>
        public byte Pages { get; set; }

        /// <summary>
        /// Font to use.
        /// </summary>
        public IFont Font { get; set; }


        /// <summary>
        /// Constructs instance of Ssd13xx
        /// </summary>
        /// <param name="i2cDevice">I2C device used to communicate with the device</param>
        /// <param name="resolution">Screen resolution to use for device init.</param>
        public Ssd13xx(I2cDevice i2cDevice, DisplayResolution resolution = DisplayResolution.OLED128x64)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

            Width = 128;

            switch (resolution)
            {
                case DisplayResolution.OLED128x64:
                    Height = 64;
                    _i2cDevice.Write(_oled128x64Init);
                    break;
                case DisplayResolution.OLED128x32:
                    Height = 32;
                    _i2cDevice.Write(_oled128x32Init);
                    break;
            }

            Pages = (byte)(Height / 8);
            _genericBuffer = new byte[Pages * Width];
            _pageData = new byte[Width + 1];
        }

        /// <summary>
        /// Verifies value is within a specific range.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="start">Starting value of range.</param>
        /// <param name="end">Ending value of range.</param>
        /// <returns>Determines if value is within range.</returns>
        internal static bool InRange(uint value, uint start, uint end)
        {
            return (value - start) <= (end - start);
        }

        /// <summary>
        /// Send a command to the display controller.
        /// </summary>
        /// <param name="command">The command to send to the display controller.</param>
        public abstract void SendCommand(ISharedCommand command);

        /// <summary>
        /// Send data to the display controller.
        /// </summary>
        /// <param name="data">The data to send to the display controller.</param>
        public virtual void SendData(SpanByte data)
        {
            if (data.IsEmpty)
            {
                throw new ArgumentNullException(nameof(data));
            }

            SpanByte writeBuffer = SliceGenericBuffer(data.Length + 1);

            writeBuffer[0] = 0x40; // Control byte.
            data.CopyTo(writeBuffer.Slice(1));
            _i2cDevice.Write(writeBuffer);
        }


        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
        }

        /// <summary>
        /// Acquires span of specific length pointing to the command buffer.
        /// If length of the command buffer is too small it will be reallocated.
        /// </summary>
        /// <param name="length">Requested length</param>
        /// <returns>Span of bytes pointing to the command buffer</returns>
        protected SpanByte SliceGenericBuffer(int length) => SliceGenericBuffer(0, length);

        /// <summary>
        /// Acquires span of specific length at specific position in command buffer.
        /// If length of the command buffer is too small it will be reallocated.
        /// </summary>
        /// <param name="start">Start index of the requested span</param>
        /// <param name="length">Requested length</param>
        /// <returns>Span of bytes pointing to the command buffer</returns>
        protected SpanByte SliceGenericBuffer(int start, int length)
        {
            if (_genericBuffer.Length < length)
            {
                var newBuffer = new byte[_genericBuffer.Length * 2];
                _genericBuffer.CopyTo(newBuffer, 0);
                _genericBuffer = newBuffer;
            }

            return _genericBuffer;
        }


        /// <summary>
        /// Draws a pixel on the screen.
        /// </summary>
        /// <param name="x">The x coordinate on the screen.</param>
        /// <param name="y">The y coordinate on the screen.</param>
        /// <param name="useWhite">Indicates if color to be used, default yes unless is inverted screen.</param>
        public void DrawPixel(int x, int y, bool useWhite = true)
        {
            if ((x >= Width) || (y >= Height))
                return;

            // x specifies the column
            int idx = x + (y / 8) * Width;

            if (useWhite)
                _genericBuffer[idx] |= (byte)(1 << (y & 7));
            else
                _genericBuffer[idx] &= (byte)~(1 << (y & 7));

            //INVERSE COLOR: _genericBuffer[idx] ^= (byte)(1 << (y & 7));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="bitmap"></param>
        public void DrawBitmap(int x, int y, int width, int height, byte[] bitmap)
        {
            if ((width * height) != bitmap.Length)
                throw new ArgumentException("Width and height do not match the bitmap size.");

            for (var yO = 0; yO < height; yO++)
            {
                for (var xA = 0; xA < width; xA++)
                {
                    var b = bitmap[(yO * width) + xA];
                    byte mask = 0x01;
                    for (var pixel = 0; pixel < 8; pixel++)
                    {
                        DrawPixel(x + (8 * xA) + pixel, y + yO, (b & mask) > 0);
                        mask <<= 1;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="str"></param>
        public void DrawString(int x, int y, string str)
        {
            byte[] bitMap = GetTextBytes(str);

            DrawBitmap(x, y, bitMap.Length / Font.Height, Font.Height, bitMap);
        }


        /// <summary>
        /// Get the bytes to be drawn on the screen for text, from the font
        /// </summary>
        /// <param name="text">Strint to be shown on the screen.</param>
        /// <returns></returns>
        byte[] GetTextBytes(string text)
        {
            byte[] bitMap;

            if (Font.Width == 8)
            {
                bitMap = new byte[text.Length * Font.Height * Font.Width / 8];

                for (int i = 0; i < text.Length; i++)
                {
                    var characterMap = Font[text[i]];

                    for (int segment = 0; segment < Font.Height; segment++)
                    {
                        bitMap[i + (segment * text.Length)] = characterMap[segment];
                    }
                }
            }
            else
            {
                throw new Exception("Font width must be 8");
            }

            return bitMap;
        }

        /// <summary>
        /// Displays the information on the screen.
        /// </summary>
        public void Display()
        {
            for (byte i = 0; i < Pages; i++)
            {
                _pageCmd[1] = (byte)(PageAddress.Page0 + i); // page number
                _i2cDevice.Write(_pageCmd);

                _pageData[0] = 0x40; // is data
                Array.Copy(_genericBuffer, i * Width, _pageData, 1, Width);
                _i2cDevice.Write(_pageData);
            }
        }

        /// <summary>
        /// Clears the screen.
        /// </summary>
        public void ClearScreen()
        {
            Array.Clear(_genericBuffer, 0, _genericBuffer.Length);

            Display();
        }



        /// <summary>
        ///     Sequence of bytes that should be sent to a 128x64 OLED display to setup the device.
        ///		First byte is the command byte 0x00.
        /// </summary>
        private readonly byte[] _oled128x64Init =
        {
            0x00, 0xae, 0xd5, 0x80, 0xa8, 0x3f, 0xd3, 0x00, 0x40 | 0x0, 0x8d, 0x14, 0x20, 0x00, 0xa0 | 0x1, 0xc8,
            0xda, 0x12, 0x81, 0xcf, 0xd9, 0xf1, 0xdb, 0x40, 0xa4, 0xa6, 0xaf
        };

        /// <summary>
		///     Sequence of bytes that should be sent to a 128x32 OLED display to setup the device.
		///		First byte is the command byte 0x00.
		/// </summary>
		private readonly byte[] _oled128x32Init =
        {
            0x00,0xae, 0xd5, 0x80, 0xa8, 0x1f, 0xd3, 0x00, 0x40 | 0x0, 0x8d, 0x14, 0x20, 0x00, 0xa0 | 0x1, 0xc8,
            0xda, 0x02, 0x81, 0x8f, 0xd9, 0x1f, 0xdb, 0x40, 0xa4, 0xa6, 0xaf
        };


        /// <summary>
        /// Resolution specifier.
        /// </summary>
        public enum DisplayResolution
        {
            OLED128x64,
            OLED128x32
        }


        /// <summary>
        /// Page mode output command bytes.
        /// </summary>
        private byte[] _pageCmd = new byte[]
        {
            0x00, // is command
            0xB0, // page address (B0-B7)
            0x00, // lower columns address =0
            0x10, // upper columns address =0
        };
    }
}
