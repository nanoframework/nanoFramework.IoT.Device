////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

using System;
using System.Device.I2c;
using System.Device.Model;
using UnitsNet;

namespace Iot.Device.Dac63004
{
    /// <summary>
    /// Ultra-low-power four-channel 12-bit smart DAC Dac63004.
    /// </summary>
    [Interface("Ultra-low-power four-channel 12-bit smart DAC Dac63004")]
    public class Dac63004 : IDisposable
    {
        // DAC63004/W: 04h
        private const byte DAC63004DeviceId = 0b0001_0000;

        private const byte DevicePartNumberMask = 0b1111_1100;
        private const byte InternalRefEnableMask = 0b0001_0000;
        private const byte InternalRefDisableMask = 0b1110_1111;

        private const byte ConfigPowerUpCurrentOutChannel0Mask = 0b0000_0001;
        private const byte ConfigPowerDownCurrentOutChannel0Mask = 0b1111_1110;
        private const byte ConfigPowerUpCurrentOutChannel1Mask = 0b0000_1000;
        private const byte ConfigPowerDownCurrentOutChannel1Mask = 0b1111_0111;
        private const byte ConfigPowerUpCurrentOutChannel2Mask = 0b0100_0000;
        private const byte ConfigPowerDownCurrentOutChannel2Mask = 0b1011_1111;
        private const ushort ConfigPowerUpCurrentOutChannel3Mask = 0b0000_0010_0000_0000;
        private const ushort ConfigPowerDownCurrentOutChannel3Mask = 0b1111_1101_0000_0000;

        private const byte ConfigPowerUpVoltageOutChannel0Mask = 0b0000_0110;
        private const byte ConfigPowerDownVoltageOutChannel0Mask = 0b1111_1001;
        private const byte ConfigPowerUpVoltageOutChannel1Mask = 0b0011_0000;
        private const byte ConfigPowerDownVoltageOutChannel1Mask = 0b1100_1111;
        private const ushort ConfigPowerUpVoltageOutChannel2Mask = 0b0000_0001_1000_0000;
        private const ushort ConfigPowerDownVoltageOutChannel2Mask = 0b1111_1110_0111_1111;
        private const ushort ConfigPowerUpVoltageOutChannel3Mask = 0b0000_1100_0000_0000;
        private const ushort ConfigPowerDownVoltageOutChannel3Mask = 0b1111_0011_0000_0000;

        private I2cDevice _i2cDevice;
        private byte _deviceModel;

        /// <summary>
        /// Dac63004 Default I2C Address.
        /// </summary>
        /// <remarks>
        /// Assuming A0 pin connected to GND.
        /// For other address configurations see datasheet 7.5.2.2.1 Address Byte.
        /// <para>
        /// - A0 connected to GND: 0b01001000
        /// </para>
        /// <para>
        /// - A0 connected to VDD: 0b01001001
        /// </para>
        /// <para>
        /// A0 connected to SDA: 0b01001010
        /// </para>
        /// <para>
        /// A0 connected to SCL: 0b01001011
        /// </para>
        /// </remarks>
        public const byte DefaultI2cAddress = 0b01001000;

        /// <summary>
        /// Gets or sets a value indicating whether the internal reference is enable.
        /// </summary>
        /// <value><see langword="true"/> if internal reference is enabled, otherwise <see langword="false"/>.</value>
        [Property]
        public bool InternalRefEnabled
        {
            get => GetInternalRefEnabled();
            set => SetInternalRefEnabled(value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dac63004" /> class.
        /// </summary>
        /// <param name="i2cDevice"><see cref="I2cDevice"/> to communicate with DAC63004 device.</param>
        /// <exception cref="InvalidOperationException">When failing to read part information.</exception>
        /// <exception cref="NotSupportedException">If the part information returned is invalid, thus the connected part is not one of the supported DAC63004 devices.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="i2cDevice"/> is null.</exception>
        public Dac63004(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException();

            // read part information
            CheckDeviceId();
        }

        /// <summary>
        /// Configures the DAC channel funcional mode.
        /// </summary>
        /// <param name="channel">The <see cref="Channel"/> to configure.</param>
        /// <param name="mode">The funcional mode to put the channel in.</param>
        /// <exception cref="NotSupportedException">If the <paramref name="mode"/> is not supported.</exception>"
        public void ConfigureChannelMode(
            Channel channel,
            Mode mode)
        {
            var currentConfig = ReadFromRegister(Register.Reg1F_CommonConfig, 2);
            ushort newConfig;

            switch (channel)
            {
                case Channel.Channel0:
                    newConfig = mode == Mode.CurrentOutput ? (ushort)(ConfigPowerUpCurrentOutChannel0Mask | ConfigPowerDownVoltageOutChannel0Mask) : (ushort)(ConfigPowerUpVoltageOutChannel0Mask | ConfigPowerDownCurrentOutChannel0Mask);
                    break;

                case Channel.Channel1:
                    newConfig = mode == Mode.CurrentOutput ? (ushort)(ConfigPowerUpCurrentOutChannel1Mask | ConfigPowerDownVoltageOutChannel1Mask) : (ushort)(ConfigPowerUpVoltageOutChannel1Mask | ConfigPowerDownCurrentOutChannel1Mask);
                    break;

                case Channel.Channel2:
                    newConfig = mode == Mode.CurrentOutput ? (ushort)(ConfigPowerUpCurrentOutChannel2Mask | ConfigPowerDownVoltageOutChannel2Mask) : (ushort)(ConfigPowerUpVoltageOutChannel2Mask | ConfigPowerDownCurrentOutChannel2Mask);
                    break;

                case Channel.Channel3:
                    newConfig = mode == Mode.CurrentOutput ? (ushort)(ConfigPowerUpCurrentOutChannel3Mask | ConfigPowerDownVoltageOutChannel3Mask) : (ushort)(ConfigPowerUpVoltageOutChannel3Mask | ConfigPowerDownCurrentOutChannel3Mask);
                    break;

                default:
                    throw new NotSupportedException();
            }

            // MSB 1st
            currentConfig[0] &= (byte)~(newConfig >> 8);
            currentConfig[1] &= (byte)~newConfig;

            WriteToRegister(Register.Reg1F_CommonConfig, currentConfig);
        }

        /// <summary>
        /// Configures the Vout gain for DAC channel.
        /// </summary>
        /// <param name="channel">The <see cref="Channel"/> to configure.</param>
        /// <param name="gain">The <see cref="VoutGain"/> to configure the <paramref name="channel"/> to.</param>
        public void ConfigureChannelVoutGain(
            Channel channel,
            VoutGain gain)
        {
            Register registerAddress = DacVoutConfigFromChannel(channel);
            var currentConfig = ReadFromRegister(registerAddress, 2);

            // MSB 1st
            currentConfig[0] &= (byte)gain;

            WriteToRegister(registerAddress, currentConfig);
        }

        /// <summary>
        /// Sets output data value for DAC channel.
        /// </summary>
        /// <param name="channel">The <see cref="Channel"/> to set data to.</param>
        /// <param name="value">The value to configure the data register for the <paramref name="channel"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="value"/> is not in the range of 0 to 4095 (12 bits).</exception>
        public void SetChannelDataValue(
            Channel channel,
            int value)
        {
            // check allowed range (12 bits)
            if (value < 0 || value > 4095)
            {
                throw new ArgumentOutOfRangeException();
            }

            byte[] buffer = new byte[2];

            Register registerAddress = Register.Reg19_DAC0_Data + (byte)channel;

            // Data are in straight-binary format. MSB left-aligned.
            var data = value << 4;

            // split and fill buffer (MSB 1st)
            buffer[0] = (byte)(data >> 8);
            buffer[1] = (byte)data;

            WriteToRegister(registerAddress, buffer);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }

        /// <summary>
        /// Read device information to perform a sanity check if expected device is connected.
        /// </summary>
        private void CheckDeviceId()
        {
            byte[] buffer = ReadFromRegister(Register.Reg22_GeneralStatus, 2);

            // device model is stored in bits 7:2 of the LSB byte
            _deviceModel = (byte)(buffer[1] & DevicePartNumberMask);

            // sanity check for valid device ID
            if (_deviceModel != DAC63004DeviceId)
            {
                throw new NotSupportedException();
            }
        }

        #region Common configuration register

        // REG1F COMMON-CONFIG Register 
        // |   15   |  14  |    13    |    12   |  11 - 10   |   9   |   8 - 7    |   6   |   5 - 4    |   3   |    2 - 1   |   0   |
        // |  WIN-  | DEV- | EE-READ- | EN-INT- | VOUT-PDN-3 | IOUT- | VOUT-PDN-2 | IOUT- | VOUT-PDN-1 | IOUT- | VOUT-PDN-0 | IOUT- |
        // | LATCH- | LOCK |   ADDR   |   REF   |            | PDN-3 |            | PDN-2 |            | PDN-1 |            | PDN-0 |
        // |  EN    |      |          |         |            |       |            |       |            |       |            |       |
        ////

        private void SetInternalRefEnabled(bool value)
        {
            byte[] buffer = ReadFromRegister(Register.Reg1F_CommonConfig, 2);

            // setting EN-INT-REF bit
            if (value)
            {
                buffer[0] |= InternalRefEnableMask;
            }
            else
            {
                buffer[0] &= InternalRefDisableMask;
            }

            WriteToRegister(Register.Reg1F_CommonConfig, buffer);
        }

        private bool GetInternalRefEnabled()
        {
            byte[] buffer = ReadFromRegister(Register.Reg1F_CommonConfig, 2);

            // reading EN-INT-REF bit
            return (buffer[0] & InternalRefEnableMask) != 0;
        }

        #endregion

        #region Helper Methods to Read and Write to Registers

        internal void WriteToRegister(Register register, byte[] contents)
        {
            byte[] writeBuffer = new byte[contents.Length + 1];
            writeBuffer[0] = (byte)register;

            contents.CopyTo(writeBuffer, 1);

            I2cTransferResult result = _i2cDevice.Write(writeBuffer);

            if (result.Status != I2cTransferStatus.FullTransfer)
            {
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

        #endregion

        private Register DacVoutConfigFromChannel(Channel channel)
        {
            switch (channel)
            {
                case Channel.Channel0:
                    return Register.Reg03_DAC0_VoutCompareConfig;

                case Channel.Channel1:
                    return Register.Reg09_DAC1_VoutCompareConfig;

                case Channel.Channel2:
                    return Register.Reg0F_DAC2_VoutCompareConfig;

                case Channel.Channel3:
                    return Register.Reg15_DAC3_VoutCompareConfig;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
