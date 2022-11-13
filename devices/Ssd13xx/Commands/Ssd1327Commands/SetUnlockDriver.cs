// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ssd13xx.Commands.Ssd1327Commands
{
    /// <summary>
    /// Represents SetUnlockDriver command.
    /// </summary>
    public class SetUnlockDriver : ISsd1327Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetUnlockDriver" /> class.
        /// This command sets the display to be normal.
        /// </summary>
        /// <param name="unlock">Represents if driver have to be unlocked.</param>
        public SetUnlockDriver(bool unlock)
        {
            SetUnlock = (byte)(unlock ? 0b0001_0010 : 0b0001_0110);
        }

        /// <summary>
        /// Gets the value that represents the command.
        /// </summary>
        public byte Id => 0xFD;

        /// <summary>
        /// Gets the value that represents if driver should be unlocked.
        /// </summary>
        private byte SetUnlock { get; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id, SetUnlock };
        }
    }
}
