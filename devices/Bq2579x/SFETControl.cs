////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// The external ship FET control modes.
    /// </summary>
    public enum SFETControl
    {
        /// <summary>
        /// Idle. This is the default value.
        /// </summary>
        Idle = 0b0000_0000,

        /// <summary>
        /// Shutdown Mode.
        /// </summary>
        ShutdownMode = 0b0000_0010,

        /// <summary>
        /// Ship Mode.
        /// </summary>
        ShipMode = 0b0000_0100,

        /// <summary>
        /// System Power Reset.
        /// </summary>
        SystemPowerReset = 0b0000_0110,
    }
}
