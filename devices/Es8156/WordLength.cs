// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Es8156
{
    /// <summary>
    /// Audio sample word length (bits per sample) on the I2S bus.
    /// </summary>
    /// <remarks>
    /// Values correspond to the SP_WL field (bits 6:4) of register 0x11 and are pre-shifted so they can be
    /// combined with a <see cref="SerialAudioFormat"/> value using a bitwise OR.
    /// </remarks>
    public enum WordLength : byte
    {
        /// <summary>
        /// 24 bits per sample.
        /// </summary>
        Bits24 = 0x00,

        /// <summary>
        /// 20 bits per sample.
        /// </summary>
        Bits20 = 0x10,

        /// <summary>
        /// 18 bits per sample.
        /// </summary>
        Bits18 = 0x20,

        /// <summary>
        /// 16 bits per sample.
        /// </summary>
        Bits16 = 0x30,

        /// <summary>
        /// 32 bits per sample.
        /// </summary>
        Bits32 = 0x40,
    }
}
