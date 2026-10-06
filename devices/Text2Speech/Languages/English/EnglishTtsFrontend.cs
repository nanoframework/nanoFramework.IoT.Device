// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Text2Speech
{
    /// <summary>
    /// Expands English text and converts spelling patterns into a bounded phoneme sequence.
    /// </summary>
    internal sealed class EnglishTtsFrontend : ITtsLanguageFrontend
    {
        private static readonly string[] DigitWords =
        {
            "zeero", "wun", "too", "three", "for", "five", "six", "seven", "eight", "nine",
        };

        private static readonly string[] TeenWords =
        {
            "ten", "eleven", "twelve", "thirteen", "forteen",
            "fifteen", "sixteen", "seventeen", "eighteen", "nighnteen",
        };

        private static readonly string[] TensWords =
        {
            string.Empty, string.Empty, "twenty", "thirty", "forty",
            "fifty", "sixty", "seventy", "eighty", "nighntee",
        };

        private readonly char[] _expandedText =
            new char[(EnglishTtsLanguage.DefaultMaximumTextLength * 3) + 1];

        private TtsPhonemeBuffer _phonemes;
        private bool _overflowed;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnglishTtsFrontend" /> class.
        /// </summary>
        public EnglishTtsFrontend()
        {
        }

        /// <summary>
        /// Expands and parses text into the reusable phoneme sequence.
        /// </summary>
        /// <param name="text">The English text accepted by the synthesizer.</param>
        /// <param name="phonemes">The reusable destination phoneme buffer.</param>
        /// <returns>
        /// <see langword="true" /> when all phonemes fit; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="text" /> or <paramref name="phonemes" /> is <see langword="null" />.
        /// </exception>
        public bool TryPrepare(string text, TtsPhonemeBuffer phonemes)
        {
            if (text == null || phonemes == null)
            {
                throw new ArgumentNullException();
            }

            phonemes.Clear();
            _phonemes = phonemes;
            _overflowed = false;
            int expandedLength = ExpandText(text, _expandedText);
            bool isQuestion = false;
            int letterTotal = 0;
            for (int i = 0; i < expandedLength; i++)
            {
                char value = _expandedText[i];
                if (value == '?')
                {
                    isQuestion = true;
                }

                if (IsLetter(value))
                {
                    letterTotal++;
                }
            }

            int letterPosition = 0;
            int wordLength = 0;
            for (int i = 0; i < expandedLength; i++)
            {
                char current = ToLowerAscii(_expandedText[i]);
                if (current == ' ')
                {
                    Add(PhonemeData.Space, 0);
                    wordLength = 0;
                    continue;
                }

                if (current == ',')
                {
                    Add(PhonemeData.Comma, 0);
                    wordLength = 0;
                    continue;
                }

                if (current == '.' || current == '!' || current == '?')
                {
                    Add(PhonemeData.Stop, 0);
                    wordLength = 0;
                    continue;
                }

                if (!IsLowerLetter(current))
                {
                    wordLength = 0;
                    continue;
                }

                char next = ToLowerAscii(CharAt(_expandedText, expandedLength, i + 1));
                bool nextLetter = IsLowerLetter(next);
                if (current == 'e'
                    && next == 'd'
                    && wordLength >= 3
                    && !IsLowerLetter(ToLowerAscii(CharAt(_expandedText, expandedLength, i + 2))))
                {
                    letterPosition++;
                    continue;
                }

                if (current == 'e' && !nextLetter && wordLength >= 3)
                {
                    letterPosition++;
                    continue;
                }

                int pitchOffset = PitchForPosition(letterPosition, letterTotal, isQuestion);
                bool previousLetter = i > 0 && IsLowerLetter(ToLowerAscii(_expandedText[i - 1]));
                if (!previousLetter && !nextLetter && (current == 'i' || current == 'a'))
                {
                    Add(current == 'i' ? PhonemeData.Eye : PhonemeData.Schwa, pitchOffset);
                    letterPosition++;
                    wordLength++;
                    continue;
                }

                if (wordLength >= 1 && IsVowel(current))
                {
                    char afterNext = ToLowerAscii(CharAt(_expandedText, expandedLength, i + 2));
                    char fourth = ToLowerAscii(CharAt(_expandedText, expandedLength, i + 3));
                    bool nextConsonant = IsLowerLetter(next) && !IsVowel(next) && next != 'r';
                    if (nextConsonant && afterNext == 'e' && !IsLowerLetter(fourth))
                    {
                        Add(LongVowel(current), pitchOffset);
                        letterPosition++;
                        wordLength++;
                        continue;
                    }
                }

                Pattern pattern = FindPattern(_expandedText, expandedLength, i);
                if (pattern != null)
                {
                    TtsPhoneme first = pattern.First;
                    if (pattern.Text == "ow" && !IsLowerLetter(ToLowerAscii(CharAt(_expandedText, expandedLength, i + 2))))
                    {
                        first = PhonemeData.Oh;
                    }

                    Add(first, pitchOffset);
                    if (pattern.Second != null)
                    {
                        Add(pattern.Second, pitchOffset);
                    }

                    if (pattern.Third != null)
                    {
                        Add(pattern.Third, pitchOffset);
                    }

                    i += pattern.Text.Length - 1;
                    letterPosition += pattern.Text.Length;
                    wordLength += pattern.Text.Length;
                    continue;
                }

                if (current == 'c' && (next == 'e' || next == 'i' || next == 'y'))
                {
                    Add(Letter('s'), pitchOffset);
                    letterPosition++;
                    wordLength++;
                    continue;
                }

                if (current == 'y' && !nextLetter && wordLength > 0)
                {
                    Add(wordLength <= 2 ? PhonemeData.Eye : PhonemeData.Iy, pitchOffset);
                    letterPosition++;
                    wordLength++;
                    continue;
                }

                Add(Letter(current), pitchOffset);
                letterPosition++;
                wordLength++;
                if (next == current)
                {
                    i++;
                    letterPosition++;
                    wordLength++;
                }
            }

            return !_overflowed;
        }

        private static int ExpandText(string input, char[] output)
        {
            int outputIndex = 0;
            for (int i = 0; i < input.Length && outputIndex < output.Length - 1;)
            {
                char current = input[i];
                if (current == '\'')
                {
                    i++;
                    continue;
                }

                if (current >= '0' && current <= '9')
                {
                    int run = 0;
                    while (i + run < input.Length && IsDigit(input[i + run]))
                    {
                        run++;
                    }

                    if (run == 2)
                    {
                        int value = ((current - '0') * 10) + (input[i + 1] - '0');
                        if (value < 10)
                        {
                            outputIndex = Append(output, outputIndex, DigitWords[value]);
                        }
                        else if (value < 20)
                        {
                            outputIndex = Append(output, outputIndex, TeenWords[value - 10]);
                        }
                        else
                        {
                            outputIndex = Append(output, outputIndex, TensWords[value / 10]);
                            if ((value % 10) != 0)
                            {
                                outputIndex = Append(output, outputIndex, " ");
                                outputIndex = Append(output, outputIndex, DigitWords[value % 10]);
                            }
                        }
                    }
                    else
                    {
                        for (int digit = 0; digit < run; digit++)
                        {
                            if (digit != 0)
                            {
                                outputIndex = Append(output, outputIndex, " ");
                            }

                            outputIndex = Append(output, outputIndex, DigitWords[input[i + digit] - '0']);
                        }
                    }

                    i += run;
                    continue;
                }

                output[outputIndex++] = current;
                i++;
            }

            return outputIndex;
        }

        private static int Append(char[] output, int index, string value)
        {
            for (int i = 0; i < value.Length && index < output.Length - 1; i++)
            {
                output[index++] = value[i];
            }

            return index;
        }

        private static Pattern FindPattern(char[] text, int textLength, int offset)
        {
            for (int i = 0; i < PhonemeData.Patterns.Length; i++)
            {
                Pattern pattern = PhonemeData.Patterns[i];
                bool matches = true;
                for (int index = 0; index < pattern.Text.Length; index++)
                {
                    if (ToLowerAscii(CharAt(text, textLength, offset + index)) != pattern.Text[index])
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                {
                    return pattern;
                }
            }

            return null;
        }

        private static int PitchForPosition(int position, int total, bool question)
        {
            if (total <= 4)
            {
                return question ? 2 : 0;
            }

            if (question)
            {
                if ((position * 2) < total)
                {
                    return -1;
                }

                int halfPosition = position - (total / 2);
                int halfTotal = total - (total / 2);
                return -1 + ((halfPosition * 5) / halfTotal);
            }

            if ((position * 2) < total)
            {
                return (position * 6) / total;
            }

            int fallingPosition = position - (total / 2);
            int fallingTotal = total - (total / 2);
            return 3 - ((fallingPosition * 5) / fallingTotal);
        }

        private static TtsPhoneme LongVowel(char value)
        {
            if (value == 'a')
            {
                return PhonemeData.Ay;
            }

            if (value == 'e')
            {
                return PhonemeData.Ee;
            }

            if (value == 'i')
            {
                return PhonemeData.Eye;
            }

            if (value == 'o')
            {
                return PhonemeData.Oh;
            }

            return PhonemeData.Uu;
        }

        private static TtsPhoneme Letter(char value)
        {
            return PhonemeData.Letters[value - 'a'];
        }

        private static char CharAt(char[] value, int length, int index)
        {
            return index >= 0 && index < length ? value[index] : '\0';
        }

        private static char ToLowerAscii(char value)
        {
            return value >= 'A' && value <= 'Z' ? (char)(value - 'A' + 'a') : value;
        }

        private static bool IsDigit(char value)
        {
            return value >= '0' && value <= '9';
        }

        private static bool IsLetter(char value)
        {
            char lower = ToLowerAscii(value);
            return IsLowerLetter(lower);
        }

        private static bool IsLowerLetter(char value)
        {
            return value >= 'a' && value <= 'z';
        }

        private static bool IsVowel(char value)
        {
            return value == 'a' || value == 'e' || value == 'i' || value == 'o' || value == 'u';
        }

        private void Add(TtsPhoneme phoneme, int pitchOffset)
        {
            if (!_phonemes.TryAdd(phoneme, pitchOffset))
            {
                _overflowed = true;
            }
        }
    }
}
