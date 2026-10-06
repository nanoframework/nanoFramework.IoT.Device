// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Text2Speech
{
    /// <summary>
    /// Expands decimal integers using bounded French cardinal-number rules.
    /// </summary>
    internal static class FrenchNumberNormalizer
    {
        /// <summary>
        /// Normalizes case, apostrophes, hyphens, and decimal integers into a caller-owned buffer.
        /// </summary>
        /// <param name="input">The source text.</param>
        /// <param name="output">The normalization workspace.</param>
        /// <param name="length">The number of normalized characters.</param>
        /// <returns><see langword="true" /> when the full normalized text fits.</returns>
        public static bool TryExpand(string input, char[] output, out int length)
        {
            int outputIndex = 0;
            for (int inputIndex = 0; inputIndex < input.Length;)
            {
                char current = input[inputIndex];
                if (current == '\'' || current == '\u2019')
                {
                    inputIndex++;
                    continue;
                }

                if (IsDigit(current))
                {
                    int runLength = 1;
                    while (inputIndex + runLength < input.Length && IsDigit(input[inputIndex + runLength]))
                    {
                        runLength++;
                    }

                    if (!TryAppendNumber(input, inputIndex, runLength, output, ref outputIndex))
                    {
                        length = 0;
                        return false;
                    }

                    inputIndex += runLength;
                    continue;
                }

                if (outputIndex >= output.Length)
                {
                    length = 0;
                    return false;
                }

                output[outputIndex++] = NormalizeCharacter(current);
                inputIndex++;
            }

            length = outputIndex;
            return true;
        }

        private static bool TryAppendNumber(
            string input,
            int offset,
            int runLength,
            char[] output,
            ref int outputIndex)
        {
            bool hasWord = false;
            if (runLength <= 9)
            {
                int value = 0;
                for (int i = 0; i < runLength; i++)
                {
                    value = (value * 10) + (input[offset + i] - '0');
                }

                return TryAppendCardinal(value, output, ref outputIndex, ref hasWord);
            }

            for (int i = 0; i < runLength; i++)
            {
                if (!TryAppendWord(DigitWord(input[offset + i] - '0'), output, ref outputIndex, ref hasWord))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryAppendCardinal(
            int value,
            char[] output,
            ref int outputIndex,
            ref bool hasWord)
        {
            if (value == 0)
            {
                return TryAppendWord("zéro", output, ref outputIndex, ref hasWord);
            }

            int millions = value / 1000000;
            if (millions > 0)
            {
                if (millions == 1)
                {
                    if (!TryAppendWord("un", output, ref outputIndex, ref hasWord))
                    {
                        return false;
                    }
                }
                else if (!TryAppendBelowThousand(millions, output, ref outputIndex, ref hasWord))
                {
                    return false;
                }

                if (!TryAppendWord(millions > 1 ? "millions" : "million", output, ref outputIndex, ref hasWord))
                {
                    return false;
                }

                value %= 1000000;
            }

            int thousands = value / 1000;
            if (thousands > 0)
            {
                if (thousands > 1 && !TryAppendBelowThousand(thousands, output, ref outputIndex, ref hasWord))
                {
                    return false;
                }

                if (!TryAppendWord("mille", output, ref outputIndex, ref hasWord))
                {
                    return false;
                }

                value %= 1000;
            }

            return value == 0 || TryAppendBelowThousand(value, output, ref outputIndex, ref hasWord);
        }

        private static bool TryAppendBelowThousand(
            int value,
            char[] output,
            ref int outputIndex,
            ref bool hasWord)
        {
            int hundreds = value / 100;
            int remainder = value % 100;
            if (hundreds > 0)
            {
                if (hundreds > 1
                    && !TryAppendWord(DigitWord(hundreds), output, ref outputIndex, ref hasWord))
                {
                    return false;
                }

                if (!TryAppendWord(
                    hundreds > 1 && remainder == 0 ? "cents" : "cent",
                    output,
                    ref outputIndex,
                    ref hasWord))
                {
                    return false;
                }
            }

            return remainder == 0 || TryAppendBelowHundred(remainder, output, ref outputIndex, ref hasWord);
        }

        private static bool TryAppendBelowHundred(
            int value,
            char[] output,
            ref int outputIndex,
            ref bool hasWord)
        {
            if (value < 17)
            {
                return TryAppendWord(SmallNumberWord(value), output, ref outputIndex, ref hasWord);
            }

            if (value < 20)
            {
                return TryAppendWord("dix", output, ref outputIndex, ref hasWord)
                    && TryAppendWord(DigitWord(value - 10), output, ref outputIndex, ref hasWord);
            }

            if (value < 70)
            {
                int tens = value / 10;
                int units = value % 10;
                if (!TryAppendWord(TensWord(tens), output, ref outputIndex, ref hasWord))
                {
                    return false;
                }

                if (units == 1 && !TryAppendWord("et", output, ref outputIndex, ref hasWord))
                {
                    return false;
                }

                return units == 0 || TryAppendWord(DigitWord(units), output, ref outputIndex, ref hasWord);
            }

            if (value < 80)
            {
                if (!TryAppendWord("soixante", output, ref outputIndex, ref hasWord))
                {
                    return false;
                }

                int remainder = value - 60;
                if (remainder == 11 && !TryAppendWord("et", output, ref outputIndex, ref hasWord))
                {
                    return false;
                }

                return TryAppendBelowHundred(remainder, output, ref outputIndex, ref hasWord);
            }

            if (!TryAppendWord("quatre", output, ref outputIndex, ref hasWord)
                || !TryAppendWord(value == 80 ? "vingts" : "vingt", output, ref outputIndex, ref hasWord))
            {
                return false;
            }

            int finalRemainder = value - 80;
            return finalRemainder == 0
                || TryAppendBelowHundred(finalRemainder, output, ref outputIndex, ref hasWord);
        }

        private static bool TryAppendWord(
            string value,
            char[] output,
            ref int outputIndex,
            ref bool hasWord)
        {
            int required = value.Length + (hasWord ? 1 : 0);
            if (outputIndex + required > output.Length)
            {
                return false;
            }

            if (hasWord)
            {
                output[outputIndex++] = '-';
            }

            for (int i = 0; i < value.Length; i++)
            {
                output[outputIndex++] = value[i];
            }

            hasWord = true;
            return true;
        }

        private static string DigitWord(int value)
        {
            switch (value)
            {
                case 0: return "zéro";
                case 1: return "un";
                case 2: return "deux";
                case 3: return "trois";
                case 4: return "quatre";
                case 5: return "cinq";
                case 6: return "six";
                case 7: return "sept";
                case 8: return "huit";
                default: return "neuf";
            }
        }

        private static string SmallNumberWord(int value)
        {
            switch (value)
            {
                case 0: return "zéro";
                case 1: return "un";
                case 2: return "deux";
                case 3: return "trois";
                case 4: return "quatre";
                case 5: return "cinq";
                case 6: return "six";
                case 7: return "sept";
                case 8: return "huit";
                case 9: return "neuf";
                case 10: return "dix";
                case 11: return "onze";
                case 12: return "douze";
                case 13: return "treize";
                case 14: return "quatorze";
                case 15: return "quinze";
                default: return "seize";
            }
        }

        private static string TensWord(int value)
        {
            switch (value)
            {
                case 2: return "vingt";
                case 3: return "trente";
                case 4: return "quarante";
                case 5: return "cinquante";
                default: return "soixante";
            }
        }

        private static char NormalizeCharacter(char value)
        {
            if (value >= 'A' && value <= 'Z')
            {
                return (char)(value - 'A' + 'a');
            }

            switch (value)
            {
                case 'À': return 'à';
                case 'Â': return 'â';
                case 'Ä': return 'ä';
                case 'Æ': return 'æ';
                case 'Ç': return 'ç';
                case 'É': return 'é';
                case 'È': return 'è';
                case 'Ê': return 'ê';
                case 'Ë': return 'ë';
                case 'Î': return 'î';
                case 'Ï': return 'ï';
                case 'Ô': return 'ô';
                case 'Ö': return 'ö';
                case 'Œ': return 'œ';
                case 'Ù': return 'ù';
                case 'Û': return 'û';
                case 'Ü': return 'ü';
                case 'Ÿ': return 'ÿ';
                case '\u2010':
                case '\u2011':
                case '\u2013': return '-';
                default: return value;
            }
        }

        private static bool IsDigit(char value)
        {
            return value >= '0' && value <= '9';
        }
    }
}
