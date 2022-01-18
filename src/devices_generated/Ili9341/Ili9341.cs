// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Iot.Device.Ili9341
{
    /// <summary>
    /// The ILI9341 is a QVGA (Quarter VGA) driver integrated circuit that is used to control 240×320 VGA LCD screens.
    /// </summary>
    public partial class Ili9341 : IDisposable
    {
        /// <summary>
        /// Default frequency for SPI
        /// </summary>
        public const int DefaultSpiClockFrequency = 12_000_000;

        /// <summary>
        /// Default mode for SPI
        /// </summary>
        public const SpiMode DefaultSpiMode = SpiMode.Mode3;

        private const int ScreenWidthPx = 240;
        private const int ScreenHeightPx = 320;
        private const int DefaultSPIBufferSize = 0x1000;
        private const byte LcdPortraitConfig = 8 | 0x40;
        private const byte LcdLandscapeConfig = 44;

        private readonly int _dcPinId;
        private readonly int _resetPinId;
        private readonly int _backlightPin;
        private readonly int _spiBufferSize;
        private readonly bool _shouldDispose;

        private SpiDevice _spiDevice;
        private GpioController _gpioDevice;

        /// <summary>
        /// Initializes new instance of ILI9341 device that will communicate using SPI bus.
        /// </summary>
        /// <param name="spiDevice">The SPI device used for communication. This Spi device will be displayed along with the ILI9341 device.</param>
        /// <param name="dataCommandPin">The id of the GPIO pin used to control the DC line (data/command).</param>
        /// <param name="resetPin">The id of the GPIO pin used to control the /RESET line (data/command).</param>
        /// <param name="backlightPin">The pin for turning the backlight on and off, or -1 if not connected.</param>
        /// <param name="spiBufferSize">The size of the SPI buffer. If data larger than the buffer is sent then it is split up into multiple transmissions. The default value is 4096.</param>
        /// <param name="gpioController">The GPIO controller used for communication and controls the the <paramref name="resetPin"/> and the <paramref name="dataCommandPin"/>
        /// If no Gpio controller is passed in then a default one will be created and disposed when ILI9341 device is disposed.</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public Ili9341(SpiDevice spiDevice, int dataCommandPin, int resetPin, int backlightPin = -1, int spiBufferSize = DefaultSPIBufferSize, GpioController? gpioController = null, bool shouldDispose = true)
        {
            if (!InRange((uint)spiBufferSize, 0x1000, 0x10000))
            {
                throw new ArgumentException(nameof(spiBufferSize), "Value must be between 4096 and 65536.");
            }

            _spiDevice = spiDevice;
            _dcPinId = dataCommandPin;
            _resetPinId = resetPin;
            _backlightPin = backlightPin;
            _gpioDevice = gpioController ?? new GpioController();
            _shouldDispose = shouldDispose || gpioController is null;

            _gpioDevice.OpenPin(_dcPinId, PinMode.Output);
            _gpioDevice.OpenPin(_resetPinId, PinMode.Output);

            _spiBufferSize = spiBufferSize;

            if (_backlightPin != -1)
            {
                _gpioDevice.OpenPin(_backlightPin, PinMode.Output);
                TurnBacklightOn();
            }

            ResetDisplayAsync().Wait();

            SendCommand(Ili9341Command.SoftwareReset);
            SendCommand(Ili9341Command.DisplayOff);
            Thread.Sleep(10);
            SendCommand(Ili9341Command.MemoryAccessControl, LcdPortraitConfig);
            SendCommand(Ili9341Command.ColModPixelFormatSet, 0x55); // 16-bits per pixel
            SendCommand(Ili9341Command.FrameRateControlInNormalMode, 0x00, 0x1B);
            SendCommand(Ili9341Command.GammaSet, 0x01);
            SendCommand(Ili9341Command.ColumnAddressSet, 0x00, 0x00, 0x00, 0xEF); // width of the screen
            SendCommand(Ili9341Command.PageAddressSet, 0x00, 0x00, 0x01, 0x3F); // height of the screen
            SendCommand(Ili9341Command.EntryModeSet, 0x07);
            SendCommand(Ili9341Command.DisplayFunctionControl, 0x0A, 0x82, 0x27, 0x00);
            SendCommand(Ili9341Command.SleepOut);
            Thread.Sleep(120);
            SendCommand(Ili9341Command.DisplayOn);
            Thread.Sleep(100);
            SendCommand(Ili9341Command.MemoryWrite);
        }

        /// <summary>
        /// Convert a color structure to a byte tuple representing the colour in 565 format.
        /// </summary>
        /// <param name="color">The color to be converted.</param>
        /// <returns>
        /// This method returns the low byte and the high byte of the 16bit value representing RGB565 or BGR565 value
        ///
        /// byte    11111111 00000000
        /// bit     76543210 76543210
        ///
        /// For ColorSequence.RGB
        ///         RRRRRGGG GGGBBBBB
        ///         43210543 21043210
        ///
        /// For ColorSequence.BGR
        ///         BBBBBGGG GGGRRRRR
        ///         43210543 21043210
        /// </returns>
        private (byte Low, byte High) Color565(Color color)
        {
            // get the top 5 MSB of the blue or red value
            UInt16 retval = (UInt16)(color.R >> 3);
            // shift right to make room for the green Value
            retval <<= 6;
            // combine with the 6 MSB if the green value
            retval |= (UInt16)(color.G >> 2);
            // shift right to make room for the red or blue Value
            retval <<= 5;
            // combine with the 6 MSB if the red or blue value
            retval |= (UInt16)(color.B >> 3);
            return ((byte)(retval >> 8), (byte)(retval & 0xFF));
        }

        /// <summary>
        /// Send filled rectangle to the ILI9341 display.
        /// </summary>
        /// <param name="color">The color to fill the rectangle with.</param>
        /// <param name="x">The x co-ordinate of the point to start the rectangle at in pixels.</param>
        /// <param name="y">The y co-ordinate of the point to start the rectangle at in pixels.</param>
        /// <param name="w">The width of the rectangle in pixels.</param>
        /// <param name="h">The height of the rectangle in pixels.</param>
        public void FillRect(Color color, uint x, uint y, uint w, uint h)
        {
            SpanByte colourBytes = new byte[2]; // create a short span that holds the colour data to be sent to the display
            SpanByte displayBytes = new byte[(int)(w * h * 2)]; // span used to form the data to be written out to the SPI interface

            // set the colourbyte array to represent the fill colour
            (colourBytes[0], colourBytes[1]) = Color565(color);

            // set the pixels in the array representing the raw data to be sent to the display
            // to the fill color
            for (int i = 0; i < w * h; i++)
            {
                displayBytes[i * 2 + 0] = colourBytes[0];
                displayBytes[i * 2 + 1] = colourBytes[1];
            }

            // specify a location for the rows and columns on the display where the data is to be written
            SetWindow(x, y, x + w - 1, y + h - 1);

            // write out the pixel data
            SendData(displayBytes);
        }

        /// <summary>
        /// Clears screen
        /// </summary>
        public void ClearScreen()
        {
            FillRect(Color.Black, 0, 0, ScreenWidthPx, ScreenHeightPx);
        }

        /// <summary>
        /// Resets the display.
        /// </summary>
        public async Task ResetDisplayAsync()
        {
            _gpioDevice.Write(_resetPinId, PinValue.High);
            await Task.Delay(20).ConfigureAwait(false);
            _gpioDevice.Write(_resetPinId, PinValue.Low);
            await Task.Delay(20).ConfigureAwait(false);
            _gpioDevice.Write(_resetPinId, PinValue.High);
            await Task.Delay(20).ConfigureAwait(false);
        }

        /// <summary>
        /// This command turns the backlight panel off.
        /// </summary>
        public void TurnBacklightOn()
        {
            if (_backlightPin == -1)
            {
                throw new InvalidOperationException("Backlight pin not set");
            }

            _gpioDevice.Write(_backlightPin, PinValue.High);
        }

        /// <summary>
        /// This command turns the backlight panel off.
        /// </summary>
        public void TurnBacklightOff()
        {
            if (_backlightPin == -1)
            {
                throw new InvalidOperationException("Backlight pin not set");
            }

            _gpioDevice.Write(_backlightPin, PinValue.Low);
        }

        private void SetWindow(uint x0 = 0, uint y0 = 0, uint x1 = ScreenWidthPx - 1, uint y1 = ScreenWidthPx - 1)
        {
            SendCommand(Ili9341Command.ColumnAddressSet);
            SpanByte data = new byte[4]
            {
                (byte)(x0 >> 8),
                (byte)x0,
                (byte)(x1 >> 8),
                (byte)x1,
            };
            SendData(data);
            SendCommand(Ili9341Command.PageAddressSet);
            SpanByte data2 = new byte[4]
            {
                (byte)(y0 >> 8),
                (byte)y0,
                (byte)(y1 >> 8),
                (byte)y1,
            };
            SendData(data2);
            SendCommand(Ili9341Command.MemoryWrite);
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
            return value >= start && value <= end;
        }

        /// <summary>
        /// Send a command to the the display controller along with associated parameters.
        /// </summary>
        /// <param name="command">Command to send.</param>
        /// <param name="commandParameters">parameteters for the command to be sent</param>
        private void SendCommand(Ili9341Command command, params byte[] commandParameters)
        {
            SendCommand(command, commandParameters.AsSpan());
        }

        /// <summary>
        /// Send a command to the the display controller along with parameters.
        /// </summary>
        /// <param name="command">Command to send.</param>
        /// <param name="data">Span to send as parameters for the command.</param>
        private void SendCommand(Ili9341Command command, SpanByte data)
        {
            SpanByte commandSpan = new byte[]
            {
                (byte)command
            };

            SendSPI(commandSpan, true);

            if (data != null && data.Length > 0)
            {
                SendSPI(data);
            }
        }

        /// <summary>
        /// Send data to the display controller.
        /// </summary>
        /// <param name="data">The data to send to the display controller.</param>
        private void SendData(SpanByte data)
        {
            SendSPI(data, blnIsCommand: false);
        }

        /// <summary>
        /// Write a block of data to the SPI device
        /// </summary>
        /// <param name="data">The data to be sent to the SPI device</param>
        /// <param name="blnIsCommand">A flag indicating that the data is really a command when true or data when false.</param>
        private void SendSPI(SpanByte data, bool blnIsCommand = false)
        {
            int index = 0;
            int len;

            // set the DC pin to indicate if the data being sent to the display is DATA or COMMAND bytes.
            _gpioDevice.Write(_dcPinId, blnIsCommand ? PinValue.Low : PinValue.High);

            // write the array of bytes to the display. (in chunks of SPI Buffer Size)
            do
            {
                // calculate the amount of spi data to send in this chunk
                len = Math.Min(data.Length - index, _spiBufferSize);
                // send the slice of data off set by the index and of length len.
                _spiDevice.Write(data.Slice(index, len));
                // add the length just sent to the index
                index += len;
            }
            while (index < data.Length); // repeat until all data sent.
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _gpioDevice?.Dispose();
                _gpioDevice = null!;
            }

            _spiDevice?.Dispose();
            _spiDevice = null!;
        }
    }
}
