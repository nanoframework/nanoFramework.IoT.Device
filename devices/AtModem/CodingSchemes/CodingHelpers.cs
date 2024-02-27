// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.AtModem.CodingSchemes
{
    /// <summary>
    /// Internal helpers to convert HEX strings to byte arrays.
    /// </summary>
    internal class CodingHelpers
    {
        /// <summary>
        /// Converts a hexadecimal string to a byte array.
        /// </summary>
        /// <param name="hex">The hexadecimal string to convert.</param>
        /// <returns>A byte array representing the hexadecimal string.</returns>
        public static byte[] StringToByteArray(string hex)
        {
            // nanoFramework does not support System.Linq
            ////return Enumerable.Range(0, hex.Length)
            ////                 .Where(x => x % 2 == 0)
            ////                 .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            ////                 .ToArray();

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                byte b = Convert.ToByte(hex.Substring(i, 2), 16);
                bytes[i / 2] = b;
            }

            return bytes;
        }

        /// <summary>
        /// Converts a byte array to a hexadecimal string.
        /// </summary>
        /// <param name="bytes">The byte aray to convert.</param>
        /// <returns>The hexadecimal string.</returns>
        public static string ByteArrayToStringHex(byte[] bytes)
        {
            string hex = string.Empty;
            foreach (byte b in bytes)
            {
                hex += b.ToString("X2");
            }

            return hex;
        }
    }
}
