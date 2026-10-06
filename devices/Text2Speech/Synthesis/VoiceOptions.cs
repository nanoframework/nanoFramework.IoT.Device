// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Text2Speech
{
    /// <summary>
    /// Controls the character and timing of a synthesized Text2Speech voice.
    /// </summary>
    public sealed class VoiceOptions
    {
        private const int ScaleShift = 8;
        private const int ScaleOne = 1 << ScaleShift;

        private readonly int _formantScale;
        private readonly int _noiseScale;

        /// <summary>
        /// Initializes a new instance of the <see cref="VoiceOptions" /> class.
        /// </summary>
        /// <param name="pitchShift">The base pitch shift in semitones, from -12 through 12.</param>
        /// <param name="speedPercent">The speaking speed percentage, from 60 through 160.</param>
        /// <param name="formantScalePercent">The vocal-tract formant scale, from 80 through 120.</param>
        /// <param name="intonationPercent">The sentence pitch-contour strength, from 0 through 200.</param>
        /// <param name="fricativeNoisePercent">The fricative and stop noise level, from 0 through 150.</param>
        /// <param name="transitionSmoothingPercent">The voiced-phoneme smoothing amount, from 0 through 200.</param>
        /// <exception cref="ArgumentOutOfRangeException">A control is outside its documented range.</exception>
        public VoiceOptions(
            int pitchShift = 0,
            int speedPercent = 100,
            int formantScalePercent = 100,
            int intonationPercent = 100,
            int fricativeNoisePercent = 100,
            int transitionSmoothingPercent = 100)
        {
            ValidateRange(pitchShift, -12, 12, "pitchShift");
            ValidateRange(speedPercent, 60, 160, "speedPercent");
            ValidateRange(formantScalePercent, 80, 120, "formantScalePercent");
            ValidateRange(intonationPercent, 0, 200, "intonationPercent");
            ValidateRange(fricativeNoisePercent, 0, 150, "fricativeNoisePercent");
            ValidateRange(
                transitionSmoothingPercent,
                0,
                200,
                "transitionSmoothingPercent");

            PitchShift = pitchShift;
            SpeedPercent = speedPercent;
            FormantScalePercent = formantScalePercent;
            IntonationPercent = intonationPercent;
            FricativeNoisePercent = fricativeNoisePercent;
            TransitionSmoothingPercent = transitionSmoothingPercent;
            _formantScale = ((formantScalePercent * ScaleOne) + 50) / 100;
            _noiseScale = ((fricativeNoisePercent * ScaleOne) + 50) / 100;
        }

        /// <summary>
        /// Gets the base pitch shift in semitones.
        /// </summary>
        public int PitchShift { get; }

        /// <summary>
        /// Gets the speaking speed percentage. Values above 100 speak faster.
        /// </summary>
        public int SpeedPercent { get; }

        /// <summary>
        /// Gets the formant-frequency scale percentage.
        /// </summary>
        public int FormantScalePercent { get; }

        /// <summary>
        /// Gets the sentence pitch-contour strength percentage.
        /// </summary>
        public int IntonationPercent { get; }

        /// <summary>
        /// Gets the fricative and stop noise level percentage.
        /// </summary>
        public int FricativeNoisePercent { get; }

        /// <summary>
        /// Gets the voiced-phoneme transition smoothing percentage.
        /// </summary>
        public int TransitionSmoothingPercent { get; }

        internal int DurationToSamples(int milliseconds)
        {
            return DurationToSamples(milliseconds, 100);
        }

        internal int DurationToSamples(int milliseconds, int durationPercent)
        {
            if (milliseconds <= 0)
            {
                return 0;
            }

            int divisor = 1000 * SpeedPercent;
            int samples = ((milliseconds * TtsSynthesizer.SampleRate * durationPercent)
                + (divisor / 2))
                / divisor;
            return samples > 0 ? samples : 1;
        }

        internal int ScaleFormant(int hertz)
        {
            int scaled = (hertz * _formantScale) >> ScaleShift;
            return scaled > 4000 ? 4000 : scaled;
        }

        internal int ScaleIntonation(int semitones)
        {
            int scaled = semitones * IntonationPercent;
            return scaled >= 0 ? (scaled + 50) / 100 : (scaled - 50) / 100;
        }

        internal int ScaleNoise(int sample)
        {
            int scaled = sample * _noiseScale;
            return scaled >= 0 ? scaled >> ScaleShift : -((-scaled) >> ScaleShift);
        }

        internal int TransitionSamples(int normalSamples)
        {
            return (normalSamples * TransitionSmoothingPercent) / 100;
        }

        private static void ValidateRange(int value, int minimum, int maximum, string parameterName)
        {
            if (value < minimum || value > maximum)
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }
}
