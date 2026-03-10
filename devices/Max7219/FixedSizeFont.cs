// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Max7219
{
    /// <summary>
    /// Implementation of a <see cref="IFont"/> that uses a common array for all characters.
    /// The number of bytes per character is constant and zero values between the characters are trimmed.
    /// </summary>
    public class FixedSizeFont : IFont
    {
        private readonly byte[] _data;
        private readonly int _bytesPerCharacter;
        private readonly byte[] _space;

        /// <summary>
        /// Constructs FixedSizeFont instance
        /// </summary>
        /// <param name="bytesPerCharacter">number of bytes per character</param>
        /// <param name="data">Font data</param>
        /// <param name="spaceWidth">Space width</param>
        public FixedSizeFont(int bytesPerCharacter, byte[] data, int spaceWidth = 3)
        {
            _data = data;
            _bytesPerCharacter = bytesPerCharacter;
            _space = new byte[spaceWidth];
        }

        /// <summary>
        /// Get character information
        /// </summary>
        public ListByte this[char chr]
        {
            get
            {
                int start = chr * _bytesPerCharacter;
                int end = start + _bytesPerCharacter;
                if (end > _data.Length)
                {
                    return new ListByte(_space); // character is not defined
                }

                if (chr == ' ')
                {
                    return new ListByte(_space);
                }

                // trim the font
                while (start < end && _data[start] == 0)
                {
                    start++;
                }

                while (end > start && _data[end - 1] == 0)
                {
                    end--;
                }

                return new ListByte(new Span<byte>(_data, start, end - start).ToArray());
            }
        }
    }
}
