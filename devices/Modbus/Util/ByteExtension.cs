// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System
{
    /// <summary>
    /// Provides extension methods for the <see cref="Byte"/> class.
    /// </summary>
    internal static class ByteExtension
    {
        internal static readonly bool IsLittleEndian = false;

        /// <summary>
        /// Reverses the order of bytes in the specified byte array.
        /// </summary>
        /// <param name="bytes">The byte array to reverse.</param>
        public static void JudgReverse(this byte[] bytes)
        {
            if (IsLittleEndian != BitConverter.IsLittleEndian &&
                bytes != null && bytes.Length != 0)
            {
                int i = 0;
                int j = bytes.Length - 1;

                while (i < j)
                {
                    byte temp = bytes[i];
                    bytes[i] = bytes[j];
                    bytes[j] = temp;
                    i++;
                    j--;
                }
            }
        }
    }
}
