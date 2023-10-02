// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.AtModem.FileStorage
{
    /// <summary>
    /// File mdoe creation.
    /// </summary>
    public enum CreateMode
    {
        /// <summary>
        /// Open file for reading.
        /// </summary>
        Override = 0,

        /// <summary>
        /// Open file for appending.
        /// </summary>
        Append = 1
    }
}
