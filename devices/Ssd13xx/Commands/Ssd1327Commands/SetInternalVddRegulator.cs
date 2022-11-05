// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Ssd13xx.Commands.Ssd1327Commands
{
    /// <summary>
    /// Represents SetInternalVddRegulator command.
    /// </summary>
    public class SetInternalVddRegulator : ISsd1327Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetInternalVddRegulator" /> class.
        /// This command is used to enable internal Vdd regulator.
        /// </summary>
        /// <param name="enable">Represents if internal Vdd have to be enabled.</param>
        public SetInternalVddRegulator(bool enable)
        {
            UseInternalVdd = (byte)(enable ? 0b0000_0001 : 0b0000_0000);
        }

        /// <summary>
        /// Gets the value that represents the command.
        /// </summary>
        public byte Id => 0xAB;

        /// <summary>
        /// Gets the value that represent if internal or external Vdd should be used.
        /// </summary>
        public byte UseInternalVdd { get; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id, UseInternalVdd };
        }
    }
}