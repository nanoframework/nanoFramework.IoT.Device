//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System;
using System.Device.Spi;
using System.Diagnostics;
using System.IO;

namespace Iot.Device.XPT2046
{
    public class Xpt2046 : IDisposable
    {
        private readonly SpiDevice _spiDevice;
        private readonly int _xMax;
        private readonly int _yMax;
        private bool _disposedValue = false;

        private const byte StartBit = 0b10000000;
        private const int maxConversion = 0b111111111111;

        enum PowerMode
        {
            PowerDown = 0,              //Power Down between conversions
            RefOff_ADCOn_IntOff = 1,    //Reference is off and ADC is on. Interupt off (used for double ended conversion)
            RefOn_ADCOff_IntOn = 2,     //Reference is on and ADC is off. Interupt on
            AlwaysOn = 3                //Always on. Interupt is disabled
        }

        enum MultiplexerChannel : byte
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

        enum ConversionMode
        {
            Bits12 = 0b00000000,
            Bits8 = 0b00001000
        }

        enum ReferenceSelectMode
        {
            DoubleEnded = 0b00000000,
            SingleEnded = 0b00000100
        }

        //See https://docs.nanoframework.net/content/getting-started-guides/spi-explained.html

        /// <summary>
        /// Creates an instance of an Xpt2046
        /// </summary>
        /// <param name="spiDevice">The spiDevice connected to the touchscreen controller</param>
        /// <param name="xMax">The X size of the screen</param>
        /// <param name="yMax">The Y size of the screen</param>
        public Xpt2046(SpiDevice spiDevice, int xMax = 320, int yMax = 240)
        {
            _spiDevice = spiDevice;
            _xMax = xMax;
            _yMax = yMax;
        }

        /// <summary>
        /// Returns the version of the Xpt2046 driver.
        /// </summary>
        public string GetVersion()
        {
            return "0.2";
        }

        /// <summary>
        /// Gets the current point from the touchscreen controller.
        /// </summary>
        public Point GetPoint()
        {
            // Get best of 3 samples
            Point[] samples = new Point[3];
            samples[0] = read();
            samples[1] = read();
            samples[2] = read();

            // Get the average of the two closest samples
            return new Point
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


        private Point read()
        {
            // Overlapped buffer: control, 0, control, 0, control, 0, control, 0, 0
            // Total 9 bytes: each control overlaps with previous read except the last
            SpanByte writeBuffer = new byte[9];
            SpanByte readBuffer = new byte[9];

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

            // Single SPI transaction for all overlapped measurements
            _spiDevice.TransferFullDuplex(writeBuffer, readBuffer);

            // Extract values from overlapped read buffer
            // Skip first byte (dummy), then read pairs
            var Z1 = ((int)readBuffer[1]) * 256 + readBuffer[2];  // skip[0], read[1,2]
            var Z2 = ((int)readBuffer[3]) * 256 + readBuffer[4];  // skip[2], read[3,4] 
            var X = ((int)readBuffer[5]) * 256 + readBuffer[6];   // skip[4], read[5,6]
            var Y = ((int)readBuffer[7]) * 256 + readBuffer[8];   // skip[6], read[7,8]

            Z2 = Z2 == 0 ? 1 : Z2;
            //Got this from the datasheet but might not be correct
            var weight = (X / 4096) * ((Z1 / Z2) - 1);

            //Scale the X and Y values to the screen size
            var xScaled = (((X >> 3) & maxConversion) * _xMax) / maxConversion;
            var yScaled = (((Y >> 3) & maxConversion) * _yMax) / maxConversion;

            return new Point() { Weight = weight, X = xScaled, Y = yScaled };
        }

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
