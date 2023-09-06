// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Modbus
{
    /// <summary>
    /// Modbus error code.
    /// </summary>
    public enum ErrorCode : byte
    {
        /// <summary>
        /// No error. 无错误.
        /// </summary>
        NoError = 0,

        /// <summary>
        /// Illegal function. 不​支持​接收​的​功能​码。​要​确认​原始​功能​码，​请​从​返回​值​中​减去​0x80.
        /// </summary>
        IllegalFunction = 1,

        /// <summary>
        /// Illegal data address. 请求​尝试​访问​的​地址​无效.
        /// </summary>
        IllegalDataAddress = 2,

        /// <summary>
        /// Illegal data value. 请求​包含​不​正确​的​数据.
        /// </summary>
        IllegalDataValue = 3,

        /// <summary>
        /// Slva device failure. 尝试​处理​请求​时​发生​不可​恢复​的​错误. 这​是​一个​常见​异常​码，​表示​请求​有效，​但从​设备​无法​执行​该​请求.
        /// </summary>
        SlaveDeviceFailure = 4,

        /// <summary>
        /// Acknoledge. 确认。已经接受请求，并且正在处理这个请求，但是需要长持续时间进行这些操作. 返回这个响应防止在客户机（或主站）中发生超时错误.
        /// </summary>
        Acknowledge = 5,

        /// <summary>
        /// Slave device busy. 从属设备忙.
        /// </summary>
        SlaveDeviceBusy = 6,

        /// <summary>
        /// Negative acknowledge. 对于功能 13 或 14（此器件不支持的）请求，将返回此代码.
        /// </summary>
        NegativeAcknowledge = 7,

        /// <summary>
        /// Memory parity error. 存储奇偶性差错.
        /// </summary>
        MemoryParityError = 8,

        /// <summary>
        /// Gateway path. 不可用网关路径.
        /// </summary>
        GatewayPath = 10,

        /// <summary>
        /// Gateway target device. 网关目标设备无响应.
        /// </summary>
        GatewayTargetDevice = 11
    }
}
