// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Bmi270
{
    /// <summary>
    /// BMI270 register addresses.
    /// </summary>
    internal enum Register : byte
    {
        /// <summary>Chip ID register, expected value 0x24.</summary>
        ChipId = 0x00,

        /// <summary>Error register.</summary>
        Error = 0x02,

        /// <summary>Sensor status (data ready bits).</summary>
        Status = 0x03,

        /// <summary>Auxiliary data bytes (0x04-0x0B).</summary>
        AuxData0 = 0x04,

        /// <summary>Accelerometer X-axis LSB.</summary>
        AccelerometerXLsb = 0x0C,

        /// <summary>Accelerometer X-axis MSB.</summary>
        AccelerometerXMsb = 0x0D,

        /// <summary>Accelerometer Y-axis LSB.</summary>
        AccelerometerYLsb = 0x0E,

        /// <summary>Accelerometer Y-axis MSB.</summary>
        AccelerometerYMsb = 0x0F,

        /// <summary>Accelerometer Z-axis LSB.</summary>
        AccelerometerZLsb = 0x10,

        /// <summary>Accelerometer Z-axis MSB.</summary>
        AccelerometerZMsb = 0x11,

        /// <summary>Gyroscope X-axis LSB.</summary>
        GyroscopeXLsb = 0x12,

        /// <summary>Gyroscope X-axis MSB.</summary>
        GyroscopeXMsb = 0x13,

        /// <summary>Gyroscope Y-axis LSB.</summary>
        GyroscopeYLsb = 0x14,

        /// <summary>Gyroscope Y-axis MSB.</summary>
        GyroscopeYMsb = 0x15,

        /// <summary>Gyroscope Z-axis LSB.</summary>
        GyroscopeZLsb = 0x16,

        /// <summary>Gyroscope Z-axis MSB.</summary>
        GyroscopeZMsb = 0x17,

        /// <summary>Feature interrupt status (step, activity, motion, etc.).</summary>
        InterruptStatus0 = 0x1C,

        /// <summary>Data interrupt status (drdy, FIFO, error).</summary>
        InterruptStatus1 = 0x1D,

        /// <summary>Step counter output LSB.</summary>
        StepCounterOutput0 = 0x1E,

        /// <summary>Step counter output MSB.</summary>
        StepCounterOutput1 = 0x1F,

        /// <summary>Wrist gesture and activity type output.</summary>
        WristGestureActivity = 0x20,

        /// <summary>Internal status (init result).</summary>
        InternalStatus = 0x21,

        /// <summary>Temperature LSB.</summary>
        TemperatureLsb = 0x22,

        /// <summary>Temperature MSB.</summary>
        TemperatureMsb = 0x23,

        /// <summary>FIFO byte count LSB.</summary>
        FifoLength0 = 0x24,

        /// <summary>FIFO byte count MSB (bits [5:0]).</summary>
        FifoLength1 = 0x25,

        /// <summary>FIFO data read port.</summary>
        FifoData = 0x26,

        /// <summary>Feature configuration page select.</summary>
        FeaturePage = 0x2F,

        /// <summary>Accelerometer configuration (ODR, bandwidth, filter).</summary>
        AccelerometerConfig = 0x40,

        /// <summary>Accelerometer range.</summary>
        AccelerometerRange = 0x41,

        /// <summary>Gyroscope configuration (ODR, bandwidth, filter).</summary>
        GyroscopeConfig = 0x42,

        /// <summary>Gyroscope range.</summary>
        GyroscopeRange = 0x43,

        /// <summary>FIFO downsampling configuration.</summary>
        FifoDownsampling = 0x45,

        /// <summary>FIFO watermark level LSB.</summary>
        FifoWatermark0 = 0x46,

        /// <summary>FIFO watermark level MSB (bits [4:0]).</summary>
        FifoWatermark1 = 0x47,

        /// <summary>FIFO configuration 0 (filter, stop-on-full).</summary>
        FifoConfig0 = 0x48,

        /// <summary>FIFO configuration 1 (enable acc/gyr/aux, header mode).</summary>
        FifoConfig1 = 0x49,

        /// <summary>Auxiliary device I2C address.</summary>
        AuxDeviceId = 0x4B,

        /// <summary>Auxiliary interface configuration (burst length, ODR).</summary>
        AuxInterfaceConfig = 0x4C,

        /// <summary>Auxiliary read address.</summary>
        AuxReadAddress = 0x4D,

        /// <summary>Auxiliary write address.</summary>
        AuxWriteAddress = 0x4E,

        /// <summary>Auxiliary write data.</summary>
        AuxWriteData = 0x4F,

        /// <summary>INT1 output pin configuration.</summary>
        Int1IoControl = 0x53,

        /// <summary>INT2 output pin configuration.</summary>
        Int2IoControl = 0x54,

        /// <summary>Feature interrupt mapping to INT1.</summary>
        Int1MapFeature = 0x56,

        /// <summary>Feature interrupt mapping to INT2.</summary>
        Int2MapFeature = 0x57,

        /// <summary>Data interrupt mapping (INT1 bits [3:0], INT2 bits [7:4]).</summary>
        IntMapData = 0x58,

        /// <summary>Config load control.</summary>
        InitControl = 0x59,

        /// <summary>Config burst address low byte.</summary>
        InitAddress0 = 0x5B,

        /// <summary>Config burst address high byte.</summary>
        InitAddress1 = 0x5C,

        /// <summary>Config data write register.</summary>
        InitData = 0x5E,

        /// <summary>Auxiliary I2C interface configuration.</summary>
        InterfaceConfig = 0x6B,

        /// <summary>Gyroscope offset compensation registers (4 bytes, 0x74-0x77).</summary>
        GyroscopeOffsetX = 0x74,

        /// <summary>Power configuration.</summary>
        PowerConfig = 0x7C,

        /// <summary>Power control (enable accel/gyro/aux/temp).</summary>
        PowerControl = 0x7D,

        /// <summary>Command register.</summary>
        Command = 0x7E,
    }
}
