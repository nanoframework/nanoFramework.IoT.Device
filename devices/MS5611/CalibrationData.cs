namespace Iot.Device.MS5611
{
    /// <summary>
    /// MS5611 calibration data
    /// </summary>
    internal class CalibrationData
    {
        public int PressureSensitivity { private set; get; }
        public int PressureOffset { private set; get; }
        public int TemperatureCoefficientOfPressureSensitivity { private set; get; }
        public int TemperatureCoefficientOfPressureOffset { private set; get; }
        public int ReferenceTemperature { private set; get; }
        public int TemperatureCoefficientOfTheTemperature { private set; get; }

        /// <summary>
        /// Reads data from device
        /// </summary>
        /// <param name="ms5611">Sensor object</param>
        internal void ReadFromDevice(Ms5611 ms5611)
        {
            PressureSensitivity = ms5611.ReadRegister((int)CommandAddresses.MS5611_CMD_READ_PROM);
            PressureOffset = ms5611.ReadRegister((int)CommandAddresses.MS5611_CMD_READ_PROM + 2);
            TemperatureCoefficientOfPressureSensitivity = ms5611.ReadRegister((int)CommandAddresses.MS5611_CMD_READ_PROM + 4);
            TemperatureCoefficientOfPressureOffset = ms5611.ReadRegister((int)CommandAddresses.MS5611_CMD_READ_PROM + 6);
            ReferenceTemperature = ms5611.ReadRegister((int)CommandAddresses.MS5611_CMD_READ_PROM + 8);
            TemperatureCoefficientOfTheTemperature = ms5611.ReadRegister((int)CommandAddresses.MS5611_CMD_READ_PROM + 10);
        }
    }
}