// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Modbus.Util
{
    /// <summary>
    /// Provides utility methods for converting boolean values to and from Modbus-specific formats.
    /// </summary>
    public static class BoolConverter
    {
        /// <summary>
        /// Converts an unsigned 32-bit integer value to a boolean value.
        /// </summary>
        /// <param name="value">The unsigned 32-bit integer value to convert.</param>
        /// <returns>A boolean value.</returns>
        public static bool From(uint value)
        {
            return value != 0;
        }

        /// <summary>
        /// Converts an integer value to a boolean value.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
        /// <returns>A boolean value.</returns>
        public static bool From(int value)
        {
            return value != 0;
        }

        /// <summary>
        /// Converts an unsigned 16-bit integer to a boolean value.
        /// </summary>
        /// <param name="value">The unsigned 16-bit integer to convert.</param>
        /// <returns>A boolean value.</returns>
        public static bool From(ushort value)
        {
            return value != 0;
        }

        /// <summary>
        /// Converts a short value to a boolean value.
        /// </summary>
        /// <param name="value">The short value to convert.</param>
        /// <returns>A boolean value.</returns>
        public static bool From(short value)
        {
            return value != 0;
        }

        /// <summary>
        /// Converts a boolean value to an unsigned 16-bit integer.
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
        /// <returns>An unsigned 16-bit integer.</returns>
        public static ushort ToUInt16(bool value)
        {
            if (value)
            {
                return 1;
            }
            else
            {
                return ushort.MinValue;
            }
        }

        /// <summary>
        /// Converts a boolean value to a short value.
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
        /// <returns>A short value.</returns>
        public static short ToInt16(bool value)
        {
            if (value)
            {
                return 1;
            }
            else
            {
                return short.MinValue;
            }
        }
    }
}
