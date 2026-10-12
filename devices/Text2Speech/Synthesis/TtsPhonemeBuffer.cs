// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
            return TryAdd(phoneme, pitchOffset, 100);
        }

        /// <summary>
        /// Appends a phoneme with relative pitch and local duration adjustments.
        /// </summary>
        /// <param name="phoneme">The immutable acoustic phoneme definition.</param>
        /// <param name="pitchOffset">The relative pitch adjustment in semitones.</param>
        /// <param name="durationPercent">
        /// The local duration percentage, from 50 through 200.
        /// </param>
        /// <returns>
        /// <see langword="true" /> when appended; otherwise, <see langword="false" /> when full.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="phoneme" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="durationPercent" /> is outside the supported range.
        /// </exception>
        public bool TryAdd(TtsPhoneme phoneme, int pitchOffset, int durationPercent)
        {
            if (phoneme == null)
            {
                throw new ArgumentNullException();
            }

            if (durationPercent < 50 || durationPercent > 200)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (_count >= _sequence.Length)
            {
                return false;
            }

            _sequence[_count].Phoneme = phoneme;
            _sequence[_count].PitchOffset = pitchOffset;
            _sequence[_count].DurationPercent = durationPercent;
            _count++;
            return true;
        }

        internal int GetSampleCount(VoiceOptions options)
        {
            int count = 0;
            for (int i = 0; i < _count; i++)
            {
                TtsPhoneme phoneme = _sequence[i].Phoneme;
                int durationPercent = _sequence[i].DurationPercent;
                if (phoneme.Type != TtsPhonemeType.Stop)
                {
                    count += options.DurationToSamples(phoneme.Duration, durationPercent);
                }
                else if (phoneme.Voiced)
                {
                    count += options.DurationToSamples(18, durationPercent)
                        + options.DurationToSamples(20, durationPercent)
                        + options.DurationToSamples(18, durationPercent);
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

                    count += options.DurationToSamples(20, durationPercent)
                        + options.DurationToSamples(burst, durationPercent);
                }
            }

            return count;
        }
    }
}
