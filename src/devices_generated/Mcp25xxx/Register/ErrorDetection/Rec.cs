// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp25xxx.Register.ErrorDetection
{
    /// <summary>
    /// Receiver Error Counter Register.
    /// </summary>
    public class Rec : IRegister
    {
        /// <summary>
        /// Initializes a new instance of the Rec class.
        /// </summary>
        /// <param name="receiveErrorCount">REC[7:0]: Receive Error Count bits.</param>
        public Rec(byte receiveErrorCount)
        {
            ReceiveErrorCount = receiveErrorCount;
        }

        /// <summary>
        /// REC[7:0]: Receive Error Count bits.
        /// </summary>
        public byte ReceiveErrorCount { get; }

        /// <summary>
        /// Gets the address of the register.
        /// </summary>
        /// <returns>The address of the register.</returns>
        public Address Address => Address.Rec;

        /// <summary>
        /// Converts register contents to a byte.
        /// </summary>
        /// <returns>The byte that represent the register contents.</returns>
        public byte ToByte() => ReceiveErrorCount;
    }
}
