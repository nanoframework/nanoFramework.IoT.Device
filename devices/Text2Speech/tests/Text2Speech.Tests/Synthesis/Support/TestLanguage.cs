// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Text2Speech.Tests
{
    /// <summary>
    /// Provides a minimal language definition for frontend extension tests.
    /// </summary>
    internal sealed class TestLanguage : ITtsLanguage
    {
        internal static readonly TtsPhoneme Tone = new TtsPhoneme(
            TtsPhonemeType.Voiced,
            730,
            1090,
            2440,
            6,
            5,
            2,
            45,
            50,
            false,
            0,
            0,
            0);

        /// <summary>
        /// Gets the test language name.
        /// </summary>
        /// <value><c>Test</c>.</value>
        public string Name => "Test";

        /// <summary>
        /// Gets the deliberately small test input limit.
        /// </summary>
        /// <value>Four characters.</value>
        public int MaximumTextLength => 4;

        /// <summary>
        /// Creates an independent test frontend.
        /// </summary>
        /// <returns>A new test frontend.</returns>
        public ITtsLanguageFrontend CreateFrontend()
        {
            return new TestLanguageFrontend();
        }
    }
}
