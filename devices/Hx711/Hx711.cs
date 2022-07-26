// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using System.Diagnostics;
using System.Threading;

namespace Iot.Device.Hx711
{
    /// <summary>
    /// Scale class proving a wrapper to the Hx711 to be used as a scale for weight measurements.
    /// </summary>
    public class Scale
    {
        // pulse train required to read a sample and setup gain factor for next reading
        private readonly byte[] _readSamplePulseTrain;

        // sample buffer to hold data read from DOUT
        private readonly byte[] _readSampleBuffer;

        // setup Dout wait buffers
        private readonly byte[] _clkWaitDoutBuffer;
        private readonly byte[] _doutWaitBuffer;

        private readonly SpiDevice _spiDevice;

        /// <summary>
        /// This is the default clock frequency to use in order to generate the expected signal for communicating with the Hx711.
        /// </summary>
        public const int DefaultClockFrequency = 700_000;

        /// <summary>
        /// Gets or sets the value that's subtracted from the actual reading.
        /// </summary>
        public double Offset { get; set; } = 0;

        /// <summary>
        /// Gets or sets the gain factor that the Hx711 uses when sampling.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="GainLevel.GainA128"/>.
        /// </remarks>
        public GainLevel Gain { get; set; }

        /// <summary>
        /// Gets or sets the number of samples that will be taken and then averaged when performing a <see cref="Read"/> operation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The default value is 3 samples.
        /// </para>
        /// <para>
        /// The number of samples to take has impact on the time it takes for a <see cref="Read"/> operation to complete. The Hx711, in the current configuration, as a capability of sampling 10 samples per second.
        /// </para>
        /// </remarks>
        public uint SampleAveraging { get; set; } = 3;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scale"/> class.
        /// </summary>
        /// <param name="spiDevice">The <see cref="SpiDevice"/> that is used as channel to communicate with the Hx711.</param>
        /// <param name="gain"><see cref="GainLevel"/> that will be used for the scale. If not provided, the default is <see cref="GainLevel.GainA128"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"> <see cref="GainLevel.None"/>.</exception>
        public Scale(
            SpiDevice spiDevice,
            GainLevel gain = GainLevel.GainA128)
        {
            if (gain == GainLevel.None)
            {
                throw new ArgumentOutOfRangeException();
            }

            _spiDevice = spiDevice;
            Gain = gain;

            // setup the pulse train required to read a sample AND setup gain factor for next reading
            // gain factor from property
            // these will produce a train of signals at MOSI pin that will feed the PC_CLK pin of Hx711
            _readSamplePulseTrain = new byte[]
            {
                0b1010_1010,
                0b1010_1010,
                0b1010_1010,
                0b1010_1010,
                0b1010_1010,
                0b1010_1010,
                (byte)Gain
            };

            // setup buffer to hold data read from DOUT
            _readSampleBuffer = new byte[7];

            // setup wait DOUT buffers
            _clkWaitDoutBuffer = new byte[] { 0x00 };
            _doutWaitBuffer = new byte[1];
        }

        /// <summary>
        /// Read weight from the scale.
        /// </summary>
        /// <returns>The weight reading from the load cell.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <see cref="SampleAveraging"/> is set to 0.</exception>
        public double Read()
        {
            if (SampleAveraging == 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            return ReadValue() - Offset;
        }

        /// <summary>
        /// Sets the <see cref="Offset"/> value for tare weight.
        /// </summary>
        public void Tare()
        {
            Offset = ReadValue();
        }

        /// <summary>
        /// Puts the device into power down mode.
        /// </summary>
        public void PowerDown()
        {
            // transition of CLK signal low > high with 60us
            // 1 bit ~= 1.5uS, 16*4 *1.5uS = 96uS
            _spiDevice.Write(new ushort[] { 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF });
        }

        /// <summary>
        /// Wakes up and resets the device. Optional set gain level and channel.
        /// </summary>
        /// <param name="gain"><see cref="GainLevel"/> that will be used for the scale. If not provided, the default is <see cref="GainLevel.GainA128"/>.</param>
        public void PowerUp(GainLevel gain = GainLevel.None)
        {
            // PowerDown then PowerUP to activate on-chip power on rest circuitry
            PowerDown();

            // set PD_CLK low to awake and reset to default mode GainA128
            // Wait for DOUT low means HX711 ready to accept new commands
            WaitForConversion();
            
            // switch to another channel mode if it is specified
            if (gain != GainLevel.None)
            {
                Gain = gain;
                _readSamplePulseTrain[_readSamplePulseTrain.Length - 1] = (byte)Gain;
                SetChannelAndGainFactor();
            }
        }

        private double ReadValue()
        {
            Debug.WriteLine("INFO: Reading sample.");

            // setup buffer to drive PD_SCK
            SpanByte clkTrain = new SpanByte(_readSamplePulseTrain);

            // setup buffer to hold data read from DOUT
            SpanByte readBuffer = new SpanByte(_readSampleBuffer);

            // setup array to hold readings for averaging
            int[] values = new int[SampleAveraging];

            for (int round = 0; round < SampleAveraging; round++)
            {
                // need to way for the next conversion
                if (WaitForConversion())
                {
                    // perform SPI transaction
                    _spiDevice.TransferFullDuplex(clkTrain, readBuffer);

                    values[round] = ParseRawData(readBuffer);
                }
            }

            return ComputeAverage(values);
        }

        private bool WaitForConversion()
        {
            Debug.WriteLine("INFO: Setup sampling to detect that a sample is ready");

            // send it in full duplex mode to be platform independent to not let spi send FF's by default
            SpanByte clkWaitDoutBuffer = new SpanByte(_clkWaitDoutBuffer);
            SpanByte doutWaitBuffer = new SpanByte(_doutWaitBuffer);
            _spiDevice.TransferFullDuplex(clkWaitDoutBuffer, doutWaitBuffer);
            var currentDout = doutWaitBuffer[0];

            while (currentDout != 0)
            {
                Thread.Sleep(10);
                _spiDevice.TransferFullDuplex(clkWaitDoutBuffer, doutWaitBuffer);
                currentDout = doutWaitBuffer[0];
            }

            return true;
        }

        private void SetChannelAndGainFactor()
        {
            // send N clock pulses according to the set gain factor
            // 1 clock pulse per gain factor
            Debug.WriteLine("INFO: Setting channel and gain.");

            // setup buffer to drive PD_SCK
            SpanByte clkTrain = new SpanByte(_readSamplePulseTrain);

            // setup buffer to hold data read from DOUT
            SpanByte readBuffer = new SpanByte(_readSampleBuffer);

            if (WaitForConversion())
            {
                // perform SPI transaction
                _spiDevice.TransferFullDuplex(clkTrain, readBuffer);
            }
        }

        private int ParseRawData(SpanByte readBuffer)
        {
            uint value24bit = 0;
            int rotationFactor = 20;

            // raw data is received in as 24 bits in 2’s complement format. MSB first.
            // When input differential signal goes out of the 24 bit range, the output data will be saturated
            // at 800000h(MIN) or 7FFFFFh(MAX), until the input signal comes back to the input range.

            // don't care about the last position (it's the setting of gain factor)
            for (int i = 0; i < readBuffer.Length - 1; i++, rotationFactor -= 4)
            {
                value24bit |= (uint)ParseNibble(readBuffer[i]) << rotationFactor;
            }

            return ConvertFrom24BitTwosComplement(value24bit);
        }

        private byte ParseNibble(byte value)
        {
            // take the even bits
            // because we can be sure that on the second half of the clock cycle the value in the bit it's the one output from the device
            int finalValue = 0;
            int mask = 0b0100_0000;

            for (int i = 3; i >= 0; i--)
            {
                // capture bit value at mask position
                // add bit to nibble
                finalValue <<= 1;
                if ((value & mask) != 0)
                {
                    finalValue++;
                }

                mask >>= 2;
            }

            return (byte)finalValue;
        }

        private int ConvertFrom24BitTwosComplement(uint twosComp)
        {
            // convert from 2's cpmplement 24 bit to int (32 bit)
            int normalValue = ((twosComp & 0x800000) != 0) ? (0 - (int)((twosComp ^ 0xffffff) + 1)) : (int)twosComp;
            return normalValue;
        }

        private double ComputeAverage(int[] values)
        {
            double value = 0;

            for (int i = 0; i < values.Length; i++)
            {
                value += values[i];
            }

            return value / values.Length;
        }
    }
}
