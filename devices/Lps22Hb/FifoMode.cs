// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lps22Hb
{
    /// <summary>
    /// FIFO mode selection.
    /// </summary>
    public enum FifoMode : byte
    {
        /// <summary>
        /// Bypass mode.
        /// </summary>
        Bypass = 0,

        /// <summary>
        /// FIFO mode.
        /// </summary>
        Fifo = 1,

        /// <summary>
        /// Stream mode.
        /// </summary>
        Stream = 2,

        /// <summary>
        /// Stream-to-FIFO mode.
        /// </summary>
        StreamToFifo = 3,

        /// <summary>
        /// Bypass-to-Stream mode.
        /// </summary>
        BypassToStream = 4,

        /// <summary>
        /// Dynamic-Stream mode.
        /// </summary>
        DynamicStream = 6,

        /// <summary>
        /// Bypass-to-FIFO mode.
        /// </summary>
        BypassToFifo = 7
    }
}
