// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

using Iot.Device.EPaper.Buffers;
using Iot.Device.EPaper.Enums;
using Iot.Device.EPaper.Utilities;

namespace Iot.Device.EPaper.Drivers.Ssd168x
{
    /// <summary>
    /// Base class for SSD168X-based ePaper devices.
    /// </summary>
    public abstract class Ssd168x : IEPaperDisplay
    {
        private readonly bool _shouldDispose;
        private SpiDevice _spiDevice;
        private GpioController _gpioController;
        private GpioPin _resetPin;
        private GpioPin _busyPin;
        private GpioPin _dataCommandPin;

        private bool _disposed;

        /// <summary>
        /// The max supported clock frequency for the SSD168x controller. 20MHz.
        /// </summary>
        public const int SpiClockFrequency = 20_000_000;

        /// <summary>
        /// The supported <see cref="System.Device.Spi.SpiMode"/> by the SSD168x controller.
        /// </summary>
        public const SpiMode SpiMode = System.Device.Spi.SpiMode.Mode0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ssd168x"/> class.
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
        protected Ssd168x(
            SpiDevice spiDevice,
            int resetPin,
            int busyPin,
            int dataCommandPin,
            int width,
            int height,
            GpioController gpioController = null,
            bool enableFramePaging = true,
            bool shouldDispose = true)
        {
            this._spiDevice = spiDevice ?? throw new ArgumentNullException(nameof(spiDevice));
            this._gpioController = gpioController ?? new GpioController();

            // setup the gpio pins
            this._resetPin = resetPin >= 0 ? gpioController.OpenPin(resetPin, PinMode.Output) : null;
            this._dataCommandPin = gpioController.OpenPin(dataCommandPin, PinMode.Output);
            this._busyPin = gpioController.OpenPin(busyPin, PinMode.Input);

            this._shouldDispose = shouldDispose || gpioController == null;

            Width = width;
            Height = height;
            PagedFrameDrawEnabled = enableFramePaging;

            PowerState = PowerState.Unknown;

            Debug.WriteLine($"Busy Pin Status: {_busyPin.Read().ToString()}");

            InitializeFrameBuffer(width, height, enableFramePaging);
            CalculateFrameBufferPageBounds();
        }

        /// <inheritdoc/>
        public virtual int Width { get; protected set; }

        /// <inheritdoc/>
        public virtual int Height { get; protected set; }

        /// <inheritdoc/>
        public virtual IFrameBuffer FrameBuffer
        {
            get
            {
                return FrameBuffer2bpp;
            }

            protected set
            {
                FrameBuffer2bpp = (FrameBuffer2BitPerPixel)value;
            }
        }

        /// <inheritdoc/>
        public virtual bool PagedFrameDrawEnabled { get; protected set; }

        /// <summary>
        /// Gets or sets the current power state of the display panel.
        /// </summary>
        public virtual PowerState PowerState { get; protected set; }

        /// <summary>
        /// Gets or sets the current frame buffer page index.
        /// </summary>
        protected int CurrentFrameBufferPage { get; set; }

        /// <summary>
        /// Gets or sets the current frame buffer page lower bounds.
        /// </summary>
        protected int CurrentFrameBufferPageLowerBound { get; set; }

        /// <summary>
        /// Gets or sets the current frame buffer page upper bounds.
        /// </summary>
        protected int CurrentFrameBufferPageUpperBound { get; set; }

        /// <summary>
        /// Gets or sets the current frame buffer start X-Position.
        /// </summary>
        protected int CurrentFrameBufferStartXPosition { get; set; }

        /// <summary>
        /// Gets or sets the current frame buffer start Y-Position.
        /// </summary>
        protected int CurrentFrameBufferStartYPosition { get; set; }

        /// <summary>
        /// Gets the number of pages in every frame buffer.
        /// </summary>
        protected abstract int PagesPerFrame { get; }

        /// <summary>
        /// Gets or sets the <see cref="FrameBuffer2BitPerPixel"/> used internally by <see cref="Ssd168x"/> devices to represents the frame.
        /// </summary>
        protected FrameBuffer2BitPerPixel FrameBuffer2bpp { get; set; }

        /// <summary>
        /// Gets the index of the first frame page.
        /// </summary>
        protected int FirstPageIndex { get; } = 0;

        /// <inheritdoc/>
        public virtual void BeginFrameDraw()
        {
            // make sure we start from the first page with clear buffers
            SetFrameBufferPage(FirstPageIndex);
        }

        /// <summary>
        /// Calculates the upper and lower bounds of the current frame buffer page.
        /// </summary>
        protected virtual void CalculateFrameBufferPageBounds()
        {
            CurrentFrameBufferPageLowerBound = CurrentFrameBufferPage * FrameBuffer.BufferByteCount / FrameBuffer.BitDepth;
            CurrentFrameBufferPageUpperBound = (CurrentFrameBufferPage + 1) * FrameBuffer.BufferByteCount / FrameBuffer.BitDepth;

            CurrentFrameBufferStartXPosition = GetXPositionFromFrameBufferIndex(CurrentFrameBufferPageLowerBound);
            CurrentFrameBufferStartYPosition = GetYPositionFromFrameBufferIndex(CurrentFrameBufferPageLowerBound);

            FrameBuffer.StartPoint = new Point(CurrentFrameBufferStartXPosition, CurrentFrameBufferStartYPosition);
            FrameBuffer.CurrentFramePage = CurrentFrameBufferPage;
        }

        /// <inheritdoc/>
        public virtual void Clear(bool triggerPageRefresh = false)
        {
            SetFrameBufferPage(FirstPageIndex);

            // paging is enabled, flush as per number of pages to ensure all display RAM is cleared
            if (PagedFrameDrawEnabled)
            {
                do
                {
                    Flush();

                    // sleep for 20ms to allow other threads to have their chance to execute
                    Thread.Sleep(20);

                    CurrentFrameBufferPage++;
                    CalculateFrameBufferPageBounds();
                }
                while (CurrentFrameBufferPage < PagesPerFrame);

                CurrentFrameBufferPage = FirstPageIndex;
                CalculateFrameBufferPageBounds();
            }
            else
            {
                // paging is disabled, so the internal frame covers the entire display frame. we only need to flush once.
                Flush();
            }

            if (triggerPageRefresh)
            {
                PerformFullRefresh();
            }
        }

        /// <summary>
        /// Draws the specified buffer directly to the Black/White RAM on the display.
        /// Call <see cref="PerformFullRefresh"/> after to update the display.
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
        /// Call <see cref="PerformFullRefresh"/> after to update the display.
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
        /// Draws a single pixel to the appropriate frame buffer.
        /// </summary>
        /// <param name="x">The X Position.</param>
        /// <param name="y">The Y Position.</param>
        /// <param name="inverted">True to invert the pixel from white to black.</param>
        /// <remarks>
        /// The SSD168x comes with 2 RAMs: a Black and White RAM and a Red RAM.
        /// Writing to the B/W RAM draws B/W pixels on the panel. While writing to the Red RAM draws red pixels on the panel (if the panel supports red).
        /// However, the SSD168x doesn't support specifying the color level (no grayscaling), therefore the way the buffer is selected 
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
        /// The SSD168x comes with 2 RAMs: a Black and White RAM and a Red RAM.
        /// Writing to the B/W RAM draws B/W pixels on the panel. While writing to the Red RAM draws red pixels on the panel (if the panel supports red).
        /// However, the SSD168x doesn't support specifying the color level (no grayscaling), therefore the way the buffer is selected 
        /// is by performing a simple binary check: 
        /// if R >= 128 and G == 0 and B == 0 then write a red pixel to the Red Buffer/RAM
        /// if R == 0 and G == 0 and B == 0 then write a black pixel to B/W Buffer/RAM
        /// else, assume white pixel and write to B/W Buffer/RAM.
        /// </remarks>
        public virtual void DrawPixel(int x, int y, Color color)
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
                 * Lookup Table for colors on SSD168x
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
                 *  LUT for Black and White ePaper display with SSD168x
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
                    FrameBuffer2bpp.ColorBuffer[pageByteIndex] |= (byte)(128 >> (x & 7));
                }
                else if (color.R == 0 && color.G == 0 && color.B == 0)
                {
                    // black pixel
                    FrameBuffer2bpp.BlackBuffer[pageByteIndex] &= (byte)~(128 >> (x & 7));
                }
                else
                {
                    // assume white if R, G, and B > 0
                    FrameBuffer2bpp.BlackBuffer[pageByteIndex] |= (byte)(128 >> (x & 7));
                }
            }
        }

        /// <inheritdoc />
        public virtual void EndFrameDraw()
        {
            Flush();
        }

        /// <summary>
        /// Snaps the provided coordinates to the lower bounds of the display if out of allowed range.
        /// </summary>
        /// <param name="x">The X position.</param>
        /// <param name="y">The Y position.</param>
        protected virtual void EnforceBounds(ref int x, ref int y)
        {
            x = x < 0 || x > Width - 1 ? 0 : x;
            y = y < 0 || y > Height - 1 ? 0 : y;
        }

        /// <inheritdoc/>
        public virtual void Flush()
        {
            // write B/W and Color (RED) frame to the display's RAM.
            DirectDrawBuffer(
                CurrentFrameBufferStartXPosition,
                CurrentFrameBufferStartYPosition,
                FrameBuffer2bpp.BlackBuffer.Buffer);

            DirectDrawColorBuffer(
                CurrentFrameBufferStartXPosition,
                CurrentFrameBufferStartYPosition,
                FrameBuffer2bpp.ColorBuffer.Buffer);
        }

        /// <summary>
        /// Gets the index of the byte containing the pixel specified by the <paramref name="x"/> and <paramref name="y"/> parameters.
        /// </summary>
        /// <param name="x">The X position of the pixel.</param>
        /// <param name="y">The Y position of the pixel.</param>
        /// <returns>The index of the byte in the frame buffer which contains the specified pixel.</returns>
        protected virtual int GetFrameBufferIndex(int x, int y)
        {
            // x specifies the column
            return (x + (y * Width)) / 8;
        }

        /// <summary>
        /// Gets the X position from a buffer index.
        /// </summary>
        /// <param name="index">The buffer index.</param>
        /// <returns>The X position of a pixel.</returns>
        protected virtual int GetXPositionFromFrameBufferIndex(int index)
        {
            if (index <= 0)
            {
                return 0;
            }

            return (index * 8) % Width;
        }

        /// <summary>
        /// Gets the Y position from a buffer index.
        /// </summary>
        /// <param name="index">The buffer index.</param>
        /// <returns>The Y position of a pixel.</returns>
        protected virtual int GetYPositionFromFrameBufferIndex(int index)
        {
            if (index <= 0)
            {
                return 0;
            }

            return index * 8 / Height;
        }

        /// <summary>
        /// Performs the hardware reset commands sequence on the display.
        /// </summary>
        protected virtual void HardwareReset()
        {
            if (_resetPin == null)
            {
                // caller opted to reset outside of the driver by not passing a valid reset pin number.
                // do nothing.
                return;
            }

            // specs say to wait 10ms after supplying voltage to the display
            WaitMs(10);

            _resetPin.Write(PinValue.Low);
            WaitMs(200);

            _resetPin.Write(PinValue.High);
            WaitMs(200);
        }

        /// <summary>
        /// Perform the required initialization steps to set up the display.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to initialize the display until it has been powered on. Call PowerOn() first.</exception>
        public virtual void Initialize()
        {
            if (PowerState != PowerState.PoweredOn)
            {
                throw new InvalidOperationException();
            }

            // set gate lines and scanning sequence
            SendCommand((byte)Command.DriverOutputControl);

            // refer to the datasheet for a description of the parameters
            SendData((byte)Height, 0x00, 0x00);

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
            SendData(0x00, 0x00, (byte)(Height - 1), 0x00);

            // Set Panel Border
            SendCommand((byte)Command.BorderWaveformControl);
            SendData(0xc0);

            // Set Temperature sensor to use internal temp sensor
            SendCommand((byte)Command.TempSensorControlSelection);
            SendData(0x80);

            // Do a full refresh of the display
            PerformFullRefresh();
        }

        /// <summary>
        /// Initializes a new instance of the intenral frame buffer. Supports paging.
        /// </summary>
        /// <param name="width">The width of the frame buffer in pixels.</param>
        /// <param name="height">The height of the frame buffer in pixels.</param>
        /// <param name="enableFramePaging">If <see langword="true"/>, enables paging the frame.</param>
        protected virtual void InitializeFrameBuffer(int width, int height, bool enableFramePaging)
        {
            var frameBufferHeight = enableFramePaging ? height / PagesPerFrame : height;
            FrameBuffer2bpp = new FrameBuffer2BitPerPixel(frameBufferHeight, width);
        }

        /// <inheritdoc/>
        public virtual bool NextFramePage()
        {
            if (PagedFrameDrawEnabled && CurrentFrameBufferPage < PagesPerFrame - 1)
            {
                Flush();

                CurrentFrameBufferPage++;
                SetFrameBufferPage(CurrentFrameBufferPage);

                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public virtual bool PerformFullRefresh()
        {
            SendCommand((byte)Command.BoosterSoftStartControl);
            SendData(0x8b, 0x9c, 0x96, 0x0f);

            SendCommand((byte)Command.DisplayUpdateControl2);
            SendData((byte)RefreshMode.FullRefresh); // Display Mode 1 (Full Refresh)

            SendCommand((byte)Command.MasterActivation);
            return WaitReady();
        }

        /// <inheritdoc/>
        public virtual bool PerformPartialRefresh()
        {
            SendCommand((byte)Command.BoosterSoftStartControl);
            SendData(0x8b, 0x9c, 0x96, 0x0f);

            SendCommand((byte)Command.DisplayUpdateControl2);
            SendData((byte)RefreshMode.PartialRefresh); // Display Mode 2 (Partial Refresh)

            SendCommand((byte)Command.MasterActivation);
            return WaitReady();
        }

        /// <summary>
        /// Puts the display to sleep using the specified <see cref="SleepMode"/>.
        /// </summary>
        /// <param name="sleepMode">The sleep mode to use when powering down the display.</param>
        public virtual void PowerDown(SleepMode sleepMode = SleepMode.Normal)
        {
            SendCommand((byte)Command.DeepSleepMode);
            SendData((byte)sleepMode);

            PowerState = PowerState.PoweredOff;
        }

        /// <summary>
        /// Performs the required steps to "power on" the display.
        /// </summary>
        public virtual void PowerOn()
        {
            HardwareReset();
            SoftwareReset();

            PowerState = PowerState.PoweredOn;
        }

        /// <inheritdoc/>
        public virtual void SendCommand(params byte[] command)
        {
            // make sure we are setting data/command pin to low (command mode)
            _dataCommandPin.Write(PinValue.Low);

            foreach (var b in command)
            {
                // write the command byte to the display controller
                _spiDevice.WriteByte(b);
            }
        }

        /// <inheritdoc/>
        public virtual void SendData(params byte[] data)
        {
            // set the data/command pin to high to indicate to the display we will be sending data
            _dataCommandPin.Write(PinValue.High);

            foreach (var @byte in data)
            {
                _spiDevice.WriteByte(@byte);
            }

            // go back to low (command mode)
            _dataCommandPin.Write(PinValue.Low);
        }

        /// <summary>
        /// Sets the current active frame buffer page to the specified page index.
        /// Existing frame buffer is reused by clearing it first and page bounds are recalculated.
        /// Make sure to call <see cref="PerformFullRefresh"/> or <see cref="PerformPartialRefresh"/>
        /// to persist the frame to the display's RAM before calling this method.
        /// </summary>
        /// <param name="page">The frame buffer page index to move to.</param>
        protected virtual void SetFrameBufferPage(int page)
        {
            if (page < 0 || page >= PagesPerFrame)
            {
                page = 0;
            }

            FrameBuffer2bpp.Clear();
            CurrentFrameBufferPage = page;

            CalculateFrameBufferPageBounds();
        }

        /// <inheritdoc/>
        public virtual void SetPosition(int x, int y)
        {
            EnforceBounds(ref x, ref y);

            SendCommand((byte)Command.SetRAMAddressCounterX);
            SendData((byte)(x / 8));

            SendCommand((byte)Command.SetRAMAddressCounterY);
            SendData((byte)y);
        }

        /// <summary>
        /// Performs the software reset commands sequence on the display.
        /// </summary>
        protected virtual void SoftwareReset()
        {
            SendCommand((byte)Command.SoftwareReset);

            WaitReady();
        }

        /// <summary>
        /// A simple wait method that wraps <see cref="Thread.Sleep(int)"/>.
        /// </summary>
        /// <param name="milliseconds">Number of milliseconds to sleep.</param>
        protected virtual void WaitMs(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }

        /// <inheritdoc/>
        public virtual bool WaitReady(CancellationToken cancellationToken = default)
            => _busyPin.WaitUntilPinValueEquals(PinValue.Low, cancellationToken);

        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _resetPin?.Dispose();
                    _resetPin = null;

                    _busyPin?.Dispose();
                    _busyPin = null;

                    _dataCommandPin?.Dispose();
                    _dataCommandPin = null;

                    if (_shouldDispose)
                    {
                        _gpioController?.Dispose();
                    }

                    _spiDevice = null;
                    _gpioController = null;

                    FrameBuffer = null;
                }

                _disposed = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
