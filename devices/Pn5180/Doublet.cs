// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Pn5180
{
    /// <summary>
    /// Doublet containing the number of available bytes and valid bits
    /// </summary>
    public class Doublet
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bytes">Available bytes</param>
        /// <param name="validBits">Number of valid bits</param>
        public Doublet(int bytes, int validBits)
        {
            Bytes = bytes;
            ValidBits = validBits;
        }

        /// <summary>
        /// Available bytes
        /// </summary>
        public int Bytes { get; set; } 

        /// <summary>
        /// Number of valid bits
        /// </summary>
        public int ValidBits { get; set; }
    }
}
