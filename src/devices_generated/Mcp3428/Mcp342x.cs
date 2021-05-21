// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Device.I2c;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Iot.Device.Mcp3428
{
    /// <summary>
    /// Base type for MCP342X ADC
    /// </summary>
    public abstract class Mcp342x : IDisposable
    {
        /// <summary>
        /// Protected constructor to easily generate MCP3426/7 devices whose only difference is channel count and I2C address
        /// </summary>
        /// <param name="i2cDevice">The i2 c device.</param>
        /// <param name="channels">The channels.</param>
        /// <autogeneratedoc />
        protected Mcp342x(I2cDevice i2cDevice, int channels)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            ChannelCount = channels;
            ReadValue(); // Don't like this in constructor, makes sure props are valid
        }

        /// <summary>
        /// Gets the last transmitted bytes. Debug function
        /// </summary>
        /// <value>The last bytes.</value>
        /// <autogeneratedoc />
        public ReadOnlyMemory<byte> LastBytes => _readBuffer;

        /// <summary>
        /// Channel most recently read
        /// </summary>
        /// <value>The last channel.</value>
        /// <autogeneratedoc />
        public byte LastChannel => LastConversion.Channel;

        /// <summary>
        /// ADC mode
        /// </summary>
        public AdcMode Mode
        {
            get => _mode;
            set
            {
                WriteConfig(Helpers.SetModeBit(LastConfigByte, value));
                _mode = value;
            }
        }

        /// <summary>
        /// Gets or sets the input gain.
        /// </summary>
        /// <value>The pga gain.</value>
        /// <autogeneratedoc />
        public AdcGain InputGain
        {
            get => _pgaGain;
            set
            {
                WriteConfig(Helpers.SetGainBits(LastConfigByte, value));
                _pgaGain = value;
            }
        }

        /// <summary>
        /// Gets or sets the bit resolution of the result.
        /// </summary>
        /// <value>The resolution.</value>
        /// <autogeneratedoc />
        public AdcResolution Resolution
        {
            get => _resolution;
            set
            {
                WriteConfig(Helpers.SetResolutionBits(LastConfigByte, value));
                _resolution = value;
            }
        }

        /// <summary>
        /// Reads the channel.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <returns>System.Double.</returns>
        /// <autogeneratedoc />
        public double ReadChannel(int channel) => ReadValue(channel);

        private readonly byte[] _readBuffer = new byte[3];
        private I2cDevice _i2cDevice;

        private bool _isReadyBit = false;
        private byte _lastChannel = 0xFF;
        private ConversionResult _lastConversion;
        private AdcMode _mode = AdcMode.Continuous;

        // Config params
        private AdcGain _pgaGain = AdcGain.X1;

        private AdcResolution _resolution = AdcResolution.Bit12;

        private byte LastConfigByte => _readBuffer[2];

        /// <summary>
        /// Initiates One-shot reading and waits for the conversion to finish.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <exception cref="System.IO.IOException">
        /// Device is not in One-Shot mode
        /// or
        /// ADC Conversion was not ready after {tries}
        /// </exception>
        /// <autogeneratedoc />
        protected void OneShotRead(int channel = -1)
        {
            if (Mode != AdcMode.OneShot)
            {
                throw new IOException("Device is not in One-Shot mode");
            }

            _isReadyBit = false;
            var conf = Helpers.SetReadyBit(LastConfigByte, false);
            if (channel >= 0 && channel != LastChannel)
            {
                conf = Helpers.SetChannelBits(conf, channel);
            }

            WriteConfig(conf);
            using (var source = new CancellationTokenSource(TimeSpan.FromMilliseconds(WaitTime * 5)))
            {
                WaitForConversion(TimeSpan.FromMilliseconds(WaitTime), cancellationToken: source.Token);

                if (!_isReadyBit)
                {
                    throw new IOException($"ADC Conversion was not ready after {WaitTime * 5} ms.");
                }
            }
        }

        /// <summary>
        /// Waits for conversion to complete
        /// </summary>
        /// <param name="waitSpan">Time to wait for conversion before timing out</param>
        /// <param name="progressCallback">Action to be called to report the progress</param>
        /// <param name="cancellationToken">Token which can be used to cancel the operation</param>
        protected void WaitForConversion(TimeSpan? waitSpan = null, Action<int>? progressCallback = null,
            CancellationToken cancellationToken = default)
        {
            waitSpan = waitSpan ?? TimeSpan.FromMilliseconds(WaitTime);
            var allms = 0;
            _isReadyBit = false;
            while (!_isReadyBit && !cancellationToken.IsCancellationRequested)
            {
                _i2cDevice.Read(_readBuffer);
                ReadConfigByte(LastConfigByte);
                if (_isReadyBit)
                {
                    break;
                }

                Thread.Sleep(waitSpan.Value);
                cancellationToken.ThrowIfCancellationRequested();
                allms += (int)(waitSpan.Value.TotalMilliseconds);
                progressCallback?.Invoke(allms);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Read (or load) configuration byte
        /// </summary>
        /// <param name="config">Configuration to be read</param>
        protected void ReadConfigByte(byte config)
        {
            _isReadyBit = (config & Helpers.Masks.ReadyMask) == 0; // Negated bit
            _lastChannel = (byte)((config & Helpers.Masks.ChannelMask) >> 5);
            _mode = (AdcMode)(config & Helpers.Masks.ModeMask);
            _pgaGain = (AdcGain)(config & Helpers.Masks.GainMask);
            _resolution = (AdcResolution)(config & Helpers.Masks.ResolutionMask);
        }

        /// <summary>
        /// Reads value on the specified channel
        /// </summary>
        /// <param name="channel">Channel to read the data from</param>
        /// <returns>Value read from the channel</returns>
        protected double ReadValue(int channel = -1)
        {
            if (Mode == AdcMode.OneShot)
            {
                OneShotRead(channel);
            }
            else
            {
                if (channel > 0 && channel != LastChannel)
                {
                    var conf = Helpers.SetChannelBits(LastConfigByte, channel);
                    WriteConfig(conf);
                }

                using (var source = new CancellationTokenSource(TimeSpan.FromMilliseconds(WaitTime * 5)))
                {
                    WaitForConversion(TimeSpan.FromMilliseconds(WaitTime / 5), cancellationToken: source.Token);
                }
            }

            var value = BinaryPrimitives.ReadInt16BigEndian(_readBuffer.AsSpan().Slice(0, 2));
            LastConversion = new ConversionResult(_lastChannel, value, Resolution);

            return LastConversion.Voltage;
        }

        /// <summary>
        /// Write configuration register and read back value
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="resolution">The resolution.</param>
        /// <param name="pgaGain">The pga gain.</param>
        /// <param name="errorList">List to write errors on failure</param>
        /// <returns><c>true</c> if all values are set correctly, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">channel</exception>
        protected bool SetConfig(int channel = 0, AdcMode mode = AdcMode.Continuous,
            AdcResolution resolution = AdcResolution.Bit12, AdcGain pgaGain = AdcGain.X1,
            ListString? errorList = null)
        {
            if (channel < 0 || channel > ChannelCount - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(channel));
            }

            byte conf = 0;
            var ok = true;
            conf = Helpers.SetModeBit(conf, mode);
            conf = Helpers.SetChannelBits(conf, channel);
            conf = Helpers.SetGainBits(conf, pgaGain);
            conf = Helpers.SetResolutionBits(conf, resolution);
            conf = Helpers.SetReadyBit(conf, false);

            _i2cDevice.WriteByte(conf);

            _i2cDevice.Read(_readBuffer);
            ReadConfigByte(LastConfigByte);

            if (_lastChannel != channel)
            {
                errorList?.Add($"Channel update failed from {_lastChannel} to {channel}");
                ok = false;
            }

            if (Resolution != resolution)
            {
                errorList?.Add($"Resolution update failed from {Resolution} to {resolution}");
                ok = false;
            }

            if (mode != Mode)
            {
                errorList?.Add($"Mode update failed from {Mode} to {mode}");
                ok = false;
            }

            if (InputGain != pgaGain)
            {
                errorList?.Add($"PGAGain update failed from {InputGain} to {pgaGain}");
                ok = false;
            }

            if (!ok)
            {
                // Only use console on error
                errorList?.Add($"Sent config byte {conf:X}, received {LastConfigByte:X}");
            }

            return ok;
        }

        /// <summary>
        /// Wait period before operation is cancelled
        /// </summary>
        protected int WaitTime => (int)(1000.0 / Helpers.UpdateFrequency(Resolution));

        /// <summary>
        /// Number of channels
        /// </summary>
        public int ChannelCount { get; }

        /// <summary>
        /// Last conversion result
        /// </summary>
        protected ConversionResult LastConversion
        {
            get => _lastConversion;
            set
            {
                _lastConversion = value;
                OnConversion?.Invoke(this, _lastConversion);
            }
        }

        /// <summary>
        /// Event called when conversion is complete
        /// </summary>
        public event EventHandler<ConversionResult>? OnConversion;

        /// <summary>
        /// Writes configuration
        /// </summary>
        /// <param name="configByte">Configuration to write</param>
        protected void WriteConfig(byte configByte) => _i2cDevice.WriteByte(configByte);

        private static int _asyncThreshold = 20;

        /// <summary>
        /// Sets the lower time limit in ms. If the current configuration reads data faster the synchronous API is used.
        /// This can save some overhead.
        /// </summary>
        /// <remarks>
        /// The default configuration is 20ms. This means that only in 16 bit resolution read waits asynchronously.
        /// Setting it to 0 or lower disables the function.
        /// </remarks>
        /// <param name="thresh">Time limit in ms. Default: 20ms</param>
        /// <autogeneratedoc />
        public static void SetAsyncThreshold(int thresh) => _asyncThreshold = thresh;

        /// <summary>
        /// One-shot read as an asynchronous operation. Initiates read and waits for it to finish.
        /// Async API was required as reading in 16bit resolution can take more than 60ms.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>ValueTask.</returns>
        /// <exception cref="System.IO.IOException">
        /// Device is not in One-Shot mode
        /// or
        /// ADC Conversion was not ready after {tries}
        /// </exception>
        /// <autogeneratedoc />
        protected ValueTask OneShotReadAsync(int channel = -1, CancellationToken cancellationToken = default)
        {
            if (Mode != AdcMode.OneShot)
            {
                throw new IOException("Device is not in One-Shot mode");
            }

            _isReadyBit = false;
            var conf = Helpers.SetReadyBit(LastConfigByte, false);
            if (channel >= 0 && channel != LastChannel)
            {
                conf = Helpers.SetChannelBits(conf, channel);
            }

            WriteConfig(conf);

            return WaitForConversionAsync(TimeSpan.FromMilliseconds(WaitTime), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Initiates reading value
        /// </summary>
        /// <param name="channel">Channel to read data from</param>
        /// <param name="cancellationToken">Token which can be used to cancel the operation</param>
        /// <returns>Task which can be used to wait for <see cref="ConversionResult"/></returns>
        protected async ValueTask<ConversionResult> ReadValueAsync(int channel = -1,
            CancellationToken cancellationToken = default)
        {
            if (Mode == AdcMode.OneShot)
            {
                await OneShotReadAsync(channel, cancellationToken);
            }
            else
            {
                if (channel > 0 && channel != LastChannel)
                {
                    var conf = Helpers.SetChannelBits(LastConfigByte, channel);
                    WriteConfig(conf);
                }

                await WaitForConversionAsync(TimeSpan.FromMilliseconds(WaitTime / 5),
                    cancellationToken: cancellationToken); // In continuous mode poll more often
            }

            cancellationToken.ThrowIfCancellationRequested();

            var value = BinaryPrimitives.ReadInt16BigEndian(_readBuffer.AsSpan().Slice(0, 2));
            LastConversion = new ConversionResult(_lastChannel, value, Resolution);

            return LastConversion;
        }

        /// <summary>
        /// Initiates waiting for conversion
        /// </summary>
        /// <param name="waitSpan">Time to wait for conversion</param>
        /// <param name="progressCallback">Callback to be called to report progress</param>
        /// <param name="cancellationToken">Token which can be used to cancel the operation</param>
        /// <returns>Task which can be used to wait for opertation to finish</returns>
        protected async ValueTask WaitForConversionAsync(TimeSpan? waitSpan = null, Action<int>? progressCallback = null,
            CancellationToken cancellationToken = default)
        {
            waitSpan = waitSpan ?? TimeSpan.FromMilliseconds(WaitTime);
            var allms = 0;
            _isReadyBit = false;
            while (!_isReadyBit && !cancellationToken.IsCancellationRequested)
            {
                _i2cDevice.Read(_readBuffer);
                ReadConfigByte(LastConfigByte);
                if (_isReadyBit)
                {
                    break;
                }

                await Task.Delay(waitSpan.Value, cancellationToken);
                allms += (int)(waitSpan.Value.TotalMilliseconds);
                progressCallback?.Invoke(allms);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Reads the channel. Async API is mostly useful for greater resolutions and one-shot mode, when conversion time can be significant.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>ValueTask&lt;System.Double&gt;.</returns>
        /// <autogeneratedoc />
        public async ValueTask<double> ReadChannelAsync(int channel, CancellationToken cancellationToken = default)
        {
            if ((Resolution == AdcResolution.Bit12 && Mode == AdcMode.Continuous) || WaitTime < _asyncThreshold)
            {
                return ReadValue(channel);
            }

            await ReadValueAsync(channel, cancellationToken);
            return LastConversion.Voltage;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
        }
    }
}
