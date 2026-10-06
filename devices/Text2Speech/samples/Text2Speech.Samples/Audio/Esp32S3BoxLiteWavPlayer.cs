// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.I2s;
using System.Diagnostics;
using System.IO;
using Iot.Device.Es8156;
using nanoFramework.Hardware.Esp32;
using Es8156Device = Iot.Device.Es8156.Es8156;

namespace Iot.Device.Text2Speech.Samples
{
    /// <summary>
    /// Plays Text2Speech audio through the ESP32-S3-BOX-Lite ES8156 audio path.
    /// </summary>
    internal sealed class Esp32S3BoxLiteWavPlayer : IDisposable
    {
        private const int I2sBus = 1;
        private const int I2cBus = 1;
        private const int I2cDataPin = 8;
        private const int I2cClockPin = 18;
        private const int I2sMasterClockPin = 2;
        private const int I2sBitClockPin = 17;
        private const int I2sWordSelectPin = 47;
        private const int I2sDataOutPin = 15;
        private const int PowerAmplifierPin = 46;
        private const byte VolumePercent = 70;
        private const int FastPlaybackSampleRate = 8000;
        private const int InterpolatedPlaybackSampleRate = 16000;
        private const int I2sBufferSize = 40000;

        private readonly I2sDevice _i2sDevice;
        private readonly InterpolatedI2sPcmSink _pcmSink;
        private readonly I2sPlaybackMode _playbackMode;
        private readonly Es8156Device _dac;
        private readonly GpioController _gpioController;
        private readonly GpioPin _powerAmplifier;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Esp32S3BoxLiteWavPlayer" /> class.
        /// </summary>
        public Esp32S3BoxLiteWavPlayer()
            : this(I2sPlaybackMode.Interpolated16Khz)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Esp32S3BoxLiteWavPlayer" /> class.
        /// </summary>
        /// <param name="playbackMode">The playback rate and conversion mode.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="playbackMode" /> is not a supported playback mode.
        /// </exception>
        public Esp32S3BoxLiteWavPlayer(I2sPlaybackMode playbackMode)
        {
            if (playbackMode != I2sPlaybackMode.Fast8Khz
                && playbackMode != I2sPlaybackMode.Interpolated16Khz)
            {
                throw new ArgumentOutOfRangeException();
            }

            _playbackMode = playbackMode;
            int playbackSampleRate = playbackMode == I2sPlaybackMode.Fast8Khz
                ? FastPlaybackSampleRate
                : InterpolatedPlaybackSampleRate;

            Configuration.SetPinFunction(I2cDataPin, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(I2cClockPin, DeviceFunction.I2C1_CLOCK);

            I2cConnectionSettings i2cSettings =
                new I2cConnectionSettings(I2cBus, Es8156Device.DefaultI2cAddress);
            _dac = new Es8156Device(new I2cDevice(i2cSettings));

            _gpioController = new GpioController();
            _powerAmplifier = _gpioController.OpenPin(PowerAmplifierPin, PinMode.Output);
            _powerAmplifier.Write(PinValue.Low);

            Configuration.SetPinFunction(I2sMasterClockPin, DeviceFunction.I2S1_MCK);
            Configuration.SetPinFunction(I2sBitClockPin, DeviceFunction.I2S1_BCK);
            Configuration.SetPinFunction(I2sWordSelectPin, DeviceFunction.I2S1_WS);
            Configuration.SetPinFunction(I2sDataOutPin, DeviceFunction.I2S1_DATA_OUT);
            ValidatePinRoutes();

            _i2sDevice = new I2sDevice(
                new I2sConnectionSettings(I2sBus)
                {
                    Mode = I2sMode.Master | I2sMode.Tx,
                    CommunicationFormat = I2sCommunicationFormat.I2S,
                    SampleRate = playbackSampleRate,
                    BitsPerSample = I2sBitsPerSample.Bit16,
                    ChannelFormat = I2sChannelFormat.RightLeft,
                    BufferSize = I2sBufferSize,
                });
            _pcmSink = new InterpolatedI2sPcmSink(
                _i2sDevice,
                playbackMode == I2sPlaybackMode.Interpolated16Khz);

            // Start the master clocks before configuring the slave codec.
            _i2sDevice.Write(new byte[512]);
            _dac.Initialize();
            _dac.SetFormat(SerialAudioFormat.I2s, WordLength.Bits16);
            _dac.Volume = VolumePercent;
            _dac.Muted = false;
            _powerAmplifier.Write(PinValue.High);

            Debug.WriteLine(
                "ES8156 initialized at " + _dac.Volume.ToString()
                + "% volume; ESP32-S3-BOX-Lite amplifier enabled; playback="
                + _playbackMode.ToString() + ".");
        }

        /// <summary>
        /// Reads a WAV file and writes signed 16-bit stereo frames in the selected playback mode.
        /// </summary>
        /// <param name="path">The full path of the 8 kHz WAV file to play.</param>
        /// <exception cref="IOException">The WAV data ends before the declared data length.</exception>
        /// <exception cref="ArgumentException">The file does not contain a supported WAV header.</exception>
        public void Play(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                int remaining = WavFile.ValidateAndPositionAtData(stream);
                byte[] input = new byte[1024];
                _pcmSink.Reset();
                try
                {
                    while (remaining > 0)
                    {
                        int wanted = remaining < input.Length ? remaining : input.Length;
                        int read = stream.Read(input, 0, wanted);
                        if (read == 0)
                        {
                            throw new IOException();
                        }

                        _pcmSink.Write(input, 0, read);
                        remaining -= read;
                    }

                    _pcmSink.Complete();
                }
                catch
                {
                    _pcmSink.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Pre-renders compact PCM in memory and then plays it without using a WAV file.
        /// </summary>
        /// <param name="synthesizer">The configured Text2Speech synthesizer.</param>
        /// <param name="text">The text to synthesize and play.</param>
        /// <returns>The number of generated 8 kHz source samples.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="synthesizer" /> or <paramref name="text" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">The text cannot be synthesized.</exception>
        /// <remarks>
        /// Pre-rendering guarantees continuous playback when synthesis is slower than real time.
        /// The temporary buffer costs approximately 8 KB per second of speech.
        /// </remarks>
        public int Speak(TtsSynthesizer synthesizer, string text)
        {
            return Speak(synthesizer, text, null);
        }

        /// <summary>
        /// Pre-renders PCM, optionally saves it as a WAV file, and then plays it.
        /// </summary>
        /// <param name="synthesizer">The configured Text2Speech synthesizer.</param>
        /// <param name="text">The text to synthesize and play.</param>
        /// <param name="wavPath">The caller-provided WAV path, or <see langword="null" /> not to save.</param>
        /// <returns>The number of generated 8 kHz source samples.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="synthesizer" /> or <paramref name="text" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">The text cannot be synthesized.</exception>
        public int Speak(TtsSynthesizer synthesizer, string text, string wavPath)
        {
            if (synthesizer == null)
            {
                throw new ArgumentNullException();
            }

            byte[] pcm = synthesizer.Speak(text);
            if (wavPath != null)
            {
                WavFile.Write(wavPath, pcm);
            }

            PlayPcm(pcm);
            return pcm.Length;
        }

        /// <summary>
        /// Plays an in-memory unsigned 8-bit mono PCM buffer.
        /// </summary>
        /// <param name="pcm">The 8 kHz source PCM to interpolate and play.</param>
        /// <exception cref="ArgumentNullException"><paramref name="pcm" /> is <see langword="null" />.</exception>
        public void Play(byte[] pcm)
        {
            if (pcm == null)
            {
                throw new ArgumentNullException();
            }

            PlayPcm(pcm);
        }

        /// <summary>
        /// Synthesizes and plays concurrently with bounded buffering.
        /// </summary>
        /// <param name="synthesizer">The configured Text2Speech synthesizer.</param>
        /// <param name="text">The text to synthesize and play.</param>
        /// <returns>The number of generated 8 kHz source samples.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="synthesizer" /> or <paramref name="text" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">The text cannot be synthesized.</exception>
        /// <remarks>
        /// This minimizes startup latency but can underrun when synthesis remains slower than playback.
        /// </remarks>
        public int SpeakStreaming(TtsSynthesizer synthesizer, string text)
        {
            return SpeakStreaming(synthesizer, text, null);
        }

        /// <summary>
        /// Synthesizes and plays concurrently, optionally saving the source PCM as a WAV file.
        /// </summary>
        /// <param name="synthesizer">The configured Text2Speech synthesizer.</param>
        /// <param name="text">The text to synthesize and play.</param>
        /// <param name="wavPath">The caller-provided WAV path, or <see langword="null" /> not to save.</param>
        /// <returns>The number of generated 8 kHz source samples.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="synthesizer" /> or <paramref name="text" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">The text cannot be synthesized.</exception>
        public int SpeakStreaming(TtsSynthesizer synthesizer, string text, string wavPath)
        {
            if (text == null)
            {
                throw new ArgumentNullException();
            }

            string[] texts = new string[] { text };
            string[] wavPaths = wavPath == null ? null : new string[] { wavPath };
            return SpeakStreaming(synthesizer, texts, wavPaths);
        }

        /// <summary>
        /// Synthesizes multiple text rounds through one continuous producer-consumer session.
        /// </summary>
        /// <param name="synthesizer">The configured Text2Speech synthesizer.</param>
        /// <param name="texts">The independently synthesizable text rounds.</param>
        /// <returns>The total number of generated 8 kHz source samples.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="synthesizer" />, <paramref name="texts" />, or a text round is
        /// <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// No text rounds were supplied or a text round cannot be synthesized.
        /// </exception>
        public int SpeakStreaming(TtsSynthesizer synthesizer, string[] texts)
        {
            return SpeakStreaming(synthesizer, texts, null);
        }

        /// <summary>
        /// Synthesizes multiple text rounds continuously and optionally saves each round separately.
        /// </summary>
        /// <param name="synthesizer">The configured Text2Speech synthesizer.</param>
        /// <param name="texts">The independently synthesizable text rounds.</param>
        /// <param name="wavPaths">
        /// One WAV path per text round, or <see langword="null" /> not to save any round.
        /// Individual paths may be <see langword="null" />.
        /// </param>
        /// <returns>The total number of generated 8 kHz source samples.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="synthesizer" />, <paramref name="texts" />, or a text round is
        /// <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// No text rounds were supplied, the WAV path count differs from the text round count,
        /// or a text round cannot be synthesized.
        /// </exception>
        public int SpeakStreaming(
            TtsSynthesizer synthesizer,
            string[] texts,
            string[] wavPaths)
        {
            if (synthesizer == null || texts == null)
            {
                throw new ArgumentNullException();
            }

            if (texts.Length == 0 || (wavPaths != null && wavPaths.Length != texts.Length))
            {
                throw new ArgumentException();
            }

            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] == null)
                {
                    throw new ArgumentNullException();
                }
            }

            int totalCount = 0;
            _pcmSink.Reset(enableDiagnostics: true);
            try
            {
                for (int i = 0; i < texts.Length; i++)
                {
                    WavFileWriter writer = null;
                    try
                    {
                        string wavPath = wavPaths == null ? null : wavPaths[i];
                        writer = wavPath == null ? null : new WavFileWriter(wavPath);
                        IPcmSink sink = writer == null
                            ? (IPcmSink)_pcmSink
                            : new TeePcmSink(_pcmSink, writer);
                        totalCount += synthesizer.Speak(texts[i], sink);
                        if (writer != null)
                        {
                            writer.Complete();
                        }
                    }
                    catch
                    {
                        if (writer != null)
                        {
                            writer.Abort();
                        }

                        throw;
                    }
                    finally
                    {
                        if (writer != null)
                        {
                            writer.Dispose();
                        }
                    }
                }

                _pcmSink.Complete();
                return totalCount;
            }
            catch
            {
                _pcmSink.Abort();
                throw;
            }
        }

        /// <summary>
        /// Mutes and releases the codec, amplifier, I2S device, and WAV stream.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _dac.Muted = true;
            _powerAmplifier.Write(PinValue.Low);
            _i2sDevice.Dispose();
            _dac.Dispose();
            _powerAmplifier.Dispose();
            _gpioController.Dispose();
            _disposed = true;
        }

        private static void ValidatePinRoutes()
        {
            ValidatePinRoute(DeviceFunction.I2S1_MCK, I2sMasterClockPin, "MCLK");
            ValidatePinRoute(DeviceFunction.I2S1_BCK, I2sBitClockPin, "BCLK");
            ValidatePinRoute(DeviceFunction.I2S1_WS, I2sWordSelectPin, "WS");
            ValidatePinRoute(DeviceFunction.I2S1_DATA_OUT, I2sDataOutPin, "DOUT");
        }

        private static void ValidatePinRoute(DeviceFunction function, int expectedPin, string signal)
        {
            int actualPin = Configuration.GetFunctionPin(function);
            Debug.WriteLine(
                "I2S1 " + signal + " routed to GPIO" + actualPin.ToString()
                + " (expected GPIO" + expectedPin.ToString() + ").");
            if (actualPin != expectedPin)
            {
                throw new InvalidOperationException();
            }
        }

        private void PlayPcm(byte[] pcm)
        {
            _pcmSink.Reset();
            try
            {
                _pcmSink.Write(pcm, 0, pcm.Length);
                _pcmSink.Complete();
            }
            catch
            {
                _pcmSink.Abort();
                throw;
            }
        }
    }
}
