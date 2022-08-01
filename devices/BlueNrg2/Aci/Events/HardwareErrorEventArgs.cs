// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing HardwareErrorEventArgs.
    /// </summary>
    public class HardwareErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Hardware Error Event code. Error code 0x01 and 0x02 are
        /// errors generally caused by hardware issue on the PCB; another possible
        /// cause is a slow crystal startup. In the latter case, the
        /// HS_STARTUP_TIME in the device configuration needs to be tuned. Error
        /// code 0x03 indicates an internal error of the protocol stack. After
        /// this event is recommended to force device reset.
        /// </summary>
        public readonly HardwareErrorCode HardwareCode;

        internal HardwareErrorEventArgs(HardwareErrorCode hardwareCode)
        {
            HardwareCode = hardwareCode;
        }
    }

    /// <summary>
    /// Hardware error codes.
    /// </summary>
    public enum HardwareErrorCode : byte
    {
        /// <summary>
        /// Radio state error. Generally caused by hardware issue on the PCB.
        /// </summary>
        RadioStateError = 0x01,

        /// <summary>
        /// Timer overrun error. Generally caused by hardware issue on the PCB; another possible
        /// cause is a slow crystal startup. In the latter case, the
        /// HS_STARTUP_TIME in the device configuration needs to be tuned.
        /// </summary>
        TimerOverrunError = 0x02,

        /// <summary>
        /// Internal queue overflow error. Indicates an internal error of the protocol stack.
        /// </summary>
        InternalQueueOverflowError = 0x03,
    }
}
