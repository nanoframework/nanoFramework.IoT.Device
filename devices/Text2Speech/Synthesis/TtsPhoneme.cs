// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Text2Speech
{
    /// <summary>
    /// Defines the formants, amplitudes, pitch, duration, and optional glide of one phoneme.
    /// </summary>
    public sealed class TtsPhoneme
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TtsPhoneme" /> class.
        /// </summary>
        /// <param name="type">The renderer excitation and timing category.</param>
        /// <param name="f1">The first formant frequency in hertz.</param>
        /// <param name="f2">The second formant frequency in hertz.</param>
        /// <param name="f3">The third formant frequency in hertz.</param>
        /// <param name="a1">The first formant amplitude.</param>
        /// <param name="a2">The second formant amplitude.</param>
        /// <param name="a3">The third formant amplitude.</param>
        /// <param name="pitchMidi">The base pitch as a MIDI note number.</param>
        /// <param name="duration">The nominal duration in milliseconds.</param>
        /// <param name="voiced">Whether a stop consonant includes voiced excitation.</param>
        /// <param name="glide1">The optional first-formant glide target in hertz.</param>
        /// <param name="glide2">The optional second-formant glide target in hertz.</param>
        /// <param name="glide3">The optional third-formant glide target in hertz.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// A category, frequency, amplitude, pitch, duration, or glide is outside the renderer's
        /// supported range.
        /// </exception>
        public TtsPhoneme(
            TtsPhonemeType type,
            int f1,
            int f2,
            int f3,
            int a1,
            int a2,
            int a3,
            int pitchMidi,
            int duration,
            bool voiced,
            int glide1,
            int glide2,
            int glide3)
        {
            if (type < TtsPhonemeType.Voiced || type > TtsPhonemeType.VoicedFricative
                || f1 < 0 || f1 > 4000
                || f2 < 0 || f2 > 4000
                || f3 < 0 || f3 > 4000
                || a1 < 0 || a1 > 127
                || a2 < 0 || a2 > 127
                || a3 < 0 || a3 > 127
                || pitchMidi < 0 || pitchMidi > 127
                || duration <= 0 || duration > 2000
                || glide1 < 0 || glide1 > 4000
                || glide2 < 0 || glide2 > 4000
                || glide3 < 0 || glide3 > 4000)
            {
                throw new ArgumentOutOfRangeException();
            }

            Type = type;
            F1 = f1;
            F2 = f2;
            F3 = f3;
            A1 = a1;
            A2 = a2;
            A3 = a3;
            PitchMidi = pitchMidi;
            Duration = duration;
            Voiced = voiced;
            Glide1 = glide1;
            Glide2 = glide2;
            Glide3 = glide3;
        }

        /// <summary>
        /// Gets the renderer category.
        /// </summary>
        /// <value>The excitation and timing category.</value>
        public TtsPhonemeType Type { get; }

        /// <summary>
        /// Gets the first formant frequency.
        /// </summary>
        /// <value>The first formant frequency in hertz.</value>
        public int F1 { get; }

        /// <summary>
        /// Gets the second formant frequency.
        /// </summary>
        /// <value>The second formant frequency in hertz.</value>
        public int F2 { get; }

        /// <summary>
        /// Gets the third formant frequency.
        /// </summary>
        /// <value>The third formant frequency in hertz.</value>
        public int F3 { get; }

        /// <summary>
        /// Gets the first formant amplitude.
        /// </summary>
        /// <value>The first formant amplitude coefficient.</value>
        public int A1 { get; }

        /// <summary>
        /// Gets the second formant amplitude.
        /// </summary>
        /// <value>The second formant amplitude coefficient.</value>
        public int A2 { get; }

        /// <summary>
        /// Gets the third formant amplitude.
        /// </summary>
        /// <value>The third formant amplitude coefficient.</value>
        public int A3 { get; }

        /// <summary>
        /// Gets the base pitch.
        /// </summary>
        /// <value>The MIDI note number used for periodic excitation.</value>
        public int PitchMidi { get; }

        /// <summary>
        /// Gets the nominal duration.
        /// </summary>
        /// <value>The duration in milliseconds before speed scaling.</value>
        public int Duration { get; }

        /// <summary>
        /// Gets a value indicating whether the phoneme uses voiced stop excitation.
        /// </summary>
        /// <value><see langword="true" /> for a voiced stop; otherwise, <see langword="false" />.</value>
        public bool Voiced { get; }

        /// <summary>
        /// Gets the first-formant glide target.
        /// </summary>
        /// <value>The target frequency in hertz, or zero when no glide is used.</value>
        public int Glide1 { get; }

        /// <summary>
        /// Gets the second-formant glide target.
        /// </summary>
        /// <value>The target frequency in hertz, or zero when no glide is used.</value>
        public int Glide2 { get; }

        /// <summary>
        /// Gets the third-formant glide target.
        /// </summary>
        /// <value>The target frequency in hertz, or zero when no glide is used.</value>
        public int Glide3 { get; }
    }
}
