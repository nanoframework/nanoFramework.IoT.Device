// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Si7021
{
    /// <summary>
    /// Extensions for <see cref="Si7021"/> class.
    /// </summary>
    static public class Extensions
    {
        /// <summary>
        /// Returns a string representation of a byte array. Hexadecimal formated.
        /// </summary>
        /// <param name="array">The <see cref="byte"/>[] to convert to a <see cref="string"/>.</param>
        /// <returns>A <see cref="string"/> with the representation of <paramref name="array"/>.</returns>
        public static string AsText(this byte[] array)
        {
            string output = "";

            foreach(var item in array)
            {
                output += item.ToString("X2");
            }

            return output;
        }
    }
}
