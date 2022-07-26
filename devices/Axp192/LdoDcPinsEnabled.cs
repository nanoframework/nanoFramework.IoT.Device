﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Axp192
{
    /// <summary>
    /// LDO and DC pin enabled.
    /// </summary>
    [Flags]
    public enum LdoDcPinsEnabled
    {
        /// <summary>LDO3 pin.</summary>
        Ldo3 = 0b0000_1000,

        /// <summary>LDO2 pin.</summary>
        Ldo2 = 0b0000_0100,

        /// <summary>DC-DC3 pin.</summary>
        DcDc3 = 0b0000_0010,

        /// <summary>DC-DC1 pin.</summary>
        DcDc1 = 0b0000_0001,

        /// <summary>None enabled.</summary>
        None = 0b0000_0000,

        /// <summary>All enabled.</summary>
        All = 0b0000_1111,
    }
}
