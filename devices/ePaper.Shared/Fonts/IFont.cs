namespace Iot.Device.ePaper.Shared.Fonts
{
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