// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Ssd13xx.Commands.Ssd1327Commands
{
    /// <summary>
    /// Represents SetSecondPreChargeVsl command
    /// </summary>
    public class SetSecondPreChargeVsl : ISsd1327Command
    {
        /// <summary>
        /// This command sets the first pre-charge voltage (phase 2) level of segment pins.
        /// </summary>
        /// <param name="secondPrecharge">Enable/disable second precharge.</param>
        /// <param name="externalVsl"> Switch between internal and external VSL.</param>
        public SetSecondPreChargeVsl(bool secondPrecharge = false, bool externalVsl = false)
        {
            Config = (byte)(secondPrecharge ? 0b_0110_0010 : 0b_0110_0000);
            Config |= (byte)(externalVsl ? 0b_0110_0001 : 0b_0110_0000);
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0xD5;

        /// <summary>
        /// The value that represents configuration
        /// </summary>
        public byte Config { get; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id, Config };
        }
    }
}
