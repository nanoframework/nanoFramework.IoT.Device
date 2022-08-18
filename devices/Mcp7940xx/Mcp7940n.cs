// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Common;

namespace Iot.Device.Mcp7940xx
{
    /// <summary>
    /// Battery-Backed I2C Real-Time Clock/Calendar with SRAM.
    /// </summary>
    public partial class Mcp7940n : Mcp7940m
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mcp7940n" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to use for communication.</param>
        /// <param name="clockSource">The clocks oscillator configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <c>null</c>.</exception>
        public Mcp7940n(I2cDevice i2cDevice, ClockSource clockSource)
            : base(i2cDevice, clockSource)
        {
        }

        #region Battery Backup

        /// <summary>
        /// Enables external battery backup.
        /// </summary>
        public void EnableExternalBatteryBackup()
        {
            RegisterHelper.SetRegisterBit(_I2cDevice, (byte)Register.TimekeepingWeekday, (byte)TimekeepingWeekdayRegister.ExternalBatteryBackupEnabled);
        }

        /// <summary>
        /// Disables external battery backup.
        /// </summary>
        public void DisableExternalBatteryBackup()
        {
            RegisterHelper.ClearRegisterBit(_I2cDevice, (byte)Register.TimekeepingWeekday, (byte)TimekeepingWeekdayRegister.ExternalBatteryBackupEnabled);
        }

        /// <summary>
        /// Checks if external battery backup enabled.
        /// </summary>
        /// <returns>Returns <c>true</c> if external battery backup is enabled, <c>false</c> if not.</returns>
        public bool IsEnabledExternalBatteryBackup()
        {
            return RegisterHelper.RegisterBitIsSet(_I2cDevice, (byte)Register.TimekeepingWeekday, (byte)TimekeepingWeekdayRegister.ExternalBatteryBackupEnabled);
        }

        /// <summary>
        /// Checks if the device has experienced a power failure.
        /// </summary>
        /// <returns>Returns <c>true</c> if a power failure has occurred, <c>false</c> if not.</returns>
        public bool HasPowerFailureAlert()
        {
            return RegisterHelper.RegisterBitIsSet(_I2cDevice, (byte)Register.TimekeepingWeekday, (byte)TimekeepingWeekdayRegister.PowerFailureStatus);
        }

        /// <summary>
        /// Clears the power failure alert flag.
        /// </summary>
        public void ClearPowerFailureAlert()
        {
            RegisterHelper.ClearRegisterBit(_I2cDevice, (byte)Register.TimekeepingWeekday, (byte)TimekeepingWeekdayRegister.PowerFailureStatus);
        }

        /// <summary>
        /// Gets a <see cref = "PowerEvent" /> object that is set to the date and time of the last power down event.
        /// </summary>
        /// <returns>An object whose value is the date and time of the last power down event.</returns>
        public PowerEvent GetLastPowerDown()
        {
            return new PowerEvent(_I2cDevice, Register.PowerDownMinute);
        }

        /// <summary>
        /// Gets a <see cref = "PowerEvent" /> object that is set to the date and time of the last power up event.
        /// </summary>
        /// <returns>An object whose value is the date and time of the last power up event.</returns>
        public PowerEvent GetLastPowerUp()
        {
            return new PowerEvent(_I2cDevice, Register.PowerUpMinute);
        }

        #endregion
    }
}
