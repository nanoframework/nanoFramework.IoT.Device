// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using UnitsNet;

namespace Iot.Device.Max1704x
{
    /// <summary>
    /// Represents the MAX17048 class, which inherits from MAX17044.
    /// </summary>
    public class Max17048 : Max17044
    {
        /// <summary>
        /// <inheritdoc />
        /// </summary>
        protected override float FullScale => 5.12f;

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        protected override float Divider => 65536.0f;

        /// <summary>
        /// Gets or sets the reset voltage of a device.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the provided value is outside the valid range of 0 to 5.08.</exception>
        public ElectricPotential ResetVoltage
        {
            get
            {
                var value = Read16(Registers.Max17048VresetId) >> 9;
                return ElectricPotential.FromVolts(value * 0.04f);
            }

            set
            {
                if (value.Volts < 0 || value.Volts > 5.08)
                {
                    throw new ArgumentOutOfRangeException();
                }

                var thresh = (byte)(value.Volts / 0.04);
                var vreset = Read16(Registers.Max17048VresetId);
                vreset &= 0x01FF;
                vreset |= (ushort)(thresh << 9);
                Write16(vreset, Registers.Max17048VresetId);
            }
        }

        /// <summary>
        /// Gets the status of the property.
        /// </summary>
        /// <returns>The status as a byte value.</returns>
        private byte Status
        {
            get
            {
                var statusReg = Read16(Registers.Max17048Status) >> 8;
                return (byte)(statusReg & 0x7F);
            }
        }

        /// <summary>
        /// Gets the change rate of the property.
        /// </summary>
        /// <remarks>
        /// Approximate charge or discharge rate of the battery. 
        /// </remarks>
        public Ratio ChangeRate
        {
            get
            {
                var changeRate = Read16(Registers.Max17048Crate);
                return Ratio.FromPercent(changeRate * 0.208f);
            }
        }

        /// <summary>
        /// Gets the ID of the property.
        /// </summary>
        /// <remarks>
        /// This property returns the ID of the specified property. It retrieves the value from the
        /// MAX17048_VRESET_ID register and returns the lower 8 bits as a byte.
        /// </remarks>
        /// <returns>The ID of the property as a byte.</returns>
        public byte Id
        {
            get
            {
                var vresetID = Read16(Registers.Max17048VresetId);
                return (byte)(vresetID & 0xFF);
            }
        }

        /// <summary>
        /// Gets or sets maximum voltage threshold in volts for the overvoltage (VALRT) interrupt.
        /// </summary>
        /// <remarks>
        /// This property gets or sets the maximum voltage threshold for the VALRT interrupt.
        /// The value is in volts, ranging from 0 to 5.1 volts.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the assigned value is less than 0 or greater than 5.1.</exception>
        public ElectricPotential OvervoltageAlertThreshold
        {
            get
            {
                var valrt = Read16(Registers.Max17048Cvalrt);
                valrt &= 0x00FF; // Mask off max bits
                return ElectricPotential.FromVolts(valrt * 0.02f);
            }

            set
            {
                if (value.Volts < 0 || value.Volts > 5.1)
                {
                    throw new ArgumentOutOfRangeException();
                }

                var thresh = value.Volts / 0.02f;
                var valrt = Read16(Registers.Max17048Cvalrt);
                valrt &= 0xFF00;
                valrt |= (ushort)thresh;
                Write16(valrt, Registers.Max17048Cvalrt);
            }
        }
        
        /// <summary>
        /// Gets or sets maximum voltage threshold in volts for the undervoltage (VALRT) interrupt.
        /// </summary>
        /// <remarks>
        /// This property gets or sets the maximum voltage threshold for the VALRT interrupt.
        /// The value is in volts, ranging from 0 to 5.1 volts.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the assigned value is less than 0 or greater than 5.1.</exception>
        public ElectricPotential UnervoltageAlertThreshold
        {
            get
            {
                var valrt = Read16(Registers.Max17048Cvalrt);
                valrt >>= 8; // Shift min into LSB
                return ElectricPotential.FromVolts(valrt * 0.02f);
            }

            set
            {
                if (value.Volts < 0 || value.Volts > 5.1)
                {
                    throw new ArgumentOutOfRangeException();
                }

                var thresh = value.Volts / 0.02f;
                var valrt = Read16(Registers.Max17048Cvalrt);
                valrt &= 0x00FF; // Mask off min bits
                valrt |= (ushort)((ushort)thresh << 8);
                Write16(valrt, Registers.Max17048Cvalrt);
            }
        }

        /// <summary>
        /// Gets or sets the Hibernation Active Threshold value.
        /// This property represents the battery threshold for exiting hibernation mode.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is less than 0 or greater than 0.31875.</exception>
        public ElectricPotential HibernateExitThreshold
        {
            get
            {
                var hibrt = Read16(Registers.Max17048Hibrt);
                hibrt &= 0x00FF;
                return ElectricPotential.FromVolts(hibrt * 0.00125f);
            }

            set
            {
                if (value.Volts < 0 || value.Volts > 0.31875)
                {
                    throw new ArgumentOutOfRangeException();
                }

                var thresh = value.Volts / 0.00125f;
                var hibrt = Read16(Registers.Max17048Hibrt);
                hibrt &= 0xFF00; // Mask off Act bits
                hibrt |= (ushort)thresh;
                Write16(hibrt, Registers.Max17048Hibrt);
            }
        }

        /// <summary>
        /// Gets or sets High Current Battery Threshold (HIBTHR) property.
        /// This property represents the battery threshold for entering hibernation mode.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is less than 0 or greater than 53.04.</exception>
        public ElectricPotential HibernateThreshold
        {
            get
            {
                var hibrt = Read16(Registers.Max17048Hibrt);
                hibrt >>= 8; // Shift HibThr into LSB
                return ElectricPotential.FromVolts(hibrt * 0.208f);    
            }

            set
            {
                if (value.Volts < 0 || value.Volts > 53.04)
                {
                    throw new ArgumentOutOfRangeException();
                }
                
                var thresh = value.Volts / 0.208f;
                var hibrt = Read16(Registers.Max17048Hibrt);
                hibrt &= 0x00FF;
                hibrt |= (ushort)(((ushort)thresh) << 8);
                Write16(hibrt, Registers.Max17048Hibrt);
            }
        }

        /// <summary>
        /// Initializes a new instance of the Max17048 class.
        /// </summary>
        /// <param name="i2CDevice">The I2C device used for communication with device.</param>
        public Max17048(I2cDevice i2CDevice) : base(i2CDevice)
        {
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public override void Sleep()
        {
            Write16((ushort)Registers16.Max17048ModeEnsleep, Registers.Max17043Mode);
            base.Sleep();
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public override void Wake()
        {
            base.Wake();
            Write16(0x0000, Registers.Max17043Mode);
        }

        /// <summary>
        /// Enables the alert feature of the MAX17048 battery fuel gauge.
        /// </summary>
        public void EnableAlert()
        {
            var statusReg = Read16(Registers.Max17048Status);
            statusReg |= (ushort)Registers16.Max1704XStatusEnVr;
            Write16(statusReg, Registers.Max17048Status);
        }

        /// <summary>
        /// Disables the alert functionality of the MAX17048 chip.
        /// </summary>
        public void DisableAlert()
        {
            var statusReg = Read16(Registers.Max17048Status);
            statusReg &= Negate((ushort)Registers16.Max1704XStatusEnVr);
            Write16(statusReg, Registers.Max17048Status);
        }

        /// <summary>
        /// Enables the comparator functionality of the MAX17048 chip by clearing a specific bit in the VRESET_ID register.
        /// </summary>
        public void EnableComparator()
        {
            var vresetReg = Read16(Registers.Max17048VresetId);
            vresetReg &= Negate(1 << 8);
            Write16(vresetReg, Registers.Max17048VresetId);
        }

        /// <summary>
        /// Disables the comparator of the MAX17048 module.
        /// </summary>
        public void DisableComparator()
        {
            var vresetReg = Read16(Registers.Max17048VresetId);
            vresetReg |= 1 << 8;
            Write16(vresetReg, Registers.Max17048VresetId);
        }

        /// <summary>
        /// Checks if the reset flag is set.
        /// </summary>
        /// <param name="clear">Optional. Specifies whether to clear the reset flag. Default is false.</param>
        /// <returns>True if the reset flag is set; otherwise, false.</returns>
        public bool IsReset(bool clear = false)
        {
            var status = Status;
            var flag = (status & (byte)Registers.Max1704XStatusRi) > 0;
            if (flag && clear)
            {
                ClearStatusRegBits((byte)Registers.Max1704XStatusRi << 8);
            }

            return flag;
        }

        /// <summary>
        /// Checks if the voltage is high.
        /// </summary>
        /// <param name="clear">Optional. If set to true, clears the high voltage status bit.</param>
        /// <returns>True if voltage is high; otherwise false.</returns>
        public bool IsVoltageHigh(bool clear = false)
        {
            var status = Status;
            var flag = (status & (byte)Registers.Max1704XStatusVh) > 0;
            if (flag && clear)
            {
                ClearStatusRegBits((byte)Registers.Max1704XStatusVh << 8);
            }

            return flag;
        }

        /// <summary>
        /// Checks if the voltage is low.
        /// </summary>
        /// <param name="clear">Specifies whether to clear the status register bits if the voltage is low.</param>
        /// <returns><c>true</c> if the voltage is low; otherwise, <c>false</c>.</returns>
        public bool IsVoltageLow(bool clear = false)
        {
            var status = Status;
            var flag = (status & (byte)Registers.Max1704XStatusVl) > 0;
            if (flag && clear)
            {
                ClearStatusRegBits((byte)Registers.Max1704XStatusVl << 8);
            }

            return flag;
        }

        /// <summary>
        /// Returns whether the voltage has been reset. Optionally, can also clear the reset flag. </summary>
        /// <param name="clear">Indicates whether to clear the reset flag.</param>
        /// <returns>True if the voltage has been reset; otherwise, false.</returns>
        public bool IsVoltageReset(bool clear = false)
        {
            var status = Status;
            var flag = (status & (byte)Registers.Max1704XStatusVr) > 0;
            if (flag && clear)
            {
                ClearStatusRegBits((byte)Registers.Max1704XStatusVr << 8);
            }

            return flag;
        }

        /// <summary>
        /// Determines if the battery level is low.
        /// </summary>
        /// <param name="clear">Specifies whether to clear the low battery flag if it is set.</param>
        /// <returns>True if the battery level is low; otherwise, false.</returns>
        public bool IsLow(bool clear = false)
        {
            var status = Status;
            var flag = (status & (byte)Registers.Max1704XStatusHd) > 0;
            if (flag && clear)
            {
                ClearStatusRegBits((byte)Registers.Max1704XStatusHd << 8);
            }

            return flag;
        }

        /// <summary>
        /// Checks if the status register has changed.
        /// </summary>
        /// <param name="clear">If set to true, clears the status register bits if they have changed.</param>
        /// <returns>Returns true if the status register has changed; otherwise, false.</returns>
        public bool IsChange(bool clear = false)
        {
            var status = Status;
            var flag = (status & (byte)Registers.Max1704XStatusSc) > 0;
            if (flag && clear)
            {
                ClearStatusRegBits((byte)Registers.Max1704XStatusSc << 8);
            }

            return flag;
        }

        /// <summary>
        /// Enables the hibernate mode of the MAX17048 by writing the appropriate value to the register.
        /// </summary>
        public void EnableHibernate()
        {
            if (IsHibernating())
            {
                return;
            }

            Write16((ushort)Registers16.Max17048HibrtEnhib, Registers.Max17048Hibrt);
        }

        /// <summary>
        /// Disables the Hibernate mode of the MAX17048 device.
        /// </summary>
        public void DisableHibernate()
        {
            if (!IsHibernating())
            {
                return;
            }

            Write16((byte)Registers.Max17048HibrtDishib, Registers.Max17048Hibrt);
        }

        /// <summary>
        /// Checks if the device is in hibernation mode.
        /// </summary>
        /// <returns>
        /// Returns a boolean value indicating if the device is in hibernation mode.
        /// True if the device is in hibernation mode, otherwise false.
        /// </returns>
        public bool IsHibernating()
        {
            var mode = Read16(Registers.Max17043Mode);
            return (mode & (ushort)Registers16.Max17048ModeHibstat) > 0;
        }

        /// <summary>
        /// Enables the State of Charge (SOC) alert on the MAX17043 device.
        /// </summary>
        public void EnableStateOfChangeAlert()
        {
            var configReg = GetConfigRegister();
            configReg |= (byte)Registers.Max17043ConfigAlsc;
            Write16(configReg, Registers.Max17043Config);
        }

        /// <summary>
        /// Disables the State of Charge (SOC) alert on the MAX17043 device.
        /// </summary>
        public void DisableStateOfChangeAlert()
        {
            var configReg = GetConfigRegister();
            configReg &= Negate((byte)Registers.Max17043ConfigAlsc);
            Write16(configReg, Registers.Max17043Config);
        }

        /// <summary>
        /// Clears specified bits in the status register.
        /// </summary>
        /// <param name="mask">The mask indicating which bits to clear.</param>
        private void ClearStatusRegBits(int mask)
        {
            var statusReg = Read16(Registers.Max17048Status);
            statusReg &= Negate(mask);
            Write16(statusReg, Registers.Max17048Status);
        }
    }
}