// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Drawing;
using System.Threading;
using Iot.Device.EPaper.Buffers;
using Iot.Device.EPaper.Enums;
using nanoFramework.UI;

namespace Iot.Device.EPaper.Drivers.GDEW0154x
{
    /// <summary>
    /// Base class for GDEW0154-based ePaper devices.
    /// </summary>
    public abstract class Gdew0154x : IEPaperDisplay
    {
        private readonly int _maxWaitingTime = 500;
        private readonly bool _shouldDispose;
        private readonly byte[] _whiteBuffer;
        private bool _disposed;

        private SpiDevice _spiDevice;
        private GpioController _gpioController;
        private GpioPin _resetPin;
        private GpioPin _busyPin;
        private GpioPin _dataCommandPin;

        /// <summary>
        /// The max supported clock frequency for the GDEW0154x controller. 10MHz.
        /// </summary>
        public const int SpiClockFrequency = 10_000_000;

        /// <summary>
        /// The supported <see cref="System.Device.Spi.SpiMode"/> by the GDEW0154x controller.
        /// </summary>
        public const SpiMode SpiMode = System.Device.Spi.SpiMode.Mode0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Gdew0154x"/> class.
        /// </summary>
        /// <param name="spiDevice">The communication channel to the SSD1681-based dispay.</param>
        /// <param name="resetPin">The reset GPIO pin. Passing an invalid pin number such as -1 will prevent this driver from opening the pin. Caller should handle hardware resets.</param>
        /// <param name="busyPin">The busy GPIO pin.</param>
        /// <param name="dataCommandPin">The data/command GPIO pin.</param>
        /// <param name="width">The width of the display.</param>
        /// <param name="height">The height of the display.</param>
        /// <param name="gpioController">The <see cref="GpioController"/> to use when initializing the pins.</param>
        /// <param name="enableFramePaging">Page the frame buffer and all operations to use less memory.</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller.</param>
        /// <exception cref="ArgumentNullException"><paramref name="spiDevice"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Display width and height can't be less than 0 or greater than 200.</exception>
        protected Gdew0154x(
            SpiDevice spiDevice,
            int resetPin,
            int busyPin,
            int dataCommandPin,
            int width,
            int height,
            GpioController gpioController,
            bool enableFramePaging = false,
            bool shouldDispose = true)
        {
            _spiDevice = spiDevice ?? throw new ArgumentNullException(nameof(spiDevice));
            _gpioController = gpioController ?? new GpioController();
            _shouldDispose = shouldDispose || gpioController is null;

            // setup the gpio pins
            _resetPin = resetPin >= 0 ? _gpioController.OpenPin(resetPin, PinMode.Output) : null;
            _dataCommandPin = _gpioController.OpenPin(dataCommandPin, PinMode.Output);
            _busyPin = _gpioController.OpenPin(busyPin, PinMode.Input);

            Width = width;
            Height = height;
            PagedFrameDrawEnabled = enableFramePaging;

            _whiteBuffer = new byte[Width * Height];
            for (int i = 0; i < _whiteBuffer.Length; i++)
            {
                _whiteBuffer[i] = 0xff;
            }

            PowerState = PowerState.Unknown;

            InitializeFrameBuffer(width, height, enableFramePaging);
            CalculateFrameBufferPageBounds();
            FrameBuffer1bpp.Clear(Color.White);
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
                return FrameBuffer1bpp;
            }

            protected set
            {
                FrameBuffer1bpp = (FrameBuffer1BitPerPixel)value;
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
        /// Gets or sets the <see cref="FrameBuffer1BitPerPixel"/> used internally by <see cref="GDEW0154x"/> devices to represents the frame.
        /// </summary>
        protected FrameBuffer1BitPerPixel FrameBuffer1bpp { get; set; }

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
        /// <param name="buffer">The buffer array to draw.</param>
        public void DirectDrawBuffer(params byte[] buffer)
        {
            SendCommand((byte)Command.DataStartTransmission1);
            SendData(_whiteBuffer);
            SendCommand((byte)Command.DataStartTransmission2);
            SendData(buffer);
            WaitMs(5);

            PerformFullRefresh();
        }

        /// <summary>
        /// Draws a single pixel to the appropriate frame buffer.
        /// </summary>
        /// <param name="x">The X Position.</param>
        /// <param name="y">The Y Position.</param>
        /// <param name="inverted">True to invert the pixel from white to black.</param>
        /// <remarks>
        /// The GDEW0154x comes with 2 RAMs: a Black and White RAM and a Red RAM.
        /// Writing to the B/W RAM draws B/W pixels on the panel. While writing to the Red RAM draws red pixels on the panel (if the panel supports red).
        /// However, the GDEW0154x doesn't support specifying the color level (no grayscaling), therefore the way the buffer is selected 
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
        /// The GDEW0154x comes with 2 RAMs: a Black and White RAM and a Red RAM.
        /// Writing to the B/W RAM draws B/W pixels on the panel. While writing to the Red RAM draws red pixels on the panel (if the panel supports red).
        /// However, the GDEW0154x doesn't support specifying the color level (no grayscaling), therefore the way the buffer is selected 
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
                 * Lookup Table for colors on GDEW0154x
                 * 
                 *  LUT for Black and White ePaper display with GDEW0154x
                 * |                |                    |
                 * |  Data B/W RAM  | Result Pixel Color |
                 * |----------------|--------------------|
                 * |        0       |       Black        |
                 * |        1       |       White        |
                 */

                if (color == Color.Black)
                {
                    // black pixel
                    FrameBuffer1bpp.Buffer[pageByteIndex] &= (byte)~(128 >> (x & 7));
                }
                else
                {
                    // white pixel
                    FrameBuffer1bpp.Buffer[pageByteIndex] |= (byte)(128 >> (x & 7));
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
            // write B/W frame to the display's RAM.
            DirectDrawBuffer(FrameBuffer1bpp.Buffer);
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

            _resetPin.Write(PinValue.High);
            WaitMs(10);

            _resetPin.Write(PinValue.Low);
            WaitMs(10);

            _resetPin.Write(PinValue.High);

            // as per samples from screen manufacturer, wait finally 100ms for reset IC + select BUS
            WaitMs(100);
        }

        /// <summary>
        /// Perform the required initialization steps to set up the display.
        /// </summary>
        public virtual void Initialize()
        {
            if (PowerState == PowerState.DeepSleep)
            {
                // When in deep sleep mode, hardware init is required to set the screen on stand-by.
                HardwareReset();
            }

            SendCommand((byte)Command.PanelSetting);
            SendData(0xdf, 0x0e);

            // FITIinternal code
            SendCommand(0x4d);
            SendData(0x55);

            SendCommand(0xaa);
            SendData(0x0f);

            SendCommand(0xe9);
            SendData(0x02);

            SendCommand(0xb6);
            SendData(0x11);

            SendCommand(0xf3);
            SendData(0x0a);

            SendCommand((byte)Command.ResolutionSetting);
            SendData(0xc8, 0x00, 0xc8);

            SendCommand((byte)Command.TCONSetting);
            SendData(0x00);

            SendCommand((byte)Command.PowerSaving);
            SendData(0x00);

            SendCommand((byte)Command.PowerOn);

            if (WaitReady(_maxWaitingTime))
            {
                PowerState = PowerState.PoweredOn;
            }
            else
            {
                // Set to deep sleep to force an hardware reset on next request
                PowerState = PowerState.DeepSleep;
            }
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
            FrameBuffer1bpp = new FrameBuffer1BitPerPixel(frameBufferHeight, width);
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
        public virtual void PerformFullRefresh()
        {
            SendCommand((byte)Command.DisplayRefresh);

            // as per samples from screen manufacturer, refresh wait should be at least 200µs
            // using a spin wait of 5ms, any value should be ok
            // and use a large waiting time in case of something unexpected happens.
            WaitReady(_maxWaitingTime);
        }

        /// <inheritdoc/>
        public virtual void PerformPartialRefresh()
        {
            // No partial refresh implemented
            PerformFullRefresh();
        }

        /// <summary>
        /// Puts the display to sleep using the specified <see cref="SleepMode"/>.
        /// </summary>
        /// <param name="sleepMode">The sleep mode to use when powering down the display.</param>
        public virtual void PowerDown(SleepMode sleepMode = SleepMode.Normal)
        {
            SendCommand(0x50);
            SendData(0x07);
            SendCommand((byte)Command.PowerOff);
            WaitReady(_maxWaitingTime);

            // as per samples from screen manufacturer, power off request a time delay of 1s before continue.
            WaitMs(1000);

            if (sleepMode == SleepMode.DeepSleepMode)
            {
                SendCommand((byte)Command.DeepSleepMode);
                SendData(0xa5);
                PowerState = PowerState.DeepSleep;
            }
            else
            {
                PowerState = PowerState.PoweredOff;
            }
        }

        /// <summary>
        /// Performs the required steps to "power on" the display.
        /// </summary>
        public virtual void PowerOn()
        {
            if (PowerState == PowerState.DeepSleep)
            {
                // When in deep sleep mode, hardware init is required to set the screen on stand-by.
                HardwareReset();
            }

            SendCommand(0x50);
            SendData(0xd7);
            SendCommand((byte)Command.PowerOn);
            WaitReady(_maxWaitingTime);

            PowerState = PowerState.PoweredOn;
        }

        /// <inheritdoc/>
        public virtual void SendCommand(params byte[] command)
        {
            // make sure we are setting data/command pin to low (command mode)
            _dataCommandPin.Write(PinValue.Low);

            // write the command byte to the display controller
            _spiDevice.Write(command);
        }

        /// <inheritdoc/>
        public virtual void SendData(params byte[] data)
        {
            // set the data/command pin to high to indicate to the display we will be sending data
            _dataCommandPin.Write(PinValue.High);

            _spiDevice.Write(data);

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

            FrameBuffer1bpp.Clear();
            CurrentFrameBufferPage = page;

            CalculateFrameBufferPageBounds();
        }

        /// <inheritdoc/>
        public virtual void SetPosition(int x, int y)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Performs the software reset commands sequence on the display.
        /// </summary>
        protected virtual void SoftwareReset()
        {
            SendCommand(0x04);

            WaitReady(_maxWaitingTime);
            WaitMs(10);
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
        public virtual bool WaitReady(int waitingTime)
        {
            int currentWait = 0;
            while (currentWait < waitingTime && _busyPin.Read() == PinValue.Low)
            {
                SpinWait.SpinUntil(5);
                currentWait += 5;
            }

            return currentWait >= waitingTime;
        }

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
