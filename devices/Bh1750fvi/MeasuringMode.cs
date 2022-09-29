// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Bh1750fvi
{
    /// <summary>
    /// The measuring mode of BH1750FVI.
    /// </summary>
    public enum MeasuringMode : byte
    {
        // Details in the datasheet P5

        /// <summary>
        /// Start measurement at 1lx resolution
        /// Measurement Time is typically 120ms.
        /// </summary>
        ContinuouslyHighResolutionMode = 0b00010000,

        /// <summary>
        /// Start measurement at 0.5lx resolution
        /// Measurement Time is typically 120ms.
        /// </summary>
        ContinuouslyHighResolutionMode2 = 0b00010001,

        /// <summary>
        /// Start measurement at 4lx resolution
        /// Measurement Time is typically 16ms.
        /// </summary>
        ContinuouslyLowResolutionMode = 0b00010011,

        /// <summary>
        /// Start measurement at 1lx resolution once
        /// Measurement Time is typically 120ms.
        /// It is automatically set to Power Down mode after measurement.
        /// </summary>
        OneTimeHighResolutionMode = 0b00100000,

        /// <summary>
        /// Start measurement at 0.5lx resolution once
        /// Measurement Time is typically 120ms.
        /// It is automatically set to Power Down mode after measurement.
        /// </summary>
        OneTimeHighResolutionMode2 = 0b00100001,

        /// <summary>
        /// Start measurement at 4lx resolution once
        /// Measurement Time is typically 16ms.
        /// It is automatically set to Power Down mode after measurement.
        /// </summary>
        OneTimeLowResolutionMode = 0b00100011
    }
}
