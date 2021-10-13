using System;

namespace Iot.Device.Ssd13xx
{
    /// <summary>
    /// Base class for font implementation.
    /// </summary>
    public abstract class IFont
    {
        /// <summary>
        /// Font width.
        /// </summary>
        public virtual byte Width { get; private set; }

        /// <summary>
        /// Font height.
        /// </summary>
        public virtual byte Height { get; private set; }

        /// <summary>
		///     Get the binary representation of the ASCII character from the font table.
		/// </summary>
		/// <param name="character">Character to look up.</param>
		/// <returns>Array of bytes representing the binary bit pattern of the character.</returns>
		public abstract byte[] this[char character] { get; }
    }
}
