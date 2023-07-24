// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.AtModem.DTOs
{
    /// <summary>
    /// Specifies the type of operator status.
    /// </summary>
    public enum OperatorType
    {
        /// <summary>
        /// The operator status is unknown or not determined.
        /// </summary>
        Unknown,

        /// <summary>
        /// The operator is available for use.
        /// </summary>
        Available,

        /// <summary>
        /// The operator is currently in use.
        /// </summary>
        Current,

        /// <summary>
        /// The operator is forbidden or not allowed for use.
        /// </summary>
        Forbidden,
    }
}
