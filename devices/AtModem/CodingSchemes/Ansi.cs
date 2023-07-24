// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.AtModem.CodingSchemes
{
    /// <summary>
    /// Encode / decode ANSI 8-bit strings.
    /// </summary>
    public class Ansi
    {
        /// <summary>
        /// Data coding scheme code for ANSI 8-bit.
        /// </summary>
        public const byte DataCodingSchemeCode = 0xF4;

        /// <summary>
        /// Encode to ANSI 8-bit.
        /// </summary>
        /// <param name="input">String to encode.</param>
        /// <returns>ANSI encoded string.</returns>
        public static string Encode(string input)
        {
            // nanoFramework do not have Encoding.ASCII.GetBytes(string)
            ////byte[] bytes = Encoding.ASCII.GetBytes(input);
            ////return BitConverter.ToString(bytes).Replace("-", "");
            string strRet = string.Empty;
            for (int i = 0; i < input.Length; i++)
            {
                strRet += ((int)input[i]).ToString("X2");
            }

            return strRet;
        }

        /// <summary>
        /// Decode from ANSI 8-bit.
        /// </summary>
        /// <param name="input">ANSI encoded string.</param>
        /// <returns>Decoded string.</returns>
        public static string Decode(string input)
        {
            byte[] bytes = CodingHelpers.StringToByteArray(input);
            
            // nanoFramework do not have Encoding.ASCII.GetString(byte[])
            ////return Encoding.ASCII.GetString(bytes);
            string strRet = string.Empty;
            for (int i = 0; i < bytes.Length; i++)
            {
                strRet += (char)bytes[i];
            }

            return strRet;
        }
    }
}
