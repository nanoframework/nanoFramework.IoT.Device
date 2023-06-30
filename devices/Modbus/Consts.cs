namespace Iot.Device.Modbus
{
    /// <summary>
    /// Contains protocol constant definitions.
    /// </summary>
    public static class Consts
    {
        #region Protocol limitations

        /// <summary>
        /// The lowest accepted device id on RTU protocol.
        /// </summary>
        public const byte MinDeviceId = 0x01;

        /// <summary>
        /// The highest accepted device id on RTU protocol.
        /// </summary>
        public const byte MaxDeviceId = 0xF7; // 247

        /// <summary>
        /// The lowest address.
        /// </summary>
        public const ushort MinAddress = 0x0000;

        /// <summary>
        /// The highest address.
        /// </summary>
        public const ushort MaxAddress = 0xFFFF; // 65535

        /// <summary>
        /// The lowest number of requested data sets.
        /// </summary>
        public const ushort MinCount = 0x01;

        /// <summary>
        /// The highest number of requested coils to read.
        /// </summary>
        public const ushort MaxCoilCountRead = 0x7D0; // 2000

        /// <summary>
        /// The highest number of requested coils to write.
        /// </summary>
        public const ushort MaxCoilCountWrite = 0x7B0; // 1968

        /// <summary>
        /// The highest number of requested registers to read.
        /// </summary>
        public const ushort MaxRegisterCountRead = 0x7D; // 125

        /// <summary>
        /// The highest number of requested registers to write.
        /// </summary>
        public const ushort MaxRegisterCountWrite = 0x7B; // 123

        #endregion

        #region Error/Exception

        /// <summary>
        /// The Bit-Mask to filter the error-state of a Modbus response.
        /// </summary>
        public const byte ErrorMask = 0x80;

        #endregion
    }
}
