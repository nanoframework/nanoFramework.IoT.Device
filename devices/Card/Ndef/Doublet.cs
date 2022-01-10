// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ndef
{
    /// <summary>
    /// Doublet containing the start and size of the message
    /// </summary>
    public class Doublet
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="b1">Start</param>
        /// <param name="b2">Size</param>
        public Doublet(int  b1, int b2)
        {
            Start = b1;
            Size = b2;
        }

        /// <summary>
        /// Start of the message
        /// </summary>
        public int Start { get; set; }

        /// <summary>
        /// Size of the message
        /// </summary>
        public int Size { get; set; }
    }
}
