// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Ip5306
{
    /// <summary>
    /// Button status
    /// </summary>
    [Flags]
    public enum ButtonStatus
    {
        /// <summary>Double click</summary>
        DoubleClick = 0b0000_0100,

        /// <summary>Long press</summary>
        LongPress = 0b0000_0010,

        /// <summary>Short press</summary>
        ShortPress = 0b0000_0001,

        /// <summary>Not pressed</summary>
        NotPressed = 0b0000_0000,
    }
}
