// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Text2Speech
{
    /// <summary>
    /// Identifies the renderer path used for a phoneme.
    /// </summary>
    public enum TtsPhonemeType
    {
        /// <summary>
        /// Uses periodic excitation for vowels and other voiced sounds.
        /// </summary>
        Voiced,

        /// <summary>
        /// Uses noise excitation for unvoiced fricatives.
        /// </summary>
        Fricative,

        /// <summary>
        /// Uses closure and burst phases for stop consonants.
        /// </summary>
        Stop,

        /// <summary>
        /// Produces a timed silent interval.
        /// </summary>
        Silence,

        /// <summary>
        /// Combines periodic and noise excitation for voiced fricatives.
        /// </summary>
        VoicedFricative,
    }
}
