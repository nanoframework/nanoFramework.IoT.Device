// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Ssd13xx.Commands.Ssd1327Commands
{
    /// <summary>
    /// Represents SetPreChargeVoltage command.
    /// </summary>
    public class SetPreChargeVoltage : ISsd1327Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetPreChargeVoltage" /> class.
        /// This command sets the first pre-charge voltage (phase 2) level of segment pins.
        /// </summary>
        /// <param name="level">
        /// Pre-charge voltage level.
        /// Parameter values between 0b_0000 and 0b_0111 leads to voltage values between 0.2 x Vcc and 0.613 x Vcc Volts.
        /// Parameter value 0b_1XXX leads to voltage value equals to Vcomh.
        /// </param>
        public SetPreChargeVoltage(byte level = 0x05)
        {
            if (level > 0x08)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            Level = level;
        }

        /// <summary>
        /// Gets the value that represents the command.
        /// </summary>
        public byte Id => 0xBC;

        /// <summary>
        /// Gets Pre-charge voltage level.
        /// </summary>
        public byte Level { get; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id, Level };
        }
    }
}
