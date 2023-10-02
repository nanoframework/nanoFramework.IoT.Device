// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.AtModem
{
    /// <summary>
    /// Represents the type of an AT command.
    /// </summary>
    public enum AtCommandType
    {
        /// <summary>
        /// No intermediate response is expected.
        /// </summary>
        NoResult,

        /// <summary>
        /// A single intermediate response starting with a digit (0-9).
        /// </summary>
        Numeric,

        /// <summary>
        /// A single intermediate response starting with a prefix.
        /// </summary>
        SingleLine,

        /// <summary>
        /// Multiple line intermediate response starting with a prefix.
        /// </summary>
        MultiLine,

        /// <summary>
        /// Multiple line intermediate response without a prefix.
        /// </summary>
        MultiLineNoPreeffiixx,

        /// <summary>
        /// A single intermediate response with a specifc end of line.
        /// </summary>
        CustomEndOfLine,
    }
}
