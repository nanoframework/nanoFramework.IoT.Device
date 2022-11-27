// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Drawing;
using System.Threading;

using Iot.Device.EPaper.Buffers;
using Iot.Device.EPaper.Drivers.Ssd168x.Ssd1681;
using Iot.Device.EPaper.Enums;
using Iot.Device.EPaper.Primitives;

namespace Iot.Device.EPaper.Drivers.Ssd168x
{
    /// <summary>
    /// Base class for SSD168X-based ePaper devices.
    /// </summary>
    public abstract class Ssd168x : IEPaperDisplay
    {
        private SpiDevice _spiDevice;
        private GpioController _gpioController;
        private GpioPin _resetPin;
        private GpioPin _busyPin;
        private GpioPin _dataCommandPin;

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

        private bool _shouldDispose;
        private bool _disposed;

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
            if (width <= 0 || width > 200)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height <= 0 || height > 200)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

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

            InitializeFrameBuffer(width, height, enableFramePaging);
            CalculateFrameBufferPageBounds();
        }

        /// <inheritdoc/>
        public virtual int Width { get; protected set; }

        /// <inheritdoc/>
        public virtual int Height { get; protected set; }

        /// <inheritdoc/>
        public abstract IFrameBuffer FrameBuffer { get; protected set; }

        /// <inheritdoc/>
        public virtual bool PagedFrameDrawEnabled { get; protected set; }

        /// <summary>
        /// Gets or sets the current power state of the display panel.
        /// </summary>
        public virtual PowerState PowerState { get; protected set; }

        /// <inheritdoc/>
        public abstract void BeginFrameDraw();

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
        public abstract void Clear(bool triggerPageRefresh = false);

        /// <inheritdoc/>
        public abstract void DrawPixel(int x, int y, Color color);

        /// <inheritdoc/>
        public abstract void EndFrameDraw();

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
        public abstract void Flush();

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
        public abstract void Initialize();

        /// <summary>
        /// Initializes a new instance of the intenral frame buffer. Supports paging.
        /// </summary>
        /// <param name="width">The width of the frame buffer in pixels.</param>
        /// <param name="height">The height of the frame buffer in pixels.</param>
        /// <param name="enableFramePaging">If <see langword="true"/>, enables paging the frame.</param>
        protected abstract void InitializeFrameBuffer(int width, int height, bool enableFramePaging);

        /// <inheritdoc/>
        public abstract bool NextFramePage();

        /// <inheritdoc/>
        public virtual void PerformFullRefresh()
        {
            SendCommand((byte)Command.BoosterSoftStartControl);
            SendData(0x8b, 0x9c, 0x96, 0x0f);

            SendCommand((byte)Command.DisplayUpdateControl2);
            SendData((byte)RefreshMode.FullRefresh); // Display Mode 1 (Full Refresh)

            SendCommand((byte)Command.MasterActivation);
            WaitReady();
        }

        /// <inheritdoc/>
        public virtual void PerformPartialRefresh()
        {
            SendCommand((byte)Command.BoosterSoftStartControl);
            SendData(0x8b, 0x9c, 0x96, 0x0f);

            SendCommand((byte)Command.DisplayUpdateControl2);
            SendData((byte)RefreshMode.PartialRefresh); // Display Mode 2 (Partial Refresh)

            SendCommand((byte)Command.MasterActivation);
            WaitReady();
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
        protected abstract void SetFrameBufferPage(int page);

        /// <inheritdoc/>
        public virtual void SetPosition(int x, int y)
        {
            EnforceBounds(ref x, ref y);

            SendCommand((byte)Command.SetRAMAddressCounterX);
            SendData((byte)(x / 8)); // each row can have up to 200 points each represented by a single bit.

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
        public virtual void WaitReady()
        {
            while (_busyPin.Read() == PinValue.High)
            {
                WaitMs(5);
            }
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
