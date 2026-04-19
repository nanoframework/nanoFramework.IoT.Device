// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Mcp25xxx.Can
{
    /// <summary>
    /// CAN message.
    /// </summary>
    public class CanMessage
    {
        /// <summary>
        /// Message ID (SID or EID format, depending on <see cref="IdentifierType"/>).
        /// </summary>
        public uint Id;

        /// <summary>
        /// Message identifier type.
        /// </summary>
        public CanMessageIdType IdentifierType;

        /// <summary>
        /// Message frame type.
        /// </summary>
        public CanMessageFrameType FrameType;

        /// <summary>
        /// Message data.
        /// </summary>
        /// <remarks>
        /// Maximum lenght of data buffer is 8.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">If the message buffer exceeds the maximum allowed lenght.</exception>
        public byte[] Message;

        /// <summary>
        /// Creates a CAN message.
        /// </summary>
        public CanMessage(uint id, CanMessageIdType identifierType, CanMessageFrameType frameType, byte[] message)
        {
            Id = id;
            IdentifierType = identifierType;
            FrameType = frameType;
            Message = message;
        }
    }
}
