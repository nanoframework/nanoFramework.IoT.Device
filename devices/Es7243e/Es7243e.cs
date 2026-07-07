// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.Es7243e
{
    /// <summary>
    /// Driver for the Everest Semiconductor ES7243E audio ADC (microphone capture codec).
    /// </summary>
    /// <remarks>
    /// This binding covers the I2C control plane only. It configures the codec (clocking, serial data
    /// format, microphone gain, mute and power state). The captured PCM audio samples are streamed out
    /// over I2S and are not handled by this class. The register sequences are ported from the Espressif
    /// <c>esp_codec_dev</c> component.
    /// </remarks>
    public class Es7243e : IDisposable
    {
        /// <summary>
        /// Default I2C address for the ES7243E (7-bit).
        /// </summary>
        /// <remarks>
        /// The address depends on the board wiring of the ES7243E address pins (typically 0x10 or 0x11).
        /// Confirm against the schematic.
        /// </remarks>
        public const byte DefaultI2cAddress = 0x10;

        /// <summary>
        /// Minimum microphone gain register value.
        /// </summary>
        public const byte MinMicGain = 0x10;

        /// <summary>
        /// Maximum microphone gain register value.
        /// </summary>
        public const byte MaxMicGain = 0x1E;

        // Register map (see ES7243E datasheet / Espressif esp_codec_dev driver).
        private const byte RegReset = 0x00;
        private const byte RegClockManager1 = 0x01;
        private const byte RegClockManager4 = 0x04;
        private const byte RegAdcMute = 0x0B;
        private const byte RegSystem16 = 0x16;
        private const byte RegSystem17 = 0x17;
        private const byte RegPgaGain1 = 0x20;
        private const byte RegPgaGain2 = 0x21;
        private const byte RegAnalogPower = 0xF7;
        private const byte RegBiasPower = 0xF9;

        private readonly bool _shouldDispose;
        private I2cDevice _i2cDevice;
        private bool _muted;

        /// <summary>
        /// Initializes a new instance of the <see cref="Es7243e" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication with the codec.</param>
        /// <param name="shouldDispose"><see langword="true" /> to dispose the <paramref name="i2cDevice"/> when this instance is disposed; otherwise, <see langword="false" />.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <see langword="null" />.</exception>
        public Es7243e(I2cDevice i2cDevice, bool shouldDispose = true)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException();
            _shouldDispose = shouldDispose;
        }

        /// <summary>
        /// Applies the default power-up and clocking configuration to the codec.
        /// </summary>
        /// <remarks>
        /// This runs the ES7243E initialization register sequence (slave mode, I2S data format). After
        /// calling this method the codec is configured; call <see cref="Start" /> to begin capturing.
        /// This implementation is proven to work on ESP32-S3 and youmay have to implement your own for
        /// other platforms. The ES7243E datasheet is not publicly available, so the register bit layout
        /// and function is not fully known.
        /// </remarks>
        public void Initialize()
        {
            // This sequence is ported verbatim from the Espressif <c>esp_codec_dev</c> driver. It is not
            // guaranteed to be correct or optimal for all hardware revisions.
            // The ES7243E datasheet is not publicly available, so the register bit layout and
            // function is not fully known.
            WriteRegister(0x01, 0x3A);
            WriteRegister(0x00, 0x80);
            WriteRegister(0xF9, 0x00);
            WriteRegister(0x04, 0x02);
            WriteRegister(0x04, 0x01);
            WriteRegister(0xF9, 0x01);
            WriteRegister(0x00, 0x1E);
            WriteRegister(0x01, 0x00);

            WriteRegister(0x02, 0x00);
            WriteRegister(0x03, 0x20);
            WriteRegister(0x04, 0x03);
            WriteRegister(0x0D, 0x00);
            WriteRegister(0x05, 0x00);
            WriteRegister(0x06, 0x03);
            WriteRegister(0x07, 0x00);
            WriteRegister(0x08, 0xFF);

            WriteRegister(0x09, 0xCA);
            WriteRegister(0x0A, 0x85);
            WriteRegister(0x0B, 0x00);
            WriteRegister(0x0E, 0xBF);
            WriteRegister(0x0F, 0x80);
            WriteRegister(0x14, 0x0C);
            WriteRegister(0x15, 0x0C);
            WriteRegister(0x17, 0x02);
            WriteRegister(0x18, 0x26);

            WriteRegister(0x19, 0x77);
            WriteRegister(0x1A, 0xF4);
            WriteRegister(0x1B, 0x66);
            WriteRegister(0x1C, 0x44);
            WriteRegister(0x1E, 0x00);
            WriteRegister(0x1F, 0x0C);
            WriteRegister(RegPgaGain1, 0x1A);
            WriteRegister(RegPgaGain2, 0x1A);
        }

        /// <summary>
        /// Sets the analog microphone (PGA) gain.
        /// </summary>
        /// <param name="gain">The gain register value, from <see cref="MinMicGain" /> (lowest) to <see cref="MaxMicGain" /> (highest).</param>
        /// <remarks>
        /// The same value is written to both PGA gain registers (0x20 and 0x21). Higher values increase gain.
        /// </remarks>
        public void SetMicGain(byte gain)
        {
            if (gain < MinMicGain)
            {
                gain = MinMicGain;
            }

            if (gain > MaxMicGain)
            {
                gain = MaxMicGain;
            }

            WriteRegister(RegPgaGain1, gain);
            WriteRegister(RegPgaGain2, gain);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the ADC output is muted.
        /// </summary>
        /// <value><see langword="true" /> when the capture output is muted; otherwise, <see langword="false" />.</value>
        public bool Muted
        {
            get => _muted;

            set
            {
                // Per the esp_codec_dev driver the ADC mute register (0x0B) is written with 0xC0 to
                // mute and 0x00 to un-mute.
                WriteRegister(RegAdcMute, (byte)(value ? 0xC0 : 0x00));
                _muted = value;
            }
        }

        /// <summary>
        /// Powers up the codec and starts capturing audio from the microphones.
        /// </summary>
        public void Start()
        {
            // Enable sequence ported verbatim from the Espressif esp_codec_dev ES7243E driver.
            WriteRegister(RegBiasPower, 0x00);
            WriteRegister(RegClockManager4, 0x01);
            WriteRegister(RegSystem17, 0x01);
            WriteRegister(RegPgaGain1, 0x10);
            WriteRegister(RegPgaGain2, 0x10);
            WriteRegister(RegReset, 0x80);
            WriteRegister(RegClockManager1, 0x3A);
            WriteRegister(RegSystem16, 0x3F);

            // Releasing register 0x16 (0x3F -> 0x00) takes the ADC out of its held state so it
            // starts streaming captured audio. Writing 0x17 here instead leaves the ADC muted.
            WriteRegister(RegSystem16, 0x00);
        }

        /// <summary>
        /// Stops audio capture.
        /// </summary>
        public void Stop()
        {
            // Disable sequence ported from the Espressif esp_codec_dev ES7243E driver.
            WriteRegister(RegClockManager4, 0x02);
            WriteRegister(RegClockManager4, 0x01);
            WriteRegister(RegAnalogPower, 0x30);
            WriteRegister(RegBiasPower, 0x01);
            WriteRegister(RegSystem16, 0xFF);
            WriteRegister(RegSystem17, 0x00);
            WriteRegister(RegClockManager1, 0x38);
            WriteRegister(RegPgaGain1, 0x00);
            WriteRegister(RegPgaGain2, 0x00);
            WriteRegister(RegReset, 0x00);
            WriteRegister(RegReset, 0x1E);
            WriteRegister(RegClockManager1, 0x30);
            WriteRegister(RegClockManager1, 0x00);
        }

        private void WriteRegister(byte register, byte value)
        {
            SpanByte writeBuffer = new byte[2];
            writeBuffer[0] = register;
            writeBuffer[1] = value;
            _i2cDevice.Write(writeBuffer);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _i2cDevice?.Dispose();
            }

            _i2cDevice = null;
        }
    }
}
