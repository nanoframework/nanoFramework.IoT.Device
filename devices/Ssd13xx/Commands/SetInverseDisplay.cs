// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ssd13xx.Commands
{
    /// <summary>
    /// Represents SetInverseDisplay command
    /// </summary>
    public class SetInverseDisplay : ISharedCommand
    {
        /// <summary>
        /// This command sets the display to be inverse.  Displays a RAM data of 0 indicates an ON pixel.
        /// </summary>
        public SetInverseDisplay()
        {
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0xA7;

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
