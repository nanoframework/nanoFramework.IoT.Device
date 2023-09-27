// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Modbus.Structures;

namespace Iot.Device.Modbus.Server
{
    /// <summary>
    /// Represents a Modbus device.
    /// </summary>
    public abstract class ModbusDevice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModbusDevice"/> class with the specified device ID.
        /// </summary>
        /// <param name="id">The device ID. Default is the minimum device ID.</param>
        public ModbusDevice(byte id = Consts.MinDeviceId)
        {
            if (id < Consts.MinDeviceId || id > Consts.MaxDeviceId)
            {
                throw new ArgumentNullException(nameof(id));
            }

            DeviceId = id;
        }

        /// <summary>
        /// Gets or sets the device ID.
        /// </summary>
        public virtual byte DeviceId { get; protected set; }

        /// <summary>
        /// Tries to read a discrete input from the Modbus device at the specified address.
        /// </summary>
        /// <param name="address">The address of the discrete input.</param>
        /// <param name="value">When this method returns, contains the value of the discrete input if the read operation succeeds, or false if the read operation fails.</param>
        /// <returns>True if the read operation succeeds; otherwise, false.</returns>
        protected abstract bool TryReadDiscreteInput(ushort address, out bool value);

        /// <summary>
        /// Tries to read an input register from the Modbus device at the specified address.
        /// </summary>
        /// <param name="address">The address of the input register.</param>
        /// <param name="value">When this method returns, contains the value of the input register if the read operation succeeds, or zero if the read operation fails.</param>
        /// <returns>True if the read operation succeeds; otherwise, false.</returns>
        protected abstract bool TryReadInputRegister(
            ushort address,
            out short value);

        /// <summary>
        /// Tries to read a coil from the Modbus device at the specified address.
        /// </summary>
        /// <param name="address">The address of the coil.</param>
        /// <param name="value">When this method returns, contains the value of the coil if the read operation succeeds, or false if the read operation fails.</param>
        /// <returns>True if the read operation succeeds; otherwise, false.</returns>
        protected abstract bool TryReadCoil(ushort address, out bool value);

        /// <summary>
        /// Tries to write a coil to the Modbus device at the specified address.
        /// </summary>
        /// <param name="address">The address of the coil.</param>
        /// <param name="value">The value to write to the coil.</param>
        /// <returns>True if the write operation succeeds; otherwise, false.</returns>
        protected abstract bool TryWriteCoil(ushort address, bool value);

        /// <summary>
        /// Tries to read a holding register from the Modbus device at the specified address.
        /// </summary>
        /// <param name="address">The address of the holding register.</param>
        /// <param name="value">When this method returns, contains the value of the holding register if the read operation succeeds, or zero if the read operation fails.</param>
        /// <returns>True if the read operation succeeds; otherwise, false.</returns>
        protected abstract bool TryReadHoldingRegister(
            ushort address,
            out short value);

        /// <summary>
        /// Tries to write a holding register to the Modbus device at the specified address.
        /// </summary>
        /// <param name="address">The address of the holding register.</param>
        /// <param name="value">The value to write to the holding register.</param>
        /// <returns>True if the write operation succeeds; otherwise, false.</returns>
        protected abstract bool TryWriteHoldingRegister(
            ushort address,
            short value);

        /// <summary>
        /// Method called before reading from the Modbus device.
        /// </summary>
        protected internal virtual void BeginRead()
        {
        }

        /// <summary>
        /// Method called after reading from the Modbus device.
        /// </summary>
        protected internal virtual void EndRead()
        {
        }

        /// <summary>
        /// Method called before writing to the Modbus device.
        /// </summary>
        protected internal virtual void BeginWrite()
        {
        }

        /// <summary>
        /// Method called after writing to the Modbus device.
        /// </summary>
        protected internal virtual void EndWrite()
        {
        }

        /// <summary>
        /// Gets a discrete input from the Modbus device at the specified address.
        /// </summary>
        /// <param name="address">The address of the discrete input.</param>
        /// <param name="discreteInput">When this method returns, contains the discrete input if the read operation succeeds, or null if the read operation fails.</param>
        /// <returns>True if the read operation succeeds; otherwise, false.</returns>
        internal bool GetDiscreteInput(ushort address, out DiscreteInput discreteInput)
        {
            discreteInput = null;

            var result = TryReadDiscreteInput(address, out bool value);
            if (result)
            {
                discreteInput = new DiscreteInput { Address = address, Value = value };
            }

            return result;
        }

        /// <summary>
        /// Gets an input register from the Modbus device at the specified address.
        /// </summary>
        /// <param name="address">The address of the input register.</param>
        /// <param name="inputRegister">When this method returns, contains the input register if the read operation succeeds, or null if the read operation fails.</param>
        /// <returns>True if the read operation succeeds; otherwise, false.</returns>
        internal bool GetInputRegister(
            ushort address,
            out InputRegister inputRegister)
        {
            inputRegister = null;

            var result = TryReadInputRegister(
                address,
                out short value);
            
            if (result)
            {
                inputRegister = new InputRegister() { Address = address, Value = value };
            }

            return result;
        }

        /// <summary>
        /// Gets a coil from the Modbus device at the specified address.
        /// </summary>
        /// <param name="address">The address of the coil.</param>
        /// <param name="coil">When this method returns, contains the coil if the read operation succeeds, or null if the read operation fails.</param>
        /// <returns>True if the read operation succeeds; otherwise, false.</returns>
        internal bool GetCoil(ushort address, out Coil coil)
        {
            coil = null;

            var result = TryReadCoil(address, out bool value);
            if (result)
            {
                coil = new Coil { Address = address, Value = value };
            }

            return result;
        }

        /// <summary>
        /// Sets a coil on the Modbus device at the specified address.
        /// </summary>
        /// <param name="address">The address of the coil.</param>
        /// <param name="value">The value to set for the coil.</param>
        /// <returns>True if the write operation succeeds; otherwise, false.</returns>
        internal bool SetCoil(ushort address, bool value)
        {
            return TryWriteCoil(address, value);
        }

        /// <summary>
        /// Gets a holding register from the Modbus device at the specified address.
        /// </summary>
        /// <param name="address">The address of the holding register.</param>
        /// <param name="holdingRegister">When this method returns, contains the holding register if the read operation succeeds, or null if the read operation fails.</param>
        /// <returns>True if the read operation succeeds; otherwise, false.</returns>
        internal bool GetHoldingRegister(
            ushort address,
            out HoldingRegister holdingRegister)
        {
            holdingRegister = null;

            var result = TryReadHoldingRegister(
                address,
                out short value);

            if (result)
            {
                holdingRegister = new HoldingRegister { Address = address, Value = value };
            }

            return result;
        }

        /// <summary>
        /// Sets a holding register on the Modbus device at the specified address.
        /// </summary>
        /// <param name="address">The address of the holding register.</param>
        /// <param name="value">The value to set for the holding register.</param>
        /// <returns>True if the write operation succeeds; otherwise, false.</returns>
        internal bool SetHoldingRegister(
            ushort address,
            short value)
        {
            return TryWriteHoldingRegister(
                address,
                value);
        }
    }
}
