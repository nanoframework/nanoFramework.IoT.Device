// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.Es8156
{
    /// <summary>
    /// Driver for the Everest Semiconductor ES8156 low-power stereo audio DAC (playback codec).
    /// </summary>
    /// <remarks>
    /// This binding covers the I2C control plane only. It configures the codec (clocking, serial data
    /// format, volume, mute and power state). The actual PCM audio samples are streamed over I2S and are
    /// not handled by this class. The register sequences are ported from the Espressif
    /// <c>esp_codec_dev</c> component and should be validated on real hardware.
    /// </remarks>
    public class Es8156 : IDisposable
    {
        /// <summary>
        /// Default I2C address for the ES8156 (7-bit).
        /// </summary>
        /// <remarks>
        /// The address depends on the board wiring of the ES8156 CE pin. Confirm against the schematic.
        /// </remarks>
        public const byte DefaultI2cAddress = 0x08;

        // Register map (see ES8156 datasheet).
        private const byte RegReset = 0x00;
        private const byte RegMainClockControl = 0x01;
        private const byte RegSclkMode = 0x02;
        private const byte RegClockOnOff = 0x08;
        private const byte RegTimeControl1 = 0x0A;
        private const byte RegTimeControl2 = 0x0B;
        private const byte RegP2sControl = 0x0D;
        private const byte RegSdpInterface1 = 0x11;
        private const byte RegDacMute = 0x13;
        private const byte RegDacVolume = 0x14;
        private const byte RegMiscControl3 = 0x18;
        private const byte RegAnalogSys1 = 0x20;
        private const byte RegAnalogSys2 = 0x21;
        private const byte RegAnalogSys3 = 0x22;
        private const byte RegAnalogSys4 = 0x23;
        private const byte RegAnalogLp = 0x24;
        private const byte RegAnalogSys5 = 0x25;

        private readonly bool _shouldDispose;
        private I2cDevice _i2cDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="Es8156" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication with the codec.</param>
        /// <param name="shouldDispose"><see langword="true" /> to dispose the <paramref name="i2cDevice"/> when this instance is disposed; otherwise, <see langword="false" />.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <see langword="null" />.</exception>
        public Es8156(I2cDevice i2cDevice, bool shouldDispose = true)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException();
            _shouldDispose = shouldDispose;
        }

        /// <summary>
        /// Applies the default power-up and clocking configuration to the codec.
        /// </summary>
        /// <remarks>
        /// This runs the ES8156 initialization register sequence (slave mode, I2S data format). After
        /// calling this method the codec is powered up and ready to receive an I2S stream. Use
        /// <see cref="Volume" /> to set the output level and <see cref="Muted" /> to mute.
        /// </remarks>
        public void Initialize()
        {
            // The register sequence is ported from the Espressif esp_codec_dev driver. The datasheet does not
            // provide a complete initialization sequence, so this is the only known working configuration.
            WriteRegister(RegSclkMode, 0x04);
            WriteRegister(RegAnalogSys1, 0x2A);
            WriteRegister(RegAnalogSys2, 0x3C);
            WriteRegister(RegAnalogSys3, 0x00);
            WriteRegister(RegAnalogLp, 0x07);
            WriteRegister(RegAnalogSys4, 0x00);

            WriteRegister(RegTimeControl1, 0x01);
            WriteRegister(RegTimeControl2, 0x01);
            WriteRegister(RegSdpInterface1, 0x00);
            WriteRegister(RegDacVolume, 0xBF);
            WriteRegister(RegP2sControl, 0x14);
            WriteRegister(RegMiscControl3, 0x00);
            WriteRegister(RegClockOnOff, 0x3F);
            WriteRegister(RegReset, 0x02);
            WriteRegister(RegReset, 0x03);
            WriteRegister(RegAnalogSys5, 0x20);
        }

        /// <summary>
        /// Configures the serial audio data port (I2S) format and word length.
        /// </summary>
        /// <param name="format">The serial audio data format the codec expects on the I2S bus.</param>
        /// <param name="wordLength">The number of bits per audio sample on the I2S bus.</param>
        /// <remarks>
        /// The bit mapping follows the ES8156 datasheet register 0x11 (SDP interface configuration 1).
        /// Verify the resulting configuration matches your I2S controller settings on real hardware.
        /// </remarks>
        public void SetFormat(SerialAudioFormat format, WordLength wordLength)
        {
            byte value = (byte)((byte)format | (byte)wordLength);
            WriteRegister(RegSdpInterface1, value);
        }

        /// <summary>
        /// Gets or sets the DAC output volume as a percentage from 0 (minimum) to 100 (maximum).
        /// </summary>
        /// <value>The output volume as a percentage from 0 (minimum) to 100 (maximum).</value>
        /// <remarks>
        /// The percentage is mapped linearly onto the 8-bit ES8156 digital volume register (0x14).
        /// </remarks>
        public byte Volume
        {
            get
            {
                // Read the raw register back and convert it to a percentage (inverse of the set
                // mapping, rounded to the nearest percent).
                return (byte)(((ReadRegister(RegDacVolume) * 100) + 128) / 256);
            }

            set
            {
                if (value > 100)
                {
                    value = 100;
                }

                int registerValue = value * 256 / 100;
                if (registerValue > 255)
                {
                    registerValue = 255;
                }

                WriteRegister(RegDacVolume, (byte)registerValue);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the DAC output is muted.
        /// </summary>
        /// <value><see langword="true" /> when the output is muted; otherwise, <see langword="false" />.</value>
        public bool Muted
        {
            get => (ReadRegister(RegDacMute) & 0x20) != 0;

            set
            {
                byte registerValue = (byte)(ReadRegister(RegDacMute) & 0xDF);
                if (value)
                {
                    registerValue |= 0x20;
                }

                WriteRegister(RegDacMute, registerValue);
            }
        }

        /// <summary>
        /// Powers up the codec analog and clock blocks so it can play audio.
        /// </summary>
        public void Start()
        {
            WriteRegister(RegClockOnOff, 0x3F);
            WriteRegister(RegReset, 0x02);
            WriteRegister(RegReset, 0x03);
            WriteRegister(RegAnalogSys5, 0x20);
        }

        /// <summary>
        /// Puts the codec into a low-power standby state, powering down the analog blocks.
        /// </summary>
        public void Standby()
        {
            WriteRegister(RegAnalogSys5, 0x1F);
            WriteRegister(RegAnalogSys4, 0x8F);
            WriteRegister(RegAnalogLp, 0xFF);
            WriteRegister(RegAnalogSys3, 0x02);
            WriteRegister(RegClockOnOff, 0x00);
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
