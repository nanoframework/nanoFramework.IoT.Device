// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ssd13xx.Commands.Ssd1306Commands
{
    /// <summary>
    /// Represents NoOperation command.
    /// </summary>
    public class NoOperation : ISsd1306Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoOperation" /> class.
        /// This command is a no operation command.
        /// </summary>
        public NoOperation()
        {
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0xE3;

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id };
        }
    }
}