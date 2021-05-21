// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Bmxx80.Register
{
    /// <summary>
    /// Registers shared in the Bmxx80 family.
    /// </summary>
    internal enum Bmxx80Register : byte
    {
        CHIPID = 0xD0,
        RESET = 0xE0
    }
}
