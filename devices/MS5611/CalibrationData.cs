// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ms5611
{
    /// <summary>
    /// MS5611 calibration data. Check data sheet, page 7, read calibration data from PROM section
    /// </summary>
    internal class CalibrationData
    {
        public int PressureSensitivity { get; private set; }
        public int PressureOffset { get; private set; }
        public int TemperatureCoefficientOfPressureSensitivity { get; private set; }
        public int TemperatureCoefficientOfPressureOffset { get; private set; }
        public int ReferenceTemperature { get; private set; }
        public int TemperatureCoefficientOfTheTemperature { get; private set; }

        /// <summary>
        /// Reads data from device.
        /// </summary>
        /// <param name="ms5611">Sensor object</param>
        internal void ReadFromDevice(Ms5611 ms5611)
        {
            PressureSensitivity = ms5611.ReadRegister(CommandAddress.PressureSensitivity);
            PressureOffset = ms5611.ReadRegister(CommandAddress.PressureOffset);
            TemperatureCoefficientOfPressureSensitivity = ms5611.ReadRegister(CommandAddress.TemperatureCoefficientOfPressureSensitivity);
            TemperatureCoefficientOfPressureOffset = ms5611.ReadRegister(CommandAddress.TemperatureCoefficientOfPressureOffset);
            ReferenceTemperature = ms5611.ReadRegister(CommandAddress.ReferenceTemperature);
            TemperatureCoefficientOfTheTemperature = ms5611.ReadRegister(CommandAddress.TemperatureCoefficientOfTheTemperature);
        }
    }
}