// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ms5611
{
    /// <summary>
    /// MS5611 command addresses
    /// </summary>
    internal enum CommandAddress : byte
    {
        /// <summary>
        /// ADC read
        /// </summary>
        AdcRead = 0x00,

        /// <summary>
        /// Reset
        /// </summary>
        Reset = 0x1E,

        /// <summary>
        /// Sampling rate for pressure
        /// </summary>
        SamplingRatePressure = 0x40,

        /// <summary>
        /// Sampling rate for temperature
        /// </summary>
        SamplingRateTemperature = 0x50,

        /// <summary>
        /// Read PROM command
        /// </summary>
        ReadProm = 0xA2,

        /// <summary>
        /// Calibration data - pressure sensitivity
        /// </summary>
        PressureSensitivity = 0xA2,

        /// <summary>
        /// Calibration data - pressure offset
        /// </summary>
        PressureOffset = 0xA4,

        /// <summary>
        /// Calibration data - temperature coefficient of pressure sensitivity
        /// </summary>
        TemperatureCoefficientOfPressureSensitivity = 0xA6,

        /// <summary>
        /// Calibration data - temperature coefficient of pressure offset
        /// </summary>
        TemperatureCoefficientOfPressureOffset = 0xA8,

        /// <summary>
        /// Calibration data - reference temperature
        /// </summary>
        ReferenceTemperature = 0xAA,

        /// <summary>
        /// Calibration data - temperature coefficient of the temperature
        /// </summary>
        TemperatureCoefficientOfTheTemperature = 0xAC,

        /// <summary>
        /// Low sampling rate temperature
        /// </summary>
        LowSamplingRateTemperature = 0x52,

        /// <summary>
        /// Standard sampling rate temperature
        /// </summary>
        StandardSamplingRateTemperature = 0x54,

        /// <summary>
        /// High sampling rate temperature
        /// </summary>
        HighSamplingRateTemperature = 0x56,

        /// <summary>
        /// Ultra high sampling rate temperature
        /// </summary>
        UltraHighSamplingRateTemperature = 0x58,

        /// <summary>
        /// Low sampling rate pressure
        /// </summary>
        LowSamplingRatePressure = 0x42,

        /// <summary>
        /// Standard sampling rate pressure
        /// </summary>
        StandardSamplingRatePressure = 0x44,

        /// <summary>
        /// High sampling rate pressure
        /// </summary>
        HighSamplingRatePressure = 0x46,

        /// <summary>
        /// Ultra high sampling rate pressure
        /// </summary>
        UltraHighSamplingRatePressure = 0x48
    }
}