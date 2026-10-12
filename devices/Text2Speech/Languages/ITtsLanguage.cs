// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Text2Speech
{
    /// <summary>
    /// Creates language-specific text frontends for the shared formant renderer.
    /// </summary>
    public interface ITtsLanguage
    {
        /// <summary>
        /// Gets the display name of the language.
        /// </summary>
        /// <value>The human-readable language name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the maximum input characters accepted by one synthesis round.
        /// </summary>
        /// <value>The deterministic per-round text limit.</value>
        int MaximumTextLength { get; }

        /// <summary>
        /// Creates an independent reusable frontend for one synthesizer or segmenter.
        /// </summary>
        /// <returns>A language-specific text frontend.</returns>
        ITtsLanguageFrontend CreateFrontend();
    }
}
