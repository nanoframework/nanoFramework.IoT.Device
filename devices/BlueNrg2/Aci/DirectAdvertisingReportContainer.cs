// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Data in a direct advertising report.
    /// </summary>
    public struct DirectAdvertisingReportContainer
    {
        /// <summary>
        /// <see cref="AdvertisingType"/>>.
        /// </summary>
        public AdvertisingType EventType;

        /// <summary>
        /// <see cref="Aci.AddressType"/>.
        /// </summary>
        public AddressType AddressType;

        /// <summary>
        /// Public Device Address, Random Device Address, Public Identity Address or Random
        /// (static) Identity Address of the advertising device.
        /// </summary>
        public byte[] Address;

        /// <summary>
        /// 0x01 Random device address.
        /// </summary>
        public AddressType DirectAddressType;

        /// <summary>
        /// Random device address.
        /// </summary>
        public byte[] DirectAddress;

        /// <summary>
        /// N Size: 1 Octet (signed integer) Units: dBm
        /// </summary>
        public sbyte RSSI;
    }
}
