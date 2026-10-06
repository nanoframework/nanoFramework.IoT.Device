// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

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
