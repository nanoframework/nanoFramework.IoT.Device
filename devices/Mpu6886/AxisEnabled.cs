namespace Iot.Device.Mpu6886
{
    /// <summary>
    /// Axes to enable
    /// </summary>
    public enum EnabledAxis
    {
        /// <summary>
        /// Accelerometer X axis.
        /// </summary>
        AccelerometerX = 0b0010_0000,
        /// <summary>
        /// Accelerometer Y axis.
        /// </summary>
        AccelerometerY = 0b0001_0000,
        /// <summary>
        /// Accelerometer Z axis.
        /// </summary>
        AccelerometerZ = 0b0000_1000,
        /// <summary>
        /// Gyroscope X axis.
        /// </summary>
        GyroscopeX = 0b0000_0100,
        /// <summary>
        /// Gyroscope Y axis.
        /// </summary>
        GyroscopeY = 0b0000_0010,
        /// <summary>
        /// Gyroscope Z axis.
        /// </summary>
        GyroscopeZ = 0b0000_0001,
    }
}
