// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Device.Model;
using System.Threading;

namespace Iot.Device.Gc0308
{
    /// <summary>
    /// GC0308 0.3MP (VGA) CMOS camera sensor driver.
    /// Communicates via SCCB (I2C-compatible) for register configuration.
    /// Frame capture requires platform-specific DVP/camera controller support.
    /// </summary>
    [Interface("GC0308 - 0.3MP CMOS Camera Sensor")]
    public class Gc0308 : IDisposable
    {
        /// <summary>
        /// Default SCCB/I2C address for the GC0308 (0x21).
        /// </summary>
        public const int DefaultI2cAddress = 0x21;

        /// <summary>
        /// Expected chip ID value (0x9B) read from register 0x00.
        /// </summary>
        public const byte ExpectedChipId = 0x9B;

        private I2cDevice _i2cDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="Gc0308"/> class.
        /// After construction the sensor is initialized with default settings
        /// for VGA (640x480) YCbCr 4:2:2 output with auto exposure and auto white balance.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for SCCB communication.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is null.</exception>
        public Gc0308(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

            Initialize();
        }

        /// <summary>
        /// Gets the chip identification value (read-only).
        /// Expected to return <see cref="ExpectedChipId"/> (0x9B) for a GC0308 sensor.
        /// </summary>
        [Property]
        public byte ChipId => ReadRegister(Gc0308Register.ChipId);

        /// <summary>
        /// Gets or sets the output pixel format.
        /// For YCbCr and RGB565, only the lower 4 bits are modified.
        /// Grayscale mode requires a full register write (0xB1).
        /// </summary>
        [Property]
        public OutputFormat OutputFormat
        {
            get
            {
                byte regVal = ReadRegister(Gc0308Register.OutputFormat);
                if (regVal == (byte)OutputFormat.Grayscale)
                {
                    return OutputFormat.Grayscale;
                }

                return (OutputFormat)(regVal & 0x0F);
            }

            set
            {
                if (value == OutputFormat.Grayscale)
                {
                    WriteRegister(Gc0308Register.OutputFormat, (byte)OutputFormat.Grayscale);
                }
                else
                {
                    UpdateRegisterBits(Gc0308Register.OutputFormat, 0x0F, (byte)value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the image orientation (mirror/flip).
        /// </summary>
        [Property]
        public MirrorFlip Orientation
        {
            get => (MirrorFlip)(ReadRegister(Gc0308Register.AnalogMode1) & 0x03);
            set => UpdateRegisterBits(Gc0308Register.AnalogMode1, 0x03, (byte)value);
        }

        /// <summary>
        /// Gets or sets the exposure control mode.
        /// Controls bit 0 of the AAAA_EN register (0x22).
        /// </summary>
        [Property]
        public ExposureMode ExposureMode
        {
            get => (ExposureMode)(ReadRegister(Gc0308Register.AaaaEnable) & 0x01);
            set => UpdateRegisterBits(Gc0308Register.AaaaEnable, 0x01, (byte)value);
        }

        /// <summary>
        /// Gets or sets the contrast level.
        /// 0x40 = 1.0x (default). Higher values increase contrast.
        /// </summary>
        [Property]
        public byte Contrast
        {
            get => ReadRegister(Gc0308Register.Contrast);
            set => WriteRegister(Gc0308Register.Contrast, value);
        }

        /// <summary>
        /// Gets or sets the color saturation level.
        /// 0x40 = 1.0x (default). Setting applies to both Cb and Cr channels.
        /// Set to 0 for grayscale output. Higher values produce more vivid colors.
        /// </summary>
        [Property]
        public byte Saturation
        {
            get => ReadRegister(Gc0308Register.SaturationCb);
            set
            {
                WriteRegister(Gc0308Register.SaturationCb, value);
                WriteRegister(Gc0308Register.SaturationCr, value);
            }
        }

        /// <summary>
        /// Gets or sets the AEC target luminance (also acts as brightness control).
        /// When auto exposure is enabled, AEC adjusts exposure to match this target Y value.
        /// Default is 0x48. Higher values produce brighter images.
        /// </summary>
        [Property]
        public byte AecTarget
        {
            get => ReadRegister(Gc0308Register.AecTargetY);
            set => WriteRegister(Gc0308Register.AecTargetY, value);
        }

        /// <summary>
        /// Gets or sets the global analog gain.
        /// </summary>
        [Property]
        public byte GlobalGain
        {
            get => ReadRegister(Gc0308Register.GlobalGain);
            set => WriteRegister(Gc0308Register.GlobalGain, value);
        }

        /// <summary>
        /// Initializes the sensor with the default register configuration.
        /// Configures VGA (640x480), YCbCr 4:2:2 output, AEC on, AWB on.
        /// </summary>
        public void Initialize()
        {
            // Apply the full default initialization table
            WriteRegisterTable(Gc0308Config.DefaultInitSequence);

            // Allow the ISP to stabilize
            Thread.Sleep(50);
        }

        /// <summary>
        /// Performs a software reset and re-initializes the sensor.
        /// After reset, the sensor is in the default VGA configuration.
        /// </summary>
        public void Reset()
        {
            // Write 0xF0 to RESET_RELATED (0xFE) to trigger a software reset
            WriteRegister(Gc0308Register.PageSelect, 0xF0);
            Thread.Sleep(80);
            Initialize();
        }

        /// <summary>
        /// Sets the output resolution using sub-sampling (Page 1 registers) and windowing.
        /// </summary>
        /// <param name="resolution">The desired output resolution.</param>
        public void SetResolution(Resolution resolution)
        {
            // Ensure we start on page 0
            WriteRegister(Gc0308Register.PageSelect, 0x00);

            switch (resolution)
            {
                case Resolution.Vga640x480:
                    // Disable sub-sampling on page 1
                    WriteRegister(Gc0308Register.PageSelect, 0x01);
                    UpdateRegisterBits(0x53, 0x80, 0x00);
                    WriteRegister(Gc0308Register.PageSelect, 0x00);

                    // Full window (640+8 x 480+8)
                    SetWindowInternal(0, 0, 648, 488);
                    break;

                case Resolution.Qvga320x240:
                    // Full window for maximum field of view
                    SetWindowInternal(0, 0, 648, 488);

                    // Enable 1/2 sub-sampling on page 1
                    WriteRegister(Gc0308Register.PageSelect, 0x01);
                    UpdateRegisterBits(0x53, 0x80, 0x80);
                    UpdateRegisterBits(0x55, 0x01, 0x01);
                    WriteRegister(0x54, 0x22);
                    WriteRegister(0x56, 0x00);
                    WriteRegister(0x57, 0x00);
                    WriteRegister(0x58, 0x00);
                    WriteRegister(0x59, 0x00);
                    WriteRegister(Gc0308Register.PageSelect, 0x00);
                    break;

                case Resolution.Qqvga160x120:
                    // Full window for maximum field of view
                    SetWindowInternal(0, 0, 648, 488);

                    // Enable 1/4 sub-sampling on page 1
                    WriteRegister(Gc0308Register.PageSelect, 0x01);
                    UpdateRegisterBits(0x53, 0x80, 0x80);
                    UpdateRegisterBits(0x55, 0x01, 0x01);
                    WriteRegister(0x54, 0x44);
                    WriteRegister(0x56, 0x00);
                    WriteRegister(0x57, 0x00);
                    WriteRegister(0x58, 0x00);
                    WriteRegister(0x59, 0x00);
                    WriteRegister(Gc0308Register.PageSelect, 0x00);
                    break;

                case Resolution.Cif352x288:
                    // Disable sub-sampling
                    WriteRegister(Gc0308Register.PageSelect, 0x01);
                    UpdateRegisterBits(0x53, 0x80, 0x00);
                    WriteRegister(Gc0308Register.PageSelect, 0x00);

                    // Center a 352x288 window on the sensor
                    ushort colStart = (640 - 352) / 2;
                    ushort rowStart = (480 - 288) / 2;
                    SetWindowInternal(colStart, rowStart, (ushort)(352 + 8), (ushort)(288 + 8));
                    break;
            }
        }

        /// <summary>
        /// Sets the custom capture window (region of interest) on the sensor array.
        /// The sensor adds an 8-pixel ISP margin automatically; provide the desired output dimensions.
        /// </summary>
        /// <param name="x">Horizontal start offset (column).</param>
        /// <param name="y">Vertical start offset (row).</param>
        /// <param name="width">Desired output width (must not exceed active array width minus offset).</param>
        /// <param name="height">Desired output height (must not exceed active array height minus offset).</param>
        public void SetWindow(ushort x, ushort y, ushort width, ushort height)
        {
            SetWindowInternal(x, y, (ushort)(width + 8), (ushort)(height + 8));
        }

        /// <summary>
        /// Sets the special image effect.
        /// Controls bits[1:0] of the SPECIAL_EFFECT register (0x23).
        /// </summary>
        /// <param name="effect">The desired special effect.</param>
        public void SetSpecialEffect(SpecialEffect effect)
        {
            byte index = (byte)effect;
            if (index >= Gc0308Config.SpecialEffectValues.Length)
            {
                return;
            }

            UpdateRegisterBits(Gc0308Register.SpecialEffect, 0x03, Gc0308Config.SpecialEffectValues[index]);
        }

        /// <summary>
        /// Sets the white balance mode.
        /// When set to <see cref="WhiteBalanceMode.Auto"/>, AWB is enabled via bit 1 of AAAA_EN
        /// and the sensor adjusts R/G/B gains automatically.
        /// When set to a preset, AWB is disabled and fixed gains are applied.
        /// </summary>
        /// <param name="mode">The desired white balance mode.</param>
        public void SetWhiteBalance(WhiteBalanceMode mode)
        {
            if (mode == WhiteBalanceMode.Auto)
            {
                // Enable AWB (bit 1 of AAAA_EN)
                UpdateRegisterBits(Gc0308Register.AaaaEnable, 0x02, 0x02);

                // Restore default AWB gains
                byte[] autoGains = Gc0308Config.WhiteBalancePresets[0];
                WriteRegister(Gc0308Register.AwbRGain, autoGains[0]);
                WriteRegister(Gc0308Register.AwbGGain, autoGains[1]);
                WriteRegister(Gc0308Register.AwbBGain, autoGains[2]);
            }
            else
            {
                // Disable AWB (bit 1 of AAAA_EN)
                UpdateRegisterBits(Gc0308Register.AaaaEnable, 0x02, 0x00);

                byte index = (byte)mode;
                if (index < Gc0308Config.WhiteBalancePresets.Length)
                {
                    byte[] preset = Gc0308Config.WhiteBalancePresets[index];
                    WriteRegister(Gc0308Register.AwbRGain, preset[0]);
                    WriteRegister(Gc0308Register.AwbGGain, preset[1]);
                    WriteRegister(Gc0308Register.AwbBGain, preset[2]);
                }
            }
        }

        /// <summary>
        /// Sets the pixel clock divider to control the frame rate.
        /// Higher divider values reduce the pixel clock and thus the frame rate.
        /// </summary>
        /// <param name="divider">Clock divider value (0 = no division).</param>
        public void SetFrameRate(byte divider)
        {
            WriteRegister(Gc0308Register.ClockDiv, divider);
        }

        /// <summary>
        /// Enables or disables the built-in color bar test pattern.
        /// Controls bit 0 of the OUT_CTRL register (0x2E).
        /// </summary>
        /// <param name="enable">True to enable test pattern output, false for normal camera output.</param>
        public void SetTestPattern(bool enable)
        {
            UpdateRegisterBits(Gc0308Register.OutCtrl, 0x01, enable ? (byte)0x01 : (byte)0x00);
        }

        /// <summary>
        /// Reads a single register value via SCCB.
        /// </summary>
        /// <param name="register">The register address to read.</param>
        /// <returns>The register value.</returns>
        public byte ReadRegister(byte register)
        {
            _i2cDevice.WriteByte(register);
            return _i2cDevice.ReadByte();
        }

        /// <summary>
        /// Writes a single register value via SCCB.
        /// </summary>
        /// <param name="register">The register address to write.</param>
        /// <param name="value">The value to write.</param>
        public void WriteRegister(byte register, byte value)
        {
            SpanByte buffer = new byte[2];
            buffer[0] = register;
            buffer[1] = value;
            _i2cDevice.Write(buffer);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_i2cDevice != null)
            {
                _i2cDevice?.Dispose();
                _i2cDevice = null;
            }
        }

        private void UpdateRegisterBits(Gc0308Register register, byte mask, byte value)
        {
            byte current = ReadRegister((byte)register);
            current = (byte)((current & ~mask) | (value & mask));
            WriteRegister((byte)register, current);
        }

        private void UpdateRegisterBits(byte register, byte mask, byte value)
        {
            byte current = ReadRegister(register);
            current = (byte)((current & ~mask) | (value & mask));
            WriteRegister(register, current);
        }

        private void SetWindowInternal(ushort col, ushort row, ushort width, ushort height)
        {
            WriteRegister(Gc0308Register.RowStartHigh, (byte)(row >> 8));
            WriteRegister(Gc0308Register.RowStartLow, (byte)(row & 0xFF));
            WriteRegister(Gc0308Register.ColStartHigh, (byte)(col >> 8));
            WriteRegister(Gc0308Register.ColStartLow, (byte)(col & 0xFF));
            WriteRegister(Gc0308Register.WindowHeightHigh, (byte)(height >> 8));
            WriteRegister(Gc0308Register.WindowHeightLow, (byte)(height & 0xFF));
            WriteRegister(Gc0308Register.WindowWidthHigh, (byte)(width >> 8));
            WriteRegister(Gc0308Register.WindowWidthLow, (byte)(width & 0xFF));
        }

        private byte ReadRegister(Gc0308Register register)
        {
            return ReadRegister((byte)register);
        }

        private void WriteRegister(Gc0308Register register, byte value)
        {
            WriteRegister((byte)register, value);
        }

        private void WriteRegisterTable(byte[][] table)
        {
            for (int i = 0; i < table.Length; i++)
            {
                WriteRegister((Gc0308Register)table[i][0], table[i][1]);
            }
        }
    }
}
