// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Atecc608
{
    /// <summary>
    /// ATECC608 status and error codes returned by the device.
    /// </summary>
    public enum Atecc608Status : byte
    {
        /// <summary>Command executed successfully.</summary>
        Success = 0x00,

        /// <summary>CheckMac or Verify miscompare.</summary>
        CheckmacVerifyMiscompare = 0x01,

        /// <summary>Parse error: the command was not properly formed.</summary>
        ParseError = 0x03,

        /// <summary>ECC fault occurred during computation.</summary>
        EccFault = 0x05,

        /// <summary>Self test error.</summary>
        SelfTestError = 0x07,

        /// <summary>Health test error.</summary>
        HealthTestError = 0x08,

        /// <summary>Execution error: the command could not execute.</summary>
        ExecutionError = 0x0F,

        /// <summary>Device is awake, returned after a successful wake.</summary>
        AfterWake = 0x11,

        /// <summary>Watchdog is about to expire, re-wake required.</summary>
        WatchdogExpiring = 0xEE,

        /// <summary>Communication error (CRC mismatch or other).</summary>
        CommunicationError = 0xFF,
    }
}
