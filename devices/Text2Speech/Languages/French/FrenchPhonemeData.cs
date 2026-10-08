// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Text2Speech
{
    /// <summary>
    /// Contains the acoustic models used by the French frontend.
    /// </summary>
    internal static class FrenchPhonemeData
    {
        public static readonly TtsPhoneme A = P(TtsPhonemeType.Voiced, 657, 1310, 2440, 6, 5, 2, 45, 125);
        public static readonly TtsPhoneme EClose = P(TtsPhonemeType.Voiced, 367, 2039, 2550, 5, 6, 2, 47, 112);
        public static readonly TtsPhoneme EOpen = P(TtsPhonemeType.Voiced, 503, 1805, 2820, 6, 5, 2, 46, 118);
        public static readonly TtsPhoneme I = P(TtsPhonemeType.Voiced, 274, 2253, 3010, 5, 7, 2, 48, 118);
        public static readonly TtsPhoneme OClose = P(TtsPhonemeType.Voiced, 403, 875, 2400, 7, 4, 2, 44, 125);
        public static readonly TtsPhoneme OOpen = P(TtsPhonemeType.Voiced, 518, 1015, 2410, 7, 4, 2, 44, 122);
        public static readonly TtsPhoneme U = P(TtsPhonemeType.Voiced, 314, 788, 2200, 6, 5, 2, 43, 122);
        public static readonly TtsPhoneme YRounded = P(TtsPhonemeType.Voiced, 290, 1881, 2400, 6, 5, 2, 46, 118);
        public static readonly TtsPhoneme Eu = P(TtsPhonemeType.Voiced, 372, 1499, 2300, 6, 5, 2, 45, 120);
        public static readonly TtsPhoneme Oe = P(TtsPhonemeType.Voiced, 566, 1498, 2250, 6, 5, 2, 44, 120);
        public static readonly TtsPhoneme Schwa = P(TtsPhonemeType.Voiced, 500, 1500, 2400, 5, 4, 2, 44, 82);

        // The current renderer has no anti-formants. Reduced upper-formant amplitudes provide
        // a lightweight approximation of French nasal vowels without another renderer path.
        public static readonly TtsPhoneme NasalAn = P(TtsPhonemeType.Voiced, 650, 1150, 2300, 7, 3, 1, 43, 145);
        public static readonly TtsPhoneme NasalIn = P(TtsPhonemeType.Voiced, 450, 1600, 2350, 7, 3, 1, 44, 142);
        public static readonly TtsPhoneme NasalOn = P(TtsPhonemeType.Voiced, 500, 900, 2200, 7, 3, 1, 43, 145);
        public static readonly TtsPhoneme NasalUn = P(TtsPhonemeType.Voiced, 450, 1400, 2200, 7, 3, 1, 44, 142);

        public static readonly TtsPhoneme PConsonant = P(TtsPhonemeType.Stop, 300, 700, 2400, 0, 5, 0, 45, 68);
        public static readonly TtsPhoneme B = P(TtsPhonemeType.Stop, 350, 700, 2400, 0, 4, 0, 45, 80, true);
        public static readonly TtsPhoneme T = P(TtsPhonemeType.Stop, 300, 700, 2800, 2, 6, 0, 45, 62);
        public static readonly TtsPhoneme D = P(TtsPhonemeType.Stop, 300, 800, 2800, 2, 5, 0, 45, 72, true);
        public static readonly TtsPhoneme K = P(TtsPhonemeType.Stop, 300, 800, 2500, 1, 6, 0, 45, 68);
        public static readonly TtsPhoneme G = P(TtsPhonemeType.Stop, 260, 700, 2800, 1, 4, 0, 45, 75, true);
        public static readonly TtsPhoneme F = P(TtsPhonemeType.Fricative, 0, 0, 0, 0, 4, 0, 0, 82);
        public static readonly TtsPhoneme V = P(TtsPhonemeType.VoicedFricative, 300, 700, 0, 4, 5, 0, 44, 78);
        public static readonly TtsPhoneme S = P(TtsPhonemeType.Fricative, 0, 0, 0, 2, 8, 0, 0, 88);
        public static readonly TtsPhoneme Z = P(TtsPhonemeType.VoicedFricative, 300, 1700, 0, 3, 5, 2, 44, 82);
        public static readonly TtsPhoneme Sh = P(TtsPhonemeType.Fricative, 0, 0, 0, 1, 7, 0, 0, 92);
        public static readonly TtsPhoneme Zh = P(TtsPhonemeType.VoicedFricative, 300, 1800, 2600, 3, 6, 1, 44, 92);
        public static readonly TtsPhoneme M = P(TtsPhonemeType.Voiced, 280, 900, 2200, 7, 3, 1, 43, 105);
        public static readonly TtsPhoneme N = P(TtsPhonemeType.Voiced, 280, 1700, 2600, 7, 3, 1, 44, 95);
        public static readonly TtsPhoneme Ny = P(TtsPhonemeType.Voiced, 280, 1850, 2600, 7, 3, 1, 45, 95);
        public static readonly TtsPhoneme Ng = P(TtsPhonemeType.Voiced, 280, 800, 2200, 7, 3, 1, 43, 95);
        public static readonly TtsPhoneme L = P(TtsPhonemeType.Voiced, 360, 1030, 2880, 6, 4, 2, 44, 92);
        public static readonly TtsPhoneme R = P(TtsPhonemeType.VoicedFricative, 400, 1100, 2100, 4, 4, 1, 43, 88);
        public static readonly TtsPhoneme J = P(TtsPhonemeType.Voiced, 280, 2200, 3000, 5, 5, 2, 47, 62);
        public static readonly TtsPhoneme W = P(TtsPhonemeType.Voiced, 300, 700, 2200, 6, 5, 2, 43, 62);
        public static readonly TtsPhoneme H = P(TtsPhonemeType.Voiced, 300, 1700, 2400, 5, 5, 2, 46, 62);

        public static readonly TtsPhoneme Space = P(TtsPhonemeType.Silence, 0, 0, 0, 0, 0, 0, 0, 25);
        public static readonly TtsPhoneme Comma = P(TtsPhonemeType.Silence, 0, 0, 0, 0, 0, 0, 0, 220);
        public static readonly TtsPhoneme Clause = P(TtsPhonemeType.Silence, 0, 0, 0, 0, 0, 0, 0, 320);
        public static readonly TtsPhoneme Stop = P(TtsPhonemeType.Silence, 0, 0, 0, 0, 0, 0, 0, 400);
        public static readonly TtsPhoneme Question = P(TtsPhonemeType.Silence, 0, 0, 0, 0, 0, 0, 0, 500);

        private static TtsPhoneme P(
            TtsPhonemeType type,
            int f1,
            int f2,
            int f3,
            int a1,
            int a2,
            int a3,
            int pitch,
            int duration,
            bool voiced = false)
        {
            return new TtsPhoneme(type, f1, f2, f3, a1, a2, a3, pitch, duration, voiced, 0, 0, 0);
        }
    }
}
