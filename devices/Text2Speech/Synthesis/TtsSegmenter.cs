// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Text2Speech
{
    /// <summary>
    /// Splits long text into synthesis rounds accepted by a selected language frontend.
    /// </summary>
    public sealed class TtsSegmenter
    {
        private readonly object _syncRoot = new object();
        private readonly ITtsLanguageFrontend _frontend;
        private readonly TtsPhonemeBuffer _phonemes = new TtsPhonemeBuffer();

        /// <summary>
        /// Initializes a new instance of the <see cref="TtsSegmenter" /> class for a language definition.
        /// </summary>
        /// <param name="language">The language whose limits and frontend validate each segment.</param>
        /// <exception cref="ArgumentNullException"><paramref name="language" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">
        /// The language reports a non-positive maximum input length.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The language does not create a frontend.
        /// </exception>
        public TtsSegmenter(ITtsLanguage language)
        {
            if (language == null)
            {
                throw new ArgumentNullException();
            }

            if (language.MaximumTextLength <= 0)
            {
                throw new ArgumentException();
            }

            ITtsLanguageFrontend frontend = language.CreateFrontend();
            if (frontend == null)
            {
                throw new InvalidOperationException();
            }

            Language = language;
            _frontend = frontend;
        }

        /// <summary>
        /// Gets the language used to validate segment boundaries and phoneme capacity.
        /// </summary>
        /// <value>The immutable language definition.</value>
        public ITtsLanguage Language { get; }

        /// <summary>
        /// Splits text at sentence or word boundaries into independently synthesizable rounds.
        /// </summary>
        /// <param name="text">The text to split.</param>
        /// <returns>One or more segments accepted by the configured language.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="text" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="text" /> is empty, contains no supported characters, or cannot be split
        /// within the synthesizer's phoneme capacity.
        /// </exception>
        public string[] Split(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException();
            }

            if (text.Length == 0)
            {
                throw new ArgumentException();
            }

            lock (_syncRoot)
            {
                string[] workspace = new string[text.Length];
                int count = 0;
                int offset = 0;
                while (offset < text.Length)
                {
                    while (offset < text.Length && IsWhitespace(text[offset]))
                    {
                        offset++;
                    }

                    if (offset >= text.Length)
                    {
                        break;
                    }

                    int length = GetRoundLength(text, offset);
                    string segment = text.Substring(offset, length).Trim();
                    offset += length;
                    if (segment.Length != 0)
                    {
                        AddSegment(segment, workspace, ref count);
                    }
                }

                if (count == 0)
                {
                    throw new ArgumentException();
                }

                string[] result = new string[count];
                Array.Copy(workspace, result, count);
                return result;
            }
        }

        private static int FindRetrySplit(string text)
        {
            int middle = text.Length / 2;
            for (int i = middle; i > 0; i--)
            {
                if (IsWhitespace(text[i]))
                {
                    return i;
                }

                if (IsSentenceEnd(text[i]) && i + 1 < text.Length)
                {
                    return i + 1;
                }
            }

            for (int i = middle + 1; i < text.Length; i++)
            {
                if (IsWhitespace(text[i]))
                {
                    return i;
                }

                if (IsSentenceEnd(text[i]) && i + 1 < text.Length)
                {
                    return i + 1;
                }
            }

            return middle;
        }

        private static bool IsSentenceEnd(char value)
        {
            return value == '.' || value == '!' || value == '?' || value == ';' || value == ':';
        }

        private static bool IsWhitespace(char value)
        {
            return value == ' ' || value == '\r' || value == '\n' || value == '\t';
        }

        private int GetRoundLength(string text, int offset)
        {
            int maximumTextLength = Language.MaximumTextLength;
            int remaining = text.Length - offset;
            if (remaining <= maximumTextLength)
            {
                return remaining;
            }

            int maximumEnd = offset + maximumTextLength;
            int minimumSentenceEnd = offset + (maximumTextLength / 2);
            for (int i = maximumEnd - 1; i >= minimumSentenceEnd; i--)
            {
                if (IsSentenceEnd(text[i])
                    && (i + 1 >= text.Length || IsWhitespace(text[i + 1])))
                {
                    return (i - offset) + 1;
                }
            }

            for (int i = maximumEnd - 1; i > offset; i--)
            {
                if (IsWhitespace(text[i]))
                {
                    return i - offset;
                }
            }

            return maximumTextLength;
        }

        private void AddSegment(string text, string[] segments, ref int count)
        {
            _phonemes.Clear();
            bool prepared = _frontend.TryPrepare(text, _phonemes);
            if (prepared)
            {
                if (_phonemes.Count > 0)
                {
                    segments[count++] = text;
                }

                return;
            }

            int split = FindRetrySplit(text);
            if (split <= 0 || split >= text.Length)
            {
                throw new ArgumentException();
            }

            string first = text.Substring(0, split).Trim();
            string second = text.Substring(split).Trim();
            if (first.Length == 0 || second.Length == 0)
            {
                throw new ArgumentException();
            }

            AddSegment(first, segments, ref count);
            AddSegment(second, segments, ref count);
        }
    }
}
