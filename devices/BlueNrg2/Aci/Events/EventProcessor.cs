// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    internal delegate BleStatus EventProcess(byte[] bufferIn);

    /// <summary>
    /// Class containing and generating all events.
    /// </summary>
    public class EventProcessor
    {
        internal readonly EventTableType[] LeMetaEventsTable;
        internal readonly EventTableType[] VendorSpecificEventsTable;
        internal EventTableType[] EventsTable;

        internal EventProcessor()
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
                new(0x0c17, aci_gatt_server_confirmation_event_process),
                /* aci_gatt_prepare_write_permit_req_event */
                new(0x0c18, aci_gatt_prepare_write_permit_req_event_process)
            };
        }

        /// <summary>
        /// The LE Advertising Report event indicates that a Bluetooth device or
        /// multiple Bluetooth devices have responded to an active scan or
        /// received some information during a passive scan. The Controller may
        /// queue these advertising reports and send information from multiple
        /// devices in one LE Advertising Report event. 
        /// </summary>
        public AdvertisingReport AdvertisingReportEvent;

        /// <summary>
        /// Function prototype for an advertising report.
        /// </summary>
        public delegate void AdvertisingReport(AdvertisingReportEventArgs eventArgs);

        /// <summary>
        /// This event is generated in response to an Exchange MTU request (local
        /// or from the peer). See <see cref="Aci.Gatt.ExchangeConfiguration"/>.
        /// </summary>
        public AttExchangeMtuResponse AttExchangeMtuResponseEvent;

        public delegate void AttExchangeMtuResponse(AttExchangeMtuResponseEventArgs eventArgs);

        /// <summary>
        /// This event is generated in response to an Execute Write Request.
        /// </summary>
        public AttExecuteWriteResponse AttExecuteWriteResponseEvent;

        public delegate void AttExecuteWriteResponse(AttExecuteWriteResponseEventArgs eventArgs);

        /// <summary>
        /// This event is generated in response to a <see cref="Aci.Att.FindByTypeValueRequest"/>
        /// </summary>
        public AttFindByTypeValueResponse AttFindByTypeValueResponseEvent;

        public delegate void AttFindByTypeValueResponse(AttFindByTypeValueResponseEventArgs eventArgs);

        /// <summary>
        /// This event is generated in response to a Find Information Request. See
        /// <see cref="Aci.Att.FindInformationRequest"/> and Find Information Response in Bluetooth
        /// Core v4.1 spec.
        /// </summary>
        public AttFindInfoResponse AttFindInfoResponseEvent;

        public delegate void AttFindInfoResponse(AttFindInfoResponseEventArgs eventArgs);

        /// <summary>
        /// This event is generated in response to a <see cref="Aci.Att.PrepareWriteRequest"/>.
        /// </summary>
        public AttPrepareWriteResponse AttPrepareWriteResponseEvent;

        public delegate void AttPrepareWriteResponse(AttPrepareWriteResponseEventArgs eventArgs);

        /// <summary>
        /// This event can be generated during a read long characteristic value
        /// procedure. See <see cref="Aci.Gatt.ReadLongCharacteristicValue"/>.
        /// </summary>
        public AttReadBlobResponse AttReadBlobResponseEvent;

        public delegate void AttReadBlobResponse(AttReadBlobResponseEventArgs eventArgs);

        /// <summary>
        /// This event is generated in response to a Read By Group Type Request.
        /// See <see cref="Aci.Gatt.DiscoverAllPrimaryServices"/>.
        /// </summary>
        public AttReadByGroupTypeResponse AttReadByGroupTypeResponseEvent;

        public delegate void AttReadByGroupTypeResponse(AttReadByGroupTypeResponseEventArgs eventArgs);

        /// <summary>
        /// This event is generated in response to a @ref
        /// aci_att_read_by_type_req. See <see cref="Aci.Gatt.FindIncludedServices"/> and
        /// <see cref="Aci.Gatt.DiscoverAllCharacteristicDescriptors"/>.
        /// </summary>
        public AttReadByTypeResponse AttReadByTypeResponseEvent;

        public delegate void AttReadByTypeResponse(AttReadByTypeResponseEventArgs eventArgs);

        /// <summary>
        /// This event is generated in response to a Read Multiple Request. 
        /// </summary>
        public AttReadMultipleResponse AttReadMultipleResponseEvent;

        public delegate void AttReadMultipleResponse(AttReadMultipleResponseEventArgs eventArgs);

        /// <summary>
        /// This event is generated in response to a Read Request. See <see cref="Aci.Gatt.ReadCharacteristicValue"/>.
        /// </summary>
        public AttReadResponse AttReadResponseEvent;

        public delegate void AttReadResponse(AttReadResponseEventArgs eventArgs);

        /// <summary>
        /// This event is given to the application after the
        /// <see cref="BlueInitializedEvent"/> when a system crash is detected.
        /// This events returns system crash information for debugging purposes.
        /// Information reported are useful to understand the root cause of the crash.
        /// </summary>
        public BlueCrashInfo BlueCrashInfoEvent;

        public delegate void BlueCrashInfo(BlueCrashInfoEventArgs eventArgs);

        /// <summary>
        /// This event is generated when an overflow occurs in the event queue
        /// read by the external microcontroller. This is normally caused when the
        /// external microcontroller does  not read pending events. The returned
        /// bitmap indicates which event has been lost. Please  note that one bit
        /// set to 1 indicates one or more occurrences of the particular events.
        /// The event ACI_BLUE_EVENTS_LOST_EVENT cannot be lost and it will
        /// inserted in the event queue as soon as a position is freed in the
        /// event queue. This event should not happen under normal operating
        /// condition where external microcontroller promptly reads events
        /// signaled by IRQ pin. It is provided to detected unexpected behavior of
        /// the external microcontroller or to allow application to recover
        /// situations where critical events are lost.
        /// </summary>
        public BlueEventsLost BlueEventsLostEvent;

        public delegate void BlueEventsLost(BlueEventsLostEventArgs eventArgs);

        /// <summary>
        /// This event inform the application that the network coprocessor has
        /// been reset. If the reason code is a system crash, a following event
        /// <see cref="BlueCrashInfoEvent"/> will provide more information regarding
        /// the system crash details.
        /// </summary>
        public BlueInitialized BlueInitializedEvent;

        public delegate void BlueInitialized(BlueInitializedEventArgs eventArgs);

        /// <summary>
        /// The LE Connection Complete event indicates to both of the Hosts
        /// forming the connection that a new connection has been created. Upon
        /// the creation of the connection a Connection_Handle shall be assigned
        /// by the Controller, and passed to the Host in this event. If the
        /// connection establishment fails this event shall be provided to the
        /// Host that had issued the LE_Create_Connection command. This event
        /// indicates to the Host which issued a LE_Create_Connection command and
        /// received a Command Status event if the connection establishment failed
        /// or was successful. The Master_Clock_Accuracy parameter is only valid
        /// for a slave. On a master, this parameter shall be set to 0x00.
        /// </summary>
        public ConnectionComplete ConnectionCompleteEvent;

        public delegate void ConnectionComplete(ConnectionCompleteEventArgs eventArgs);

        /// <summary>
        /// The LE Connection Update Complete event is used to indicate that the
        /// Controller process to update the connection has completed. On a slave,
        /// if no connection parameters are updated, then this event shall not be
        /// issued. On a master, this event shall be issued if the
        /// Connection_Update command was sent.
        /// </summary>
        public ConnectionUpdateComplete ConnectionUpdateCompleteEvent;

        public delegate void ConnectionUpdateComplete(ConnectionUpdateCompleteEventArgs eventArgs);

        /// <summary>
        /// This event is used to indicate that the Controller's data buffers
        /// have been overflowed. This can occur if the Host has sent more packets
        /// than allowed. The Link_Type parameter is used to indicate that the
        /// overflow was caused by ACL data.
        /// </summary>
        public DataBufferOverflow DataBufferOverflowEvent;

        public delegate void DataBufferOverflow(DataBufferOverflowEventArgs eventArgs);

        /// <summary>
        /// The LE Data Length Change event notifies the Host of a change to
        /// either the maximum Payload length or the maximum transmission time of
        /// Data Channel PDUs in either direction. The values reported are the
        /// maximum that will actually be used on the connection following the
        /// change.
        /// </summary>
        public DataLengthChange DataLengthChangeEvent;

        public delegate void DataLengthChange(DataLengthChangeEventArgs eventArgs);

        /// <summary>
        /// The LE Direct Advertising Report event indicates that directed
        /// advertisements have been received where the advertiser is using a
        /// resolvable private address for the InitA field in the ADV_DIRECT_IND
        /// PDU and the Scanning_Filter_Policy is equal to 0x02 or 0x03, see
        /// HCI_LE_Set_Scan_Parameters. Direct_Address_Type and Direct_Addres is
        /// the address the directed advertisements are being directed to.
        /// Address_Type and Address is the address of the advertiser sending the
        /// directed advertisements.
        /// </summary>
        public DirectAdvertisingReport DirectAdvertisingReportEvent;

        public delegate void DirectAdvertisingReport(DirectAdvertisingReportEventArgs eventArgs);

        /// <summary>
        /// The Disconnection Complete event occurs when a connection is
        /// terminated. The status parameter indicates if the disconnection was
        /// successful or not. The reason parameter indicates the reason for the
        /// disconnection if the disconnection was successful. If the
        /// disconnection was not successful, the value of the reason parameter
        /// can be ignored by the Host. For example, this can be the case if the
        /// Host has issued the Disconnect command and there was a parameter
        /// error, or the command was not presently allowed, or a
        /// Connection_Handle that didn't correspond to a connection was given.
        /// </summary>
        public DisconnectionComplete DisconnectionCompleteEvent;

        public delegate void DisconnectionComplete(DisconnectionCompleteEventArgs eventArgs);

        /// <summary>
        /// The Encryption Change event is used to indicate that the change of the
        /// encryption mode has been completed. The Connection_Handle will be a
        /// Connection_Handle for an ACL connection. The Encryption_Enabled event
        /// parameter specifies the new Encryption_Enabled parameter for the
        /// Connection_Handle specified by the Connection_Handle event parameter.
        /// This event will occur on both devices to notify the Hosts when
        /// Encryption has changed for the specified Connection_Handle between two
        /// devices. Note: This event shall not be generated if encryption is
        /// paused or resumed; during a role switch, for example. The meaning of
        /// the Encryption_Enabled parameter depends on whether the Host has
        /// indicated support for Secure Connections in the
        /// Secure_Connections_Host_Support parameter. When
        /// Secure_Connections_Host_Support is 'disabled' or the Connection_Handle
        /// refers to an LE link, the Controller shall only use Encryption_Enabled
        /// values 0x00 (OFF) and 0x01 (ON). (See Bluetooth Specification v.4.1,
        /// Vol. 2, Part E, 7.7.8)
        /// </summary>
        public EncryptionChange EncryptionChangeEvent;

        public delegate void EncryptionChange(EncryptionChangeEventArgs eventArgs);

        /// <summary>
        /// This event is used to indicate to the
        /// Host that the encryption key was refreshed on the given
        /// <see cref="EncryptionKeyRefreshCompleteEventArgs.ConnectionHandle"/> any time encryption is paused and then resumed. If
        /// the Encryption Key Refresh Complete event was generated due to an
        /// encryption pause and resume operation embedded within a change
        /// connection link key procedure, the Encryption Key Refresh Complete
        /// event shall be sent prior to the Change Connection Link Key Complete
        /// event. If the Encryption Key Refresh Complete event was generated due
        /// to an encryption pause and resume operation embedded within a role
        /// switch procedure, the Encryption Key Refresh Complete event shall be
        /// sent prior to the Role Change event.
        /// </summary>
        public EncryptionKeyRefreshComplete EncryptionKeyRefreshCompleteEvent;

        public delegate void EncryptionKeyRefreshComplete(EncryptionKeyRefreshCompleteEventArgs eventArgs);

        /// <summary>
        /// This event indicates to both of the Hosts forming the connection that a new connection has been created.
        /// Upon the creation of the connection a <see cref="EnhancedConnectionCompleteEventArgs.ConnectionHandle"/> shall be
        /// assigned by the Controller, and passed to the Host in this event. If
        /// the connection establishment fails, this event shall be provided to
        /// the Host that had issued the <see cref="Hci.LeCreateConnection"/> command. If this
        /// event is unmasked and <see cref="ConnectionCompleteEvent"/> event is unmasked, only
        /// the Enhanced Connection Complete event is sent when a new
        /// connection has been completed. This event indicates to the Host that
        /// issued a <see cref="Hci.LeCreateConnection"/> command and received a Command Status
        /// event if the connection establishment failed or was successful. The
        /// <see cref="EnhancedConnectionCompleteEventArgs.MasterClockAccuracy"/> parameter is only valid for a slave. On a
        /// master, this parameter shall be set to 0x00.
        /// </summary>
        public EnhancedConnectionComplete EnhancedConnectionCompleteEvent;

        public delegate void EnhancedConnectionComplete(EnhancedConnectionCompleteEventArgs eventArgs);

        /// <summary>
        /// This event is sent only by a privacy enabled Peripheral. The event is
        /// sent to the upper layers when the peripheral is unsuccessful in
        /// resolving the resolvable address of the peer device after connecting
        /// to it.
        /// </summary>
        public GapAddressNotResolved GapAddressNotResolvedEvent;

        public delegate void GapAddressNotResolved(GapAddressNotResolvedEventArgs eventArgs);

        /// <summary>
        /// This event is generated by the Security manager to the application
        /// when the application has set that authorization is required for
        /// reading/writing of attributes. This event will be generated as soon as
        /// the pairing is complete. When this event is received, @ref
        /// aci_gap_authorization_resp command should be used to respond by the
        /// application.
        /// </summary>
        public GapAuthorizationRequest GapAuthorizationRequestEvent;

        public delegate void GapAuthorizationRequest(GapAuthorizationRequestEventArgs eventArgs);

        /// <summary>
        /// This event is generated on the slave when a
        /// <see cref="Aci.Gap.SlaveSecurityRequest"/> is called to reestablish the bond with
        /// a master but the master has lost the bond. When this event is
        /// received, the upper layer has to issue the <see cref="Aci.Gap.AllowRebond"/>
        /// command in order to allow the slave to continue the pairing process
        /// with the master. On the master this event is raised when
        /// <see cref="Aci.Gap.SendPairingRequest"/> is called to reestablish a bond with a
        /// slave but the slave has lost the bond. In order to create a new bond
        /// the master has to launch <see cref="Aci.Gap.SendPairingRequest"/> with
        /// forceRebond set to true.
        /// </summary>
        public GapBondLost GapBondLostEvent;

        public delegate void GapBondLost(GapBondLostEventArgs eventArgs);

        /// <summary>
        /// This event is sent only during SC v.4.2 Pairing, when Keypress
        /// Notifications are supported, in order to show the input type signalled
        /// by the peer device, having Keyboard only I/O capabilities. When this
        /// event is received, no action is required to the User.
        /// </summary>
        public GapKeypressNotification GapKeypressNotificationEvent;

        public delegate void GapKeypressNotification(GapKeypressNotificationEventArgs eventArgs);

        /// <summary>
        /// This event is generated by the controller when the limited
        /// discoverable mode ends due to timeout. The timeout is 180 seconds.
        /// </summary>
        public GapLimitedDiscoverable GapLimitedDiscoverableEvent;

        public delegate void GapLimitedDiscoverable(GapLimitedDiscoverableEventArgs eventArgs);

        /// <summary>
        /// This event is sent only during SC v.4.2 Pairing, when Numeric
        /// Comparison Association model is selected, in order to show the Numeric
        /// Value generated, and to ask for Confirmation to the User. When this
        /// event is received, the application has to respond with the <see cref="Aci.Gap.NumericComparisonValueConfirmYesNo"/> command.
        /// </summary>
        public GapNumericComparisonValue GapNumericComparisonValueEvent;

        public delegate void GapNumericComparisonValue(GapNumericComparisonValueEventArgs eventArgs);

        /// <summary>
        /// This event is generated when the pairing process has completed
        /// successfully or a pairing procedure timeout has occurred or the
        /// pairing has failed. This is to notify the application that we have
        /// paired with a remote device so that it can take further actions or to
        /// notify that a timeout has occurred so that the upper layer can decide
        /// to disconnect the link.
        /// </summary>
        public GapPairingComplete GapPairingCompleteEvent;

        public delegate void GapPairingComplete(GapPairingCompleteEventArgs eventArgs);

        /// <summary>
        /// This event is generated by the Security manager to the application
        /// when a passkey is required for pairing. When this event is received,
        /// the application has to respond with the <see cref="Aci.Gap.PassKeyResponse"/> command.
        /// </summary>
        public GapPassKeyRequest GapPassKeyRequestEvent;

        public delegate void GapPassKeyRequest(GapPassKeyRequestEventArgs eventArgs);

        /// <summary>
        /// This event is sent by the GAP to the upper layers when a procedure
        /// previously started has been terminated by the upper layer or has
        /// completed for any other reason.
        /// </summary>
        public GapProcessComplete GapProcessCompleteEvent;

        public delegate void GapProcessComplete(GapProcessCompleteEventArgs eventArgs);

        /// <summary>
        /// This event is generated when the slave security request is
        /// successfully sent to the master.
        /// </summary>
        public GapSlaveSecurityInitiated GapSlaveSecurityInitiatedEvent;

        public delegate void GapSlaveSecurityInitiated(GapSlaveSecurityInitiatedEventArgs eventArgs);

        /// <summary>
        /// This event is generated to the application by the GATT server when a
        /// client modifies any attribute on the server, as consequence of one of
        /// the following GATT procedures:
        /// <list type="bullet">
        /// <item>write without response</item>
        /// <item>signed write without response</item>
        /// <item>write characteristic value</item>
        /// <item>write long characteristic value</item>
        /// <item>reliable write</item>
        /// </list>
        /// </summary>
        public GattAttributeModified GattAttributeModifiedEvent;

        public delegate void GattAttributeModified(GattAttributeModifiedEventArgs eventArgs);

        /// <summary>
        /// This event can be generated during a "Discover Characteristics By
        /// UUID" procedure or a "Read using Characteristic UUID" procedure. The
        /// attribute value will be a service declaration as defined in Bluetooth
        /// Core v4.1 spec (vol.3, Part G, ch. 3.3.1), when a "Discover
        /// Characteristics By UUID" has been started. It will be the value of the
        /// Characteristic if a* "Read using Characteristic UUID" has been performed.
        /// </summary>
        public GattDiscoverReadCharacteristicByUuidResponse GattDiscoverReadCharacteristicByUuidResponseEvent;

        public delegate void GattDiscoverReadCharacteristicByUuidResponse(GattDiscoverReadCharacteristicByUuidResponseEventArgs eventArgs);

        /// <summary>
        /// This event is generated when an Error Response is received from the
        /// server. The error response can be given by the server at the end of
        /// one of the GATT discovery procedures. This does not mean that the
        /// procedure ended with an error, but this error event is part of the
        /// procedure itself.
        /// </summary>
        public GattErrorResponse GattErrorResponseEvent;

        public delegate void GattErrorResponse(GattErrorResponseEventArgs eventArgs);

        /// <summary>
        /// This event is generated when an indication is received from the server.
        /// </summary>
        public GattIndication GattIndicationEvent;

        public delegate void GattIndication(GattIndicationEventArgs eventArgs);

        /// <summary>
        /// This event is generated when a notification is received from the server.
        /// </summary>
        public GattNotification GattNotificationEvent;

        public delegate void GattNotification(GattNotificationEventArgs eventArgs);

        /// <summary>
        /// This event is given to the application when a prepare write request is
        /// received by the server from the client. This event will be given to
        /// the application only if the event bit for this event generation is set
        /// when the characteristic was added. When this event is received, the
        /// application has to check whether the value being requested for write
        /// can be allowed to be written and respond with the command <see cref="Aci.Gatt.WriteResponse"/>.
        /// Based on the response from the application, the
        /// attribute value will be modified by the stack. If the write is
        /// rejected by the application, then the value of the attribute will not
        /// be modified and an error response will be sent to the client, with the
        /// error code as specified by the application.
        /// </summary>
        public GattPrepareWritePermitRequest GattPrepareWritePermitRequestEvent;

        public delegate void GattPrepareWritePermitRequest(GattPrepareWritePermitRequestEventArgs eventArgs);

        /// <summary>
        /// This event is generated when a GATT client procedure completes either
        /// with error or successfully.
        /// </summary>
        public GattProcessComplete GattProcessCompleteEvent;

        public delegate void GattProcessComplete(GattProcessCompleteEventArgs eventArgs);

        /// <summary>
        /// This event is generated by the client/server to the application on a
        /// GATT timeout (30 seconds). This is a critical event that should not
        /// happen during normal operating conditions. It is an indication of
        /// either a major disruption in the communication link or a mistake in
        /// the application which does not provide a reply to GATT procedures.
        /// After this event, the GATT channel is closed and no more GATT
        /// communication can be performed. The applications is exptected to issue
        /// an <see cref="Aci.Gap.Terminate"/> to disconnect from the peer device. It is
        /// important to leave an 100 ms blank window before sending the 
        /// <see cref="Aci.Gap.Terminate"/>, since immediately after this event, system could
        /// save important information in non volatile memory.
        /// </summary>
        public GattProcessTimeout GattProcessTimeoutEvent;

        public delegate void GattProcessTimeout(GattProcessTimeoutEventArgs eventArgs);

        /// <summary>
        /// This event is given to the application when a read multiple request or
        /// read by type request is received by the server from the client. This
        /// event will be given to the application only if the event bit for this
        /// event generation is set when the characteristic was added. On
        /// receiving this event, the application can update the values of the
        /// handles if it desires and when done, it has to send the 
        /// <see cref="Aci.Gatt.AllowRead"/> command to indicate to the stack that it can send
        /// the response to the client.
        /// </summary>
        public GattReadMultiPermitRequest GattReadMultiPermitRequestEvent;

        public delegate void GattReadMultiPermitRequest(GattReadMultiPermitRequestEventArgs eventArgs);

        /// <summary>
        /// This event is given to the application when a read request or read
        /// blob request is received by the server from the client. This event
        /// will be given to the application only if the event bit for this event
        /// generation is set when the characteristic was added. On receiving this
        /// event, the application can update the value of the handle if it
        /// desires and when done, it has to send the <see cref="Aci.Gatt.AllowRead"/>
        /// command to indicate to the stack that it can send the response to the client.
        /// </summary>
        public GattReadPermitRequest GattReadPermitRequestEvent;

        public delegate void GattReadPermitRequest(GattReadPermitRequestEventArgs eventArgs);

        /// <summary>
        /// This event is generated when the client has sent the confirmation to a
        /// previously sent indication.
        /// </summary>
        public GattServerConfirmation GattServerConfirmationEvent;

        public delegate void GattServerConfirmation(GattServerConfirmationEventArgs eventArgs);

        /// <summary>
        /// Each time BLE FW stack raises the error code <see cref="BleStatus.InsufficientResources"/>,
        /// this event is generated as soon as the available buffer size  is greater than maximum ATT MTU
        /// (on stack versions below v2.1 this event is generated when at least 2 packets with MTU of 23 bytes are available).
        /// </summary>
        public GattTxPoolAvailable GattTxPoolAvailableEvent;

        public delegate void GattTxPoolAvailable(GattTxPoolAvailableEventArgs eventArgs);

        /// <summary>
        /// This event is given to the application when a write request, write
        /// command or signed write command is received by the server from the
        /// client. This event will be given to the application only if the event
        /// bit for this event generation is set when the characteristic was
        /// added. When this event is received, the application has to check
        /// whether the value being requested for write can be allowed to be
        /// written and respond with the command <see cref="Aci.Gatt.WriteResponse"/>. The
        /// details of the parameters of the command can be found. Based on the
        /// response from the application, the attribute value will be modified by
        /// the stack. If the write is rejected by the application, then the value
        /// of the attribute will not be modified. In case of a write REQ, an
        /// error response will be sent to the client, with the error code as
        /// specified by the application. In case of write/signed write commands,
        /// no response is sent to the client but the attribute is not modified.
        /// </summary>
        public GattWritePermitRequest GattWritePermitRequestEvent;

        public delegate void GattWritePermitRequest(GattWritePermitRequestEventArgs eventArgs);

        /// <summary>
        /// This event indicates that LE Diffie Hellman key generation has been
        /// completed by the Controller.
        /// </summary>
        public GenerateDhKeyComplete GenerateDhKeyCompleteEvent;

        public delegate void GenerateDhKeyComplete(GenerateDhKeyCompleteEventArgs eventArgs);

        /// <summary>
        /// This event is generated when the device completes a radio activity and
        /// provide information when a new radio activity will be performed.
        /// Information provided includes type of radio activity and absolute time
        /// in system ticks when a new radio activity is schedule, if any.
        /// Application can use this information to schedule user activities
        /// synchronous to selected radio activities. A command 
        /// <see cref="Aci.Hal.SetRadioActivityMask"/> is provided to enable radio activity
        /// events of user interests, by default no events are enabled. User
        /// should take into account that enabling radio events in application
        /// with intense radio activity could lead to a fairly high rate of events
        /// generated. Application use cases includes synchronizing notification
        /// with connection interval, switching  antenna at the end of advertising
        /// or performing flash erase operation while radio is idle.
        /// </summary>
        public HalEndOfRadioActivity HalEndOfRadioActivityEvent;

        public delegate void HalEndOfRadioActivity(HalEndOfRadioActivityEventArgs eventArgs);

        /// <summary>
        /// This event is generated to report firmware error informations. After
        /// this event with error type equal to either 0x01, 0x02 or 0x3, it is
        /// recommended to disconnect the link (handle is reported in Data field).
        /// </summary>
        public HalFirmwareError HalFirmwareErrorEvent;

        public delegate void HalFirmwareError(HalFirmwareErrorEventArgs eventArgs);

        /// <summary>
        /// This event is reported to the application after a scan request is
        /// received and a scan response is scheduled to be transmitted.
        /// </summary>
        public HalScanRequestReport HalScanRequestReportEvent;

        public delegate void HalScanRequestReport(HalScanRequestReportEventArgs eventArgs);

        /// <summary>
        /// The Hardware Error event is used to indicate some implementation
        /// specific type of hardware failure for the controller. This event is
        /// used to notify the Host that a hardware failure has occurred in the
        /// Controller.
        /// </summary>
        public HardwareError HardwareErrorEvent;

        public delegate void HardwareError(HardwareErrorEventArgs eventArgs);

        /// <summary>
        /// This event is generated when the master responds to the connection
        /// update request packet with a command reject packet.
        /// </summary>
        public L2CapCommandReject L2CapCommandRejectEvent;

        public delegate void L2CapCommandReject(L2CapCommandRejectEventArgs eventArgs);

        /// <summary>
        /// The event is given by the L2CAP layer when a connection update request
        /// is received from the slave. The upper layer which receives this event
        /// has to respond by sending a <see cref="Aci.L2Cap.ConnectionParameterUpdateResponse"/> command.
        /// </summary>
        public L2CapConnectionUpdateRequest L2CapConnectionUpdateRequestEvent;

        public delegate void L2CapConnectionUpdateRequest(L2CapConnectionUpdateRequestEventArgs eventArgs);

        /// <summary>
        /// This event is generated when the master responds to the connection
        /// update request packet with a connection update response packet.
        /// </summary>
        public L2CapConnectionUpdateResponse L2CapConnectionUpdateResponseEvent;

        public delegate void L2CapConnectionUpdateResponse(L2CapConnectionUpdateResponseEventArgs eventArgs);

        /// <summary>
        /// This event is generated when the master does not respond to the
        /// connection update request packet with a connection update response
        /// packet or a command reject packet within 30 seconds.
        /// </summary>
        public L2CapProcessTimeout L2CapProcessTimeoutEvent;

        public delegate void L2CapProcessTimeout(L2CapProcessTimeoutEventArgs eventArgs);

        /// <summary>
        /// The LE Long Term Key Request event indicates that the master device is
        /// attempting to encrypt or re-encrypt the link and is requesting the
        /// Long Term Key from the Host. (See [Vol 6] Part B, Section 5.1.3).
        /// </summary>
        public LongTermKeyRequest LongTermKeyRequestEvent;

        public delegate void LongTermKeyRequest(LongTermKeyRequestEventArgs eventArgs);

        /// <summary>
        /// The Number Of Completed Packets event is used by the Controller to
        /// indicate to the Host how many HCI Data Packets have been completed
        /// (transmitted or flushed) for each Connection_Handle since the previous
        /// Number Of Completed Packets event was sent to the Host. This means
        /// that the corresponding buffer space has been freed in the Controller.
        /// Based on this information, and the HC_Total_Num_ACL_Data_Packets and
        /// HC_Total_Num_Synchronous_- Data_Packets return parameter of the
        /// Read_Buffer_Size command, the Host can determine for which
        /// Connection_Handles the following HCI Data Packets should be sent to
        /// the Controller. The Number Of Completed Packets event must not be sent
        /// before the corresponding Connection Complete event. While the
        /// Controller has HCI data packets in its buffer, it must keep sending
        /// the Number Of Completed Packets event to the Host at least
        /// periodically, until it finally reports that all the pending ACL Data
        /// Packets have been transmitted or flushed.
        /// </summary>
        public NumberOfCompletePackets NumberOfCompletePacketsEvent;

        public delegate void NumberOfCompletePackets(NumberOfCompletePacketsEventArgs eventArgs);

        /// <summary>
        /// This event is generated when local P-256 key generation is complete.
        /// </summary>
        public ReadLocalP256PublicKeyComplete ReadLocalP256PublicKeyCompleteEvent;

        public delegate void ReadLocalP256PublicKeyComplete(ReadLocalP256PublicKeyCompleteEventArgs eventArgs);

        /// <summary>
        /// The LE Read Remote Used Features Complete event is used to indicate the completion
        /// of the process of the Controller obtaining the used features of the remote
        /// Bluetooth device specified by the <see cref="ReadRemoteUsedFeaturesCompleteEventArgs.ConnectionHandle"/> event parameter.
        /// </summary>
        public ReadRemoteUsedFeaturesComplete ReadRemoteUsedFeaturesCompleteEvent;

        public delegate void ReadRemoteUsedFeaturesComplete(ReadRemoteUsedFeaturesCompleteEventArgs eventArgs);

        public ReadRemoteVersionInformationComplete ReadRemoteVersionInformationCompleteEvent;

        public delegate void ReadRemoteVersionInformationComplete(ReadRemoteVersionInformationCompleteEventArgs eventArgs);

        /// <summary>
        /// The Read Remote Version Information Complete event is used to indicate
        /// the completion of the process obtaining the version information of the
        /// remote Controller specified by the <see cref="ReadRemoteVersionInformationCompleteEventArgs.ConnectionHandle"/> event parameter.
        /// The <see cref="ReadRemoteVersionInformationCompleteEventArgs.ConnectionHandle"/> shall be for an ACL connection. The Version event
        /// parameter defines the specification version of the LE Controller. The
        /// <see cref="ReadRemoteVersionInformationCompleteEventArgs.ManufacturerName"/> event parameter indicates the manufacturer of the remote
        /// Controller. The Subversion event parameter is controlled by the manufacturer
        /// and is implementation dependent. The <see cref="ReadRemoteVersionInformationCompleteEventArgs.Subversion"/> event parameter defines the
        /// various revisions that each version of the Bluetooth hardware will go through
        /// as design processes change and errors are fixed. This allows the software
        /// to determine what Bluetooth hardware is being used and, if necessary, to
        /// work around various bugs in the hardware. When the <see cref="ReadRemoteVersionInformationCompleteEventArgs.ConnectionHandle"/> is
        /// associated with an LE-U logical link, the Version event parameter shall
        /// be Link Layer VersNr parameter, the <see cref="ReadRemoteVersionInformationCompleteEventArgs.ManufacturerName"/> event parameter
        /// shall be the CompId parameter, and the <see cref="ReadRemoteVersionInformationCompleteEventArgs.Subversion"/> event parameter shall
        /// be the SubVersNr parameter.
        /// (See Bluetooth Specification v.4.1, Vol. 2, Part E, 7.7.12)
        /// </summary>
        /// <seealso cref="Hci.ReadRemoteVersionInformation"/>
        //public EventHandler<ReadRemoteVersionInformationCompleteEventArgs> ReadRemoteVersionInformationCompleteEvent;
        private BleStatus hci_encryption_key_refresh_complete_event_process(byte[] bufferIn)
        {
            if (EncryptionKeyRefreshCompleteEvent is not null)
                EncryptionKeyRefreshCompleteEvent.Invoke(new EncryptionKeyRefreshCompleteEventArgs(bufferIn![0], BitConverter.ToUInt16(bufferIn, 1)));

            return BleStatus.Success;
        }

        private BleStatus hci_data_buffer_overflow_event_process(byte[] bufferIn)
        {
            if (DataBufferOverflowEvent is not null)
                DataBufferOverflowEvent.Invoke(new DataBufferOverflowEventArgs((LinkType)bufferIn![0]));
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
                NumberOfCompletePacketsEvent.Invoke(new NumberOfCompletePacketsEventArgs(numberOfHandles, handlePacketPairs));

            return BleStatus.Success;
        }

        private BleStatus hci_hardware_error_event_process(byte[] bufferIn)
        {
            if (HardwareErrorEvent is not null)
                HardwareErrorEvent.Invoke(new HardwareErrorEventArgs((HardwareErrorCode)bufferIn![0]));
            return BleStatus.Success;
        }

        private BleStatus hci_read_remote_version_information_complete_event_process(byte[] bufferIn)
        {
            if (ReadRemoteVersionInformationCompleteEvent is not null)
                ReadRemoteVersionInformationCompleteEvent.Invoke(
                    new ReadRemoteVersionInformationCompleteEventArgs(
                        bufferIn![0],
                        BitConverter.ToUInt16(bufferIn, 1),
                        bufferIn[3],
                        BitConverter.ToUInt16(bufferIn, 4),
                        BitConverter.ToUInt16(bufferIn, 5)
                    )
                );
            return BleStatus.Success;
        }

        private BleStatus hci_encryption_change_event_process(byte[] bufferIn)
        {
            if (EncryptionChangeEvent is not null)
                EncryptionChangeEvent.Invoke(new EncryptionChangeEventArgs(bufferIn![0], BitConverter.ToUInt16(bufferIn, 1), bufferIn[3]));
            return BleStatus.Success;
        }

        private BleStatus hci_le_direct_advertising_report_event_process(byte[] bufferIn)
        {
            var reportCount = bufferIn![0];
            byte ptr = 1;
            var directAdvertisingReports = new DirectAdvertisingReportContainer[8];
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
                DirectAdvertisingReportEvent.Invoke(new DirectAdvertisingReportEventArgs(reportCount, directAdvertisingReports));

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
                    new EnhancedConnectionCompleteEventArgs(
                        bufferIn[0],
                        BitConverter.ToUInt16(bufferIn, 1),
                        (DeviceRole)bufferIn[3],
                        (AddressType)bufferIn[4],
                        peerAddress,
                        localResolvablePrivateAddress,
                        peerResolvablePrivateAddress,
                        BitConverter.ToUInt16(bufferIn, 23),
                        BitConverter.ToUInt16(bufferIn, 25),
                        BitConverter.ToUInt16(bufferIn, 27),
                        (ClockAccuracy)bufferIn[28]
                    )
                );
            return BleStatus.Success;
        }

        private BleStatus hci_le_generate_dhkey_complete_event_process(byte[] bufferIn)
        {
            var dhKey = new byte[32];
            Array.Copy(bufferIn!, 1, dhKey, 0, 32);
            if (GenerateDhKeyCompleteEvent is not null)
                GenerateDhKeyCompleteEvent.Invoke(new GenerateDhKeyCompleteEventArgs(bufferIn[0], dhKey));
            return BleStatus.Success;
        }

        private BleStatus hci_le_read_local_p256_public_key_complete_event_process(byte[] bufferIn)
        {
            var localP256PublicKey = new byte[64];
            Array.Copy(bufferIn!, 1, localP256PublicKey, 0, 64);
            if (ReadLocalP256PublicKeyCompleteEvent is not null)
                ReadLocalP256PublicKeyCompleteEvent.Invoke(new ReadLocalP256PublicKeyCompleteEventArgs(bufferIn[0], localP256PublicKey));
            return BleStatus.Success;
        }

        private BleStatus hci_le_data_length_change_event_process(byte[] bufferIn)
        {
            if (DataLengthChangeEvent is not null)
                DataLengthChangeEvent.Invoke(
                    new DataLengthChangeEventArgs(
                        BitConverter.ToUInt16(bufferIn!, 0),
                        BitConverter.ToUInt16(bufferIn, 2),
                        BitConverter.ToUInt16(bufferIn, 4),
                        BitConverter.ToUInt16(bufferIn, 6),
                        BitConverter.ToUInt16(bufferIn, 8)
                    )
                );
            return BleStatus.Success;
        }

        private BleStatus hci_le_long_term_key_request_event_process(byte[] bufferIn)
        {
            if (LongTermKeyRequestEvent is not null)
                LongTermKeyRequestEvent.Invoke(
                    new LongTermKeyRequestEventArgs(
                        BitConverter.ToUInt16(bufferIn!, 0),
                        BitConverter.ToUInt64(bufferIn, 2),
                        BitConverter.ToUInt16(bufferIn, 10)
                    )
                );
            return BleStatus.Success;
        }

        private BleStatus hci_le_connection_update_complete_event_process(byte[] bufferIn)
        {
            if (ConnectionUpdateCompleteEvent is not null)
                ConnectionUpdateCompleteEvent.Invoke(
                    new ConnectionUpdateCompleteEventArgs(
                        bufferIn![0],
                        BitConverter.ToUInt16(bufferIn, 1),
                        BitConverter.ToUInt16(bufferIn, 3),
                        BitConverter.ToUInt16(bufferIn, 5),
                        BitConverter.ToUInt16(bufferIn, 7)
                    )
                );
            return BleStatus.Success;
        }

        private BleStatus hci_le_read_remote_used_features_complete_event_process(byte[] bufferIn)
        {
            var features = new byte[8];
            Array.Copy(bufferIn!, 3, features, 0, 8);
            if (ReadRemoteUsedFeaturesCompleteEvent is not null)
                ReadRemoteUsedFeaturesCompleteEvent.Invoke(
                    new ReadRemoteUsedFeaturesCompleteEventArgs(bufferIn[0], BitConverter.ToUInt16(bufferIn, 1), features)
                );
            return BleStatus.Success;
        }

        private BleStatus hci_le_advertising_report_event_process(byte[] bufferIn)
        {
            var reportCount = bufferIn![0];
            var reports = new AdvertisingReportContainer[9];
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
                AdvertisingReportEvent.Invoke(new AdvertisingReportEventArgs(reportCount, reports));

            return BleStatus.Success;
        }

        private BleStatus hci_le_connection_complete_event_process(byte[] bufferIn)
        {
            var peerAddress = new byte[6];
            Array.Copy(bufferIn!, 5, peerAddress, 0, 6);

            if (ConnectionCompleteEvent is not null)
                ConnectionCompleteEvent.Invoke(
                    new ConnectionCompleteEventArgs(
                        bufferIn[0],
                        BitConverter.ToUInt16(bufferIn, 1),
                        (DeviceRole)bufferIn[3],
                        (AddressType)bufferIn[4],
                        peerAddress,
                        BitConverter.ToUInt16(bufferIn, 11),
                        BitConverter.ToUInt16(bufferIn, 13),
                        BitConverter.ToUInt16(bufferIn, 15),
                        (ClockAccuracy)bufferIn[16]
                    )
                );

            return BleStatus.Success;
        }

        private BleStatus aci_gap_bond_lost_event_process(byte[] bufferIn)
        {
            if (GapBondLostEvent is not null)
                GapBondLostEvent.Invoke(new GapBondLostEventArgs());
            return BleStatus.Success;
        }

        private BleStatus aci_gap_slave_security_initiated_event_process(byte[] bufferIn)
        {
            if (GapSlaveSecurityInitiatedEvent is not null)
                GapSlaveSecurityInitiatedEvent.Invoke(new GapSlaveSecurityInitiatedEventArgs());
            return BleStatus.Success;
        }

        private BleStatus aci_gap_authorization_req_event_process(byte[] bufferIn)
        {
            if (GapAuthorizationRequestEvent is not null)
                GapAuthorizationRequestEvent.Invoke(new GapAuthorizationRequestEventArgs(BitConverter.ToUInt16(bufferIn!, 0)));
            return BleStatus.Success;
        }

        private BleStatus aci_gap_pass_key_req_event_process(byte[] bufferIn)
        {
            if (GapPassKeyRequestEvent is not null)
                GapPassKeyRequestEvent.Invoke(new GapPassKeyRequestEventArgs(BitConverter.ToUInt16(bufferIn!, 0)));
            return BleStatus.Success;
        }

        private BleStatus aci_gap_limited_discoverable_event_process(byte[] bufferIn)
        {
            if (GapLimitedDiscoverableEvent is not null)
                GapLimitedDiscoverableEvent.Invoke(new GapLimitedDiscoverableEventArgs());
            return BleStatus.Success;
        }

        private BleStatus aci_hal_fw_error_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![1];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 2, data, 0, dataLength);
            if (HalFirmwareErrorEvent is not null)
                HalFirmwareErrorEvent.Invoke(new HalFirmwareErrorEventArgs((FirmwareErrorType)bufferIn[0], dataLength, data));
            return BleStatus.Success;
        }

        private BleStatus aci_hal_scan_req_report_event_process(byte[] bufferIn)
        {
            var peerAddress = new byte[6];
            Array.Copy(bufferIn!, 2, peerAddress, 0, 6);
            if (HalScanRequestReportEvent is not null)
                HalScanRequestReportEvent.Invoke(new HalScanRequestReportEventArgs((sbyte)bufferIn[0], (AddressType)bufferIn[1], peerAddress));
            return BleStatus.Success;
        }

        private BleStatus aci_hal_end_of_radio_activity_event_process(byte[] bufferIn)
        {
            if (HalEndOfRadioActivityEvent is not null)
                HalEndOfRadioActivityEvent.Invoke(
                    new HalEndOfRadioActivityEventArgs((RadioState)bufferIn![0], (RadioState)bufferIn[1], BitConverter.ToUInt32(bufferIn, 2))
                );
            return BleStatus.Success;
        }

        private BleStatus aci_blue_crash_info_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![18];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 19, data, 0, dataLength);
            if (BlueCrashInfoEvent is not null)
                BlueCrashInfoEvent.Invoke(
                    new BlueCrashInfoEventArgs(
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
                    )
                );
            return BleStatus.Success;
        }

        private BleStatus aci_gap_pairing_complete_event_process(byte[] bufferIn)
        {
            if (GapPairingCompleteEvent is not null)
                GapPairingCompleteEvent.Invoke(
                    new GapPairingCompleteEventArgs(
                        BitConverter.ToUInt16(bufferIn!, 0),
                        (GapPairingCompleteStatus)bufferIn[2],
                        (GapPairingCompleteReason)bufferIn[3]
                    )
                );
            return BleStatus.Success;
        }

        private BleStatus aci_gap_keypress_notification_event_process(byte[] bufferIn)
        {
            if (GapKeypressNotificationEvent is not null)
                GapKeypressNotificationEvent.Invoke(
                    new GapKeypressNotificationEventArgs(BitConverter.ToUInt16(bufferIn!, 0), (KeyPressNotificationType)bufferIn[2])
                );
            return BleStatus.Success;
        }

        private BleStatus aci_gap_numeric_comparison_value_event_process(byte[] bufferIn)
        {
            if (GapNumericComparisonValueEvent is not null)
                GapNumericComparisonValueEvent.Invoke(
                    new GapNumericComparisonValueEventArgs(BitConverter.ToUInt16(bufferIn!, 0), BitConverter.ToUInt32(bufferIn, 2))
                );
            return BleStatus.Success;
        }

        private BleStatus aci_l2cap_proc_timeout_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![2];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 3, data, 0, dataLength);
            if (L2CapProcessTimeoutEvent is not null)
                L2CapProcessTimeoutEvent.Invoke(new L2CapProcessTimeoutEventArgs(BitConverter.ToUInt16(bufferIn!, 0), dataLength, data));
            return BleStatus.Success;
        }

        private BleStatus aci_l2cap_connection_update_req_event_process(byte[] bufferIn)
        {
            if (L2CapConnectionUpdateRequestEvent is not null)
                L2CapConnectionUpdateRequestEvent.Invoke(
                    new L2CapConnectionUpdateRequestEventArgs(
                        BitConverter.ToUInt16(bufferIn!, 0),
                        bufferIn[2],
                        BitConverter.ToUInt16(bufferIn, 3),
                        BitConverter.ToUInt16(bufferIn, 5),
                        BitConverter.ToUInt16(bufferIn, 7),
                        BitConverter.ToUInt16(bufferIn, 9),
                        BitConverter.ToUInt16(bufferIn, 11)
                    )
                );
            return BleStatus.Success;
        }

        private BleStatus aci_l2cap_connection_update_resp_event_process(byte[] bufferIn)
        {
            if (L2CapConnectionUpdateResponseEvent is not null)
                L2CapConnectionUpdateResponseEvent.Invoke(
                    new L2CapConnectionUpdateResponseEventArgs(BitConverter.ToUInt16(bufferIn!, 0), BitConverter.ToUInt16(bufferIn, 2))
                );
            return BleStatus.Success;
        }

        private BleStatus aci_gap_addr_not_resolved_event_process(byte[] bufferIn)
        {
            if (GapAddressNotResolvedEvent is not null)
                GapAddressNotResolvedEvent.Invoke(new GapAddressNotResolvedEventArgs(BitConverter.ToUInt16(bufferIn!, 0)));
            return BleStatus.Success;
        }

        private BleStatus aci_gatt_attribute_modified_event_process(byte[] bufferIn)
        {
            var attributeLength = bufferIn![6];
            var attributeData = new byte[attributeLength];
            Array.Copy(bufferIn, 7, attributeData, 0, attributeLength);
            if (GattAttributeModifiedEvent is not null)
                GattAttributeModifiedEvent.Invoke(
                    new GattAttributeModifiedEventArgs(
                        BitConverter.ToUInt16(bufferIn!, 0),
                        BitConverter.ToUInt16(bufferIn, 2),
                        BitConverter.ToUInt16(bufferIn, 4),
                        attributeLength,
                        attributeData
                    )
                );
            return BleStatus.Success;
        }

        private BleStatus aci_l2cap_command_reject_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![5];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 6, data, 0, dataLength);
            if (L2CapCommandRejectEvent is not null)
                L2CapCommandRejectEvent.Invoke(
                    new L2CapCommandRejectEventArgs(
                        BitConverter.ToUInt16(bufferIn!, 0),
                        bufferIn[2],
                        BitConverter.ToUInt16(bufferIn, 3),
                        dataLength,
                        data
                    )
                );
            return BleStatus.Success;
        }

        private BleStatus aci_gatt_proc_timeout_event_process(byte[] bufferIn)
        {
            if (GattProcessTimeoutEvent is not null)
                GattProcessTimeoutEvent.Invoke(new GattProcessTimeoutEventArgs(BitConverter.ToUInt16(bufferIn!, 0)));
            return BleStatus.Success;
        }

        private BleStatus aci_att_exchange_mtu_resp_event_process(byte[] bufferIn)
        {
            if (AttExchangeMtuResponseEvent is not null)
                AttExchangeMtuResponseEvent.Invoke(
                    new AttExchangeMtuResponseEventArgs(BitConverter.ToUInt16(bufferIn!, 0), BitConverter.ToUInt16(bufferIn, 2))
                );
            return BleStatus.Success;
        }

        private BleStatus aci_gap_proc_complete_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![2];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 3, data, 0, dataLength);
            if (GapProcessCompleteEvent is not null)
                GapProcessCompleteEvent.Invoke(new GapProcessCompleteEventArgs((ProcedureCode)bufferIn[0], bufferIn[1], dataLength, data));
            return BleStatus.Success;
        }

        private BleStatus aci_att_read_by_type_resp_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![3];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 4, data, 0, dataLength);
            if (AttReadByTypeResponseEvent is not null)
                AttReadByTypeResponseEvent.Invoke(
                    new AttReadByTypeResponseEventArgs(
                        BitConverter.ToUInt16(bufferIn, 0),
                        bufferIn[2],
                        dataLength,
                        data
                    )
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
                AttFindByTypeValueResponseEvent.Invoke(
                    new AttFindByTypeValueResponseEventArgs(BitConverter.ToUInt16(bufferIn, 0), handlePairCount, handlePairs)
                );
            return BleStatus.Success;
        }

        private BleStatus aci_att_find_info_resp_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![3];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 4, data, 0, dataLength);
            if (AttFindInfoResponseEvent is not null)
                AttFindInfoResponseEvent.Invoke(
                    new AttFindInfoResponseEventArgs(
                        BitConverter.ToUInt16(bufferIn, 0),
                        bufferIn[2],
                        dataLength,
                        data
                    )
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
                    new AttReadResponseEventArgs(
                        BitConverter.ToUInt16(bufferIn, 0),
                        dataLength,
                        data
                    )
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
                    new AttReadBlobResponseEventArgs(
                        BitConverter.ToUInt16(bufferIn, 0),
                        dataLength,
                        data
                    )
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
                    new AttReadMultipleResponseEventArgs(
                        BitConverter.ToUInt16(bufferIn, 0),
                        dataLength,
                        data
                    )
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
                    new AttReadByGroupTypeResponseEventArgs(
                        BitConverter.ToUInt16(bufferIn, 0),
                        bufferIn[2],
                        dataLength,
                        data
                    )
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
                    new AttPrepareWriteResponseEventArgs(
                        BitConverter.ToUInt16(bufferIn, 0),
                        BitConverter.ToUInt16(bufferIn, 2),
                        BitConverter.ToUInt16(bufferIn, 4),
                        dataLength,
                        data
                    )
                );
            return BleStatus.Success;
        }

        private BleStatus aci_att_exec_write_resp_event_process(byte[] bufferIn)
        {
            if (AttExecuteWriteResponseEvent is not null)
                AttExecuteWriteResponseEvent.Invoke(new AttExecuteWriteResponseEventArgs(BitConverter.ToUInt16(bufferIn!, 0)));
            return BleStatus.Success;
        }

        private BleStatus aci_blue_events_lost_event_process(byte[] bufferIn)
        {
            if (BlueEventsLostEvent is not null)
                BlueEventsLostEvent.Invoke(new BlueEventsLostEventArgs((LostEventBitmap)BitConverter.ToUInt64(bufferIn!, 0)));
            return BleStatus.Success;
        }

        private BleStatus aci_gatt_indication_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![4];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 5, data, 0, dataLength);
            if (GattIndicationEvent is not null)
                GattIndicationEvent.Invoke(
                    new GattIndicationEventArgs(
                        BitConverter.ToUInt16(bufferIn, 0),
                        BitConverter.ToUInt16(bufferIn, 2),
                        dataLength,
                        data
                    )
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
                    new GattNotificationEventArgs(
                        BitConverter.ToUInt16(bufferIn, 0),
                        BitConverter.ToUInt16(bufferIn, 2),
                        dataLength,
                        data
                    )
                );
            return BleStatus.Success;
        }

        private BleStatus aci_gatt_proc_complete_event_process(byte[] bufferIn)
        {
            if (GattProcessCompleteEvent is not null)
                GattProcessCompleteEvent.Invoke(
                    new GattProcessCompleteEventArgs(
                        BitConverter.ToUInt16(bufferIn!, 0),
                        (BleStatus)bufferIn[2]
                    )
                );
            return BleStatus.Success;
        }

        private BleStatus aci_gatt_error_resp_event_process(byte[] bufferIn)
        {
            if (GattErrorResponseEvent is not null)
                GattErrorResponseEvent.Invoke(
                    new GattErrorResponseEventArgs(
                        BitConverter.ToUInt16(bufferIn!, 0),
                        bufferIn[2],
                        BitConverter.ToUInt16(bufferIn, 3),
                        (AttErrorCode)bufferIn[5]
                    )
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
                    new GattDiscoverReadCharacteristicByUuidResponseEventArgs(
                        BitConverter.ToUInt16(bufferIn!, 0),
                        BitConverter.ToUInt16(bufferIn, 2),
                        dataLength,
                        data
                    )
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
                    new GattWritePermitRequestEventArgs(
                        BitConverter.ToUInt16(bufferIn!, 0),
                        BitConverter.ToUInt16(bufferIn, 2),
                        dataLength,
                        data
                    )
                );
            return BleStatus.Success;
        }

        private BleStatus aci_gatt_read_permit_req_event_process(byte[] bufferIn)
        {
            if (GattReadPermitRequestEvent is not null)
                GattReadPermitRequestEvent.Invoke(
                    new GattReadPermitRequestEventArgs(
                        BitConverter.ToUInt16(bufferIn!, 0),
                        BitConverter.ToUInt16(bufferIn, 2),
                        BitConverter.ToUInt16(bufferIn, 4)
                    )
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
                    new GattReadMultiPermitRequestEventArgs(
                        BitConverter.ToUInt16(bufferIn!, 0),
                        handleCount,
                        handles
                    )
                );
            return BleStatus.Success;
        }

        private BleStatus aci_gatt_tx_pool_available_event_process(byte[] bufferIn)
        {
            if (GattTxPoolAvailableEvent is not null)
                GattTxPoolAvailableEvent.Invoke(
                    new GattTxPoolAvailableEventArgs(BitConverter.ToUInt16(bufferIn!, 0), BitConverter.ToUInt16(bufferIn, 2))
                );
            return BleStatus.Success;
        }

        private BleStatus aci_gatt_server_confirmation_event_process(byte[] bufferIn)
        {
            if (GattServerConfirmationEvent is not null)
                GattServerConfirmationEvent.Invoke(new GattServerConfirmationEventArgs(BitConverter.ToUInt16(bufferIn!, 0)));
            return BleStatus.Success;
        }

        private BleStatus aci_gatt_prepare_write_permit_req_event_process(byte[] bufferIn)
        {
            var dataLength = bufferIn![5];
            var data = new byte[dataLength];
            Array.Copy(bufferIn, 6, data, 0, dataLength);
            if (GattPrepareWritePermitRequestEvent is not null)
                GattPrepareWritePermitRequestEvent.Invoke(
                    new GattPrepareWritePermitRequestEventArgs(
                        BitConverter.ToUInt16(bufferIn!, 0),
                        BitConverter.ToUInt16(bufferIn, 2),
                        BitConverter.ToUInt16(bufferIn, 4),
                        dataLength,
                        data
                    )
                );
            return BleStatus.Success;
        }

        private BleStatus aci_blue_initialized_event_process(byte[] bufferIn)
        {
            if (BlueInitializedEvent is not null)
                BlueInitializedEvent.Invoke(new BlueInitializedEventArgs((ReasonCode)bufferIn![0]));
            return BleStatus.Success;
        }

        private BleStatus hci_disconnection_complete_event_process(byte[] bufferIn)
        {
            var status = bufferIn![0];
            var connectionHandle = BitConverter.ToUInt16(bufferIn, 1);
            var reason = bufferIn[3];
            if (DisconnectionCompleteEvent is not null)
                DisconnectionCompleteEvent.Invoke(new DisconnectionCompleteEventArgs(status, connectionHandle, reason));

            return BleStatus.Success;
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
}
