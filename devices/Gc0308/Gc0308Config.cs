// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Gc0308
{
    /// <summary>
    /// Default register initialization sequences for the GC0308 camera sensor.
    /// These tables contain register address/value pairs written during initialization
    /// to configure the analog front end, ISP pipeline, and output interface.
    /// </summary>
    /// <remarks>
    /// The initialization sequence is derived from the Espressif esp32-camera
    /// reference implementation (gc0308_settings.h) and the GC0308 datasheet.
    /// Each pair in the arrays is { register_address, register_value }.
    /// </remarks>
    internal static class Gc0308Config
    {
        /// <summary>
        /// Default initialization sequence for the GC0308.
        /// Derived from the Espressif esp32-camera gc0308_sensor_default_regs table.
        /// Sets up analog front end, ISP, AEC, AWB, gamma, and output format
        /// for VGA (640x480) YCbCr 4:2:2 output.
        /// </summary>
        internal static readonly byte[][] DefaultInitSequence = new byte[][]
        {
            // ── Page 0 select ──
            new byte[] { 0xFE, 0x00 },

            // ── Misc / Timing ──
            new byte[] { 0xEC, 0x20 },

            // ── Window (VGA 640x480 + 8px ISP margin) ──
            new byte[] { 0x05, 0x00 },  // Row start high
            new byte[] { 0x06, 0x00 },  // Row start low
            new byte[] { 0x07, 0x00 },  // Column start high
            new byte[] { 0x08, 0x00 },  // Column start low
            new byte[] { 0x09, 0x01 },  // Window height high (488)
            new byte[] { 0x0A, 0xE8 },  // Window height low
            new byte[] { 0x0B, 0x02 },  // Window width high (648)
            new byte[] { 0x0C, 0x88 },  // Window width low
            new byte[] { 0x0D, 0x02 },  // VSYNC start
            new byte[] { 0x0E, 0x02 },  // VSYNC end

            // ── Analog front end ──
            new byte[] { 0x10, 0x26 },
            new byte[] { 0x11, 0x0D },
            new byte[] { 0x12, 0x2A },
            new byte[] { 0x13, 0x00 },
            new byte[] { 0x14, 0x10 },  // CISCTL_MODE1: no mirror, no flip
            new byte[] { 0x15, 0x0A },
            new byte[] { 0x16, 0x05 },
            new byte[] { 0x17, 0x01 },
            new byte[] { 0x18, 0x44 },
            new byte[] { 0x19, 0x44 },
            new byte[] { 0x1A, 0x2A },
            new byte[] { 0x1B, 0x00 },
            new byte[] { 0x1C, 0x49 },
            new byte[] { 0x1D, 0x9A },
            new byte[] { 0x1E, 0x61 },
            new byte[] { 0x1F, 0x00 },  // Pad drive <=24MHz

            // ── ISP block enable ──
            new byte[] { 0x20, 0x7F },
            new byte[] { 0x21, 0xFA },
            new byte[] { 0x22, 0x57 },  // AAAA_EN: AEC+AWB+AGC enabled

            // ── Output format ──
            new byte[] { 0x24, 0xA2 },  // YCbCr 4:2:2 (bits[3:0]=0x2)

            // ── Misc ISP ──
            new byte[] { 0x25, 0x0F },
            new byte[] { 0x26, 0x03 },
            new byte[] { 0x28, 0x00 },  // Clock divider: no division
            new byte[] { 0x2D, 0x0A },
            new byte[] { 0x2F, 0x01 },
            new byte[] { 0x30, 0xF7 },
            new byte[] { 0x31, 0x50 },
            new byte[] { 0x32, 0x00 },
            new byte[] { 0x33, 0x28 },
            new byte[] { 0x34, 0x2A },
            new byte[] { 0x35, 0x28 },
            new byte[] { 0x39, 0x04 },
            new byte[] { 0x3A, 0x20 },
            new byte[] { 0x3B, 0x20 },
            new byte[] { 0x3C, 0x00 },
            new byte[] { 0x3D, 0x00 },
            new byte[] { 0x3E, 0x00 },
            new byte[] { 0x3F, 0x00 },

            // ── Global gain ──
            new byte[] { 0x50, 0x14 },  // Default global gain

            // ── Analog gain ──
            new byte[] { 0x52, 0x41 },
            new byte[] { 0x53, 0x80 },
            new byte[] { 0x54, 0x80 },
            new byte[] { 0x55, 0x80 },
            new byte[] { 0x56, 0x80 },

            // ── AWB gains ──
            new byte[] { 0x5A, 0x56 },  // AWB R gain
            new byte[] { 0x5B, 0x40 },  // AWB G gain
            new byte[] { 0x5C, 0x4A },  // AWB B gain

            // ── AWB ranges ──
            new byte[] { 0x8B, 0x20 },
            new byte[] { 0x8C, 0x20 },
            new byte[] { 0x8D, 0x20 },
            new byte[] { 0x8E, 0x14 },
            new byte[] { 0x8F, 0x10 },
            new byte[] { 0x90, 0x14 },
            new byte[] { 0x91, 0x3C },
            new byte[] { 0x92, 0x50 },

            // ── AWB parameters ──
            new byte[] { 0x5D, 0x12 },
            new byte[] { 0x5E, 0x1A },
            new byte[] { 0x5F, 0x24 },
            new byte[] { 0x60, 0x07 },
            new byte[] { 0x61, 0x15 },
            new byte[] { 0x62, 0x08 },
            new byte[] { 0x64, 0x03 },
            new byte[] { 0x66, 0xE8 },
            new byte[] { 0x67, 0x86 },
            new byte[] { 0x68, 0x82 },
            new byte[] { 0x69, 0x18 },
            new byte[] { 0x6A, 0x0F },
            new byte[] { 0x6B, 0x00 },
            new byte[] { 0x6C, 0x5F },
            new byte[] { 0x6D, 0x8F },
            new byte[] { 0x6E, 0x55 },
            new byte[] { 0x6F, 0x38 },
            new byte[] { 0x70, 0x15 },
            new byte[] { 0x71, 0x33 },
            new byte[] { 0x72, 0xDC },
            new byte[] { 0x73, 0x00 },
            new byte[] { 0x74, 0x02 },
            new byte[] { 0x75, 0x3F },
            new byte[] { 0x76, 0x02 },

            // ── Edge enhancement ──
            new byte[] { 0x77, 0x38 },
            new byte[] { 0x78, 0x88 },
            new byte[] { 0x79, 0x81 },
            new byte[] { 0x7A, 0x81 },
            new byte[] { 0x7B, 0x22 },
            new byte[] { 0x7C, 0xFF },

            // ── Color matrix ──
            new byte[] { 0x93, 0x48 },
            new byte[] { 0x94, 0x02 },
            new byte[] { 0x95, 0x07 },
            new byte[] { 0x96, 0xE0 },
            new byte[] { 0x97, 0x40 },
            new byte[] { 0x98, 0xF0 },

            // ── Saturation / Contrast ──
            new byte[] { 0xB1, 0x40 },  // Saturation Cb: 1.0x
            new byte[] { 0xB2, 0x40 },  // Saturation Cr: 1.0x
            new byte[] { 0xB3, 0x40 },  // Contrast: 1.0x
            new byte[] { 0xB6, 0xE0 },
            new byte[] { 0xBD, 0x38 },
            new byte[] { 0xBE, 0x36 },

            // ── AEC control ──
            new byte[] { 0xD0, 0xCB },  // AEC mode 1
            new byte[] { 0xD1, 0x10 },  // AEC mode 2
            new byte[] { 0xD2, 0x90 },  // AEC mode 3
            new byte[] { 0xD3, 0x48 },  // AEC target Y (brightness)
            new byte[] { 0xD5, 0xF2 },
            new byte[] { 0xD6, 0x16 },
            new byte[] { 0xDB, 0x92 },
            new byte[] { 0xDC, 0xA5 },
            new byte[] { 0xDF, 0x23 },
            new byte[] { 0xD9, 0x00 },
            new byte[] { 0xDA, 0x00 },
            new byte[] { 0xE0, 0x09 },
            new byte[] { 0xED, 0x04 },
            new byte[] { 0xEE, 0xA0 },
            new byte[] { 0xEF, 0x40 },
            new byte[] { 0x80, 0x03 },

            // ── Gamma curve (Y) ──
            new byte[] { 0x9F, 0x10 },
            new byte[] { 0xA0, 0x20 },
            new byte[] { 0xA1, 0x38 },
            new byte[] { 0xA2, 0x4E },
            new byte[] { 0xA3, 0x63 },
            new byte[] { 0xA4, 0x76 },
            new byte[] { 0xA5, 0x87 },
            new byte[] { 0xA6, 0xA2 },
            new byte[] { 0xA7, 0xB8 },
            new byte[] { 0xA8, 0xCA },
            new byte[] { 0xA9, 0xD8 },
            new byte[] { 0xAA, 0xE3 },
            new byte[] { 0xAB, 0xEB },
            new byte[] { 0xAC, 0xF0 },
            new byte[] { 0xAD, 0xF8 },
            new byte[] { 0xAE, 0xFD },
            new byte[] { 0xAF, 0xFF },

            // ── Color correction / Gamma (edge) ──
            new byte[] { 0xC0, 0x00 },
            new byte[] { 0xC1, 0x10 },
            new byte[] { 0xC2, 0x1C },
            new byte[] { 0xC3, 0x30 },
            new byte[] { 0xC4, 0x43 },
            new byte[] { 0xC5, 0x54 },
            new byte[] { 0xC6, 0x65 },
            new byte[] { 0xC7, 0x75 },
            new byte[] { 0xC8, 0x93 },
            new byte[] { 0xC9, 0xB0 },
            new byte[] { 0xCA, 0xCB },
            new byte[] { 0xCB, 0xE6 },
            new byte[] { 0xCC, 0xFF },

            // ── Cropping ──
            new byte[] { 0xF0, 0x02 },
            new byte[] { 0xF1, 0x01 },
            new byte[] { 0xF2, 0x02 },
            new byte[] { 0xF3, 0x30 },
            new byte[] { 0xF7, 0x04 },  // SubColN
            new byte[] { 0xF8, 0x02 },  // SubRowN
            new byte[] { 0xF9, 0x9F },  // SubColN1
            new byte[] { 0xFA, 0x78 },  // SubRowN1

            // ── Page 1 registers ──
            new byte[] { 0xFE, 0x01 },  // Switch to Page 1

            new byte[] { 0x00, 0xF5 },
            new byte[] { 0x02, 0x20 },
            new byte[] { 0x04, 0x10 },
            new byte[] { 0x05, 0x08 },
            new byte[] { 0x06, 0x20 },
            new byte[] { 0x08, 0x0A },
            new byte[] { 0x0A, 0xA0 },
            new byte[] { 0x0B, 0x60 },
            new byte[] { 0x0C, 0x08 },
            new byte[] { 0x0E, 0x44 },
            new byte[] { 0x0F, 0x32 },
            new byte[] { 0x10, 0x41 },
            new byte[] { 0x11, 0x37 },
            new byte[] { 0x12, 0x22 },
            new byte[] { 0x13, 0x19 },
            new byte[] { 0x14, 0x44 },
            new byte[] { 0x15, 0x44 },
            new byte[] { 0x16, 0xC2 },
            new byte[] { 0x17, 0xA8 },
            new byte[] { 0x18, 0x18 },
            new byte[] { 0x19, 0x50 },
            new byte[] { 0x1A, 0xD8 },
            new byte[] { 0x1B, 0xF5 },
            new byte[] { 0x70, 0x40 },
            new byte[] { 0x71, 0x58 },
            new byte[] { 0x72, 0x30 },
            new byte[] { 0x73, 0x48 },
            new byte[] { 0x74, 0x20 },
            new byte[] { 0x75, 0x60 },
            new byte[] { 0x77, 0x20 },
            new byte[] { 0x78, 0x32 },
            new byte[] { 0x30, 0x03 },
            new byte[] { 0x31, 0x40 },
            new byte[] { 0x32, 0x10 },
            new byte[] { 0x33, 0xE0 },
            new byte[] { 0x34, 0xE0 },
            new byte[] { 0x35, 0x00 },
            new byte[] { 0x36, 0x80 },
            new byte[] { 0x37, 0x00 },
            new byte[] { 0x38, 0x04 },
            new byte[] { 0x39, 0x09 },
            new byte[] { 0x3A, 0x12 },
            new byte[] { 0x3B, 0x1C },
            new byte[] { 0x3C, 0x28 },
            new byte[] { 0x3D, 0x31 },
            new byte[] { 0x3E, 0x44 },
            new byte[] { 0x3F, 0x57 },
            new byte[] { 0x40, 0x6C },
            new byte[] { 0x41, 0x81 },
            new byte[] { 0x42, 0x94 },
            new byte[] { 0x43, 0xA7 },
            new byte[] { 0x44, 0xB8 },
            new byte[] { 0x45, 0xD6 },
            new byte[] { 0x46, 0xEE },
            new byte[] { 0x47, 0x0D },
            new byte[] { 0x62, 0xF7 },
            new byte[] { 0x63, 0x68 },
            new byte[] { 0x64, 0xD3 },
            new byte[] { 0x65, 0xD3 },
            new byte[] { 0x66, 0x60 },

            // ── Back to Page 0 ──
            new byte[] { 0xFE, 0x00 },

            // ── Frame / blanking ──
            new byte[] { 0x01, 0x32 },  // HBlanking
            new byte[] { 0x02, 0x0C },  // VBlanking
            new byte[] { 0x0F, 0x01 },  // Misc control

            // ── Anti-flicker / exposure levels ──
            new byte[] { 0xE2, 0x00 },
            new byte[] { 0xE3, 0x78 },
            new byte[] { 0xE4, 0x00 },
            new byte[] { 0xE5, 0xFE },
            new byte[] { 0xE6, 0x01 },
            new byte[] { 0xE7, 0xE0 },
            new byte[] { 0xE8, 0x01 },
            new byte[] { 0xE9, 0xE0 },
            new byte[] { 0xEA, 0x01 },
            new byte[] { 0xEB, 0xE0 },

            // ── Ensure page 0 at end ──
            new byte[] { 0xFE, 0x00 },
        };

        /// <summary>
        /// White balance preset gains.
        /// Each entry is { R gain, G gain, B gain }.
        /// Values from the Espressif esp32-camera reference driver.
        /// </summary>
        internal static readonly byte[][] WhiteBalancePresets = new byte[][]
        {
            // Auto (default gains, AWB handles adjustment)
            new byte[] { 0x56, 0x40, 0x4A },

            // Sunny / Daylight (~5500K)
            new byte[] { 0x74, 0x52, 0x40 },

            // Cloudy (~6500K)
            new byte[] { 0x8C, 0x50, 0x40 },

            // Office / Incandescent (~3200K)
            new byte[] { 0x48, 0x40, 0x5C },

            // Home / Fluorescent (~4000K)
            new byte[] { 0x40, 0x42, 0x50 },
        };

        /// <summary>
        /// Special effect register values.
        /// Each entry is the value for bits[1:0] of the SPECIAL_EFFECT register (0x23).
        /// 0x00 = normal, 0x01 = negative, 0x02 = grayscale/tint.
        /// </summary>
        internal static readonly byte[] SpecialEffectValues = new byte[]
        {
            0x00,   // Normal
            0x02,   // Grayscale
            0x02,   // Sepia (grayscale mode, tint not fully supported)
            0x01,   // Negative
            0x02,   // Green tint (grayscale mode)
            0x02,   // Blue tint (grayscale mode)
            0x02,   // Red tint (grayscale mode)
        };
    }
}
