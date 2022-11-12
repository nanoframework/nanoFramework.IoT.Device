// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;

using Iot.Device.ePaper.Buffers;
using Iot.Device.ePaper.Primitives;

namespace Iot.Device.ePaper.Drivers
{
    /// <summary>
    /// A driver class for the SSD1681 display controller.
    /// </summary>
    public sealed class Ssd1681 : IePaperDisplay
    {
        private const int PagesPerFrame = 5;
        private const int FirstPageIndex = 0;

        private readonly GpioPin resetPin;
        private readonly GpioPin busyPin;
        private readonly GpioPin dataCommandPin;
        private readonly SpiDevice spiDevice;

        private int currentFrameBufferPage;
        private int currentFrameBufferPageLowerBound;
        private int currentFrameBufferPageUpperBound;
        private int currentFrameBufferStartXPosition;
        private int currentFrameBufferStartYPosition;

        private FrameBuffer2BitPerPixel frameBuffer2bpp;

        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ssd1681"/> class.
        /// </summary>
        /// <param name="resetPin">The reset GPIO pin.</param>
        /// <param name="busyPin">The busy GPIO pin.</param>
        /// <param name="dataCommandPin">The data/command GPIO pin.</param>
        /// <param name="spiBusId">The SPI bus to use to communicate with the display.</param>
        /// <param name="chipSelectLinePin">The chip select line pin.</param>
        /// <param name="gpioController">The <see cref="GpioController"/> to use when initializing the pins.</param>
        /// <param name="width">The width of the display.</param>
        /// <param name="height">The height of the display.</param>
        /// <param name="enableFramePaging">Page the frame buffer and all operations to use less memory.</param>
        /// <remarks>
        /// For a 200x200 SSD1681 display, a full Frame requires about 5KB of RAM ((200 * 200) / 8). SSD1681 has 2 RAMs for B/W and Red pixel.
        /// This means to have a full frame in memory, you need about 10KB of RAM. If you can't guarantee 10KB to be available to the driver
        /// then enable paging by setting <paramref name="enableFramePaging"/> to true. A page uses about 2KB (1KB for B/W and 1KB for Red).
        /// </remarks>
        public Ssd1681(
            int resetPin,
            int busyPin,
            int dataCommandPin,
            int spiBusId,
            int chipSelectLinePin,
            GpioController gpioController,
            int width,
            int height,
            bool enableFramePaging = true)
        {
            if (width <= 0 || width > 200)
                throw new ArgumentOutOfRangeException(nameof(width), "Display width can't be less than 0 or greater than 200");

            if (height <= 0 || height > 200)
                throw new ArgumentOutOfRangeException(nameof(height), "Display height can't be less than 0 or greater than 200");

            // Setup SPI connection with the display
            var spiConnectionSettings = new SpiConnectionSettings(spiBusId, chipSelectLinePin)
            {
                ClockFrequency = 20_000_000, // 20MHz
                Mode = SpiMode.Mode0,
                ChipSelectLineActiveState = false,
                Configuration = SpiBusConfiguration.HalfDuplex,
                DataFlow = DataFlow.MsbFirst
            };

            this.spiDevice = new SpiDevice(spiConnectionSettings);

            // setup the gpio pins
            this.resetPin = gpioController.OpenPin(resetPin, PinMode.Output);
            this.dataCommandPin = gpioController.OpenPin(dataCommandPin, PinMode.Output);
            this.busyPin = gpioController.OpenPin(busyPin, PinMode.Input);

            this.Width = width;
            this.Height = height;
            PowerState = PowerState.Unknown;

            this.InitializeFrameBuffer(width, height, enableFramePaging);
            this.CalculateFrameBufferPageBounds();
        }


        /// <inheritdoc/>
        public int Width { get; }

        /// <inheritdoc/>
        public int Height { get; }

        /// <inheritdoc/>
        public IFrameBuffer FrameBuffer
            => this.frameBuffer2bpp;

        /// <summary>
        /// Gets the current power state of the display panel.
        /// </summary>
        public PowerState PowerState { get; private set; }



        /// <summary>
        /// Performs the required steps to "power on" the display.
        /// </summary>
        public void PowerOn()
        {
            this.HardwareReset();
            this.SoftwareReset();

            PowerState = PowerState.PoweredOn;
        }

        /// <summary>
        /// Puts the display to sleep using the specified <see cref="SleepMode"/>.
        /// </summary>
        /// <param name="sleepMode">The sleep mode to use when powering down the display.</param>
        public void PowerDown(SleepMode sleepMode = SleepMode.Normal)
        {
            this.SendCommand((byte)Command.DeepSleepMode);
            this.SendData((byte)sleepMode);

            PowerState = PowerState.PoweredOff;
        }

        /// <summary>
        /// Perform the required initialization steps to set up the display.
        /// </summary>
        public void Initialize()
        {
            if (PowerState != PowerState.PoweredOn)
                throw new InvalidOperationException("Unable to initialize the display until it has been powered on. Call PowerOn() first.");

            // set gate lines and scanning sequence
            this.SendCommand((byte)Command.DriverOutputControl);
            this.SendData((byte)(this.Height - 1), 0x00, 0x00); // refer to spec for description

            // Set data entry sequence
            this.SendCommand((byte)Command.DataEntryModeSetting);
            this.SendData(0x03); // Y Increment, X Increment with RAM address counter auto updated in the X direction.

            // Set RAM X start / end position
            this.SendCommand((byte)Command.SetRAMAddressXStartEndPosition);
            this.SendData(
                /* Start at 0*/ 0x00,
                /* End */ (byte)(this.Width / 8 - 1)); // end at width bits converted to bytes (starts @ 0)

            // Set RAM Y start / end positon
            this.SendCommand((byte)Command.SetRAMAddressYStartEndPosition);
            this.SendData(/* Start at 0 */ 0x00, 0x00, /* End */ (byte)(this.Height - 1), 0x00);

            // Set Panel Border
            this.SendCommand((byte)Command.BorderWaveformControl);
            this.SendData(0xc0); // was 0x05

            // Set Temperature sensor
            this.SendCommand((byte)Command.TempSensorControlSelection);
            this.SendData(0x80); // use internal temp sensor

            // Do a full refresh of the display
            this.PerformFullRefresh();
        }


        /// <summary>
        /// Perform full panel refresh sequence.
        /// </summary>
        public void PerformFullRefresh()
        {
            this.SendCommand((byte)Command.BoosterSoftStartControl);
            this.SendData(0x8b, 0x9c, 0x96, 0x0f);

            this.SendCommand((byte)Command.DisplayUpdateControl2);
            this.SendData((byte)RefreshMode.FullRefresh); // Display Mode 1 (Full Refresh)

            this.SendCommand((byte)Command.MasterActivation);
            this.WaitReady();
        }

        /// <summary>
        /// Perform partial panel refresh sequence.
        /// </summary>
        public void PerformPartialRefresh()
        {
            this.SendCommand((byte)Command.BoosterSoftStartControl);
            this.SendData(0x8b, 0x9c, 0x96, 0x0f);

            this.SendCommand((byte)Command.DisplayUpdateControl2);
            this.SendData((byte)RefreshMode.PartialRefresh); // Display Mode 2 (Partial Refresh)

            this.SendCommand((byte)Command.MasterActivation);
            this.WaitReady();
        }


        /// <inheritdoc/>
        public void Clear(bool triggerPageRefresh = false)
        {
            this.BeginFrameDraw();
            do
            {
                // do nothing. internal frame buffers already cleared by BeginFrameDraw()
            }
            while (this.NextFramePage());

            this.EndFrameDraw();

            if (triggerPageRefresh)
                this.PerformFullRefresh();
        }

        /// <summary>
        /// Sets the 'cursor' position within the display's RAM.
        /// </summary>
        /// <param name="x">X location within the display's RAM.</param>
        /// <param name="y">Y location within the display's RAM.</param>
        public void SetPosition(int x, int y)
        {
            this.EnforceBounds(ref x, ref y);

            this.SendCommand((byte)Command.SetRAMAddressCounterX);
            this.SendData((byte)(x / 8)); // each row can have up to 200 points each represented by a single bit.

            this.SendCommand((byte)Command.SetRAMAddressCounterY);
            this.SendData((byte)y);
        }

        /// <summary>
        /// Draws a single pixel to the appropriate frame buffer.
        /// </summary>
        /// <param name="x">The X Position.</param>
        /// <param name="y">The Y Position.</param>
        /// <param name="inverted">True to invert the pixel from white to black.</param>
        public void DrawPixel(int x, int y, bool inverted)
            => this.DrawPixel(x, y, inverted ? Color.Black : Color.White);

        /// <summary>
        /// Draws a single pixel to the appropriate frame buffer based on the specified color.
        /// </summary>
        /// <param name="x">The X Position.</param>
        /// <param name="y">The Y Position.</param>
        /// <param name="color">Pixel color. See the remarks for how a buffer is selected based on the color value.</param>
        /// <remarks>
        /// The SSD1681 comes with 2 RAMs: a Black and White RAM and a Red RAM.
        /// Writing to the B/W RAM draws B/W pixels on the panel. While writing to the Red RAM draws red pixels on the panel (if the panel supports red).
        /// However, the SSD1681 doesn't support specifying the color level (no grayscaling), therefore the way the buffer is selected 
        /// is by performing a simple binary check: 
        /// if R >= 128 and G == 0 and B == 0 then write a red pixel to the Red Buffer/RAM
        /// if R == 0 and G == 0 and B == 0 then write a black pixel to B/W Buffer/RAM
        /// else, assume white pixel and write to B/W Buffer/RAM.
        /// </remarks>
        public void DrawPixel(int x, int y, Color color)
        {
            // ignore out of bounds draw attempts
            if (x < 0 || x >= this.Width || y < 0 || y >= this.Height)
            {
                return;
            }

            var frameByteIndex = this.GetFrameBufferIndex(x, y);
            var pageByteIndex = frameByteIndex - this.currentFrameBufferPageLowerBound;

            // if the specified point falls in the current page, update the buffer
            if (this.currentFrameBufferPageLowerBound <= frameByteIndex
                && frameByteIndex < this.currentFrameBufferPageUpperBound)
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
                    this.frameBuffer2bpp.ColorBuffer[pageByteIndex] |= (byte)(128 >> (x & 7));
                }
                else if (color.R == 0 && color.G == 0 && color.B == 0)
                {
                    // black pixel
                    this.frameBuffer2bpp.BlackBuffer[pageByteIndex] &= (byte)~(128 >> (x & 7));
                }
                else
                {
                    // assume white if R, G, and B > 0
                    this.frameBuffer2bpp.BlackBuffer[pageByteIndex] |= (byte)(128 >> (x & 7));
                }
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
            this.SetPosition(startXPos, startYPos);

            this.SendCommand((byte)Command.WriteBackWhiteRAM);
            this.SendData(buffer);
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
            this.SetPosition(startXPos, startYPos);

            this.SendCommand((byte)Command.WriteRedRAM);
            this.SendData(buffer);
        }



        /// <summary>
        /// Begins a frame draw operation with frame paging support.
        /// <code>
        /// SSD1681.BeginFrameDraw();
        /// do {
        ///     // Drawing calls
        /// } while (SSD1681.NextFramePage());
        /// SSD1681.EndFrameDraw();
        /// </code>
        /// </summary>
        public void BeginFrameDraw()
        {
            // make sure we start from the first page with clear buffers
            this.SetFrameBufferPage(FirstPageIndex);
        }

        /// <summary>
        /// Moves the current buffers to the next frame page and returns true if successful.
        /// </summary>
        /// <returns>True if the next frame page is available and the internal buffers have moved to it, otherwise; false.</returns>
        public bool NextFramePage()
        {
            if (this.currentFrameBufferPage < PagesPerFrame - 1)
            {
                this.WriteInternalBuffersToDevice();

                this.currentFrameBufferPage++;
                this.SetFrameBufferPage(this.currentFrameBufferPage);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Ends the frame draw and flushes the current page to the device.
        /// </summary>
        public void EndFrameDraw()
        {
            this.WriteInternalBuffersToDevice();
            this.SetFrameBufferPage(FirstPageIndex);
        }



        /// <summary>
        /// Sends a command to the <see cref="SpiDevice"/>.
        /// </summary>
        /// <param name="command">The command's byte(s) to send.</param>
        public void SendCommand(params byte[] command)
        {
            // make sure we are setting data/command pin to low (command mode)
            this.dataCommandPin.Write(PinValue.Low);

            foreach (var b in command)
            {
                // write the command byte
                this.spiDevice.WriteByte(b);
            }
        }

        /// <summary>
        /// Sends data to the <see cref="SpiDevice"/>.
        /// </summary>
        /// <param name="data">The data byte(s) to send.</param>
        public void SendData(params byte[] data)
        {
            // set the data/command pin to high to indicate to the display we will be sending data
            this.dataCommandPin.Write(PinValue.High);

            foreach (var @byte in data)
            {
                this.spiDevice.WriteByte(@byte);
            }

            // go back to low (command mode)
            this.dataCommandPin.Write(PinValue.Low);
        }

        /// <summary>
        /// Waits for the busy pin on the display to read <see cref="PinValue.Low"/> (which indicates the display is ready) before unblocking execution.
        /// </summary>
        public void WaitReady()
        {
            while (this.busyPin.Read() == PinValue.High)
            {
                this.WaitMs(5);
            }
        }




        /// <summary>
        /// Performs the hardware reset commands sequence on the display.
        /// </summary>
        private void HardwareReset()
        {
            // specs say to wait 10ms after supplying voltage to the display
            this.WaitMs(10);

            this.resetPin.Write(PinValue.Low);
            this.WaitMs(200);

            this.resetPin.Write(PinValue.High);
            this.WaitMs(200);
        }

        /// <summary>
        /// Performs the software reset commands sequence on the display.
        /// </summary>
        private void SoftwareReset()
        {
            this.SendCommand((byte)Command.SoftwareReset);

            this.WaitReady();
            this.WaitMs(10);
        }

        /// <summary>
        /// A simple wait method that wraps <see cref="Thread.Sleep(int)"/>.
        /// </summary>
        /// <param name="milliseconds">Number of milliseconds to sleep.</param>
        private void WaitMs(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }

        /// <summary>
        /// Snaps the provided coordinates to the lower bounds of the display if out of allowed range.
        /// </summary>
        /// <param name="x">The X position.</param>
        /// <param name="y">The Y position.</param>
        private void EnforceBounds(ref int x, ref int y)
        {
            x = x < 0 || x > this.Width - 1 ? 0 : x;
            y = y < 0 || y > this.Height - 1 ? 0 : y;
        }

        /// <summary>
        /// Gets the index of the byte containing the pixel specified by the <paramref name="x"/> and <paramref name="y"/> parameters.
        /// </summary>
        /// <param name="x">The X position of the pixel.</param>
        /// <param name="y">The Y position of the pixel.</param>
        /// <returns>The index of the byte in the frame buffer which contains the specified pixel.</returns>
        private int GetFrameBufferIndex(int x, int y)
        {
            // x specifies the column
            return (x + y * this.Width) / 8;
        }

        /// <summary>
        /// Gets the X position from a buffer index.
        /// </summary>
        /// <param name="index">The buffer index.</param>
        /// <returns>The X position of a pixel.</returns>
        private int GetXPositionFromFrameBufferIndex(int index)
        {
            if (index <= 0)
                return 0;

            return index * 8 % this.Width;
        }

        /// <summary>
        /// Gets the Y position from a buffer index.
        /// </summary>
        /// <param name="index">The buffer index.</param>
        /// <returns>The Y position of a pixel.</returns>
        private int GetYPositionFromFrameBufferIndex(int index)
        {
            if (index <= 0)
                return 0;

            return index * 8 / this.Height;
        }

        /// <summary>
        /// Initializes a new instance of the intenral frame buffer. Supports paging.
        /// </summary>
        /// <param name="width">The width of the frame buffer in pixels.</param>
        /// <param name="height">The height of the frame buffer in pixels.</param>
        /// <param name="enableFramePaging">If <see langword="true"/>, enables paging the frame.</param>
        private void InitializeFrameBuffer(int width, int height, bool enableFramePaging)
        {
            var frameBufferHeight = enableFramePaging ? height / PagesPerFrame : height;
            this.frameBuffer2bpp = new FrameBuffer2BitPerPixel(frameBufferHeight, width);
        }

        /// <summary>
        /// Sets the current active frame buffer page to the specified page index.
        /// Existing frame buffer is reused by clearing it first and page bounds are recalculated.
        /// Make sure to call <see cref="PerformFullRefresh"/> or <see cref="PerformPartialRefresh"/>
        /// to persist the frame to the display's RAM before calling this method.
        /// </summary>
        /// <param name="page">The frame buffer page index to move to.</param>
        private void SetFrameBufferPage(int page)
        {
            if (page < 0 || page >= PagesPerFrame)
                page = 0;

            this.frameBuffer2bpp.Clear();

            this.currentFrameBufferPage = page;
            this.CalculateFrameBufferPageBounds();
        }

        /// <summary>
        /// Calculates the upper and lower bounds of the current frame buffer page.
        /// </summary>
        private void CalculateFrameBufferPageBounds()
        {
            // black and color buffers have the same size, so we will work with only the black buffer
            // in these calculations.
            this.currentFrameBufferPageLowerBound = this.currentFrameBufferPage * this.frameBuffer2bpp.BlackBuffer.BufferByteCount;
            this.currentFrameBufferPageUpperBound = (this.currentFrameBufferPage + 1) * this.frameBuffer2bpp.BlackBuffer.BufferByteCount;

            this.currentFrameBufferStartXPosition = this.GetXPositionFromFrameBufferIndex(this.currentFrameBufferPageLowerBound);
            this.currentFrameBufferStartYPosition = this.GetYPositionFromFrameBufferIndex(this.currentFrameBufferPageLowerBound);

            this.FrameBuffer.StartPoint = new Point(this.currentFrameBufferStartXPosition, this.currentFrameBufferStartYPosition);
            this.FrameBuffer.CurrentFramePage = this.currentFrameBufferPage;
        }

        /// <summary>
        /// A shortcut method to write the contents of the <see cref="FrameBuffer"/> to the display's RAM.
        /// </summary>
        private void WriteInternalBuffersToDevice()
        {
            // write B/W and Color (RED) frame to the display's RAM.
            this.DirectDrawBuffer(
                this.currentFrameBufferStartXPosition,
                this.currentFrameBufferStartYPosition,
                this.frameBuffer2bpp.BlackBuffer.Buffer);

            this.DirectDrawColorBuffer(
                this.currentFrameBufferStartXPosition,
                this.currentFrameBufferStartYPosition,
                this.frameBuffer2bpp.ColorBuffer.Buffer);
        }


        #region Enums

        /// <summary>
        /// Commands supported by SSD1681.
        /// </summary>
        public enum Command : byte
        {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1602 // The enumeration sub-item must have a documentation header
            DriverOutputControl = 0x01,
            GateDrivingVoltage = 0x03,
            SourceDrivingVoltageControl = 0x04,
            InitialCodeSettingOtpProgram = 0x08,
            WriteRegisterForInitialCodeSetting = 0x09,
            ReadRegisterForInitialCodeSetting = 0x0a,
            BoosterSoftStartControl = 0x0c,
            DeepSleepMode = 0x10,
            DataEntryModeSetting = 0x11,
            SoftwareReset = 0x12,
            HvReadyDetection = 0x14,
            VciDetection = 0x15,
            TempSensorControlSelection = 0x18,
            TempSensorControlWriteRegister = 0x1a,
            TempSensorControlReadRegister = 0x1b,
            ExternalTempSensorControlWrite = 0x1c,
            MasterActivation = 0x20,
            DisplayUpdateControl1 = 0x21,
            DisplayUpdateControl2 = 0x22,
            WriteBackWhiteRAM = 0x24,
            WriteRedRAM = 0x26,
            ReadRAM = 0x27,
            VCOMSense = 0x28,
            VCOMSenseDuration = 0x29,
            ProgramVCOMOTP = 0x2a,
            WriteRegisterControlVCOM = 0x2b,
            WriteVCOMRegister = 0x2c,
            OTPRegisterReadDisplayOption = 0x2d,
            UserIdRead = 0x2e,
            StatusBitRead = 0x2f,
            ProgramWSOTP = 0x30,
            LoadWSOTP = 0x31,
            WriteLUTRegister = 0x32,
            CrcCalculation = 0x34,
            CrcStatusRead = 0x35,
            ProgramOTPSelection = 0x36,
            WriteRegisterForDisplayOption = 0x37,
            WriteRegisterForUserId = 0x38,
            OTPProgramMode = 0x39,
            BorderWaveformControl = 0x3c,
            EndOption = 0x3f,
            ReadRAMOption = 0x41,
            SetRAMAddressXStartEndPosition = 0x44,
            SetRAMAddressYStartEndPosition = 0x45,
            AutoWriteRAMForRegularPatternRed = 0x46,
            AutoWriteRAMForRegularPatternBlackWhite = 0x47,
            SetRAMAddressCounterX = 0x4e,
            SetRAMAddressCounterY = 0x4f,
            NOP = 0x7f
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1602 // The enumeration sub-item must have a documentation header
        }

        /// <summary>
        /// SSD1681 Supported Sleep Modes.
        /// </summary>
        public enum SleepMode : byte
        {
            /// <summary>
            /// Normal Sleep Mode.
            /// In this mode: 
            /// - DC/DC Off 
            /// - No Clock 
            /// - No Output load 
            /// - MCU Interface Access: ON
            /// - RAM Data Access: ON.
            /// </summary>
            Normal = 0x00,

            /// <summary>
            /// Deep Sleep Mode 1.
            /// In this mode: 
            /// - DC/DC Off 
            /// - No Clock 
            /// - No Output load 
            /// - MCU Interface Access: OFF
            /// - RAM Data Access: ON (RAM contents retained).
            /// </summary>
            DeepSleepModeOne = 0x01,

            /// <summary>
            /// Deep Sleep Mode 2.
            /// In this mode: 
            /// - DC/DC Off 
            /// - No Clock 
            /// - No Output load 
            /// - MCU Interface Access: OFF
            /// - RAM Data Access: OFF (RAM contents NOT retained).
            /// </summary>
            DeepSleepModeTwo = 0x11,
        }

        /// <summary>
        /// SSD1861 Supported Refresh Modes.
        /// </summary>
        public enum RefreshMode : byte
        {
            /// <summary>
            /// Causes the display to perform a full refresh of the panel (Display Mode 1).
            /// </summary>
            FullRefresh = 0xf7,

            /// <summary>
            /// Causes the display to perform a partial refresh of the panel (Display Mode 2).
            /// </summary>
            PartialRefresh = 0xff,
        }

        /// <summary>
        /// SSD1681 RAM.
        /// </summary>
        public enum Ram : byte
        {
            /// <summary>
            /// Specifies the black and white RAM area.
            /// </summary>
            BlackWhite = 0x00,

            /// <summary>
            /// Specifies the Colored RAM area (Red for SSD1681).
            /// </summary>
            Color = 0x01,
        }

#pragma warning restore SA1201 // Missing XML comment for publicly visible type or member

        #endregion

        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.resetPin.Dispose();
                    this.busyPin.Dispose();
                    this.dataCommandPin.Dispose();
                    this.spiDevice.Dispose();

                    this.frameBuffer2bpp = null;
                }

                this.disposed = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
