// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Iot.Device.AtModem.CodingSchemes
{
    internal class ConvertHelper
    {
        /// <summary>
        /// Converts ASCII to String.
        /// </summary>
        /// <param name="bytes">The raw bytes.</param>
        /// <returns>The string converted from ASCII.</returns>
        public static string ConvertAsciiToString(ArrayList bytes)
        {
            string result = string.Empty;
            for (int i = 0; i < bytes.Count; i++)
            {
                // Boxing/Unboxing paradox as we need to convert fist to byte and then to char
                result += (char)((byte)bytes[i]);
            }

            return result;
        }
    }
}
