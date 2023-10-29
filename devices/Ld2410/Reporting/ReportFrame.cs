// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ld2410.Reporting
{
    /// <summary>
    /// The base class for all measurement report frames.
    /// </summary>
    public abstract class ReportFrame
    {
        /// <summary>
        /// The frame header value.
        /// </summary>
        public static readonly byte[] Header = new byte[4] { 0xF4, 0xF3, 0xF2, 0xF1 };

        /// <summary>
        /// The frame footer value.
        /// </summary>
        public static readonly byte[] End = new byte[4] { 0xF8, 0xF7, 0xF6, 0xF5 };

        /// <summary>
        /// Gets the type of report of this instance.
        /// </summary>
        public ReportingType DataType { get; internal set; }
    }
}
