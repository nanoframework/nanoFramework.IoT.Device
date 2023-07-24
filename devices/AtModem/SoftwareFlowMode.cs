// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.Sim7080
{
    /// <summary>
    /// The data flow control on the serial interface for data mode.
    /// </summary>
    public enum SoftwareFlowMode
    {
        /// <summary>
        /// No Flow Control.
        /// </summary>
        NoFlowControl,
        
        /// <summary>
        /// Software Flow Control.
        /// </summary>
        SoftwareFlowControl,
        
        /// <summary>
        /// Hardware Flow Control, default setting.
        /// </summary>
        HardwareFlowControl
    }
}
