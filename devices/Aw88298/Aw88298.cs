// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.Aw88298
{
    /// <summary>
    /// Driver for the Awinic AW88298 I2S Class-D smart audio amplifier (speaker output).
    /// </summary>
    /// <remarks>
    /// The AW88298 drives a speaker from an I2S PCM stream. This binding covers the I2C control plane
    /// only (reset, power/enable, output volume, mute and the boost configuration). The PCM audio samples
    /// are streamed over I2S using <c>System.Device.I2s</c> and are not handled by this class. The
    /// register sequences are ported from the Espressif <c>esp_codec_dev</c> component. The AW88298 uses
    /// 16-bit registers.
    /// </remarks>
    public class Aw88298 : IDisposable
    {
        /// <summary>
        /// Default I2C address for the AW88298 (7-bit).
        /// </summary>
        public const byte DefaultI2cAddress = 0x36;

        // Register map (see AW88298 datasheet / Espressif esp_codec_dev driver). Registers are 16-bit.
        private const byte RegReset = 0x00;
        private const byte RegSysControl = 0x04;
        private const byte RegSysControl2 = 0x05;
        private const byte RegI2sControl = 0x06;
        private const byte RegVolume = 0x0C;
        private const byte RegBoostControl2 = 0x61;

        // Mute bit inside the system control 2 register (0x05).
        private const ushort MuteBit = 1 << 4;

        // The digital volume register value for maximum (0 dB) and minimum (-96 dB) output.
        private const int VolumeRegisterMax = 0x00;
        private const int VolumeRegisterMin = 0xC0;

        private readonly bool _shouldDispose;
        private I2cDevice _i2cDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="Aw88298" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication with the amplifier.</param>
        /// <param name="shouldDispose"><see langword="true" /> to dispose the <paramref name="i2cDevice"/> when this instance is disposed; otherwise, <see langword="false" />.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <see langword="null" />.</exception>
        public Aw88298(I2cDevice i2cDevice, bool shouldDispose = true)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException();
            _shouldDispose = shouldDispose;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the amplifier output is muted.
        /// </summary>
        /// <value><see langword="true" /> when the output is muted; otherwise, <see langword="false" />.</value>
        public bool Muted
        {
            get => (ReadRegister(RegSysControl2) & MuteBit) != 0;

            set
            {
                ushort registerValue = (ushort)(ReadRegister(RegSysControl2) & ~MuteBit);
                if (value)
                {
                    registerValue |= MuteBit;
                }

                WriteRegister(RegSysControl2, registerValue);
            }
        }

        /// <summary>
        /// Gets or sets the amplifier output volume as a percentage from 0 (minimum) to 100 (maximum).
        /// </summary>
        /// <value>The output volume as a percentage from 0 (minimum) to 100 (maximum).</value>
        /// <remarks>
        /// The percentage is mapped linearly onto the digital volume field (bits 15:8) of register 0x0C.
        /// </remarks>
        public byte Volume
        {
            get
            {
                int registerValue = (ReadRegister(RegVolume) >> 8) & 0xFF;
                return (byte)(100 - (((registerValue * 100) + (VolumeRegisterMin / 2)) / VolumeRegisterMin));
            }

            set
            {
                if (value > 100)
                {
                    value = 100;
                }

                // 100% maps to 0x00 (0 dB, loudest) and 0% maps to 0xC0 (-96 dB, quietest).
                int registerValue = ((100 - value) * VolumeRegisterMin) / 100;
                WriteRegister(RegVolume, (ushort)((registerValue << 8) | 0x64));
            }
        }

        /// <summary>
        /// Resets and configures the amplifier for I2S playback (16-bit samples).
        /// </summary>
        /// <param name="sampleRate">The I2S sample rate, in Hz, of the audio stream that will be sent to the amplifier (for example 16000).</param>
        /// <remarks>
        /// This runs the AW88298 initialization register sequence ported from the M5Stack CoreS3
        /// (M5Unified) / Espressif <c>esp_codec_dev</c> drivers. The I2S control register (0x06) is
        /// derived from <paramref name="sampleRate"/> so the amplifier locks to the incoming stream.
        /// The amplifier is left enabled; call <see cref="Start" /> to (re-)enable the output stage after
        /// a <see cref="Stop" />.
        /// </remarks>
        public void Initialize(int sampleRate = 16000)
        {
            WriteRegister(RegBoostControl2, 0x0673);
            WriteRegister(RegSysControl, 0x4040);
            WriteRegister(RegSysControl2, 0x0008);
            WriteRegister(RegI2sControl, GetI2sControlRegister(sampleRate));
            WriteRegister(RegVolume, 0x0064);
        }

        // Builds the I2SCTRL (0x06) register value for a given sample rate. Ported from the M5Stack
        // CoreS3 / esp_codec_dev AW88298 driver: the sample rate is encoded as an index into a table of
        // 2205 Hz (44100/20) multiples, OR'd with the 16-bit BCK base value (0x14C0 = 16*2 BCK mode).
        private static ushort GetI2sControlRegister(int sampleRate)
        {
            byte[] rateTable = new byte[] { 4, 5, 6, 8, 10, 11, 15, 20, 22, 44 };
            int rate = (sampleRate + 1102) / 2205;
            int index = 0;
            while (index < rateTable.Length - 1 && rate > rateTable[index])
            {
                index++;
            }

            return (ushort)(0x14C0 | index);
        }

        /// <summary>
        /// Powers up the amplifier output stage so it can drive the speaker.
        /// </summary>
        public void Start()
        {
            ushort value = (ushort)(ReadRegister(RegSysControl) & ~0x03);
            value |= 1 << 6;
            WriteRegister(RegSysControl, value);
        }

        /// <summary>
        /// Powers down the amplifier output stage.
        /// </summary>
        public void Stop()
        {
            ushort value = (ushort)(ReadRegister(RegSysControl) | 0x03);
            value &= unchecked((ushort)~(1 << 6));
            WriteRegister(RegSysControl, value);
        }

        private void WriteRegister(byte register, ushort value)
        {
            SpanByte writeBuffer = new byte[3];
            writeBuffer[0] = register;
            writeBuffer[1] = (byte)(value >> 8);
            writeBuffer[2] = (byte)(value & 0xFF);
            _i2cDevice.Write(writeBuffer);
        }

        private ushort ReadRegister(byte register)
        {
            SpanByte writeBuffer = new byte[1];
            writeBuffer[0] = register;
            SpanByte readBuffer = new byte[2];
            _i2cDevice.WriteRead(writeBuffer, readBuffer);
            return (ushort)((readBuffer[0] << 8) | readBuffer[1]);
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
