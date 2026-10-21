// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Gc0308
{
    /// <summary>
    /// GC0308 register addresses.
    /// Register addresses verified against the Espressif esp32-camera reference driver
    /// and the GC0308 datasheet. Use <see cref="Gc0308Register.PageSelect"/> to switch
    /// between register pages 0–3.
    /// </summary>
    internal enum Gc0308Register : byte
    {
        // ───── System / Chip ID ─────

        /// <summary>Chip ID register (read-only, expected value 0x9B).</summary>
        ChipId = 0x00,

        // ───── Blanking / Timing ─────

        /// <summary>Horizontal blanking interval register.</summary>
        HBlanking = 0x01,

        /// <summary>Vertical blanking interval register.</summary>
        VBlanking = 0x02,

        // ───── Exposure ─────

        /// <summary>Exposure time high byte (bits 11:4).</summary>
        ExposureHigh = 0x03,

        /// <summary>Exposure time low byte (bits 3:0 in upper nibble).</summary>
        ExposureLow = 0x04,

        // ───── Windowing ─────

        /// <summary>Row start address high byte.</summary>
        RowStartHigh = 0x05,

        /// <summary>Row start address low byte.</summary>
        RowStartLow = 0x06,

        /// <summary>Column start address high byte.</summary>
        ColStartHigh = 0x07,

        /// <summary>Column start address low byte.</summary>
        ColStartLow = 0x08,

        /// <summary>Window height high byte.</summary>
        WindowHeightHigh = 0x09,

        /// <summary>Window height low byte.</summary>
        WindowHeightLow = 0x0A,

        /// <summary>Window width high byte.</summary>
        WindowWidthHigh = 0x0B,

        /// <summary>Window width low byte.</summary>
        WindowWidthLow = 0x0C,

        /// <summary>VSYNC start position.</summary>
        VsyncStart = 0x0D,

        /// <summary>VSYNC end position.</summary>
        VsyncEnd = 0x0E,

        // ───── Sensor Control ─────

        /// <summary>CISCTL mode 1. Bit[0]: horizontal mirror, Bit[1]: vertical flip.</summary>
        AnalogMode1 = 0x14,

        // ───── Auto Algorithm Enable (AAAA_EN) ─────

        /// <summary>
        /// Auto algorithm enable register.
        /// Bit[0]: AEC enable, Bit[1]: AWB enable, Bit[2]: AGC enable.
        /// </summary>
        AaaaEnable = 0x22,

        // ───── Special Effects ─────

        /// <summary>Special effect control. Bit[1:0]: 00=normal, 01=negative, 10=grayscale.</summary>
        SpecialEffect = 0x23,

        // ───── Output Format ─────

        /// <summary>Output format register. Bit[3:0]: format selection (0x02=YCbCr, 0x06=RGB565).</summary>
        OutputFormat = 0x24,

        // ───── Clock / Frame Rate ─────

        /// <summary>Pixel clock divider register.</summary>
        ClockDiv = 0x28,

        // ───── Output Control ─────

        /// <summary>Output control register. Bit[0]: color bar test pattern enable.</summary>
        OutCtrl = 0x2E,

        // ───── Global Gain ─────

        /// <summary>Global analog gain register.</summary>
        GlobalGain = 0x50,

        // ───── White Balance Gains ─────

        /// <summary>AWB red channel gain.</summary>
        AwbRGain = 0x5A,

        /// <summary>AWB green channel gain.</summary>
        AwbGGain = 0x5B,

        /// <summary>AWB blue channel gain.</summary>
        AwbBGain = 0x5C,

        // ───── Saturation ─────

        /// <summary>Cb channel saturation gain (0x40 = 1.0x).</summary>
        SaturationCb = 0xB1,

        /// <summary>Cr channel saturation gain (0x40 = 1.0x).</summary>
        SaturationCr = 0xB2,

        // ───── Contrast ─────

        /// <summary>Contrast control (0x40 = 1.0x).</summary>
        Contrast = 0xB3,

        // ───── AEC Target ─────

        /// <summary>AEC target luminance Y value (default 0x48). Also controls brightness.</summary>
        AecTargetY = 0xD3,

        // ───── Crop Window (for windowing mode) ─────

        /// <summary>Subsample column start (units of 4 pixels).</summary>
        SubColN = 0xF7,

        /// <summary>Subsample row start (units of 4 pixels).</summary>
        SubRowN = 0xF8,

        /// <summary>Subsample column end (units of 4 pixels).</summary>
        SubColN1 = 0xF9,

        /// <summary>Subsample row end (units of 4 pixels).</summary>
        SubRowN1 = 0xFA,

        // ───── Page Select ─────

        /// <summary>Page select register. Write page number (0–3) to switch register pages.</summary>
        PageSelect = 0xFE,
    }
}
