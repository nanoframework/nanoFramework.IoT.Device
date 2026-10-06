// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Text2Speech
{
    /// <summary>
    /// Stores a bounded sequence of phonemes prepared by a language frontend.
    /// </summary>
    public sealed class TtsPhonemeBuffer
    {
        /// <summary>
        /// The fixed number of phonemes available to one synthesis round.
        /// </summary>
        public const int MaximumPhonemes = 128;

        private readonly PhonemeEntry[] _sequence = new PhonemeEntry[MaximumPhonemes];
        private int _count;

        /// <summary>
        /// Initializes a new instance of the <see cref="TtsPhonemeBuffer" /> class.
        /// </summary>
        public TtsPhonemeBuffer()
        {
            for (int i = 0; i < _sequence.Length; i++)
            {
                _sequence[i] = new PhonemeEntry();
            }
        }

        /// <summary>
        /// Gets the number of prepared phonemes.
        /// </summary>
        /// <value>The populated sequence length.</value>
        public int Count => _count;

        /// <summary>
        /// Gets the fixed phoneme capacity.
        /// </summary>
        /// <value><see cref="MaximumPhonemes" />.</value>
        public int Capacity => _sequence.Length;

        internal PhonemeEntry this[int index] => _sequence[index];

        /// <summary>
        /// Removes all prepared phonemes without reallocating the workspace.
        /// </summary>
        public void Clear()
        {
            _count = 0;
        }

        /// <summary>
        /// Appends a phoneme and its relative sentence-level pitch adjustment.
        /// </summary>
        /// <param name="phoneme">The immutable acoustic phoneme definition.</param>
        /// <param name="pitchOffset">The relative pitch adjustment in semitones.</param>
        /// <returns>
        /// <see langword="true" /> when appended; otherwise, <see langword="false" /> when full.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="phoneme" /> is <see langword="null" />.</exception>
        public bool TryAdd(TtsPhoneme phoneme, int pitchOffset)
        {
            if (phoneme == null)
            {
                throw new ArgumentNullException();
            }

            if (_count >= _sequence.Length)
            {
                return false;
            }

            _sequence[_count].Phoneme = phoneme;
            _sequence[_count].PitchOffset = pitchOffset;
            _count++;
            return true;
        }

        internal int GetSampleCount(VoiceOptions options)
        {
            int count = 0;
            for (int i = 0; i < _count; i++)
            {
                TtsPhoneme phoneme = _sequence[i].Phoneme;
                if (phoneme.Type != TtsPhonemeType.Stop)
                {
                    count += options.DurationToSamples(phoneme.Duration);
                }
                else if (phoneme.Voiced)
                {
                    count += options.DurationToSamples(18)
                        + options.DurationToSamples(20)
                        + options.DurationToSamples(18);
                }
                else
                {
                    int burst = phoneme.Duration - 20;
                    if (burst < 20)
                    {
                        burst = 20;
                    }
                    else if (burst > 45)
                    {
                        burst = 45;
                    }

                    count += options.DurationToSamples(20) + options.DurationToSamples(burst);
                }
            }

            return count;
        }
    }
}
