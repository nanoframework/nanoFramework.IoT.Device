// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Drawing;

namespace Iot.Device.Graphics
{
    /// <summary>
    /// Font Extensions for nanoFramework
    /// </summary>
    public static class FontExtensions
    {
        /// <summary>
        /// Checks if it Starts With
        /// </summary>
        /// <param name="spanChar">The Span<char></param>
        /// <param name="toSearch">The string to search</param>
        /// <returns>True or False</returns>
        internal static bool StartsWith(this Span<char> spanChar, string toSearch)
        {
            bool found = true;
            for (int i = 0; i < toSearch.Length; i++)
            {
                if (spanChar[i] != toSearch[i])
                {
                    found = false;
                    break;
                }
            }

            return found;
        }

        internal static int CompareTo(this Span<char> spanChar, string toSearch)
        {
            if (spanChar.StartsWith(toSearch) && spanChar.Length == toSearch.Length)
            {
                return 0;
            }

            return spanChar.Length > toSearch.Length ? -1 : 1;
        }

        /// <summary>
        /// Tries to get the value from an integer
        /// </summary>
        /// <param name="table">The table</param>
        /// <param name="character">The character</param>
        /// <param name="index">The index</param>
        /// <returns>True or False</returns>
        public static bool TryGetValue(this Hashtable table, int character, out int index)
        {
            if (table[character] != null)
            {
                index = (int)table[character];
                return true;
            }

            index = -1;
            return false;
        }

        /// <summary>
        /// Tries to get the value from an index
        /// </summary>
        /// <param name="table">The table</param>
        /// <param name="index">The index</param>
        /// <param name="data">The data</param>
        /// <returns>True or False</returns>
        public static bool TryGetValue(this Hashtable table, int index, out byte[]? data)
        {
            if (table[index] != null)
            {
                data = (byte[])table[index];
                return true;
            }

            data = null;
            return false;
        }

        /// <summary>
        /// Converts color to hex
        /// </summary>
        /// <param name="color">The Color</param>
        /// <returns>The color as hex</returns>
        public static string ToHex(this Color color)
        {
            return $"{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}
