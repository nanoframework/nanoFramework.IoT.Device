// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Card.Icode
{
    /// <summary>
    /// List of commands available for ISO 15693 / ICODE cards.
    /// See ISO/IEC 15693-3 for command definitions.
    /// </summary>
    internal enum IcodeCardCommand
    {
        /// <summary>
        /// Inventory — detect cards in the RF field
        /// </summary>
        Inventory = 0x01,

        /// <summary>
        /// Stay Quiet — set VICC to quiet state
        /// </summary>
        StayQuiet = 0x02,

        /// <summary>
        /// Read Single Block — read a single 4-byte block
        /// </summary>
        ReadSingleBlock = 0x20,

        /// <summary>
        /// Write Single Block — write a single 4-byte block
        /// </summary>
        WriteSingleBlock = 0x21,

        /// <summary>
        /// Lock Block — permanently lock a block
        /// </summary>
        LockBlock = 0x22,

        /// <summary>
        /// Read Multiple Blocks — read contiguous blocks
        /// </summary>
        ReadMultipleBlocks = 0x23,

        /// <summary>
        /// Write Multiple Blocks — write contiguous blocks
        /// </summary>
        WriteMultipleBlocks = 0x24,

        /// <summary>
        /// Select — select a specific card by UID
        /// </summary>
        Select = 0x25,

        /// <summary>
        /// Reset to Ready — return card to ready state
        /// </summary>
        ResetToReady = 0x26,

        /// <summary>
        /// Write AFI — write Application Family Identifier
        /// </summary>
        WriteAfi = 0x27,

        /// <summary>
        /// Lock AFI — permanently lock the AFI
        /// </summary>
        LockAfi = 0x28,

        /// <summary>
        /// Write DSFID — write Data Storage Format Identifier
        /// </summary>
        WriteDsfid = 0x29,

        /// <summary>
        /// Lock DSFID — permanently lock the DSFID
        /// </summary>
        LockDsfid = 0x2A,

        /// <summary>
        /// Get System Information — retrieve card system info (DSFID, AFI, capacity, IC reference)
        /// </summary>
        GetSystemInformation = 0x2B,

        /// <summary>
        /// Get Multiple Block Security Status — retrieve lock status of blocks
        /// </summary>
        GetMultipleBlockSecurityStatus = 0x2C,
    }
}
