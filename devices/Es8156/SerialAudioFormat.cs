// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Es8156
{
    /// <summary>
    /// Serial audio data port format expected by the ES8156 on the I2S bus.
    /// </summary>
    /// <remarks>
    /// Values correspond to the SP_PROTOCAL field (bits 1:0) of register 0x11.
    /// </remarks>
    public enum SerialAudioFormat : byte
    {
        /// <summary>
        /// Standard I2S format.
        /// </summary>
        I2s = 0x00,

        /// <summary>
        /// Left-justified format.
        /// </summary>
        LeftJustified = 0x01,

        /// <summary>
        /// DSP / PCM format.
        /// </summary>
        DspPcm = 0x03,
    }
}
