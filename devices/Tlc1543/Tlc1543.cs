// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device;
using System.Device.Gpio;
using System.Device.Spi;

namespace Iot.Device.Tlc1543
{
    /// <summary>
    /// Tlc1543 device.
    /// </summary>
    public class Tlc1543 : IDisposable
    {
        /// <summary>
        /// DataBitLength to set on SPI device.
        /// </summary>
        public const int SpiDataBitLength = 5;

        // 21 microseconds wait time in case end-of-conversion pin is disconnected (spec: operating characteristic, t_conv)
        // Adding 999 is to perform Math.Ceil without going through double and Math.Ceil
        private static readonly TimeSpan ConversionTime = new TimeSpan(((21 * TimeSpan.TicksPerMillisecond) + 999) / 1000);
        private readonly int _endOfConversion;
        private readonly bool _shouldDispose;
        private SpiDevice _spiDevice;
        private GpioController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tlc1543" /> class.
        /// </summary>
        /// <param name="spiDevice">Device used for SPI communication.</param>
        /// <param name="endOfConversion">End of Conversion pin, if GpioController is not provided logical numbering scheme is used (default for GpioController).</param>
        /// <param name="controller">The GPIO controller for defined external pins. If not specified, the default controller will be used.</param>
        /// <param name="shouldDispose">True to dispose the GPIO controller and SPI device.</param>
        public Tlc1543(SpiDevice spiDevice, int endOfConversion = -1, GpioController? controller = null, bool shouldDispose = true)
        {
            if (spiDevice == null)
            {
                throw new ArgumentNullException(nameof(spiDevice));
            }

            if (spiDevice.ConnectionSettings.DataBitLength != SpiDataBitLength)
            {
                throw new ArgumentException($"SpiDevice is required to be have DataBitLength={SpiDataBitLength}.", nameof(spiDevice));
            }

            _spiDevice = spiDevice;
            _shouldDispose = controller == null || shouldDispose;
            _controller = controller ?? new GpioController();

            _endOfConversion = endOfConversion;

            if (_endOfConversion != -1)
            {
                _controller.OpenPin(_endOfConversion, PinMode.InputPullUp);
            }
        }

        private static bool IsValidChannel(Channel channel) => channel >= Channel.A0 && channel <= Channel.SelfTestMax;

        /// <summary>
        /// Reads previous reading and prepares next reading for specified channel.
        /// </summary>
        /// <param name="channelToCharge">Channel to prepare.</param>
        /// <returns>10 bit value corresponding to relative voltage level on channel.</returns>
        public int ReadPreviousAndChargeChannel(Channel channelToCharge)
        {
            if (!IsValidChannel(channelToCharge))
            {
                throw new ArgumentOutOfRangeException(nameof(channelToCharge));
            }

            SpanByte readBuffer = new byte[2];
            SpanByte writeBuffer = new byte[2];

            writeBuffer[0] = (byte)((int)channelToCharge << 1);
            _spiDevice.TransferFullDuplex(writeBuffer, readBuffer);

            int previousReading = ((readBuffer[0] & 0b11111) << 5) | (readBuffer[1] & 0b11111);

            // Max conversion time (21us) as seen in table on page 10 in TLC1543 documentation
            DelayHelper.Delay(ConversionTime, false);

            return previousReading;
        }

        /// <summary>
        /// Reads sensor value.
        /// First cycle: Ask for value on the channel <paramref name="channelNumber"/>.
        /// Second cycle: Return value from the channel while charging <paramref name="nextChannelToCharge"/>.
        /// </summary>
        /// <param name="channelNumber">Channel to be read.</param>
        /// <param name="nextChannelToCharge">Next channel to charge.</param>
        /// <returns>A 10 bit value corresponding to relative voltage level on specified device channel.</returns>
        public int ReadChannel(Channel channelNumber, Channel nextChannelToCharge = Channel.SelfTestHalf)
        {
            ReadPreviousAndChargeChannel(channelNumber);
            return ReadPreviousAndChargeChannel(nextChannelToCharge);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_controller != null)
            {
                if (_shouldDispose)
                {
                    _controller.Dispose();
                    _spiDevice?.Dispose();
                }
                else if (_endOfConversion != -1)
                {
                    _controller.ClosePin(_endOfConversion);
                }

                _controller = null;
                _spiDevice = null;
            }
        }
    }
}
