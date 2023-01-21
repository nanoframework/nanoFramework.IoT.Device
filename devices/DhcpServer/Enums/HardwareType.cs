// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.DhcpServer.Enums
{
    /// <summary>
    /// Hardware type.
    /// </summary>
    public enum HardwareType
    {
        /// <summary>Ethernet.</summary>
        Ethernet = 0x01,

        /// <summary>Experimental ethernet.</summary>
        ExperimentalEthernet,

        /// <summary>Amateur radio.</summary>
        AmateurRadio,

        /// <summary>Proteon token ring.</summary>
        ProteonTokenRing,

        /// <summary>Chaos.</summary>
        Chaos,

        /// <summary>IEEE802 networks.</summary>
        IEEE802Networks,

        /// <summary>ARC Net.</summary>
        ArcNet,

        /// <summary>Hyper channel.</summary>
        HyperChannel,

        /// <summary>Lanstar.</summary>
        Lanstar
    }
}
