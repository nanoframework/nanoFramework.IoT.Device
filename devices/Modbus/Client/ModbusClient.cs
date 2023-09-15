// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO.Ports;
using Iot.Device.Modbus.Protocol;
using Iot.Device.Modbus.Structures;
using Iot.Device.Modbus.Util;

namespace Iot.Device.Modbus.Client
{
    /// <summary>
    /// Represents a Modbus client that communicates with a Modbus device over a serial port.
    /// </summary>
    public class ModbusClient : Port
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModbusClient"/> class with the specified port settings.
        /// </summary>
        /// <param name="portName">The name of the serial port.</param>
        /// <param name="baudRate">The baud rate. Default is 9600.</param>
        /// <param name="parity">The parity. Default is None.</param>
        /// <param name="dataBits">The number of data bits. Default is 8.</param>
        /// <param name="stopBits">The number of stop bits. Default is One.</param>
        public ModbusClient(
            string portName,
            int baudRate = 9600,
            Parity parity = Parity.None,
            int dataBits = 8,
            StopBits stopBits = StopBits.One)
            : base(portName, baudRate, parity, dataBits, stopBits)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModbusClient"/> class with the specified serial port.
        /// </summary>
        /// <param name="port">The serial port.</param>
        public ModbusClient(SerialPort port) : base(port)
        {
        }

        #region Read methods
        /// <summary>
        /// Reads the values of discrete inputs from the Modbus device.
        /// </summary>
        /// <param name="deviceId">The device ID.</param>
        /// <param name="startAddress">The starting address of the inputs.</param>
        /// <param name="count">The number of inputs to read.</param>
        /// <returns>An array of boolean values representing the state of the discrete inputs.</returns>
        public bool[] ReadDiscreteInputs(byte deviceId, ushort startAddress, ushort count)
        {
            var data = Read(deviceId, startAddress, count, FunctionCode.ReadDiscreteInputs);
            if (data != null)
            {
                var values = new bool[count];
                for (int i = 0; i < count; i++)
                {
                    int posByte = i / 8;
                    int posBit = i % 8;

                    int val = data[posByte] & (byte)Math.Pow(2, posBit);
                    var dinput = new DiscreteInput
                    {
                        Address = (ushort)(startAddress + i),
                        Value = val > 0
                    };
                    values[i] = dinput.Value;
                }

                return values;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Reads the values of input registers from the Modbus device.
        /// </summary>
        /// <param name="deviceId">The device ID.</param>
        /// <param name="startAddress">The starting address of the registers.</param>
        /// <param name="count">The number of registers to read.</param>
        /// <returns>An array of ushort values representing the values of the input registers.</returns>
        public short[] ReadInputRegisters(
            byte deviceId,
            ushort startAddress,
            ushort count)
        {
            var data = Read(
                deviceId,
                startAddress,
                count,
                FunctionCode.ReadInputRegisters);

            if (data != null)
            {
                var values = new short[count];

                for (int i = 0; i < count; i++)
                {
                    var register = new InputRegister
                    {
                        Address = (ushort)(startAddress + i),
                        HiByte = data[i * 2],
                        LoByte = data[(i * 2) + 1]
                    };

                    values[i] = register.Value;
                }

                return values;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Reads the values of coils from the Modbus device.
        /// </summary>
        /// <param name="deviceId">The device ID.</param>
        /// <param name="startAddress">The starting address of the coils.</param>
        /// <param name="count">The number of coils to read.</param>
        /// <returns>An array of boolean values representing the state of the coils.</returns>
        public bool[] ReadCoils(byte deviceId, ushort startAddress, ushort count)
        {
            var data = Read(deviceId, startAddress, count, FunctionCode.ReadCoils);
            if (data != null)
            {
                var values = new bool[count];
                for (int i = 0; i < count; i++)
                {
                    int posByte = i / 8;
                    int posBit = i % 8;

                    int val = data[posByte] & (byte)Math.Pow(2, posBit);
                    var coil = new Coil
                    {
                        Address = (ushort)(startAddress + i),
                        Value = val > 0
                    };

                    values[i] = coil.Value;
                }

                return values;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Reads the values of holding registers from the Modbus device.
        /// </summary>
        /// <param name="deviceId">The device ID.</param>
        /// <param name="startAddress">The starting address of the registers.</param>
        /// <param name="count">The number of registers to read.</param>
        /// <returns>An array of ushort values representing the values of the holding registers.</returns>
        public short[] ReadHoldingRegisters(
            byte deviceId,
            ushort startAddress,
            ushort count)
        {
            var data = Read(
                deviceId,
                startAddress,
                count,
                FunctionCode.ReadHoldingRegisters);

            if (data != null)
            {
                var values = new short[count];

                for (int i = 0; i < count; i++)
                {
                    var register = new HoldingRegister
                    {
                        Address = (ushort)(startAddress + i),
                        HiByte = data[i * 2],
                        LoByte = data[(i * 2) + 1]
                    };

                    values[i] = register.Value;
                }

                return values;
            }
            else
            {
                return null;
            }
        }

        private byte[] Read(byte deviceId, ushort startAddress, ushort count, FunctionCode function)
        {
            if (function != FunctionCode.ReadCoils &&
                function != FunctionCode.ReadDiscreteInputs &&
                function != FunctionCode.ReadHoldingRegisters &&
                function != FunctionCode.ReadInputRegisters)
            {
                throw new ArgumentException(nameof(function));
            }

            // We cannot use the new way of whicth like this becaquse of Style Cop:
            ////ValidParameters(deviceId, startAddress, count, function switch
            ////{
            ////    FunctionCode.ReadCoils => Consts.MaxCoilCountRead,
            ////    FunctionCode.ReadDiscreteInputs => Consts.MaxCoilCountRead,
            ////    FunctionCode.ReadHoldingRegisters => Consts.MaxRegisterCountRead,
            ////    FunctionCode.ReadInputRegisters => Consts.MaxRegisterCountRead,
            ////    _ => ushort.MaxValue
            ////});

            switch (function)
            {
                case FunctionCode.ReadCoils:
                case FunctionCode.ReadDiscreteInputs:
                    ValidParameters(deviceId, startAddress, count, Consts.MaxCoilCountRead);
                    break;
                case FunctionCode.ReadHoldingRegisters:
                case FunctionCode.ReadInputRegisters:
                    ValidParameters(deviceId, startAddress, count, Consts.MaxRegisterCountRead);
                    break;
                default:
                    ValidParameters(deviceId, startAddress, count, ushort.MaxValue);
                    break;
            }

            var response = this.SendRequest(new Request
            {
                DeviceId = deviceId,
                Function = function,
                Address = startAddress,
                Count = count
            });

            if (response.IsValid)
            {
                ValidError(response);
                return response.Data.Buffer;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region Write methods
        /// <summary>
        /// Writes a single coil value to the Modbus device.
        /// </summary>
        /// <param name="deviceId">The device ID.</param>
        /// <param name="startAddress">The address of the coil to write.</param>
        /// <param name="value">The value to write to the coil.</param>
        /// <returns>True if the write operation is successful, false otherwise.</returns>
        public bool WriteSingleCoil(byte deviceId, ushort startAddress, bool value)
        {
            var buffer = new DataBuffer(2);
            buffer.Set(0, (ushort)(value ? 0xFF00 : 0x0000));

            return Write(deviceId, startAddress, 0, buffer, FunctionCode.WriteSingleCoil);
        }

        /// <summary>
        /// Writes a single register value to the Modbus device.
        /// </summary>
        /// <param name="deviceId">The device ID.</param>
        /// <param name="startAddress">The address of the register to write.</param>
        /// <param name="value">The value to write to the register.</param>
        /// <returns>True if the write operation is successful, false otherwise.</returns>
        public bool WriteSingleRegister(
            byte deviceId,
            ushort startAddress,
            short value)
        {
            var register = new HoldingRegister { Value = value };
            var buffer = new DataBuffer(new[] { register.HiByte, register.LoByte });

            return Write(
                deviceId,
                startAddress,
                0,
                buffer,
                FunctionCode.WriteSingleRegister);
        }

        /// <summary>
        /// Writes multiple coil values to the Modbus device.
        /// </summary>
        /// <param name="deviceId">The device ID.</param>
        /// <param name="startAddress">The starting address of the coils to write.</param>
        /// <param name="values">An array of boolean values representing the state of the coils to write.</param>
        /// <returns>True if the write operation is successful, false otherwise.</returns>
        public bool WriteMultipleCoils(byte deviceId, ushort startAddress, bool[] values)
        {
            if (values.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(values));
            }

            int numBytes = (int)Math.Ceiling(values.Length / 8.0);
            byte[] coilBytes = new byte[numBytes];
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i])
                {
                    int posByte = i / 8;
                    int posBit = i % 8;

                    byte mask = (byte)Math.Pow(2, posBit);
                    coilBytes[posByte] = (byte)(coilBytes[posByte] | mask);
                }
            }

            var buffer = new DataBuffer(coilBytes);
            return Write(deviceId, startAddress, (ushort)values.Length, buffer, FunctionCode.WriteMultipleCoils);
        }

        /// <summary>
        /// Writes multiple register values to the Modbus device.
        /// </summary>
        /// <param name="deviceId">The device ID.</param>
        /// <param name="startAddress">The starting address of the registers to write.</param>
        /// <param name="values">An array of ushort values representing the values to write to the registers.</param>
        /// <returns>True if the write operation is successful, false otherwise.</returns>
        public bool WriteMultipleRegisters(byte deviceId, ushort startAddress, ushort[] values)
        {
            if (values.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(values));
            }

            var buffer = new DataBuffer((values.Length * 2) + 1);
            buffer.Set(0, (byte)(values.Length * 2));
            for (int i = 0; i < values.Length; i++)
            {
                buffer.Set((i * 2) + 1, values[i]);
            }

            return Write(deviceId, startAddress, (ushort)values.Length, buffer, FunctionCode.WriteMultipleRegisters);
        }

        private bool Write(byte deviceId, ushort startAddress, ushort count, DataBuffer buffer, FunctionCode function)
        {
            if (function != FunctionCode.WriteSingleCoil &&
                function != FunctionCode.WriteSingleRegister &&
                function != FunctionCode.WriteMultipleCoils &&
                function != FunctionCode.WriteMultipleRegisters)
            {
                throw new ArgumentException(nameof(function));
            }

            if (count > 0)
            {
                // We can't use the new way of using switch because of StyleCop
                ////ValidParameters(deviceId, startAddress, count, function switch
                ////{
                ////    FunctionCode.ReadCoils => Consts.MaxCoilCountWrite,
                ////    FunctionCode.ReadDiscreteInputs => Consts.MaxCoilCountWrite,
                ////    FunctionCode.ReadHoldingRegisters => Consts.MaxRegisterCountWrite,
                ////    FunctionCode.ReadInputRegisters => Consts.MaxRegisterCountWrite,
                ////    _ => ushort.MaxValue
                ////});

                switch (function)
                {
                    case FunctionCode.ReadCoils:
                    case FunctionCode.ReadDiscreteInputs:
                        ValidParameters(deviceId, startAddress, count, Consts.MaxCoilCountWrite);
                        break;
                    case FunctionCode.ReadHoldingRegisters:
                    case FunctionCode.ReadInputRegisters:
                        ValidParameters(deviceId, startAddress, count, Consts.MaxRegisterCountWrite);
                        break;
                    default:
                        ValidParameters(deviceId, startAddress, count, ushort.MaxValue);
                        break;
                }
            }
            else
            {
                ValidParameters(deviceId, startAddress);
            }

            var request = new Request
            {
                DeviceId = deviceId,
                Function = function,
                Address = startAddress,
                Count = count,
                Data = buffer
            };

            var response = SendRequest(request);
            ValidError(response);

            return
               response.IsValid &&
               response.DeviceId == deviceId &&
               response.Function == function &&
               response.Address == startAddress;
        }
        #endregion

        #region Send Raw

        /// <summary>
        /// Sends a raw Modbus request to the device and returns the raw response.
        /// </summary>
        /// <param name="deviceId">The device ID.</param>
        /// <param name="function">The Modbus function code.</param>
        /// <param name="data">The raw data of the request.</param>
        /// <returns>The raw data of the response.</returns>
        public byte[] Raw(byte deviceId, FunctionCode function, byte[] data)
        {
            var request = new Request
            {
                DeviceId = deviceId,
                Function = function,
                Data = new DataBuffer(data)
            };

            var response = SendRequest(request);
            ValidError(response);

            if (response.IsValid)
            {
                return response.Data.Buffer;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region Private Methods

        private Response SendRequest(Request request)
        {
            var buffer = request.Serialize();
            DataWrite(buffer, 0, buffer.Length);

            byte id = 0;

            Response response;

            try
            {
                // read dummy byte (from specs: 3.5 chars time start marker, will never ever be read in a understandible manner)
                _ = DataRead();

                // read device ID
                id = DataRead();

                // Function number
                var fn = DataRead();

                var responseBytes = new DataBuffer(new byte[] { id, fn });
                byte expectedBytes = 1;

                var hasError = (fn & Consts.ErrorMask) > 0;
                if (!hasError)
                {
                    var function = (FunctionCode)fn;
                    switch (function)
                    {
                        case FunctionCode.ReadCoils:
                        case FunctionCode.ReadDiscreteInputs:
                        case FunctionCode.ReadHoldingRegisters:
                        case FunctionCode.ReadInputRegisters:
                            expectedBytes = DataRead();
                            responseBytes.Add(expectedBytes);
                            break;

                        case FunctionCode.WriteSingleCoil:
                        case FunctionCode.WriteSingleRegister:
                        case FunctionCode.WriteMultipleCoils:
                        case FunctionCode.WriteMultipleRegisters:
                            expectedBytes = 4;
                            break;

                        default:
                            if ((fn & Consts.ErrorMask) != 0)
                            {
                                expectedBytes = 1;
                            }

                            break;
                    }
                }

                // CRC Check
                expectedBytes += 2;
                responseBytes.Add(DataRead(expectedBytes));

#if DEBUG
                System.Diagnostics.Debug.WriteLine($"{PortName} RX ({responseBytes.Length}): {Format(responseBytes.Buffer)}");
#endif
                response = new Response(responseBytes.Buffer);
#if DEBUG
                if (response.ErrorCode != ErrorCode.NoError)
                {
                    System.Diagnostics.Debug.WriteLine($"{PortName} RX (E): Modbus ErrorCode {response.ErrorCode}");
                }
#endif
            }
            catch (TimeoutException)
            {
                response = new Response(new byte[0]);
            }

            return response;
        }

        private void ValidParameters(byte deviceId, ushort startAddress)
        {
            if (deviceId < Consts.MinDeviceId || Consts.MaxDeviceId < deviceId)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceId));
            }

            if (startAddress < Consts.MinAddress || Consts.MaxAddress < startAddress)
            {
                throw new ArgumentOutOfRangeException(nameof(startAddress));
            }
        }

        private void ValidParameters(byte deviceId, ushort startAddress, ushort count, ushort maxReadWrite)
        {
            if (deviceId < Consts.MinDeviceId || Consts.MaxDeviceId < deviceId)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceId));
            }

            if (startAddress < Consts.MinAddress || Consts.MaxAddress < startAddress + count)
            {
                throw new ArgumentOutOfRangeException(nameof(startAddress));
            }

            if (count < Consts.MinCount || maxReadWrite < count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
        }

        private void ValidError(Response response)
        {
            if (response.IsValid && response.ErrorCode != ErrorCode.NoError)
            {
                throw new Exception($"{PortName} RX: Modbus ErrorCode {response.ErrorCode}");
            }
        }

        #endregion
    }
}
