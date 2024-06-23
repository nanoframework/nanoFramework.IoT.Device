// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Sht4x
{
    /// <summary>
    /// Represents the measurement mode for the Sht4X sensor.
    /// </summary>
    /// <remarks>
    /// The measurement mode determines the heating and precision settings for the sensor measurements.
    /// </remarks>
    public enum MeasurementMode : byte
    {
        /// <summary>
        /// Heater off, high precision. 10 MS delay.
        /// </summary>
        NoHeaterHighPrecision = 0xFD,

        /// <summary>
        /// Heater off, medium precision. 5 MS delay.
        /// </summary>
        NoHeaterMediumPrecision = 0xF6,

        /// <summary>
        /// Heater off, low precision. 2 MS delay.
        /// </summary>
        NoHeaterLowPrecision = 0xE0,

        /// <summary>
        /// Heater on, high power, high precision. 1100MS delay.
        /// </summary>
        HighHeat1S = 0x39,

        /// <summary>
        /// Heater on, high power, high precision. 100MS delay.
        /// </summary>
        HighHeat100Ms = 0x32,

        /// <summary>
        /// Heater on, medium power, high precision. 1100MS delay.
        /// </summary>
        MediumHeat1S = 0x2F,

        /// <summary>
        /// Heater on, medium power, high precision. 100MS delay.
        /// </summary>
        MediumHeat100Ms = 0x24,

        /// <summary>
        /// Heater on, low power, high precision. 1100MS delay.
        /// </summary>
        LowHeat1S = 0x1E,

        /// <summary>
        /// Heater on, low power, high precision. 100MS delay.
        /// </summary>
        LowHeat100Ms = 0x15
    }
}