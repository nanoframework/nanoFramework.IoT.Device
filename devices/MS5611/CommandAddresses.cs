namespace Iot.Device.MS5611
{
    /// <summary>
    /// MS5611 command addresses
    /// </summary>
    internal enum CommandAddresses : byte
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
        /// Sampling rate for pressure
        /// </summary>
        MS5611_CMD_READ_PROM = 0xA2
    }
}