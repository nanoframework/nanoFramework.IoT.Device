// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Enum holding address types.
    /// </summary>
    public enum AddressType : byte
    {
        /// <summary>
        /// Public device address.
        /// </summary>
        PublicDeviceAddress = 0x00,

        /// <summary>
        /// Random device address.
        /// </summary>
        RandomDeviceAddress = 0x01,

        /// <summary>
        /// Public identity address.
        /// </summary>
        PublicIdentityAddress = 0x02,

        /// <summary>
        /// Random (static) identity address.
        /// </summary>
        RandomIdentityAddress = 0x03
    }
}
