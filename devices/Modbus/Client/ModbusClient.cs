using Iot.Device.Modbus.Protocol;
using Iot.Device.Modbus.Structures;
using Iot.Device.Modbus.Util;
using System;
using System.IO.Ports;

namespace Iot.Device.Modbus.Client
{
    public class ModbusClient : Port
    {
        public ModbusClient(
            string portName,
            int baudRate = 9600,
            Parity parity = Parity.None,
            int dataBits = 8,
            StopBits stopBits = StopBits.One
            ) : base(portName, baudRate, parity, dataBits, stopBits)
        {
        }

        public ModbusClient(SerialPort port) : base(port)
        {
        }

        #region Read methods
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
                return null;
        }

        public ushort[] ReadInputRegisters(byte deviceId, ushort startAddress, ushort count)
        {
            var data = Read(deviceId, startAddress, count, FunctionCode.ReadInputRegisters);
            if (data != null)
            {
                var values = new ushort[count];
                for (int i = 0; i < count; i++)
                {
                    var register = new InputRegister
                    {
                        Address = (ushort)(startAddress + i),
                        HiByte = data[i * 2],
                        LoByte = data[i * 2 + 1]
                    };
                    values[i] = register.Value;
                }
                return values;
            }
            else
                return null;
        }

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
                return null;
        }

        public ushort[] ReadHoldingRegisters(byte deviceId, ushort startAddress, ushort count)
        {
            var data = Read(deviceId, startAddress, count, FunctionCode.ReadHoldingRegisters);
            if (data != null)
            {
                var values = new ushort[count];
                for (int i = 0; i < count; i++)
                {
                    var register = new HoldingRegister
                    {
                        Address = (ushort)(startAddress + i),
                        HiByte = data[i * 2],
                        LoByte = data[i * 2 + 1]
                    };
                    values[i] = register.Value;
                }
                return values;
            }
            else
                return null;
        }

        private byte[] Read(byte deviceId, ushort startAddress, ushort count, FunctionCode function)
        {
            if (function != FunctionCode.ReadCoils &&
                function != FunctionCode.ReadDiscreteInputs &&
                function != FunctionCode.ReadHoldingRegisters &&
                function != FunctionCode.ReadInputRegisters)
                throw new ArgumentException(nameof(function));

            ValidParameters(deviceId, startAddress, count, function switch
            {
                FunctionCode.ReadCoils => Consts.MaxCoilCountRead,
                FunctionCode.ReadDiscreteInputs => Consts.MaxCoilCountRead,
                FunctionCode.ReadHoldingRegisters => Consts.MaxRegisterCountRead,
                FunctionCode.ReadInputRegisters => Consts.MaxRegisterCountRead,
                _ => ushort.MaxValue
            });

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
                return null;
        }
        #endregion

        #region Write methods
        public bool WriteSingleCoil(byte deviceId, ushort startAddress, bool value)
        {
            var buffer = new DataBuffer(2);
            buffer.Set(0, (ushort)(value ? 0xFF00 : 0x0000));

            return Write(deviceId, startAddress, 0, buffer, FunctionCode.WriteSingleCoil);
        }

        public bool WriteSingleRegister(byte deviceId, ushort startAddress, ushort value)
        {
            var register = new HoldingRegister { Value = value };
            var buffer = new DataBuffer(new[] { register.HiByte, register.LoByte });

            return Write(deviceId, startAddress, 0, buffer, FunctionCode.WriteSingleRegister);
        }

        public bool WriteMultipleCoils(byte deviceId, ushort startAddress, bool[] values)
        {
            if (values.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(values));

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

        public bool WriteMultipleRegisters(byte deviceId, ushort startAddress, ushort[] values)
        {
            if (values.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(values));

            var buffer = new DataBuffer(values.Length * 2 + 1);
            buffer.Set(0, (byte)(values.Length * 2));
            for (int i = 0; i < values.Length; i++)
            {
                buffer.Set(i * 2 + 1, values[i]);
            }

            return Write(deviceId, startAddress, (ushort)values.Length, buffer, FunctionCode.WriteMultipleRegisters);
        }

        private bool Write(byte deviceId, ushort startAddress, ushort count, DataBuffer buffer, FunctionCode function)
        {
            if (function != FunctionCode.WriteSingleCoil &&
                function != FunctionCode.WriteSingleRegister &&
                function != FunctionCode.WriteMultipleCoils &&
                function != FunctionCode.WriteMultipleRegisters)
                throw new ArgumentException(nameof(function));

            if (count > 0)
            {
                ValidParameters(deviceId, startAddress, count, function switch
                {
                    FunctionCode.ReadCoils => Consts.MaxCoilCountWrite,
                    FunctionCode.ReadDiscreteInputs => Consts.MaxCoilCountWrite,
                    FunctionCode.ReadHoldingRegisters => Consts.MaxRegisterCountWrite,
                    FunctionCode.ReadInputRegisters => Consts.MaxRegisterCountWrite,
                    _ => ushort.MaxValue
                });
            }
            else
                ValidParameters(deviceId, startAddress);

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
                return response.Data.Buffer;
            else
                return null;
        }
        #endregion

        #region Private Methods

        Response SendRequest(Request request)
        {
            var buffer = request.Serialize();
            this.DataWrite(buffer, 0, buffer.Length);

            Response response;
            try
            {
                // Device/Slave ID
                var id = DataRead();
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
                                expectedBytes = 1;

                            break;
                    }
                }

                expectedBytes += 2; // CRC Check
                responseBytes.Add(DataRead(expectedBytes));

#if DEBUG
                System.Diagnostics.Debug.WriteLine(String.Format("{0} RX ({1}): {2}", this.PortName, responseBytes.Length, Format(responseBytes.Buffer)));
#endif
                response = new Response(responseBytes.Buffer);
#if DEBUG
                if (response.ErrorCode != ErrorCode.NoError)
                    System.Diagnostics.Debug.WriteLine(String.Format("{0} RX (E): Modbus ErrorCode {1}", this.PortName, response.ErrorCode));
#endif
            }
            catch (TimeoutException)
            {
                response = new Response(new byte[0]);
            }
            return response;
        }

        void ValidParameters(byte deviceId, ushort startAddress)
        {
            if (deviceId < Consts.MinDeviceId || Consts.MaxDeviceId < deviceId)
                throw new ArgumentOutOfRangeException(nameof(deviceId));

            if (startAddress < Consts.MinAddress || Consts.MaxAddress < startAddress)
                throw new ArgumentOutOfRangeException(nameof(startAddress));
        }

        void ValidParameters(byte deviceId, ushort startAddress, ushort count, ushort maxReadWrite)
        {
            if (deviceId < Consts.MinDeviceId || Consts.MaxDeviceId < deviceId)
                throw new ArgumentOutOfRangeException(nameof(deviceId));

            if (startAddress < Consts.MinAddress || Consts.MaxAddress < startAddress + count)
                throw new ArgumentOutOfRangeException(nameof(startAddress));

            if (count < Consts.MinCount || maxReadWrite < count)
                throw new ArgumentOutOfRangeException(nameof(count));
        }

        void ValidError(Response response)
        {
            if (response.IsValid && response.ErrorCode != ErrorCode.NoError)
                throw new Exception(String.Format("{0} RX: Modbus ErrorCode {1}", this.PortName, response.ErrorCode));
        }

        #endregion
    }
}
