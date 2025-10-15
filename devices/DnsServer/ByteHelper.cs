// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.DnsServer
{
    /// <summary>
    /// Helper class for byte conversions, especially for working with network byte order (big-endian).
    /// </summary>
    public static class ByteHelper
    {
        /// <summary>
        /// Reads a 16-bit unsigned integer from a byte array in network byte order (big-endian).
        /// </summary>
        /// <param name="data">The source byte array.</param>
        /// <param name="offset">The offset into the byte array.</param>
        /// <returns>A 16-bit unsigned integer in host byte order.</returns>
        public static ushort ReadUInt16NetworkOrder(
            byte[] data,
            int offset)
        {
            if (offset < 0 || offset > data.Length - 2)
            {
                throw new ArgumentOutOfRangeException();
            }

            // Network byte order is big-endian (most significant byte first)
            return (ushort)((data[offset] << 8) | data[offset + 1]);
        }

        /// <summary>
        /// Writes a 16-bit unsigned integer to a byte array in network byte order (big-endian).
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="data">The destination byte array.</param>
        /// <param name="offset">The offset into the byte array.</param>
        public static void WriteUInt16NetworkOrder(
            ushort value,
            byte[] data,
            int offset)
        {
            if (offset < 0 || offset > data.Length - 2)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            // Network byte order is big-endian (most significant byte first)
            data[offset] = (byte)(value >> 8);
            data[offset + 1] = (byte)(value & 0xFF);
        }

        /// <summary>
        /// Reads a 32-bit unsigned integer from a byte array in network byte order (big-endian).
        /// </summary>
        /// <param name="data">The source byte array.</param>
        /// <param name="offset">The offset into the byte array.</param>
        /// <returns>A 32-bit unsigned integer in host byte order.</returns>
        public static uint ReadUInt32NetworkOrder(
            byte[] data,
            int offset)
        {
            if (offset < 0 || offset > data.Length - 4)
            {
                throw new ArgumentOutOfRangeException();
            }

            return (uint)((data[offset] << 24) |
                          (data[offset + 1] << 16) |
                          (data[offset + 2] << 8) |
                           data[offset + 3]);
        }

        /// <summary>
        /// Writes a 32-bit unsigned integer to a byte array in network byte order (big-endian).
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="data">The destination byte array.</param>
        /// <param name="offset">The offset into the byte array.</param>
        public static void WriteUInt32NetworkOrder(
            uint value,
            byte[] data,
            int offset)
        {
            if (offset < 0 || offset > data.Length - 4)
            {
                throw new ArgumentOutOfRangeException();
            }

            data[offset] = (byte)(value >> 24);
            data[offset + 1] = (byte)((value >> 16) & 0xFF);
            data[offset + 2] = (byte)((value >> 8) & 0xFF);
            data[offset + 3] = (byte)(value & 0xFF);
        }
    }
}