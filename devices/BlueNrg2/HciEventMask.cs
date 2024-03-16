// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2
{
    /// <summary>
    /// Event mask for HCI events.
    /// </summary>
    [Flags]
    public enum HciEventMask : ulong
    {
        /// <summary>
        /// No events specified.
        /// </summary>
        NoEventsSpecified = 0x0000000000000000,

        /// <summary>
        /// Inquiry complete event bit.
        /// </summary>
        InquiryCompleteEvent = 0x0000000000000001,

        /// <summary>
        /// Inquiry result event bit.
        /// </summary>
        InquiryResultEvent = 0x0000000000000002,

        /// <summary>
        /// Connection complete event bit.
        /// </summary>
        ConnectionCompleteEvent = 0x0000000000000004,

        /// <summary>
        /// Connection request event bit.
        /// </summary>
        ConnectionRequestEvent = 0x0000000000000008,

        /// <summary>
        /// Disconnection complete event bit.
        /// </summary>
        DisconnectionCompleteEvent = 0x0000000000000010,

        /// <summary>
        /// Authentication complete event bit.
        /// </summary>
        AuthenticationCompleteEvent = 0x0000000000000020,

        /// <summary>
        /// Remote name request complete event bit.
        /// </summary>
        RemoteNameRequestCompleteEvent = 0x0000000000000040,

        /// <summary>
        /// Encryption change event bit.
        /// </summary>
        EncryptionChangeEvent = 0x0000000000000080,

        /// <summary>
        /// Change connection link key complete event bit.
        /// </summary>
        ChangeConnectionLinkKeyCompleteEvent = 0x0000000000000100,

        /// <summary>
        /// Master link key complete event bit.
        /// </summary>
        MasterLinkKeyCompleteEvent = 0x0000000000000200,

        /// <summary>
        /// Read remote supported features complete event bit.
        /// </summary>
        ReadRemoteSupportedFeaturesCompleteEvent = 0x0000000000000400,

        /// <summary>
        /// read remote version information complete event bit.
        /// </summary>
        ReadRemoteVersionInformationCompleteEvent = 0x0000000000000800,

        /// <summary>
        /// QoS setup complete event bit.
        /// </summary>
        QosSetupCompleteEvent = 0x0000000000001000,

        /// <summary>
        /// Hardware error event bit.
        /// </summary>
        HardwareErrorEvent = 0x0000000000008000,

        /// <summary>
        /// Flush occurred event bit.
        /// </summary>
        FlushOccurredEvent = 0x0000000000010000,

        /// <summary>
        /// Role change event bit.
        /// </summary>
        RoleChangeEvent = 0x0000000000020000,

        /// <summary>
        /// Mode change event bit.
        /// </summary>
        ModeChangeEvent = 0x0000000000080000,

        /// <summary>
        /// return link keys event bit.
        /// </summary>
        ReturnLinkKeysEvent = 0x0000000000100000,

        /// <summary>
        /// PIN code request event bit.
        /// </summary>
        PinCodeRequestEvent = 0x0000000000200000,

        /// <summary>
        /// Link key request event bit.
        /// </summary>
        LinkKeyRequestEvent = 0x0000000000400000,

        /// <summary>
        /// Link key notification event bit.
        /// </summary>
        LinkKeyNotificationEvent = 0x0000000000800000,

        /// <summary>
        /// Loopback command event bit.
        /// </summary>
        LoopbackCommandEvent = 0x0000000001000000,

        /// <summary>
        /// Data buffer overflow event bit.
        /// </summary>
        DataBufferOverflowEvent = 0x0000000002000000,

        /// <summary>
        /// Max slots change event bit.
        /// </summary>
        MaxSlotsChangeEvent = 0x0000000004000000,

        /// <summary>
        /// Read clock offset complete event bit.
        /// </summary>
        ReadClockOffsetCompleteEvent = 0x0000000008000000,

        /// <summary>
        /// Connection packet type changed event bit.
        /// </summary>
        ConnectionPacketTypeChangedEvent = 0x0000000010000000,

        /// <summary>
        /// QoS violation event bit.
        /// </summary>
        QosViolationEvent = 0x0000000020000000,

        /// <summary>
        /// Page scan mode change event bit.
        /// </summary>
        PageScanModeChangeEvent = 0x0000000040000000,

        /// <summary>
        /// Page scan repetition mode change event bit.
        /// </summary>
        PageScanRepetitionModeChangeEvent = 0x0000000080000000,

        /// <summary>
        /// Flow specification complete event bit.
        /// </summary>
        FlowSpecificationCompleteEvent = 0x0000000100000000,

        /// <summary>
        /// Inquiry result with RSSI event bit.
        /// </summary>
        InquiryResultWithRssiEvent = 0x0000000200000000,

        /// <summary>
        /// Read remote extended features complete event bit.
        /// </summary>
        ReadRemoteExtendedFeaturesCompleteEvent = 0x0000000400000000,

        /// <summary>
        /// Synchronous connection complete event bit.
        /// </summary>
        SynchronousConnectionCompleteEvent = 0x0000080000000000,

        /// <summary>
        /// Synchronous connection changed event bit.
        /// </summary>
        SynchronousConnectionChangedEvent = 0x0000100000000000,

        /// <summary>
        /// Sniff subrating event bit.
        /// </summary>
        SniffSubratingEvent = 0x0000200000000000,

        /// <summary>
        /// Extended inquiry result event bit.
        /// </summary>
        ExtendedInquiryResultEvent = 0x0000400000000000,

        /// <summary>
        /// Encryption key refresh complete event bit.
        /// </summary>
        EncryptionKeyRefreshCompleteEvent = 0x0000800000000000,

        /// <summary>
        /// IO capability request event bit.
        /// </summary>
        IoCapabilityRequestEvent = 0x0001000000000000,

        /// <summary>
        /// IO capability request reply event bit.
        /// </summary>
        IoCapabilityRequestReplyEvent = 0x0002000000000000,

        /// <summary>
        /// User confirmation request event bit.
        /// </summary>
        UserConfirmationRequestEvent = 0x0004000000000000,

        /// <summary>
        /// User passkey request event bit.
        /// </summary>
        UserPasskeyRequestEvent = 0x0008000000000000,

        /// <summary>
        /// Remote OOB data request event.
        /// </summary>
        RemoteOobDataRequestEvent = 0x0010000000000000,

        /// <summary>
        /// Simple pairing complete event bit.
        /// </summary>
        SimplePairingCompleteEvent = 0x0020000000000000,

        /// <summary>
        /// Link supervision timeout changed event bit.
        /// </summary>
        LinkSupervisionTimeoutChangedEvent = 0x0080000000000000,

        /// <summary>
        /// Enhanced flush complete event bit.
        /// </summary>
        EnhancedFlushCompleteEvent = 0x0100000000000000,

        /// <summary>
        /// User passkey notification event bit.
        /// </summary>
        UserPasskeyNotificationEvent = 0x0400000000000000,

        /// <summary>
        /// Keypress notification event bit.
        /// </summary>
        KeypressNotificationEvent = 0x0800000000000000,

        /// <summary>
        /// Remote host supported features notification event bit.
        /// </summary>
        RemoteHostSupportedFeaturesNotificationEvent = 0x1000000000000000,

        /// <summary>
        /// LE meta-event bit.
        /// </summary>
        LeMetaEvent = 0x2000000000000000,
    }
}
