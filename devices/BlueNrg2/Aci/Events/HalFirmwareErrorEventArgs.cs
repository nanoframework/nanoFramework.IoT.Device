// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing HalFirmwareErrorEventArgs.
    /// </summary>
    public class HalFirmwareErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Error code identifying the type of error that has occurred.
        /// </summary>
        public readonly FirmwareErrorType ErrorType;

        /// <summary>
        /// Length of Data in octets.
        /// </summary>
        public readonly byte DataLength;

        /// <summary>
        /// If FW_Error_Type is 0x01, 0x02 or 0x03, this parameter contains
        /// the connection handle where the abnormal condition has occurred.
        /// </summary>
        public readonly byte[] Data;

        internal HalFirmwareErrorEventArgs(FirmwareErrorType errorType, byte dataLength, byte[] data)
        {
            ErrorType = errorType;
            DataLength = dataLength;
            Data = data;
        }
    }
}
