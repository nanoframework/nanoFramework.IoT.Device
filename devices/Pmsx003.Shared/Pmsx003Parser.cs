// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Pmsx003.Shared
{
    internal static class Pmsx003Parser
    {
        internal const int FrameLength = 32;

        internal static Pmsx003Reading Parse(byte[] frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            if (frame.Length != FrameLength)
            {
                throw new ArgumentException("A PMSx003 frame must contain exactly 32 bytes.", nameof(frame));
            }

            if (frame[0] != 0x42 || frame[1] != 0x4D)
            {
                throw new InvalidOperationException("The PMSx003 frame header is invalid.");
            }

            if (ReadUInt16(frame, 2) != 28)
            {
                throw new InvalidOperationException("The PMSx003 frame length is invalid.");
            }

            ushort checksum = 0;
            for (int index = 0; index < FrameLength - 2; index++)
            {
                checksum += frame[index];
            }

            if (checksum != ReadUInt16(frame, FrameLength - 2))
            {
                throw new InvalidOperationException("The PMSx003 frame checksum is invalid.");
            }

            ushort[] values = new ushort[12];
            for (int index = 0; index < values.Length; index++)
            {
                values[index] = ReadUInt16(frame, 4 + (index * 2));
            }

            return new Pmsx003Reading(values, frame[28], frame[29]);
        }

        private static ushort ReadUInt16(byte[] buffer, int offset)
        {
            return (ushort)((buffer[offset] << 8) | buffer[offset + 1]);
        }
    }
}