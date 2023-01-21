// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.

namespace Iot.Device.Swarm
{
    /// <summary>
    /// Operation mode for GPIO1 pin.
    /// </summary>
    public enum GpioMode : byte
    {
        /// <summary>
        /// Analog, pin is internally disconnected and not used.
        /// </summary>
        Analog = 0,

        /// <summary>
        /// Input, low-to-high transition exits sleep mode.
        /// </summary>
        ExitSleepLowToHigh,

        /// <summary>
        /// Input, high-to-low transition exits sleep mode.
        /// </summary>
        ExitSleepHighToLow,

        /// <summary>
        /// Output, set low (see remark note 1).
        /// </summary>
        OutputLow,

        /// <summary>
        /// Output, set high (see remark note 1).
        /// </summary>
        OutputHigh,

        /// <summary>
        /// Output, low indicates messages pending for client (see remark note 2).
        /// </summary>
        MessagesPendingLow,

        /// <summary>
        /// Output, high indicates messages pending for client (see remark note 2).
        /// </summary>
        MessagesPendingHigh,

        /// <summary>
        /// Output, low while transmitting. Otherwise output is high (see remark note 3).
        /// </summary>
        TransmittingLow,

        /// <summary>
        /// Output, high while transmitting. Otherwise output is low (see remark note 3).
        /// </summary>
        TransmittingHigh,

        /// <summary>
        /// Output, low indicates in sleep mode (see remark note 4). Otherwise output is high.
        /// </summary>
        SleepLow,

        /// <summary>
        /// Output, high indicates in sleep mode (see remark note 4). Otherwise output is low.
        /// </summary>
        SleepHigh,

        /// <summary>
        /// Unknown mode.
        /// </summary>
        Unknwon = 255
    }
}
