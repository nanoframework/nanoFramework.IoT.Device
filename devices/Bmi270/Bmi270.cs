// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Device.Model;
using System.IO;
using System.Numerics;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Bmi270
{
    /// <summary>
    /// BMI270 6-axis IMU - accelerometer and gyroscope.
    /// </summary>
    [Interface("BMI270 accelerometer and gyroscope")]
    public class Bmi270AccelerometerGyroscope : IDisposable
    {
        /// <summary>
        /// The default I2C address for the BMI270 sensor (SDO connected to GND).
        /// </summary>
        public const int DefaultI2cAddress = 0x68;

        /// <summary>
        /// The secondary I2C address for the BMI270 sensor (SDO connected to VDDIO).
        /// This is the address used on the M5Stack CoreS3.
        /// </summary>
        public const int SecondaryI2cAddress = 0x69;

        /// <summary>
        /// Expected chip ID value for BMI270.
        /// </summary>
        private const byte ExpectedChipId = 0x24;

        /// <summary>
        /// Soft reset command value written to CMD register.
        /// </summary>
        private const byte SoftResetCommand = 0xB6;

        /// <summary>
        /// Maximum burst write size for config upload (in bytes).
        /// </summary>
        private const int ConfigBurstSize = 16;

        private const int ConfigBurstDelayMs = 1;

        /// <summary>
        /// Upper bits for ACC_CONF: filter_perf=1, bwp=normal(010) = 0xA0.
        /// </summary>
        private const byte AccelerometerConfigBase = 0xA0;

        /// <summary>
        /// Upper bits for GYR_CONF: filter_perf=1, noise_perf=0, bwp=normal(10) = 0xA0.
        /// </summary>
        private const byte GyroscopeConfigBase = 0xA0;

        private const int ConfigReadyTimeoutMs = 200;

        private const int ConfigReadyPollDelayMs = 10;

        private const byte AuxBusyMask = 0x04;

        private const int AuxBusyPollDelayMs = 1;

        private const int AuxBusyPollRetries = 50;

        private I2cDevice _i2c;
        private AccelerometerRange _accelerometerRange;
        private GyroscopeRange _gyroscopeRange;
        private AccelerometerOutputDataRate _accelerometerOdr;
        private GyroscopeOutputDataRate _gyroscopeOdr;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bmi270AccelerometerGyroscope"/> class.
        /// Performs the full initialization sequence including config file upload.
        /// </summary>
        /// <param name="i2cDevice">The I2C device.</param>
        /// <param name="accelerometerRange">Initial accelerometer range. Default is +-8g.</param>
        /// <param name="gyroscopeRange">Initial gyroscope range. Default is +-2000 dps.</param>
        /// <param name="accelerometerOdr">Initial accelerometer output data rate. Default is 100 Hz.</param>
        /// <param name="gyroscopeOdr">Initial gyroscope output data rate. Default is 200 Hz.</param>
        public Bmi270AccelerometerGyroscope(
            I2cDevice i2cDevice,
            AccelerometerRange accelerometerRange = AccelerometerRange.Range8G,
            GyroscopeRange gyroscopeRange = GyroscopeRange.Range2000Dps,
            AccelerometerOutputDataRate accelerometerOdr = AccelerometerOutputDataRate.Odr100Hz,
            GyroscopeOutputDataRate gyroscopeOdr = GyroscopeOutputDataRate.Odr200Hz)
        {
            _i2c = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

            // Step 1: Soft reset
            WriteByte(Register.Command, SoftResetCommand);
            Thread.Sleep(10);

            // Step 2: Verify chip ID
            byte chipId = ReadByte(Register.ChipId);
            if (chipId != ExpectedChipId)
            {
                throw new IOException($"BMI270 chip ID mismatch. Expected 0x{ExpectedChipId:X2}, got 0x{chipId:X2}.");
            }

            // Step 3: Disable advanced power save mode
            WriteByte(Register.PowerConfig, 0x00);
            Thread.Sleep(1);

            // Step 4-6: Upload config file
            UploadConfigFile();

            // Bosch's reference sequence re-enables advanced power save immediately
            // after the config blob is committed and before INTERNAL_STATUS is checked.
            WriteByte(Register.PowerConfig, 0x01);
            Thread.Sleep(1);

            // Step 7: Verify initialization success.
            // Bosch's initialization sequence requires noticeably more time than a single short delay,
            // especially after the 8 KB config file has been committed.
            if (!WaitForInitialization())
            {
                byte internalStatus = ReadByte(Register.InternalStatus);
                byte errorStatus = ReadByte(Register.Error);
                throw new IOException($"BMI270 initialization failed. INTERNAL_STATUS = 0x{internalStatus:X2}, ERR_REG = 0x{errorStatus:X2}.");
            }

            // Step 8: Enable accelerometer, gyroscope and temperature sensor
            // PWR_CTRL: bit 3 = temp_en, bit 2 = acc_en, bit 1 = gyr_en, bit 0 = aux_en
            WriteByte(Register.PowerControl, 0x0E);
            Thread.Sleep(1);

            // Step 9: Set power mode to normal (disable advanced power save)
            WriteByte(Register.PowerConfig, 0x00);
            Thread.Sleep(1);

            // Step 10: Configure accelerometer ODR and bandwidth
            _accelerometerOdr = accelerometerOdr;
            WriteByte(Register.AccelerometerConfig, (byte)(AccelerometerConfigBase | (byte)accelerometerOdr));
            Thread.Sleep(1);

            // Step 11: Configure gyroscope ODR and bandwidth
            _gyroscopeOdr = gyroscopeOdr;
            WriteByte(Register.GyroscopeConfig, (byte)(GyroscopeConfigBase | (byte)gyroscopeOdr));
            Thread.Sleep(1);

            // Step 12: Set ranges
            _accelerometerRange = accelerometerRange;
            _gyroscopeRange = gyroscopeRange;
            WriteByte(Register.AccelerometerRange, (byte)accelerometerRange);
            Thread.Sleep(1);
            WriteByte(Register.GyroscopeRange, (byte)gyroscopeRange);
            Thread.Sleep(100);
        }

        /// <summary>
        /// Gets or sets the accelerometer full-scale range.
        /// </summary>
        public AccelerometerRange AccelerometerScale
        {
            get => _accelerometerRange;
            set
            {
                WriteByte(Register.AccelerometerRange, (byte)value);
                _accelerometerRange = value;
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Gets or sets the accelerometer output data rate.
        /// </summary>
        public AccelerometerOutputDataRate AccelerometerOdr
        {
            get => _accelerometerOdr;
            set
            {
                WriteByte(Register.AccelerometerConfig, (byte)(AccelerometerConfigBase | (byte)value));
                _accelerometerOdr = value;
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Gets or sets the gyroscope output data rate.
        /// </summary>
        public GyroscopeOutputDataRate GyroscopeOdr
        {
            get => _gyroscopeOdr;
            set
            {
                WriteByte(Register.GyroscopeConfig, (byte)(GyroscopeConfigBase | (byte)value));
                _gyroscopeOdr = value;
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Gets or sets the gyroscope full-scale range.
        /// </summary>
        public GyroscopeRange GyroscopeScale
        {
            get => _gyroscopeRange;
            set
            {
                WriteByte(Register.GyroscopeRange, (byte)value);
                _gyroscopeRange = value;
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Reads the current accelerometer values, scaled to g.
        /// </summary>
        /// <returns>Acceleration vector in g.</returns>
        [Telemetry]
        public Vector3 GetAccelerometer()
        {
            Vector3 raw = ReadAccelerometerRaw();
            double divisor = GetAccelerometerDivisor();
            return new Vector3(raw.X / divisor, raw.Y / divisor, raw.Z / divisor);
        }

        /// <summary>
        /// Reads the current gyroscope values, scaled to degrees per second (dps).
        /// </summary>
        /// <returns>Angular rate vector in dps.</returns>
        [Telemetry]
        public Vector3 GetGyroscope()
        {
            Vector3 raw = ReadGyroscopeRaw();
            double divisor = GetGyroscopeDivisor();
            return new Vector3(raw.X / divisor, raw.Y / divisor, raw.Z / divisor);
        }

        /// <summary>
        /// Reads the internal temperature sensor.
        /// </summary>
        /// <returns>Temperature of the sensor die.</returns>
        [Telemetry]
        public Temperature GetInternalTemperature()
        {
            SpanByte data = new byte[2];
            ReadRegister(Register.TemperatureLsb, data);
            short raw = BinaryPrimitives.ReadInt16LittleEndian(data);

            // BMI270 datasheet: Temperature in degrees C = (raw / 512) + 23
            double tempC = (raw / 512.0) + 23.0;
            return new Temperature(tempC, UnitsNet.Units.TemperatureUnit.DegreeCelsius);
        }

        /// <summary>
        /// Calibrates the gyroscope by calculating the offset values.
        /// Keep the sensor still and in a fixed position during calibration.
        /// </summary>
        /// <param name="iterations">Number of samples to average.</param>
        /// <returns>The calculated offset vector.</returns>
        public Vector3 Calibrate(int iterations)
        {
            if (iterations <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(iterations), "iterations must be greater than 0.");
            }

            var gyrSum = new double[3];

            for (int i = 0; i < iterations; i++)
            {
                var gyr = ReadGyroscopeRaw();
                gyrSum[0] += gyr.X;
                gyrSum[1] += gyr.Y;
                gyrSum[2] += gyr.Z;
                Thread.Sleep(2);
            }

            Vector3 offset = new Vector3(
                gyrSum[0] / iterations,
                gyrSum[1] / iterations,
                gyrSum[2] / iterations);

            SetGyroscopeOffset(offset);
            return offset;
        }

        /// <summary>
        /// Sets the gyroscope offset compensation registers.
        /// </summary>
        /// <param name="offset">The offset vector (raw values).</param>
        public void SetGyroscopeOffset(Vector3 offset)
        {
            // BMI270 gyroscope offset registers: 0x74-0x77
            // 0x74: GYR_OFF_X [7:0], 0x75: GYR_OFF_Y [7:0], 0x76: GYR_OFF_Z [7:0]
            // 0x77: GYR_OFF_EN (bit 6) + upper 2 bits of each axis offset
            byte gyrOffsetEn = 0x40; // bit 6 enables offset compensation

            short offX = (short)offset.X;
            short offY = (short)offset.Y;
            short offZ = (short)offset.Z;

            SpanByte regData = new byte[5];
            regData[0] = (byte)Register.GyroscopeOffsetX;
            regData[1] = (byte)(offX & 0xFF);
            regData[2] = (byte)(offY & 0xFF);
            regData[3] = (byte)(offZ & 0xFF);
            regData[4] = (byte)(gyrOffsetEn | ((offX >> 8) & 0x03) | (((offY >> 8) & 0x03) << 2) | (((offZ >> 8) & 0x03) << 4));

            _i2c.Write(regData);
        }

        /// <summary>
        /// Puts the sensor into suspend mode (low power).
        /// </summary>
        public void Sleep()
        {
            // Disable accelerometer and gyroscope
            WriteByte(Register.PowerControl, 0x00);
            Thread.Sleep(1);

            // Enable advanced power save
            WriteByte(Register.PowerConfig, 0x01);
            Thread.Sleep(1);
        }

        /// <summary>
        /// Enables the BMI270 auxiliary I2C master interface to communicate with
        /// a secondary sensor (e.g. BMM150 magnetometer) on the auxiliary bus.
        /// </summary>
        /// <param name="auxiliaryDeviceAddress">
        /// The 7-bit I2C address of the auxiliary device (e.g. 0x10 for BMM150).
        /// </param>
        /// <param name="manualMode">
        /// When true, configures manual-access mode where reads/writes are triggered
        /// by setting AUX_RD_ADDR or AUX_WR_ADDR/DATA. This is the mode used by
        /// <see cref="Bmm150I2cBmi270"/>. Default is true.
        /// </param>
        public void EnableAuxiliaryI2c(byte auxiliaryDeviceAddress, bool manualMode = true)
        {
            if (auxiliaryDeviceAddress < 0x08 || auxiliaryDeviceAddress > 0x77)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(auxiliaryDeviceAddress),
                    "Auxiliary I2C device address must be a 7-bit address in the range 0x08 to 0x77.");
            }

            // Match M5Unified CoreS3 AUX setup.
            // IF_CONF bit 5 enables the AUX I2C interface path.
            byte ifConf = ReadByte(Register.InterfaceConfig);
            WriteByte(Register.InterfaceConfig, (byte)(ifConf | 0x20));
            Thread.Sleep(1);

            // Ensure advanced power save is disabled while configuring AUX.
            WriteByte(Register.PowerConfig, 0x00);
            Thread.Sleep(1);

            // Keep AUX sensor disabled during setup, as in M5Unified.
            byte pwrCtrl = ReadByte(Register.PowerControl);
            WriteByte(Register.PowerControl, (byte)(pwrCtrl & 0xFE));
            Thread.Sleep(1);

            // Set auxiliary device I2C address (AUX_DEV_ID register 0x4B).
            // Bits [7:1] contain the 7-bit address.
            WaitForAuxNotBusy();
            WriteByte(Register.AuxDeviceId, (byte)(auxiliaryDeviceAddress << 1));
            Thread.Sleep(1);

            if (manualMode)
            {
                // Manual mode, burst length 1 (M5Unified uses 0x80 for aux register access).
                WaitForAuxNotBusy();
                WriteByte(Register.AuxInterfaceConfig, 0x80);
                Thread.Sleep(1);
            }
            else
            {
                // Automatic mode with 8-byte burst for continuous AUX data reads.
                WaitForAuxNotBusy();
                WriteByte(Register.AuxInterfaceConfig, 0x4F);
                Thread.Sleep(1);

                // In automatic mode, enable AUX sensor power.
                pwrCtrl = ReadByte(Register.PowerControl);
                WriteByte(Register.PowerControl, (byte)(pwrCtrl | 0x01));
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Wakes the sensor from suspend mode.
        /// </summary>
        public void WakeUp()
        {
            // Disable advanced power save
            WriteByte(Register.PowerConfig, 0x00);
            Thread.Sleep(1);

            // Enable accelerometer, gyroscope and temperature sensor
            WriteByte(Register.PowerControl, 0x0E);
            Thread.Sleep(50);
        }

        #region Step Counter and Activity Recognition

        /// <summary>
        /// Enables the built-in step counter feature.
        /// The step count is accumulated in internal registers and can be read via <see cref="GetStepCount"/>.
        /// </summary>
        /// <param name="enableActivityRecognition">
        /// When true, also enables activity recognition (still/walking/running).
        /// Activity can be read via <see cref="GetActivity"/>.
        /// </param>
        public void EnableStepCounter(bool enableActivityRecognition = false)
        {
            // BMI270 step counter/activity feature config:
            // Feature config space byte address 0x0A (start_addr 0x08 + enable offset 2)
            // Page = 0x0A / 16 = 0, byte offset = 0x0A % 16 = 10
            // Register = 0x30 + 10 = 0x3A
            // Bit 0 = step_counter_en, Bit 3 = activity_en
            WriteByte(Register.FeaturePage, 0x00);
            Thread.Sleep(1);

            SpanByte pageData = new byte[16];
            ReadRegister((Register)0x30, pageData);

            byte enableByte = pageData[10];
            enableByte |= 0x01; // step counter enable

            if (enableActivityRecognition)
            {
                enableByte |= 0x08; // activity enable
            }

            pageData[10] = enableByte;

            // Write the full page back
            SpanByte writeData = new byte[17];
            writeData[0] = 0x30;
            for (int i = 0; i < 16; i++)
            {
                writeData[i + 1] = pageData[i];
            }

            _i2c.Write(writeData);
            Thread.Sleep(1);
        }

        /// <summary>
        /// Disables the step counter and activity recognition features.
        /// </summary>
        public void DisableStepCounter()
        {
            WriteByte(Register.FeaturePage, 0x00);
            Thread.Sleep(1);

            SpanByte pageData = new byte[16];
            ReadRegister((Register)0x30, pageData);

            // Clear step counter enable (bit 0), step detector (bit 1), and activity enable (bit 3)
            pageData[10] &= 0xF4;

            SpanByte writeData = new byte[17];
            writeData[0] = 0x30;
            for (int i = 0; i < 16; i++)
            {
                writeData[i + 1] = pageData[i];
            }

            _i2c.Write(writeData);
            Thread.Sleep(1);
        }

        /// <summary>
        /// Reads the current step count from the step counter output registers.
        /// The step counter must be enabled via <see cref="EnableStepCounter"/> first.
        /// </summary>
        /// <returns>The accumulated step count since the last reset.</returns>
        [Telemetry]
        public int GetStepCount()
        {
            SpanByte data = new byte[2];
            ReadRegister(Register.StepCounterOutput0, data);
            return BinaryPrimitives.ReadUInt16LittleEndian(data);
        }

        /// <summary>
        /// Resets the step counter to zero.
        /// </summary>
        public void ResetStepCounter()
        {
            // BMI270 step counter reset: feature config page 0, byte offset 8 (start addr 0x08)
            // The reset_counter bit is bit 2 of the word at start_addr offset 0
            WriteByte(Register.FeaturePage, 0x00);
            Thread.Sleep(1);

            SpanByte pageData = new byte[16];
            ReadRegister((Register)0x30, pageData);

            // Set reset bit (bit 2 at byte offset 8)
            pageData[8] |= 0x04;

            SpanByte writeData = new byte[17];
            writeData[0] = 0x30;
            for (int i = 0; i < 16; i++)
            {
                writeData[i + 1] = pageData[i];
            }

            _i2c.Write(writeData);
            Thread.Sleep(10);

            // Clear the reset bit
            pageData[8] &= 0xFB;
            writeData[0] = 0x30;
            for (int i = 0; i < 16; i++)
            {
                writeData[i + 1] = pageData[i];
            }

            _i2c.Write(writeData);
            Thread.Sleep(1);
        }

        /// <summary>
        /// Reads the current recognized activity type.
        /// Activity recognition must be enabled via <see cref="EnableStepCounter"/> with
        /// <c>enableActivityRecognition = true</c>.
        /// </summary>
        /// <returns>The current activity type.</returns>
        [Telemetry]
        public ActivityType GetActivity()
        {
            byte reg = ReadByte(Register.WristGestureActivity);
            return (ActivityType)(reg & 0x0F);
        }

        #endregion

        #region Interrupt Configuration

        /// <summary>
        /// Configures an interrupt output pin (INT1 or INT2).
        /// </summary>
        /// <param name="pinNumber">Pin number: 1 for INT1, 2 for INT2.</param>
        /// <param name="outputEnable">True to enable the pin output.</param>
        /// <param name="activeHigh">True for active-high, false for active-low.</param>
        /// <param name="openDrain">True for open-drain, false for push-pull.</param>
        public void ConfigureInterruptPin(int pinNumber, bool outputEnable = true, bool activeHigh = true, bool openDrain = false)
        {
            if (pinNumber != 1 && pinNumber != 2)
            {
                throw new ArgumentOutOfRangeException(nameof(pinNumber), "pinNumber must be 1 (INT1) or 2 (INT2).");
            }

            // INT1_IO_CTRL (0x53) / INT2_IO_CTRL (0x54):
            // Bit 3 = output_en, Bit 2 = od (open drain), Bit 1 = lvl (active high), Bit 0 = edge
            Register reg = pinNumber == 1 ? Register.Int1IoControl : Register.Int2IoControl;

            byte val = 0x00;
            if (outputEnable)
            {
                val |= 0x08;
            }

            if (openDrain)
            {
                val |= 0x04;
            }

            if (activeHigh)
            {
                val |= 0x02;
            }

            WriteByte(reg, val);
            Thread.Sleep(1);
        }

        /// <summary>
        /// Maps feature interrupt sources to an interrupt pin.
        /// </summary>
        /// <param name="pinNumber">Pin number: 1 for INT1, 2 for INT2.</param>
        /// <param name="sources">Feature interrupt sources to map.</param>
        public void MapFeatureInterrupt(int pinNumber, FeatureInterruptSource sources)
        {
            Register reg = pinNumber == 1 ? Register.Int1MapFeature : Register.Int2MapFeature;
            WriteByte(reg, (byte)sources);
            Thread.Sleep(1);
        }

        /// <summary>
        /// Maps data interrupt sources to an interrupt pin.
        /// </summary>
        /// <param name="pinNumber">Pin number: 1 for INT1, 2 for INT2.</param>
        /// <param name="sources">Data interrupt sources to map.</param>
        public void MapDataInterrupt(int pinNumber, DataInterruptSource sources)
        {
            // INT_MAP_DATA (0x58): INT1 uses bits [3:0], INT2 uses bits [7:4]
            byte current = ReadByte(Register.IntMapData);

            if (pinNumber == 1)
            {
                current = (byte)((current & 0xF0) | ((byte)sources & 0x0F));
            }
            else
            {
                current = (byte)((current & 0x0F) | (((byte)sources & 0x0F) << 4));
            }

            WriteByte(Register.IntMapData, current);
            Thread.Sleep(1);
        }

        /// <summary>
        /// Reads the feature interrupt status register.
        /// Reading this register clears the latched interrupt status.
        /// </summary>
        /// <returns>Active feature interrupt sources.</returns>
        public FeatureInterruptSource GetFeatureInterruptStatus()
        {
            return (FeatureInterruptSource)ReadByte(Register.InterruptStatus0);
        }

        /// <summary>
        /// Reads the data interrupt status register.
        /// </summary>
        /// <returns>Active data interrupt sources.</returns>
        public DataInterruptSource GetDataInterruptStatus()
        {
            byte status = ReadByte(Register.InterruptStatus1);

            // INT_STATUS_1: bit 7=acc_drdy, bit 6=gyr_drdy, bit 5=aux_drdy,
            //               bit 2=err, bit 1=fwm, bit 0=ffull
            DataInterruptSource result = DataInterruptSource.None;
            if ((status & 0x01) != 0)
            {
                result |= DataInterruptSource.FifoFull;
            }

            if ((status & 0x02) != 0)
            {
                result |= DataInterruptSource.FifoWatermark;
            }

            if ((status & 0x80) != 0 || (status & 0x40) != 0)
            {
                result |= DataInterruptSource.DataReady;
            }

            if ((status & 0x04) != 0)
            {
                result |= DataInterruptSource.Error;
            }

            return result;
        }

        #endregion

        #region FIFO

        /// <summary>
        /// Enables the FIFO buffer for accelerometer and/or gyroscope data.
        /// </summary>
        /// <param name="enableAccelerometer">True to store accelerometer data in FIFO.</param>
        /// <param name="enableGyroscope">True to store gyroscope data in FIFO.</param>
        /// <param name="watermarkBytes">
        /// FIFO watermark level in bytes. When the FIFO fill level reaches this value,
        /// a watermark interrupt can be generated. Set to 0 to disable watermark. Max 8191.
        /// </param>
        /// <param name="stopOnFull">True to stop writing when FIFO is full, false to overwrite oldest data.</param>
        public void EnableFifo(bool enableAccelerometer, bool enableGyroscope, int watermarkBytes = 0, bool stopOnFull = false)
        {
            // FIFO_CONFIG_1 (0x49): bit 7=acc_en, bit 6=gyr_en, bit 4=header_en
            byte config1 = 0x00;
            if (enableAccelerometer)
            {
                config1 |= 0x80;
            }

            if (enableGyroscope)
            {
                config1 |= 0x40;
            }

            // FIFO_CONFIG_0 (0x48): bit 0 = stop_on_full
            byte config0 = 0x00;
            if (stopOnFull)
            {
                config0 |= 0x01;
            }

            WriteByte(Register.FifoConfig0, config0);
            WriteByte(Register.FifoConfig1, config1);
            Thread.Sleep(1);

            // Set watermark
            if (watermarkBytes > 0)
            {
                if (watermarkBytes > 8191)
                {
                    watermarkBytes = 8191;
                }

                WriteByte(Register.FifoWatermark0, (byte)(watermarkBytes & 0xFF));
                WriteByte(Register.FifoWatermark1, (byte)((watermarkBytes >> 8) & 0x1F));
            }
        }

        /// <summary>
        /// Disables the FIFO buffer.
        /// </summary>
        public void DisableFifo()
        {
            WriteByte(Register.FifoConfig1, 0x00);
            Thread.Sleep(1);
        }

        /// <summary>
        /// Gets the number of bytes currently stored in the FIFO buffer.
        /// </summary>
        /// <returns>Number of bytes available in the FIFO.</returns>
        public int GetFifoByteCount()
        {
            SpanByte data = new byte[2];
            ReadRegister(Register.FifoLength0, data);
            return BinaryPrimitives.ReadUInt16LittleEndian(data) & 0x1FFF;
        }

        /// <summary>
        /// Reads raw bytes from the FIFO buffer.
        /// </summary>
        /// <param name="buffer">Buffer to read FIFO data into.</param>
        /// <returns>Number of bytes actually read.</returns>
        /// <remarks>
        /// In headerless mode with accelerometer + gyroscope enabled, each frame is 12 bytes:
        /// 6 bytes accelerometer (X,Y,Z as 16-bit little-endian) followed by
        /// 6 bytes gyroscope (X,Y,Z as 16-bit little-endian).
        /// </remarks>
        public int ReadFifo(SpanByte buffer)
        {
            int available = GetFifoByteCount();
            int toRead = buffer.Length;
            if (toRead > available)
            {
                toRead = available;
            }

            if (toRead <= 0)
            {
                return 0;
            }

            _i2c.WriteByte((byte)Register.FifoData);
            _i2c.Read(buffer.Slice(0, toRead));
            return toRead;
        }

        /// <summary>
        /// Flushes (clears) all data from the FIFO buffer.
        /// </summary>
        public void FlushFifo()
        {
            // Write 0xB0 to CMD register to flush FIFO
            WriteByte(Register.Command, 0xB0);
            Thread.Sleep(1);
        }

        #endregion

        /// <inheritdoc />
        public void Dispose()
        {
            _i2c?.Dispose();
            _i2c = null;
        }

        private Vector3 ReadAccelerometerRaw()
        {
            SpanByte data = new byte[6];
            ReadRegister(Register.AccelerometerXLsb, data);

            short x = BinaryPrimitives.ReadInt16LittleEndian(data.Slice(0, 2));
            short y = BinaryPrimitives.ReadInt16LittleEndian(data.Slice(2, 2));
            short z = BinaryPrimitives.ReadInt16LittleEndian(data.Slice(4, 2));

            return new Vector3(x, y, z);
        }

        private Vector3 ReadGyroscopeRaw()
        {
            SpanByte data = new byte[6];
            ReadRegister(Register.GyroscopeXLsb, data);

            short x = BinaryPrimitives.ReadInt16LittleEndian(data.Slice(0, 2));
            short y = BinaryPrimitives.ReadInt16LittleEndian(data.Slice(2, 2));
            short z = BinaryPrimitives.ReadInt16LittleEndian(data.Slice(4, 2));

            return new Vector3(x, y, z);
        }

        private double GetAccelerometerDivisor()
        {
            // 16-bit signed, full-scale maps to 32768
            switch (_accelerometerRange)
            {
                case AccelerometerRange.Range2G:
                    return 32768.0 / 2.0;
                case AccelerometerRange.Range4G:
                    return 32768.0 / 4.0;
                case AccelerometerRange.Range8G:
                    return 32768.0 / 8.0;
                case AccelerometerRange.Range16G:
                    return 32768.0 / 16.0;
                default:
                    return 32768.0 / 8.0;
            }
        }

        private double GetGyroscopeDivisor()
        {
            // 16-bit signed, full-scale maps to 32768
            switch (_gyroscopeRange)
            {
                case GyroscopeRange.Range125Dps:
                    return 32768.0 / 125.0;
                case GyroscopeRange.Range250Dps:
                    return 32768.0 / 250.0;
                case GyroscopeRange.Range500Dps:
                    return 32768.0 / 500.0;
                case GyroscopeRange.Range1000Dps:
                    return 32768.0 / 1000.0;
                case GyroscopeRange.Range2000Dps:
                    return 32768.0 / 2000.0;
                default:
                    return 32768.0 / 2000.0;
            }
        }

        private void UploadConfigFile()
        {
            byte[] configData = Bmi270Config.ConfigFile;

            // Prepare for config load
            SetConfigLoad(false);
            Thread.Sleep(1);

            // Write config in bursts
            int totalBytes = configData.Length;
            for (int offset = 0; offset < totalBytes; offset += ConfigBurstSize)
            {
                int burstLength = totalBytes - offset;
                if (burstLength > ConfigBurstSize)
                {
                    burstLength = ConfigBurstSize;
                }

                // Bosch encodes the 12-bit config address split across INIT_ADDR_0/1:
                // low 4 bits in INIT_ADDR_0, upper 8 bits in INIT_ADDR_1.
                int wordAddress = offset / 2;
                WriteByte(Register.InitAddress0, (byte)(wordAddress & 0x0F));
                WriteByte(Register.InitAddress1, (byte)((wordAddress >> 4) & 0xFF));

                // Write burst: register address + data
                SpanByte burst = new byte[burstLength + 1];
                burst[0] = (byte)Register.InitData;
                for (int i = 0; i < burstLength; i++)
                {
                    burst[i + 1] = configData[offset + i];
                }

                _i2c.Write(burst);
                Thread.Sleep(ConfigBurstDelayMs);
            }

            // Complete config load
            SetConfigLoad(true);
        }

        private void SetConfigLoad(bool enable)
        {
            byte initControl = ReadByte(Register.InitControl);
            initControl = enable ? (byte)(initControl | 0x01) : (byte)(initControl & 0xFE);
            WriteByte(Register.InitControl, initControl);
        }

        private bool WaitForInitialization()
        {
            for (int elapsed = 0; elapsed < ConfigReadyTimeoutMs; elapsed += ConfigReadyPollDelayMs)
            {
                Thread.Sleep(ConfigReadyPollDelayMs);

                if ((ReadByte(Register.InternalStatus) & 0x01) == 0x01)
                {
                    return true;
                }
            }

            return false;
        }

        private void WaitForAuxNotBusy()
        {
            for (int i = 0; i < AuxBusyPollRetries; i++)
            {
                if ((ReadByte(Register.Status) & AuxBusyMask) == 0)
                {
                    return;
                }

                Thread.Sleep(AuxBusyPollDelayMs);
            }
        }

        private void WriteByte(Register register, byte data)
        {
            SpanByte buff = new byte[2]
            {
                (byte)register,
                data
            };

            _i2c.Write(buff);
        }

        private byte ReadByte(Register register)
        {
            _i2c.WriteByte((byte)register);
            return _i2c.ReadByte();
        }

        private void ReadRegister(Register register, SpanByte buffer)
        {
            _i2c.WriteByte((byte)register);
            _i2c.Read(buffer);
        }
    }
}
