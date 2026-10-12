// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Text2Speech
{
    /// <summary>
    /// Provides allocation-free French accentual-group timing and pitch rules.
    /// </summary>
    internal static class FrenchProsody
    {
        public const int NormalDurationPercent = 102;
        public const int AccentedDurationPercent = 118;

        public static int FindGroupEnd(char[] text, int offset, int length)
        {
            for (int i = offset; i < length; i++)
            {
                char value = text[i];
                if (value == ',' || value == ';' || value == ':'
                    || value == '.' || value == '!' || value == '?')
                {
                    return i;
                }
            }

            return length;
        }

        public static int CountLetters(char[] text, int offset, int end)
        {
            int count = 0;
            for (int i = offset; i < end; i++)
            {
                if (IsLetter(text[i]))
                {
                    count++;
                }
            }

            return count;
        }

        public static int FindAccentOffset(char[] text, int offset, int end)
        {
            for (int i = end - 1; i >= offset; i--)
            {
                char value = text[i];
                if (!IsVowel(value))
                {
                    continue;
                }

                if (value == 'e' && (i + 1 == end || !IsLetter(text[i + 1])))
                {
                    continue;
                }

                return i;
            }

            return -1;
        }

        public static int PitchForPosition(
            int position,
            int total,
            bool question,
            bool finalGroup)
        {
            if (total <= 1)
            {
                return question && finalGroup ? 2 : 0;
            }

            int progress = (position * 100) / (total - 1);
            if (!finalGroup)
            {
                return progress < 60 ? 0 : 1;
            }

            if (question)
            {
                if (progress < 55)
                {
                    return -1;
                }

                return progress < 80 ? 1 : 3;
            }

            if (progress < 55)
            {
                return 0;
            }

            return progress < 82 ? 1 : -2;
        }

        public static int DurationPercent(char[] text, int offset, int accentOffset)
        {
            if (accentOffset < offset || !IsVowel(text[offset]))
            {
                return NormalDurationPercent;
            }

            for (int i = offset; i <= accentOffset; i++)
            {
                if (!IsVowel(text[i]))
                {
                    return NormalDurationPercent;
                }
            }

            return AccentedDurationPercent;
        }

        private static bool IsLetter(char value)
        {
            return (value >= 'a' && value <= 'z') || IsVowel(value) || value == 'ç';
        }

        private static bool IsVowel(char value)
        {
            return value == 'a' || value == 'à' || value == 'â' || value == 'ä' || value == 'æ'
                || value == 'e' || value == 'é' || value == 'è' || value == 'ê' || value == 'ë'
                || value == 'i' || value == 'î' || value == 'ï' || value == 'o' || value == 'ô'
                || value == 'ö' || value == 'œ' || value == 'u' || value == 'ù' || value == 'û'
                || value == 'ü' || value == 'y' || value == 'ÿ';
        }
    }
}
