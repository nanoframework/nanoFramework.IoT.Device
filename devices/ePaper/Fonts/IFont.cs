// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.EPaper.Fonts
{
    /// <summary>
    /// Represents a font that can be used by the graphics library to render text. 
    /// </summary>
    public interface IFont
    {
        /// <summary>
        /// Get the byte array representing the specified character.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <returns>A byte array representing the specified character.</returns>
        byte[] this[char character] { get; }

        /// <summary>
        /// Gets the height of a character.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets the width of a character.
        /// </summary>
        int Width { get; }
    }
}