// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers.Binary;

namespace Iot.Device.Ld2410
{
    /// <summary>
    /// Defines the set of Baud Rates that the device's serial port supports.
    /// </summary>
    /// <remarks>
    /// The serial communication of LD2410 uses little-endian format.
    /// All the values in this enum are in big-endian to match the manufacturer docs.
    /// Use <see cref="BinaryPrimitives.WriteUInt16LittleEndian(System.Span<byte>, ushort)"/> before writing to the serial port
    /// to ensure that the byte representation will be converted to little-endian if the code is running on a big-endian CPU.
    /// </remarks>
    public enum BaudRate : ushort
    {
        /// <summary>
        /// 9600 baud.
        /// </summary>
        BaudRate9600 = 0x0001,

        /// <summary>
        /// 19200 baud.
        /// </summary>
        BaudRate19200 = 0x0002,

        /// <summary>
        /// 38400 baud.
        /// </summary>
        BaudRate38400 = 0x0003,

        /// <summary>
        /// 57600 baud.
        /// </summary>
        BaudRate57600 = 0x0004,

        /// <summary>
        /// 115200 baud.
        /// </summary>
        BaudRate115200 = 0x0005,

        /// <summary>
        /// 230400 baud.
        /// </summary>
        BaudRate230400 = 0x0006,

        /// <summary>
        /// 256000 baud.
        /// </summary>
        BaudRate256000 = 0x0007,

        /// <summary>
        /// 460800 baud.
        /// </summary>
        BaudRate460800 = 0x0008
    }
}
