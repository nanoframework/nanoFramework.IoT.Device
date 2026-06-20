// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.Aw9523x
{
    /// <summary>
    /// AW9523X - 16-channel I2C GPIO and LED controller.
    /// </summary>
    public class Aw9523x : IDisposable
    {
        /// <summary>
        /// AW9523X default I2C address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x58;

        /// <summary>
        /// AW9523X expected chip ID value.
        /// </summary>
        public const byte ExpectedChipId = 0x23;

        /// <summary>
        /// Represents port 0 in AW9523X port-based APIs.
        /// </summary>
        public const Port Port0 = Port.Port0;

        /// <summary>
        /// Represents port 1 in AW9523X port-based APIs.
        /// </summary>
        public const Port Port1 = Port.Port1;

        private I2cDevice _i2c;

        /// <summary>
        /// Initializes a new instance of the <see cref="Aw9523x"/> class.
        /// </summary>
        /// <param name="i2c">The I2C device.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2c"/> is null.</exception>
        public Aw9523x(I2cDevice i2c)
        {
            _i2c = i2c ?? throw new ArgumentNullException(nameof(i2c));
        }

        /// <summary>
        /// Gets the AW9523X chip ID (register 0x10).
        /// </summary>
        public byte ChipId => ReadRegister(Register.ChipId);

        /// <summary>
        /// Gets a value indicating whether the detected chip ID matches AW9523X.
        /// </summary>
        public bool IsAw9523x => ChipId == ExpectedChipId;

        /// <summary>
        /// Reads one of the two input-port registers.
        /// </summary>
        /// <param name="port">Port selector.</param>
        /// <returns>Port input value.</returns>
        public byte ReadInputPort(Port port)
        {
            ValidatePort(port);
            return ReadRegister(GetInputRegister(port));
        }

        /// <summary>
        /// Reads one of the two output-port registers.
        /// </summary>
        /// <param name="port">Port selector.</param>
        /// <returns>Port output value.</returns>
        public byte ReadOutputPort(Port port)
        {
            ValidatePort(port);
            return ReadRegister(GetOutputRegister(port));
        }

        /// <summary>
        /// Writes one of the two output-port registers.
        /// </summary>
        /// <param name="port">Port selector.</param>
        /// <param name="value">Output value to write.</param>
        public void WriteOutputPort(Port port, byte value)
        {
            ValidatePort(port);
            WriteRegister(GetOutputRegister(port), value);
        }

        /// <summary>
        /// Sets selected bits in one of the output-port registers.
        /// </summary>
        /// <param name="port">Port selector.</param>
        /// <param name="mask">Bit mask to set.</param>
        public void SetOutputBits(Port port, OutputMask mask)
        {
            ValidatePort(port);
            ValidateMask(mask);
            Register register = GetOutputRegister(port);
            byte value = ReadRegister(register);
            WriteRegister(register, (byte)(value | (byte)mask));
        }

        /// <summary>
        /// Clears selected bits in one of the output-port registers.
        /// </summary>
        /// <param name="port">Port selector.</param>
        /// <param name="mask">Bit mask to clear.</param>
        public void ClearOutputBits(Port port, OutputMask mask)
        {
            ValidatePort(port);
            ValidateMask(mask);
            Register register = GetOutputRegister(port);
            byte value = ReadRegister(register);
            WriteRegister(register, (byte)(value & ~(byte)mask));
        }

        /// <summary>
        /// Reads one of the two direction registers.
        /// A set bit configures input mode; a cleared bit configures output mode.
        /// </summary>
        /// <param name="port">Port selector.</param>
        /// <returns>Direction register value.</returns>
        public byte ReadDirectionPort(Port port)
        {
            ValidatePort(port);
            return ReadRegister(GetDirectionRegister(port));
        }

        /// <summary>
        /// Writes one of the two direction registers.
        /// A set bit configures input mode; a cleared bit configures output mode.
        /// </summary>
        /// <param name="port">Port selector.</param>
        /// <param name="value">Direction register value.</param>
        public void WriteDirectionPort(Port port, byte value)
        {
            ValidatePort(port);
            WriteRegister(GetDirectionRegister(port), value);
        }

        /// <summary>
        /// Configures port 0 as open-drain or push-pull.
        /// </summary>
        /// <param name="openDrain">True for open-drain, false for push-pull.</param>
        public void SetPort0OpenDrain(bool openDrain)
        {
            const byte Port0OpenDrainMask = 0x10;

            byte control = ReadRegister(Register.GlobalControl);
            control = openDrain
                ? (byte)(control | Port0OpenDrainMask)
                : (byte)(control & ~Port0OpenDrainMask);

            WriteRegister(Register.GlobalControl, control);
        }

        /// <summary>
        /// Updates selected bits in an output-port register.
        /// </summary>
        /// <param name="port">Port selector.</param>
        /// <param name="mask">Bit mask to update.</param>
        /// <param name="enable">True to set bits, false to clear bits.</param>
        public void UpdateOutputBits(Port port, OutputMask mask, bool enable)
        {
            ValidatePort(port);
            ValidateMask(mask);
            if (enable)
            {
                SetOutputBits(port, mask);
            }
            else
            {
                ClearOutputBits(port, mask);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2c?.Dispose();
            _i2c = null;
        }

        private static Register GetInputRegister(Port port)
        {
            return port == Port0 ? Register.InputPort0 : Register.InputPort1;
        }

        private static Register GetOutputRegister(Port port)
        {
            return port == Port0 ? Register.OutputPort0 : Register.OutputPort1;
        }

        private static Register GetDirectionRegister(Port port)
        {
            return port == Port0 ? Register.DirectionPort0 : Register.DirectionPort1;
        }

        private static void ValidatePort(Port port)
        {
            if (port != Port0 && port != Port1)
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }
        }

        private static void ValidateMask(OutputMask mask)
        {
            if (mask == OutputMask.None)
            {
                throw new ArgumentOutOfRangeException(nameof(mask));
            }
        }

        private byte ReadRegister(Register register)
        {
            byte[] writeBuffer = new byte[] { (byte)register };
            byte[] readBuffer = new byte[1];
            _i2c.WriteRead(writeBuffer, readBuffer);
            return readBuffer[0];
        }

        private void WriteRegister(Register register, byte value)
        {
            byte[] writeBuffer = new byte[] { (byte)register, value };
            _i2c.Write(writeBuffer);
        }
    }
}
