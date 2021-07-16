// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Card.Mifare
{
    /// <summary>
    /// Triplet to handle the Bytes 6, 7 and 8 of a sector tailer
    /// </summary>
    public class Triplet
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="b6">Byte 6</param>
        /// <param name="b7">Byte 7</param>
        /// <param name="b8">Byte 8</param>
        public Triplet(byte b6, byte b7, byte b8)
        {
            B6 = b6;
            B7 = b7;
            B8 = b8;
        }

        /// <summary>
        /// Byte 6
        /// </summary>
        public byte B6 { get; set;  }

        /// <summary>
        /// Byte 7
        /// </summary>
        public byte B7 { get; private set; }

        /// <summary>
        /// Byte 8
        /// </summary>
        public byte B8 { get; private set; }
    }
}
