// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Radio states.
    /// </summary>
    public enum RadioState : byte
    {
        /// <summary>
        /// Idle.
        /// </summary>
        Idle = 0x00,

        /// <summary>
        /// Advertising.
        /// </summary>
        Advertising = 0x01,

        /// <summary>
        /// Connection event slave.
        /// </summary>
        ConnectionEventSlave = 0x02,

        /// <summary>
        /// Scanning.
        /// </summary>
        Scanning = 0x03,

        /// <summary>
        /// Connection request.
        /// </summary>
        ConnectionRequest = 0x04,

        /// <summary>
        /// Connection event master.
        /// </summary>
        ConnectionEventMaster = 0x05,

        /// <summary>
        /// Transmitter test mode.
        /// </summary>
        TxTestMode = 0x06,

        /// <summary>
        /// Receiver test mode.
        /// </summary>
        RxTestMode = 0x07
    }
}
