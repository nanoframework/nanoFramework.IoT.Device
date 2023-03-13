// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// File Automatically Generated by Bitmap2Font

using System;

namespace Iot.Device.Ssd13xx
{
    /// <summary>
    /// Retro8x16 font.
    /// </summary>
    public class Retro8x16 : IFont
    {
        private static readonly byte[][] _fontTable =
        {
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x00, 0x00, 0x08, 0x08, 0x00, 0x00 },
            new byte[] { 0x24, 0x24, 0x24, 0x24, 0x24, 0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x24, 0x24, 0x24, 0x24, 0x7E, 0x7E, 0x24, 0x24, 0x7E, 0x7E, 0x24, 0x24, 0x24, 0x24, 0x00, 0x00 },
            new byte[] { 0x08, 0x08, 0x3C, 0x3C, 0x0A, 0x0A, 0x1C, 0x1C, 0x28, 0x28, 0x1E, 0x1E, 0x08, 0x08, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x46, 0x46, 0x26, 0x26, 0x10, 0x10, 0x08, 0x08, 0x64, 0x64, 0x62, 0x62, 0x00, 0x00 },
            new byte[] { 0x0C, 0x0C, 0x12, 0x12, 0x12, 0x12, 0x0C, 0x0C, 0x52, 0x52, 0x22, 0x22, 0x5C, 0x5C, 0x00, 0x00 },
            new byte[] { 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x08, 0x08, 0x04, 0x04, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x04, 0x04, 0x08, 0x08, 0x00, 0x00 },
            new byte[] { 0x08, 0x08, 0x10, 0x10, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x10, 0x10, 0x08, 0x08, 0x00, 0x00 },
            new byte[] { 0x08, 0x08, 0x2A, 0x2A, 0x1C, 0x1C, 0x08, 0x08, 0x1C, 0x1C, 0x2A, 0x2A, 0x08, 0x08, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x08, 0x08, 0x08, 0x08, 0x3E, 0x3E, 0x08, 0x08, 0x08, 0x08, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x10, 0x10, 0x10, 0x08, 0x08, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7E, 0x7E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x08, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x40, 0x40, 0x20, 0x20, 0x10, 0x10, 0x08, 0x08, 0x04, 0x04, 0x02, 0x02, 0x00, 0x00 },
            new byte[] { 0x3C, 0x3C, 0x42, 0x42, 0x62, 0x62, 0x5A, 0x5A, 0x46, 0x46, 0x42, 0x42, 0x3C, 0x3C, 0x00, 0x00 },
            new byte[] { 0x10, 0x10, 0x18, 0x18, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x38, 0x38, 0x00, 0x00 },
            new byte[] { 0x3C, 0x3C, 0x42, 0x42, 0x40, 0x40, 0x38, 0x38, 0x04, 0x04, 0x02, 0x02, 0x7E, 0x7E, 0x00, 0x00 },
            new byte[] { 0x7E, 0x7E, 0x40, 0x40, 0x20, 0x20, 0x38, 0x38, 0x40, 0x40, 0x42, 0x42, 0x3C, 0x3C, 0x00, 0x00 },
            new byte[] { 0x20, 0x20, 0x30, 0x30, 0x28, 0x28, 0x24, 0x24, 0x7E, 0x7E, 0x20, 0x20, 0x20, 0x20, 0x00, 0x00 },
            new byte[] { 0x7E, 0x7E, 0x02, 0x02, 0x3E, 0x3E, 0x40, 0x40, 0x40, 0x40, 0x42, 0x42, 0x3C, 0x3C, 0x00, 0x00 },
            new byte[] { 0x78, 0x78, 0x04, 0x04, 0x02, 0x02, 0x3E, 0x3E, 0x42, 0x42, 0x42, 0x42, 0x3C, 0x3C, 0x00, 0x00 },
            new byte[] { 0x7E, 0x7E, 0x40, 0x40, 0x20, 0x20, 0x10, 0x10, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x00, 0x00 },
            new byte[] { 0x3C, 0x3C, 0x42, 0x42, 0x42, 0x42, 0x3C, 0x3C, 0x42, 0x42, 0x42, 0x42, 0x3C, 0x3C, 0x00, 0x00 },
            new byte[] { 0x3C, 0x3C, 0x42, 0x42, 0x42, 0x42, 0x7C, 0x7C, 0x40, 0x40, 0x20, 0x20, 0x1E, 0x1E, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x08, 0x08, 0x00, 0x00, 0x08, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x10, 0x10, 0x00, 0x00, 0x10, 0x10, 0x10, 0x10, 0x08, 0x00, 0x00, 0x00 },
            new byte[] { 0x20, 0x20, 0x10, 0x10, 0x08, 0x08, 0x04, 0x04, 0x08, 0x08, 0x10, 0x10, 0x20, 0x20, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x7E, 0x7E, 0x00, 0x00, 0x7E, 0x7E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x04, 0x04, 0x08, 0x08, 0x10, 0x10, 0x20, 0x20, 0x10, 0x10, 0x08, 0x08, 0x04, 0x04, 0x00, 0x00 },
            new byte[] { 0x04, 0x04, 0x02, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x3C, 0x3C, 0x42, 0x42, 0x52, 0x52, 0x6A, 0x6A, 0x32, 0x32, 0x02, 0x02, 0x7C, 0x7C, 0x00, 0x00 },
            new byte[] { 0x18, 0x18, 0x24, 0x24, 0x42, 0x42, 0x42, 0x42, 0x7E, 0x7E, 0x42, 0x42, 0x42, 0x42, 0x00, 0x00 },
            new byte[] { 0x3E, 0x3E, 0x42, 0x42, 0x42, 0x42, 0x3E, 0x3E, 0x42, 0x42, 0x42, 0x42, 0x3E, 0x3E, 0x00, 0x00 },
            new byte[] { 0x3C, 0x3C, 0x42, 0x42, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x42, 0x42, 0x3C, 0x3C, 0x00, 0x00 },
            new byte[] { 0x3E, 0x3E, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x3E, 0x3E, 0x00, 0x00 },
            new byte[] { 0x7E, 0x7E, 0x02, 0x02, 0x02, 0x02, 0x3E, 0x3E, 0x02, 0x02, 0x02, 0x02, 0x7E, 0x7E, 0x00, 0x00 },
            new byte[] { 0x7E, 0x7E, 0x02, 0x02, 0x02, 0x02, 0x3E, 0x3E, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x00, 0x00 },
            new byte[] { 0x7C, 0x7C, 0x02, 0x02, 0x02, 0x02, 0x02, 0x72, 0x72, 0x42, 0x42, 0x42, 0x7C, 0x7C, 0x00, 0x00 },
            new byte[] { 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x7E, 0x7E, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x00, 0x00 },
            new byte[] { 0x1C, 0x1C, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x1C, 0x1C, 0x00, 0x00 },
            new byte[] { 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x42, 0x42, 0x3C, 0x3C, 0x00, 0x00 },
            new byte[] { 0x42, 0x42, 0x22, 0x22, 0x12, 0x12, 0x0E, 0x0E, 0x12, 0x12, 0x22, 0x22, 0x42, 0x42, 0x00, 0x00 },
            new byte[] { 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x7E, 0x7E, 0x00, 0x00 },
            new byte[] { 0x42, 0x42, 0x66, 0x66, 0x5A, 0x5A, 0x5A, 0x5A, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x00, 0x00 },
            new byte[] { 0x42, 0x42, 0x46, 0x46, 0x4A, 0x4A, 0x5A, 0x5A, 0x52, 0x52, 0x62, 0x62, 0x42, 0x42, 0x00, 0x00 },
            new byte[] { 0x3C, 0x3C, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x3C, 0x3C, 0x00, 0x00 },
            new byte[] { 0x3E, 0x3E, 0x42, 0x42, 0x42, 0x42, 0x3E, 0x3E, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x00, 0x00 },
            new byte[] { 0x3C, 0x3C, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x52, 0x52, 0x22, 0x22, 0x5C, 0x5C, 0x00, 0x00 },
            new byte[] { 0x3E, 0x3E, 0x42, 0x42, 0x42, 0x42, 0x3E, 0x3E, 0x12, 0x12, 0x22, 0x22, 0x42, 0x42, 0x00, 0x00 },
            new byte[] { 0x3C, 0x3C, 0x42, 0x42, 0x02, 0x02, 0x3C, 0x3C, 0x40, 0x40, 0x42, 0x42, 0x3C, 0x3C, 0x00, 0x00 },
            new byte[] { 0x3E, 0x3E, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x00, 0x00 },
            new byte[] { 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x3C, 0x3C, 0x00, 0x00 },
            new byte[] { 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x24, 0x24, 0x18, 0x18, 0x00, 0x00 },
            new byte[] { 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x5A, 0x5A, 0x5A, 0x5A, 0x66, 0x66, 0x42, 0x42, 0x00, 0x00 },
            new byte[] { 0x42, 0x42, 0x42, 0x42, 0x24, 0x24, 0x18, 0x18, 0x24, 0x24, 0x42, 0x42, 0x42, 0x42, 0x00, 0x00 },
            new byte[] { 0x22, 0x22, 0x22, 0x22, 0x14, 0x14, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x00, 0x00 },
            new byte[] { 0x7E, 0x7E, 0x40, 0x40, 0x20, 0x20, 0x18, 0x18, 0x04, 0x04, 0x02, 0x02, 0x7E, 0x7E, 0x00, 0x00 },
            new byte[] { 0x7E, 0x7E, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x7E, 0x7E, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x02, 0x02, 0x04, 0x04, 0x08, 0x08, 0x10, 0x10, 0x20, 0x20, 0x40, 0x40, 0x00, 0x00 },
            new byte[] { 0x7E, 0x7E, 0x60, 0x60, 0x60, 0x60, 0x60, 0x60, 0x60, 0x60, 0x60, 0x60, 0x7E, 0x7E, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x08, 0x08, 0x14, 0x14, 0x22, 0x22, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7E, 0x7E, 0x00, 0x00 },
            new byte[] { 0x04, 0x04, 0x08, 0x08, 0x10, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x3C, 0x3C, 0x40, 0x40, 0x7C, 0x7C, 0x42, 0x42, 0x7C, 0x7C, 0x00, 0x00 },
            new byte[] { 0x02, 0x02, 0x02, 0x02, 0x3E, 0x3E, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x3E, 0x3E, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x7C, 0x7C, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x7C, 0x7C, 0x00, 0x00 },
            new byte[] { 0x40, 0x40, 0x40, 0x40, 0x7C, 0x7C, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x7C, 0x7C, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x3C, 0x3C, 0x42, 0x42, 0x7E, 0x7E, 0x02, 0x02, 0x7C, 0x7C, 0x00, 0x00 },
            new byte[] { 0x38, 0x38, 0x44, 0x44, 0x04, 0x04, 0x3E, 0x3E, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x3C, 0x3C, 0x42, 0x42, 0x42, 0x42, 0x7C, 0x7C, 0x40, 0x40, 0x3C, 0x3C },
            new byte[] { 0x02, 0x02, 0x02, 0x02, 0x3E, 0x3E, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x00, 0x00 },
            new byte[] { 0x08, 0x08, 0x00, 0x00, 0x0C, 0x0C, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x1C, 0x1C, 0x00, 0x00 },
            new byte[] { 0x20, 0x20, 0x00, 0x00, 0x3C, 0x3C, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x22, 0x22, 0x1C, 0x1C },
            new byte[] { 0x02, 0x02, 0x02, 0x02, 0x42, 0x42, 0x22, 0x22, 0x1E, 0x1E, 0x22, 0x22, 0x42, 0x42, 0x00, 0x00 },
            new byte[] { 0x0C, 0x0C, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x1C, 0x1C, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x66, 0x66, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x42, 0x42, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x3E, 0x3E, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x3C, 0x3C, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x3C, 0x3C, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x3E, 0x3E, 0x42, 0x42, 0x42, 0x42, 0x3E, 0x3E, 0x02, 0x02, 0x02, 0x02 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x7C, 0x7C, 0x42, 0x42, 0x42, 0x42, 0x7C, 0x7C, 0x40, 0x40, 0x40, 0x40 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x7A, 0x7A, 0x06, 0x06, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x7C, 0x7C, 0x02, 0x02, 0x3C, 0x3C, 0x40, 0x40, 0x3E, 0x3E, 0x00, 0x00 },
            new byte[] { 0x08, 0x08, 0x08, 0x08, 0x3E, 0x3E, 0x08, 0x08, 0x08, 0x08, 0x48, 0x48, 0x30, 0x30, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x62, 0x62, 0x5C, 0x5C, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x24, 0x24, 0x18, 0x18, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x42, 0x42, 0x42, 0x42, 0x5A, 0x5A, 0x5A, 0x5A, 0x66, 0x66, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x42, 0x42, 0x24, 0x24, 0x18, 0x18, 0x24, 0x24, 0x42, 0x42, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x7C, 0x7C, 0x40, 0x40, 0x3C, 0x3C },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x7E, 0x7E, 0x20, 0x20, 0x18, 0x18, 0x04, 0x04, 0x7E, 0x7E, 0x00, 0x00 },
            new byte[] { 0x70, 0x70, 0x18, 0x18, 0x18, 0x18, 0x0E, 0x0E, 0x18, 0x18, 0x18, 0x18, 0x70, 0x70, 0x00, 0x00 },
            new byte[] { 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x00, 0x00 },
            new byte[] { 0x0E, 0x0E, 0x18, 0x18, 0x18, 0x18, 0x70, 0x70, 0x18, 0x18, 0x18, 0x18, 0x0E, 0x0E, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x24, 0x24, 0x2A, 0x2A, 0x12, 0x12, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
        };

        /// <inheritdoc/>
        public override byte Width { get => 8; }

        /// <inheritdoc/>
        public override byte Height { get => 16; }

        /// <inheritdoc/>
        public override byte[] this[char character]
        {
            get
            {
                var index = (byte)character;
                if ((index < 32) || (index > 127))
                {
                    return _fontTable[32];
                }
                else
                {
                    return _fontTable[index - 32];
                }
            }
        }
    }
}
