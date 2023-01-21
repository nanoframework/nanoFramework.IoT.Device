// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lp3943
{
    /// <summary>
    /// The states a led can be in
    /// </summary>
    public enum LedState : byte
    {
        /// <summary>
        /// The led is off
        /// </summary>
        Off = 0b00,

        /// <summary>
        /// The led is on
        /// </summary>
        On = 0b01,

        /// <summary>
        /// the led is driven by dim0
        /// </summary>
        Dim0 = 0b10,

        /// <summary>
        /// the led is driven by dim1
        /// </summary>
        Dim1 = 0b11
    }
}
