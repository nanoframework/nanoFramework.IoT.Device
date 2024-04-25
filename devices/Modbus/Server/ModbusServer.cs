// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO.Ports;
using Iot.Device.Modbus.Protocol;
using Iot.Device.Modbus.Structures;
using Iot.Device.Modbus.Util;

namespace Iot.Device.Modbus.Server
{
    /// <summary>
    /// Modbus server implementation that listens for Modbus requests and handles them.
    /// </summary>
    public class ModbusServer : Port
    {
        private readonly ModbusDevice _device;

        /// <summary>
        /// Initializes a new instance of the ModbusServer class with the specified Modbus device and communication parameters.
        /// </summary>
        /// <param name="device">The Modbus device.</param>
        /// <param name="portName">The name of the serial port.</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="parity">The parity.</param>
        /// <param name="dataBits">The number of data bits.</param>
        /// <param name="stopBits">The stop bits.</param>
        /// <param name="mode">The mode of serial port, default is RS485.</param>
        public ModbusServer(
            ModbusDevice device,
            string portName,
            int baudRate = 9600,
            Parity parity = Parity.None,
            int dataBits = 8,
            StopBits stopBits = StopBits.One,
            SerialMode mode = SerialMode.RS485)
            : base(portName, baudRate, parity, dataBits, stopBits, 6, mode: mode)
        {
            _device = device;
        }

        /// <summary>
        /// Initializes a new instance of the ModbusServer class with the specified Modbus device and serial port.
        /// </summary>
        /// <param name="device">The Modbus device.</param>
        /// <param name="port">The serial port.</param>
        /// <param name="mode">The mode of serial port, default is RS485.</param>
        public ModbusServer(ModbusDevice device, SerialPort port, SerialMode mode = SerialMode.RS485) : base(port, 6, mode)
        {
            _device = device;
        }

        /// <summary>
        /// Starts listening for Modbus requests on the server.
        /// </summary>
        public void StartListening()
        {
            CheckOpen();
        }

        /// <summary>
        /// Called when data is received on the serial port.
        /// </summary>
        /// <param name="bytesToRead">The number of bytes to read from the serial port.</param>
        protected override void DataReceived(int bytesToRead)
        {
            var buffer = DataRead(bytesToRead);

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"{PortName} RX ({buffer.Length}): {Format(buffer)}");
#endif

            if (buffer.Length >= 6)
            {
                try
                {
                    var request = new Request(buffer);
                    if (request.DeviceId == _device.DeviceId)
                    {
                        if (request.IsValid)
                        {
                            var response = HandleRequest(new Request(buffer));
                            var bytes = response.Serialize();

                            DataWrite(bytes, 0, bytes.Length);
                        }
                        else
                        {
                            DataWrite(0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

        #region Private methods       

        private Response HandleRequest(Request request)
        {
            // We can't use this modern way of swith because of StyleCop
            ////var response = request.Function switch
            ////{
            ////    FunctionCode.ReadCoils => HandleReadCoils(request),
            ////    FunctionCode.ReadDiscreteInputs => HandleReadDiscreteInputs(request),
            ////    FunctionCode.ReadHoldingRegisters => HandleReadHoldingRegisters(request),
            ////    FunctionCode.ReadInputRegisters => HandleReadInputRegisters(request),

            ////    FunctionCode.WriteSingleCoil => HandleWriteSingleCoil(request),
            ////    FunctionCode.WriteSingleRegister => HandleWritSingleRegister(request),
            ////    FunctionCode.WriteMultipleCoils => HandleWriteMultipleCoils(request),
            ////    FunctionCode.WriteMultipleRegisters => HandleWriteMultipleRegisters(request),

            ////    _ => new Response(request)
            ////    {
            ////        ErrorCode = (request.Function == FunctionCode.EncapsulatedInterface ?
            ////            ErrorCode.NegativeAcknowledge :
            ////            ErrorCode.IllegalFunction)
            ////    },
            ////};
            Response response;
            switch (request.Function)
            {
                case FunctionCode.ReadCoils:
                    response = HandleReadCoils(request);
                    break;
                case FunctionCode.ReadDiscreteInputs:
                    response = HandleReadDiscreteInputs(request);
                    break;
                case FunctionCode.ReadHoldingRegisters:
                    response = HandleReadHoldingRegisters(request);
                    break;
                case FunctionCode.ReadInputRegisters:
                    response = HandleReadInputRegisters(request);
                    break;
                case FunctionCode.WriteSingleCoil:
                    response = HandleWriteSingleCoil(request);
                    break;
                case FunctionCode.WriteSingleRegister:
                    response = HandleWritSingleRegister(request);
                    break;
                case FunctionCode.WriteMultipleCoils:
                    response = HandleWriteMultipleCoils(request);
                    break;
                case FunctionCode.WriteMultipleRegisters:
                    response = HandleWriteMultipleRegisters(request);
                    break;
                default:
                    response = new Response(request)
                    {
                        ErrorCode = request.Function == FunctionCode.EncapsulatedInterface ?
                        ErrorCode.NegativeAcknowledge :
                        ErrorCode.IllegalFunction
                    };
                    break;
            }

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"ResponeCode: {response.ErrorCode}");
#endif
            return response;
        }

        #region Read requests

        private Response HandleReadCoils(Request request)
        {
            var response = new Response(request);
            if (response.Count < Consts.MinCount || response.Count > Consts.MaxCoilCountRead)
            {
                response.ErrorCode = ErrorCode.IllegalDataValue;
            }

            if (response.ErrorCode != ErrorCode.NoError)
            {
                return response;
            }

            int len = (int)Math.Ceiling(request.Count / 8.0);
            response.Data = new DataBuffer(len);

            _device.BeginRead();
            try
            {
                for (int i = 0; i < request.Count; i++)
                {
                    ushort addr = (ushort)(request.Address + i);
                    if (_device.GetCoil(addr, out Coil coil))
                    {
                        if (coil.Value)
                        {
                            int posByte = i / 8;
                            int posBit = i % 8;

                            byte mask = (byte)Math.Pow(2, posBit);
                            response.Data[posByte] = (byte)(response.Data[posByte] | mask);
                        }
                    }
                    else
                    {
                        response.ErrorCode = ErrorCode.IllegalDataAddress;
                        break;
                    }
                }
            }
            catch
            {
                response.ErrorCode = ErrorCode.SlaveDeviceFailure;
            }
            finally
            {
                _device.EndRead();
            }

            return response;
        }

        private Response HandleReadDiscreteInputs(Request request)
        {
            var response = new Response(request);
            if (response.Count < Consts.MinCount || request.Count > Consts.MaxCoilCountRead)
            {
                response.ErrorCode = ErrorCode.IllegalDataValue;
            }

            if (response.ErrorCode != ErrorCode.NoError)
            {
                return response;
            }

            int len = (int)Math.Ceiling(request.Count / 8.0);
            response.Data = new DataBuffer(len);

            _device.BeginRead();
            try
            {
                for (int i = 0; i < request.Count; i++)
                {
                    ushort addr = (ushort)(request.Address + i);
                    if (_device.GetDiscreteInput(addr, out DiscreteInput discreteInput))
                    {
                        if (discreteInput.Value)
                        {
                            int posByte = i / 8;
                            int posBit = i % 8;

                            byte mask = (byte)Math.Pow(2, posBit);
                            response.Data[posByte] = (byte)(response.Data[posByte] | mask);
                        }
                    }
                    else
                    {
                        response.ErrorCode = ErrorCode.IllegalDataAddress;
                        break;
                    }
                }
            }
            catch
            {
                response.ErrorCode = ErrorCode.SlaveDeviceFailure;
            }
            finally
            {
                _device.EndRead();
            }

            return response;
        }

        private Response HandleReadHoldingRegisters(Request request)
        {
            var response = new Response(request);
            if (response.Count < Consts.MinCount || response.Count > Consts.MaxRegisterCountRead)
            {
                response.ErrorCode = ErrorCode.IllegalDataValue;
            }

            if (response.ErrorCode != ErrorCode.NoError)
            {
                return response;
            }

            response.Data = new DataBuffer(request.Count * 2);

            _device.BeginRead();

            try
            {
                for (int i = 0; i < request.Count; i++)
                {
                    ushort addr = (ushort)(request.Address + i);

                    if (_device.GetHoldingRegister(addr, out HoldingRegister holdingRegister))
                    {
                        response.Data.Set(i * 2, holdingRegister.Value);
                    }
                    else
                    {
                        response.ErrorCode = ErrorCode.IllegalDataAddress;
                        break;
                    }
                }
            }
            catch
            {
                response.ErrorCode = ErrorCode.SlaveDeviceFailure;
            }
            finally
            {
                _device.EndRead();
            }

            return response;
        }

        private Response HandleReadInputRegisters(Request request)
        {
            var response = new Response(request);
            if (response.Count < Consts.MinCount || request.Count > Consts.MaxRegisterCountRead)
            {
                response.ErrorCode = ErrorCode.IllegalDataValue;
            }

            if (response.ErrorCode != ErrorCode.NoError)
            {
                return response;
            }

            response.Data = new DataBuffer(request.Count * 2);

            _device.BeginRead();
            try
            {
                for (int i = 0; i < request.Count; i++)
                {
                    ushort addr = (ushort)(request.Address + i);
                    if (_device.GetInputRegister(addr, out InputRegister inputRegister))
                    {
                        response.Data.Set(i * 2, inputRegister.Value);
                    }
                    else
                    {
                        response.ErrorCode = ErrorCode.IllegalDataAddress;
                        break;
                    }
                }
            }
            catch
            {
                response.ErrorCode = ErrorCode.SlaveDeviceFailure;
            }
            finally
            {
                _device.EndRead();
            }

            return response;
        }

        #endregion

        #region Write requests

        private Response HandleWriteSingleCoil(Request request)
        {
            var response = new Response(request);

            ushort val = request.Data.GetUInt16(0);
            if (val != 0x0000 && val != 0xFF00)
            {
                response.ErrorCode = ErrorCode.IllegalDataValue;
            }

            if (response.ErrorCode != ErrorCode.NoError)
            {
                return response;
            }

            response.Data = request.Data;

            _device.BeginWrite();
            try
            {
                if (!_device.SetCoil(request.Address, val > 0))
                {
                    response.ErrorCode = ErrorCode.IllegalDataAddress;
                }
            }
            catch
            {
                response.ErrorCode = ErrorCode.SlaveDeviceFailure;
            }
            finally
            {
                _device.EndWrite();
            }

            return response;
        }

        private Response HandleWritSingleRegister(Request request)
        {
            var response = new Response(request);
            if (response.ErrorCode != ErrorCode.NoError)
            {
                return response;
            }

            short val = request.Data.GetInt16(0);
            response.Data = request.Data;

            _device.BeginWrite();
            try
            {
                if (!_device.SetHoldingRegister(request.Address, val))
                {
                    response.ErrorCode = ErrorCode.IllegalDataAddress;
                }
            }
            catch
            {
                response.ErrorCode = ErrorCode.SlaveDeviceFailure;
            }
            finally
            {
                _device.EndWrite();
            }

            return response;
        }

        private Response HandleWriteMultipleCoils(Request request)
        {
            var response = new Response(request);

            int numBytes = (int)Math.Ceiling(request.Count / 8.0);
            if (request.Count < Consts.MinCount || request.Count > Consts.MaxCoilCountWrite || numBytes != request.Data.Length)
            {
                response.ErrorCode = ErrorCode.IllegalDataValue;
            }

            if (response.ErrorCode != ErrorCode.NoError)
            {
                return response;
            }

            _device.BeginWrite();
            try
            {
                for (int i = 0; i < request.Count; i++)
                {
                    ushort addr = (ushort)(request.Address + i);

                    int posByte = i / 8;
                    int posBit = i % 8;

                    byte mask = (byte)Math.Pow(2, posBit);
                    int val = request.Data[posByte] & mask;

                    if (!_device.SetCoil(addr, val > 0))
                    {
                        response.ErrorCode = ErrorCode.IllegalDataAddress;
                        break;
                    }
                }
            }
            catch
            {
                response.ErrorCode = ErrorCode.SlaveDeviceFailure;
            }
            finally
            {
                _device.EndWrite();
            }

            return response;
        }

        private Response HandleWriteMultipleRegisters(Request request)
        {
            var response = new Response(request);

            if (request.Count < Consts.MinCount || request.Count > Consts.MaxRegisterCountWrite || request.Count * 2 != request.Data.Length)
            {
                response.ErrorCode = ErrorCode.IllegalDataValue;
            }

            if (response.ErrorCode != ErrorCode.NoError)
            {
                return response;
            }

            _device.BeginWrite();

            try
            {
                for (int i = 0; i < request.Count; i++)
                {
                    ushort addr = (ushort)(request.Address + i);
                    short val = request.Data.GetInt16(i * 2);

                    if (!_device.SetHoldingRegister(addr, val))
                    {
                        response.ErrorCode = ErrorCode.IllegalDataAddress;
                        break;
                    }
                }
            }
            catch
            {
                response.ErrorCode = ErrorCode.SlaveDeviceFailure;
            }
            finally
            {
                _device.EndWrite();
            }

            return response;
        }

        #endregion

        #endregion
    }
}
