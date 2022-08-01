// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing ReadRemoteVersionInformationCompleteEventArgs.
    /// </summary>
    public class ReadRemoteVersionInformationCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// For standard error codes see Bluetooth specification, Vol. 2, part D. For proprietary error code refer to Error codes section.
        /// </summary>
        public readonly byte Status;

        /// <summary>
        /// Connection handle for which the command is given.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// Version of the Current LMP in the remote Controller.
        /// </summary>
        public readonly byte Version;

        /// <summary>
        /// Manufacturer Name of the remote Controller.
        /// </summary>
        public readonly ushort ManufacturerName;

        /// <summary>
        /// Subversion of the LMP in the remote Controller.
        /// </summary>
        public readonly ushort Subversion;

        internal ReadRemoteVersionInformationCompleteEventArgs(
            byte status,
            ushort connectionHandle,
            byte version,
            ushort manufacturerName,
            ushort subversion)
        {
            Status = status;
            ConnectionHandle = connectionHandle;
            Version = version;
            ManufacturerName = manufacturerName;
            Subversion = subversion;
        }
    }
}
