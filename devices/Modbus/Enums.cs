namespace Iot.Device.Modbus
{
    public enum FunctionCode : byte
    {
        /// <summary>
        /// 读​​线圈 Read coils (Fn 1).
        /// </summary>
        ReadCoils = 0x01,

        /// <summary>
        /// 读​​离散​量​输入 Read discrete inputs (Fn 2).
        /// </summary>
        ReadDiscreteInputs = 0x02,

        /// <summary>
        /// 读​​保持寄存器 Reads holding registers (Fn 3).
        /// </summary>
        ReadHoldingRegisters = 0x03,

        /// <summary>
        /// 读​​输入​寄存器 Reads input registers (Fn 4).
        /// </summary>
        ReadInputRegisters = 0x04,

        /// <summary>
        /// 写​​单​个​线圈 Writes a single coil (Fn 5).
        /// </summary>
        WriteSingleCoil = 0x05,

        /// <summary>
        /// 写​​单​个​寄存器 Writes a single register (Fn 6).
        /// </summary>
        WriteSingleRegister = 0x06,

        /// <summary>
        /// 诊断功能 Diagnostic (Fn 8).
        /// </summary>
        Diagnostics = 0x08,

        /// <summary>
        /// 写​​多个​线圈 Writes multiple coils (Fn 15).
        /// </summary>
        WriteMultipleCoils = 0x0F,

        /// <summary>
        /// 写​多个​寄存器 Writes multiple registers (Fn 16).
        /// </summary>
        WriteMultipleRegisters = 0x10,

        /// <summary>
		/// 封​装​接口​(MEI)​​ Tunnels service requests and method invocations (Fn 43).
		/// </summary>
        EncapsulatedInterface = 0x2B
    }

    public enum ErrorCode : byte
    {
        /// <summary>
        /// 无错误
        /// </summary>
        NoError = 0,

        /// <summary>
        /// 不​支持​接收​的​功能​码。​要​确认​原始​功能​码，​请​从​返回​值​中​减去​0x80
        /// </summary>
        IllegalFunction = 1,

        /// <summary>
        /// 请求​尝试​访问​的​地址​无效
        /// </summary>
        IllegalDataAddress = 2,

        /// <summary>
        /// 请求​包含​不​正确​的​数据
        /// </summary>
        IllegalDataValue = 3,

        /// <summary>
        /// 尝试​处理​请求​时​发生​不可​恢复​的​错误
        /// 这​是​一个​常见​异常​码，​表示​请求​有效，​但从​设备​无法​执行​该​请求
        /// </summary>
        SlaveDeviceFailure = 4,

        /// <summary>
        /// 确认。已经接受请求，并且正在处理这个请求，但是需要长持续时间进行这些操作
        /// 返回这个响应防止在客户机（或主站）中发生超时错误
        /// </summary>
        Acknowledge = 5,

        /// <summary>
        /// 从属设备忙
        /// </summary>
        SlaveDeviceBusy = 6,

        /// <summary>
        /// 对于功能 13 或 14（此器件不支持的）请求，将返回此代码
        /// </summary>
        NegativeAcknowledge = 7,

        /// <summary>
        /// 存储奇偶性差错
        /// </summary>
        MemoryParityError = 8,

        /// <summary>
        /// 不可用网关路径
        /// </summary>
        GatewayPath = 10,

        /// <summary>
        /// 网关目标设备无响应
        /// </summary>
        GatewayTargetDevice = 11
    }
}
