﻿//
// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace Iot.Device.SPS30.Utils
{
    /// <summary>
    /// The SPS30 does everything big-endian. We need a little helper class to help us with conversion in case BitConverter is little endian on the current platform.
    /// </summary>
    public static class BigEndianBitConverter
    {
        /// <summary>
        /// Read a float using big endianness
        /// </summary>
        public static float ToSingle(byte[] value, int startIndex)
        {
            return BitConverter.IsLittleEndian ? BitConverter.ToSingle(new byte[] { value[startIndex + 3], value[startIndex + 2], value[startIndex + 1], value[startIndex] }, 0) : BitConverter.ToSingle(value, startIndex);
        }

        /// <summary>
        /// Read an uint using big endianness
        /// </summary>
        public static uint ToUInt32(byte[] value, int startIndex)
        {
            return BitConverter.IsLittleEndian ? BitConverter.ToUInt32(new byte[] { value[startIndex + 3], value[startIndex + 2], value[startIndex + 1], value[startIndex] }, 0) : BitConverter.ToUInt32(value, startIndex);
        }

        /// <summary>
        /// Read a ushort using big endianness
        /// </summary>
        public static ushort ToUInt16(byte[] value, int startIndex)
        {
            return BitConverter.IsLittleEndian ? BitConverter.ToUInt16(new byte[] { value[startIndex + 1], value[startIndex] }, 0) : BitConverter.ToUInt16(value, startIndex);
        }

        /// <summary>
        /// Convert a uint to bytes using big endianness
        /// </summary>
        public static byte[] GetBytes(uint value)
        {
            var data = BitConverter.GetBytes(value);
            return BitConverter.IsLittleEndian ? new byte[] { data[3], data[2], data[1], data[0] } : data;
        }
    }
}
