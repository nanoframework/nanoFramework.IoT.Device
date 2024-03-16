// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Bitmap of lost events. Each bit indicates one or more
    /// occurrences of the specific event.
    /// </summary>
    [Flags]
    public enum LostEventBitmap : ulong
    {
        /// <summary>
        /// HciDisconnectionCompleteEvent.
        /// </summary>
        HciDisconnectionCompleteEvent = 0x0000000000000001,

        /// <summary>
        /// HciEncryptionChangeEvent.
        /// </summary>
        HciEncryptionChangeEvent = 0x0000000000000002,

        /// <summary>
        /// HciReadRemoteVersionInformationCompleteEvent.
        /// </summary>
        HciReadRemoteVersionInformationCompleteEvent = 0x0000000000000004,

        /// <summary>
        /// HciCommandCompleteEvent.
        /// </summary>
        HciCommandCompleteEvent = 0x0000000000000008,

        /// <summary>
        /// HciCommandStatusEvent.
        /// </summary>
        HciCommandStatusEvent = 0x0000000000000010,

        /// <summary>
        /// HciHardwareErrorEvent.
        /// </summary>
        HciHardwareErrorEvent = 0x0000000000000020,

        /// <summary>
        /// HciNumberOfCompletedPacketsEvent.
        /// </summary>
        HciNumberOfCompletedPacketsEvent = 0x0000000000000040,

        /// <summary>
        /// HciEncryptionKeyRefreshCompleteEvent.
        /// </summary>
        HciEncryptionKeyRefreshCompleteEvent = 0x0000000000000080,

        /// <summary>
        /// AciBlueInitializedEvent.
        /// </summary>
        AciBlueInitializedEvent = 0x0000000000000100,

        /// <summary>
        /// AciGapLimitedDiscoverableEvent.
        /// </summary>
        AciGapLimitedDiscoverableEvent = 0x0000000000000200,

        /// <summary>
        /// AciGapPairingCompleteEvent.
        /// </summary>
        AciGapPairingCompleteEvent = 0x0000000000000400,

        /// <summary>
        /// AciGapPassKeyReqEvent.
        /// </summary>
        AciGapPassKeyReqEvent = 0x0000000000000800,

        /// <summary>
        /// AciGapAuthorizationReqEvent.
        /// </summary>
        AciGapAuthorizationReqEvent = 0x0000000000001000,

        /// <summary>
        /// AciGapSlaveSecurityInitiatedEvent.
        /// </summary>
        AciGapSlaveSecurityInitiatedEvent = 0x0000000000002000,

        /// <summary>
        /// AciGapBondLostEvent.
        /// </summary>
        AciGapBondLostEvent = 0x0000000000004000,

        /// <summary>
        /// AciGapProcCompleteEvent.
        /// </summary>
        AciGapProcCompleteEvent = 0x0000000000008000,

        /// <summary>
        /// AciGapAddrNotResolvedEvent.
        /// </summary>
        AciGapAddrNotResolvedEvent = 0x0000000000010000,

        /// <summary>
        /// AciL2CapConnectionUpdateRespEvent.
        /// </summary>
        AciL2CapConnectionUpdateRespEvent = 0x0000000000020000,

        /// <summary>
        /// AciL2CapProcTimeoutEvent.
        /// </summary>
        AciL2CapProcTimeoutEvent = 0x0000000000040000,

        /// <summary>
        /// AciL2CapConnectionUpdateReqEvent.
        /// </summary>
        AciL2CapConnectionUpdateReqEvent = 0x0000000000080000,

        /// <summary>
        /// AciGattAttributeModifiedEvent.
        /// </summary>
        AciGattAttributeModifiedEvent = 0x0000000000100000,

        /// <summary>
        /// AciGattProcTimeoutEvent.
        /// </summary>
        AciGattProcTimeoutEvent = 0x0000000000200000,

        /// <summary>
        /// AciAttExchangeMtuRespEvent.
        /// </summary>
        AciAttExchangeMtuRespEvent = 0x0000000000400000,

        /// <summary>
        /// AciAttFindInfoRespEvent.
        /// </summary>
        AciAttFindInfoRespEvent = 0x0000000000800000,

        /// <summary>
        /// AciAttFindByTypeValueRespEvent.
        /// </summary>
        AciAttFindByTypeValueRespEvent = 0x0000000001000000,

        /// <summary>
        /// AciAttReadByTypeRespEvent.
        /// </summary>
        AciAttReadByTypeRespEvent = 0x0000000002000000,

        /// <summary>
        /// AciAttReadRespEvent.
        /// </summary>
        AciAttReadRespEvent = 0x0000000004000000,

        /// <summary>
        /// AciAttReadBlobRespEvent.
        /// </summary>
        AciAttReadBlobRespEvent = 0x0000000008000000,

        /// <summary>
        /// AciAttReadMultipleRespEvent.
        /// </summary>
        AciAttReadMultipleRespEvent = 0x0000000010000000,

        /// <summary>
        /// AciAttReadByGroupTypeRespEvent.
        /// </summary>
        AciAttReadByGroupTypeRespEvent = 0x0000000020000000,

        /// <summary>
        /// AciAttWriteRespEvent.
        /// </summary>
        AciAttWriteRespEvent = 0x0000000040000000,

        /// <summary>
        /// AciAttPrepareWriteRespEvent.
        /// </summary>
        AciAttPrepareWriteRespEvent = 0x0000000080000000,

        /// <summary>
        /// AciAttExecWriteRespEvent.
        /// </summary>
        AciAttExecWriteRespEvent = 0x0000000100000000,

        /// <summary>
        /// AciGattIndicationEvent.
        /// </summary>
        AciGattIndicationEvent = 0x0000000200000000,

        /// <summary>
        /// AciGattNotificationEvent.
        /// </summary>
        AciGattNotificationEvent = 0x0000000400000000,

        /// <summary>
        /// AciGattProcCompleteEvent.
        /// </summary>
        AciGattProcCompleteEvent = 0x0000000800000000,

        /// <summary>
        /// AciGattErrorRespEvent.
        /// </summary>
        AciGattErrorRespEvent = 0x0000001000000000,

        /// <summary>
        /// AciGattDiscReadCharByUuidRespEvent.
        /// </summary>
        AciGattDiscReadCharByUuidRespEvent = 0x0000002000000000,

        /// <summary>
        /// AciGattWritePermitReqEvent.
        /// </summary>
        AciGattWritePermitReqEvent = 0x0000004000000000,

        /// <summary>
        /// AciGattReadPermitReqEvent.
        /// </summary>
        AciGattReadPermitReqEvent = 0x0000008000000000,

        /// <summary>
        /// AciGattReadMultiPermitReqEvent.
        /// </summary>
        AciGattReadMultiPermitReqEvent = 0x0000010000000000,

        /// <summary>
        /// AciGattTxPoolAvailableEvent.
        /// </summary>
        AciGattTxPoolAvailableEvent = 0x0000020000000000,

        /// <summary>
        /// AciGattServerConfirmationEvent.
        /// </summary>
        AciGattServerConfirmationEvent = 0x0000040000000000,

        /// <summary>
        /// AciGattPrepareWritePermitReqEvent.
        /// </summary>
        AciGattPrepareWritePermitReqEvent = 0x0000080000000000,

        /// <summary>
        /// HciLeConnectionCompleteEvent.
        /// </summary>
        HciLeConnectionCompleteEvent = 0x0000100000000000,

        /// <summary>
        /// HciLeAdvertisingReportEvent.
        /// </summary>
        HciLeAdvertisingReportEvent = 0x0000200000000000,

        /// <summary>
        /// HciLeConnectionUpdateCompleteEvent.
        /// </summary>
        HciLeConnectionUpdateCompleteEvent = 0x0000400000000000,

        /// <summary>
        /// HciLeReadRemoteUsedFeaturesCompleteEvent.
        /// </summary>
        HciLeReadRemoteUsedFeaturesCompleteEvent = 0x0000800000000000,

        /// <summary>
        /// HciLeLongTermKeyRequestEvent.
        /// </summary>
        HciLeLongTermKeyRequestEvent = 0x0001000000000000,

        /// <summary>
        /// HciLeDataLengthChangeEvent.
        /// </summary>
        HciLeDataLengthChangeEvent = 0x0002000000000000,

        /// <summary>
        /// HciLeReadLocalP256PublicKeyCompleteEvent.
        /// </summary>
        HciLeReadLocalP256PublicKeyCompleteEvent = 0x0004000000000000,

        /// <summary>
        /// HciLeGenerateDhkeyCompleteEvent.
        /// </summary>
        HciLeGenerateDhkeyCompleteEvent = 0x0008000000000000,

        /// <summary>
        /// HciLeEnhancedConnectionCompleteEvent.
        /// </summary>
        HciLeEnhancedConnectionCompleteEvent = 0x0010000000000000,

        /// <summary>
        /// HciLeDirectAdvertisingReportEvent.
        /// </summary>
        HciLeDirectAdvertisingReportEvent = 0x0020000000000000,

        /// <summary>
        /// AciGapNumericComparisonValueEvent.
        /// </summary>
        AciGapNumericComparisonValueEvent = 0x0040000000000000,

        /// <summary>
        /// AciGapKeypressNotificationEvent.
        /// </summary>
        AciGapKeypressNotificationEvent = 0x0080000000000000,
    }
}
