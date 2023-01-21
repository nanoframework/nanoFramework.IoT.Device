// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace System.IO
{
    /// <summary>
    /// Extensions on the MemoryStream to facilitate byte stuffing.
    /// </summary>
    public static class MemoryStreamExtensions
    {
        /// <summary>
        /// Helper method that automatically stuffs bytes if required according to the SHDLC protocol.
        /// </summary>
        /// <param name="memoryStream">The stream to write to.</param>
        /// <param name="b">The byte that is potentially stuffed/escaped.</param>
        public static void WriteByteStuffed(this MemoryStream memoryStream, byte b)
        {
            switch (b)
            {
                case 0x7e:
                    memoryStream.Write(new byte[] { 0x7d, 0x5e }, 0, 2);
                    break;
                case 0x7d:
                    memoryStream.Write(new byte[] { 0x7d, 0x5d }, 0, 2);
                    break;
                case 0x11:
                    memoryStream.Write(new byte[] { 0x7d, 0x31 }, 0, 2);
                    break;
                case 0x13:
                    memoryStream.Write(new byte[] { 0x7d, 0x33 }, 0, 2);
                    break;
                default:
                    memoryStream.WriteByte(b);
                    break;
            }
        }

        /// <summary>
        /// Helper method that automatically stuffs bytes if required according to the SHDLC protocol.
        /// </summary>
        /// <param name="memoryStream">The stream to write to.</param>
        /// <param name="buffer">The bytes that are potentially stuffed/escaped when written.</param>
        /// <param name="offset">Location within the buffer to start at.</param>
        /// <param name="count">Number of bytes to take from the start position.</param>
        public static void WriteStuffed(this MemoryStream memoryStream, byte[] buffer, int offset, int count)
        {
            for (int i = offset; i < offset + count; i++)
            {
                memoryStream.WriteByteStuffed(buffer[i]);
            }
        }
    }
}
