// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Si7021
{
    /// <summary>
    /// Si7021 Commands.
    /// </summary>
    internal enum Command : byte
    {
        SI_TEMP = 0xF3,
        SI_HUMI = 0xF5,
        SI_RESET = 0xFE,
        SI_REVISION_MSB = 0x84,
        SI_REVISION_LSB = 0xB8,
        SI_USER_REG1_WRITE = 0xE6,
        SI_USER_REG1_READ = 0xE7,

        /// <summary>
        /// Read Electronic ID 1st Byte. 1 of 2 command sequence.
        /// </summary>
        SI_READ_Electronic_ID_1_1 = 0xFA,

        /// <summary>
        /// Read Electronic ID 1st Byte. 2 of 2 command sequence.
        /// </summary>
        SI_READ_Electronic_ID_1_2 = 0x0F,

        /// <summary>
        /// Read Electronic ID 2nd Byte. 1 of 2 command sequence.
        /// </summary>
        SI_READ_Electronic_ID_2_1 = 0xFA,

        /// <summary>
        /// Read Electronic ID 2nd Byte. 2 of 2 command sequence.
        /// </summary>
        SI_READ_Electronic_ID_2_2 = 0x0F,
    }
}
