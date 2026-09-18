// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Es8311
{
    /// <summary>
    /// Driver for the Everest Semiconductor ES8311 low-power mono audio CODEC (combined DAC playback and
    /// ADC microphone capture).
    /// </summary>
    /// <remarks>
    /// This binding covers the I2C control plane only. It configures the codec clocking, powers up the
    /// DAC (playback) and/or the ADC (microphone capture), and exposes the output volume, mute and
    /// microphone gain. The PCM audio samples are streamed over I2S using <c>System.Device.I2s</c> and are
    /// not handled by this class. The register sequences are ported from the M5Stack <c>M5Unified</c> and
    /// Espressif <c>esp_codec_dev</c> drivers. The ES8311 is used, for example, on the M5Stack M5StickS3
    /// (feeding an AW8737 speaker amplifier).
    /// </remarks>
    public class Es8311 : IDisposable
    {
        /// <summary>
        /// Default I2C address for the ES8311 (7-bit), used when the CE pin is tied low.
        /// </summary>
        public const byte DefaultI2cAddress = 0x18;

        /// <summary>
        /// Alternate I2C address for the ES8311 (7-bit), used when the CE pin is tied high.
        /// </summary>
        public const byte AlternateI2cAddress = 0x19;

        /// <summary>
        /// ES8311 device identifier (value of the chip-ID registers 0xFD and 0xFE).
        /// </summary>
        public const int DeviceId = 0x8311;

        /// <summary>
        /// Minimum volume/gain percentage.
        /// </summary>
        public const byte MinVolume = 0;

        /// <summary>
        /// Maximum volume/gain percentage.
        /// </summary>
        public const byte MaxVolume = 100;

        // Register map (see ES8311 datasheet / Espressif esp_codec_dev driver).
        private const byte RegReset = 0x00;
        private const byte RegClockManager1 = 0x01;
        private const byte RegClockManager2 = 0x02;
        private const byte RegSystemAnalog = 0x0D;
        private const byte RegSystemPga = 0x0E;
        private const byte RegSystemDac = 0x12;
        private const byte RegSystemHpDrive = 0x13;
        private const byte RegAdcMicSelect = 0x14;
        private const byte RegAdcVolume = 0x17;
        private const byte RegAdcEqualizer = 0x1C;
        private const byte RegDacMute = 0x31;
        private const byte RegDacVolume = 0x32;
        private const byte RegDacEqualizer = 0x37;
        private const byte RegChipId1 = 0xFD;
        private const byte RegChipId2 = 0xFE;

        // 0x00 = 0 dB attenuation on the DAC volume register (0xBF), used as the 100% reference so the
        // percentage range never pushes the codec into its digital-gain region.
        private const int DacVolumeZeroDb = 0xBF;

        private readonly bool _shouldDispose;
        private I2cDevice _i2cDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="Es8311" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication with the codec.</param>
        /// <param name="shouldDispose"><see langword="true" /> to dispose the <paramref name="i2cDevice"/> when this instance is disposed; otherwise, <see langword="false" />.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <see langword="null" />.</exception>
        public Es8311(I2cDevice i2cDevice, bool shouldDispose = true)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException();
            _shouldDispose = shouldDispose;
        }

        /// <summary>
        /// Gets the device identifier reported by the ES8311.
        /// </summary>
        /// <returns>The device identifier, which should equal <see cref="DeviceId" /> (0x8311).</returns>
        public int GetDeviceId() => (ReadRegister(RegChipId1) << 8) | ReadRegister(RegChipId2);

        /// <summary>
        /// Gets or sets the DAC (playback) output volume as a percentage from 0 (mute) to 100 (0 dB).
        /// </summary>
        /// <value>The output volume as a percentage from <see cref="MinVolume" /> to <see cref="MaxVolume" />.</value>
        public byte Volume
        {
            get => (byte)(((ReadRegister(RegDacVolume) * 100) + (DacVolumeZeroDb / 2)) / DacVolumeZeroDb);

            set
            {
                if (value > MaxVolume)
                {
                    value = MaxVolume;
                }

                WriteRegister(RegDacVolume, (byte)((value * DacVolumeZeroDb) / 100));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the DAC (playback) output is muted.
        /// </summary>
        /// <value><see langword="true" /> when the output is muted; otherwise, <see langword="false" />.</value>
        public bool Muted
        {
            get => (ReadRegister(RegDacMute) & 0x60) != 0;
            set => WriteRegister(RegDacMute, (byte)(value ? 0x60 : 0x00));
        }

        /// <summary>
        /// Gets or sets the microphone (ADC) digital gain as a percentage from 0 to 100 (maximum).
        /// </summary>
        /// <value>The microphone gain as a percentage from 0 to 100.</value>
        public byte MicGain
        {
            get => (byte)(((ReadRegister(RegAdcVolume) * 100) + 127) / 0xFF);

            set
            {
                if (value > MaxVolume)
                {
                    value = MaxVolume;
                }

                WriteRegister(RegAdcVolume, (byte)((value * 0xFF) / 100));
            }
        }

        /// <summary>
        /// Resets the codec and configures its clocking for I2S operation with the ESP32 as the I2S master.
        /// </summary>
        /// <remarks>
        /// Call <see cref="StartPlayback" /> to power up the DAC and/or <see cref="StartCapture" /> to power
        /// up the ADC afterwards. The clock manager is configured to derive MCLK from BCLK, matching the
        /// M5Stack M5StickS3 speaker configuration.
        /// </remarks>
        public void Initialize()
        {
            // Reset the codec exactly as the Espressif esp_codec_dev driver does: assert reset (0x1F),
            // wait for the digital core to settle, release reset, then issue the CSM power-on command.
            WriteRegister(RegReset, 0x1F);
            Thread.Sleep(20);
            WriteRegister(RegReset, 0x00);
            WriteRegister(RegReset, 0x80);
            WriteRegister(RegClockManager1, 0xB5);
            WriteRegister(RegClockManager2, 0x18);
            WriteRegister(RegSystemAnalog, 0x01);
        }

        /// <summary>
        /// Powers up the DAC and the output drive so the codec can play back audio to the speaker/amplifier.
        /// </summary>
        public void StartPlayback()
        {
            WriteRegister(RegSystemDac, 0x00);
            WriteRegister(RegSystemHpDrive, 0x10);
            WriteRegister(RegDacEqualizer, 0x08);
        }

        /// <summary>
        /// Powers up the microphone PGA and ADC so the codec can capture audio from the microphone input.
        /// </summary>
        public void StartCapture()
        {
            WriteRegister(RegSystemPga, 0x02);
            WriteRegister(RegAdcMicSelect, 0x10);
            WriteRegister(RegAdcEqualizer, 0x6A);
        }

        /// <summary>
        /// Powers down the codec analog circuitry (both DAC and ADC).
        /// </summary>
        public void Stop()
        {
            WriteRegister(RegSystemAnalog, 0xFC);
            WriteRegister(RegSystemPga, 0x6A);
            WriteRegister(RegReset, 0x00);
        }

        private void WriteRegister(byte register, byte value)
        {
            SpanByte writeBuffer = new byte[2];
            writeBuffer[0] = register;
            writeBuffer[1] = value;

            // Verify the write acknowledged; a single-shot write can NAK silently. Retry a few times.
            // The ES8311 stretches the I2C clock, which the driver reports as ClockStretchTimeout even
            // though the byte is transferred, so that status is treated as success.
            for (int attempt = 0; attempt < 5; attempt++)
            {
                I2cTransferStatus status = _i2cDevice.Write(writeBuffer).Status;
                if (status == I2cTransferStatus.FullTransfer || status == I2cTransferStatus.ClockStretchTimeout)
                {
                    return;
                }
            }
        }

        private byte ReadRegister(byte register)
        {
            SpanByte writeBuffer = new byte[1];
            writeBuffer[0] = register;
            SpanByte readBuffer = new byte[1];

            // The codec can NAK the first transaction after power-up; retry so a single miss does not
            // surface as a zero read. The ES8311 stretches the I2C clock, which the driver reports as
            // ClockStretchTimeout even though the byte is transferred, so that status is treated as success.
            for (int attempt = 0; attempt < 5; attempt++)
            {
                I2cTransferStatus status = _i2cDevice.WriteRead(writeBuffer, readBuffer).Status;
                if (status == I2cTransferStatus.FullTransfer || status == I2cTransferStatus.ClockStretchTimeout)
                {
                    return readBuffer[0];
                }
            }

            return 0;
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
