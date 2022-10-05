// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.RadioTransmitter
{
    /// <summary>
    /// FM radio transmitter module KT0803.
    /// </summary>
    public class Kt0803 : RadioTransmitterBase
    {
        private I2cDevice _i2cDevice;

        /// <summary>
        /// Kt0803 default I2C address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x3E;

        /// <summary>
        /// Kt0803 FM frequency (range from 70Mhz to 108Mhz).
        /// </summary>
        public override double Frequency { get => GetFrequency(); set => SetFrequency(value); }

        /// <summary>
        /// Gets or sets a value indicating whether Kt0803 is in standby.
        /// </summary>
        public bool Standby { get => GetStandby(); set => SetStandby(value); }

        /// <summary>
        /// Gets or sets a value indicating whether Kt0803 is muted.
        /// </summary>
        public bool Mute { get => GetMute(); set => SetMute(value); }

        /// <summary>
        /// Gets or sets Kt0803 PGA (Programmable Gain Amplifier) gain.
        /// </summary>
        public PgaGain PgaGain { get => GetPga(); set => SetPga(value); }

        /// <summary>
        /// Gets or sets Kt0803 transmission power.
        /// </summary>
        public TransmissionPower TransmissionPower
        {
            get => GetTransmissionPower();
            set => SetTransmissionPower(value);
        }

        private Region _region;

        /// <summary>
        /// Gets or sets Kt0803 region.
        /// </summary>
        public Region Region
        {
            get => _region;
            set
            {
                SetRegion(value);
                _region = value;
            }
        }

        /// <summary>
        /// Gets or sets Kt0803 bass boost.
        /// </summary>
        public BassBoost BassBoost { get => GetBassBoost(); set => SetBassBoost(value); }

        /// <summary>
        /// Initializes a new instance of the <see cref="Kt0803" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="frequency">FM frequency (range from 70MHz to 108MHz).</param>
        /// <param name="region">Region.</param>
        /// <param name="power">Transmission power.</param>
        /// <param name="pga">PGA (Programmable Gain Amplifier) gain.</param>
        public Kt0803(
            I2cDevice i2cDevice, 
            double frequency, 
            Region region, 
            TransmissionPower power = TransmissionPower.Power108dBuV, 
            PgaGain pga = PgaGain.Pga00dB)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            Frequency = frequency;
            TransmissionPower = power;
            PgaGain = pga;
            Region = region;
            Mute = false;
            Standby = false;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;

            base.Dispose(disposing);
        }

        /// <summary>
        /// Set Kt0803 FM frequency.
        /// </summary>
        /// <param name="frequency">FM frequency (range from 70MHz to 108MHz).</param>
        private void SetFrequency(double frequency)
        {
            // Details in Datasheet P7
            if (frequency < 70 || frequency > 108)
            {
                throw new ArgumentOutOfRangeException("Range from 70MHz to 108MHz.");
            }

            int freq, reg0, reg1, reg2;

            reg1 = ReadByte(Register.KT_CONFIG01);
            reg2 = ReadByte(Register.KT_CONFIG02);

            // 3 bytes
            freq = (int)(frequency * 20);
            freq &= 0b111111111111;

            if ((freq & 0b0001) > 0)
            {
                reg2 |= 0b10000000;
            }
            else
            {
                reg2 &= ~0b10000000;
            }

            reg0 = freq >> 1;
            reg1 = (reg1 & 0b11111000) | (freq >> 9);

            WriteByte(Register.KT_CONFIG02, (byte)reg2);
            WriteByte(Register.KT_CHSEL, (byte)reg0);
            WriteByte(Register.KT_CONFIG01, (byte)reg1);
        }

        /// <summary>
        /// Get Kt0803 FM frequency.
        /// </summary>
        /// <returns>FM frequency.</returns>
        private double GetFrequency()
        {
            int reg0 = ReadByte(Register.KT_CHSEL);
            int reg1 = ReadByte(Register.KT_CONFIG01);
            int reg2 = ReadByte(Register.KT_CONFIG02);

            int freq = ((reg1 & 0b0111) << 9) | (reg0 << 1) | (reg2 & 0b10000000 >> 7);

            return Math.Round(freq * 10 / 200.0);
        }

        /// <summary>
        /// Set Kt0803 PGA (Programmable Gain Amplifier) gain.
        /// </summary>
        /// <param name="pgaGain">PGA gain.</param>
        private void SetPga(PgaGain pgaGain)
        {
            // Details in Datasheet P9
            int reg1 = ReadByte(Register.KT_CONFIG01);
            int reg3 = ReadByte(Register.KT_CONFIG04);

            int pgaVal = (byte)pgaGain << 3;

            reg1 = (reg1 & 0b11000111) | pgaVal;

            switch (pgaGain)
            {
                case PgaGain.Pga00dB:
                case PgaGain.Pga04dB:
                case PgaGain.Pga08dB:
                case PgaGain.Pga12dB:
                    reg3 = (reg3 & 0b11001111) | (3 << 4);
                    break;
                case PgaGain.PgaN04dB:
                case PgaGain.PgaN08dB:
                case PgaGain.PgaN12dB:
                    reg3 = (reg3 & 0b11001111) | (0 << 4);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pgaGain));
            }

            WriteByte(Register.KT_CONFIG01, (byte)reg1);
            WriteByte(Register.KT_CONFIG04, (byte)reg3);
        }

        /// <summary>
        /// Get Kt0803 PGA (Programmable Gain Amplifier) gain.
        /// </summary>
        /// <returns>PGA gain.</returns>
        private PgaGain GetPga()
        {
            int reg1 = ReadByte(Register.KT_CONFIG01);

            return (PgaGain)((reg1 & 0b00111000) >> 3);
        }

        /// <summary>
        /// Set Kt0803 transmission power.
        /// </summary>
        /// <param name="power">Transmission power.</param>
        private void SetTransmissionPower(TransmissionPower power)
        {
            // Details in Datasheet P8 Table4
            int reg1 = ReadByte(Register.KT_CONFIG01);
            int reg2 = ReadByte(Register.KT_CONFIG02);
            int reg10 = ReadByte(Register.KT_CONFIG13);

            int powerVal = (byte)power;

            reg1 = (reg1 & 0b00111111) | (powerVal << 6);

            if ((powerVal & 0b0100) > 0)
            {
                reg10 |= 0b10000000;
            }
            else
            {
                reg10 &= ~0b10000000;
            }

            if ((powerVal & 0b1000) > 0)
            {
                reg2 |= 0b01000000;
            }
            else
            {
                reg2 &= ~0b01000000;
            }

            if (powerVal >= 8)
            {
                WriteByte(Register.KT_CONFIG0E, 0b0010);
            }
            else
            {
                WriteByte(Register.KT_CONFIG0E, 0b0000);
            }

            WriteByte(Register.KT_CONFIG01, (byte)reg1);
            WriteByte(Register.KT_CONFIG02, (byte)reg2);
            WriteByte(Register.KT_CONFIG13, (byte)reg10);
        }

        /// <summary>
        /// Get Kt0803 transmission power.
        /// </summary>
        /// <returns>Transmission power.</returns>
        private TransmissionPower GetTransmissionPower()
        {
            int reg1 = ReadByte(Register.KT_CONFIG01);
            int reg2 = ReadByte(Register.KT_CONFIG02);
            int reg10 = ReadByte(Register.KT_CONFIG13);

            return (TransmissionPower)(((reg2 & 0b01000000) >> 3) | (reg10 >> 5) | ((reg1 & 0b11000000) >> 6));
        }

        /// <summary>
        /// Set Kt0803 region.
        /// </summary>
        /// <param name="region">Region.</param>
        private void SetRegion(Region region)
        {
            // Details in Datasheet P8
            int reg2 = ReadByte(Register.KT_CONFIG02);

            switch (region)
            {
                case Region.America:
                case Region.Japan:
                    reg2 &= ~0b0001;
                    break;
                case Region.Europe:
                case Region.Australia:
                case Region.China:
                case Region.Other:
                    reg2 |= 0b0001;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(region));
            }

            WriteByte(Register.KT_CONFIG02, (byte)reg2);
        }

        /// <summary>
        /// Set Kt0803 mute.
        /// </summary>
        /// <param name="isMute">Mute when value is true.</param>
        private void SetMute(bool isMute)
        {
            // Details in Datasheet P8
            int reg2 = ReadByte(Register.KT_CONFIG02);

            if (isMute)
            {
                reg2 |= 0b1000;
            }
            else
            {
                reg2 &= ~0b1000;
            }

            WriteByte(Register.KT_CONFIG02, (byte)reg2);
        }

        /// <summary>
        /// Get Kt0803 mute.
        /// </summary>
        /// <returns>Mute when value is true.</returns>
        private bool GetMute()
        {
            int reg2 = ReadByte(Register.KT_CONFIG02);

            return (reg2 & 0b1000) >> 3 == 1 ? true : false;
        }

        /// <summary>
        /// Set Kt0803 standby.
        /// </summary>
        /// <param name="isStandby">Standby when value is true.</param>
        private void SetStandby(bool isStandby)
        {
            // Details in Datasheet P10
            int reg4 = ReadByte(Register.KT_CONFIG0B);

            if (isStandby)
            {
                reg4 |= 0b10000000;
            }
            else
            {
                reg4 &= ~0b10000000;
            }

            WriteByte(Register.KT_CONFIG0B, (byte)reg4);
        }

        /// <summary>
        /// Get Kt0803 standby.
        /// </summary>
        /// <returns>Standby when value is true.</returns>
        private bool GetStandby()
        {
            int reg4 = ReadByte(Register.KT_CONFIG0B);

            return reg4 >> 7 == 1 ? true : false;
        }

        /// <summary>
        /// Set Kt0803 bass boost.
        /// </summary>
        /// <param name="bassBoost">Boost mode.</param>
        private void SetBassBoost(BassBoost bassBoost)
        {
            // Details in Datasheet P9
            int reg3 = ReadByte(Register.KT_CONFIG04);

            reg3 &= 0b11111100;
            reg3 |= (byte)bassBoost;

            WriteByte(Register.KT_CONFIG04, (byte)reg3);
        }

        /// <summary>
        /// Get Kt0803 bass boost.
        /// </summary>
        /// <returns>Boost mode.</returns>
        private BassBoost GetBassBoost()
        {
            byte reg3 = ReadByte(Register.KT_CONFIG04);

            return (BassBoost)((reg3 << 6) >> 6);
        }

        private void WriteByte(Register register, byte value)
        {
            SpanByte writeBuffer = new byte[]
            {
                (byte)register, value
            };

            _i2cDevice.Write(writeBuffer);
        }

        private byte ReadByte(Register register)
        {
            SpanByte writeBuffer = new byte[]
            {
                (byte)register
            };
            SpanByte readBuffer = new byte[1];

            _i2cDevice.WriteRead(writeBuffer, readBuffer);

            return readBuffer[0];
        }
    }
}
