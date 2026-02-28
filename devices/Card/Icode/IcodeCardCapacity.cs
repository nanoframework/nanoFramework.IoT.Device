// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Card.Icode
{
    /// <summary>
    /// Different storage capacity for ICODE cards.
    /// Values represent total memory size in bits.
    /// See https://www.nxp.com.cn/docs/en/application-note/AN11809.pdf
    /// </summary>
    public enum IcodeCardCapacity
    {
        /// <summary>
        /// Unknown card capacity
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// ICODE SLIX — 896 bits (28 blocks x 4 bytes)
        /// </summary>
        IcodeSlix = 896,

        /// <summary>
        /// ICODE SLIX2 — 2528 bits (79 blocks x 4 bytes)
        /// </summary>
        IcodeSlix2 = 2528,

        /// <summary>
        /// ICODE DNA — 2016 bits (63 blocks x 4 bytes)
        /// </summary>
        IcodeDna = 2016,

        /// <summary>
        /// ICODE 3 — 2400 bits (75 blocks x 4 bytes)
        /// </summary>
        Icode3 = 2400,

        /// <summary>
        /// ICODE 3 TagTamper — same capacity as ICODE 3
        /// </summary>
        Icode3TagTamper = Icode3,
    }
}
