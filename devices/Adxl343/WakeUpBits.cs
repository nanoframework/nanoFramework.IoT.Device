// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.Adxl343Lib
{
    /// <summary>
    /// Wakeup Bits for use in Power Control.
    /// </summary>
    public enum WakeUpBits : byte
    {
        /// <summary>
        /// Frequency 8.
        /// </summary>
        Frequency8 = 0,

        /// <summary>
        /// Frequency 4.
        /// </summary>
        Frequency4 = 1,
        
        /// <summary>
        /// Frequency 2.
        /// </summary>
        Frequency2 = 2,

        /// <summary>
        /// Frequency 1.
        /// </summary>
        Frequency1 = 3
    }
}
