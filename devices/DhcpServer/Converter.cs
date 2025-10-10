// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net;
using System.Text;

namespace Iot.Device.DhcpServer
{
    internal class Converter
    {
        /// <summary>
        /// Copies <paramref name="source"/> to <paramref name="destination"/> starting at <paramref name="destinationIndex"/>.
        /// </summary>
        /// <param name="source">The source data.</param>
        /// <param name="destination">The destination array.</param>
        /// <param name="destinationIndex">The destinations index.</param>
        /// <returns>The number of bytes copied.</returns>
        public static int CopyTo(byte source, byte[] destination, int destinationIndex)
        {
            destination[destinationIndex] = source;
            return 1;
        }

        /// <inheritdoc cref="CopyTo(byte,byte[],int)"/>
        public static int CopyTo(byte[] source, byte[] destination, int destinationIndex)
        {
            if (destination.Length - destinationIndex < source.Length)
            {
                throw new ArgumentException();
            }

            return CopyTo(source, 0, destination, destinationIndex, source.Length);
        }

        /// <inheritdoc cref="CopyTo(byte,byte[],int)"/>
        public static int CopyTo(IPAddress source, byte[] destination, int destinationIndex) => CopyTo(GetBytes(source), destination, destinationIndex);

        /// <inheritdoc cref="CopyTo(byte,byte[],int)"/>
        public static int CopyTo(uint source, byte[] destination, int destinationIndex) => CopyTo(GetBytes(source), destination, destinationIndex);

        /// <inheritdoc cref="CopyTo(byte,byte[],int)"/>
        public static int CopyTo(ushort source, byte[] destination, int destinationIndex) => CopyTo(GetBytes(source), destination, destinationIndex);

        public static int CopyTo(byte[] source, int sourceIndex, byte[] destination, int destinationIndex, int length)
        {
            Array.Copy(source, sourceIndex, destination, destinationIndex, length);
            return length;
        }

        public static byte[] GetBytes(IPAddress value) => value.GetAddressBytes();

        public static byte[] GetBytes(string value) => Encoding.UTF8.GetBytes(value);

        public static byte[] GetBytes(TimeSpan value) => GetBytes(value > TimeSpan.Zero ? (uint)value.TotalSeconds : 0);

        public static byte[] GetBytes(uint value) => BitConverter.GetBytes(value);

        public static byte[] GetBytes(ushort value) => BitConverter.GetBytes(value);

        public static IPAddress GetIPAddress(byte[] data, int index = 0) => new (GetUInt32(data, index));

        public static string GetString(byte[] data, int index = 0, int length = -1) => Encoding.UTF8.GetString(data, index, length > -1 ? length : data.Length);

        public static TimeSpan GetTimeSpan(byte[] data, int index = 0) => TimeSpan.FromSeconds(GetUInt32(data, index));

        public static ushort GetUInt16(byte[] data, int index = 0) => BitConverter.ToUInt16(data, index);

        public static uint GetUInt32(byte[] data, int index = 0) => BitConverter.ToUInt32(data, index);
    }
}
