// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2
{
    /// <summary>
    /// Event mask for bluetooth LE events.
    /// </summary>
    public enum LeEventMask : ulong
    {
        /// <summary>
        /// No LE events specified.
        /// </summary>
        None = 0x0000000000000000,

        /// <summary>
        /// Connection complete event bit.
        /// </summary>
        ConnectionCompleteEvent = 0x0000000000000001,

        /// <summary>
        /// Advertising report event bit.
        /// </summary>
        AdvertisingReportEvent = 0x0000000000000002,

        /// <summary>
        /// Connection update complete event bit.
        /// </summary>
        ConnectionUpdateCompleteEvent = 0x0000000000000004,

        /// <summary>
        /// Read remote used features complete event bit.
        /// </summary>
        ReadRemoteUsedFeaturesCompleteEvent = 0x0000000000000008,

        /// <summary>
        /// Long term key request event bit.
        /// </summary>
        LongTermKeyRequestEvent = 0x0000000000000010,

        /// <summary>
        /// Remote connection parameter request event bit.
        /// </summary>
        RemoteConnectionParameterRequestEvent = 0x0000000000000020,

        /// <summary>
        /// Data length change event bit.
        /// </summary>
        DataLengthChangeEvent = 0x0000000000000040,

        /// <summary>
        /// Read local P-256 public key complete event bit.
        /// </summary>
        ReadPublicKeyCompleteEvent = 0x0000000000000080,

        /// <summary>
        /// Generate DHKey complete event bit.
        /// </summary>
        GenerateDhKeyCompleteEvent = 0x0000000000000100,

        /// <summary>
        /// Enhanced connection complete event bit.
        /// </summary>
        EnhancedConnectionCompleteEvent = 0x0000000000000200,

        /// <summary>
        /// Direct advertising report event bit.
        /// </summary>
        DirectAdvertisingReportEvent = 0x0000000000000400,
    }
}
