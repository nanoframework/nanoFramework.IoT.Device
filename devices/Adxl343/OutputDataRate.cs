// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.Adxl343Lib
{
    /// <summary>
    /// Output data rate.
    /// </summary>
    public enum OutputDataRate : byte
    {
        /// <summary>
        /// Output data rate .10Hz.
        /// </summary>
        Frequency0Hz10 = 0b0000,

        /// <summary>
        /// Output data rate .20Hz.
        /// </summary>
        Frequency0Hz20 = 0b0001,

        /// <summary>
        /// Output data rate .39Hz.
        /// </summary>
        Frequency0Hz39 = 0b0010,

        /// <summary>
        /// Output data rate .78Hz.
        /// </summary>
        Frequency0Hz78 = 0b0011,

        /// <summary>
        /// Output data rate 1.56Hz.
        /// </summary>
        Frequency1Hz56 = 0b0100,

        /// <summary>
        /// Output data rate 3.13Hz.
        /// </summary>
        Frequency3Hz13 = 0b0101,

        /// <summary>
        /// Output data rate 6.25Hz.
        /// </summary>
        Frequency6Hz25 = 0b0110,

        /// <summary>
        /// Output data rate 12.5Hz.
        /// </summary>
        Frequency12Hz5 = 0b0111,

        /// <summary>
        /// Output data rate 25Hz.
        /// </summary>
        Frequency25Hz = 0b1000,

        /// <summary>
        /// Output data rate 50Hz.
        /// </summary>
        Frequency50Hz = 0b1001,

        /// <summary>
        /// Output data rate 100Hz.
        /// </summary>
        Frequency100Hz = 0b1010,

        /// <summary>
        /// Output data rate 200Hz.
        /// </summary>
        Frequency200Hz = 0b1011,

        /// <summary>
        /// Output data rate 400Hz.
        /// </summary>
        Frequency400Hz = 0b1100,

        /// <summary>
        /// Output data rate 800Hz.
        /// </summary>
        Frequency800Hz = 0b1101,

        /// <summary>
        /// Output data rate 1600Hz.
        /// </summary>
        Frequency1600Hz = 0b1110,

        /// <summary>
        /// Output data rate 3200Hz.
        /// </summary>
        Frequency3200Hz = 0b1111,
    }
}
