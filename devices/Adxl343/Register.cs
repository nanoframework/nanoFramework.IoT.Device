// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.ADXL343Lib
{
    /// <summary>
    /// Register.
    /// </summary>
    internal enum Register : byte
    {
        /// <summary>
        /// Device Id.
        /// </summary>
        DevId = 0x00,

        /// <summary>
        /// Tap Threshold.
        /// </summary>
        ThreshTap = 0x1D,

        /// <summary>
        /// X Offset.
        /// </summary>
        OfsX = 0x1E,

        /// <summary>
        /// Y Offset.
        /// </summary>
        OfsY = 0x1F,

        /// <summary>
        /// Z Offset.
        /// </summary>
        OfsZ = 0x20,

        /// <summary>
        /// Durration.
        /// </summary>
        Dur = 0x21,

        /// <summary>
        /// Latency.
        /// </summary>
        Latent = 0x22,

        /// <summary>
        /// Window.
        /// </summary>
        Window = 0x23,

        /// <summary>
        /// Threshold Active.
        /// </summary>
        ThreshAct = 0x24,

        /// <summary>
        /// Threshold Inactive.
        /// </summary>
        ThreshInact = 0x25,

        /// <summary>
        /// Time Inactive.
        /// </summary>
        TimeInact = 0x26,

        /// <summary>
        /// Active/Inactive Control.
        /// </summary>
        ActInactCtl = 0x27,

        /// <summary>
        /// Threshold FF.
        /// </summary>
        ThreshFF = 0x28,

        /// <summary>
        /// Time FF.
        /// </summary>
        TimeFF = 0x29,

        /// <summary>
        /// Tap Axes.
        /// </summary>
        TapAxes = 0x2A,

        /// <summary>
        /// Active Tap Status.
        /// </summary>
        ActTapStatus = 0x2B,

        /// <summary>
        /// BW Rate.
        /// </summary>
        BwRate = 0x2C,

        /// <summary>
        /// Power Control.
        /// </summary>
        PowerCtl = 0x2D,

        /// <summary>
        /// Interrupt Enable.
        /// </summary>
        IntEnable = 0x2E,

        /// <summary>
        /// Interrupt Map.
        /// </summary>
        IntMap = 0x2F,

        /// <summary>
        /// Interrupt Source.
        /// </summary>
        IntSource = 0x30,

        /// <summary>
        /// Data Format.
        /// </summary>
        DataFormat = 0x31,

        /// <summary>
        /// X0.
        /// </summary>
        X0 = 0x32,  // 2 bytes

        /// <summary>
        /// Y0.
        /// </summary>
        Y0 = 0x34,  // 2 bytes

        /// <summary>
        /// Z0.
        /// </summary>
        Z0 = 0x36,  // 2 bytes

        /// <summary>
        /// FIFO Control.
        /// </summary>
        FifoCtl = 0x38,

        /// <summary>
        /// FIFO Status.
        /// </summary>
        FifoStatus = 0x39
    }
}
