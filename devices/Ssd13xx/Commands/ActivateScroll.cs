// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ssd13xx.Commands
{
    /// <summary>
    /// Represents ActivateScroll command.
    /// </summary>
    public class ActivateScroll : ISharedCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivateScroll" /> class.
        /// This command starts the motion of scrolling and should only be issued
        /// after the scroll setup parameters have been defined by the scrolling
        /// setup commands :26h/27h/29h/2Ah. The setting in the last scrolling setup
        /// command overwrites the setting in the previous scrolling setup commands.
        /// </summary>
        public ActivateScroll()
        {
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0x2F;

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
