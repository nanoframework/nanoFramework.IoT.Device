// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Text2Speech
{
    /// <summary>
    /// Maps a spelling pattern to one or more phonemes.
    /// </summary>
    internal sealed class Pattern
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Pattern" /> class.
        /// </summary>
        /// <param name="text">The lowercase spelling pattern.</param>
        /// <param name="first">The first emitted phoneme.</param>
        /// <param name="second">The optional second emitted phoneme.</param>
        /// <param name="third">The optional third emitted phoneme.</param>
        public Pattern(string text, TtsPhoneme first, TtsPhoneme second = null, TtsPhoneme third = null)
        {
            Text = text;
            First = first;
            Second = second;
            Third = third;
        }

        /// <summary>
        /// Gets the lowercase spelling pattern.
        /// </summary>
        /// <value>The text matched by the parser.</value>
        public string Text { get; }

        /// <summary>
        /// Gets the first emitted phoneme.
        /// </summary>
        /// <value>The required first phoneme.</value>
        public TtsPhoneme First { get; }

        /// <summary>
        /// Gets the optional second emitted phoneme.
        /// </summary>
        /// <value>The second phoneme, or <see langword="null" />.</value>
        public TtsPhoneme Second { get; }

        /// <summary>
        /// Gets the optional third emitted phoneme.
        /// </summary>
        /// <value>The third phoneme, or <see langword="null" />.</value>
        public TtsPhoneme Third { get; }
    }
}
