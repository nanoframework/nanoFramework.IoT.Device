// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
            int expandedLength;
            if (!TryExpandText(text, _expandedText, out expandedLength))
            {
                return false;
            }

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
                int irregularLength;
                if (wordLength == 0
                    && TryAddIrregularWord(
                        _expandedText,
                        expandedLength,
                        i,
                        letterPosition,
                        letterTotal,
                        isQuestion,
                        out irregularLength))
                {
                    i += irregularLength - 1;
                    letterPosition += irregularLength;
                    wordLength = irregularLength;
                    continue;
                }

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

        private static bool MatchesWord(
            char[] text,
            int textLength,
            int offset,
            string expected)
        {
            for (int i = 0; i < expected.Length; i++)
            {
                if (ToLowerAscii(CharAt(text, textLength, offset + i)) != expected[i])
                {
                    return false;
                }
            }

            return !IsLowerLetter(ToLowerAscii(CharAt(text, textLength, offset + expected.Length)));
        }

        private static bool TryExpandText(string input, char[] output, out int length)
        {
            int outputIndex = 0;
            for (int i = 0; i < input.Length;)
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
                            if (!TryAppend(output, ref outputIndex, DigitWords[value]))
                            {
                                length = 0;
                                return false;
                            }
                        }
                        else if (value < 20)
                        {
                            if (!TryAppend(output, ref outputIndex, TeenWords[value - 10]))
                            {
                                length = 0;
                                return false;
                            }
                        }
                        else
                        {
                            if (!TryAppend(output, ref outputIndex, TensWords[value / 10]))
                            {
                                length = 0;
                                return false;
                            }

                            if ((value % 10) != 0)
                            {
                                if (!TryAppend(output, ref outputIndex, " ")
                                    || !TryAppend(output, ref outputIndex, DigitWords[value % 10]))
                                {
                                    length = 0;
                                    return false;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int digit = 0; digit < run; digit++)
                        {
                            if (digit != 0)
                            {
                                if (!TryAppend(output, ref outputIndex, " "))
                                {
                                    length = 0;
                                    return false;
                                }
                            }

                            if (!TryAppend(
                                output,
                                ref outputIndex,
                                DigitWords[input[i + digit] - '0']))
                            {
                                length = 0;
                                return false;
                            }
                        }
                    }

                    i += run;
                    continue;
                }

                if (outputIndex >= output.Length)
                {
                    length = 0;
                    return false;
                }

                output[outputIndex++] = current;
                i++;
            }

            length = outputIndex;
            return true;
        }

        private static bool TryAppend(char[] output, ref int index, string value)
        {
            if (value.Length > output.Length - index)
            {
                return false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                output[index++] = value[i];
            }

            return true;
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

        private bool TryAddIrregularWord(
            char[] text,
            int textLength,
            int offset,
            int letterPosition,
            int letterTotal,
            bool isQuestion,
            out int wordLength)
        {
            if (MatchesWord(text, textLength, offset, "any"))
            {
                Add(Letter('e'), PitchForPosition(letterPosition, letterTotal, isQuestion));
                Add(Letter('n'), PitchForPosition(letterPosition + 1, letterTotal, isQuestion));
                Add(PhonemeData.Iy, PitchForPosition(letterPosition + 2, letterTotal, isQuestion));
                wordLength = 3;
                return true;
            }

            if (MatchesWord(text, textLength, offset, "other"))
            {
                Add(Letter('u'), PitchForPosition(letterPosition, letterTotal, isQuestion));
                Add(PhonemeData.VoicedTh, PitchForPosition(letterPosition + 1, letterTotal, isQuestion));
                Add(PhonemeData.Er, PitchForPosition(letterPosition + 3, letterTotal, isQuestion));
                wordLength = 5;
                return true;
            }

            wordLength = 0;
            return false;
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
