// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Modbus
{
    /// <summary>
    /// Modbus function codes.
    /// </summary>
    public enum FunctionCode : byte
    {
        /// <summary>
        /// Read coils (Fn 1). 读​​线圈.
        /// </summary>
        ReadCoils = 0x01,

        /// <summary>
        /// Read discrete inputs (Fn 2). 读​​离散​量​输入.
        /// </summary>
        ReadDiscreteInputs = 0x02,

        /// <summary>
        /// Reads holding registers (Fn 3). 读​​保持寄存器.
        /// </summary>
        ReadHoldingRegisters = 0x03,

        /// <summary>
        /// Reads input registers (Fn 4). 读​​输入​寄存器.
        /// </summary>
        ReadInputRegisters = 0x04,

        /// <summary>
        /// Writes a single coil (Fn 5). 写​​单​个​线圈.
        /// </summary>
        WriteSingleCoil = 0x05,

        /// <summary>
        /// Writes a single register (Fn 6). 写​​单​个​寄存器.
        /// </summary>
        WriteSingleRegister = 0x06,

        /// <summary>
        /// Diagnostic (Fn 8). 诊断功能.
        /// </summary>
        Diagnostics = 0x08,

        /// <summary>
        /// Writes multiple coils (Fn 15). 写​​多个​线圈.
        /// </summary>
        WriteMultipleCoils = 0x0F,

        /// <summary>
        /// Writes multiple registers (Fn 16). 写​多个​寄存器.
        /// </summary>
        WriteMultipleRegisters = 0x10,

        /// <summary>
        /// Tunnels service requests and method invocations (Fn 43). 封装接口(MEI).
        /// </summary>
        EncapsulatedInterface = 0x2B
    }    
}
