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

        private readonly SpiDevice _spiDevice;

        /// <summary>
        /// This is the default clock frequency to use in order to generate the expected signal for communicating with the Hx711.
        /// </summary>
        public const int DefaultClockFrequency = 700_000;

        /// <summary>
        /// Gets or sets the value that's subtracted from the actual reading.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Gets or sets the gain factor that the Hx711 uses when sampling.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="GainLevel.Gain128"/>.
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
        /// <param name="gain"><see cref="GainLevel"/> that will be used for the scale. If not provided, the default is <see cref="GainLevel.Gain128"/>.</param>
        public Scale(
            SpiDevice spiDevice,
            GainLevel gain = GainLevel.Gain128
            )
        {
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
            _spiDevice.Write(new ushort[] { 0xFFFF, 0xFFFF, 0xFFFF });
        }

        /// <summary>
        /// Wakes up the device from power down mode.
        /// </summary>
        public void PowerUp()
        {
            // only required if the device is in power down mode
            var currentDout = _spiDevice.ReadByte();

            if (currentDout != 0)
            {
                // transition of CLK signal high > low to wake-up device

                // set gain factor
                SetChannelAndGainFactor();
            }
        }

        private int ReadValue()
        {
            Debug.WriteLine("INFO: Reading sample.");

            // setup buffer to drive PD_SCK
            SpanByte clkTrain = new(_readSamplePulseTrain);

            // setup buffer to hold data read from DOUT
            SpanByte readBuffer = new(new byte[7]);

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

            var currentDout = _spiDevice.ReadByte();

            while (currentDout != 0)
            {
                Thread.Sleep(10);
                currentDout = _spiDevice.ReadByte();
            }

            return true;
        }

        private void SetChannelAndGainFactor()
        {
            // send N clock pulses according to the set gain factor
            // 1 clock pulse per gain factor
            _spiDevice.WriteByte((byte)Gain);
        }

        private int ParseRawData(SpanByte readBuffer)
        {
            int value = 0;
            int rotationFactor = 20;

            // raw data is received in as 24 bits in 2’s complement format. MSB first.
            // When input differential signal goes out of the 24 bit range, the output data will be saturated
            // at 800000h(MIN) or 7FFFFFh(MAX), until the input signal comes back to the input range.

            // don't care about the last position (it's the setting of gain factor)
            for (int i = 0; i < readBuffer.Length - 1; i++, rotationFactor -= 4)
            {
                value |= ParseNibble(readBuffer[i]) << rotationFactor;
            }

            return value;
        }

        private byte ParseNibble(byte value)
        {
            // take the even bits
            // because we can be sure that on the second half of the clock cycle the value in the bit it's the one output from the device
            value = (byte)(value & 0b01010101);

            int finalValue = 0;
            int mask = 0b1100_0000;

            for (int i = 3; i >= 0; i--)
            {
                // capture bit value at mask position
                // add bit to nibble
                if ((value & mask) > 0)
                {
                    finalValue |= 1 << i;
                }

                mask = mask >> 2;
            }

            return (byte)finalValue;
        }

        private int ComputeAverage(int[] values)
        {
            double value = 0;

            for (int i = 0; i < values.Length; i++)
            {
                value += values[i];
            }

            return (int)(value / values.Length);
        }
    }
}
