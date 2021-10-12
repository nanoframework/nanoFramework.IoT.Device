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

            switch (resolution)
            {
                case DisplayResolution.OLED128x64:
                    Width = 128;
                    Height = 64;
                    _i2cDevice.Write(_oled128x64Init);
                    break;
                case DisplayResolution.OLED128x32:
                    Width = 128;
                    Height = 32;
                    _i2cDevice.Write(_oled128x32Init);
                    break;
                case DisplayResolution.OLED96x16:
                    Width = 96;
                    Height = 16;
                    _i2cDevice.Write(_oled96x16Init);
                    break;
            }

            Pages = (byte)(Height / 8);
            _genericBuffer = new byte[Pages * Width + 4];//adding 4 bytes make it SSH1106 IC OLED compatible
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
        /// <param name="inverted">Indicates if color to be used turn the pixel on, or leave off.</param>
        public void DrawPixel(int x, int y, bool inverted = true)
        {
            if ((x >= Width) || (y >= Height))
            {
                return;
            }

            // x specifies the column
            int idx = x + (y / 8) * Width;

            if (inverted)
            {
                _genericBuffer[idx] |= (byte)(1 << (y & 7));
            }
            else
            {
                _genericBuffer[idx] &= (byte)~(1 << (y & 7));
            }
        }

        /// <summary>
        /// Draws a horizontal line.
        /// </summary>
        /// <param name="x0">x coordinate starting of the line.</param>
        /// <param name="y0">y coordinate starting of line.</param>
        /// <param name="length">Line length.</param>
        /// <param name="inverted">Turn the pixel on (true) or off (false).</param>
        public void DrawHorizontalLine(int x0, int y0, int length, bool inverted = true)
        {
            for (var x = x0; (x - x0) < length; x++)
            {
                DrawPixel(x, y0, inverted);
            }
        }

        /// <summary>
        /// Draws a vertical line.
        /// </summary>
        /// <param name="x0">x coordinate starting of the line.</param>
        /// <param name="y0">y coordinate starting of line.</param>
        /// <param name="length">Line length.</param>
        /// <param name="inverted">Turn the pixel on (true) or off (false).</param>
        public void DrawVerticalLine(int x0, int y0, int length, bool inverted = true)
        {
            for (var y = y0; (y - y0) < length; y++)
            {
                DrawPixel(x0, y, inverted);
            }
        }

        /// <summary>
        /// Draws a rectangle that is solid/filled.
        /// </summary>
        /// <param name="x0">x coordinate starting of the top left.</param>
        /// <param name="y0">y coordinate starting of the top left.</param>
        /// <param name="width">Width of rectabgle in pixels.</param>
        /// <param name="height">Height of rectangle in pixels</param>
        /// <param name="inverted">Turn the pixel on (true) or off (false).</param>
        public void DrawFilledRectangle(int x0, int y0, int width, int height, bool inverted = true)
        {
            // width--;
            // height--;
            for (int i = 0; i <= height; i++)
            {
                DrawHorizontalLine(x0, y0 + i, width, inverted);
            }
        }

        /// <summary>
        /// Displays the  1 bit bit map.
        /// </summary>
        /// <param name="x">The x coordinate on the screen.</param>
        /// <param name="y">The y coordinate on the screen.</param>
        /// <param name="width">Width in bytes.</param>
        /// <param name="height">Height in bytes.</param>
        /// <param name="bitmap">Bitmap to display.</param>
        /// <param name="size">Drawing size, normal = 1, larger use 2,3 etc.</param>
        public void DrawBitmap(int x, int y, int width, int height, byte[] bitmap, byte size = 1)
        {
            if ((width * height) != bitmap.Length)
            {
                throw new ArgumentException("Width and height do not match the bitmap size.");
            }

            for (var yO = 0; yO < height; yO++)
            {
                byte mask = 0x01;

                for (var xA = 0; xA < width; xA++)
                {
                    var b = bitmap[(yO * width) + xA];

                    for (var pixel = 0; pixel < 8; pixel++)
                    {
                        if (size == 1)
                        {
                            DrawPixel(x + (8 * xA) + pixel, y + yO, (b & mask) > 0);
                        }
                        else
                        {
                            DrawFilledRectangle((x + (8 * xA) + pixel) * size, (y / size + yO) * size, size, size, (b & mask) > 0);
                        }
                        mask <<= 1;
                    }

                    mask = 0x01;//reset each time to support SSH1106 OLEDs
                }
            }
        }

        /// <summary>
        /// Writes a text message on the screen with font in use.
        /// </summary>
        /// <param name="x">The x pixel-coordinate on the screen.</param>
        /// <param name="y">The y pixel-coordinate on the screen.</param>
        /// <param name="str">Text string to display.</param>
        /// <param name="size">Text size, normal = 1, larger use 2,3, 4 etc.</param>
		/// <param name="center">Indicates if text should be centered if possible.</param>
        /// <seealso cref="Write"/>
        public void DrawString(int x, int y, string str, byte size = 1, bool center = false)
        {
            if (center && str != null)
            {
                int padSize = (Width / size / Font.Width - str.Length) / 2;
                if (padSize > 0)
                    str = str.PadLeft(str.Length + padSize);
            }

            byte[] bitMap = this.GetTextBytes(str);

            this.DrawBitmap(x, y, bitMap.Length / Font.Height, Font.Height, bitMap, size);
        }

        /// <summary>
        /// Writes a text message on the screen with font in use.
        /// </summary>
        /// <param name="x">The x text-coordinate on the screen.</param>
        /// <param name="y">The y text-coordinate on the screen.</param>
        /// <param name="str">Text string to display.</param>
        /// <param name="size">Text size, normal = 1, larger use 2,3, 4 etc.</param>
		/// <param name="center">Indicates if text should be centered if possible.</param>
        /// <seealso cref="DrawString"/>
        public void Write(int x, int y, string str, byte size = 1, bool center = false)
        {
            DrawString(x * Font.Width, y * Font.Height, str, size, center);
        }

        /// <summary>
        /// Get the bytes to be drawn on the screen for text, from the font
        /// </summary>
        /// <param name="text">Strint to be shown on the screen.</param>
        /// <returns>The bytes to be drawn using current font.</returns>
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
        /// Displays the information on the screen using page mode.
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
        /// Sequence of bytes that should be sent to a 128x64 OLED display to setup the device.
        /// First byte is the command byte 0x00.
        /// </summary>
        private readonly byte[] _oled128x64Init =
        {
            0x00,       // is command
            0xae,       // turn display off
            0xd5,0x80,  // set display clock divide ratio/oscillator,  set ratio = 0x80
            0xa8, 0x3f, // set multiplex ratio 0x00-0x3f        
            0xd3, 0x00, // set display offset 0x00-0x3f, no offset = 0x00
            0x40 | 0x0, // set display start line 0x40-0x7F
            0x8d, 0x14, // set charge pump,  enable  = 0x14  disable = 0x10
            0x20, 0x00, // 0x20 set memory address mode,  0x0 = horizontal addressing mode
            0xa0 | 0x1, // set segment re-map
            0xc8,       // set com output scan direction
            0xda, 0x12, // set COM pins HW configuration
            0x81, 0xcf, // set contrast control for BANK0, extVcc = 0x9F,  internal = 0xcf
            0xd9, 0xf1, // set pre-charge period  to 0xf1,  if extVcc then set to 0x22
            0xdb,       // set VCOMH deselect level
            0x40,       // set display start line
            0xa4,       // set display ON
            0xa6,       // set normal display
            0xaf        // turn display on 0xaf
        };

        /// <summary>
		/// Sequence of bytes that should be sent to a 128x32 OLED display to setup the device.
		/// First byte is the command byte 0x00.
		/// </summary>
		private readonly byte[] _oled128x32Init =
        {
            0x00,       // is command
            0xae,       // turn display off
            0xd5,0x80,  // set display clock divide ratio/oscillator,  set ratio = 0x80
            0xa8, 0x1f, // set multiplex ratio 0x00-0x1f        
            0xd3, 0x00, // set display offset 0x00-0x3f, no offset = 0x00
            0x40 | 0x0, // set display start line 0x40-0x7F
            0x8d, 0x14, // set charge pump,  enable  = 0x14  disable = 0x10
            0x20, 0x00, // 0x20 set memory address mode,  0x0 = horizontal addressing mode
            0xa0 | 0x1, // set segment re-map
            0xc8,       // set com output scan direction
            0xda, 0x02, // set COM pins HW configuration
            0x81, 0x8f, // set contrast control for BANK0, extVcc = 0x9F,  internal = 0xcf
            0xd9, 0xf1, // set pre-charge period  to 0xf1,  if extVcc then set to 0x22
            0xdb,       // set VCOMH deselect level
            0x40,       // set display start line
            0xa4,       // set display ON
            0xa6,       // set normal display
            0xaf        // turn display on 0xaf
        };

        /// <summary>
		/// Sequence of bytes that should be sent to a 96x16 OLED display to setup the device.
		///	First byte is the command byte 0x00.
		/// </summary>
		private readonly byte[] _oled96x16Init =
        {
            0x00,       // is command
            0xae,       // turn display off
            0xd5,0x80,  // set display clock divide ratio/oscillator,  set ratio = 0x80
            0xa8, 0x1f, // set multiplex ratio 0x00-0x1f        
            0xd3, 0x00, // set display offset 0x00-0x3f, no offset = 0x00
            0x40 | 0x0, // set display start line 0x40-0x7F
            0x8d, 0x14, // set charge pump,  enable  = 0x14  disable = 0x10
            0x20, 0x00, // 0x20 set memory address mode,  0x0 = horizontal addressing mode
            0xa0 | 0x1, // set segment re-map
            0xc8,       // set com output scan direction
            0xda, 0x02, // set COM pins HW configuration
            0x81, 0xaf, // set contrast control for BANK0, extVcc = 0x9F,  internal = 0xcf
            0xd9, 0xf1, // set pre-charge period  to 0xf1,  if extVcc then set to 0x22
            0xdb,       // set VCOMH deselect level
            0x40,       // set display start line
            0xa4,       // set display ON
            0xa6,       // set normal display
            0xaf        // turn display on 0xaf
        };

        /// <summary>
        /// Resolution specifier.
        /// </summary>
        public enum DisplayResolution
        {
            /// <summary>
            /// Option for 128x64 OLED
            /// </summary>
            OLED128x64,
            /// <summary>
            /// Option for 128x32 OLED
            /// </summary>
            OLED128x32,
            /// <summary>
            /// Option for 96x16 OLED
            /// </summary>
            OLED96x16
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