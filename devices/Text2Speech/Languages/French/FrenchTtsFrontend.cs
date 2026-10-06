// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Text2Speech
{
    /// <summary>
    /// Normalizes French text and converts spelling patterns into a bounded phoneme sequence.
    /// </summary>
    internal sealed class FrenchTtsFrontend : ITtsLanguageFrontend
    {
        private readonly char[] _expandedText =
            new char[(FrenchTtsLanguage.DefaultMaximumTextLength * 8) + 1];

        private TtsPhonemeBuffer _phonemes;
        private bool _overflowed;

        /// <summary>
        /// Expands and parses French text into the reusable phoneme sequence.
        /// </summary>
        /// <param name="text">The French text accepted by the synthesizer.</param>
        /// <param name="phonemes">The reusable destination phoneme buffer.</param>
        /// <returns>
        /// <see langword="true" /> when normalization and all phonemes fit; otherwise,
        /// <see langword="false" />.
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
            if (!FrenchNumberNormalizer.TryExpand(text, _expandedText, out expandedLength))
            {
                return false;
            }

            bool isQuestion = false;
            int letterTotal = 0;
            for (int i = 0; i < expandedLength; i++)
            {
                if (_expandedText[i] == '?')
                {
                    isQuestion = true;
                }

                if (IsLetter(_expandedText[i]))
                {
                    letterTotal++;
                }
            }

            int letterPosition = 0;
            for (int i = 0; i < expandedLength; i++)
            {
                char current = _expandedText[i];
                if (current == ' ' || current == '\t' || current == '\r' || current == '\n')
                {
                    Add(FrenchPhonemeData.Space, 0);
                    continue;
                }

                if (current == ',' || current == ';' || current == ':')
                {
                    Add(FrenchPhonemeData.Comma, 0);
                    continue;
                }

                if (current == '.' || current == '!' || current == '?')
                {
                    Add(FrenchPhonemeData.Stop, 0);
                    continue;
                }

                if (current == '-')
                {
                    continue;
                }

                if (!IsLetter(current))
                {
                    continue;
                }

                int pitchOffset = PitchForPosition(letterPosition, letterTotal, isQuestion);
                int consumed = ParsePattern(i, expandedLength, pitchOffset);
                if (consumed == 0)
                {
                    ParseCharacter(i, expandedLength, pitchOffset);
                    consumed = 1;
                }

                i += consumed - 1;
                letterPosition += consumed;
            }

            return !_overflowed;
        }

        private int ParsePattern(int offset, int length, int pitchOffset)
        {
            if (IsWholeWord(offset, length, "et"))
            {
                Add(FrenchPhonemeData.EClose, pitchOffset);
                return 2;
            }

            if (IsWholeWord(offset, length, "est"))
            {
                Add(FrenchPhonemeData.EOpen, pitchOffset);
                return 3;
            }

            if (IsWholeWord(offset, length, "es"))
            {
                Add(FrenchPhonemeData.EOpen, pitchOffset);
                return 2;
            }

            if (Match(offset, length, "eaux"))
            {
                Add(FrenchPhonemeData.OClose, pitchOffset);
                return 4;
            }

            if (Match(offset, length, "tion"))
            {
                Add(FrenchPhonemeData.S, pitchOffset);
                Add(FrenchPhonemeData.J, pitchOffset);
                Add(FrenchPhonemeData.NasalOn, pitchOffset);
                return 4;
            }

            if (Match(offset, length, "eau"))
            {
                Add(FrenchPhonemeData.OClose, pitchOffset);
                return 3;
            }

            if (Match(offset, length, "oin") && IsNasalContext(offset + 3, length))
            {
                Add(FrenchPhonemeData.W, pitchOffset);
                Add(FrenchPhonemeData.NasalIn, pitchOffset);
                return 3;
            }

            if (Match(offset, length, "uin") && IsNasalContext(offset + 3, length))
            {
                Add(FrenchPhonemeData.H, pitchOffset);
                Add(FrenchPhonemeData.NasalIn, pitchOffset);
                return 3;
            }

            if (Match(offset, length, "ien") && IsNasalContext(offset + 3, length))
            {
                Add(FrenchPhonemeData.J, pitchOffset);
                Add(FrenchPhonemeData.NasalIn, pitchOffset);
                return 3;
            }

            if (Match(offset, length, "ill"))
            {
                if (!IsVowel(CharAt(offset - 1, length)))
                {
                    Add(FrenchPhonemeData.I, pitchOffset);
                }

                Add(FrenchPhonemeData.J, pitchOffset);
                return 3;
            }

            if (Match(offset, length, "oeu"))
            {
                Add(IsWordEnd(offset + 3, length) ? FrenchPhonemeData.Eu : FrenchPhonemeData.Oe, pitchOffset);
                return 3;
            }

            if (Match(offset, length, "ain") || Match(offset, length, "ein"))
            {
                if (IsNasalContext(offset + 3, length))
                {
                    Add(FrenchPhonemeData.NasalIn, pitchOffset);
                    return 3;
                }
            }

            if (Match(offset, length, "gn"))
            {
                Add(FrenchPhonemeData.Ny, pitchOffset);
                return 2;
            }

            if (Match(offset, length, "ch"))
            {
                Add(FrenchPhonemeData.Sh, pitchOffset);
                return 2;
            }

            if (Match(offset, length, "ph"))
            {
                Add(FrenchPhonemeData.F, pitchOffset);
                return 2;
            }

            if (Match(offset, length, "th"))
            {
                Add(FrenchPhonemeData.T, pitchOffset);
                return 2;
            }

            if (Match(offset, length, "qu"))
            {
                Add(FrenchPhonemeData.K, pitchOffset);
                return 2;
            }

            if (Match(offset, length, "gu") && IsFrontVowel(CharAt(offset + 2, length)))
            {
                Add(FrenchPhonemeData.G, pitchOffset);
                return 2;
            }

            if (Match(offset, length, "sc") && IsFrontVowel(CharAt(offset + 2, length)))
            {
                Add(FrenchPhonemeData.S, pitchOffset);
                return 2;
            }

            if ((Match(offset, length, "an") || Match(offset, length, "am")
                || Match(offset, length, "en") || Match(offset, length, "em"))
                && IsNasalContext(offset + 2, length))
            {
                Add(FrenchPhonemeData.NasalAn, pitchOffset);
                return 2;
            }

            if ((Match(offset, length, "in") || Match(offset, length, "im")
                || Match(offset, length, "yn") || Match(offset, length, "ym"))
                && IsNasalContext(offset + 2, length))
            {
                Add(FrenchPhonemeData.NasalIn, pitchOffset);
                return 2;
            }

            if ((Match(offset, length, "on") || Match(offset, length, "om"))
                && IsNasalContext(offset + 2, length))
            {
                Add(FrenchPhonemeData.NasalOn, pitchOffset);
                return 2;
            }

            if ((Match(offset, length, "un") || Match(offset, length, "um"))
                && IsNasalContext(offset + 2, length))
            {
                Add(FrenchPhonemeData.NasalUn, pitchOffset);
                return 2;
            }

            if (Match(offset, length, "ou"))
            {
                Add(IsVowel(CharAt(offset + 2, length)) ? FrenchPhonemeData.W : FrenchPhonemeData.U, pitchOffset);
                return 2;
            }

            if (Match(offset, length, "oi"))
            {
                Add(FrenchPhonemeData.W, pitchOffset);
                Add(FrenchPhonemeData.A, pitchOffset);
                return 2;
            }

            if (Match(offset, length, "ui"))
            {
                Add(FrenchPhonemeData.H, pitchOffset);
                Add(FrenchPhonemeData.I, pitchOffset);
                return 2;
            }

            if (Match(offset, length, "au"))
            {
                Add(FrenchPhonemeData.OClose, pitchOffset);
                return 2;
            }

            if (Match(offset, length, "ai") || Match(offset, length, "ei"))
            {
                Add(IsWordEnd(offset + 2, length) ? FrenchPhonemeData.EClose : FrenchPhonemeData.EOpen, pitchOffset);
                return 2;
            }

            if (Match(offset, length, "eu"))
            {
                Add(IsWordEndAfterSilentFinal(offset + 2, length) ? FrenchPhonemeData.Eu : FrenchPhonemeData.Oe, pitchOffset);
                return 2;
            }

            if (Match(offset, length, "er") || Match(offset, length, "ez"))
            {
                if (IsWordEnd(offset + 2, length))
                {
                    Add(FrenchPhonemeData.EClose, pitchOffset);
                    return 2;
                }
            }

            if (Match(offset, length, "il") && IsVowel(CharAt(offset - 1, length)) && IsWordEnd(offset + 2, length))
            {
                Add(FrenchPhonemeData.J, pitchOffset);
                return 2;
            }

            return 0;
        }

        private void ParseCharacter(int offset, int length, int pitchOffset)
        {
            char current = CharAt(offset, length);
            char next = CharAt(offset + 1, length);
            bool wordEnd = IsWordEnd(offset + 1, length);

            if ((wordEnd && IsSilentFinal(current) && !IsPronouncedFinalException(offset, length))
                || (current == 'p' && next == 't' && IsWholeWord(offset, length, "sept")))
            {
                return;
            }

            switch (current)
            {
                case 'a':
                case 'à':
                case 'â':
                case 'ä':
                case 'æ':
                    Add(FrenchPhonemeData.A, pitchOffset);
                    break;
                case 'é':
                    Add(FrenchPhonemeData.EClose, pitchOffset);
                    break;
                case 'è':
                case 'ê':
                case 'ë':
                    Add(FrenchPhonemeData.EOpen, pitchOffset);
                    break;
                case 'e':
                    if (!wordEnd)
                    {
                        TtsPhoneme ePhoneme = IsConsonant(next) && IsConsonant(CharAt(offset + 2, length))
                            ? FrenchPhonemeData.EOpen
                            : FrenchPhonemeData.Schwa;
                        Add(ePhoneme, pitchOffset);
                    }
                    else if (IsSchwaFunctionWord(offset, length))
                    {
                        Add(FrenchPhonemeData.Schwa, pitchOffset);
                    }

                    break;
                case 'i':
                case 'î':
                case 'ï':
                case 'y':
                case 'ÿ':
                    Add(IsVowel(next) ? FrenchPhonemeData.J : FrenchPhonemeData.I, pitchOffset);
                    break;
                case 'o':
                case 'ö':
                    TtsPhoneme oPhoneme = wordEnd || IsWordEndAfterSilentFinal(offset + 1, length)
                        ? FrenchPhonemeData.OClose
                        : FrenchPhonemeData.OOpen;
                    Add(oPhoneme, pitchOffset);
                    break;
                case 'ô':
                    Add(FrenchPhonemeData.OClose, pitchOffset);
                    break;
                case 'u':
                case 'ù':
                case 'û':
                case 'ü':
                    Add(IsVowel(next) ? FrenchPhonemeData.H : FrenchPhonemeData.YRounded, pitchOffset);
                    break;
                case 'œ':
                    Add(wordEnd ? FrenchPhonemeData.Eu : FrenchPhonemeData.Oe, pitchOffset);
                    break;
                case 'p':
                    Add(FrenchPhonemeData.PConsonant, pitchOffset);
                    break;
                case 'b':
                    Add(FrenchPhonemeData.B, pitchOffset);
                    break;
                case 't':
                    Add(FrenchPhonemeData.T, pitchOffset);
                    break;
                case 'd':
                    Add(FrenchPhonemeData.D, pitchOffset);
                    break;
                case 'k':
                case 'q':
                    Add(FrenchPhonemeData.K, pitchOffset);
                    break;
                case 'g':
                    Add(IsFrontVowel(next) ? FrenchPhonemeData.Zh : FrenchPhonemeData.G, pitchOffset);
                    break;
                case 'f':
                    Add(FrenchPhonemeData.F, pitchOffset);
                    break;
                case 'v':
                    Add(FrenchPhonemeData.V, pitchOffset);
                    break;
                case 'c':
                case 'ç':
                    Add(current == 'ç' || IsFrontVowel(next) ? FrenchPhonemeData.S : FrenchPhonemeData.K, pitchOffset);
                    break;
                case 's':
                    Add(IsVowel(CharAt(offset - 1, length)) && IsVowel(next) ? FrenchPhonemeData.Z : FrenchPhonemeData.S, pitchOffset);
                    break;
                case 'z':
                    Add(FrenchPhonemeData.Z, pitchOffset);
                    break;
                case 'j':
                    Add(FrenchPhonemeData.Zh, pitchOffset);
                    break;
                case 'm':
                    Add(FrenchPhonemeData.M, pitchOffset);
                    break;
                case 'n':
                    Add(FrenchPhonemeData.N, pitchOffset);
                    break;
                case 'l':
                    Add(FrenchPhonemeData.L, pitchOffset);
                    break;
                case 'r':
                    Add(FrenchPhonemeData.R, pitchOffset);
                    break;
                case 'w':
                    Add(FrenchPhonemeData.W, pitchOffset);
                    break;
                case 'x':
                    if (wordEnd && IsPronouncedFinalException(offset, length))
                    {
                        Add(FrenchPhonemeData.S, pitchOffset);
                        break;
                    }

                    bool voicedX = IsVowel(CharAt(offset - 1, length)) && IsVowel(next);
                    Add(voicedX ? FrenchPhonemeData.G : FrenchPhonemeData.K, pitchOffset);
                    Add(voicedX ? FrenchPhonemeData.Z : FrenchPhonemeData.S, pitchOffset);
                    break;
            }
        }

        private bool Match(int offset, int length, string value)
        {
            if (offset + value.Length > length)
            {
                return false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                if (_expandedText[offset + i] != value[i])
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsNasalContext(int offset, int length)
        {
            char next = CharAt(offset, length);
            return !IsLetter(next) || IsConsonant(next);
        }

        private bool IsWordEnd(int offset, int length)
        {
            return !IsLetter(CharAt(offset, length));
        }

        private bool IsWordEndAfterSilentFinal(int offset, int length)
        {
            char value = CharAt(offset, length);
            return IsWordEnd(offset, length)
                || (IsSilentFinal(value) && IsWordEnd(offset + 1, length));
        }

        private bool IsPronouncedFinalException(int offset, int length)
        {
            return IsWholeWord(offset, length, "six")
                || IsWholeWord(offset, length, "dix")
                || IsWholeWord(offset, length, "sept")
                || IsWholeWord(offset, length, "huit");
        }

        private bool IsSchwaFunctionWord(int offset, int length)
        {
            return IsWholeWord(offset, length, "ce")
                || IsWholeWord(offset, length, "de")
                || IsWholeWord(offset, length, "je")
                || IsWholeWord(offset, length, "le")
                || IsWholeWord(offset, length, "me")
                || IsWholeWord(offset, length, "ne")
                || IsWholeWord(offset, length, "se")
                || IsWholeWord(offset, length, "te")
                || IsWholeWord(offset, length, "que");
        }

        private bool IsWholeWord(int offset, int length, string value)
        {
            int start = offset;
            while (start > 0 && IsLetter(CharAt(start - 1, length)))
            {
                start--;
            }

            if (!Match(start, length, value) || !IsWordEnd(start + value.Length, length))
            {
                return false;
            }

            return offset >= start && offset < start + value.Length;
        }

        private char CharAt(int offset, int length)
        {
            return offset >= 0 && offset < length ? _expandedText[offset] : '\0';
        }

        private int PitchForPosition(int position, int total, bool question)
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

        private bool IsSilentFinal(char value)
        {
            return value == 'd' || value == 'g' || value == 'p' || value == 's'
                || value == 't' || value == 'x' || value == 'z';
        }

        private bool IsFrontVowel(char value)
        {
            return value == 'e' || value == 'é' || value == 'è' || value == 'ê'
                || value == 'ë' || value == 'i' || value == 'î' || value == 'ï'
                || value == 'y' || value == 'ÿ';
        }

        private bool IsConsonant(char value)
        {
            return IsLetter(value) && !IsVowel(value);
        }

        private bool IsVowel(char value)
        {
            return value == 'a' || value == 'à' || value == 'â' || value == 'ä' || value == 'æ'
                || value == 'e' || value == 'é' || value == 'è' || value == 'ê' || value == 'ë'
                || value == 'i' || value == 'î' || value == 'ï' || value == 'o' || value == 'ô'
                || value == 'ö' || value == 'œ' || value == 'u' || value == 'ù' || value == 'û'
                || value == 'ü' || value == 'y' || value == 'ÿ';
        }

        private bool IsLetter(char value)
        {
            return (value >= 'a' && value <= 'z') || IsVowel(value) || value == 'ç';
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
