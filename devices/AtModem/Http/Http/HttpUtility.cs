//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System.Text;

namespace System.Web
{
    /// <summary>
    /// Utilities to encode and decode URLs.
    /// </summary>
    public class HttpUtility
    {
        /// <summary>
        /// Encodes a URL string.
        /// </summary>
        /// <param name="str">The text to encode.</param>
        /// <returns>An encoded string.</returns>
        public static string UrlEncode(string str)
        {
            if ((str == null) || (str == string.Empty))
            {
                return string.Empty;
            }

            return new string(Encoding.UTF8.GetChars(UrlEncodeToBytes(str, Encoding.UTF8)));
        }

        /// <summary>
        /// Encodes a URL string using the specified encoding object.
        /// </summary>
        /// <param name="str">The text to encode.</param>
        /// <param name="e">The <see cref="Encoding"/> object that specifies the encoding scheme.</param>
        /// <returns>An encoded string.</returns>
        public static string UrlEncode(
            string str,
            Encoding e)
        {
            if ((str == null) || (str == string.Empty))
            {
                return string.Empty;
            }

            return new string(e.GetChars(UrlEncodeToBytes(str, e)));
        }

        /// <summary>
        /// Converts a byte array into a URL-encoded string, starting at the specified position in the array and continuing for the specified number of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to encode.</param>
        /// <param name="offset">The position in the byte array at which to begin encoding.</param>
        /// <param name="count">The number of bytes to encode.</param>
        /// <returns>An encoded string.</returns>
        public static string UrlEncode(
            byte[] bytes,
            int offset,
            int count)
        {
            return new string(Encoding.UTF8.GetChars(UrlEncodeBytesToBytesInternal(bytes, offset, count, false)));
        }

        /// <summary>
        /// Converts a byte array into an encoded URL string.
        /// </summary>
        /// <param name="bytes">The array of bytes to encode.</param>
        /// <returns>An encoded string.</returns>
        public static string UrlEncode(byte[] bytes) => UrlEncode(bytes, 0, bytes.Length);

        /// <summary>
        /// Converts a string into a URL-encoded array of bytes using the specified encoding object.
        /// </summary>
        /// <param name="str">The string to encode.</param>
        /// <param name="e">The <see cref="Encoding"/> that specifies the encoding scheme.</param>
        /// <returns>An encoded array of bytes.</returns>
        public static byte[] UrlEncodeToBytes(string str, Encoding e)
        {
            if (str == null)
            {
                return null;
            }

            var bytes = e.GetBytes(str);

            return UrlEncodeBytesToBytesInternal(bytes, 0, bytes.Length, false);
        }

        /// <summary>
        /// Converts a URL-encoded array of bytes into a decoded array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to encode</param>
        /// <returns>A decoded array of bytes.</returns>
        public static byte[] UrlEncodeToBytes(byte[] bytes) => UrlEncodeBytesToBytesInternal(bytes, 0, bytes.Length, false);

        /// <summary>
        /// Converts an array of bytes into a URL-encoded array of bytes, starting at the specified position in the array and continuing for the specified number of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to encode</param>
        /// <param name="offset">The position in the byte array at which to begin encoding</param>
        /// <param name="count">The number of bytes to encode</param>
        /// <returns>An encoded array of bytes.</returns>		
        public static byte[] UrlEncodeToBytes(
            byte[] bytes,
            int offset,
            int count) => UrlEncodeBytesToBytesInternal(
                bytes,
                offset,
                count,
                false);

        private static byte[] UrlEncodeBytesToBytesInternal(
            byte[] bytes,
            int offset,
            int count,
            bool alwaysCreateReturnValue)
        {
            var num = 0;
            var num2 = 0;

            for (var i = 0; i < count; i++)
            {
                var ch = (char)bytes[offset + i];

                if (ch == ' ')
                {
                    num++;
                }
                else if (!IsSafe(ch))
                {
                    num2++;
                }
            }
            if ((!alwaysCreateReturnValue && (num == 0))
                && (num2 == 0))
            {
                return bytes;
            }

            var buffer = new byte[count + (num2 * 2)];
            var num4 = 0;

            for (var j = 0; j < count; j++)
            {
                var num6 = bytes[offset + j];
                var ch2 = (char)num6;

                if (IsSafe(ch2))
                {
                    buffer[num4++] = num6;
                }
                else if (ch2 == ' ')
                {
                    buffer[num4++] = 0x2b;
                }
                else
                {
                    buffer[num4++] = 0x25;
                    buffer[num4++] = (byte)IntToHex((num6 >> 4) & 15);
                    buffer[num4++] = (byte)IntToHex(num6 & 15);
                }
            }

            return buffer;
        }

        private static char IntToHex(int n)
        {
            if (n <= 9)
            {
                return (char)(n + 0x30);
            }

            return (char)((n - 10) + 0x41);
        }

        private static bool IsSafe(char ch)
        {
            if ((((ch >= 'a') && (ch <= 'z')) || ((ch >= 'A') && (ch <= 'Z')))
                || ((ch >= '0') && (ch <= '9')))
            {
                return true;
            }

            switch (ch)
            {
                case '(':
                case ')':
                case '*':
                case '-':
                case '.':
                case '_':
                case '!':
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Converts a string that has been encoded for transmission in a URL into a decoded string.
        /// </summary>
        /// <param name="str">The string to decode./param>
        /// <returns>The decoded URL</returns>
        public static string UrlDecode(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            var data = Encoding.UTF8.GetBytes(str);

            return new string(Encoding.UTF8.GetChars(UrlDecodeToBytes(data, 0, data.Length)));
        }

        /// <summary>
        /// Converts a URL-encoded string into a decoded string, using the specified encoding object.
        /// </summary>
        /// <param name="str">The string to decode.</param>
        /// <param name="e">The <see cref="Encoding"/> that specifies the decoding scheme.</param>
        /// <returns>A decoded string.</returns>
        public static string UrlDecode(string str, Encoding e)
        {
            if ((str == null) || (str == string.Empty))
            {
                return string.Empty;
            }

            var data = e.GetBytes(str);
            return new string(e.GetChars(UrlDecodeToBytes(data, 0, data.Length)));
        }

        /// <summary>
        /// Converts a URL-encoded byte array into a decoded string using the specified encoding object, starting at the specified position in the array, and continuing for the specified number of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to decode.</param>
        /// <param name="offset">The position in the byte to begin decoding</param>
        /// <param name="count">The number of bytes to decode.</param>
        /// <param name="e">The <see cref="Encoding"/> object that specifies the encoding scheme.</param>
        /// <returns>A decoded string.</returns>
        public static string UrlDecode(
            byte[] bytes,
            int offset,
            int count,
            Encoding e) => new(e.GetChars(UrlDecodeToBytes(
                bytes,
                offset,
                count)));

        /// <summary>
        /// Converts a URL-encoded byte array into a decoded string using the specified decoding object.
        /// </summary>
        /// <param name="bytes">The array of bytes to decode.</param>
        /// <param name="e">The <see cref="Encoding"/> object that specifies the encoding scheme.</param>
        /// <returns>A decoded string.</returns>
        public static string UrlDecode(
            byte[] bytes,
            Encoding e) => new string(e.GetChars(UrlDecodeToBytes(
                bytes,
                0,
                bytes.Length)));

        /// <summary>
        /// Converts a URL-encoded array of bytes into a decoded array of bytes, starting at the specified position in the array and continuing for the specified number of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to decode.</param>
        /// <param name="offset">The position in the byte array at which to begin decoding.</param>
        /// <param name="count">The number of bytes to decode.</param>
        /// <returns>A decoded array of bytes.</returns>
        public static byte[] UrlDecodeToBytes(
            byte[] bytes,
            int offset,
            int count)
        {
            var length = 0;
            var sourceArray = new byte[count];

            for (var i = 0; i < count; i++)
            {
                var index = offset + i;
                var num4 = bytes[index];

                if (num4 == 0x2b)
                {
                    num4 = 0x20;
                }
                else if ((num4 == 0x25)
                         && (i < (count - 2)))
                {
                    var num5 = HexToInt((char)bytes[index + 1]);
                    var num6 = HexToInt((char)bytes[index + 2]);

                    if ((num5 >= 0)
                        && (num6 >= 0))
                    {
                        num4 = (byte)((num5 << 4) | num6);
                        i += 2;
                    }
                }
                sourceArray[length++] = num4;
            }

            if (length < sourceArray.Length)
            {
                var destinationArray = new byte[length];
                Array.Copy(sourceArray, destinationArray, length);
                sourceArray = destinationArray;
            }

            return sourceArray;
        }

        /// <summary>
        /// Converts a URL-encoded string into a decoded array of bytes using the specified decoding object.
        /// </summary>
        /// <param name="str">The string to encode.</param>
        /// <param name="e">The <see cref="Encoding"/> object that specifies the encoding scheme.</param>
        /// <returns>A decoded array of bytes.</returns>
        public static byte[] UrlDecodeToBytes(
            string str,
            Encoding e)
        {
            var data = e.GetBytes(str);
            return UrlDecodeToBytes(data, 0, data.Length);
        }

        /// <summary>
        /// Converts a URL-encoded string into a decoded array of bytes.
        /// </summary>
        /// <param name="str">The string to decode.</param>
        /// <returns>A decoded array of bytes.</returns>
        public static byte[] UrlDecodeToBytes(string str) => UrlDecodeToBytes(
            str,
            Encoding.UTF8);

        /// <summary>
        /// Converts a URL-encoded array of bytes into a decoded array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to decode.</param>
        /// <returns>A decoded array of bytes.</returns>
        public static byte[] UrlDecodeToBytes(byte[] bytes) => UrlDecodeToBytes(
            bytes,
            0,
            bytes.Length);

        /// <summary>
        /// Get the int value of a char.
        /// </summary>
        /// <param name="h">The <see cref="char"/> to convert.</param>
        /// <returns>The <see cref="int"/> value of the <see cref="char"/>.</returns>
        public static int HexToInt(char h)
        {
            if ((h >= '0') &&
                (h <= '9'))
            {
                return (h - '0');
            }

            if ((h >= 'a') &&
                (h <= 'f'))
            {
                return ((h - 'a') + 10);
            }

            if ((h >= 'A') &&
                (h <= 'F'))
            {
                return ((h - 'A') + 10);
            }

            return -1;
        }
    }
}