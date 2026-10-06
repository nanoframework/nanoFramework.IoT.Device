// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Text2Speech
{
    /// <summary>
    /// Contains immutable English spelling and acoustic phoneme definitions.
    /// </summary>
    internal static class PhonemeData
    {
        /// <summary>
        /// The default inter-word silence duration in milliseconds.
        /// </summary>
        public const int SpaceDuration = 80;

        /// <summary>
        /// The comma pause duration in milliseconds.
        /// </summary>
        public const int CommaDuration = 120;

        /// <summary>
        /// The sentence-ending pause duration in milliseconds.
        /// </summary>
        public const int StopDuration = 160;

        /// <summary>
        /// The fallback phoneme definition for each lowercase Latin letter.
        /// </summary>
        public static readonly TtsPhoneme[] Letters =
        {
            P(TtsPhonemeType.Voiced, 730, 1090, 2440, 6, 5, 2, 45, 130),
            P(TtsPhonemeType.Stop, 350, 700, 2400, 0, 4, 0, 45, 80, true),
            P(TtsPhonemeType.Stop, 300, 800, 2500, 1, 6, 0, 45, 72),
            P(TtsPhonemeType.Stop, 300, 800, 2800, 2, 5, 0, 45, 72, true),
            P(TtsPhonemeType.Voiced, 530, 1760, 2820, 6, 5, 2, 46, 118),
            P(TtsPhonemeType.Fricative, 0, 0, 0, 0, 4, 0, 0, 82),
            P(TtsPhonemeType.Stop, 260, 700, 2800, 1, 4, 0, 45, 75, true),
            P(TtsPhonemeType.Fricative, 0, 0, 0, 0, 2, 0, 0, 50),
            P(TtsPhonemeType.Voiced, 390, 1990, 2550, 5, 6, 2, 47, 108),
            P(TtsPhonemeType.Stop, 300, 800, 2800, 1, 5, 0, 45, 90, true),
            P(TtsPhonemeType.Stop, 300, 800, 2500, 1, 6, 0, 45, 68),
            P(TtsPhonemeType.Voiced, 360, 1030, 2880, 6, 4, 2, 44, 92),
            P(TtsPhonemeType.Voiced, 280, 900, 2200, 7, 3, 1, 43, 105),
            P(TtsPhonemeType.Voiced, 280, 1700, 2600, 7, 3, 1, 44, 95),
            P(TtsPhonemeType.Voiced, 570, 840, 2410, 7, 4, 2, 44, 128),
            P(TtsPhonemeType.Stop, 300, 700, 2400, 0, 5, 0, 45, 68),
            P(TtsPhonemeType.Stop, 300, 800, 2500, 1, 6, 0, 45, 68),
            P(TtsPhonemeType.Voiced, 490, 800, 1690, 6, 5, 2, 44, 100),
            P(TtsPhonemeType.Fricative, 0, 0, 0, 2, 8, 0, 0, 88),
            P(TtsPhonemeType.Stop, 300, 700, 2800, 2, 6, 0, 45, 62),
            P(TtsPhonemeType.Voiced, 440, 960, 2300, 7, 4, 1, 44, 112),
            P(TtsPhonemeType.VoicedFricative, 300, 700, 0, 4, 5, 0, 44, 78),
            P(TtsPhonemeType.Voiced, 300, 610, 2200, 6, 5, 2, 43, 80),
            P(TtsPhonemeType.Fricative, 0, 0, 0, 2, 8, 0, 0, 78),
            P(TtsPhonemeType.Voiced, 280, 2250, 3100, 5, 6, 2, 46, 72),
            P(TtsPhonemeType.VoicedFricative, 300, 1700, 0, 3, 5, 2, 44, 82),
        };

        /// <summary>
        /// The unvoiced sh fricative phoneme.
        /// </summary>
        public static readonly TtsPhoneme Sh = P(TtsPhonemeType.Fricative, 0, 0, 0, 1, 7, 0, 0, 92);

        /// <summary>
        /// The ch stop-affricate phoneme.
        /// </summary>
        public static readonly TtsPhoneme Ch = P(TtsPhonemeType.Stop, 300, 800, 2800, 1, 6, 0, 45, 95);

        /// <summary>
        /// The unvoiced th fricative phoneme.
        /// </summary>
        public static readonly TtsPhoneme Th = P(TtsPhonemeType.Fricative, 0, 0, 0, 0, 3, 0, 0, 88);

        /// <summary>
        /// The voiced ng nasal phoneme.
        /// </summary>
        public static readonly TtsPhoneme Ng = P(TtsPhonemeType.Voiced, 280, 800, 2200, 7, 3, 1, 43, 95);

        /// <summary>
        /// The long ee vowel phoneme.
        /// </summary>
        public static readonly TtsPhoneme Ee = P(TtsPhonemeType.Voiced, 270, 2290, 3010, 5, 7, 2, 48, 135);

        /// <summary>
        /// The short terminal y vowel phoneme.
        /// </summary>
        public static readonly TtsPhoneme Iy = P(TtsPhonemeType.Voiced, 270, 2290, 3010, 5, 7, 2, 47, 90);

        /// <summary>
        /// The long oo vowel phoneme.
        /// </summary>
        public static readonly TtsPhoneme Uu = P(TtsPhonemeType.Voiced, 300, 870, 2240, 7, 4, 1, 44, 132);

        /// <summary>
        /// The rhotic er vowel phoneme.
        /// </summary>
        public static readonly TtsPhoneme Er = P(TtsPhonemeType.Voiced, 490, 1350, 1690, 6, 5, 2, 44, 115);

        /// <summary>
        /// The rhotic or vowel phoneme.
        /// </summary>
        public static readonly TtsPhoneme Or = P(TtsPhonemeType.Voiced, 490, 1000, 1690, 6, 5, 2, 44, 115);

        /// <summary>
        /// The rhotic ar vowel phoneme.
        /// </summary>
        public static readonly TtsPhoneme Ar = P(TtsPhonemeType.Voiced, 850, 1200, 2000, 7, 4, 1, 43, 115);

        /// <summary>
        /// The neutral schwa vowel phoneme.
        /// </summary>
        public static readonly TtsPhoneme Schwa = P(TtsPhonemeType.Voiced, 620, 1200, 2400, 5, 4, 2, 44, 80);

        /// <summary>
        /// The silent gh spelling phoneme.
        /// </summary>
        public static readonly TtsPhoneme Gh = P(TtsPhonemeType.Silence, 0, 0, 0, 0, 0, 0, 0, 18);

        /// <summary>
        /// The long a diphthong phoneme.
        /// </summary>
        public static readonly TtsPhoneme Ay = P(TtsPhonemeType.Voiced, 530, 1760, 2820, 6, 5, 2, 46, 140, false, 390, 1990, 2550);

        /// <summary>
        /// The long i diphthong phoneme.
        /// </summary>
        public static readonly TtsPhoneme Eye = P(TtsPhonemeType.Voiced, 750, 1200, 2400, 6, 5, 2, 45, 160, false, 390, 1990, 2550);

        /// <summary>
        /// The long o vowel phoneme.
        /// </summary>
        public static readonly TtsPhoneme Oh = P(TtsPhonemeType.Voiced, 570, 840, 2410, 7, 4, 2, 44, 140, false, 440, 960, 2300);

        /// <summary>
        /// The oy diphthong phoneme.
        /// </summary>
        public static readonly TtsPhoneme Oy = P(TtsPhonemeType.Voiced, 570, 840, 2410, 7, 4, 2, 44, 165, false, 390, 1990, 2550);

        /// <summary>
        /// The ow diphthong phoneme.
        /// </summary>
        public static readonly TtsPhoneme Ow = P(TtsPhonemeType.Voiced, 730, 1090, 2440, 7, 4, 2, 44, 150, false, 440, 960, 2300);

        /// <summary>
        /// The inter-word silence phoneme.
        /// </summary>
        public static readonly TtsPhoneme Space = P(TtsPhonemeType.Silence, 0, 0, 0, 0, 0, 0, 0, SpaceDuration);

        /// <summary>
        /// The comma pause phoneme.
        /// </summary>
        public static readonly TtsPhoneme Comma = P(TtsPhonemeType.Silence, 0, 0, 0, 0, 0, 0, 0, CommaDuration);

        /// <summary>
        /// The sentence-ending pause phoneme.
        /// </summary>
        public static readonly TtsPhoneme Stop = P(TtsPhonemeType.Silence, 0, 0, 0, 0, 0, 0, 0, StopDuration);

        /// <summary>
        /// The ordered multi-letter spelling patterns matched before single letters.
        /// </summary>
        public static readonly Pattern[] Patterns =
        {
            new Pattern("eigh", Ay),
            new Pattern("tion", Sh, Schwa, L('n')),
            new Pattern("igh", Eye),
            new Pattern("dge", L('j')),
            new Pattern("sh", Sh),
            new Pattern("ch", Ch),
            new Pattern("th", Th),
            new Pattern("ph", L('f')),
            new Pattern("wh", L('w')),
            new Pattern("ng", Ng),
            new Pattern("ck", L('k')),
            new Pattern("qu", L('k'), L('w')),
            new Pattern("ee", Ee),
            new Pattern("ea", Ee),
            new Pattern("oo", Uu),
            new Pattern("ew", Uu),
            new Pattern("ue", Uu),
            new Pattern("ou", Ow),
            new Pattern("ow", Ow),
            new Pattern("oa", Oh),
            new Pattern("oi", Oy),
            new Pattern("oy", Oy),
            new Pattern("ai", Ay),
            new Pattern("ay", Ay),
            new Pattern("aw", L('o')),
            new Pattern("au", L('o')),
            new Pattern("gh", Gh),
            new Pattern("er", Er),
            new Pattern("ir", Er),
            new Pattern("ur", Er),
            new Pattern("or", Or),
            new Pattern("ar", Ar),
        };

        private static TtsPhoneme L(char value)
        {
            return Letters[value - 'a'];
        }

        private static TtsPhoneme P(
            TtsPhonemeType type,
            int f1,
            int f2,
            int f3,
            int a1,
            int a2,
            int a3,
            int pitchMidi,
            int duration,
            bool voiced = false,
            int glide1 = 0,
            int glide2 = 0,
            int glide3 = 0)
        {
            return new TtsPhoneme(
                type,
                f1,
                f2,
                f3,
                a1,
                a2,
                a3,
                pitchMidi,
                duration,
                voiced,
                glide1,
                glide2,
                glide3);
        }
    }
}
