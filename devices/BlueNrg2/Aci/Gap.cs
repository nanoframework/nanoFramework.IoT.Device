// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci
{
	public class Gap
	{
		[Flags]
		public enum EventMask : ushort
		{
			NoEvents = 0x0000,
			LimitedDiscoverableEvent = 0x0001,
			PairingCompleteEvent = 0x0002,
			PassKeyReqEvent = 0x0004,
			AuthorizationReqEvent = 0x0008,
			SlaveSecurityInitiatedEvent = 0x0010,
			BondLostEvent = 0x0020,
			ProcCompleteEvent = 0x0080,
			L2CapConnectionUpdateRequestEvent = 0x0100,
			L2CapConnectionUpdateResponseEvent = 0x0200,
			L2CapProcessTimeoutEvent = 0x0400,
			AddressNotResolvedEvent = 0x0800
		}

		private readonly TransportLayer _transportLayer;

		internal Gap(TransportLayer transportLayer)
		{
			_transportLayer = transportLayer;
		}

		/// <summary>
		/// Put the device in non-discoverable mode. This command disables the LL advertising.
		/// </summary>
		/// <returns>Value indicating success or error code.</returns>
		public BleStatus SetNonDiscoverable()
		{
			var status = new byte[1];
			var rq = new Request
			{
				OpCodeGroup = 0x3f,
				OpCodeCommand = 0x081,
				ResponseParameter = status,
				ResponseLength = 1
			};
			if (_transportLayer.SendRequest(ref rq, false) < 0)
				return BleStatus.Timeout;
			return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
		}

		/// <summary>
		/// Put the device in limited discoverable mode (as defined in Bluetooth
		/// Specification v.4.1, Vol. 3, Part C, section 9.2.3). The device will
		/// be discoverable for maximum period of TGAP (lim_adv_timeout) = 180
		/// seconds (from errata). The advertising can be disabled at any time by
		/// issuing <see cref="SetNonDiscoverable" /> command. The
		/// Adv_Interval_Min and Adv_Interval_Max parameters are optional. If both
		/// are set to 0, the GAP will use default values for adv intervals for
		/// limited discoverable mode (250 ms and 500 ms respectively). To allow a
		/// fast connection, the host can set Local_Name, Service_Uuid_List,
		/// Slave_Conn_Interval_Min and Slave_Conn_Interval_Max. If provided,
		/// these data will be  inserted into the advertising packet payload as AD
		/// data. These parameters are optional in this command. These values can
		/// be set in advertised data using GAP_Update_Adv_Data command
		/// separately. The total size of data in advertising packet cannot exceed
		/// 31 bytes. With this command, the BLE Stack will also add automatically
		/// the following standard AD types: - AD Flags - Power Level When
		/// advertising timeout happens (i.e. limited discovery period has
		/// elapsed), controller generates <see cref="E:BlueNrg2.Events.GapLimitedDiscoverableEvent" /> event
		/// </summary>
		/// <param name="advertisingType">Advertising type.</param>
		/// <param name="minimumAdvertisingInterval"></param>
		/// <param name="maximumAdvertisingInterval"></param>
		/// <param name="ownAddressType"></param>
		/// <param name="advertisingFilterPolicy">
		/// Advertising filter policy: not applicable
		/// (the value of the <paramref name="advertisingFilterPolicy" /> parameter is not used inside the Stack)
		/// </param>
		/// <param name="localNameLength">
		/// Length of the local_name field in octets. If length
		/// is set to 0x00, the <paramref name="localName" /> parameter is not used.
		/// </param>
		/// <param name="localName">
		/// Local name of the device. First byte must be 0x08 for
		/// Shortened Local Name  or 0x09 for Complete Local Name. No null
		/// character at the end.
		/// </param>
		/// <param name="serviceUuidLength">
		/// Length of the Service Uuid List in octets. If
		/// there is no service to be advertised, set this field to 0x00.
		/// </param>
		/// <param name="serviceUuidList">
		/// This is the list of the UUIDs as defined in Volume
		/// 3, Section 11 of GAP Specification. First byte is the AD Type. See
		/// also Supplement to the Bluetooth Core specification.
		/// </param>
		/// <param name="minimumSlaveConnectionInterval"></param>
		/// <param name="maximumSlaveConnectionInterval"></param>
		/// <returns>Value indicating success or error code.</returns>
		public BleStatus SetLimitedDiscoverable(
			AdvertisingType advertisingType,
			ushort minimumAdvertisingInterval,
			ushort maximumAdvertisingInterval,
			AddressType ownAddressType,
			byte advertisingFilterPolicy,
			byte localNameLength,
			byte[] localName,
			byte serviceUuidLength,
			byte[] serviceUuidList,
			ushort minimumSlaveConnectionInterval,
			ushort maximumSlaveConnectionInterval)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Put the device in general discoverable mode (as defined in Bluetooth
		/// Specification v.4.1, Vol. 3, Part C, section 9.2.4). The device will
		/// be discoverable until the host issues  the <see cref="SetNonDiscoverable" /> command. The Adv_Interval_Min and
		/// Adv_Interval_Max parameters are optional. If both are set to 0, the
		/// GAP uses the default values for adv intervals for general discoverable
		/// mode. When using connectable undirected advertising events: -
		/// Adv_Interval_Min = 30 ms  - Adv_Interval_Max = 60 ms When using non-
		/// connectable advertising events or scannable undirected advertising
		/// events: - Adv_Interval_Min = 100 ms  - Adv_Interval_Max = 150 ms  Host
		/// can set the Local Name, a Service UUID list and the Slave Connection
		/// Interval Range. If provided, these data will be inserted into the
		/// advertising packet payload as AD data. These parameters are optional
		/// in this command. These values can be also set using
		/// aci_gap_update_adv_data() separately. The total size of data in
		/// advertising packet cannot exceed 31 bytes. With this command, the BLE
		/// Stack will also add automatically the following standard AD types: -
		/// AD Flags - TX Power Level
		/// </summary>
		/// <param name="advertisingType">Advertising type.</param>
		/// <param name="minimumAdvertisingInterval"></param>
		/// <param name="maximumAdvertisingInterval"></param>
		/// <param name="ownAddressType"></param>
		/// <param name="advertisingFilterPolicy">
		/// Advertising filter policy: not applicable
		/// (the value of the <paramref name="advertisingFilterPolicy" /> parameter is not used inside the Stack)
		/// </param>
		/// <param name="localNameLength">
		/// Length of the local_name field in octets. If length
		/// is set to 0x00, the <paramref name="localName" /> parameter is not used.
		/// </param>
		/// <param name="localName">
		/// Local name of the device. First byte must be 0x08 for
		/// Shortened Local Name  or 0x09 for Complete Local Name. No null
		/// character at the end.
		/// </param>
		/// <param name="serviceUuidLength">
		/// Length of the Service Uuid List in octets. If
		/// there is no service to be advertised, set this field to 0x00.
		/// </param>
		/// <param name="serviceUuidList">
		/// This is the list of the UUIDs as defined in Volume
		/// 3, Section 11 of GAP Specification. First byte is the AD Type. See
		/// also Supplement to the Bluetooth Core specification.
		/// </param>
		/// <param name="minimumSlaveConnectionInterval"></param>
		/// <param name="maximumSlaveConnectionInterval"></param>
		/// <returns>Value indicating success or error code.</returns>
		public BleStatus SetDiscoverable(
			AdvertisingType advertisingType,
			ushort minimumAdvertisingInterval,
			ushort maximumAdvertisingInterval,
			AddressType ownAddressType,
			byte advertisingFilterPolicy,
			byte localNameLength,
			byte[] localName,
			byte serviceUuidLength,
			byte[] serviceUuidList,
			ushort minimumSlaveConnectionInterval,
			ushort maximumSlaveConnectionInterval)
		{
			var command = new byte[12 + localNameLength + serviceUuidLength];
			var ptr = 0;
			command[ptr] = (byte)advertisingType;
			ptr += 1;
			BitConverter.GetBytes(minimumAdvertisingInterval).CopyTo(command, ptr);
			ptr += 2;
			BitConverter.GetBytes(maximumAdvertisingInterval).CopyTo(command, ptr);
			ptr += 2;
			command[ptr] = (byte)ownAddressType;
			ptr += 1;
			command[ptr] = advertisingFilterPolicy;
			ptr += 1;
			command[ptr] = localNameLength;
			ptr += 1;
			if (localNameLength != 0)
			{
				if (localName is null || localName.Length != localNameLength)
					throw new ArgumentException($"{nameof(localName)} has to be of the same length as indicated in {nameof(localNameLength)}");
				localName.CopyTo(command, ptr);
			}

			ptr += localNameLength;
			command[ptr] = serviceUuidLength;
			if (serviceUuidLength != 0)
			{
				if (serviceUuidList is null || serviceUuidList.Length != serviceUuidLength)
					throw new ArgumentException($"{nameof(serviceUuidList)} has to be of the same length as indicated in {nameof(serviceUuidLength)}");
				serviceUuidList.CopyTo(command, ptr);
			}

			ptr += serviceUuidLength;
			BitConverter.GetBytes(minimumSlaveConnectionInterval).CopyTo(command, ptr);
			ptr += 2;
			BitConverter.GetBytes(maximumSlaveConnectionInterval).CopyTo(command, ptr);
			ptr += 2;

			var status = new byte[1];
			var rq = new Request
			{
				OpCodeGroup = 0x3f,
				OpCodeCommand = 0x083,
				CommandParameter = command,
				CommandLength = (uint)ptr,
				ResponseParameter = status,
				ResponseLength = 1
			};
			if (_transportLayer.SendRequest(ref rq, false) < 0)
				return BleStatus.Timeout;
			return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
		}

		public BleStatus SetDirectConnectable(
			AddressType addressType,
			AdvertisingType directedAdvertisingType,
			AddressType directAddressType,
			byte[] directAddress,
			ushort minimumAdvertisingInterval,
			ushort maximumAdvertisingInterval
		)
		{
			throw new NotImplementedException();
		}

		public BleStatus SetIoCapability(IoCapability ioCapability)
		{
			var command = new byte[1];
			command[0] = (byte)ioCapability;

			var status = new byte[1];
			var rq = new Request
			{
				OpCodeGroup = 0x3f,
				OpCodeCommand = 0x085,
				CommandParameter = command,
				CommandLength = 1,
				ResponseParameter = status,
				ResponseLength = 1
			};
			if (_transportLayer.SendRequest(ref rq, false) < 0)
				return BleStatus.Timeout;
			return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
		}

		public BleStatus SetAuthenticationRequirement(
			bool enableBondingMode,
			bool mitmProtectionRequired,
			SecureConnectionSupport secureConnectionSupport,
			bool keyPressNotificationIsSupported,
			byte minimumEncryptionKeySize,
			byte maximumEncryptionSize,
			bool useFixedPin,
			uint fixedPin,
			AddressType identityAddressType)
		{
			var command = new byte[12];
			command[0] = (byte)(enableBondingMode ? 0x01 : 0x00);
			command[1] = (byte)(mitmProtectionRequired ? 0x01 : 0x00);
			command[2] = (byte)secureConnectionSupport;
			command[3] = (byte)(keyPressNotificationIsSupported ? 0x01 : 0x00);
			command[4] = minimumEncryptionKeySize;
			command[5] = maximumEncryptionSize;
			command[6] = (byte)(useFixedPin ? 0x01 : 0x00);
			BitConverter.GetBytes(fixedPin).CopyTo(command, 7);
			command[11] = (byte)identityAddressType;

			var status = new byte[1];
			var rq = new Request
			{
				OpCodeGroup = 0x3f,
				OpCodeCommand = 0x086,
				CommandParameter = command,
				CommandLength = 12,
				ResponseParameter = status,
				ResponseLength = 1
			};
			if (_transportLayer.SendRequest(ref rq, false) < 0)
				return BleStatus.Timeout;
			return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
		}

		public BleStatus SetAuthorizationRequirement(ushort connectionHandle, bool requireAuthorization)
		{
			var command = new byte[3];
			BitConverter.GetBytes(connectionHandle).CopyTo(command, 0);
			command[2] = (byte)(requireAuthorization ? 0x01 : 0x00);

			var status = new byte[1];
			var rq = new Request
			{
				OpCodeGroup = 0x3f,
				OpCodeCommand = 0x087,
				CommandParameter = command,
				CommandLength = 3,
				ResponseParameter = status,
				ResponseLength = 1
			};
			if (_transportLayer.SendRequest(ref rq, false) < 0)
				return BleStatus.Timeout;
			return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
		}

		public BleStatus PassKeyResponse(ushort connectionHandle, uint passKey)
		{
			var command = new byte[6];
			BitConverter.GetBytes(connectionHandle).CopyTo(command, 0);
			BitConverter.GetBytes(passKey).CopyTo(command, 2);

			var status = new byte[1];
			var rq = new Request
			{
				OpCodeGroup = 0x3f,
				OpCodeCommand = 0x088,
				CommandParameter = command,
				CommandLength = 6,
				ResponseParameter = status,
				ResponseLength = 1
			};
			if (_transportLayer.SendRequest(ref rq, false) < 0)
				return BleStatus.Timeout;
			return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
		}

		public BleStatus AuthorizationResponse(ushort connectionHandle, bool authorize)
		{
			var command = new byte[3];
			BitConverter.GetBytes(connectionHandle).CopyTo(command, 0);
			command[2] = (byte)(authorize ? 0x01 : 0x00);

			var status = new byte[1];
			var rq = new Request
			{
				OpCodeGroup = 0x3f,
				OpCodeCommand = 0x089,
				CommandParameter = command,
				CommandLength = 3,
				ResponseParameter = status,
				ResponseLength = 1
			};
			if (_transportLayer.SendRequest(ref rq, false) < 0)
				return BleStatus.Timeout;
			return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
		}

		public BleStatus Init(
			Role role,
			bool enablePrivacy,
			byte deviceNameCharacteristicLength,
			ref ushort serviceHandle,
			ref ushort deviceNameCharacteristicHandle,
			ref ushort appearanceCharacteristicHandle)
		{
			var command = new byte[3];
			command[0] = (byte)role;
			command[1] = (byte)(enablePrivacy ? 0x01 : 0x00);
			command[2] = deviceNameCharacteristicLength;

			var response = new byte[7];
			var rq = new Request
			{
				OpCodeGroup = 0x3f,
				OpCodeCommand = 0x08a,
				CommandParameter = command,
				CommandLength = 3,
				ResponseParameter = response,
				ResponseLength = 7
			};
			if (_transportLayer.SendRequest(ref rq, false) < 0)
				return BleStatus.Timeout;
			if (response[0] != 0)
				return (BleStatus)response[0];

			serviceHandle = BitConverter.ToUInt16(response, 1);
			deviceNameCharacteristicHandle = BitConverter.ToUInt16(response, 3);
			appearanceCharacteristicHandle = BitConverter.ToUInt16(response, 5);

			return BleStatus.Success;
		}

		public BleStatus SetNonConnectable(
			byte advertisingEventType,
			AddressType ownAddressType
		)
		{
			throw new NotImplementedException();
		}

		public BleStatus SetUndirectedConnectable(
			ushort minimumAdvertisingInterval,
			ushort maximumAdvertisingInterval,
			AddressType ownAddressType,
			FilterPolicy advertisingFilterPolicy
		)
		{
			throw new NotImplementedException();
		}

		public BleStatus SlaveSecurityRequest(ushort connectionHandle)
		{
			var command = new byte[2];
			BitConverter.GetBytes(connectionHandle).CopyTo(command, 0);

			var status = new byte[1];
			var rq = new Request
			{
				OpCodeGroup = 0x3f,
				OpCodeCommand = 0x08d,
				CommandParameter = command,
				CommandLength = 2,
				ResponseParameter = status,
				ResponseLength = 1
			};
			if (_transportLayer.SendRequest(ref rq, false) < 0)
				return BleStatus.Timeout;
			return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
		}

		public BleStatus UpdateAdvertisingData(byte advertisingDataLength, byte[] advertisingData)
		{
			var command = new byte[1 + advertisingDataLength];
			var ptr = 0;
			command[ptr] = advertisingDataLength;
			ptr++;
			if (advertisingData is null || advertisingData.Length != advertisingDataLength)
				throw new ArgumentException(
					$"the length of {nameof(advertisingData)} needs to be the same as indicated in {nameof(advertisingDataLength)}"
				);
			advertisingData.CopyTo(command, ptr);
			ptr += advertisingDataLength;

			var status = new byte[1];
			var rq = new Request
			{
				OpCodeGroup = 0x3f,
				OpCodeCommand = 0x08e,
				CommandParameter = command,
				CommandLength = (uint)ptr,
				ResponseParameter = status,
				ResponseLength = 1
			};
			if (_transportLayer.SendRequest(ref rq, false) < 0)
				return BleStatus.Timeout;
			return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
		}

		public BleStatus DeleteAdvertisingType(
			AdvertisingType advertisingType
		)
		{
			throw new NotImplementedException();
		}

		public BleStatus GetSecurityLevel(
			ushort connectionHandle,
			ref byte securityMode,
			ref byte securityLevel
		)
		{
			throw new NotImplementedException();
		}

		public BleStatus SetEventMask(
			EventMask eventMask
		)
		{
			throw new NotImplementedException();
		}

		public BleStatus ConfigureWhitelist()
		{
			throw new NotImplementedException();
		}

		public BleStatus Terminate(ushort connectionHandle, Reason reason)
		{
			throw new NotImplementedException();
		}

		public BleStatus ClearSecurityDatabase()
		{
			var status = new byte[1];
			var rq = new Request
			{
				OpCodeGroup = 0x3f,
				OpCodeCommand = 0x094,
				ResponseParameter = status,
				ResponseLength = 1
			};
			if (_transportLayer.SendRequest(ref rq, false) < 0)
				return BleStatus.Timeout;
			return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
		}

		public BleStatus AllowReBond(ushort connectionHandle)
		{
			throw new NotImplementedException();
		}

		public BleStatus StartLimitedDiscoveryProcedure(
			ushort scanInterval,
			ushort scanWindow,
			AddressType ownAddressType,
			bool filterDuplicates
		)
		{
			throw new NotImplementedException();
		}

		public BleStatus StartGeneralDiscoveryProcedure(
			ushort scanInterval,
			ushort scanWindow,
			AddressType ownAddressType,
			bool filterDuplicates
		)
		{
			throw new NotImplementedException();
		}

		public BleStatus StartNameDiscoveryProcedure(
			ushort scanInterval,
			ushort scanWindow,
			AddressType peerAddressType,
			byte[] peerAddress,
			AddressType ownAddressType,
			ushort minimumConnectionInterval,
			ushort maximumConnectionInterval,
			ushort connectionLatency,
			ushort supervisionTimeout,
			ushort minimumConnectionLength,
			ushort maximumConnectionLength
		)
		{
			throw new NotImplementedException();
		}

		public BleStatus StartAutomaticConnectionEstablishProcedure(
			ushort scanInterval,
			ushort scanWindow,
			AddressType ownAddressType,
			ushort minimumConnectionInterval,
			ushort maximumConnectionInterval,
			ushort connectionLatency,
			ushort supervisionTimeout,
			ushort minimumConnectionLength,
			ushort maximumConnectionLength,
			byte whitelistEntryCount,
			DeviceAddressEntry[] whitelistEntries
		)
		{
			throw new NotImplementedException();
		}

		public BleStatus StartGeneralConnectionEstablishProcedure(
			ScanType scanType,
			ushort scanInterval,
			ushort scanWindow,
			AddressType ownAddressType,
			FilterPolicy scanningFilterPolicy,
			bool filterDuplicates
		)
		{
			throw new NotImplementedException();
		}

		public BleStatus StartSelectiveConnectionEstablishProcedure(
			ScanType scanType,
			ushort scanInterval,
			ushort scanWindow,
			AddressType ownAddressType,
			FilterPolicy scanningFilterPolicy,
			bool filterDuplicates,
			byte whitelistEntryCount,
			DeviceAddressEntry[] whitelistEntries
		)
		{
			throw new NotImplementedException();
		}

		public BleStatus CreateConnection(
			ushort scanInterval,
			ushort scanWindow,
			AddressType peerAddressType,
			byte[] peerAddress,
			AddressType ownAddressType,
			ushort minimumConnectionInterval,
			ushort maximumConnectionInterval,
			ushort connectionLatency,
			ushort supervisionTimeout,
			ushort minimumConnectionLength,
			ushort maximumConnectionLength
		)
		{
			throw new NotImplementedException();
		}

		public BleStatus TerminateGapProcedure(ProcedureCode procedureCode)
		{
			throw new NotImplementedException();
		}

		public BleStatus StartConnectionUpdate(
			ushort connectionHandle,
			ushort minimumConnectionInterval,
			ushort maximumConnectionInterval,
			ushort connectionLatency,
			ushort supervisionTimeout,
			ushort minimumConnectionLength,
			ushort maximumConnectionLength
		)
		{
			throw new NotImplementedException();
		}

		public BleStatus SendPairingRequest(ushort connectionHandle, bool forceReBond)
		{
			throw new NotImplementedException();
		}

		public BleStatus ResolvePrivateAddress(byte[] address, ref byte[] actualAddress)
		{
			throw new NotImplementedException();
		}

		public BleStatus SetBroadcastMode(
			ushort minimumAdvertisingInterval,
			ushort maximumAdvertisingInterval,
			AdvertisingType advertisingType,
			AddressType ownAddressType,
			byte advertisingDataLength,
			byte[] advertisingData,
			byte whitelistEntryCount,
			DeviceAddressEntry[] whitelistEntries
		)
		{
			throw new NotImplementedException();
		}

		public BleStatus StartObservationProcedure(
			ushort scanInterval,
			ushort scanWindow,
			ScanType scanType,
			AddressType ownAddressType,
			bool filterDuplicates,
			FilterPolicy scanningFilterPolicy
		)
		{
			throw new NotImplementedException();
		}

		public BleStatus GetBondedDevices(
			ref byte addressCount,
			ref DeviceAddressEntry[] bondedDeviceAddresses
		)
		{
			throw new NotImplementedException();
		}

		public BleStatus IsDeviceBonded(AddressType peerAddressType, byte[] peerAddress)
		{
			throw new NotImplementedException();
		}

		public BleStatus NumericComparisonValueConfirmYesNo(ushort connectionHandle, bool confirmYesNo)
		{
			throw new NotImplementedException();
		}

		public BleStatus PasskeyInput(ushort connectionHandle, KeyPressNotificationType inputType)
		{
			throw new NotImplementedException();
		}

		public BleStatus GetOobData(
			OobDataType oobDataType,
			ref AddressType addressType,
			ref byte[] address,
			ref byte oobDataLength,
			ref byte[] oobData)
		{
			throw new NotImplementedException();
		}

		public BleStatus SetOobData(
			byte deviceType,
			AddressType addressType,
			byte[] address,
			OobDataType oobDataType,
			byte oobDataLength,
			byte[] oobData)
		{
			throw new NotImplementedException();
		}

		public BleStatus AddDeviceToResolvingList(
			byte resolvingListEntriesCount,
			DeviceAddressEntry[] whiteListIdentityEntry,
			bool clearResolvingList
		)
		{
			throw new NotImplementedException();
		}

		public BleStatus RemoveBondedDevice(
			AddressType peerIdentityAddressType,
			byte[] peerIdentityAddress
		)
		{
			throw new NotImplementedException();
		}
	}

	public enum OobDataType : byte
	{
		TemporaryKey = 0x00,
		RandomValue = 0x01,
		ConfirmValue = 0x02
	}

	public struct DeviceAddressEntry
	{
		public AddressType AddressType;
		public byte[] Address;
	}

	public enum Reason : byte
	{
		AuthenticationFailure = 0x05,
		RemoteUserTerminatedConnection = 0x13,
		RemoteDeviceTerminatedConnectionDueToLowResources = 0x14,
		RemoteDeviceTerminatedConnectionDueToPowerOff = 0x15,
		UnsupportedRemoteFeature = 0x1A,
		UnacceptableConnectionParameters = 0x3B
	}

	[Flags]
	public enum Role : byte
	{
		Peripheral = 0x01,
		BroadCaster = 0x02,
		Central = 0x04,
		Observer = 0x08
	}

	public enum SecureConnectionSupport : byte
	{
		NotSupported = 0x00,
		Supported = 0x01,
		Mandatory = 0x02
	}

	public enum IoCapability : byte
	{
		DisplayOnly = 0x00,
		DisplayYesNo = 0x01,
		KeyboardOnly = 0x02,
		NoInputNoOutput = 0x03,
		KeyboardDisplay = 0x04
	}
}
