// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace IoT.Device.AtModem.CodingSchemes
{
    /// <summary>
    /// Encode / decode UCS2 strings
    /// Unicode 16 bits.
    /// </summary>
    public class UCS2
    {
        /// <summary>
        /// Data coding scheme code for UCS2.
        /// </summary>
        public const byte DataCodingSchemeCode = 0x08;

        /// <summary>
        /// Encode to UCS2.
        /// </summary>
        /// <param name="input">String to encode.</param>
        /// <returns>UCS2 encoded string.</returns>
        public static string Encode(string input)
        {
            // not implemented in nanoFramework, for later on.
            ////byte[] bytes = Encoding.BigEndianUnicode.GetBytes(input);
            ////return BitConverter.ToString(bytes).Replace("-", "");
            throw new NotImplementedException();
        }

        /// <summary>
        /// Decode from UCS2.
        /// </summary>
        /// <param name="input">UCS2 encoded string.</param>
        /// <returns>Decoded string.</returns>
        public static string Decode(string input)
        {
            // not implemented in nanoFramework, for later on.
            ////IEnumerable<byte> bytes = CodingHelpers.StringToByteArray(input);
            ////return Encoding.BigEndianUnicode.GetString(bytes.ToArray());
            throw new NotImplementedException();
        }
    }
}
