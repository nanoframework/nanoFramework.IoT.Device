// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using BlueNrg2;

namespace Iot.Device.BlueNrg2
{
    public delegate BleStatus EventProcess(byte[] bufferIn);

    public class Events
    {
        public delegate void AdvertisingReport(byte reportCount, AdvertisingReport_t[] reports);

        public delegate void AttExchangeMtuResponse(ushort connectionHandle, ushort serverRxMtu);

        public delegate void AttExecuteWriteResponse(ushort connectionHandle);

        public delegate void AttFindByTypeValueResponse(
            ushort connectionHandle,
            byte numberOfHandlePairs,
            AttributeGroupHandlePair[] attributeGroupHandlePairs);

        public delegate void AttFindInfoResponse(ushort connectionHandle, byte format, byte eventDataLength, byte[] uuidPairHandle);

        public delegate void AttPrepareWriteResponse(
            ushort connectionHandle,
            ushort attributeHandle,
            ushort offset,
            byte partAttributeValueLength,
            byte[] partAttributeValue);

        public delegate void AttReadBlobResponse(ushort connectionHandle, byte eventDataLength, byte[] attributeValue);

        public delegate void AttReadByGroupTypeResponse(ushort connectionHandle, byte attributeDataLength, byte dataLength, byte[] attributeDataList);

        public delegate void AttReadByTypeResponse(ushort connectionHandle, byte handleValuePairLenght, byte dataLenght, byte[] handleValuePairData);

        public delegate void AttReadMultipleResponse(ushort connectionHandle, byte eventDataLength, byte[] setOfValues);

        public delegate void AttReadResponse(ushort connectionHandle, byte eventDataLenght, byte[] attributeValue);

        public delegate void BlueCrashInfo(
            CrashType crashType,
            ushort stackPointer,
            ushort register0,
            ushort register1,
            ushort register2,
            ushort register3,
            ushort register12,
            ushort linkRegister,
            ushort programCounter,
            ushort xPsrRegister,
            byte debugDataLength,
            byte[] debugData);

        public delegate void BlueEventsLost(byte[] lostEvents);

        public delegate void BlueInitialized(ReasonCode reasonCode);

        public delegate void ConnectionComplete(
            byte status,
            ushort connectionHandle,
            DeviceRole role,
            AddressType peerAddressType,
            byte[] peerAddress,
            ushort connectionInterval,
            ushort connectionLatency,
            ushort supervisionTimeout,
            ClockAccuracy masterClockAccuracy);

        public delegate void ConnectionUpdateComplete(
            byte status,
            ushort connectionHandle,
            ushort connectionInterval,
            ushort connectionLatency,
            ushort supervisionTimeout);

        public delegate void DataBufferOverflow(byte linkType);

        public delegate void DataLengthChange(ushort connectionHandle, ushort maxTxOctets, ushort maxTxTime, ushort maxRxOctets, ushort maxRxTime);

        public delegate void DirectAdvertisingReport(byte reportCount, DirectAdvertisingReport_t[] directAdvertisingReports);

        public delegate void DisconnectionComplete(byte status, ushort connectionHandle, byte reason);

        public delegate void EncryptionChange(byte status, ushort connectionHandle, byte encryptionEnabled);

        public delegate void EncryptionComplete(byte status, ushort connectionHandle);

        public delegate void EnhancedConnectionComplete(
            byte status,
            ushort connectionHandle,
            DeviceRole role,
            AddressType peerAddressType,
            byte[] peerAddress,
            byte[] localResolvablePrivateAddress,
            byte[] peerResolvablePrivateAddress,
            ushort connectionInterval,
            ushort supervisionTimeout,
            ClockAccuracy masterClockAccuracy);

        public delegate void GapAddressNotResolved(ushort connectionHandle);

        public delegate void GapAuthorizationRequest(ushort connectionHandle);

        public delegate void GapBondLost();

        public delegate void GapKeypressNotification(ushort connectionHandle, KeyPressNotificationType notificationType);

        public delegate void GapLimitedDiscoverable();

        public delegate void GapNumericComparisonValue(ushort connectionHandle, uint numericValue);

        public delegate void GapPairingComplete(ushort connectionHandle, GapPairingCompleteStatus status, GapPairingCompleteReason reason);

        public delegate void GapPassKeyRequest(ushort connectionHandle);

        public delegate void GapProcessComplete(ProcedureCode procedureCodes, byte status, byte dataLength, byte[] data);

        public delegate void GapSlaveSecurityInitiated();

        public delegate void GattAttributeModified(
            ushort connectionHandle,
            ushort attributeHandle,
            ushort offset,
            ushort attributeDataLength,
            byte[] attributeData);

        public delegate void GattDiscoverReadCharacteristicByUuidResponse(
            ushort connectionHandle,
            ushort attributeHandle,
            byte attributeValueLength,
            byte[] attributeValue);

        public delegate void GattErrorResponse(
            ushort connectionHandle,
            byte requestOpcode,
            ushort attributeHandle,
            ErrorResponseErrorCode errorCode);

        public delegate void GattIndication(ushort connectionHandle, ushort attributeHandle, byte attributeValueLength, byte[] attributeValue);

        public delegate void GattNotification(ushort connectionHandle, ushort attributeHandle, byte attributeValueLength, byte[] attributeValue);

        public delegate void GattPrepareWritePermitRequest(
            ushort connectionHandle,
            ushort attributeHandle,
            ushort offset,
            byte dataLength,
            byte[] data);

        public delegate void GattProcessComplete(ushort connectionHandle, NotificationErrorCode errorCode);

        public delegate void GattProcessTimeout(ushort connectionHandle);

        public delegate void GattReadMultiPermitRequest(ushort connectionHandle, byte numberOfHandles, ushort[] handles);

        public delegate void GattReadPermitRequest(ushort connectionHandle, ushort attributeHandle, ushort offset);

        public delegate void GattServerConfirmation(ushort connectionHandle);

        public delegate void GattTxPoolAvailable(ushort connectionHandle, ushort availableBuffers);

        public delegate void GattWritePermitRequest(ushort connectionHandle, ushort attributeHandle, byte dataLength, byte[] data);

        public delegate void GenerateDhKeyComplete(byte status, byte[] dhKey);

        public delegate void HalEndOfRadioActivity(RadioState lastState, RadioState nextState, uint nextStateSysTime);

        public delegate void HalFirmwareError(FirmwareErrorType errorType, byte dataLength, byte[] data);

        public delegate void HalScanRequestReport(sbyte rssi, AddressType peerAddressType, byte[] peerAddress);

        public delegate void HardwareError(byte hardwareCode);

        public delegate void L2CapCommandReject(ushort connectionHandle, byte identifier, ushort reason, byte dataLength, byte[] data);

        public delegate void L2CapConnectionUpdateRequest(
            ushort connectionHandle,
            byte identifier,
            ushort l2CapLength,
            ushort intervalMin,
            ushort intervalMax,
            ushort slaveLatency,
            ushort timeoutMultiplier);

        public delegate void L2CapConnectionUpdateResponse(ushort connectionHandle, ushort result);

        public delegate void L2CapProcessTimeout(ushort connectionHandle, byte dataLength, byte[] data);

        public delegate void LongTermKeyRequest(ushort connectionHandle, byte[] randomNumber, ushort encryptedDiversifier);

        public delegate void NumberOfCompletePackets(byte numberOfHandles, HandlePacketPair[] handlePacketPairs);

        public delegate void ReadLocalP256PublicKeyComplete(byte status, byte[] localP356PublicKey);

        public delegate void ReadRemoteUsedFeaturesComplete(byte status, ushort connectionHandle, byte[] features);

        public delegate void ReadRemoteVersionInformationComplete(
            byte status,
            ushort connectionHandle,
            byte version,
            ushort manufacturerName,
            ushort subversion);

        internal readonly EventTableType[] LeMetaEventsTable;
        internal readonly EventTableType[] VendorSpecificEventsTable;
        internal EventTableType[] EventsTable;

        public Events()
        {
            EventsTable = new[]
            {
                /* hci_disconnection_complete_event */
                new EventTableType(0x0005, hci_disconnection_complete_event_process),
                /* hci_encryption_change_event */
                new(0x0008, hci_encryption_change_event_process),
                /* hci_read_remote_version_information_complete_event */
                new(0x000c, hci_read_remote_version_information_complete_event_process),
                /* hci_hardware_error_event */
                new(0x0010, hci_hardware_error_event_process),
                /* hci_number_of_completed_packets_event */
                new(0x0013, hci_number_of_completed_packets_event_process),
                /* hci_data_buffer_overflow_event */
                new(0x001a, hci_data_buffer_overflow_event_process),
                /* hci_encryption_key_refresh_complete_event */
                new(0x0030, hci_encryption_key_refresh_complete_event_process)
            };

            LeMetaEventsTable = new[]
            {
                /* hci_le_connection_complete_event */
                new EventTableType(0x0001, hci_le_connection_complete_event_process),
                /* hci_le_advertising_report_event */
                new(0x0002, hci_le_advertising_report_event_process),
                /* hci_le_connection_update_complete_event */
                new(0x0003, hci_le_connection_update_complete_event_process),
                /* hci_le_read_remote_used_features_complete_event */
                new(0x0004, hci_le_read_remote_used_features_complete_event_process),
                /* hci_le_long_term_key_request_event */
                new(0x0005, hci_le_long_term_key_request_event_process),
                /* hci_le_data_length_change_event */
                new(0x0007, hci_le_data_length_change_event_process),
                /* hci_le_read_local_p256_public_key_complete_event */
                new(0x0008, hci_le_read_local_p256_public_key_complete_event_process),
                /* hci_le_generate_dhkey_complete_event */
                new(0x0009, hci_le_generate_dhkey_complete_event_process),
                /* hci_le_enhanced_connection_complete_event */
                new(0x000a, hci_le_enhanced_connection_complete_event_process),
                /* hci_le_direct_advertising_report_event */
                new(0x000b, hci_le_direct_advertising_report_event_process)
            };

            VendorSpecificEventsTable = new[]
            {
                /* aci_blue_initialized_event */
                new EventTableType(0x0001, aci_blue_initialized_event_process),
                /* aci_blue_events_lost_event */
                new(0x0002, aci_blue_events_lost_event_process),
                /* aci_blue_crash_info_event */
                new(0x0003, aci_blue_crash_info_event_process),
                /* aci_hal_end_of_radio_activity_event */
                new(0x0004, aci_hal_end_of_radio_activity_event_process),
                /* aci_hal_scan_req_report_event */
                new(0x0005, aci_hal_scan_req_report_event_process),
                /* aci_hal_fw_error_event */
                new(0x0006, aci_hal_fw_error_event_process),
                /* aci_gap_limited_discoverable_event */
                new(0x0400, aci_gap_limited_discoverable_event_process),
                /* aci_gap_pairing_complete_event */
                new(0x0401, aci_gap_pairing_complete_event_process),
                /* aci_gap_pass_key_req_event */
                new(0x0402, aci_gap_pass_key_req_event_process),
                /* aci_gap_authorization_req_event */
                new(0x0403, aci_gap_authorization_req_event_process),
                /* aci_gap_slave_security_initiated_event */
                new(0x0404, aci_gap_slave_security_initiated_event_process),
                /* aci_gap_bond_lost_event */
                new(0x0405, aci_gap_bond_lost_event_process),
                /* aci_gap_proc_complete_event */
                new(0x0407, aci_gap_proc_complete_event_process),
                /* aci_gap_addr_not_resolved_event */
                new(0x0408, aci_gap_addr_not_resolved_event_process),
                /* aci_gap_numeric_comparison_value_event */
                new(0x0409, aci_gap_numeric_comparison_value_event_process),
                /* aci_gap_keypress_notification_event */
                new(0x040a, aci_gap_keypress_notification_event_process),
                /* aci_l2cap_connection_update_resp_event */
                new(0x0800, aci_l2cap_connection_update_resp_event_process),
                /* aci_l2cap_proc_timeout_event */
                new(0x0801, aci_l2cap_proc_timeout_event_process),
                /* aci_l2cap_connection_update_req_event */
                new(0x0802, aci_l2cap_connection_update_req_event_process),
                /* aci_l2cap_command_reject_event */
                new(0x080a, aci_l2cap_command_reject_event_process),
                /* aci_gatt_attribute_modified_event */
                new(0x0c01, aci_gatt_attribute_modified_event_process),
                /* aci_gatt_proc_timeout_event */
                new(0x0c02, aci_gatt_proc_timeout_event_process),
                /* aci_att_exchange_mtu_resp_event */
                new(0x0c03, aci_att_exchange_mtu_resp_event_process),
                /* aci_att_find_info_resp_event */
                new(0x0c04, aci_att_find_info_resp_event_process),
                /* aci_att_find_by_type_value_resp_event */
                new(0x0c05, aci_att_find_by_type_value_resp_event_process),
                /* aci_att_read_by_type_resp_event */
                new(0x0c06, aci_att_read_by_type_resp_event_process),
                /* aci_att_read_resp_event */
                new(0x0c07, aci_att_read_resp_event_process),
                /* aci_att_read_blob_resp_event */
                new(0x0c08, aci_att_read_blob_resp_event_process),
                /* aci_att_read_multiple_resp_event */
                new(0x0c09, aci_att_read_multiple_resp_event_process),
                /* aci_att_read_by_group_type_resp_event */
                new(0x0c0a, aci_att_read_by_group_type_resp_event_process),
                /* aci_att_prepare_write_resp_event */
                new(0x0c0c, aci_att_prepare_write_resp_event_process),
                /* aci_att_exec_write_resp_event */
                new(0x0c0d, aci_att_exec_write_resp_event_process),
                /* aci_gatt_indication_event */
                new(0x0c0e, aci_gatt_indication_event_process),
                /* aci_gatt_notification_event */
                new(0x0c0f, aci_gatt_notification_event_process),
                /* aci_gatt_proc_complete_event */
                new(0x0c10, aci_gatt_proc_complete_event_process),
                /* aci_gatt_error_resp_event */
                new(0x0c11, aci_gatt_error_resp_event_process),
                /* aci_gatt_disc_read_char_by_uuid_resp_event */
                new(0x0c12, aci_gatt_disc_read_char_by_uuid_resp_event_process),
                /* aci_gatt_write_permit_req_event */
                new(0x0c13, aci_gatt_write_permit_req_event_process),
                /* aci_gatt_read_permit_req_event */
                new(0x0c14, aci_gatt_read_permit_req_event_process),
                /* aci_gatt_read_multi_permit_req_event */
                new(0x0c15, aci_gatt_read_multi_permit_req_event_process),
                /* aci_gatt_tx_pool_available_event */
                new(0x0c16, aci_gatt_tx_pool_available_event_process),
                /* aci_gatt_server_confirmation_event */
                new(0x0c17, aci_gatt_server_confirmation_event_process)
            };
        }

        public event AttReadResponse AttReadResponseEvent;

        public event BlueCrashInfo BlueCrashInfoEvent;

        public event BlueEventsLost BlueEventsLostEvent;

        public event BlueInitialized BlueInitializedEvent;

        public event ConnectionComplete ConnectionCompleteEvent;

        public event ConnectionUpdateComplete ConnectionUpdateCompleteEvent;

        public event DataLengthChange DataLengthChangeEvent;

        public event DirectAdvertisingReport DirectAdvertisingReportEvent;

        public event EncryptionChange EncryptionChangeEvent;

        public event EnhancedConnectionComplete EnhancedConnectionCompleteEvent;

        public event GapAddressNotResolved GapAddressNotResolvedEvent;

        public event GapAuthorizationRequest GapAuthorizationRequestEvent;

        public event GapBondLost GapBondLostEvent;

        public event GapKeypressNotification GapKeypressNotificationEvent;

        public event GapLimitedDiscoverable GapLimitedDiscoverableEvent;

        public event GapNumericComparisonValue GapNumericComparisonValueEvent;

        public event GapPairingComplete GapPairingCompleteEvent;

        public event GapPassKeyRequest GapPassKeyRequestEvent;

        public event GapProcessComplete GapProcessCompleteEvent;

        public event GapSlaveSecurityInitiated GapSlaveSecurityInitiatedEvent;

        public event GattAttributeModified GattAttributeModifiedEvent;

        public event GattDiscoverReadCharacteristicByUuidResponse GattDiscoverReadCharacteristicByUuidResponseEvent;

        public event AttReadByGroupTypeResponse AttReadByGroupTypeResponseEvent;
        public event GattErrorResponse GattErrorResponseEvent;

        public event GattIndication GattIndicationEvent;

        public event GattNotification GattNotificationEvent;

        public event GattProcessComplete GattProcessCompleteEvent;

        public event GattProcessTimeout GattProcessTimeoutEvent;

        public event GattReadMultiPermitRequest GattReadMultiPermitRequestEvent;

        public event GattReadPermitRequest GattReadPermitRequestEvent;

        public event GattServerConfirmation GattServerConfirmationEvent;

        public event GattTxPoolAvailable GattTxPoolAvailableEvent;

        public event GattWritePermitRequest GattWritePermitRequestEvent;

        public event GenerateDhKeyComplete GenerateDhKeyCompleteEvent;

        public event HalEndOfRadioActivity HalEndOfRadioActivityEvent;

        public event HalFirmwareError HalFirmwareErrorEvent;

        public event HalScanRequestReport HalScanRequestReportEvent;
        public event NumberOfCompletePackets NumberOfCompletePacketsEvent;

        public event L2CapCommandReject L2CapCommandRejectEvent;

        public event L2CapConnectionUpdateRequest L2CapConnectionUpdateRequestEvent;

        public event L2CapConnectionUpdateResponse L2CapConnectionUpdateResponseEvent;

        public event L2CapProcessTimeout L2CapProcessTimeoutEvent;

        public event LongTermKeyRequest LongTermKeyRequestEvent;

        public event ReadLocalP256PublicKeyComplete ReadLocalP256PublicKeyCompleteEvent;

        public event ReadRemoteUsedFeaturesComplete ReadRemoteUsedFeaturesCompleteEvent;

        public event ReadRemoteVersionInformationComplete ReadRemoteVersionInformationCompleteEvent;

        public event AdvertisingReport AdvertisingReportEvent;

        public event AttExchangeMtuResponse AttExchangeMtuResponseEvent;

        public event DisconnectionComplete DisconnectionCompleteEvent;
        public event AttExecuteWriteResponse AttExecuteWriteResponseEvent;
        public event AttFindByTypeValueResponse AttFindByTypeValueResponseEvent;
        public event AttFindInfoResponse AttFindInfoResponseEvent;
        public event AttPrepareWriteResponse AttPrepareWriteResponseEvent;
        public event AttReadBlobResponse AttReadBlobResponseEvent;
        public event AttReadByTypeResponse AttReadByTypeResponseEvent;
        public event AttReadMultipleResponse AttReadMultipleResponseEvent;
        public event EncryptionComplete EncryptionCompleteEvent;

        public event DataBufferOverflow DataBufferOverflowEvent;
        public event HardwareError HardwareErrorEvent;

        private BleStatus hci_encryption_key_refresh_complete_event_process(byte[] bufferIn)
        {
            if (EncryptionCompleteEvent is not null)
                EncryptionCompleteEvent.Invoke(bufferIn![0], BitConverter.ToUInt16(bufferIn, 1));

            return BleStatus.Success;
        }

        private BleStatus hci_data_buffer_overflow_event_process(byte[] bufferIn)
        {
            if (DataBufferOverflowEvent is not null)
                DataBufferOverflowEvent.Invoke(bufferIn![0]);
            return BleStatus.Success;
        }

        private BleStatus hci_number_of_completed_packets_event_process(byte[] bufferIn)
        {
            var numberOfHandles = bufferIn![0];
            var handlePacketPairs = new HandlePacketPair[32];
            byte ptr = 1;
            for (var i = 0; i < numberOfHandles; i++)
            {
                handlePacketPairs[i].ConnectionHandle = BitConverter.ToUInt16(bufferIn, ptr);
                ptr += 2;
                handlePacketPairs[i].NumberOfCompletedPackets = BitConverter.ToUInt16(bufferIn, ptr);
                ptr += 2;
            }

            if (NumberOfCompletePacketsEvent is not null)
                NumberOfCompletePacketsEvent.Invoke(numberOfHandles, handlePacketPairs);

            return BleStatus.Success;
        }

        private BleStatus hci_hardware_error_event_process(byte[] bufferIn)
        {
            if (HardwareErrorEvent is not null)
                HardwareErrorEvent.Invoke(bufferIn![0]);
            return BleStatus.Success;
        }

        private BleStatus hci_read_remote_version_information_complete_event_process(byte[] bufferIn)
        {
            if (ReadRemoteVersionInformationCompleteEvent is not null)
                ReadRemoteVersionInformationCompleteEvent.Invoke(
                    bufferIn![0],
                    BitConverter.ToUInt16(bufferIn, 1),
                    bufferIn[3],
                    BitConverter.ToUInt16(bufferIn, 4),
                    BitConverter.ToUInt16(bufferIn, 5)
                );
            return BleStatus.Success;
        }

        private BleStatus hci_encryption_change_event_process(byte[] bufferIn)
        {
            if (EncryptionChangeEvent is not null)
                EncryptionChangeEvent.Invoke(bufferIn![0], BitConverter.ToUInt16(bufferIn, 1), bufferIn[3]);
            return BleStatus.Success;
        }

        private BleStatus hci_le_direct_advertising_report_event_process(byte[] bufferIn)
        {
            var reportCount = bufferIn![0];
            byte ptr = 1;
            var directAdvertisingReports = new DirectAdvertisingReport_t[8];
            for (var i = 0; i < reportCount; i++)
            {
                directAdvertisingReports[i].EventType = (AdvertisingType)bufferIn[ptr];
                ptr += 1;
                directAdvertisingReports[i].AddressType = (AddressType)bufferIn[ptr];
                ptr += 1;
                Array.Copy(bufferIn, ptr, directAdvertisingReports[i].Address, 0, 6);
                ptr += 6;
                directAdvertisingReports[i].DirectAddressType = (AddressType)bufferIn[ptr];
                ptr += 1;
                Array.Copy(bufferIn, ptr, directAdvertisingReports[i].DirectAddress, 0, 6);
                ptr += 6;
                directAdvertisingReports[i].RSSI = (sbyte)bufferIn[ptr];
                ptr += 1;
            }

            if (DirectAdvertisingReportEvent is not null)
                DirectAdvertisingReportEvent.Invoke(reportCount, directAdvertisingReports);

            return BleStatus.Success;
        }

        private BleStatus hci_le_enhanced_connection_complete_event_process(byte[] bufferIn)
        {
            byte[] peerAddress = new byte[6], localResolvablePrivateAddress = new byte[6], peerResolvablePrivateAddress = new byte[6];
            Array.Copy(bufferIn!, 5, peerAddress, 0, 6);
            Array.Copy(bufferIn!, 11, localResolvablePrivateAddress, 0, 6);
            Array.Copy(bufferIn!, 17, peerResolvablePrivateAddress, 0, 6);

            if (EnhancedConnectionCompleteEvent is not null)
                EnhancedConnectionCompleteEvent.Invoke(
                    bufferIn[0],
                    BitConverter.ToUInt16(bufferIn, 1),
                    (DeviceRole)bufferIn[3],
                    (AddressType)bufferIn[4],
                    peerAddress,
                    localResolvablePrivateAddress,
                    peerResolvablePrivateAddress,
                    BitConverter.ToUInt16(bufferIn, 23),
                    BitConverter.ToUInt16(bufferIn, 25),
                    (ClockAccuracy)bufferIn[26]
                );
            return BleStatus.Success;
        }

        private BleStatus hci_le_generate_dhkey_complete_event_process(byte[] bufferIn)
        {
            var dhKey = new byte[32];
            Array.Copy(bufferIn!, 1, dhKey, 0, 32);
            if (GenerateDhKeyCompleteEvent is not null)
                GenerateDhKeyCompleteEvent.Invoke(bufferIn[0], dhKey);
            return BleStatus.Success;
        }

        private BleStatus hci_le_read_local_p256_public_key_complete_event_process(byte[] bufferIn)
        {
            var localP256PublicKey = new byte[64];
            Array.Copy(bufferIn!, 1, localP256PublicKey, 0, 64);
            if (ReadLocalP256PublicKeyCompleteEvent is not null)
                ReadLocalP256PublicKeyCompleteEvent.Invoke(bufferIn[0], localP256PublicKey);
            return BleStatus.Success;
        }

        private BleStatus hci_le_data_length_change_event_process(byte[] bufferIn)
        {
            if (DataLengthChangeEvent is not null)
                DataLengthChangeEvent.Invoke(
                    BitConverter.ToUInt16(bufferIn!, 0),
                    BitConverter.ToUInt16(bufferIn, 2),
                    BitConverter.ToUInt16(bufferIn, 4),
                    BitConverter.ToUInt16(bufferIn, 6),
                    BitConverter.ToUInt16(bufferIn, 8)
                );
            return BleStatus.Success;
        }

        private BleStatus hci_le_long_term_key_request_event_process(byte[] bufferIn)
        {
            var randomNumber = new byte[8];
            Array.Copy(bufferIn!, 2, randomNumber, 0, 8);
            if (LongTermKeyRequestEvent is not null)
                LongTermKeyRequestEvent.Invoke(BitConverter.ToUInt16(bufferIn, 0), randomNumber, BitConverter.ToUInt16(bufferIn, 10));
            return BleStatus.Success;
        }

        private BleStatus hci_le_connection_update_complete_event_process(byte[] bufferIn)
        {
            if (ConnectionUpdateCompleteEvent is not null)
                ConnectionUpdateCompleteEvent.Invoke(
                    bufferIn![0],
                    BitConverter.ToUInt16(bufferIn, 1),
                    BitConverter.ToUInt16(bufferIn, 3),
                    BitConverter.ToUInt16(bufferIn, 5),
                    BitConverter.ToUInt16(bufferIn, 7)
                );
            return BleStatus.Success;
        }

        private BleStatus hci_le_read_remote_used_features_complete_event_process(byte[] bufferIn)
        {
            var features = new byte[8];
            Array.Copy(bufferIn!, 3, features, 0, 8);
            if (ReadRemoteUsedFeaturesCompleteEvent is not null)
                ReadRemoteUsedFeaturesCompleteEvent.Invoke(bufferIn[0], BitConverter.ToUInt16(bufferIn, 1), features);
            return BleStatus.Success;
        }

        private BleStatus hci_le_advertising_report_event_process(byte[] bufferIn)
        {
            var reportCount = bufferIn![0];
            var reports = new AdvertisingReport_t[9];
            var ptr = 1;
            for (var i = 0; i < reportCount; i++)
            {
                reports[i].EventType = (AdvertisingType)bufferIn[ptr];
                ptr += 1;
                reports[i].AddressType = (AddressType)bufferIn[ptr];
                ptr += 1;
                Array.Copy(bufferIn, ptr, reports[i].Address, 0, 6);
                ptr += 6;
                reports[i].DataLength = bufferIn[ptr];
                ptr += 1;
                Array.Copy(bufferIn, ptr, reports[i].Data, 0, reports[i].DataLength);
                ptr += reports[i].DataLength;
                reports[i].Rssi = (sbyte)bufferIn[ptr];
                ptr += 1;
            }

            if (AdvertisingReportEvent is not null)
                AdvertisingReportEvent.Invoke(reportCount, reports);

            return BleStatus.Success;
        }

        private BleStatus hci_le_connection_complete_event_process(byte[] bufferIn)
        {
            var peerAddress = new byte[6];
            Array.Copy(bufferIn!, 5, peerAddress, 0, 6);

            if (ConnectionCompleteEvent is not null)
                ConnectionCompleteEvent.Invoke(
                    bufferIn[0],
                    BitConverter.ToUInt16(bufferIn, 1),
                    (DeviceRole)bufferIn[3],
                    (AddressType)bufferIn[4],
                    peerAddress,
                    BitConverter.ToUInt16(bufferIn, 11),
                    BitConverter.ToUInt16(bufferIn, 13),
                    BitConverter.ToUInt16(bufferIn, 15),
                    (ClockAccuracy)bufferIn[16]
                );

            return BleStatus.Success;
        }

        private BleStatus aci_gap_bond_lost_event_process(byte[] bufferIn)
        {
            if (GapBondLostEvent is not null)
                GapBondLostEvent.Invoke();
            return BleStatus.Success;
        }

        private BleStatus aci_gap_slave_security_initiated_event_process(byte[] bufferIn)
        {
            if (GapSlaveSecurityInitiatedEvent is not null)
                GapSlaveSecurityInitiatedEvent.Invoke();
            return BleStatus.Success;
        }

        private BleStatus aci_gap_authorization_req_event_process(byte[] bufferIn)
        {
            if (GapAuthorizationRequestEvent is not null)
                GapAuthorizationRequestEvent.Invoke(BitConverter.ToUInt16(bufferIn!, 0));
            return BleStatus.Success;
        }

        private BleStatus aci_gap_pass_key_req_event_process(byte[] bufferIn)
        {
            if (GapPassKeyRequestEvent is not null)
                GapPassKeyRequestEvent.Invoke(BitConverter.ToUInt16(bufferIn!, 0));
            return BleStatus.Success;
        }

        private BleStatus aci_gap_limited_discoverable_event_process(byte[] bufferIn)
        {
            if (GapLimitedDiscoverableEvent is not null)
                GapLimitedDiscoverableEvent.Invoke();
            return BleStatus.Success;
        }

        private BleStatus aci_hal_fw_error_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![1];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 2, data, 0, dataLength);
            if (HalFirmwareErrorEvent is not null)
                HalFirmwareErrorEvent.Invoke((FirmwareErrorType)bufferIn[0], dataLength, data);
            return BleStatus.Success;
        }

        private BleStatus aci_hal_scan_req_report_event_process(byte[] bufferIn)
        {
            var peerAddress = new byte[6];
            Array.Copy(bufferIn!, 2, peerAddress, 0, 6);
            if (HalScanRequestReportEvent is not null)
                HalScanRequestReportEvent.Invoke((sbyte)bufferIn[0], (AddressType)bufferIn[1], peerAddress);
            return BleStatus.Success;
        }

        private BleStatus aci_hal_end_of_radio_activity_event_process(byte[] bufferIn)
        {
            if (HalEndOfRadioActivityEvent is not null)
                HalEndOfRadioActivityEvent.Invoke((RadioState)bufferIn![0], (RadioState)bufferIn[1], BitConverter.ToUInt32(bufferIn, 2));
            return BleStatus.Success;
        }

        private BleStatus aci_blue_crash_info_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![18];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 19, data, 0, dataLength);
            if (BlueCrashInfoEvent is not null)
                BlueCrashInfoEvent.Invoke(
                    (CrashType)bufferIn![0],
                    BitConverter.ToUInt16(bufferIn, 1),
                    BitConverter.ToUInt16(bufferIn, 3),
                    BitConverter.ToUInt16(bufferIn, 5),
                    BitConverter.ToUInt16(bufferIn, 7),
                    BitConverter.ToUInt16(bufferIn, 9),
                    BitConverter.ToUInt16(bufferIn, 11),
                    BitConverter.ToUInt16(bufferIn, 13),
                    BitConverter.ToUInt16(bufferIn, 15),
                    BitConverter.ToUInt16(bufferIn, 17),
                    dataLength,
                    data
                );
            return BleStatus.Success;
        }

        private BleStatus aci_gap_pairing_complete_event_process(byte[] bufferIn)
        {
            if (GapPairingCompleteEvent is not null)
                GapPairingCompleteEvent.Invoke(
                    BitConverter.ToUInt16(bufferIn!, 0),
                    (GapPairingCompleteStatus)bufferIn[2],
                    (GapPairingCompleteReason)bufferIn[3]
                );
            return BleStatus.Success;
        }

        private BleStatus aci_gap_keypress_notification_event_process(byte[] bufferIn)
        {
            if (GapKeypressNotificationEvent is not null)
                GapKeypressNotificationEvent.Invoke(BitConverter.ToUInt16(bufferIn!, 0), (KeyPressNotificationType)bufferIn[2]);
            return BleStatus.Success;
        }

        private BleStatus aci_gap_numeric_comparison_value_event_process(byte[] bufferIn)
        {
            if (GapNumericComparisonValueEvent is not null)
                GapNumericComparisonValueEvent.Invoke(BitConverter.ToUInt16(bufferIn!, 0), BitConverter.ToUInt32(bufferIn, 2));
            return BleStatus.Success;
        }

        private BleStatus aci_l2cap_proc_timeout_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![2];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 3, data, 0, dataLength);
            if (L2CapProcessTimeoutEvent is not null)
                L2CapProcessTimeoutEvent.Invoke(BitConverter.ToUInt16(bufferIn!, 0), dataLength, data);
            return BleStatus.Success;
        }

        private BleStatus aci_l2cap_connection_update_req_event_process(byte[] bufferIn)
        {
            if (L2CapConnectionUpdateRequestEvent is not null)
                L2CapConnectionUpdateRequestEvent.Invoke(
                    BitConverter.ToUInt16(bufferIn!, 0),
                    bufferIn[2],
                    BitConverter.ToUInt16(bufferIn, 3),
                    BitConverter.ToUInt16(bufferIn, 5),
                    BitConverter.ToUInt16(bufferIn, 7),
                    BitConverter.ToUInt16(bufferIn, 9),
                    BitConverter.ToUInt16(bufferIn, 11)
                );
            return BleStatus.Success;
        }

        private BleStatus aci_l2cap_connection_update_resp_event_process(byte[] bufferIn)
        {
            if (L2CapConnectionUpdateResponseEvent is not null)
                L2CapConnectionUpdateResponseEvent.Invoke(BitConverter.ToUInt16(bufferIn!, 0), BitConverter.ToUInt16(bufferIn, 2));
            return BleStatus.Success;
        }

        private BleStatus aci_gap_addr_not_resolved_event_process(byte[] bufferIn)
        {
            if (GapAddressNotResolvedEvent is not null)
                GapAddressNotResolvedEvent.Invoke(BitConverter.ToUInt16(bufferIn!, 0));
            return BleStatus.Success;
        }

        private BleStatus aci_gatt_attribute_modified_event_process(byte[] bufferIn)
        {
            var attributeLength = bufferIn![6];
            var attributeData = new byte[attributeLength];
            Array.Copy(bufferIn, 7, attributeData, 0, attributeLength);
            if (GattAttributeModifiedEvent is not null)
                GattAttributeModifiedEvent.Invoke(
                    BitConverter.ToUInt16(bufferIn!, 0),
                    BitConverter.ToUInt16(bufferIn, 2),
                    BitConverter.ToUInt16(bufferIn, 4),
                    attributeLength,
                    attributeData
                );
            return BleStatus.Success;
        }

        private BleStatus aci_l2cap_command_reject_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![5];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 6, data, 0, dataLength);
            if (L2CapCommandRejectEvent is not null)
                L2CapCommandRejectEvent.Invoke(BitConverter.ToUInt16(bufferIn!, 0), bufferIn[2], BitConverter.ToUInt16(bufferIn, 3), dataLength, data);
            return BleStatus.Success;
        }

        private BleStatus aci_gatt_proc_timeout_event_process(byte[] bufferIn)
        {
            if (GattProcessTimeoutEvent is not null)
                GattProcessTimeoutEvent.Invoke(BitConverter.ToUInt16(bufferIn!, 0));
            return BleStatus.Success;
        }

        private BleStatus aci_att_exchange_mtu_resp_event_process(byte[] bufferIn)
        {
            if (AttExchangeMtuResponseEvent is not null)
                AttExchangeMtuResponseEvent.Invoke(BitConverter.ToUInt16(bufferIn!, 0), BitConverter.ToUInt16(bufferIn, 2));
            return BleStatus.Success;
        }

        private BleStatus aci_gap_proc_complete_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![2];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 3, data, 0, dataLength);
            if (GapProcessCompleteEvent is not null)
                GapProcessCompleteEvent.Invoke((ProcedureCode)bufferIn[0], bufferIn[1], dataLength, data);
            return BleStatus.Success;
        }

        private BleStatus aci_att_read_by_type_resp_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![3];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 4, data, 0, dataLength);
            if (AttReadByTypeResponseEvent is not null)
                AttReadByTypeResponseEvent.Invoke(
                    BitConverter.ToUInt16(bufferIn, 0),
                    bufferIn[2],
                    dataLength,
                    data
                );
            return BleStatus.Success;
        }

        private BleStatus aci_att_find_by_type_value_resp_event_process(byte[] bufferIn)
        {
            var handlePairCount = bufferIn![2];
            var handlePairs = new AttributeGroupHandlePair[32];
            var ptr = 3;
            for (var i = 0; i < handlePairCount; i++)
            {
                handlePairs[i].FoundAttributeHandle = BitConverter.ToUInt16(bufferIn, ptr);
                ptr += 2;
                handlePairs[i].GroupEndHandle = BitConverter.ToUInt16(bufferIn, ptr);
                ptr += 2;
            }

            if (AttFindByTypeValueResponseEvent is not null)
                AttFindByTypeValueResponseEvent.Invoke(BitConverter.ToUInt16(bufferIn, 0), handlePairCount, handlePairs);
            return BleStatus.Success;
        }

        private BleStatus aci_att_find_info_resp_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![3];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 4, data, 0, dataLength);
            if (AttFindInfoResponseEvent is not null)
                AttFindInfoResponseEvent.Invoke(
                    BitConverter.ToUInt16(bufferIn, 0),
                    bufferIn[2],
                    dataLength,
                    data
                );
            return BleStatus.Success;
        }

        private BleStatus aci_att_read_resp_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![2];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 3, data, 0, dataLength);
            if (AttReadResponseEvent is not null)
                AttReadResponseEvent.Invoke(
                    BitConverter.ToUInt16(bufferIn, 0),
                    dataLength,
                    data
                );
            return BleStatus.Success;
        }

        private BleStatus aci_att_read_blob_resp_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![2];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 3, data, 0, dataLength);
            if (AttReadBlobResponseEvent is not null)
                AttReadBlobResponseEvent.Invoke(
                    BitConverter.ToUInt16(bufferIn, 0),
                    dataLength,
                    data
                );
            return BleStatus.Success;
        }

        private BleStatus aci_att_read_multiple_resp_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![2];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 3, data, 0, dataLength);
            if (AttReadMultipleResponseEvent is not null)
                AttReadMultipleResponseEvent.Invoke(
                    BitConverter.ToUInt16(bufferIn, 0),
                    dataLength,
                    data
                );
            return BleStatus.Success;
        }

        private BleStatus aci_att_read_by_group_type_resp_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![3];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 4, data, 0, dataLength);
            if (AttReadByGroupTypeResponseEvent is not null)
                AttReadByGroupTypeResponseEvent.Invoke(
                    BitConverter.ToUInt16(bufferIn, 0),
                    bufferIn[2],
                    dataLength,
                    data
                );
            return BleStatus.Success;
        }

        private BleStatus aci_att_prepare_write_resp_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![6];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 7, data, 0, dataLength);
            if (AttPrepareWriteResponseEvent is not null)
                AttPrepareWriteResponseEvent.Invoke(
                    BitConverter.ToUInt16(bufferIn, 0),
                    BitConverter.ToUInt16(bufferIn, 2),
                    BitConverter.ToUInt16(bufferIn, 4),
                    dataLength,
                    data
                );
            return BleStatus.Success;
        }

        private BleStatus aci_att_exec_write_resp_event_process(byte[] bufferIn)
        {
            if (AttExecuteWriteResponseEvent is not null)
                AttExecuteWriteResponseEvent.Invoke(BitConverter.ToUInt16(bufferIn!, 0));
            return BleStatus.Success;
        }

        private BleStatus aci_blue_events_lost_event_process(byte[] bufferIn)
        {
            if (BlueEventsLostEvent is not null)
                BlueEventsLostEvent.Invoke(BitConverter.GetBytes(BitConverter.ToUInt64(bufferIn!, 0)));
            return BleStatus.Success;
        }

        private BleStatus aci_gatt_indication_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![4];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 5, data, 0, dataLength);
            if (GattIndicationEvent is not null)
                GattIndicationEvent.Invoke(
                    BitConverter.ToUInt16(bufferIn, 0),
                    BitConverter.ToUInt16(bufferIn, 2),
                    dataLength,
                    data
                );
            return BleStatus.Success;
        }

        private BleStatus aci_gatt_notification_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![4];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 5, data, 0, dataLength);
            if (GattNotificationEvent is not null)
                GattNotificationEvent.Invoke(
                    BitConverter.ToUInt16(bufferIn, 0),
                    BitConverter.ToUInt16(bufferIn, 2),
                    dataLength,
                    data
                );
            return BleStatus.Success;
        }

        private BleStatus aci_gatt_proc_complete_event_process(byte[] bufferIn)
        {
            if (GattProcessCompleteEvent is not null)
                GattProcessCompleteEvent.Invoke(
                    BitConverter.ToUInt16(bufferIn!, 0),
                    (NotificationErrorCode)bufferIn[2]
                );
            return BleStatus.Success;
        }

        private BleStatus aci_gatt_error_resp_event_process(byte[] bufferIn)
        {
            if (GattErrorResponseEvent is not null)
                GattErrorResponseEvent.Invoke(
                    BitConverter.ToUInt16(bufferIn!, 0),
                    bufferIn[2],
                    BitConverter.ToUInt16(bufferIn, 3),
                    (ErrorResponseErrorCode)bufferIn[5]
                );
            return BleStatus.Success;
        }

        private BleStatus aci_gatt_disc_read_char_by_uuid_resp_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![4];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 5, data, 0, dataLength);
            if (GattDiscoverReadCharacteristicByUuidResponseEvent is not null)
                GattDiscoverReadCharacteristicByUuidResponseEvent.Invoke(
                    BitConverter.ToUInt16(bufferIn!, 0),
                    BitConverter.ToUInt16(bufferIn, 2),
                    dataLength,
                    data
                );
            return BleStatus.Success;
        }

        private BleStatus aci_gatt_write_permit_req_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![4];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 5, data, 0, dataLength);
            if (GattWritePermitRequestEvent is not null)
                GattWritePermitRequestEvent.Invoke(
                    BitConverter.ToUInt16(bufferIn!, 0),
                    BitConverter.ToUInt16(bufferIn, 2),
                    dataLength,
                    data
                );
            return BleStatus.Success;
        }

        private BleStatus aci_gatt_read_permit_req_event_process(byte[] bufferIn)
        {
            if (GattReadPermitRequestEvent is not null)
                GattReadPermitRequestEvent.Invoke(
                    BitConverter.ToUInt16(bufferIn!, 0),
                    BitConverter.ToUInt16(bufferIn, 2),
                    BitConverter.ToUInt16(bufferIn, 4)
                );
            return BleStatus.Success;
        }

        private BleStatus aci_gatt_read_multi_permit_req_event_process(byte[] bufferIn)
        {
            var handleCount = bufferIn![2];
            var ptr = 3;
            var handles = new ushort[64];
            for (var i = 0; i < handleCount; i++)
            {
                handles[i] = BitConverter.ToUInt16(bufferIn, ptr);
                ptr += 2;
            }

            if (GattReadMultiPermitRequestEvent is not null)
                GattReadMultiPermitRequestEvent.Invoke(
                    BitConverter.ToUInt16(bufferIn!, 0),
                    handleCount,
                    handles
                );
            return BleStatus.Success;
        }

        private BleStatus aci_gatt_tx_pool_available_event_process(byte[] bufferIn)
        {
            if (GattTxPoolAvailableEvent is not null)
                GattTxPoolAvailableEvent.Invoke(BitConverter.ToUInt16(bufferIn!, 0), BitConverter.ToUInt16(bufferIn, 2));
            return BleStatus.Success;
        }

        private BleStatus aci_gatt_server_confirmation_event_process(byte[] bufferIn)
        {
            if (GattServerConfirmationEvent is not null)
                GattServerConfirmationEvent.Invoke(BitConverter.ToUInt16(bufferIn!, 0));
            return BleStatus.Success;
        }

        private BleStatus aci_blue_initialized_event_process(byte[] bufferIn)
        {
            if (BlueInitializedEvent is not null)
                BlueInitializedEvent.Invoke((ReasonCode)bufferIn![0]);
            return BleStatus.Success;
        }

        private BleStatus hci_disconnection_complete_event_process(byte[] bufferIn)
        {
            var status = bufferIn![0];
            var connectionHandle = BitConverter.ToUInt16(bufferIn, 1);
            var reason = bufferIn[3];
            if (DisconnectionCompleteEvent is not null)
                DisconnectionCompleteEvent.Invoke(status, connectionHandle, reason);

            return BleStatus.Success;
        }

        public struct HandlePacketPair
        {
            public ushort ConnectionHandle;
            public ushort NumberOfCompletedPackets;
        }

        internal struct EventTableType
        {
            internal readonly ushort EventCode;
            internal readonly EventProcess Process;

            public EventTableType(ushort eventCode, EventProcess process)
            {
                EventCode = eventCode;
                Process = process;
            }
        }
    }

    public enum KeyPressNotificationType
    {
        EntryStarted = 0x00,
        DigitEntered = 0x01,
        DigitErased = 0x02,
        Cleared = 0x03,
        EntryCompleted = 0x04
    }

    public enum GapPairingCompleteReason
    {
        PasskeyEntryFailed = 0x01,
        OobNotAvailable = 0x02,
        AuthReqCannotBeMet = 0x03,
        ConfirmValueFailed = 0x04,
        PairingNotSupported = 0x05,
        InsufficientEncryptionKeySize = 0x06,
        CmdNotSupported = 0x07,
        UnspecifiedReason = 0x08,
        VeryEarlyNextAttempt = 0x09,
        SmInvalidParams = 0x0A,
        SmpScDhKeyCheckFailed = 0x0B,
        SmpScNumComparisonFailed = 0x0C
    }

    public enum GapPairingCompleteStatus
    {
        Success = 0x00,
        Timeout = 0x01,
        PairingFailed = 0x02,
        LtkMissingOnLocalDevice = 0x03,
        LtkMissingOnPeerDevice = 0x04,
        EncryptionNotSupportedByRemoteDevice = 0x05
    }

    public struct DirectAdvertisingReport_t
    {
        public AdvertisingType EventType;
        public AddressType AddressType;
        public byte[] Address;
        public AddressType DirectAddressType;
        public byte[] DirectAddress;
        public sbyte RSSI;
    }

    public enum NotificationErrorCode : byte
    {
        Success = 0x00,
        UnknownHciCommand = 0x01,
        UnknownConnectionIdentifier = 0x02,
        HardwareFailure = 0x03,
        PageTimeout = 0x04,
        AuthenticationFailure = 0x05,
        PinOrKeyMissing = 0x06,
        MemoryCapacityExceeded = 0x07,
        ConnectionTimeout = 0x08,
        ConnectionLimitExceeded = 0x09,
        SynchronousConnectionLimitToADeviceExceeded = 0x0A,
        AclConnectionAlreadyExists = 0x0B,
        CommandDisallowed = 0x0C,
        ConnectionRejectedDueToLimitedResources = 0x0D,
        ConnectionRejectedDueToSecurityReasons = 0x0E,
        ConnectionRejectedDueToUnacceptableBdAddress = 0x0F,
        ConnectionAcceptTimeoutExceeded = 0x10,
        UnsupportedFeatureOrParameterValue = 0x11,
        InvalidHciCommandParameters = 0x12,
        RemoteUserTerminatedConnection = 0x13,
        RemoteDeviceTerminatedConnectionDueToLowResources = 0x14,
        RemoteDeviceTerminatedConnectionDueToPowerOff = 0x15,
        ConnectionTerminatedByLocalHost = 0x16,
        RepeatedAttempts = 0x17,
        PairingNotAllowed = 0x18,
        UnknownLmpPdu = 0x19,
        UnsupportedFeature = 0x1A,
        ScoOffsetRejected = 0x1B,
        ScoIntervalRejected = 0x1C,
        ScoAirModeRejected = 0x1D,
        InvalidLmpParameters = 0x1E,
        UnspecifiedError = 0x1F,
        UnsupportedLmpParameterValue = 0x20,
        RoleChangeNotAllowed = 0x21,
        ResponseTimeout = 0x22,
        LmpErrorTransactionCollision = 0x23,
        LmpPduNotAllowed = 0x24,
        EncryptionModeNotAcceptable = 0x25,
        LinkKeyCannotBeChanged = 0x26,
        RequestedQoSNotSupported = 0x27,
        InstantPassed = 0x28,
        PairingWithUnitKeyNotSupported = 0x29,
        DifferentTransactionCollision = 0x2A,
        QoSUnacceptableParameter = 0x2C,
        QoSRejected = 0x2D,
        ChannelAssessmentNotSupported = 0x2E,
        InsufficientSecurity = 0x2F,
        ParameterOutOfMandatoryRange = 0x30,
        RoleSwitchPending = 0x32,
        ReservedSlotViolation = 0x34,
        RoleSwitchFailed = 0x35,
        ExtendedInquiryResponseTooLarge = 0x36,
        SecureSimplePairingNotSupportedByHost = 0x37,
        HostBusyPairing = 0x38,
        ConnectionRejectedDueToNoSuitableChannelFound = 0x39,
        ControllerBusy = 0x3A,
        UnacceptableConnectionInterval = 0x3B,
        DirectedAdvertisingTimeout = 0x3C,
        ConnectionTerminatedDueToMicFailure = 0x3D,
        ConnectionFailedToBeEstablished = 0x3E,
        MacOfAmp = 0x3F,
        Failed = 0x41,
        InvalidParameters = 0x42,
        Busy = 0x43,
        InvalidLength = 0x44,
        Pending = 0x45,
        NotAllowed = 0x46,
        GatTError = 0x47,
        AddressNotResolved = 0x48,
        InvalidCid = 0x50,
        CsrkNotFound = 0x5A,
        IrkNotFound = 0x5B,
        DeviceNotFoundInDb = 0x5C,
        SecurityDbFull = 0x5D,
        DeviceNotBonded = 0x5E,
        DeviceInBlacklist = 0x5F,
        InvalidHandle = 0x60,
        InvalidParameter = 0x61,
        OutOfHandles = 0x62,
        InvalidOperation = 0x63,
        InsufficientResources = 0x64,
        InsufficientEncryptionKeySize = 0x65,
        CharacteristicAlreadyExist = 0x66,
        NoValidSlot = 0x82,
        ShortWindow = 0x83,
        NewIntervalFailed = 0x84,
        TooLargeInterval = 0x85,
        SlotLengthFailed = 0x86,
        FlashReadFailed = 0xFA,
        FlashWriteFailed = 0xFB,
        FlashEraseFailed = 0xFC
    }

    public enum ErrorResponseErrorCode : byte
    {
        InvalidHandle = 0x01,
        ReadNotPermitted = 0x02,
        WriteNotPermitted = 0x03,
        InvalidPdu = 0x04,
        InsufficientAuthentication = 0x05,
        RequestNotSupported = 0x06,
        InvalidOffset = 0x07,
        InsufficientAuthorization = 0x08,
        PrepareQueueFull = 0x09,
        AttributeNotFound = 0x0A,
        AttributeNotLong = 0x0B,
        InsufficientEncryptionKeySize = 0x0C,
        InvalidAttributeValueLength = 0x0D,
        UnlikelyError = 0x0E,
        InsufficientEncryption = 0x0F,
        UnsupportedGroupType = 0x10,
        InsufficientResources = 0x11
    }

    [Flags]
    public enum ProcedureCode : byte
    {
        LimitedDiscovery = 0x01,
        GeneralDiscovery = 0x02,
        NameDiscovery = 0x04,
        AutoConnectionEstablishment = 0x08,
        GeneralConnectionEstablishment = 0x10,
        SelectiveConnectionEstablishment = 0x20,
        DirectConnectionEstablishment = 0x40,
        Observation = 0x80
    }

    public enum RadioState : byte
    {
        Idle = 0x00,
        Advertising = 0x01,
        ConnectionEventSlave = 0x02,
        Scanning = 0x03,
        ConnectionRequest = 0x04,
        ConnectionEventMaster = 0x05,
        TxTestMode = 0x06,
        RxTestMode = 0x07
    }

    public enum FirmwareErrorType : byte
    {
        L2CapRecombinationError = 0x01
    }

    public enum ReasonCode : byte
    {
        FirmwareStartedProperly = 0x01,
        UpdaterModeWithAciCommand = 0x02,
        UpdaterModeBadBlueFlag = 0x03,
        UpdaterModeIrqPin = 0x04,
        SystemResetWatchdog = 0x05,
        SystemResetLockup = 0x06,
        SystemResetBrownout = 0x07,
        SystemResetCrash = 0x08,
        SystemResetEccError = 0x09
    }

    public enum CrashType : byte
    {
        AssertFailed = 0x00,
        NmiFault = 0x01,
        HardFault = 0x02
    }

    public struct AttributeGroupHandlePair
    {
        public ushort FoundAttributeHandle;
        public ushort GroupEndHandle;
    }

    public struct AdvertisingReport_t
    {
        public AdvertisingType EventType;
        public AddressType AddressType;
        public byte[] Address;
        public byte DataLength;
        public byte[] Data;
        public sbyte Rssi;
    }

    public enum ClockAccuracy : byte
    {
        Ppm500 = 0x00,
        Ppm250 = 0x01,
        Ppm150 = 0x02,
        Ppm100 = 0x03,
        Ppm75 = 0x04,
        Ppm50 = 0x05,
        Ppm30 = 0x06,
        Ppm20 = 0x07
    }

    public enum DeviceRole : byte
    {
        Master = 0x00,
        Slave = 0x01
    }

    public enum AddressType : byte
    {
        PublicDevice = 0x00,
        RandomDevice = 0x01,
        PublicIdentity = 0x02,
        RandomIdentity = 0x03
    }

    public enum AdvertisingType : byte
    {
        ConnectableUndirected = 0x00,
        ConnectableDirected = 0x01,
        ScannableUndirected = 0x02,
        NonConnectableUndirected = 0x03,
        ScanResponse = 0x04
    }
}
