// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Text2Speech
{
    /// <summary>
    /// Normalizes text and emits language-specific phonemes into a bounded reusable buffer.
    /// </summary>
    public interface ITtsLanguageFrontend
    {
        /// <summary>
        /// Converts one text round into phonemes.
        /// </summary>
        /// <param name="text">The text within the owning language's per-round limit.</param>
        /// <param name="phonemes">The empty destination phoneme buffer.</param>
        /// <returns>
        /// <see langword="true" /> when all phonemes fit; otherwise, <see langword="false" />.
        /// </returns>
        bool TryPrepare(string text, TtsPhonemeBuffer phonemes);
    }
}
