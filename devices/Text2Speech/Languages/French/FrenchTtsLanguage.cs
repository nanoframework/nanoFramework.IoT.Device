// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Text2Speech
{
    /// <summary>
    /// Defines the built-in French language frontend.
    /// </summary>
    public sealed class FrenchTtsLanguage : ITtsLanguage
    {
        /// <summary>
        /// The maximum source-text length accepted by one synthesis round.
        /// </summary>
        public const int DefaultMaximumTextLength = 96;

        private static FrenchTtsLanguage _instance;

        /// <summary>
        /// Gets the shared immutable French language definition.
        /// </summary>
        public static FrenchTtsLanguage Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FrenchTtsLanguage();
                }

                return _instance;
            }
        }

        /// <inheritdoc />
        public string Name => "French";

        /// <inheritdoc />
        public int MaximumTextLength => DefaultMaximumTextLength;

        /// <inheritdoc />
        public ITtsLanguageFrontend CreateFrontend()
        {
            return new FrenchTtsFrontend();
        }
    }
}
