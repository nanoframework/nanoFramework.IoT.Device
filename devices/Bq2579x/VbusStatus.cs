////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// VBUS status.
    /// </summary>
    public enum VbusStatus : byte
    {
        /// <summary>
        /// VBUS not present.
        /// </summary>
        NoInput = 0b0000_0000,

        /// <summary>
        /// USB SDP (500mA).
        /// </summary>
        UsbSdp = 0b000_00010,

        /// <summary>
        /// USB CDP (1.5A).
        /// </summary>
        UsbCdp = 0b000_00100,

        /// <summary>
        /// USB DCP (3.25A).
        /// </summary>
        UsbDcp = 0b000_00110,

        /// <summary>
        /// Adjustable High Voltage DCP (HVDCP) (1.5A).
        /// </summary>
        AdjustableHighVoltage = 0b000_01000,

        /// <summary>
        ///  Unknown adaptor (3A).
        /// </summary>
        UnknownAdapter = 0b000_01010,

        /// <summary>
        /// Non-Standard Adapter (1A/2A/2.1A/2.4A).
        /// </summary>
        NonStandardAdapter = 0b000_01100,

        /// <summary>
        /// In OTG mode.
        /// </summary>
        OtgMode = 0b000_01110,

        /// <summary>
        /// Not qualified adaptor.
        /// </summary>
        NotQualifed = 0b000_10000,

        /// <summary>
        /// Device directly powered from VBUS.
        /// </summary>
        DevicePoweredFromVbus = 0b0000_10110,

        /// <summary>
        /// Backup mode.
        /// </summary>
        BackupMode = 0b0000_11000
    }
}
