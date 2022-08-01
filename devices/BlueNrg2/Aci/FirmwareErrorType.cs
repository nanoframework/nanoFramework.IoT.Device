namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Firmware errors.
    /// </summary>
    public enum FirmwareErrorType : byte
    {
        /// <summary>
        /// Hal firmware L2CAP recombination error.
        /// </summary>
        L2CapRecombinationError = 0x01,

        /// <summary>
        /// Hal firmware GATT unexpected response error.
        /// </summary>
        GattUnexpectedResponseError = 0x02,

        /// <summary>
        /// Hal firmware GATT sequential protocol error.
        /// </summary>
        GattSequentialProtocolError = 0x03,
    }
}
