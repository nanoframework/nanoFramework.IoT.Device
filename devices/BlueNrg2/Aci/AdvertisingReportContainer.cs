// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// AdvertisingReport struct.
    /// </summary>
    public struct AdvertisingReportContainer
    {
        /// <summary>
        /// Type of advertising report event.
        /// </summary>
        public AdvertisingType EventType;

        /// <summary>
        /// Type of address.
        /// </summary>
        public AddressType AddressType;

        /// <summary>
        /// Public Device Address, Random Device Address, Public Identity Address or Random (static) Identity Address of the advertising device.
        /// </summary>
        public byte[] Address;

        /// <summary>
        /// Length of the Data[i] field for each device which responded.
        /// </summary>
        public byte DataLength;

        /// <summary>
        /// Length_Data[i] octets of advertising or scan response data formatted as defined in [Vol 3] Part C, Section 8.
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// N Size: 1 Octet (signed integer)
        /// Units: dBm
        /// </summary>
        public sbyte Rssi;
    }
}
