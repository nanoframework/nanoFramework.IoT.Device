// Copyright (c) 2024 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Drawing;
using System.Threading;
using Iot.Device.EPaper.Buffers;
using Iot.Device.EPaper.Utilities;

namespace Iot.Device.EPaper.Drivers.LcmEn2r13
{
    /// <summary>
    /// Driver for the LCMEN2R13EFC1 2.13" black-and-white e-paper display.
    /// </summary>
    /// <remarks>
    /// The visible panel is 122x250 pixels, while the controller RAM is byte-aligned to 128x250 pixels.
    /// This driver exposes the native controller RAM geometry so drawing and frame buffer layout remain consistent.
    /// The initialization and partial-refresh sequences are based on the LCMEN2R13EFC1 panel
    /// datasheet together with Heltec's HT_lCMEN2R13EFC1.h and HT_lCMEN2R13EFC1_LUT.h reference files.
    /// </remarks>
    public class LcmEn2r13 : IEPaperDisplay
    {
        /// <summary>
        /// Native controller RAM width in pixels.
        /// </summary>
        public const int PanelWidth = 128;

        /// <summary>
        /// Native controller RAM height in pixels.
        /// </summary>
        public const int PanelHeight = 250;

        /// <summary>
        /// Frame buffer size in bytes for a 1-bit-per-pixel image.
        /// </summary>
        public const int FrameBufferSize = PanelWidth * PanelHeight / 8;

        /// <summary>
        /// Maximum SPI clock frequency in hertz.
        /// </summary>
        public const int SpiClockFrequency = 6_000_000;

        /// <summary>
        /// SPI mode used by the controller.
        /// </summary>
        public const SpiMode SpiMode = System.Device.Spi.SpiMode.Mode0;

        /// <summary>
        /// Minimum wait when no busy GPIO is wired; full refresh can exceed 1.5s on this panel.
        /// </summary>
        private const int BusyPinFallbackWaitMs = 3500;

        // FITI/internal tuning commands mirrored from Heltec's HT_lCMEN2R13EFC1 reference driver.
        private const byte VendorCommandFitIInternalCode = 0x4D;
        private const byte VendorCommandAnalogBlockControl = 0xA9;
        private const byte VendorCommandFitIInternalControl = 0xF3;
        private const byte VendorCommandUnknownF8 = 0xF8;
        private const byte VendorCommandUnknownB3 = 0xB3;
        private const byte VendorCommandUnknownB4 = 0xB4;
        private const byte VendorCommandUnknownAa = 0xAA;
        private const byte VendorCommandUnknownA8 = 0xA8;
        private const byte VendorCommandPowerSaving = 0xE3;

        // Partial refresh LUT values copied from Heltec's HT_lCMEN2R13EFC1_LUT.h.
        private static readonly byte[] PartialLutVcom =
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x01, 0x83, 0x82, 0x43, 0x42, 0x01, 0x01, 0x01,
            0x03, 0x03, 0x00, 0x00, 0x01, 0x01,
        };

        private static readonly byte[] PartialLutWhiteToWhite =
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x01, 0x83, 0x82, 0x83, 0x82, 0x01, 0x01, 0x01,
            0x03, 0x83, 0x00, 0x00, 0x01, 0x01,
        };

        private static readonly byte[] PartialLutBlackToWhite =
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x01, 0x83, 0x82, 0x83, 0x82, 0x01, 0x01, 0x01,
            0x03, 0x83, 0x00, 0x00, 0x01, 0x01,
        };

        private static readonly byte[] PartialLutWhiteToBlack =
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x01, 0x43, 0x42, 0x43, 0x42, 0x01, 0x01, 0x01,
            0x43, 0x03, 0x00, 0x00, 0x01, 0x01,
        };

        private static readonly byte[] PartialLutBlackToBlack =
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x01, 0x43, 0x42, 0x43, 0x42, 0x01, 0x01, 0x01,
            0x43, 0x03, 0x00, 0x00, 0x01, 0x01,
        };

        private readonly SpiDevice _spiDevice;
        private readonly GpioController _gpioController;
        private readonly bool _shouldDispose;
        private readonly bool _useBusyPin;
        private readonly byte[] _previousFrame;
        private readonly byte[] _whiteFrame;

        private GpioPin _resetPin;
        private GpioPin _busyPin;
        private GpioPin _dataCommandPin;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="LcmEn2r13"/> class.
        /// </summary>
        /// <param name="spiDevice">SPI device used to communicate with the display.</param>
        /// <param name="resetPin">GPIO pin number for reset, or -1 if reset is not used.</param>
        /// <param name="busyPin">GPIO pin number for busy, or -1 if busy monitoring is not available.</param>
        /// <param name="dataCommandPin">GPIO pin number for data/command selection.</param>
        /// <param name="gpioController">GPIO controller instance, or null to create a new controller.</param>
        /// <param name="shouldDispose">True to dispose the provided GPIO controller when the driver is disposed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="spiDevice"/> is null.</exception>
        public LcmEn2r13(SpiDevice spiDevice, int resetPin, int busyPin, int dataCommandPin, GpioController gpioController = null, bool shouldDispose = true)
        {
            if (spiDevice == null)
            {
                throw new ArgumentNullException(nameof(spiDevice));
            }

            _spiDevice = spiDevice;
            _gpioController = gpioController ?? new GpioController();
            _shouldDispose = shouldDispose || gpioController is null;

            if (resetPin >= 0)
            {
                _resetPin = _gpioController.OpenPin(resetPin, PinMode.Output);
                _resetPin.Write(PinValue.High);
            }

            _dataCommandPin = _gpioController.OpenPin(dataCommandPin, PinMode.Output);
            _dataCommandPin.Write(PinValue.Low);

            _useBusyPin = busyPin >= 0;
            if (_useBusyPin)
            {
                _busyPin = _gpioController.OpenPin(busyPin, PinMode.Input);
            }

            _whiteFrame = new byte[FrameBufferSize];
            _previousFrame = new byte[FrameBufferSize];
            for (int i = 0; i < _whiteFrame.Length; i++)
            {
                _whiteFrame[i] = 0xFF;
                _previousFrame[i] = 0xFF;
            }

            FrameBuffer1bpp = new FrameBuffer1BitPerPixel(PanelHeight, PanelWidth);
            FrameBuffer1bpp.Clear(Color.White);
        }

        /// <summary>
        /// Gets the native panel width in pixels.
        /// </summary>
        public int Width => PanelWidth;

        /// <summary>
        /// Gets the native panel height in pixels.
        /// </summary>
        public int Height => PanelHeight;

        /// <summary>
        /// Gets the active frame buffer used for drawing.
        /// </summary>
        public IFrameBuffer FrameBuffer => FrameBuffer1bpp;

        /// <summary>
        /// Gets a value indicating whether paged frame drawing is supported.
        /// </summary>
        public bool PagedFrameDrawEnabled => false;

        /// <summary>
        /// Gets the internal 1-bit-per-pixel frame buffer.
        /// </summary>
        protected FrameBuffer1BitPerPixel FrameBuffer1bpp { get; }

        /// <summary>
        /// Begins a new frame draw operation by clearing the frame buffer to white.
        /// </summary>
        public void BeginFrameDraw()
        {
            FrameBuffer1bpp.Clear(Color.White);
        }

        /// <summary>
        /// Advances to the next frame page.
        /// </summary>
        /// <returns>Always returns false because paged drawing is not supported.</returns>
        public bool NextFramePage()
        {
            return false;
        }

        /// <summary>
        /// Ends the frame draw operation by flushing the frame buffer.
        /// </summary>
        public void EndFrameDraw()
        {
            Flush();
        }

        /// <summary>
        /// Draws a single pixel into the internal frame buffer using native panel coordinates.
        /// </summary>
        /// <param name="x">The horizontal pixel coordinate.</param>
        /// <param name="y">The vertical pixel coordinate.</param>
        /// <param name="color">The pixel color.</param>
        public void DrawPixel(int x, int y, Color color)
        {
            if (x < 0 || x >= PanelWidth || y < 0 || y >= PanelHeight)
            {
                return;
            }

            int byteIndex = GetFrameBufferIndex(x, y);
            int bitMask = 0x80 >> (x & 7);

            if (color == Color.Black)
            {
                FrameBuffer1bpp.Buffer[byteIndex] &= (byte)~bitMask;
            }
            else
            {
                FrameBuffer1bpp.Buffer[byteIndex] |= (byte)bitMask;
            }
        }

        /// <summary>
        /// Flushes the internal frame buffer to display RAM.
        /// </summary>
        public void Flush()
        {
            PrepareForWrite();
            SetRamPointerToOrigin();

            SendCommand((byte)Command.WriteCurrentImage);
            SendData(FrameBuffer1bpp.Buffer);

            SendCommand((byte)Command.WritePreviousImage);
            SendData(_previousFrame);
        }

        /// <summary>
        /// Sets the current drawing position.
        /// </summary>
        /// <param name="x">The horizontal position.</param>
        /// <param name="y">The vertical position.</param>
        /// <remarks>
        /// Positioned streaming writes are not used by this driver; callers should use <see cref="DrawPixel"/> or <see cref="Graphics"/>.
        /// </remarks>
        public void SetPosition(int x, int y)
        {
        }

        /// <summary>
        /// Performs a partial refresh of the display.
        /// </summary>
        /// <returns>True if the refresh command sequence was issued successfully.</returns>
        public bool PerformPartialRefresh()
        {
            // Heltec's dis_img_Partial_Refresh() sequence switches the controller to register-LUT
            // mode, configures the partial window, loads the LUT tables, then triggers a refresh
            // using the old/new image RAM contents previously written by Flush().
            SendCommand((byte)Command.PanelSetting);
            SendData(0xDF, 0x08);

            SendCommand((byte)Command.PartialIn);
            SendCommand((byte)Command.PartialWindow);
            SendData(0x00, 0x80, 0x00, 0xFA, 0x00);
            SendCommand((byte)Command.PartialOut);

            WritePartialRefreshLuts();

            SendCommand((byte)Command.CascadeSetting);
            SendData(0x02);

            SendCommand((byte)Command.ForceTemperature);
            SendData(0x75);

            SendCommand((byte)Command.PowerOn);
            WaitReady();
            WaitMs(100);

            SendCommand((byte)Command.DisplayRefresh);
            WaitMs(100);
            WaitReady();

            SendCommand((byte)Command.PowerOff);
            WaitReady();
            WaitMs(100);

            UpdatePreviousFrame();

            return true;
        }

        /// <summary>
        /// Performs a hardware reset of the display.
        /// </summary>
        public void HardwareReset()
        {
            if (_resetPin == null)
            {
                return;
            }

            _resetPin.Write(PinValue.High);
            WaitMs(20);
            _resetPin.Write(PinValue.Low);
            WaitMs(20);
            _resetPin.Write(PinValue.High);
            WaitMs(100);
        }

        /// <summary>
        /// Powers on and initializes the display controller.
        /// </summary>
        public void PowerOn()
        {
            HardwareReset();

            SendCommand((byte)Command.DisplayRefresh);
            WaitReady();

            // Initialization sequence: register values and order match Heltec sendInitCommands() for this panel;
            // cross-check command bytes with the LCMEN2R13EFC1 / UC8151 datasheet register descriptions.
            SendCommand((byte)Command.PanelSetting);
            SendData(0xDF);

            SendCommand(VendorCommandFitIInternalCode);
            SendData(0x55, 0x00, 0x00);

            SendCommand(VendorCommandAnalogBlockControl);
            SendData(0x25, 0x00, 0x00);

            SendCommand(VendorCommandFitIInternalControl);
            SendData(0x0A, 0x00, 0x00);

            SendCommand((byte)Command.SetXAddressRange);
            SendData(0x01, 0x0F);

            SendCommand((byte)Command.SetYAddressRange);
            SendData(0xF9, 0x00, 0x00, 0x00);

            SendCommand(0x3C);
            SendData(0x01);

            SendCommand((byte)Command.TemperatureSensorControl);
            SendData(0x80);

            SendCommand((byte)Command.SetXAddressCounter);
            SendData(0x01);

            SendCommand((byte)Command.SetYAddressCounter);
            SendData(0xF9, 0x00);

            WaitReady();
        }

        /// <summary>
        /// Clears the display buffer to white and optionally refreshes the panel.
        /// </summary>
        /// <param name="triggerPageRefresh">True to trigger a full panel refresh after clearing.</param>
        public void Clear(bool triggerPageRefresh = false)
        {
            FrameBuffer1bpp.Clear(Color.White);

            PrepareForWrite();
            SetRamPointerToOrigin();

            SendCommand((byte)Command.WriteCurrentImage);
            SendData(_whiteFrame);

            SendCommand((byte)Command.WritePreviousImage);
            SendData(_whiteFrame);

            // Keep shadow previous RAM in sync so Flush()/partial refresh see the same old/new pair as the panel.
            UpdatePreviousFrame();

            if (triggerPageRefresh)
            {
                PerformFullRefresh();
            }
        }

        /// <summary>
        /// Performs a full display refresh.
        /// </summary>
        /// <returns>True if the refresh command sequence was issued successfully.</returns>
        public bool PerformFullRefresh()
        {
            SendCommand((byte)Command.PowerOn);
            WaitReady();
            WaitMs(100);

            SendCommand((byte)Command.DisplayRefresh);
            WaitMs(100);
            WaitReady();

            SendCommand((byte)Command.PowerOff);
            WaitReady();
            WaitMs(100);
            UpdatePreviousFrame();

            return true;
        }

        /// <summary>
        /// Powers down the display and enters deep sleep mode.
        /// </summary>
        public void PowerDown()
        {
            SendCommand((byte)Command.PowerOff);
            WaitReady();

            SendCommand((byte)Command.DeepSleep);
            SendData(0xA5);
        }

        /// <summary>
        /// Sends one or more command bytes to the display.
        /// </summary>
        /// <param name="command">The command bytes to send.</param>
        public void SendCommand(params byte[] command)
        {
            _dataCommandPin.Write(PinValue.Low);
            _spiDevice.Write(command);
        }

        /// <summary>
        /// Sends one or more data bytes to the display.
        /// </summary>
        /// <param name="data">The data bytes to send.</param>
        public void SendData(params byte[] data)
        {
            _dataCommandPin.Write(PinValue.High);
            _spiDevice.Write(data);
            _dataCommandPin.Write(PinValue.Low);
        }

        /// <summary>
        /// Sends one or more 16-bit data values to the display.
        /// </summary>
        /// <param name="data">The 16-bit data values to send.</param>
        public void SendData(params ushort[] data)
        {
            _dataCommandPin.Write(PinValue.High);
            _spiDevice.Write(data);
            _dataCommandPin.Write(PinValue.Low);
        }

        /// <summary>
        /// Waits until the display controller reports ready.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for the wait operation.</param>
        /// <returns>True when the display is ready.</returns>
        /// <remarks>
        /// When a busy GPIO is used, this waits until BUSY is high (idle) for this UC8151-class controller.
        /// If no busy pin is available, <see cref="BusyPinFallbackWaitMs"/> is used instead.
        /// </remarks>
        public bool WaitReady(CancellationToken cancellationToken = default)
        {
            if (!_useBusyPin)
            {
                WaitMs(BusyPinFallbackWaitMs);
                return true;
            }

            return _busyPin.WaitUntilPinValueEquals(PinValue.High, cancellationToken);
        }

        /// <summary>
        /// Releases the resources used by the display driver.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static int GetFrameBufferIndex(int x, int y)
        {
            return (y * (PanelWidth / 8)) + (x / 8);
        }

        private void PrepareForWrite()
        {
            // Data/COM interval, TCON, resolution, PLL, and vendor tuning bytes: from Heltec INIT_JD79656_mcu() / datasheet RAM write prep.
            SendCommand((byte)Command.VcomAndDataIntervalSetting);
            SendData(0x97);

            SendCommand((byte)Command.TconSetting);
            SendData(0x22);

            SendCommand((byte)Command.ResolutionSetting);
            SendData(0x80, 0xFA);

            SendCommand((byte)Command.VcmDcSetting);
            SendData(0x13);

            SendCommand((byte)Command.PLLControl);
            SendData(0x1A);

            // Controller tuning values taken from Heltec's INIT_JD79656_mcu() helper.
            SendCommand(VendorCommandPowerSaving);
            SendData(0x88);

            SendCommand(VendorCommandUnknownF8);
            SendData(0x80);

            SendCommand(VendorCommandUnknownB3);
            SendData(0x42);

            SendCommand(VendorCommandUnknownB4);
            SendData(0x28);

            SendCommand(VendorCommandUnknownAa);
            SendData(0xB7);

            SendCommand(VendorCommandUnknownA8);
            SendData(0x3D);
        }

        private void SetRamPointerToOrigin()
        {
            SendCommand((byte)Command.SetXAddressCounter);
            SendData(0x01);

            SendCommand((byte)Command.SetYAddressCounter);
            SendData(0xF9, 0x00);
        }

        private void WritePartialRefreshLuts()
        {
            SendCommand((byte)Command.WriteLutVcom);
            SendData(PartialLutVcom);

            SendCommand((byte)Command.WriteLutWhiteToWhite);
            SendData(PartialLutWhiteToWhite);

            SendCommand((byte)Command.WriteLutBlackToWhite);
            SendData(PartialLutBlackToWhite);

            SendCommand((byte)Command.WriteLutWhiteToBlack);
            SendData(PartialLutWhiteToBlack);

            SendCommand((byte)Command.WriteLutBlackToBlack);
            SendData(PartialLutBlackToBlack);
        }

        /// <summary>
        /// Waits for the specified number of milliseconds.
        /// </summary>
        /// <param name="milliseconds">The delay in milliseconds.</param>
        protected void WaitMs(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }

        private void UpdatePreviousFrame()
        {
            byte[] currentFrame = FrameBuffer1bpp.Buffer;
            for (int i = 0; i < currentFrame.Length; i++)
            {
                _previousFrame[i] = currentFrame[i];
            }
        }

        /// <summary>
        /// Releases managed resources used by the display driver.
        /// </summary>
        /// <param name="disposing">True to dispose managed resources.</param>
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
                }

                _disposed = true;
            }
        }
    }
}
