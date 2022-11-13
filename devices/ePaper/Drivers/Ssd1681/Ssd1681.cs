// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;

using Iot.Device.EPaper.Buffers;
using Iot.Device.EPaper.Primitives;

namespace Iot.Device.EPaper.Drivers.Ssd1681
{
    /// <summary>
    /// A driver class for the SSD1681 display controller.
    /// </summary>
    public sealed class Ssd1681 : IEPaperDisplay
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
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Display width can't be less than 0 or greater than 200");
            }

            if (height <= 0 || height > 200)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Display height can't be less than 0 or greater than 200");
            }

            // Setup SPI connection with the display
            var spiConnectionSettings = new SpiConnectionSettings(spiBusId, chipSelectLinePin)
            {
                ClockFrequency = 20_000_000, // 20MHz
                Mode = SpiMode.Mode0,
                ChipSelectLineActiveState = false,
                Configuration = SpiBusConfiguration.HalfDuplex,
                DataFlow = DataFlow.MsbFirst
            };

            spiDevice = new SpiDevice(spiConnectionSettings);

            // setup the gpio pins
            this.resetPin = gpioController.OpenPin(resetPin, PinMode.Output);
            this.dataCommandPin = gpioController.OpenPin(dataCommandPin, PinMode.Output);
            this.busyPin = gpioController.OpenPin(busyPin, PinMode.Input);

            Width = width;
            Height = height;
            PowerState = PowerState.Unknown;

            InitializeFrameBuffer(width, height, enableFramePaging);
            CalculateFrameBufferPageBounds();
        }

        /// <inheritdoc/>
        public int Width { get; }

        /// <inheritdoc/>
        public int Height { get; }

        /// <inheritdoc/>
        public IFrameBuffer FrameBuffer
        {
            get
            {
                return frameBuffer2bpp;
            }
        }

        /// <summary>
        /// Gets the current power state of the display panel.
        /// </summary>
        public PowerState PowerState { get; private set; }

        /// <summary>
        /// Performs the required steps to "power on" the display.
        /// </summary>
        public void PowerOn()
        {
            HardwareReset();
            SoftwareReset();

            PowerState = PowerState.PoweredOn;
        }

        /// <summary>
        /// Puts the display to sleep using the specified <see cref="SleepMode"/>.
        /// </summary>
        /// <param name="sleepMode">The sleep mode to use when powering down the display.</param>
        public void PowerDown(SleepMode sleepMode = SleepMode.Normal)
        {
            SendCommand((byte)Command.DeepSleepMode);
            SendData((byte)sleepMode);

            PowerState = PowerState.PoweredOff;
        }

        /// <summary>
        /// Perform the required initialization steps to set up the display.
        /// </summary>
        public void Initialize()
        {
            if (PowerState != PowerState.PoweredOn)
            {
                throw new InvalidOperationException("Unable to initialize the display until it has been powered on. Call PowerOn() first.");
            }

            // set gate lines and scanning sequence
            SendCommand((byte)Command.DriverOutputControl);
            SendData((byte)(Height - 1), 0x00, 0x00); // refer to spec for description

            // Set data entry sequence
            SendCommand((byte)Command.DataEntryModeSetting);
            SendData(0x03); // Y Increment, X Increment with RAM address counter auto updated in the X direction.

            // Set RAM X start / end position
            SendCommand((byte)Command.SetRAMAddressXStartEndPosition);
            SendData(
                /* Start at 0*/ 0x00,
                /* End */ (byte)((Width / 8) - 1)); // end at width bits converted to bytes (starts @ 0)

            // Set RAM Y start / end positon
            SendCommand((byte)Command.SetRAMAddressYStartEndPosition);
            SendData(/* Start at 0 */ 0x00, 0x00, /* End */ (byte)(Height - 1), 0x00);

            // Set Panel Border
            SendCommand((byte)Command.BorderWaveformControl);
            SendData(0xc0); // was 0x05

            // Set Temperature sensor
            SendCommand((byte)Command.TempSensorControlSelection);
            SendData(0x80); // use internal temp sensor

            // Do a full refresh of the display
            PerformFullRefresh();
        }

        /// <inheritdoc/>
        public void PerformFullRefresh()
        {
            SendCommand((byte)Command.BoosterSoftStartControl);
            SendData(0x8b, 0x9c, 0x96, 0x0f);

            SendCommand((byte)Command.DisplayUpdateControl2);
            SendData((byte)RefreshMode.FullRefresh); // Display Mode 1 (Full Refresh)

            SendCommand((byte)Command.MasterActivation);
            WaitReady();
        }

        /// <inheritdoc/>
        public void PerformPartialRefresh()
        {
            SendCommand((byte)Command.BoosterSoftStartControl);
            SendData(0x8b, 0x9c, 0x96, 0x0f);

            SendCommand((byte)Command.DisplayUpdateControl2);
            SendData((byte)RefreshMode.PartialRefresh); // Display Mode 2 (Partial Refresh)

            SendCommand((byte)Command.MasterActivation);
            WaitReady();
        }

        /// <inheritdoc/>
        public void Clear(bool triggerPageRefresh = false)
        {
            BeginFrameDraw();
            do
            {
                // do nothing. internal frame buffers already cleared by BeginFrameDraw()
            }
            while (NextFramePage());

            EndFrameDraw();

            if (triggerPageRefresh)
            {
                PerformFullRefresh();
            }
        }

        /// <inheritdoc/>
        public void Flush()
        {
            // write B/W and Color (RED) frame to the display's RAM.
            DirectDrawBuffer(
                currentFrameBufferStartXPosition,
                currentFrameBufferStartYPosition,
                frameBuffer2bpp.BlackBuffer.Buffer);

            DirectDrawColorBuffer(
                currentFrameBufferStartXPosition,
                currentFrameBufferStartYPosition,
                frameBuffer2bpp.ColorBuffer.Buffer);
        }

        /// <inheritdoc/>
        public void SetPosition(int x, int y)
        {
            EnforceBounds(ref x, ref y);

            SendCommand((byte)Command.SetRAMAddressCounterX);
            SendData((byte)(x / 8)); // each row can have up to 200 points each represented by a single bit.

            SendCommand((byte)Command.SetRAMAddressCounterY);
            SendData((byte)y);
        }

        /// <summary>
        /// Draws a single pixel to the appropriate frame buffer.
        /// </summary>
        /// <param name="x">The X Position.</param>
        /// <param name="y">The Y Position.</param>
        /// <param name="inverted">True to invert the pixel from white to black.</param>
        public void DrawPixel(int x, int y, bool inverted)
        {
            DrawPixel(x, y, inverted ? Color.Black : Color.White);
        }

        /// <inheritdoc/>
        public void DrawPixel(int x, int y, Color color)
        {
            // ignore out of bounds draw attempts
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return;
            }

            var frameByteIndex = GetFrameBufferIndex(x, y);
            var pageByteIndex = frameByteIndex - currentFrameBufferPageLowerBound;

            // if the specified point falls in the current page, update the buffer
            if (currentFrameBufferPageLowerBound <= frameByteIndex
                && frameByteIndex < currentFrameBufferPageUpperBound)
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
                    frameBuffer2bpp.ColorBuffer[pageByteIndex] |= (byte)(128 >> (x & 7));
                }
                else if (color.R == 0 && color.G == 0 && color.B == 0)
                {
                    // black pixel
                    frameBuffer2bpp.BlackBuffer[pageByteIndex] &= (byte)~(128 >> (x & 7));
                }
                else
                {
                    // assume white if R, G, and B > 0
                    frameBuffer2bpp.BlackBuffer[pageByteIndex] |= (byte)(128 >> (x & 7));
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

        /// <inheritdoc/>
        public void BeginFrameDraw()
        {
            // make sure we start from the first page with clear buffers
            SetFrameBufferPage(FirstPageIndex);
        }

        /// <inheritdoc/>
        public bool NextFramePage()
        {
            if (currentFrameBufferPage < PagesPerFrame - 1)
            {
                Flush();

                currentFrameBufferPage++;
                SetFrameBufferPage(currentFrameBufferPage);

                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public void EndFrameDraw()
        {
            Flush();
            SetFrameBufferPage(FirstPageIndex);
        }

        /// <inheritdoc/>
        public void SendCommand(params byte[] command)
        {
            // make sure we are setting data/command pin to low (command mode)
            dataCommandPin.Write(PinValue.Low);

            foreach (var b in command)
            {
                // write the command byte
                spiDevice.WriteByte(b);
            }
        }

        /// <inheritdoc/>
        public void SendData(params byte[] data)
        {
            // set the data/command pin to high to indicate to the display we will be sending data
            dataCommandPin.Write(PinValue.High);

            foreach (var @byte in data)
            {
                spiDevice.WriteByte(@byte);
            }

            // go back to low (command mode)
            dataCommandPin.Write(PinValue.Low);
        }

        /// <inheritdoc/>
        public void WaitReady()
        {
            while (busyPin.Read() == PinValue.High)
            {
                WaitMs(5);
            }
        }

        /// <summary>
        /// Performs the hardware reset commands sequence on the display.
        /// </summary>
        private void HardwareReset()
        {
            // specs say to wait 10ms after supplying voltage to the display
            WaitMs(10);

            resetPin.Write(PinValue.Low);
            WaitMs(200);

            resetPin.Write(PinValue.High);
            WaitMs(200);
        }

        /// <summary>
        /// Performs the software reset commands sequence on the display.
        /// </summary>
        private void SoftwareReset()
        {
            SendCommand((byte)Command.SoftwareReset);

            WaitReady();
            WaitMs(10);
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
            x = x < 0 || x > Width - 1 ? 0 : x;
            y = y < 0 || y > Height - 1 ? 0 : y;
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
            return (x + (y * Width)) / 8;
        }

        /// <summary>
        /// Gets the X position from a buffer index.
        /// </summary>
        /// <param name="index">The buffer index.</param>
        /// <returns>The X position of a pixel.</returns>
        private int GetXPositionFromFrameBufferIndex(int index)
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
        private int GetYPositionFromFrameBufferIndex(int index)
        {
            if (index <= 0)
            {
                return 0;
            }

            return index * 8 / Height;
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
            frameBuffer2bpp = new FrameBuffer2BitPerPixel(frameBufferHeight, width);
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
            {
                page = 0;
            }

            frameBuffer2bpp.Clear();

            currentFrameBufferPage = page;
            CalculateFrameBufferPageBounds();
        }

        /// <summary>
        /// Calculates the upper and lower bounds of the current frame buffer page.
        /// </summary>
        private void CalculateFrameBufferPageBounds()
        {
            // black and color buffers have the same size, so we will work with only the black buffer
            // in these calculations.
            currentFrameBufferPageLowerBound = currentFrameBufferPage * frameBuffer2bpp.BlackBuffer.BufferByteCount;
            currentFrameBufferPageUpperBound = (currentFrameBufferPage + 1) * frameBuffer2bpp.BlackBuffer.BufferByteCount;

            currentFrameBufferStartXPosition = GetXPositionFromFrameBufferIndex(currentFrameBufferPageLowerBound);
            currentFrameBufferStartYPosition = GetYPositionFromFrameBufferIndex(currentFrameBufferPageLowerBound);

            FrameBuffer.StartPoint = new Point(currentFrameBufferStartXPosition, currentFrameBufferStartYPosition);
            FrameBuffer.CurrentFramePage = currentFrameBufferPage;
        }

        #region Enums

        /// <summary>
        /// Commands supported by SSD1681.
        /// </summary>
        public enum Command : byte
        {
            /// <summary>
            /// Driver Output Control Command. Sets the gate, scanning order, etc.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            DriverOutputControl = 0x01,

            /// <summary>
            /// Gate Driving Voltage Command.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            GateDrivingVoltage = 0x03,

            /// <summary>
            /// Source Driving Voltage Control.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            SourceDrivingVoltageControl = 0x04,

            /// <summary>
            /// Program Initial Code Setting.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            InitialCodeSettingOtpProgram = 0x08,

            /// <summary>
            /// Write Register for Initial Code Setting Selection.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            WriteRegisterForInitialCodeSetting = 0x09,

            /// <summary>
            /// Read Register for Initial Code Setting.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            ReadRegisterForInitialCodeSetting = 0x0a,

            /// <summary>
            /// Booster Enable with Phase 1, Phase 2 and Phase 3 for soft start current and duration setting.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            BoosterSoftStartControl = 0x0c,

            /// <summary>
            /// Set Deep Sleep Mode.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            DeepSleepMode = 0x10,

            /// <summary>
            /// Define data entry sequence.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            DataEntryModeSetting = 0x11,

            /// <summary>
            /// Software Reset. This resets the commands and parameters to their S/W default values except Deep Sleep Mode.
            /// The Busy pin will read high during this operation.
            /// RAM contents are not affected by this command.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            SoftwareReset = 0x12,

            /// <summary>
            /// HV Ready Detection.
            /// The Busy pin will read high during this operation.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            HvReadyDetection = 0x14,

            /// <summary>
            /// VCI Detection.
            /// The Busy pin will read high during this operation.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            VciDetection = 0x15,

            /// <summary>
            /// Temperature Sensor Selection (External vs Internal).
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            TempSensorControlSelection = 0x18,

            /// <summary>
            /// Write to temperature register.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            TempSensorControlWriteRegister = 0x1a,

            /// <summary>
            /// Read from temperature register.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            TempSensorControlReadRegister = 0x1b,

            /// <summary>
            /// Temperature Sensor Control. Write command to External temperature sensor.
            /// The Busy pin will read high during this operation.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            ExternalTempSensorControlWrite = 0x1c,

            /// <summary>
            /// Master Activation. Activate display update sequence.
            /// The Busy pin will read high during this operation.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            MasterActivation = 0x20,

            /// <summary>
            /// Display Update Control 1.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            DisplayUpdateControl1 = 0x21,

            /// <summary>
            /// Display Update Sequence Option. Enables the stage for <see cref="MasterActivation"/>.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            DisplayUpdateControl2 = 0x22,

            /// <summary>
            /// Write To B/W RAM. After this command, data will be written to the B/W RAM until another command is sent.
            /// Address pointers will advance accordingly.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            WriteBackWhiteRAM = 0x24,

            /// <summary>
            /// Write To RED RAM. After this command, data will be written to the RED RAM until another command is sent.
            /// Address pointers will advance accordingly.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            WriteRedRAM = 0x26,

            /// <summary>
            /// Read RAM. After this command, data read on the MCU bus will fetch data from RAM.
            /// The first byte read is dummy data.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            ReadRAM = 0x27,

            /// <summary>
            /// Enter VCOM sensing conditions.
            /// The Busy pin will read high during this operation.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            VCOMSense = 0x28,

            /// <summary>
            /// VCOM Sense Duration. Stabling time between entering VCOM sending mode and reading is acquired.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            VCOMSenseDuration = 0x29,

            /// <summary>
            /// Program VCOM register into OTP.
            /// The Busy pin will read high during this operation.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            ProgramVCOMOTP = 0x2a,

            /// <summary>
            /// Write Register for VCOM Control. This command is used to reduce glitch when ACVCOM toggle.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            WriteRegisterControlVCOM = 0x2b,

            /// <summary>
            /// Write VCOM register from MCU interface.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            WriteVCOMRegister = 0x2c,

            /// <summary>
            /// OTP Register Read for Display Options.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            OTPRegisterReadDisplayOption = 0x2d,

            /// <summary>
            /// Read USER ID.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            UserIdRead = 0x2e,

            /// <summary>
            /// Read Status Bit.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            StatusBitRead = 0x2f,

            /// <summary>
            /// Program Waveform Setting OTP. Contents should be written to RAM before sending this command.
            /// The Busy pin will read high during this operation.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            ProgramWSOTP = 0x30,

            /// <summary>
            /// Load OTP of Waveform Setting.
            /// The Busy pin will read high during this operation.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            LoadWSOTP = 0x31,

            /// <summary>
            /// Write LUT Register from MCU interface.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            WriteLUTRegister = 0x32,

            /// <summary>
            /// CRC Calculation Command. For information, refer to SSD1681 application note.
            /// The Busy pin will read high during this operation.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            CrcCalculation = 0x34,

            /// <summary>
            /// CRC Status Read.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            CrcStatusRead = 0x35,

            /// <summary>
            /// Program OTP Selection according to the <see cref="WriteRegisterForDisplayOption"/> and <see cref="WriteRegisterForUserId"/>.
            /// The Busy pin will read high during this operation.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            ProgramOTPSelection = 0x36,

            /// <summary>
            /// Write Register for Display Option.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            WriteRegisterForDisplayOption = 0x37,

            /// <summary>
            /// Write Register for USER ID.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            WriteRegisterForUserId = 0x38,

            /// <summary>
            /// OTP Program Mode.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            OTPProgramMode = 0x39,

            /// <summary>
            /// Set Border Waveform Control values.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            BorderWaveformControl = 0x3c,

            /// <summary>
            /// Option for LUT end.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            EndOption = 0x3f,

            /// <summary>
            /// Read RAM Option.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            ReadRAMOption = 0x41,

            /// <summary>
            /// Set RAM X-Address Start/End position.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            SetRAMAddressXStartEndPosition = 0x44,

            /// <summary>
            /// Set RAM Y-Address Start/End position.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            SetRAMAddressYStartEndPosition = 0x45,

            /// <summary>
            /// Auto Write RED RAM for regular pattern.
            /// The Busy pin will read high during this operation.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            AutoWriteRAMForRegularPatternRed = 0x46,

            /// <summary>
            /// Auto Write B/W RAM for regular pattern.
            /// The Busy pin will read high during this operation.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            AutoWriteRAMForRegularPatternBlackWhite = 0x47,

            /// <summary>
            /// Set RAM X-Address Counter.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            SetRAMAddressCounterX = 0x4e,

            /// <summary>
            /// Set RAM Y-Address Counter.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            SetRAMAddressCounterY = 0x4f,

            /// <summary>
            /// NOP. This command is an empty command; it does not have any effect on the display module.
            /// However, it can be used to terminate frame memory write or read commands.
            /// </summary>
            /// <remarks>
            /// Refer to the datasheet for detailed information about the command and its parameters.
            /// </remarks>
            NOP = 0x7f
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
            if (!disposed)
            {
                if (disposing)
                {
                    resetPin.Dispose();
                    busyPin.Dispose();
                    dataCommandPin.Dispose();
                    spiDevice.Dispose();

                    frameBuffer2bpp = null;
                }

                disposed = true;
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
