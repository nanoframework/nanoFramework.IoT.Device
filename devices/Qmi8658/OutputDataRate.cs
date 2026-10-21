// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Qmi8658
{
    /// <summary>
    /// Normal-mode output data rates supported by both sensors.
    /// </summary>
    public enum OutputDataRate : byte
    {
        /// <summary>Output data rate of 8000 Hz.</summary>
        Odr8000Hz = 0x00,

        /// <summary>Output data rate of 4000 Hz.</summary>
        Odr4000Hz = 0x01,

        /// <summary>Output data rate of 2000 Hz.</summary>
        Odr2000Hz = 0x02,

        /// <summary>Output data rate of 1000 Hz.</summary>
        Odr1000Hz = 0x03,

        /// <summary>Output data rate of 500 Hz.</summary>
        Odr500Hz = 0x04,

        /// <summary>Output data rate of 250 Hz.</summary>
        Odr250Hz = 0x05,

        /// <summary>Output data rate of 125 Hz.</summary>
        Odr125Hz = 0x06,

        /// <summary>Output data rate of 62.5 Hz.</summary>
        Odr62Point5Hz = 0x07,

        /// <summary>Output data rate of 31.25 Hz.</summary>
        Odr31Point25Hz = 0x08,
    }
}
