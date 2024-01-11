// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.EPaper.Enums
{
    /// <summary>
    /// Defines the display current power state.
    /// </summary>
    public enum PowerState
    {
        /// <summary>
        /// The display is in an unknown power state.
        /// </summary>
        Unknown,

        /// <summary>
        /// The display is powered on and ready to accept commands and data.
        /// </summary>
        PoweredOn,

        /// <summary>
        /// The display is powered off (sleeping) and depending on the sleep mode it may or may not allow some functions.
        /// </summary>
        PoweredOff,

        /// <summary>
        /// The display is powered off and in deep sleep mode.
        /// </summary>
        DeepSleep,
    }
}
