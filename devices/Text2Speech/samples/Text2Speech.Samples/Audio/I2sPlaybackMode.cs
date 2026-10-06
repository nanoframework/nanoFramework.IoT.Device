// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Text2Speech.Samples
{
    /// <summary>
    /// Selects the I2S playback sample rate and conversion quality.
    /// </summary>
    internal enum I2sPlaybackMode
    {
        /// <summary>
        /// Plays each source sample directly at 8 kHz for minimum managed conversion work.
        /// </summary>
        Fast8Khz,

        /// <summary>
        /// Inserts linear midpoints and plays at 16 kHz for smoother output.
        /// </summary>
        Interpolated16Khz,
    }
}
