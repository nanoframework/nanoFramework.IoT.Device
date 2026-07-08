// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.Es7210
{
    /// <summary>
    /// Driver for the Everest Semiconductor ES7210 4-channel audio ADC (microphone capture codec).
    /// </summary>
    /// <remarks>
    /// This binding covers the I2C control plane only. It configures the codec (clocking, microphone
    /// selection, gain, mute and power state) for a two-microphone (MIC1 + MIC2) I2S capture setup in
    /// slave mode. The captured PCM audio samples are streamed out over I2S using
    /// <c>System.Device.I2s</c> and are not handled by this class. The register sequences are ported from
    /// the Espressif <c>esp_codec_dev</c> component.
    /// </remarks>
    public class Es7210 : IDisposable
    {
        /// <summary>
        /// Default I2C address for the ES7210 (7-bit).
        /// </summary>
        public const byte DefaultI2cAddress = 0x40;

        /// <summary>
        /// Minimum microphone gain code (0 dB).
        /// </summary>
        public const byte MinMicGain = 0x00;

        /// <summary>
        /// Maximum microphone gain code (37.5 dB).
        /// </summary>
        public const byte MaxMicGain = 0x0E;

        // Register map (see ES7210 datasheet / Espressif esp_codec_dev driver).
        private const byte RegReset = 0x00;
        private const byte RegClockOff = 0x01;
        private const byte RegMainClock = 0x02;
        private const byte RegPowerDown = 0x06;
        private const byte RegOsr = 0x07;
        private const byte RegModeConfig = 0x08;
        private const byte RegTimeControl0 = 0x09;
        private const byte RegTimeControl1 = 0x0A;
        private const byte RegSdpInterface2 = 0x12;
        private const byte RegAdc34Muterange = 0x14;
        private const byte RegAdc12Muterange = 0x15;
        private const byte RegAdc34Hpf2 = 0x20;
        private const byte RegAdc34Hpf1 = 0x21;
        private const byte RegAdc12Hpf1 = 0x22;
        private const byte RegAdc12Hpf2 = 0x23;
        private const byte RegAnalog = 0x40;
        private const byte RegMic12Bias = 0x41;
        private const byte RegMic34Bias = 0x42;
        private const byte RegMic1Gain = 0x43;
        private const byte RegMic2Gain = 0x44;
        private const byte RegMic3Gain = 0x45;
        private const byte RegMic4Gain = 0x46;
        private const byte RegMic1Power = 0x47;
        private const byte RegMic2Power = 0x48;
        private const byte RegMic3Power = 0x49;
        private const byte RegMic4Power = 0x4A;
        private const byte RegMic12Power = 0x4B;
        private const byte RegMic34Power = 0x4C;

        // Default microphone gain code: 10 = 30 dB.
        private const byte DefaultMicGain = 0x0A;

        private readonly bool _shouldDispose;
        private I2cDevice _i2cDevice;
        private byte _micGain = DefaultMicGain;
        private byte _clockOffRegister;
        private Microphones _microphones = Microphones.Microphone1 | Microphones.Microphone2;

        /// <summary>
        /// Initializes a new instance of the <see cref="Es7210" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication with the codec.</param>
        /// <param name="shouldDispose"><see langword="true" /> to dispose the <paramref name="i2cDevice"/> when this instance is disposed; otherwise, <see langword="false" />.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <see langword="null" />.</exception>
        public Es7210(I2cDevice i2cDevice, bool shouldDispose = true)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException();
            _shouldDispose = shouldDispose;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the ADC output is muted.
        /// </summary>
        /// <value><see langword="true" /> when the capture output is muted; otherwise, <see langword="false" />.</value>
        public bool Muted
        {
            get => (ReadRegister(RegAdc12Muterange) & 0x03) == 0x03;

            set
            {
                byte bits = (byte)(value ? 0x03 : 0x00);
                UpdateRegister(RegAdc34Muterange, 0x03, bits);
                UpdateRegister(RegAdc12Muterange, 0x03, bits);
            }
        }

        /// <summary>
        /// Configures the codec in slave mode for a two-microphone (MIC1 + MIC2) I2S capture setup.
        /// </summary>
        /// <remarks>
        /// This runs the ES7210 initialization register sequence ported from the Espressif
        /// <c>esp_codec_dev</c> component. Call <see cref="Start" /> afterwards to begin capturing.
        /// </remarks>
        public void Initialize()
        {
            WriteRegister(RegReset, 0xFF);
            WriteRegister(RegReset, 0x41);
            WriteRegister(RegClockOff, 0x3F);
            WriteRegister(RegTimeControl0, 0x30);
            WriteRegister(RegTimeControl1, 0x30);
            WriteRegister(RegAdc12Hpf2, 0x2A);
            WriteRegister(RegAdc12Hpf1, 0x0A);
            WriteRegister(RegAdc34Hpf2, 0x0A);
            WriteRegister(RegAdc34Hpf1, 0x2A);

            // Slave mode (the ESP32 is the I2S master).
            UpdateRegister(RegModeConfig, 0x01, 0x00);

            WriteRegister(RegAnalog, 0x43);
            WriteRegister(RegMic12Bias, 0x70);
            WriteRegister(RegMic34Bias, 0x70);
            WriteRegister(RegOsr, 0x20);
            WriteRegister(RegMainClock, 0xC1);

            SelectMicrophones();

            // Remember the clock-off register value so Start() can restore it (matches esp_codec_dev).
            _clockOffRegister = ReadRegister(RegClockOff);
        }

        /// <summary>
        /// Gets or sets the microphone (PGA) gain code applied to MIC1 and MIC2.
        /// </summary>
        /// <value>The gain code, from <see cref="MinMicGain" /> (0 dB) to <see cref="MaxMicGain" /> (37.5 dB). Each step up to code 11 is roughly 3 dB. Values above <see cref="MaxMicGain" /> are clamped.</value>
        public byte MicGain
        {
            get => (byte)(ReadRegister(RegMic1Gain) & 0x0F);

            set
            {
                if (value > MaxMicGain)
                {
                    value = MaxMicGain;
                }

                _micGain = value;
                UpdateRegister(RegMic1Gain, 0x0F, value);
                UpdateRegister(RegMic2Gain, 0x0F, value);
            }
        }

        /// <summary>
        /// Gets or sets which microphone channels (MIC1, MIC2, or both) are enabled for capture.
        /// </summary>
        /// <value>A combination of <see cref="Microphones" /> flags. Defaults to <see cref="Microphones.Microphone1" /> and <see cref="Microphones.Microphone2" /> (both microphones).</value>
        /// <remarks>Setting this re-applies the microphone selection immediately.</remarks>
        public Microphones SelectedMicrophones
        {
            get => _microphones;

            set
            {
                _microphones = value;
                SelectMicrophones();
            }
        }

        /// <summary>
        /// Powers up the codec and starts capturing audio from the microphones.
        /// </summary>
        public void Start()
        {
            WriteRegister(RegClockOff, _clockOffRegister);
            WriteRegister(RegPowerDown, 0x00);
            WriteRegister(RegAnalog, 0x43);
            WriteRegister(RegMic1Power, 0x08);
            WriteRegister(RegMic2Power, 0x08);
            WriteRegister(RegMic3Power, 0x08);
            WriteRegister(RegMic4Power, 0x08);
            SelectMicrophones();
            WriteRegister(RegAnalog, 0x43);
            WriteRegister(RegReset, 0x71);
            WriteRegister(RegReset, 0x41);
        }

        /// <summary>
        /// Stops audio capture and powers down the codec.
        /// </summary>
        public void Stop()
        {
            WriteRegister(RegMic1Power, 0xFF);
            WriteRegister(RegMic2Power, 0xFF);
            WriteRegister(RegMic3Power, 0xFF);
            WriteRegister(RegMic4Power, 0xFF);
            WriteRegister(RegMic12Power, 0xFF);
            WriteRegister(RegMic34Power, 0xFF);
            WriteRegister(RegAnalog, 0xC0);
            WriteRegister(RegClockOff, 0x7F);
            WriteRegister(RegPowerDown, 0x07);
        }

        // Applies the current microphone selection and gain (ported from es7210_mic_select).
        private void SelectMicrophones()
        {
            // Deselect every channel and power all microphones down first.
            UpdateRegister(RegMic1Gain, 0x10, 0x00);
            UpdateRegister(RegMic2Gain, 0x10, 0x00);
            UpdateRegister(RegMic3Gain, 0x10, 0x00);
            UpdateRegister(RegMic4Gain, 0x10, 0x00);
            WriteRegister(RegMic12Power, 0xFF);
            WriteRegister(RegMic34Power, 0xFF);

            if ((_microphones & Microphones.Microphone1) != Microphones.None)
            {
                UpdateRegister(RegClockOff, 0x0B, 0x00);
                WriteRegister(RegMic12Power, 0x00);
                UpdateRegister(RegMic1Gain, 0x10, 0x10);
                UpdateRegister(RegMic1Gain, 0x0F, _micGain);
            }

            if ((_microphones & Microphones.Microphone2) != Microphones.None)
            {
                UpdateRegister(RegClockOff, 0x0B, 0x00);
                WriteRegister(RegMic12Power, 0x00);
                UpdateRegister(RegMic2Gain, 0x10, 0x10);
                UpdateRegister(RegMic2Gain, 0x0F, _micGain);
            }

            // Standard (non-TDM) serial data port.
            WriteRegister(RegSdpInterface2, 0x00);
        }

        private void UpdateRegister(byte register, byte mask, byte value)
        {
            byte current = ReadRegister(register);
            byte updated = (byte)((current & ~mask) | (value & mask));
            WriteRegister(register, updated);
        }

        private void WriteRegister(byte register, byte value)
        {
            SpanByte writeBuffer = new byte[2];
            writeBuffer[0] = register;
            writeBuffer[1] = value;
            _i2cDevice.Write(writeBuffer);
        }

        private byte ReadRegister(byte register)
        {
            SpanByte writeBuffer = new byte[1];
            writeBuffer[0] = register;
            SpanByte readBuffer = new byte[1];
            _i2cDevice.WriteRead(writeBuffer, readBuffer);
            return readBuffer[0];
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
