// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Modbus.Client;
using Iot.Device.Modbus.Server;
using Microsoft.Extensions.Logging;
using nanoFramework.Hardware.Esp32;
using nanoFramework.Logging.Debug;
using System;
using System.Diagnostics;
using System.Threading;

namespace Iot.Device.Modbus.Samples
{
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            // SerialPort COM2
            Configuration.SetPinFunction(16, DeviceFunction.COM2_RX);
            Configuration.SetPinFunction(17, DeviceFunction.COM2_TX);
            Configuration.SetPinFunction(18, DeviceFunction.COM2_RTS);

            // SerialPort COM3
            Configuration.SetPinFunction(25, DeviceFunction.COM3_RX);
            Configuration.SetPinFunction(26, DeviceFunction.COM3_TX);
            Configuration.SetPinFunction(27, DeviceFunction.COM3_RTS);

            // Modbus Server (RS485 mode)
            //var server = new ModbusServer(new Device(1), "COM2");

            // Modbus Server (RS232 mode)
            // var server= new ModbusServer(new Device(1),"COM2",mode:SerialMode.Normal);

            //server.ReadTimeout = server.WriteTimeout = 2000;
            //server.StartListening();

            // Modbus Client (RS485 mode)
            var client = new ModbusClient("COM2");

            // Modbus Client (RS232 mode)
            // var client = new ModbusClient("COM3",mode:SerialMode.Normal);

            client.ReadTimeout = client.WriteTimeout = 2000;

            // add logger for debugging purposes (optional)
            DebugLogger logger = new("ModbusClient");
            client.Logger = logger;

            // set desired logging level
            logger.MinLogLevel = LogLevel.Information;

            client.WriteMultipleRegisters(2, 0x5, new ushort[] { 3, 5, 2, 3 });
            client.Raw(2, FunctionCode.Diagnostics, new byte[] { 0x01, 0x01, 0x01, 0x01 });

            var data1 = client.ReadHoldingRegisters(2, 0x7, 4);
            var data2 = client.ReadCoils(2, 0x23, 2);

            Thread.Sleep(Timeout.Infinite);
        }
    }

    class Device : ModbusDevice
    {
        public Device(byte id = 1) : base(id)
        {
        }

        /// <summary>
        /// Read Coil
        /// </summary>
        /// <param name="address">Register address</param>
        /// <param name="value">Output register values</param>
        /// <returns>True: address is readable; False: address is invalid</returns>
        protected override bool TryReadCoil(ushort address, out bool value)
        {
            // Your code is here...
            switch (address)
            {
                case 100:
                    value = false;  // Gets the switch status
                    break;
                case 101:
                    value = true;  // Gets the switch status
                    break;
                default:
                    value = false;
                    return false;
            }
            return true;
        }

        protected override bool TryReadDiscreteInput(ushort address, out bool value)
        {
            // Similar to the code above...
            throw new NotImplementedException();
        }

        protected override bool TryReadHoldingRegister(
            ushort address,
            out short value)
        {
            // Similar to the code above...
            throw new NotImplementedException();
        }

        protected override bool TryReadInputRegister(ushort address, out short value)
        {
            // Similar to the code above...
            throw new NotImplementedException();
        }

        protected override bool TryWriteCoil(ushort address, bool value)
        {
            // Similar to the code above...
            throw new NotImplementedException();
        }

        protected override bool TryWriteHoldingRegister(ushort address, short value)
        {
            // Similar to the code above...
            throw new NotImplementedException();
        }
    }
}
