// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

        /// <summary>
        /// Gets or sets the phoneme-specific duration percentage.
        /// </summary>
        /// <value>The local duration scale where 100 preserves the nominal duration.</value>
        public int DurationPercent { get; set; }
    }
}
