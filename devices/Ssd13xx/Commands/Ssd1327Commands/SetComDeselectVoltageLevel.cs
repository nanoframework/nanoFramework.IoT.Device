// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Ssd13xx.Commands.Ssd1327Commands
{
    /// <summary>
    /// Represents SetComDeselectVoltageLevel command.
    /// </summary>
    public class SetComDeselectVoltageLevel : ISsd1327Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetComDeselectVoltageLevel" /> class.
        /// This command sets the high voltage level of common pins, Vcomh.
        /// </summary>
        /// <param name="level">COM deselect voltage level.</param>
        public SetComDeselectVoltageLevel(byte level = 0x05)
        {
            if (level > 0x07)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            Level = level;
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0xBE;

        /// <summary>
        /// Gets COM deselect voltage level.
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