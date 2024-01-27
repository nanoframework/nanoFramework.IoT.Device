// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ssd13xx.Commands
{
    /// <summary>
    /// Represents SetDisplayOff command.
    /// </summary>
    public class SetDisplayOff : ISharedCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetDisplayOff" /> class.
        /// This command turns the OLED panel display off.
        /// </summary>
        public SetDisplayOff()
        {
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0xAE;

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