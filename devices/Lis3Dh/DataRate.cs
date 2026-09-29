// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lis3DhAccelerometer
{
    /// <summary>
    /// Output data rate selection.
    /// </summary>
    public enum DataRate : byte
    {
        /// <summary>
        /// Power-down mode.
        /// </summary>
        PowerDownMode = 0b0000,

        /// <summary>
        /// 1 Hz output data rate.
        /// </summary>
        DataRate1Hz = 0b0001,

        /// <summary>
        /// 10 Hz output data rate.
        /// </summary>
        DataRate10Hz = 0b0010,

        /// <summary>
        /// 25 Hz output data rate.
        /// </summary>
        DataRate25Hz = 0b0011,

        /// <summary>
        /// 50 Hz output data rate.
        /// </summary>
        DataRate50Hz = 0b0100,

        /// <summary>
        /// 100 Hz output data rate.
        /// </summary>
        DataRate100Hz = 0b0101,

        /// <summary>
        /// 200 Hz output data rate.
        /// </summary>
        DataRate200Hz = 0b0110,

        /// <summary>
        /// 400 Hz output data rate.
        /// </summary>
        DataRate400Hz = 0b0111,

        /// <summary>
        /// 1.6 kHz output data rate, available only in low-power mode.
        /// </summary>
        LowPowerMode1600Hz = 0b1000,

        /// <summary>
        /// 1.344 kHz in high-resolution or normal mode, or 5.376 kHz in low-power mode.
        /// </summary>
        HighResolutionNormal1344HzOrLowPowerMode5376Hz = 0b1001,
    }
}
