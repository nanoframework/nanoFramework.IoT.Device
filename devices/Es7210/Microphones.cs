// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Es7210
{
    /// <summary>
    /// Selects which microphone input channels of the ES7210 are enabled for capture.
    /// </summary>
    [Flags]
    public enum Microphones
    {
        /// <summary>
        /// No microphone is enabled.
        /// </summary>
        None = 0,

        /// <summary>
        /// Microphone 1 (MIC1) is enabled.
        /// </summary>
        Microphone1 = 1,

        /// <summary>
        /// Microphone 2 (MIC2) is enabled.
        /// </summary>
        Microphone2 = 2,
    }
}
