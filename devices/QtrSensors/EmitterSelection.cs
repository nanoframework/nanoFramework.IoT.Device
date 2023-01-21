// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.QtrSensors
{
    /// <summary>
    /// Select emitters to use.
    /// </summary>
    public enum EmitterSelection
    {
        /// <summary>None.</summary>
        None = 0,

        /// <summary>Even only.</summary>
        Even = 1,

        /// <summary>Odd only.</summary>
        Odd = 2,

        /// <summary>All.</summary>
        All = 3,
    }
}
