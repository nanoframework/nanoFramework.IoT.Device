// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Text2Speech.Tests
{
    /// <summary>
    /// Provides a minimal language definition for frontend extension tests.
    /// </summary>
    internal sealed class TestLanguage : ITtsLanguage
    {
        internal static readonly TtsPhoneme Tone = new TtsPhoneme(
            TtsPhonemeType.Voiced,
            730,
            1090,
            2440,
            6,
            5,
            2,
            45,
            50,
            false,
            0,
            0,
            0);

        internal static readonly TtsPhoneme FirstFormantGlide = new TtsPhoneme(
            TtsPhonemeType.Voiced,
            730,
            1090,
            2440,
            6,
            5,
            2,
            45,
            50,
            false,
            900,
            0,
            0);

        internal static readonly TtsPhoneme FirstFormantGlideWithExplicitFallbacks = new TtsPhoneme(
            TtsPhonemeType.Voiced,
            730,
            1090,
            2440,
            6,
            5,
            2,
            45,
            50,
            false,
            900,
            1090,
            2440);

        internal static readonly TtsPhoneme SecondFormantGlide = new TtsPhoneme(
            TtsPhonemeType.Voiced,
            730,
            1090,
            2440,
            6,
            5,
            2,
            45,
            50,
            false,
            0,
            1500,
            0);

        internal static readonly TtsPhoneme NyquistFormants = new TtsPhoneme(
            TtsPhonemeType.Voiced,
            4000,
            4000,
            4000,
            6,
            5,
            2,
            45,
            50,
            false,
            0,
            0,
            0);

        internal static readonly TtsPhoneme Fricative = new TtsPhoneme(
            TtsPhonemeType.Fricative,
            0,
            0,
            0,
            2,
            8,
            0,
            0,
            50,
            false,
            0,
            0,
            0);

        internal static readonly TtsPhoneme VoicedFricative = new TtsPhoneme(
            TtsPhonemeType.VoicedFricative,
            400,
            1100,
            2100,
            4,
            4,
            1,
            43,
            50,
            false,
            0,
            0,
            0);

        internal static readonly TtsPhoneme UnvoicedStop = new TtsPhoneme(
            TtsPhonemeType.Stop,
            300,
            700,
            2400,
            0,
            5,
            0,
            45,
            50,
            false,
            0,
            0,
            0);

        internal static readonly TtsPhoneme VoicedStop = new TtsPhoneme(
            TtsPhonemeType.Stop,
            350,
            700,
            2400,
            0,
            4,
            0,
            45,
            50,
            true,
            0,
            0,
            0);

        /// <summary>
        /// Gets the test language name.
        /// </summary>
        /// <value><c>Test</c>.</value>
        public string Name => "Test";

        /// <summary>
        /// Gets the deliberately small test input limit.
        /// </summary>
        /// <value>Four characters.</value>
        public int MaximumTextLength => 4;

        /// <summary>
        /// Creates an independent test frontend.
        /// </summary>
        /// <returns>A new test frontend.</returns>
        public ITtsLanguageFrontend CreateFrontend()
        {
            return new TestLanguageFrontend();
        }
    }
}
