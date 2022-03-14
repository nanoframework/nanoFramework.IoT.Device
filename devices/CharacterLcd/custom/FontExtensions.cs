// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Drawing;

namespace Iot.Device.Graphics
{
    internal static class FontExtensions
    {
        internal static bool StartsWith(this SpanChar spanCar, string toSearch)
        {
            bool found = true;
            for (int i = 0; i < toSearch.Length; i++)
            {
                if (spanCar[i] != toSearch[i])
                {
                    found = false;
                    break;
                }
            }

            return found;
        }

        internal static int CompareTo(this SpanChar spanCar, string toSearch)
        {
            if (spanCar.StartsWith(toSearch) && spanCar.Length == toSearch.Length)
            {
                return 0;
            }

            return spanCar.Length > toSearch.Length ? -1 : 1;
        }

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

        public static string ToHex(this Color color)
        {
            return $"{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}
