// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.EPaper.Drivers.JD796xx
{
    /// <summary>
    /// SSD1681 Supported Sleep Modes.
    /// </summary>
    public enum SleepMode : byte
    {
        /// <summary>
        /// Normal Sleep Mode.
        /// In this mode: 
        /// - DC/DC Off 
        /// - No Clock 
        /// - No Input load 
        /// - MCU Interface Access: ON
        /// - RAM Data Access: ON.
        /// </summary>
        Normal = 0x00,

        /// <summary>
        /// Deep Sleep Mode.
        /// In this mode: 
        /// - DC/DC Off 
        /// - No Clock 
        /// - No Input load 
        /// - MCU Interface Access: OFF
        /// - RAM Data Access: OFF (RAM contents NOT retained).
        /// </summary>
        DeepSleepMode = 0x01,
    }
}
