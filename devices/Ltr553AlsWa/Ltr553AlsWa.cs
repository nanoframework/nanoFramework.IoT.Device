// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Device.Model;
using System.Threading;

namespace Iot.Device.Ltr553AlsWa
{
    /// <summary>
    /// LTR-553ALS-WA proximity and ambient light sensor.
    /// </summary>
    [Interface("LTR-553ALS-WA proximity and ambient light sensor")]
    public class Ltr553AlsWa : IDisposable
    {
        /// <summary>
        /// Default I2C address for the LTR-553ALS-WA (fixed at 0x23).
        /// </summary>
        public const int DefaultI2cAddress = 0x23;

        /// <summary>
        /// Expected manufacturer ID for Lite-On Technology.
        /// </summary>
        public const byte ExpectedManufacturerId = 0x05;

        private I2cDevice _i2cDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ltr553AlsWa"/> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to communicate with the sensor.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is null.</exception>
        public Ltr553AlsWa(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

            // Software reset to ensure known state
            Reset();
        }

        /// <summary>
        /// Gets the part identification value.
        /// </summary>
        [Property]
        public byte PartId => ReadRegister(Register.PartId);

        /// <summary>
        /// Gets the manufacturer identification value. Expected to be 0x05 for Lite-On.
        /// </summary>
        [Property]
        public byte ManufacturerId => ReadRegister(Register.ManufacturerId);

        /// <summary>
        /// Gets or sets a value indicating whether the ambient light sensor is active.
        /// </summary>
        [Property]
        public bool AlsEnabled
        {
            get => (ReadRegister(Register.AlsContr) & 0x01) != 0;
            set => UpdateRegisterBits(Register.AlsContr, 0x01, value ? (byte)0x01 : (byte)0x00);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the proximity sensor is active.
        /// </summary>
        [Property]
        public bool PsEnabled
        {
            get => (ReadRegister(Register.PsContr) & 0x02) != 0;
            set => UpdateRegisterBits(Register.PsContr, 0x02, value ? (byte)0x02 : (byte)0x00);
        }

        /// <summary>
        /// Gets or sets the ALS gain.
        /// </summary>
        [Property]
        public AlsGain AlsGain
        {
            get => (AlsGain)((ReadRegister(Register.AlsContr) >> 2) & 0x07);
            set => UpdateRegisterBits(Register.AlsContr, 0x1C, (byte)((byte)value << 2));
        }

        /// <summary>
        /// Gets or sets the ALS integration time.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when setting an unsupported integration time value.</exception>
        /// <exception cref="ArgumentException">Thrown when the integration time is greater than the current measurement rate.</exception>
        [Property]
        public AlsIntegrationTime AlsIntegrationTime
        {
            get => (AlsIntegrationTime)((ReadRegister(Register.AlsMeasRate) >> 3) & 0x07);
            set
            {
                int integrationTimeMs = GetAlsIntegrationTimeMilliseconds(value);
                int measurementRateMs = GetAlsMeasurementRateMilliseconds(AlsMeasurementRate);
                if (measurementRateMs < integrationTimeMs)
                {
                    throw new ArgumentException();
                }

                UpdateRegisterBits(Register.AlsMeasRate, 0x38, (byte)((byte)value << 3));
            }
        }

        /// <summary>
        /// Gets or sets the ALS measurement rate.
        /// Must be equal to or larger than the integration time.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when setting an unsupported measurement rate value.</exception>
        /// <exception cref="ArgumentException">Thrown when the measurement rate is less than the current integration time.</exception>
        [Property]
        public AlsMeasurementRate AlsMeasurementRate
        {
            get => (AlsMeasurementRate)(ReadRegister(Register.AlsMeasRate) & 0x07);
            set
            {
                int measurementRateMs = GetAlsMeasurementRateMilliseconds(value);
                int integrationTimeMs = GetAlsIntegrationTimeMilliseconds(AlsIntegrationTime);
                if (measurementRateMs < integrationTimeMs)
                {
                    throw new ArgumentException("ALS measurement rate must be greater than or equal to ALS integration time.", nameof(value));
                }

                UpdateRegisterBits(Register.AlsMeasRate, 0x07, (byte)value);
            }
        }

        /// <summary>
        /// Gets or sets the PS measurement rate.
        /// </summary>
        [Property]
        public PsMeasurementRate PsMeasurementRate
        {
            get => (PsMeasurementRate)(ReadRegister(Register.PsMeasRate) & 0x0F);
            set
            {
                byte val = (byte)((byte)value & 0x0F);
                WriteRegister(Register.PsMeasRate, val);
            }
        }

        /// <summary>
        /// Gets or sets the PS LED pulse frequency.
        /// </summary>
        [Property]
        public LedPulseFrequency LedPulseFrequency
        {
            get => (LedPulseFrequency)((ReadRegister(Register.PsLed) >> 5) & 0x07);
            set => UpdateRegisterBits(Register.PsLed, 0xE0, (byte)((byte)value << 5));
        }

        /// <summary>
        /// Gets or sets the PS LED duty cycle.
        /// </summary>
        [Property]
        public LedDutyCycle LedDutyCycle
        {
            get => (LedDutyCycle)((ReadRegister(Register.PsLed) >> 3) & 0x03);
            set => UpdateRegisterBits(Register.PsLed, 0x18, (byte)((byte)value << 3));
        }

        /// <summary>
        /// Gets or sets the PS LED peak current.
        /// </summary>
        [Property]
        public LedPeakCurrent LedPeakCurrent
        {
            get => (LedPeakCurrent)(ReadRegister(Register.PsLed) & 0x07);
            set => UpdateRegisterBits(Register.PsLed, 0x07, (byte)value);
        }

        /// <summary>
        /// Gets or sets the number of PS LED pulses (1–15).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when value is outside the valid range of 1 to 15.</exception>
        [Property]
        public byte LedPulseCount
        {
            get => (byte)(ReadRegister(Register.PsNPulses) & 0x0F);
            set
            {
                if (value < 1 || value > 15)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                WriteRegister(Register.PsNPulses, value);
            }
        }

        /// <summary>
        /// Reads the 11-bit proximity sensor value (0–2047).
        /// Higher values indicate closer objects.
        /// </summary>
        /// <returns>Raw proximity sensor value.</returns>
        public ushort GetProximity()
        {
            SpanByte buffer = new byte[2];
            ReadRegisterBlock(Register.PsDataLow, buffer);
            return (ushort)((buffer[0] & 0xFF) | ((buffer[1] & 0x07) << 8));
        }

        /// <summary>
        /// Reads the 11-bit proximity sensor value and reports whether the sensor is saturated.
        /// When saturated, the proximity value is unreliable.
        /// </summary>
        /// <param name="saturated">True if the PS sensor is saturated (object too close or IR too strong).</param>
        /// <returns>Raw proximity sensor value (0–2047).</returns>
        public ushort GetProximity(out bool saturated)
        {
            SpanByte buffer = new byte[2];
            ReadRegisterBlock(Register.PsDataLow, buffer);
            saturated = (buffer[1] & 0x80) != 0;
            return (ushort)((buffer[0] & 0xFF) | ((buffer[1] & 0x07) << 8));
        }

        /// <summary>
        /// Reads the 16-bit ALS channel 0 value (visible + IR light).
        /// </summary>
        /// <returns>Raw ALS channel 0 value.</returns>
        public ushort GetAlsChannel0()
        {
            SpanByte buffer = new byte[2];
            ReadRegisterBlock(Register.AlsDataCh0Low, buffer);
            return (ushort)(buffer[0] | (buffer[1] << 8));
        }

        /// <summary>
        /// Reads the 16-bit ALS channel 1 value (IR only).
        /// </summary>
        /// <returns>Raw ALS channel 1 value.</returns>
        public ushort GetAlsChannel1()
        {
            SpanByte buffer = new byte[2];
            ReadRegisterBlock(Register.AlsDataCh1Low, buffer);
            return (ushort)(buffer[0] | (buffer[1] << 8));
        }

        /// <summary>
        /// Reads both ALS channels in a single burst.
        /// Registers 0x88–0x8B: CH1 low, CH1 high, CH0 low, CH0 high.
        /// </summary>
        /// <param name="channel0">The ALS channel 0 value (visible + IR).</param>
        /// <param name="channel1">The ALS channel 1 value (IR only).</param>
        public void GetAlsData(out ushort channel0, out ushort channel1)
        {
            SpanByte buffer = new byte[4];
            ReadRegisterBlock(Register.AlsDataCh1Low, buffer);
            channel1 = (ushort)(buffer[0] | (buffer[1] << 8));
            channel0 = (ushort)(buffer[2] | (buffer[3] << 8));
        }

        /// <summary>
        /// Gets a value indicating whether new ALS data is available.
        /// </summary>
        /// <returns>True if new ALS data is ready to be read.</returns>
        public bool IsAlsDataReady()
        {
            byte status = ReadRegister(Register.AlsPsStatus);
            return (status & 0x04) != 0;
        }

        /// <summary>
        /// Gets a value indicating whether new PS data is available.
        /// </summary>
        /// <returns>True if new PS data is ready to be read.</returns>
        public bool IsPsDataReady()
        {
            byte status = ReadRegister(Register.AlsPsStatus);
            return (status & 0x01) != 0;
        }

        /// <summary>
        /// Configures the interrupt mode.
        /// </summary>
        /// <param name="mode">The interrupt mode to set.</param>
        public void SetInterruptMode(InterruptMode mode)
        {
            UpdateRegisterBits(Register.Interrupt, 0x03, (byte)mode);
        }

        /// <summary>
        /// Configures the interrupt pin polarity.
        /// </summary>
        /// <param name="polarity">The interrupt polarity to set.</param>
        public void SetInterruptPolarity(InterruptPolarity polarity)
        {
            UpdateRegisterBits(Register.Interrupt, 0x04, (byte)((byte)polarity << 2));
        }

        /// <summary>
        /// Sets the PS interrupt thresholds.
        /// An interrupt fires when the PS value goes above the upper threshold or below the lower threshold.
        /// </summary>
        /// <param name="lower">Lower threshold (11-bit, 0–2047).</param>
        /// <param name="upper">Upper threshold (11-bit, 0–2047).</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when lower or upper is outside 0 to 2047.</exception>
        /// <exception cref="ArgumentException">Thrown when lower is greater than upper.</exception>
        public void SetPsThreshold(ushort lower, ushort upper)
        {
            if (lower > 2047)
            {
                throw new ArgumentOutOfRangeException(nameof(lower));
            }

            if (upper > 2047)
            {
                throw new ArgumentOutOfRangeException(nameof(upper));
            }

            if (lower > upper)
            {
                throw new ArgumentException("Lower threshold must be less than or equal to upper threshold.");
            }

            WriteRegister(Register.PsThresholdUpLow, (byte)(upper & 0xFF));
            WriteRegister(Register.PsThresholdUpHigh, (byte)((upper >> 8) & 0x07));
            WriteRegister(Register.PsThresholdLowLow, (byte)(lower & 0xFF));
            WriteRegister(Register.PsThresholdLowHigh, (byte)((lower >> 8) & 0x07));
        }

        /// <summary>
        /// Sets the ALS interrupt thresholds.
        /// An interrupt fires when the ALS value goes above the upper threshold or below the lower threshold.
        /// </summary>
        /// <param name="lower">Lower threshold (16-bit).</param>
        /// <param name="upper">Upper threshold (16-bit).</param>
        public void SetAlsThreshold(ushort lower, ushort upper)
        {
            WriteRegister(Register.AlsThresholdUpLow, (byte)(upper & 0xFF));
            WriteRegister(Register.AlsThresholdUpHigh, (byte)((upper >> 8) & 0xFF));
            WriteRegister(Register.AlsThresholdLowLow, (byte)(lower & 0xFF));
            WriteRegister(Register.AlsThresholdLowHigh, (byte)((lower >> 8) & 0xFF));
        }

        /// <summary>
        /// Sets the number of consecutive out-of-range values required before an interrupt fires.
        /// </summary>
        /// <param name="psCount">PS persistence count (0–15), stored in bits [7:4].</param>
        /// <param name="alsCount">ALS persistence count (0–15), stored in bits [3:0].</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when psCount or alsCount is outside the valid range of 0 to 15.</exception>
        public void SetInterruptPersistence(byte psCount, byte alsCount)
        {
            if (psCount > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(psCount));
            }

            if (alsCount > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(alsCount));
            }

            byte value = (byte)((psCount << 4) | alsCount);
            WriteRegister(Register.InterruptPersist, value);
        }

        /// <summary>
        /// Gets the PS interrupt status flag.
        /// </summary>
        /// <returns>True if a PS interrupt is pending.</returns>
        public bool GetPsInterruptStatus()
        {
            byte status = ReadRegister(Register.AlsPsStatus);
            return (status & 0x02) != 0;
        }

        /// <summary>
        /// Gets the ALS interrupt status flag.
        /// </summary>
        /// <returns>True if an ALS interrupt is pending.</returns>
        public bool GetAlsInterruptStatus()
        {
            byte status = ReadRegister(Register.AlsPsStatus);
            return (status & 0x08) != 0;
        }

        /// <summary>
        /// Performs a software reset of the sensor.
        /// After reset, both ALS and PS are in standby mode with default configuration.
        /// </summary>
        public void Reset()
        {
            // Bit 1 of ALS_CONTR (0x80) is the software reset bit.
            // Writing 1 triggers reset; the bit auto-clears.
            byte current = ReadRegister(Register.AlsContr);
            WriteRegister(Register.AlsContr, (byte)(current | 0x02));

            // Wait for the reset to complete
            Thread.Sleep(10);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_i2cDevice != null)
            {
                try
                {
                    // Best-effort reset: communication can fail if device is unpowered.
                    Reset();
                }
                catch (InvalidOperationException)
                {
                }
                finally
                {
                    _i2cDevice.Dispose();
                    _i2cDevice = null;
                }
            }
        }

        private static int GetAlsIntegrationTimeMilliseconds(AlsIntegrationTime integrationTime)
        {
            switch (integrationTime)
            {
                case AlsIntegrationTime.Integration50Ms:
                    return 50;
                case AlsIntegrationTime.Integration100Ms:
                    return 100;
                case AlsIntegrationTime.Integration150Ms:
                    return 150;
                case AlsIntegrationTime.Integration200Ms:
                    return 200;
                case AlsIntegrationTime.Integration250Ms:
                    return 250;
                case AlsIntegrationTime.Integration300Ms:
                    return 300;
                case AlsIntegrationTime.Integration350Ms:
                    return 350;
                case AlsIntegrationTime.Integration400Ms:
                    return 400;
                default:
                    throw new ArgumentOutOfRangeException(nameof(integrationTime));
            }
        }

        private static int GetAlsMeasurementRateMilliseconds(AlsMeasurementRate measurementRate)
        {
            switch (measurementRate)
            {
                case AlsMeasurementRate.Rate50Ms:
                    return 50;
                case AlsMeasurementRate.Rate100Ms:
                    return 100;
                case AlsMeasurementRate.Rate200Ms:
                    return 200;
                case AlsMeasurementRate.Rate500Ms:
                    return 500;
                case AlsMeasurementRate.Rate1000Ms:
                    return 1000;
                case AlsMeasurementRate.Rate2000Ms:
                    return 2000;
                default:
                    throw new ArgumentOutOfRangeException(nameof(measurementRate));
            }
        }

        private byte ReadRegister(Register register)
        {
            SpanByte writeBuffer = new byte[1];
            SpanByte readBuffer = new byte[1];
            writeBuffer[0] = (byte)register;
            var result = _i2cDevice.WriteRead(writeBuffer, readBuffer);
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new InvalidOperationException($"I2C read failed for register 0x{(byte)register:X2}. Status: {result.Status}.");
            }

            return readBuffer[0];
        }

        private void WriteRegister(Register register, byte value)
        {
            SpanByte buffer = new byte[2];
            buffer[0] = (byte)register;
            buffer[1] = value;
            var result = _i2cDevice.Write(buffer);
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new InvalidOperationException($"I2C write failed for register 0x{(byte)register:X2}. Status: {result.Status}.");
            }
        }

        private void ReadRegisterBlock(Register startRegister, SpanByte buffer)
        {
            SpanByte writeBuffer = new byte[1];
            writeBuffer[0] = (byte)startRegister;
            var result = _i2cDevice.WriteRead(writeBuffer, buffer);
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new InvalidOperationException($"I2C block read failed from register 0x{(byte)startRegister:X2}. Status: {result.Status}.");
            }
        }

        private void UpdateRegisterBits(Register register, byte mask, byte value)
        {
            byte current = ReadRegister(register);
            current = (byte)((current & ~mask) | (value & mask));
            WriteRegister(register, current);
        }
    }
}
