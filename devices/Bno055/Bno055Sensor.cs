// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Device.Model;
using System.Numerics;
using System.Threading;
using Temperature = UnitsNet.Temperature;

namespace Iot.Device.Bno055
{
    /// <summary>
    /// BNO055 - inertial measurement unit (IMU).
    /// </summary>
    [Interface("BNO055 - inertial measurement unit (IMU)")]
    public class Bno055Sensor : IDisposable
    {
        /// <summary>
        /// The default I2C Address, page 91 of the main documentation
        /// https://ae-bst.resource.bosch.com/media/_tech/media/datasheets/BST-BNO055-DS000.pdf.
        /// </summary>
        public const byte DefaultI2cAddress = 0x28;

        /// <summary>
        /// This is the second I2C Address. It needs to be activated to be valid.
        /// </summary>
        public const byte SecondI2cAddress = 0x29;

        private const byte DeviceId = 0xA0;

        private static readonly byte[][] RegisterDefaults =
        {
            new byte[]
            {
                (byte)Registers.UNIT_SEL, (byte)(
                    Units.AccelerationMeterPerSecond |
                    Units.AngularRateDegreePerSecond |
                    Units.DataOutputFormatWindows |
                    Units.EulerAnglesDegrees |
                    Units.TemperatureCelsius)
            },
            new byte[]
            {
                (byte)Registers.TEMP_SOURCE, (byte)TemperatureSource.Gyroscope
            },
            new byte[]
            {
                (byte)Registers.PWR_MODE, (byte)PowerMode.Normal
            },
            new byte[]
            {
                (byte)Registers.AXIS_MAP_CONFIG, 0x24, // AXIS_MAP_CONFIG
                0, // AXIS_MAP_SIGN
            },
            new byte[]
            {
                (byte)Registers.ACCEL_OFFSET_X_LSB, 0, 0, 0, 0, 0, 0, // ACC_OFFSET_*_*SB
                0, 0, 0, 0, 0, 0, // MAC_OFFSET_*_*SB
                0, 0, 0, 0, 0, 0, // GYR_OFFSET_*_*SB
                0, 0, 0, 0, // *_RADIUS_*SB
            }
        };

        private readonly bool _shouldDispose;
        private I2cDevice _i2cDevice;
        private OperationMode _operationMode;
        private Units _units;

        /// <summary>
        /// Gets or sets the operation mode.
        /// </summary>
        [Property]
        public OperationMode OperationMode
        {
            get => _operationMode;
            set
            {
                _operationMode = value;
                SetConfigMode(true);
                SetOperationMode(_operationMode);
                SetConfigMode(false);
            }
        }

        /// <summary>
        /// Gets or sets the power mode.
        /// </summary>
        [Property]
        public PowerMode PowerMode
        {
            get => (PowerMode)ReadByte(Registers.PWR_MODE);
            set
            {
                SetConfigMode(true);
                WriteReg(Registers.PWR_MODE, (byte)value);
                SetConfigMode(false);
            }
        }

        /// <summary>
        /// Gets or sets the temperature source.
        /// </summary>
        [Property]
        public TemperatureSource TemperatureSource
        {
            get => (TemperatureSource)ReadByte(Registers.TEMP_SOURCE);
            set
            {
                SetConfigMode(true);
                WriteReg(Registers.TEMP_SOURCE, (byte)value);
                SetConfigMode(false);
            }
        }

        /// <summary>
        /// Gets or sets the units used. By default, international system is used.
        /// </summary>
        [Property]
        public Units Units
        {
            get => (Units)ReadByte(Registers.UNIT_SEL);
            set
            {
                SetConfigMode(true);
                _units = value;
                WriteReg(Registers.UNIT_SEL, (byte)_units);
                SetConfigMode(false);
            }
        }

        /// <summary>
        /// Gets the information about various sensor system versions and ID.
        /// </summary>
        public Info Info { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bno055Sensor" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C Device.</param>
        /// <param name="operationMode">The operation mode to setup.</param>
        /// <param name="shouldDispose">True to dispose the I2C device at dispose.</param>
        public Bno055Sensor(
            I2cDevice i2cDevice,
            OperationMode operationMode = OperationMode.AccelerometerMagnetometerGyroscopeRelativeOrientation,
            bool shouldDispose = true)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _shouldDispose = shouldDispose;

            // A first write to initate the device
            WriteReg(Registers.PAGE_ID, 0);
            SetConfigMode(true);

            // A second write
            WriteReg(Registers.PAGE_ID, 0);
            byte chipId = ReadByte(Registers.CHIP_ID);
            if (chipId != DeviceId)
            {
                throw new Exception($"{nameof(Bno055Sensor)} is not a valid sensor");
            }

            Info = new Info(
                chipId,                                     // Chip Id
                ReadByte(Registers.ACCEL_REV_ID),           // AcceleratorId
                ReadByte(Registers.MAG_REV_ID),             // MagnetometerId
                ReadByte(Registers.GYRO_REV_ID),            // GyroscopeId
                new Version(ReadByte(Registers.SW_REV_ID_MSB), ReadByte(Registers.SW_REV_ID_LSB)),  // FirmwareVersion
                new Version(ReadByte(Registers.BL_REV_ID), 0)); // BootloaderVersion
            _operationMode = operationMode;
            InitializeRegisters();
        }

        private void InitializeRegisters()
        {
            // WriteReg(Registers.SYS_TRIGGER, 0x20);
            // Using the chip's internal reset might not work:
            // https://community.bosch-sensortec.com/t5/MEMS-sensors-forum/BNO055-power-on-reset-issues/m-p/8457/highlight/true#M948
            SetConfigMode(true);

            foreach (var registerDefault in RegisterDefaults)
            {
                _i2cDevice.Write(registerDefault);
            }

            SetConfigMode(false);
        }

        /// <summary>
        /// Set internal or external crystal usage.
        /// Note: if you don't have an external crystal, don't use this function.
        /// </summary>
        /// <param name="external"><see langword="true"/> to set to external.</param>
        public void SetExternalCrystal(bool external)
        {
            SetConfigMode(true);
            if (external)
            {
                WriteReg(Registers.SYS_TRIGGER, 0x80);
            }
            else
            {
                WriteReg(Registers.SYS_TRIGGER, 0x00);
            }

            SetConfigMode(false);
        }

        /// <summary>
        /// Get the status. If there is an error, GetError() will give more details.
        /// </summary>
        /// <returns>Status of the sensor.</returns>
        [Telemetry("Status")]
        public Status GetStatus() => (Status)ReadByte(Registers.SYS_STAT);

        /// <summary>
        /// Get the latest error.
        /// </summary>
        /// <returns>Returns the latest error.</returns>
        [Telemetry("Status")]
        public Error GetError() => (Error)ReadByte(Registers.SYS_ERR);

        /// <summary>
        /// Run a self test. In case of error, use GetStatus() and GetError() to get the last error.
        /// </summary>
        /// <returns>Status fo the test.</returns>
        [Command]
        public TestResult RunSelfTest()
        {
            SetConfigMode(true);
            var oldTrigger = ReadByte(Registers.SYS_TRIGGER);

            // Set test mode
            WriteReg(Registers.SYS_TRIGGER, (byte)(oldTrigger | 0x01));

            // Wait for the test to go
            Thread.Sleep(1000);
            var testRes = ReadByte(Registers.SELFTEST_RESULT);
            SetConfigMode(false);
            return (TestResult)testRes;
        }

        /// <summary>
        /// Returns the calibration status for the system and sensors.
        /// </summary>
        /// <returns>Calibration status.</returns>
        [Command]
        public CalibrationStatus GetCalibrationStatus() => (CalibrationStatus)ReadByte(Registers.CALIB_STAT);

        /// <summary>
        /// Get the accelerometer calibration data.
        /// </summary>
        /// <returns>Returns the accelerometers calibration data.</returns>
        [Command]
        public Vector4 GetAccelerometerCalibrationData()
        {
            SetConfigMode(true);
            Vector3 vect = GetVectorData(Registers.ACCEL_OFFSET_X_LSB);
            short radius = ReadInt16(Registers.ACCEL_RADIUS_LSB);
            SetConfigMode(false);
            return new Vector4(vect, radius);
        }

        /// <summary>
        /// Set the accelerometer calibration data.
        /// </summary>
        /// <param name="calibrationData">Calibration data.</param>
        [Command]
        public void SetAccelerometerCalibrationData(Vector4 calibrationData)
        {
            SetConfigMode(true);
            SpanByte outArray = new byte[7];
            outArray[0] = (byte)Registers.ACCEL_OFFSET_X_LSB;
            BinaryPrimitives.WriteInt16LittleEndian(outArray.Slice(1), (short)calibrationData.X);
            BinaryPrimitives.WriteInt16LittleEndian(outArray.Slice(3), (short)calibrationData.Y);
            BinaryPrimitives.WriteInt16LittleEndian(outArray.Slice(5), (short)calibrationData.Z);
            _i2cDevice.Write(outArray);
            outArray[4] = (byte)Registers.ACCEL_RADIUS_LSB;
            BinaryPrimitives.WriteInt16LittleEndian(outArray.Slice(5), (short)calibrationData.W);
            _i2cDevice.Write(outArray.Slice(4));
            SetConfigMode(false);
        }

        /// <summary>
        /// Get the magnetometer calibration data.
        /// </summary>
        /// <returns>Returns the magnetometer calibration data.</returns>
        [Command]
        public Vector4 GetMagnetometerCalibrationData()
        {
            SetConfigMode(true);
            Vector3 vect = GetVectorData(Registers.MAG_OFFSET_X_LSB);
            short radius = ReadInt16(Registers.MAG_RADIUS_LSB);
            SetConfigMode(false);
            return new Vector4(vect, radius);
        }

        /// <summary>
        /// Set the magnetometer calibration data.
        /// </summary>
        /// <param name="calibrationData">Calibration data.</param>
        [Command]
        public void SetMagnetometerCalibrationData(Vector4 calibrationData)
        {
            SetConfigMode(true);
            SpanByte outArray = new byte[7];
            outArray[0] = (byte)Registers.MAG_OFFSET_X_LSB;
            BinaryPrimitives.WriteInt16LittleEndian(outArray.Slice(1), (short)calibrationData.X);
            BinaryPrimitives.WriteInt16LittleEndian(outArray.Slice(3), (short)calibrationData.Y);
            BinaryPrimitives.WriteInt16LittleEndian(outArray.Slice(5), (short)calibrationData.Z);
            _i2cDevice.Write(outArray);
            outArray[4] = (byte)Registers.MAG_RADIUS_LSB;
            BinaryPrimitives.WriteInt16LittleEndian(outArray.Slice(5), (short)calibrationData.W);
            _i2cDevice.Write(outArray.Slice(4));
            SetConfigMode(false);
        }

        /// <summary>
        /// Get the gyroscope calibration data.
        /// </summary>
        /// <returns>X, Y and Z data.</returns>
        [Command]
        public Vector3 GetGyroscopeCalibrationData()
        {
            SetConfigMode(true);
            Vector3 vect = GetVectorData(Registers.GYRO_OFFSET_X_LSB);
            SetConfigMode(false);
            return vect;
        }

        /// <summary>
        /// Set the gyroscope calibration data.
        /// </summary>
        /// <param name="calibrationData">X, Y and Z data.</param>
        [Command]
        public void SetGyroscopeCalibrationData(Vector3 calibrationData)
        {
            SetConfigMode(true);
            SpanByte outArray = new byte[7];
            outArray[0] = (byte)Registers.GYRO_OFFSET_X_LSB;
            BinaryPrimitives.WriteInt16LittleEndian(outArray.Slice(1), (short)calibrationData.X);
            BinaryPrimitives.WriteInt16LittleEndian(outArray.Slice(3), (short)calibrationData.Y);
            BinaryPrimitives.WriteInt16LittleEndian(outArray.Slice(5), (short)calibrationData.Z);
            _i2cDevice.Write(outArray);
            SetConfigMode(false);
        }

        /// <summary>
        /// Set the Axis map.
        /// </summary>
        /// <param name="x">X axis setting.</param>
        /// <param name="y">Y axis setting.</param>
        /// <param name="z">Z axis setting.</param>
        public void SetAxisMap(AxisSetting x, AxisSetting y, AxisSetting z)
        {
            SetConfigMode(true);
            WriteReg(Registers.AXIS_MAP_CONFIG, (byte)(((byte)z.Axis << 4) | ((byte)y.Axis << 2) | (byte)x.Axis));
            WriteReg(Registers.AXIS_MAP_SIGN, (byte)(((byte)z.Sign << 2) | ((byte)y.Sign << 1) | (byte)x.Sign));
            SetConfigMode(false);
        }

        /// <summary>
        /// Get the Axis map.
        /// </summary>
        /// <returns>Returns an array where first element is axis X, then Y then Z.</returns>
        public AxisSetting[] GetAxisMap()
        {
            SetConfigMode(true);
            var retMap = ReadByte(Registers.AXIS_MAP_CONFIG);
            var retSign = ReadByte(Registers.AXIS_MAP_SIGN);
            AxisSetting[] axisSettings = new AxisSetting[3];
            axisSettings[0].Axis = (AxisMap)(retMap & 0x03);
            axisSettings[1].Axis = (AxisMap)((retMap >> 2) & 0x03);
            axisSettings[2].Axis = (AxisMap)((retMap >> 4) & 0x03);
            axisSettings[0].Sign = (AxisSign)(retSign & 0x01);
            axisSettings[1].Sign = (AxisSign)((retSign >> 1) & 0x01);
            axisSettings[2].Sign = (AxisSign)((retSign >> 2) & 0x01);
            SetConfigMode(false);
            return axisSettings;
        }

        /// <summary>
        /// Gets the orientation (Euler Angles) X = Heading, Y = Roll, Z = Pitch.
        /// </summary>
        [Telemetry(null, "Orientation (Euler Angles)")]
        public Vector3 Orientation
        {
            get
            {
                Vector3 retVect = GetVectorData(Registers.EULER_H_LSB);

                // If unit is MeterG, then divide by 900, otherwise divide by 16
                if (_units.HasFlag(Units.EulerAnglesRadians))
                {
                    return retVect / 900;
                }

                return retVect / 16;
            }
        }

        /// <summary>
        /// Get the Magnetometer.
        /// </summary>
        [Telemetry]
        public Vector3 Magnetometer => GetVectorData(Registers.MAG_DATA_X_LSB) / 16;

        /// <summary>
        /// Gets the gyroscope.
        /// </summary>
        [Telemetry]
        public Vector3 Gyroscope
        {
            get
            {
                Vector3 retVect = GetVectorData(Registers.GYRO_DATA_X_LSB);
                if ((_units & Units.AngularRateRotationPerSecond) == Units.AngularRateRotationPerSecond)
                {
                    return retVect / 900;
                }
                else
                {
                    return retVect / 16;
                }
            }
        }

        /// <summary>
        /// Gets the accelerometer
        /// Acceleration Vector (100Hz)
        /// Three axis of acceleration (gravity + linear motion)
        /// Default unit in m/s^2, can be changed for mg.
        /// </summary>
        [Telemetry]
        public Vector3 Accelerometer
        {
            get
            {
                Vector3 retVect = GetVectorData(Registers.ACCEL_DATA_X_LSB);

                // If unit is MeterG, then no convertion, otherwise divide by 100
                if ((_units & Units.AccelerationMeterG) == Units.AccelerationMeterG)
                {
                    return retVect;
                }
                else
                {
                    return retVect / 100;
                }
            }
        }

        /// <summary>
        /// Gets the linear acceleration
        /// Linear Acceleration Vector (100Hz)
        /// Three axis of linear acceleration data (acceleration minus gravity)
        /// Default unit in m/s^2, can be changed for mg.
        /// </summary>
        [Telemetry]
        public Vector3 LinearAcceleration
        {
            get
            {
                Vector3 retVect = GetVectorData(Registers.LINEAR_ACCEL_DATA_X_LSB);

                // If unit is MeterG, then no convertion, otherwise divide by 100
                if ((_units & Units.AccelerationMeterG) == Units.AccelerationMeterG)
                {
                    return retVect;
                }
                else
                {
                    return retVect / 100;
                }
            }
        }

        /// <summary>
        /// Gets the gravity
        /// Gravity Vector (100Hz)
        /// Three axis of gravitational acceleration (minus any movement)
        /// Default unit in m/s^2, can be changed for mg.
        /// </summary>
        [Telemetry]
        public Vector3 Gravity
        {
            get
            {
                Vector3 retVect = GetVectorData(Registers.GRAVITY_DATA_X_LSB);

                // If unit is MeterG, then no convertion, otherwise divide by 100
                if ((_units & Units.AccelerationMeterG) == Units.AccelerationMeterG)
                {
                    return retVect;
                }
                else
                {
                    return retVect / 100;
                }
            }
        }

        /// <summary>
        /// Gets the quaternion, unit is 1 Quaternion (unit less) = 2^14 returned result.
        /// </summary>
        [Telemetry]
        public Vector4 Quaternion => new Vector4(GetVectorData(Registers.QUATERNION_DATA_X_LSB), ReadInt16(Registers.QUATERNION_DATA_W_LSB));

        /// <summary>
        /// Gets the temperature.
        /// </summary>
        [Telemetry]
        public Temperature Temperature
        {
            get
            {
                // If unit is Farenheit, then divide by 2, otherwise no convertion
                if ((_units & Units.TemperatureFarenheit) == Units.TemperatureFarenheit)
                {
                    return Temperature.FromDegreesFahrenheit(ReadByte(Registers.TEMP) / 2.0);
                }

                return Temperature.FromDegreesCelsius(ReadByte(Registers.TEMP));
            }
        }

        /// <summary>
        /// Gets the interupt status.
        /// </summary>
        /// <returns>InteruptStatus of the sensor.</returns>
        public InteruptStatus GetInteruptStatus() => (InteruptStatus)ReadByte(Registers.INTR_STAT);

        private void SetOperationMode(OperationMode operation)
        {
            WriteReg(Registers.OPR_MODE, (byte)operation);

            // It is necessary to wait 30 milliseconds
            Thread.Sleep(30);
        }

        private Vector3 GetVectorData(Registers reg)
        {
            SpanByte retArray = new byte[6];
            _i2cDevice.WriteByte((byte)reg);
            _i2cDevice.Read(retArray);
            var x = BinaryPrimitives.ReadInt16LittleEndian(retArray);
            var y = BinaryPrimitives.ReadInt16LittleEndian(retArray.Slice(2));
            var z = BinaryPrimitives.ReadInt16LittleEndian(retArray.Slice(4));
            return new Vector3(x, y, z);
        }

        private void SetConfigMode(bool mode)
        {
            if (mode)
            {
                SetOperationMode(OperationMode.Config);
            }
            else
            {
                SetOperationMode(_operationMode);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _i2cDevice?.Dispose();
                _i2cDevice = null;
            }
        }

        private void WriteReg(Registers reg, byte param)
        {
            _i2cDevice.Write(new byte[] { (byte)reg, param });
        }

        private byte ReadByte(Registers reg)
        {
            _i2cDevice.WriteByte((byte)reg);
            return _i2cDevice.ReadByte();
        }

        private short ReadInt16(Registers reg)
        {
            SpanByte retArray = new byte[2];
            _i2cDevice.WriteByte((byte)reg);
            _i2cDevice.Read(retArray);
            return BinaryPrimitives.ReadInt16LittleEndian(retArray);
        }
    }
}
