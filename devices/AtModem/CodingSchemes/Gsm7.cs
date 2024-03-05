// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text;

namespace Iot.Device.AtModem.CodingSchemes
{
    /// <summary>
    /// Encode / decode GSM 7-bit strings.
    /// </summary>
    public class Gsm7
    {
        /// <summary>
        /// GSM 7-bit Data Coding Scheme code.
        /// </summary>
        public const byte DataCodingSchemeCode = 0x00;

        /// <summary>
        /// Encodes a string to GSM 7-bit format.
        /// </summary>
        /// <param name="text">The string to encode.</param>
        /// <returns>The GSM 7-bit encoded string.</returns>
        public static string Encode(string text)
        {
            byte[] data = new byte[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                data[i] = (byte)text[i];
            }

            return Encode(data);
        }

        /// <summary>
        /// Encodes a byte array to GSM 7-bit format.
        /// </summary>
        /// <param name="data">The byte array to encode.</param>
        /// <returns>The GSM 7-bit encoded string.</returns>
        public static string Encode(byte[] data)
        {
            byte[] textBytes = Reverse(data);

            bool[] bits = new bool[textBytes.Length * 7];
            for (int i = 0; i < textBytes.Length; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    bits[(i * 7) + j] = (textBytes[i] & (0x40 >> j)) != 0;
                }
            }

            byte[] octets = new byte[(int)Math.Ceiling(bits.Length / 8.0)];
            int offset = (octets.Length * 8) - bits.Length;
            int bitShift = 0;
            for (int i = bits.Length - 1; i >= 0; i--)
            {
                octets[(i + offset) / 8] |= (byte)(bits[i] ? 0x01 << bitShift : 0x00);
                bitShift++;
                bitShift %= 8;
            }

            byte[] octetsReversed = Reverse(octets);

            return CodingHelpers.ByteArrayToStringHex(octetsReversed);
        }

        /// <summary>
        /// Decodes a GSM 7-bit encoded string.
        /// </summary>
        /// <param name="strGsm7bit">The GSM 7-bit encoded string.</param>
        /// <returns>The decoded string.</returns>
        public static string Decode(string strGsm7bit)
        {
            return Decode(CodingHelpers.StringToByteArray(strGsm7bit));
        }

        /// <summary>
        /// Decodes a GSM 7-bit encoded byte array.
        /// </summary>
        /// <param name="data">The GSM 7-bit encoded byte array.</param>
        /// <returns>The decoded string.</returns>
        public static string Decode(byte[] data)
        {
            byte[] octets = Reverse(data);

            bool[] bits = new bool[octets.Length * 8];
            for (int i = 0; i < octets.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    bits[(i * 8) + j] = (octets[i] & (0x80 >> j)) != 0;
                }
            }

            byte[] septets = new byte[(int)Math.Floor(bits.Length / 7.0)];
            int offset = bits.Length - (septets.Length * 7);
            int bitShift = 0;
            for (int i = bits.Length - 1; i >= 0; i--)
            {
                septets[(i - offset) / 7] |= (byte)(bits[i] ? 0x01 << bitShift : 0x00);
                bitShift++;
                bitShift %= 7;
            }

            septets = Reverse(septets);

            string str = string.Empty;
            for (int i = 0; i < septets.Length; i++)
            {
                str += (char)septets[i];
            }

            return str;
        }

        private static byte[] Reverse(byte[] data)
        {
            byte[] reversed = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                reversed[i] = data[data.Length - i - 1];
            }

            return reversed;
        }
    }
}
