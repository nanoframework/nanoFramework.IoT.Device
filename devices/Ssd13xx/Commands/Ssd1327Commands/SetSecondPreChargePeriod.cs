// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Ssd13xx.Commands.Ssd1327Commands
{
    /// <summary>
    /// Represents SetSecondPreChargePeriod command.
    /// </summary>
    public class SetSecondPreChargePeriod : ISsd1327Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetSecondPreChargePeriod" /> class.
        /// This command is used to set the phase 3 second pre-charge period.
        /// </summary>
        /// <param name="period">Second pre-charge period.</param>
        public SetSecondPreChargePeriod(byte period = 0x04)
        {
            if (period > 0x0F)
            {
                throw new ArgumentOutOfRangeException(nameof(period));
            }

            Period = period;
        }

        /// <summary>
        /// Gets the value that represents the command.
        /// </summary>
        public byte Id => 0xB6;

        /// <summary>
        /// Gets second Pre-charge period.
        /// </summary>
        public byte Period { get; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id, Period };
        }
    }
}