// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Text2Speech
{
    /// <summary>
    /// Provides the built-in compact English spelling and number frontend.
    /// </summary>
    public sealed class EnglishTtsLanguage : ITtsLanguage
    {
        /// <summary>
        /// The maximum English input length before number expansion.
        /// </summary>
        public const int DefaultMaximumTextLength = 96;

        private static EnglishTtsLanguage _sharedInstance;

        /// <summary>
        /// Gets the shared stateless English language definition.
        /// </summary>
        /// <value>The built-in English language.</value>
        public static EnglishTtsLanguage Instance
        {
            get
            {
                if (_sharedInstance == null)
                {
                    _sharedInstance = new EnglishTtsLanguage();
                }

                return _sharedInstance;
            }
        }

        /// <summary>
        /// Gets the language display name.
        /// </summary>
        /// <value><c>English</c>.</value>
        public string Name => "English";

        /// <summary>
        /// Gets the maximum English input length for one synthesis round.
        /// </summary>
        /// <value><see cref="DefaultMaximumTextLength" />.</value>
        public int MaximumTextLength => DefaultMaximumTextLength;

        /// <summary>
        /// Creates an independent reusable English frontend.
        /// </summary>
        /// <returns>A new English text frontend.</returns>
        public ITtsLanguageFrontend CreateFrontend()
        {
            return new EnglishTtsFrontend();
        }
    }
}
