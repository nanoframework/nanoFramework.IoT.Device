// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Text2Speech
{
    /// <summary>
    /// Stores one parsed phoneme and its sentence-level pitch adjustment.
    /// </summary>
    internal sealed class PhonemeEntry
    {
        /// <summary>
        /// Gets or sets the parsed phoneme definition.
        /// </summary>
        /// <value>The phoneme rendered at this sequence position.</value>
        public TtsPhoneme Phoneme { get; set; }

        /// <summary>
        /// Gets or sets the relative pitch offset in semitones.
        /// </summary>
        /// <value>The sentence-contour pitch adjustment.</value>
        public int PitchOffset { get; set; }
    }
}
