// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Device.Spi;
using System.Diagnostics;
using System.IO;

namespace Iot.Device.Xpt2046
{
    /// <summary>
    /// Class representing the XPT2046 device driver.
    /// </summary>
    public class Xpt2046 : IDisposable
    {
        private const byte StartBit = 0b10000000;
        private const int MaxConversion = 0b111111111111;

        private readonly SpiDevice _spiDevice;
        private readonly int _xMax;
        private readonly int _yMax;
        private bool _disposedValue = false;

        private enum PowerMode
        {
            // Power Down between conversions
            PowerDown = 0b00000000,

            // Reference is off and ADC is on. Interupt off (used for double ended conversion)
            RefOff_ADCOn_IntOff = 0b00000001,

            // Reference is on and ADC is off. Interupt on
            RefOn_ADCOff_IntOn = 0b00000010,
            
            // Always on. Interupt is disabled
            AlwaysOn = 0b00000011
        }

        private enum MultiplexerChannel : byte
        {
            Temperature0 = 0b00000000,
            Temperature1 = 0b01110000,
            Battery = 0b00100000,
            AuxIn = 0b01100000,
            Z1 = 0b00110000,
            Z2 = 0b01000000,
            X = 0b00010000,
            Y = 0b01010000
        }

        private enum ConversionMode
        {
            Bits12 = 0b00000000,
            Bits8 = 0b00001000
        }

        private enum ReferenceSelectMode
        {
            DoubleEnded = 0b00000000,
            SingleEnded = 0b00000100
        }

        // See https://docs.nanoframework.net/content/getting-started-guides/spi-explained.html

        /// <summary>
        /// Initializes a new instance of the <see cref="Xpt2046" /> class.
        /// </summary>
        /// <param name="spiDevice">The spiDevice connected to the touchscreen controller.</param>
        /// <param name="xMax">The X size of the screen.</param>
        /// <param name="yMax">The Y size of the screen.</param>
        public Xpt2046(SpiDevice spiDevice, int xMax = 320, int yMax = 240)
        {
            _spiDevice = spiDevice;
            _xMax = xMax;
            _yMax = yMax;
        }

        /// <summary>
        /// Returns the version of the Xpt2046 driver.
        /// </summary>
        /// <returns>
        /// A string with the current driver version.
        /// </returns>
        public string GetVersion()
        {
            return "1.0";
        }

        /// <summary>
        /// Gets the current point from the touchscreen controller.
        /// </summary>
        /// <returns>
        /// A point representing the x,y and weight of the touch.
        /// </returns>
        public TouchPoint GetPoint()
        {
            // Get best of 3 samples
            TouchPoint[] samples = new TouchPoint[3];
            samples[0] = Read();
            samples[1] = Read();
            samples[2] = Read();

            // Get the average of the two closest samples.
            return new TouchPoint
            {
                X = BestTwoOfThreeAverage(samples[0].X, samples[1].X, samples[2].X),
                Y = BestTwoOfThreeAverage(samples[0].Y, samples[1].Y, samples[2].Y),
                Weight = BestTwoOfThreeAverage(samples[0].Weight, samples[1].Weight, samples[2].Weight)
            };
        }

        private int BestTwoOfThreeAverage(int i1, int i2, int i3)
        {
            // If you want to use Math.Abs there's a separate nanoFramework.System.Math package for that.
            var diff12 = i1 > i2 ? (i1 - i2) : (i2 - i1);
            var diff13 = i1 > i3 ? (i1 - i3) : (i3 - i1);
            var diff23 = i2 > i3 ? (i2 - i3) : (i3 - i2);

            if (diff12 <= diff13 && diff12 <= diff23)
            {
                return (i1 + i2) / 2;
            }
            else if (diff13 <= diff12 && diff13 <= diff23)
            {
                return (i1 + i3) / 2;
            }
            else
            {
                return (i2 + i3) / 2;
            }
        }

        private TouchPoint Read()
        {
            // Overlapped buffer: control, 0, control, 0, control, 0, control, 0, 0
            // Total 9 bytes: each control overlaps with previous read except the last
            Span<byte> writeBuffer = new byte[9];
            Span<byte> readBuffer = new byte[9];

            // Prepare all control bytes for overlapped transaction
            byte z1ControlByte = (byte)StartBit | (byte)MultiplexerChannel.Z1
                                               | (byte)ConversionMode.Bits12
                                               | (byte)ReferenceSelectMode.DoubleEnded
                                               | (byte)PowerMode.RefOff_ADCOn_IntOff;

            byte z2ControlByte = (byte)StartBit | (byte)MultiplexerChannel.Z2
                                               | (byte)ConversionMode.Bits12
                                               | (byte)ReferenceSelectMode.DoubleEnded
                                               | (byte)PowerMode.RefOff_ADCOn_IntOff;

            byte xControlByte = (byte)StartBit | (byte)MultiplexerChannel.X
                                              | (byte)ConversionMode.Bits12
                                              | (byte)ReferenceSelectMode.DoubleEnded
                                              | (byte)PowerMode.RefOff_ADCOn_IntOff;

            // Final Y measurement with PowerDown to re-enable interrupt
            byte yControlByte = (byte)StartBit | (byte)MultiplexerChannel.Y
                                              | (byte)ConversionMode.Bits12
                                              | (byte)ReferenceSelectMode.DoubleEnded
                                              | (byte)PowerMode.PowerDown;

            // Fill write buffer with overlapped pattern: control, 0, control, 0, control, 0, control, 0, 0
            writeBuffer[0] = z1ControlByte;  // Z1 control
            writeBuffer[1] = 0;              // padding
            writeBuffer[2] = z2ControlByte;  // Z2 control (while reading Z1)
            writeBuffer[3] = 0;              // padding
            writeBuffer[4] = xControlByte;   // X control (while reading Z2)
            writeBuffer[5] = 0;              // padding
            writeBuffer[6] = yControlByte;   // Y control (while reading X)
            writeBuffer[7] = 0;              // padding for Y read
            writeBuffer[8] = 0;              // padding for Y read

            // Single SPI transaction for all overlapped measurements is required as device
            // resets when chip select is taken high
            _spiDevice.TransferFullDuplex(writeBuffer, readBuffer);

            // Skip first byte (dummy), then read pairs
            var z1 = (((int)readBuffer[1]) * 256) + readBuffer[2];  
            var z2 = (((int)readBuffer[3]) * 256) + readBuffer[4];  
            var x = (((int)readBuffer[5]) * 256) + readBuffer[6];   
            var y = (((int)readBuffer[7]) * 256) + readBuffer[8];   

            // Adjust the weighting to a more usuable value, recommended by the datasheet.
            z2 = z2 == 0 ? 1 : z2;
            var weight = (x / 4096) * ((z1 / z2) - 1);

            // Scale the X and Y values to the screen size.
            var xScaled = (((x >> 3) & MaxConversion) * _xMax) / MaxConversion;
            var yScaled = (((y >> 3) & MaxConversion) * _yMax) / MaxConversion;

            return new TouchPoint() { Weight = weight, X = xScaled, Y = yScaled };
        }

        /// <summary>
        /// Called automatically by Using/IDisposable.
        /// </summary>
        public void Dispose()
        {
            if (!_disposedValue)
            {
                _spiDevice?.Dispose();
                _disposedValue = true;
            }

            GC.SuppressFinalize(this);
        }
    }
}
