// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Att error codes.
    /// </summary>
    public enum AttErrorCode : byte
    {
        /// <summary>
        /// Invalid handle.
        /// </summary>
        InvalidHandle = 0x01,

        /// <summary>
        /// Read not permitted.
        /// </summary>
        ReadNotPermitted = 0x02,

        /// <summary>
        /// Write not permitted.
        /// </summary>
        WriteNotPermitted = 0x03,

        /// <summary>
        /// Invalid PDU.
        /// </summary>
        InvalidPdu = 0x04,

        /// <summary>
        /// Insufficient authentication.
        /// </summary>
        InsufficientAuthentication = 0x05,

        /// <summary>
        /// Request not supported.
        /// </summary>
        RequestNotSupported = 0x06,

        /// <summary>
        /// Invalid offset.
        /// </summary>
        InvalidOffset = 0x07,

        /// <summary>
        /// Insufficient Authorization.
        /// </summary>
        InsufficientAuthorization = 0x08,

        /// <summary>
        /// Prepare queue full.
        /// </summary>
        PrepareQueueFull = 0x09,

        /// <summary>
        /// Attribute not found.
        /// </summary>
        AttributeNotFound = 0x0A,

        /// <summary>
        /// Attribute not long.
        /// </summary>
        AttributeNotLong = 0x0B,

        /// <summary>
        /// Insufficient encryption size.
        /// </summary>
        InsufficientEncryptionKeySize = 0x0C,

        /// <summary>
        /// Invalid attribute value length.
        /// </summary>
        InvalidAttributeValueLength = 0x0D,

        /// <summary>
        /// Unlikely error.
        /// </summary>
        UnlikelyError = 0x0E,

        /// <summary>
        /// Insufficient encryption.
        /// </summary>
        InsufficientEncryption = 0x0F,

        /// <summary>
        /// Unsupported group type.
        /// </summary>
        UnsupportedGroupType = 0x10,

        /// <summary>
        /// Insufficient resources.
        /// </summary>
        InsufficientResources = 0x11,
    }
}
