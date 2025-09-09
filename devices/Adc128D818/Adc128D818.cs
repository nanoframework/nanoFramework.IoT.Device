// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using UnitsNet;

namespace Iot.Device.Adc128D818
{
    /// <summary>
    /// Driver for the ADC128D818 Analog-to-Digital Converter.
    /// </summary>
    public class Adc128D818 : IDisposable
    {
        // default manufacturer ID for ADC128D818
        private const byte DefaultManufacturerId = 0b0000_0001;

        // default revision ID for ADC128D818
        private const byte DefaultRevisionId = 0b0000_1001;

        // Configuration register masks and bits
        private const byte ConfigurationRegisterStartMask = 0b1111_1110;
        private const byte ConfigurationRegisterInitialize = 0b1000_0000;
        private const byte ConfigurationRegisterStart = 0b0000_0001;

        private const byte DeepShutdownRegisterEnable = 0b0000_0001;

        private const byte OneShotRegisterGo = 0b0000_0001;

        private const byte AdvancedConfigurationRegisterExternalRefMask = 0b0000_0001;
        private const byte AdvancedConfigurationRegisterModeMask = 0b0000_0110;

        private const byte BusyStatusRegisterBusyMask = 0b0000_0001;
        private const byte BusyStatusRegisterNotReady = 0b0000_0010;

        private bool _disposedValue;

        private I2cDevice _i2cDevice;
        private Mode _adcMode;

        /// <summary>
        /// Gets or sets ADC conversion rate.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This register must only be programmed when the device is in shutdown mode.
        /// </para>
        /// <para>
        /// Default is <see cref="ConversionRate.LowPower"/>.
        /// </para>
        /// </remarks>
        public ConversionRate ConversionRate
        {
            get => GetConversionRate();
            set => SetConversionRate(value);
        }

        /// <summary>
        /// Gets a value indicating whether the device is busy performing a conversion.
        /// </summary>
        public bool IsBusy => GetIsBusy();

        /// <summary>
        /// Gets a value indicating whether the device has completed the power-up sequence.
        /// </summary>
        public bool IsReady => !GetIsNotReady();

        /// <summary>
        /// Gets or sets the ADC operating mode.
        /// </summary>
        /// <remarks>
        /// Default is <see cref="Mode.Mode0"/>.
        /// </remarks>
        public Mode Mode
        {
            get => GetMode();
            set => SetMode(value);
        }

        /// <summary>
        /// Gets or sets the ADC voltage reference.
        /// </summary>
        /// <remarks>
        /// Default is <see cref="VoltageReference.Internal2_56"/>.
        /// </remarks>
        public VoltageReference VoltageReference
        {
            get => GetVoltageReferenceMode();
            set => SetVoltageReferenceMode(value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Adc128D818"/> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to communicate with the ADC128D818.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the device ID read from the ADC128D818 does not match the expected values. Or there is a problem communicating with the device.</exception>"
        public Adc128D818(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException();

            // read the device ID to confirm communication
            ReadPartIds();
        }

        /// <summary>
        /// Convert a raw temperature reading from the local temperature sensor to a <see cref="Temperature"/> value.
        /// </summary>
        /// <param name="rawReading">The raw temperature reading from the local temperature sensor (channel <see cref="Channel.Temperature"/>).</param>
        /// <returns>A <see cref="Temperature"/> value representing the converted temperature.</returns>
        public static Temperature ConvertLocalTemperatureReading(int rawReading)
        {
            // from the datasheet (9.1.2 Temperature Measurement System)
            byte msb = (byte)((rawReading >> 8) & 0b0000_0001);

            if (msb == 0)
            {
                // +Temp(°C) = DOUT(dec) / 2
                return Temperature.FromDegreesCelsius(rawReading / 2.0);
            }
            else
            {
                // –Temp(°C) = [2^9 – DOUT(dec)] / 2
                return Temperature.FromDegreesCelsius(-((512 - rawReading) / 2.0));
            }
        }

        /// <summary>
        /// Place the device in deep shutdown mode.
        /// </summary>
        public void DeepShutdown() => EnterDeepShutdown();

        /// <summary>
        /// Disables the specified channel.
        /// </summary>
        /// <param name="channel">The channel to read.</param>
        /// <exception cref="InvalidOperationException">Thrown when the channel is not valid for the current mode.</exception>
        /// <remarks>
        /// Default is all channels enabled.
        /// </remarks>
        public void DisableChannel(Channel channel)
        {
            // computes the bit for the requested channel
            // (this methods performs the validation of the channel vs the mode)
            byte channelMask = (byte)(1 << GetIndexForChannel(channel));

            // read current channel disable register
            byte[] disableBuffer = ReadFromRegister(Register.ChannelDisable, 1);

            // set the bit corresponding to the channel to disable
            disableBuffer[0] |= channelMask;

            // write back the configuration
            WriteToRegister(Register.ChannelDisable, disableBuffer[0]);
        }

        /// <summary>
        /// Restore registers default values.
        /// </summary>
        public void Initialize() => InitializeDevice();

        /// <summary>
        /// Initiate a single conversion and comparison cycle when the device is in shutdown mode or deep shutdown mode.
        /// </summary>
        /// <remarks>
        /// After the convertion the device returns to the respective mode that it was in.
        /// </remarks>
        public void PerformSingleConversion() => WriteToRegister(Register.OneShot, OneShotRegisterGo);

        /// <summary>
        /// Gets the readings of all channels.
        /// </summary>
        /// <returns>An array of integer values representing the readings of all channels.</returns>
        /// <remarks>
        /// <para>
        /// The array contains 8 elements, corresponding to channels IN0 to IN7.
        /// </para>
        /// <para>
        /// Depending on the current <see cref="Mode"/>, some channels may not be valid for reading, therefore the value should be discarded.
        /// </para>
        /// </remarks>
        public int[] ReadAllChannels()
        {
            // array to hold the readings
            int[] readings = new int[8];

            // get readings from all channels
            for (Channel channel = Channel.In0; channel <= Channel.In7; channel++)
            {
                readings[(int)channel] = ReadChannel(channel);
            }

            return readings;
        }

        /// <summary>
        /// Gets the reading of the specified channel.
        /// </summary>
        /// <param name="channel">The channel to read.</param>
        /// <returns>An integer value representing the reading of the specified channel.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the channel is not valid for the current mode.</exception>
        public int ReadChannel(Channel channel)
        {
            // computes the register address for the requested channel
            // (this methods performs the validation of the channel vs the mode)
            Register channelRegister = Register.ChannelReading0 + GetIndexForChannel(channel);

            // read the channel value
            byte[] readBuffer = ReadFromRegister(channelRegister, 2);

            // convert the two bytes to a 16-bit integer
            return (readBuffer[0] << 8) | readBuffer[1];
        }

        /// <summary>
        /// Shutdown the device.
        /// </summary>
        /// <remarks>
        /// This operation disables continuous monitoring.
        /// </remarks>
        public void Shutdown() => ShutdownDevice();

        /// <summary>
        /// Start conversions.
        /// </summary>
        public void Start() => StartOperation();

        #region Configuration Register - Address 00h

        // |        7       |   6 5 4  |     3     |     2    |      1     |   0   |
        // | Initialization | Reserved | INT_Clear | Reserved | INT_Enable | Start |
        ////

        private void InitializeDevice()
        {
            // read current configuration
            byte[] configBuffer = ReadFromRegister(Register.Configuration, 1);

            // set the initialization bit to reset all registers to default values
            configBuffer[0] &= ConfigurationRegisterInitialize;

            // write back the configuration
            WriteToRegister(Register.Configuration, configBuffer[0]);

            // reset our local mode cache
            _adcMode = Mode.Mode0;
        }

        private void StartOperation()
        {
            // read current configuration
            byte[] configBuffer = ReadFromRegister(Register.Configuration, 1);

            // clear the start bit to enable continuous monitoring
            configBuffer[0] &= ConfigurationRegisterStartMask;

            // set the start bit to enable continuous monitoring
            configBuffer[0] |= ConfigurationRegisterStart;

            // write back the configuration
            WriteToRegister(Register.Configuration, configBuffer[0]);
        }

        private void ShutdownDevice()
        {
            // read current configuration
            byte[] configBuffer = ReadFromRegister(Register.Configuration, 1);

            // clear the start bit to disable continuous monitoring
            configBuffer[0] &= ConfigurationRegisterStartMask;

            // write back the configuration
            WriteToRegister(Register.Configuration, configBuffer[0]);
        }

        #endregion

        #region Conversion Rate Register — Address 07h

        // | 7 6 5 4  1 |        0        |
        // |  Reserved  | Conversion rate |
        ////

        private void SetConversionRate(ConversionRate conversionRate)
        {
            // write the configuration
            WriteToRegister(Register.ConversionRate, (byte)conversionRate);
        }

        private ConversionRate GetConversionRate()
        {
            byte[] rateBuffer = ReadFromRegister(Register.ConversionRate, 1);
            return (ConversionRate)rateBuffer[0];
        }

        #endregion

        #region Deep Shutdown Register — Address 0Ah

        // | 7 6 5 4  1 |       0       |
        // |  Reserved  | Deep shutdown |
        ////

        private void EnterDeepShutdown()
        {
            // need to stop the device
            ShutdownDevice();

            // write the deep shutdown bit
            WriteToRegister(Register.ConversionRate, DeepShutdownRegisterEnable);
        }

        #endregion

        #region Advanced Configuration Register — Address 0Bh

        // | 7 6 5 4 3 |    2 1     |       0        |
        // |  Reserved | Mode selet | Ext ref enable |
        ////

        private void SetVoltageReferenceMode(VoltageReference voltageReference)
        {
            // read current configuration
            byte[] configBuffer = ReadFromRegister(Register.AdvancedConfiguration, 1);

            // clear the external reference bit
            configBuffer[0] &= AdvancedConfigurationRegisterExternalRefMask;

            // set the mode bits
            configBuffer[0] |= (byte)voltageReference;

            // write back the configuration
            WriteToRegister(Register.AdvancedConfiguration, configBuffer[0]);
        }

        private VoltageReference GetVoltageReferenceMode()
        {
            byte[] configBuffer = ReadFromRegister(Register.AdvancedConfiguration, 1);
            return (VoltageReference)(configBuffer[0] & AdvancedConfigurationRegisterExternalRefMask);
        }

        private void SetMode(Mode mode)
        {
            // read current configuration
            byte[] configBuffer = ReadFromRegister(Register.AdvancedConfiguration, 1);

            // clear the mode bits
            configBuffer[0] &= AdvancedConfigurationRegisterModeMask;

            // set the mode bits
            configBuffer[0] |= (byte)mode;

            // write back the configuration
            WriteToRegister(Register.AdvancedConfiguration, configBuffer[0]);

            // store locally for quick access
            _adcMode = mode;
        }

        private Mode GetMode()
        {
            byte[] configBuffer = ReadFromRegister(Register.AdvancedConfiguration, 1);
            return (Mode)(configBuffer[0] & AdvancedConfigurationRegisterModeMask);
        }

        #endregion

        #region Busy Status Register — Address 0Ch

        // | 7 6 5 4 3 2 |     1     |  0   |
        // |   Reserved  | Not Ready | Busy |
        ////

        private bool GetIsBusy()
        {
            byte[] statusBuffer = ReadFromRegister(Register.BusyStatus, 1);
            return (statusBuffer[0] & BusyStatusRegisterBusyMask) == BusyStatusRegisterBusyMask;
        }

        private bool GetIsNotReady()
        {
            byte[] statusBuffer = ReadFromRegister(Register.BusyStatus, 1);
            return (statusBuffer[0] & BusyStatusRegisterNotReady) == BusyStatusRegisterNotReady;
        }

        #endregion

        private byte GetIndexForChannel(Channel channel)
        {
            if (channel <= Channel.In3 && (_adcMode == Mode.Mode0 || _adcMode == Mode.Mode1 || _adcMode == Mode.Mode3))
            {
                // IN0, IN1, IN2, IN3 single-ended readings
                return (byte)channel;
            }
            else if (channel >= Channel.In4 && channel <= Channel.In6 && (_adcMode == Mode.Mode0 || _adcMode == Mode.Mode1))
            {
                // IN4, IN5, IN6 single-ended readings
                return (byte)channel;
            }
            else if (channel == Channel.In7 && _adcMode == Mode.Mode1)
            {
                // IN7 single-ended reading
                return (byte)channel;
            }
            else if (channel == Channel.Temperature && (_adcMode == Mode.Mode0 || _adcMode == Mode.Mode2 || _adcMode == Mode.Mode3))
            {
                // Local temperature reading
                return (byte)Channel.In7;
            }
            else if ((channel == Channel.In0In1Differential || channel == Channel.In3In2Differential) && _adcMode == Mode.Mode2)
            {
                // IN0+ and IN3- differential reading
                return channel - Channel.In0In1Differential;
            }
            else if ((channel == Channel.In4In5Differential || channel == Channel.In7In6Differential) && _adcMode == Mode.Mode2)
            {
                // IN4+ and IN7- differential reading
                return (byte)(Channel.In2 + (channel - Channel.In4In5Differential));
            }
            else if ((channel == Channel.In4In5Differential || channel == Channel.In7In6Differential) && _adcMode == Mode.Mode3)
            {
                // IN4+ and IN7- differential reading
                return (byte)(Channel.In4 + (channel - Channel.In4In5Differential));
            }
            else
            {
                // all other combinations are invalid
                throw new InvalidOperationException();
            }
        }

        private byte[] ReadFromRegister(Register register, int readByteCount)
        {
            SpanByte writeBuff = new byte[1] { (byte)register };

            byte[] readBuffer = new byte[readByteCount];

            I2cTransferResult result = _i2cDevice.WriteRead(writeBuff, readBuffer);

            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new InvalidOperationException();
            }

            return readBuffer;
        }

        private void WriteToRegister(Register register, byte content)
        {
            byte[] writeBuffer = new byte[1 + 1];
            writeBuffer[0] = (byte)register;
            writeBuffer[1] = content;

            I2cTransferResult result = _i2cDevice.Write(writeBuffer);

            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new InvalidOperationException();
            }
        }

        private void ReadPartIds()
        {
            byte[] manuFacturerId = ReadFromRegister(Register.ManufacturerId, 1);
            byte[] revisionId = ReadFromRegister(Register.RevisionId, 1);

            if (manuFacturerId[0] != DefaultManufacturerId
                || revisionId[0] != DefaultRevisionId)
            {
                throw new InvalidOperationException();
            }
        }

        #region Dispose pattern

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                if (_i2cDevice != null)
                {
                    _i2cDevice.Dispose();
                    _i2cDevice = null;
                }

                _disposedValue = true;
            }
        }

        /// <inheritdoc/>
        ~Adc128D818()
        {
            Dispose(disposing: false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
