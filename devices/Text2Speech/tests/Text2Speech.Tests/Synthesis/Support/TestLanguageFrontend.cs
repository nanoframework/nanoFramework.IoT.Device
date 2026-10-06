// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Text2Speech.Tests
{
    /// <summary>
    /// Converts test characters into a fixed voiced phoneme.
    /// </summary>
    internal sealed class TestLanguageFrontend : ITtsLanguageFrontend
    {
        /// <summary>
        /// Converts supported test characters into phonemes.
        /// </summary>
        /// <param name="text">The test text.</param>
        /// <param name="phonemes">The destination phoneme buffer.</param>
        /// <returns><see langword="true" /> when all phonemes fit.</returns>
        public bool TryPrepare(string text, TtsPhonemeBuffer phonemes)
        {
            for (int i = 0; i < text.Length; i++)
            {
                TtsPhoneme phoneme = null;
                if (text[i] == 'x')
                {
                    phoneme = TestLanguage.Tone;
                }
                else if (text[i] == 'f')
                {
                    phoneme = TestLanguage.FirstFormantGlide;
                }
                else if (text[i] == 'e')
                {
                    phoneme = TestLanguage.FirstFormantGlideWithExplicitFallbacks;
                }
                else if (text[i] == 's')
                {
                    phoneme = TestLanguage.SecondFormantGlide;
                }
                else if (text[i] == 'n')
                {
                    phoneme = TestLanguage.NyquistFormants;
                }

                if (phoneme != null && !phonemes.TryAdd(phoneme, 0))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
