// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.Sps30.Entities
{
    /// <summary>
    /// The SPS30 supports two output formats, depending on its firmware version. Float (1.0+) and ushorts (2.0+). Both formats are supported in this library.
    /// </summary>
    public enum MeasurementOutputFormat : byte
    {
        /// <summary>
        /// Measurement using big-endian float IEEE754. This works on earlier firmware versions.
        /// </summary>
        Float = 0x03,

        /// <summary>
        /// Measurement using big-endian unsigned 16-bit integer. This requires firmware version 2.0.
        /// </summary>
        UInt16 = 0x05
    }
}
