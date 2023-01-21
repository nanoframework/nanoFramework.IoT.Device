// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.Common
{
    /// <summary>
    /// Helper class for common register manipulation tasks.
    /// </summary>
    internal static class RegisterHelper
    {
        /// <summary>
        /// Helper function to read from a device register.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to use for communication.</param>
        /// <param name="register">The address of the register.</param>
        /// <returns>The contents of the register.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <c>null</c>.</exception>
        public static byte ReadRegister(I2cDevice i2cDevice, byte register)
        {
            if (i2cDevice == null)
            {
                throw new ArgumentNullException();
            }

            i2cDevice.WriteByte(register);
            return i2cDevice.ReadByte();
        }

        /// <summary>
        /// Helper function to write to a device register.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to use for communication.</param>
        /// <param name="register">The address of the register.</param>
        /// <param name="value">The value to write to the register.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <c>null</c>.</exception>
        public static void WriteRegister(I2cDevice i2cDevice, byte register, byte value)
        {
            if (i2cDevice == null)
            {
                throw new ArgumentNullException();
            }

            SpanByte writeBuffer = new byte[] { register, value };

            i2cDevice.Write(writeBuffer);
        }

        /// <summary>
        /// Helper function to set bit in a device register.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to use for communication.</param>
        /// <param name="register">The address of the register.</param>
        /// <param name="bitMask">The mask that specifies the bits to be set.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <c>null</c>.</exception>
        public static void SetRegisterBit(I2cDevice i2cDevice, byte register, byte bitMask)
        {
            if (i2cDevice == null)
            {
                throw new ArgumentNullException();
            }

            byte registerContents = ReadRegister(i2cDevice, register);

            // Set bits in the register.
            registerContents |= bitMask;

            WriteRegister(i2cDevice, register, registerContents);
        }

        /// <summary>
        /// Helper function to set only the bits in a device register that are specified by a mask.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to use for communication.</param>
        /// <param name="register">The address of the register.</param>
        /// <param name="bitMask">The mask that specifies the bits to be set.</param>
        /// <param name="value">The value to set the bits to.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <c>null</c>.</exception>
        public static void MaskedSetRegisterBits(I2cDevice i2cDevice, byte register, byte bitMask, byte value)
        {
            if (i2cDevice == null)
            {
                throw new ArgumentNullException();
            }

            byte registerContents = ReadRegister(i2cDevice, register);

            // Update only the masked bits in the register.
            registerContents = (byte)((registerContents & ~bitMask) | (value & bitMask));

            WriteRegister(i2cDevice, register, registerContents);
        }

        /// <summary>
        /// Helper function to clear bit in a device register.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to use for communication.</param>
        /// <param name="register">The address of the register.</param>
        /// <param name="bitMask">The mask that specifies the bits to be cleared.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <c>null</c>.</exception>
        public static void ClearRegisterBit(I2cDevice i2cDevice, byte register, byte bitMask)
        {
            if (i2cDevice == null)
            {
                throw new ArgumentNullException();
            }

            byte registerContents = ReadRegister(i2cDevice, register);

            // Clear bits in the register.
            registerContents &= (byte)~bitMask;

            WriteRegister(i2cDevice, register, registerContents);
        }

        /// <summary>
        /// Helper function to check if bit is set in a device register.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to use for communication.</param>
        /// <param name="register">The address of the register.</param>
        /// <param name="bitMask">The mask that specifies the bits to be checked.</param>
        /// <returns>Returns <c>true</c> if the bit is set, <c>false</c> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <c>null</c>.</exception>
        public static bool RegisterBitIsSet(I2cDevice i2cDevice, byte register, byte bitMask)
        {
            if (i2cDevice == null)
            {
                throw new ArgumentNullException();
            }

            byte registerContents = ReadRegister(i2cDevice, register);

            // Check if bits are set in the register.
            return (registerContents & bitMask) != 0;
        }
    }
}
