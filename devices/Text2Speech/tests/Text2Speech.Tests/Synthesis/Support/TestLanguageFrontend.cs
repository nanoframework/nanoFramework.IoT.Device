// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

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
                if (text[i] == 'x' && !phonemes.TryAdd(TestLanguage.Tone, 0))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
