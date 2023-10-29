// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;

namespace Iot.Device.Ld2410.Commands
{
    internal abstract class CommandFrame
    {
        internal static readonly byte[] Header = new byte[4] { 0xFD, 0xFC, 0xFB, 0xFA };
        internal static readonly byte[] End = new byte[4] { 0x04, 0x03, 0x02, 0x01 };

        protected const ushort DataLengthSegmentSize = 2;
        protected const ushort CommandWordLength = 2;

        internal CommandWord Command { get; set; }

        protected byte[] Value { get; set; }

        protected CommandFrame(CommandWord command)
        {
            Command = command;
            Value = new byte[0];
        }

        /// <summary>
        /// Serializes this frame to its binary format as per the LD2410 specs and writes it to a stream.
        /// </summary>
        /// <returns>A byte array representation of the frame to be sent to the radar module.</returns>
        internal virtual byte[] Serialize()
        {
            /*
            * The structure of the command frame in the stream is:
            * 
            * FRAME HEADER | FRAME DATA LENGTH | FRAME DATA (COMMAND + VALUE) | FRAME END
            * 
            * All in little-endian.
            */

            var header = new SpanByte(Header);
            var end = new SpanByte(End);

            /*
            * The full frame length is: 4 (FRAME HEADER) + 2 (Data Length Segment) + 2 (COMMAND WORD) + X (COMMAND VALUE) + 4 (FRAME END)
            */
            var serializedFrame = new byte[Header.Length +
                    DataLengthSegmentSize +
                    CommandWordLength +
                    Value.Length +
                    End.Length];

            var serializedFrameSpan = new SpanByte(serializedFrame);

            var serializedFrameCurrentPosition = 0;

            // FRAME HEADER
            header.CopyTo(serializedFrameSpan.Slice(serializedFrameCurrentPosition));
            serializedFrameCurrentPosition += header.Length;

            /*
            * FRAME DATA LENGTH = Command Word Length + Command Value Length.
            * 
            * Command Word Length is always 2 Bytes.
            * 
            * The Length value has to be 2 bytes long so we cast to C# Short.
            * 
            */
            BinaryPrimitives.WriteUInt16LittleEndian(
                destination: serializedFrameSpan.Slice(serializedFrameCurrentPosition),
                value: (ushort)(CommandWordLength + Value.Length));
            serializedFrameCurrentPosition += 2;

            // FRAME DATA: COMMAND
            BinaryPrimitives.WriteUInt16LittleEndian(
                destination: serializedFrameSpan.Slice(serializedFrameCurrentPosition),
                value: (ushort)Command);
            serializedFrameCurrentPosition += 2;

            // FRAME DATA: VALUE
            var value = new SpanByte(Value);
            value.CopyTo(serializedFrameSpan.Slice(serializedFrameCurrentPosition));
            serializedFrameCurrentPosition += value.Length;

            // FRAME END
            end.CopyTo(serializedFrameSpan.Slice(serializedFrameCurrentPosition));

            return serializedFrame;
        }
    }
}
