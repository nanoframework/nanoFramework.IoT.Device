// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp25xxx.Can
{
    /// <summary>
    /// CAN message frame type.
    /// </summary>
    public enum CanMessageFrameType
    {
        /// <summary>
        /// Data frame.
        /// </summary>
        Data = 0,

        /// <summary>
        /// Remote request frame.
        /// </summary>
        RemoteRequest
    }
}
