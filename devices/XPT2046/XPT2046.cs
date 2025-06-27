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

        /// Creates an instance of an Xpt2046
        /// </summary>
        /// <param name="spiBus">The SpiConfiguration connected to the touchscreen controller</param>
        /// <param name="xMax">The X size of the screen</param>
        /// <param name="yMax">The Y size of the screen</param>
        public Xpt2046(SpiDevice spiDevice, int xMax = 320, int yMax = 240)
        {
            _spiDevice = spiDevice;
            _xMax = xMax;
            _yMax = yMax;
        }

        public string GetVersion()
        {
            return "0.1";
        }

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

        public int BestTwoOfThreeAverage(int i1, int i2, int i3)
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
            // Some of the more sophisticated drivers get 3 samples and do a best 2 average (the 2 closest to each other)
            SpanByte readBufferSpanByte = new byte[3];

            byte controlByte = (byte)StartBit | (byte)MultiplexerChannel.Z1
                                              | (byte)ConversionMode.Bits12
                                              | (byte)ReferenceSelectMode.DoubleEnded
                                              | (byte)PowerMode.RefOff_ADCOn_IntOff;

            writeRead("Z1", controlByte, ref readBufferSpanByte, ref readBufferSpanByte);

            var Z1 = ((int)readBufferSpanByte[1]) * 256 + readBufferSpanByte[2];

            controlByte = (byte)StartBit | (byte)MultiplexerChannel.Z2
                                         | (byte)ConversionMode.Bits12
                                         | (byte)ReferenceSelectMode.DoubleEnded
                                         | (byte)PowerMode.RefOff_ADCOn_IntOff;

            writeRead("Z2", controlByte, ref readBufferSpanByte, ref readBufferSpanByte);

            var Z2 = ((int)readBufferSpanByte[1]) * 256 + readBufferSpanByte[2];

            controlByte = (byte)StartBit | (byte)MultiplexerChannel.X
                                         | (byte)ConversionMode.Bits12
                                         | (byte)ReferenceSelectMode.DoubleEnded
                                         | (byte)PowerMode.RefOff_ADCOn_IntOff;

            writeRead("X", controlByte, ref readBufferSpanByte, ref readBufferSpanByte);
            
            var X = ((int)readBufferSpanByte[1]) * 256 + readBufferSpanByte[2];

            //Finally set the mode back to power on so we can re-enable the interupt
            controlByte = (byte)StartBit | (byte)MultiplexerChannel.Y
                                         | (byte)ConversionMode.Bits12
                                         | (byte)ReferenceSelectMode.DoubleEnded
                                         | (byte)PowerMode.PowerDown;

            writeRead("Y", controlByte, ref readBufferSpanByte, ref readBufferSpanByte);

            var Y = ((int)readBufferSpanByte[1]) * 256 + readBufferSpanByte[2];

            Z2 = Z2 == 0 ? 1 : Z2;
            //Got this from the datasheet but might not be correct
            var weight = (X / 4096) * ((Z1 / Z2) - 1);

            //Scale the X and Y values to the screen size
            var xScaled = (((X >> 3) & maxConversion) * _xMax) / maxConversion;
            var yScaled = (((Y >> 3) & maxConversion) * _yMax) / maxConversion;

            return new Point() { Weight = weight, X = xScaled, Y = yScaled };
        }

        private void writeRead(string label, byte control,ref SpanByte writeBuffer, ref SpanByte readBuffer)
        {
            writeBuffer[0] = control;
            writeBuffer[1] = 0;
            writeBuffer[2] = 0;

            _spiDevice.TransferFullDuplex(writeBuffer, readBuffer);

            /*
            Debug.WriteLine($"{label}: {control}");
            Debug.Write("Raw: ");
            WriteByteInBinary(readBuffer[0]);
            WriteByteInBinary(readBuffer[1]);
            WriteByteInBinary(readBuffer[2]);
            Debug.WriteLine();
            */
        }

        private void WriteByteInBinary(byte b)
        {
            var travellingMask = 0b10000000;
            for (var i=0; i < 8; i++)
            {
                if ((b & travellingMask) == travellingMask)
                {
                    Debug.Write("1");
                }
                else
                {
                    Debug.Write("0");
                }
                travellingMask >>= 1;
            }

            Debug.Write(" ");
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
