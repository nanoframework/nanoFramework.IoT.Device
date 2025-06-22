using System;
using System.Device.SPI;
using System.IO;

namespace Iot.Device.XPT2046
{
    public class XPT2046 : IDisposable
    {
        private readonly SpiDevice _spiDevice;
        private bool _disposedValue = false;

        enum PowerMode {
            PowerDown = 0,              //Power Down between conversions
            RefOff_ADCOn_IntOff = 1,    //Reference is off and ADC is on. Interupt off (used for double ended conversion)
            RefOn_ADCOff_IntOn = 2,     //Reference is on and ADC is off. Interupt on
            AlwaysOn = 3                //Always on. Interupt is disabled
        }

        enum MultiplexerChannel {
            Temperature = 0,
            Battery = 1,
            AuxIn = 6,
            Z1 = 3,
            Z2 = 4,
            X = 1,
            Y = 1
        }

        enum ConversionMode {
            Bits12 = 0,
            Bits8 = 1
        }
        
        enum ReferenceSelectMode {
            DoubleEnded = 0,
            SingleEnded = 1
        }

        //See https://docs.nanoframework.net/content/getting-started-guides/spi-explained.html

        /// Creates an instance of an Xpt2046
        /// </summary>
        /// <param name="spiBus">The SpiConfiguration connected to the touchscreen controller</param>
        public Xpt2046(SpiDevice spiDevice)
        {
            _spiDevice = spiDevice;
        }

        public string GetVersion() {
            return "0.1";
        }

        public point GetPoint() {
            //TODO:
            //C/S low? or handled by SPI device?
            //Send control byte

/*
Control Byte
1 - Start Bit
Address x 3
Mode 12/8bit
Single End/Double End
Power Mode x 3

//Examples from other people's drivers. Also used 90 and D0 for X and Y
B1 = 0b10110001 "Z1"
C1 = 0b11000001 "Z2"
91 = 0b10010001 X
D1 = 0b11010001 Y
D0 = 0b11010000 Y + Power Down
*/

            var ControlByte = (1 << 7)
                            | (MultiplexerChannel.Z1 << 4)
                            | (ConversionMode.Bits12 << 3)
                            | (ReferenceSelectMode.DoubleEnded << 2)
                            | (PowerMode.RefOff_ADCOn_IntOff);

            spiDevice.WriteByte(ControlByte);
            //If we have busy line connected then we'd wait for it to go low
            //Before reading 16 bytes
            SpanByte readBufferSpanByte = new byte[2];
            spiDevice.Read(readBufferSpanByte);

            var Z1 = readBufferSpanByte[0] * 4096 + readBufferSpanByte[1];

            ControlByte = (ControlByte & 0b1000_1111) | MultiplexerChannel.Z2;

            spiDevice.WriteByte(ControlByte);
            SpanByte readBufferSpanByte = new byte[2];
            spiDevice.Read(readBufferSpanByte);

            var Z2 = readBufferSpanByte[0] * 4096 + readBufferSpanByte[1];

            //Some of the more sophisticated drivers get 3 samples and do a best 2 average (the 2 closest to each other)
            return new Point();
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
