// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Drawing;
using System.Threading;

using Iot.Device.EPaper.Buffers;
using Iot.Device.EPaper.Enums;
using Iot.Device.EPaper.Primitives;

namespace Iot.Device.EPaper.Drivers.Ssd168x.Ssd1681
{
    /// <summary>
    /// A driver class for the SSD1681 display controller.
    /// </summary>
    public sealed class Ssd1681 : Ssd168x
    {
        /// <summary>
        /// The max supported clock frequency for the SSD1681 controller. 20MHz.
        /// </summary>
        public const int SpiClockFrequency = 20_000_000;

        /// <summary>
        /// The supported <see cref="System.Device.Spi.SpiMode"/> by the SSD1681 controller.
        /// </summary>
        public const SpiMode SpiMode = System.Device.Spi.SpiMode.Mode0;

        private FrameBuffer2BitPerPixel _frameBuffer2bpp;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ssd1681"/> class.
        /// </summary>
        /// <param name="spiDevice">The communication channel to the SSD1681-based dispay.</param>
        /// <param name="resetPin">The reset GPIO pin. Passing an invalid pin number such as -1 will prevent this driver from opening the pin. Caller should handle hardware resets.</param>
        /// <param name="busyPin">The busy GPIO pin.</param>
        /// <param name="dataCommandPin">The data/command GPIO pin.</param>
        /// <param name="width">The width of the display.</param>
        /// <param name="height">The height of the display.</param>
        /// <param name="gpioController">The <see cref="GpioController"/> to use when initializing the pins.</param>
        /// <param name="enableFramePaging">Page the frame buffer and all operations to use less memory.</param>
        /// <param name="shouldDispose"><see langword="true"/> to dispose of the <see cref="GpioController"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="spiDevice"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Display width and height can't be less than 0 or greater than 200.</exception>
        /// <remarks>
        /// For a 200x200 SSD1681 display, a full Frame requires about 5KB of RAM ((200 * 200) / 8). SSD1681 has 2 RAMs for B/W and Red pixel.
        /// This means to have a full frame in memory, you need about 10KB of RAM. If you can't guarantee 10KB to be available to the driver
        /// then enable paging by setting <paramref name="enableFramePaging"/> to true. A page uses about 2KB (1KB for B/W and 1KB for Red).
        /// </remarks>
        public Ssd1681(
            SpiDevice spiDevice,
            int resetPin,
            int busyPin,
            int dataCommandPin,
            int width,
            int height,
            GpioController gpioController = null,
            bool enableFramePaging = true,
            bool shouldDispose = true) : base(spiDevice, resetPin, busyPin, dataCommandPin, width, height, gpioController, enableFramePaging, shouldDispose)
        {
        }

        /// <inheritdoc/>
        public override IFrameBuffer FrameBuffer
        {
            get
            {
                return _frameBuffer2bpp;
            }

            protected set
            {
                _frameBuffer2bpp = (FrameBuffer2BitPerPixel)value;
            }
        }

        /// <inheritdoc/>
        protected override int PagesPerFrame { get; } = 5;

        /// <summary>
        /// Perform the required initialization steps to set up the display.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to initialize the display until it has been powered on. Call PowerOn() first.</exception>
        public override void Initialize()
        {
            if (PowerState != PowerState.PoweredOn)
            {
                throw new InvalidOperationException();
            }

            // set gate lines and scanning sequence
            SendCommand((byte)Command.DriverOutputControl);

            // refer to the datasheet for a description of the parameters
            SendData((byte)(Height - 1), 0x00, 0x00);

            // Set data entry sequence
            SendCommand((byte)Command.DataEntryModeSetting);

            // Y Increment, X Increment with RAM address counter auto incremented in the X direction.
            SendData(0x03);

            // Set RAM X start / end position
            SendCommand((byte)Command.SetRAMAddressXStartEndPosition);

            // Param1: Start at 0 | Param2: End at display width converted to bytes
            SendData(0x00, (byte)((Width / 8) - 1));

            // Set RAM Y start / end positon
            SendCommand((byte)Command.SetRAMAddressYStartEndPosition);

            // Param1 & 2: Start at 0 | Param3 & 4: End at display height converted to bytes
            SendData(/* Start at 0 */ 0x00, 0x00, /* End */ (byte)(Height - 1), 0x00);

            // Set Panel Border
            SendCommand((byte)Command.BorderWaveformControl);
            SendData(0xc0);

            // Set Temperature sensor to use internal temp sensor
            SendCommand((byte)Command.TempSensorControlSelection);
            SendData(0x80);

            // Do a full refresh of the display
            PerformFullRefresh();
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            // write B/W and Color (RED) frame to the display's RAM.
            DirectDrawBuffer(
                CurrentFrameBufferStartXPosition,
                CurrentFrameBufferStartYPosition,
                _frameBuffer2bpp.BlackBuffer.Buffer);

            DirectDrawColorBuffer(
                CurrentFrameBufferStartXPosition,
                CurrentFrameBufferStartYPosition,
                _frameBuffer2bpp.ColorBuffer.Buffer);
        }

        /// <summary>
        /// Draws a single pixel to the appropriate frame buffer.
        /// </summary>
        /// <param name="x">The X Position.</param>
        /// <param name="y">The Y Position.</param>
        /// <param name="inverted">True to invert the pixel from white to black.</param>
        /// <remarks>
        /// The SSD1681 comes with 2 RAMs: a Black and White RAM and a Red RAM.
        /// Writing to the B/W RAM draws B/W pixels on the panel. While writing to the Red RAM draws red pixels on the panel (if the panel supports red).
        /// However, the SSD1681 doesn't support specifying the color level (no grayscaling), therefore the way the buffer is selected 
        /// is by performing a simple binary check: 
        /// if R >= 128 and G == 0 and B == 0 then write a red pixel to the Red Buffer/RAM
        /// if R == 0 and G == 0 and B == 0 then write a black pixel to B/W Buffer/RAM
        /// else, assume white pixel and write to B/W Buffer/RAM.
        /// </remarks>
        public void DrawPixel(int x, int y, bool inverted)
        {
            DrawPixel(x, y, inverted ? Color.Black : Color.White);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// The SSD1681 comes with 2 RAMs: a Black and White RAM and a Red RAM.
        /// Writing to the B/W RAM draws B/W pixels on the panel. While writing to the Red RAM draws red pixels on the panel (if the panel supports red).
        /// However, the SSD1681 doesn't support specifying the color level (no grayscaling), therefore the way the buffer is selected 
        /// is by performing a simple binary check: 
        /// if R >= 128 and G == 0 and B == 0 then write a red pixel to the Red Buffer/RAM
        /// if R == 0 and G == 0 and B == 0 then write a black pixel to B/W Buffer/RAM
        /// else, assume white pixel and write to B/W Buffer/RAM.
        /// </remarks>
        public override void DrawPixel(int x, int y, Color color)
        {
            // ignore out of bounds draw attempts
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return;
            }

            var frameByteIndex = GetFrameBufferIndex(x, y);
            var pageByteIndex = frameByteIndex - CurrentFrameBufferPageLowerBound;

            // if the specified point falls in the current page, update the buffer
            if (CurrentFrameBufferPageLowerBound <= frameByteIndex
                && frameByteIndex < CurrentFrameBufferPageUpperBound)
            {
                /*
                 * Lookup Table for colors on SSD1681
                 * 
                 *  LUT for Red, Black, and White ePaper display
                 * 
                 * |                |               |                    |
                 * | Data Red RAM   | Data B/W RAM  | Result Pixel Color |
                 * |----------------|---------------|--------------------|
                 * |        0       |       0       |       Black        |
                 * |        0       |       1       |       White        |
                 * |        1       |       0       |       Red          |
                 * |        1       |       1       |       Red          |
                 * 
                 * 
                 *  LUT for Black and White ePaper display with SSD1681
                 * |                |               |                    |
                 * | Data Red RAM   | Data B/W RAM  | Result Pixel Color |
                 * |----------------|---------------|--------------------|
                 * |        0       |       0       |       Black        |
                 * |        0       |       1       |       White        |
                 * |        1       |       0       |       Black        |
                 * |        1       |       1       |       White        |
                 */

                if (color.R >= 128 && color.G == 0 && color.B == 0)
                {
                    // this is a colored pixel
                    // according to LUT, no need to change the pixel value in B/W buffer.
                    // red frame buffer starts with 0x00. ORing it to set red pixel to 1.
                    _frameBuffer2bpp.ColorBuffer[pageByteIndex] |= (byte)(128 >> (x & 7));
                }
                else if (color.R == 0 && color.G == 0 && color.B == 0)
                {
                    // black pixel
                    _frameBuffer2bpp.BlackBuffer[pageByteIndex] &= (byte)~(128 >> (x & 7));
                }
                else
                {
                    // assume white if R, G, and B > 0
                    _frameBuffer2bpp.BlackBuffer[pageByteIndex] |= (byte)(128 >> (x & 7));
                }
            }
        }

        /// <summary>
        /// Draws the specified buffer directly to the Black/White RAM on the display.
        /// Call <see cref="Ssd168x.PerformFullRefresh"/> after to update the display.
        /// </summary>
        /// <param name="startXPos">X start position.</param>
        /// <param name="startYPos">Y start position.</param>
        /// <param name="buffer">The buffer array to draw.</param>
        public void DirectDrawBuffer(int startXPos, int startYPos, params byte[] buffer)
        {
            SetPosition(startXPos, startYPos);

            SendCommand((byte)Command.WriteBackWhiteRAM);
            SendData(buffer);
        }

        /// <summary>
        /// Draws the specified buffer directly to the Red RAM on the display.
        /// Call <see cref="Ssd168x.PerformFullRefresh"/> after to update the display.
        /// </summary>
        /// <param name="startXPos">X start position.</param>
        /// <param name="startYPos">Y start position.</param>
        /// <param name="buffer">The buffer array to draw.</param>
        public void DirectDrawColorBuffer(int startXPos, int startYPos, params byte[] buffer)
        {
            SetPosition(startXPos, startYPos);

            SendCommand((byte)Command.WriteRedRAM);
            SendData(buffer);
        }

        /// <summary>
        /// Initializes a new instance of the intenral frame buffer. Supports paging.
        /// </summary>
        /// <param name="width">The width of the frame buffer in pixels.</param>
        /// <param name="height">The height of the frame buffer in pixels.</param>
        /// <param name="enableFramePaging">If <see langword="true"/>, enables paging the frame.</param>
        protected override void InitializeFrameBuffer(int width, int height, bool enableFramePaging)
        {
            var frameBufferHeight = enableFramePaging ? height / PagesPerFrame : height;
            _frameBuffer2bpp = new FrameBuffer2BitPerPixel(frameBufferHeight, width);
        }

        /// <summary>
        /// Sets the current active frame buffer page to the specified page index.
        /// Existing frame buffer is reused by clearing it first and page bounds are recalculated.
        /// Make sure to call <see cref="Ssd168x.PerformFullRefresh"/> or <see cref="Ssd168x.PerformPartialRefresh"/>
        /// to persist the frame to the display's RAM before calling this method.
        /// </summary>
        /// <param name="page">The frame buffer page index to move to.</param>
        protected override void SetFrameBufferPage(int page)
        {
            if (page < 0 || page >= PagesPerFrame)
            {
                page = 0;
            }

            _frameBuffer2bpp.Clear();
            CurrentFrameBufferPage = page;

            CalculateFrameBufferPageBounds();
        }
    }
}
