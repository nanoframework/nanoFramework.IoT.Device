using System;

namespace BlueNrg2.Aci
{
	public class Gatt
	{
		[Flags]
		public enum Event : uint
		{
			AttributeModifiedEvent = 0x00000001,
			ProcTimeoutEvent = 0x00000002,
			ExchangeMtuRespEvent = 0x00000004,
			FindInfoRespEvent = 0x00000008,
			FindByTypeValueRespEvent = 0x00000010,
			ReadByTypeRespEvent = 0x00000020,
			ReadRespEvent = 0x00000040,
			ReadBlobRespEvent = 0x00000080,
			ReadMultipleRespEvent = 0x00000100,
			ReadByGroupTypeRespEvent = 0x00000200,
			PrepareWriteRespEvent = 0x00000800,
			ExecWriteRespEvent = 0x00001000,
			IndicationEvent = 0x00002000,
			NotificationEvent = 0x00004000,
			ErrorRespEvent = 0x00008000,
			ProcCompleteEvent = 0x00010000,
			DiscReadCharByUuidRespEvent = 0x00020000,
			TxPoolAvailableEvent = 0x00040000
		}

		[Flags]
		public enum EventMask : byte
		{
			DontNotifyEvents = 0x00,
			NotifyAttributeWrite = 0x01,
			NotifyWriteRequestAndWaitForApprovalResponse = 0x02,
			NotifyReadRequestAndWaitForApprovalResponse = 0x04
		}

		private readonly TransportLayer _transportLayer;

		public Gatt(TransportLayer transportLayer)
		{
			_transportLayer = transportLayer;
		}

		public BleStatus Init()
		{
			var status = new byte[1];
			var rq = new Request
			{
				OpCodeGroup = 0x3f,
				OpCodeCommand = 0x101,
				ResponseParameter = status,
				ResponseLength = 1
			};
			if (_transportLayer.SendRequest(ref rq, false) < 0)
				return BleStatus.Timeout;
			return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
		}

		public BleStatus AddService(
			UuidType serviceUuidType,
			byte[] serviceUuid,
			ServiceType serviceType,
			byte maximumAttributeRecords,
			ref ushort serviceHandle
		)
		{
			var ptr = 0;
			var uuidSize = serviceUuidType switch
			{
				UuidType.Uuid16 => 2, UuidType.Uuid128 => 16, _ => 0
			};
			if (serviceUuid is null || serviceUuid.Length != uuidSize)
				throw new ArgumentException();
			var command = new byte[3 + uuidSize];
			command[ptr] = (byte)serviceUuidType;
			ptr += 1;
			serviceUuid.CopyTo(command, ptr);
			ptr += uuidSize;
			command[ptr] = (byte)serviceType;
			ptr += 1;
			command[ptr] = maximumAttributeRecords;
			ptr += 1;

			var response = new byte[3];
			var rq = new Request
			{
				OpCodeGroup = 0x3f,
				OpCodeCommand = 0x102,
				CommandParameter = command,
				CommandLength = (uint)ptr,
				ResponseParameter = response,
				ResponseLength = 3
			};
			if (_transportLayer.SendRequest(ref rq, false) < 0)
				return BleStatus.Timeout;
			if (response[0] != 0)
				return (BleStatus)response[0];
			serviceHandle = BitConverter.ToUInt16(response, 1);
			return BleStatus.Success;
		}

		public BleStatus IncludeService(
			ushort serviceHandle,
			ushort includeStartHandle,
			ushort includeEndHandle,
			UuidType includeUuidType,
			byte[] includeUuid,
			ref ushort includeHandle
		)
		{
			var uuidSize = includeUuidType switch
			{
				UuidType.Uuid16 => 2, UuidType.Uuid128 => 16, _ => throw new ArgumentOutOfRangeException(nameof(includeUuidType))
			};

			if (includeUuid is null || includeUuid.Length != uuidSize)
				throw new ArgumentException();

			var command = new byte[7 + uuidSize];
			BitConverter.GetBytes(serviceHandle).CopyTo(command, 0);
			BitConverter.GetBytes(includeStartHandle).CopyTo(command, 2);
			BitConverter.GetBytes(includeEndHandle).CopyTo(command, 4);
			command[6] = (byte)includeUuidType;
			includeUuid.CopyTo(command, 7);

			var response = new byte[3];
			var rq = new Request
			{
				OpCodeGroup = 0x3f,
				OpCodeCommand = 0x103,
				CommandParameter = command,
				CommandLength = (uint)(7 + uuidSize),
				ResponseParameter = response,
				ResponseLength = 3
			};
			if (_transportLayer.SendRequest(ref rq, false) < 0)
				return BleStatus.Timeout;
			if (response[0] != 0)
				return (BleStatus)response[0];
			includeHandle = BitConverter.ToUInt16(response, 1);
			return BleStatus.Success;
		}

		public BleStatus AddCharacteristic(
			ushort serviceHandle,
			UuidType characteristicUuidType,
			byte[] characteristicUuid,
			ushort characteristicValueLength,
			CharacteristicProperties characteristicProperties,
			SecurityPermissions securityPermissions,
			EventMask eventMask,
			byte encryptionKeySize,
			bool hasVariableLength,
			ref ushort characteristicHandle
		)
		{
			var uuidSize = characteristicUuidType switch
			{
				UuidType.Uuid16 => 2, UuidType.Uuid128 => 16, _ => throw new ArgumentOutOfRangeException(nameof(characteristicUuidType))
			};

			if (characteristicUuid is null || characteristicUuid.Length != uuidSize)
				throw new ArgumentException();

			var ptr = 0;
			var command = new byte[10 + uuidSize];
			BitConverter.GetBytes(serviceHandle).CopyTo(command, ptr);
			ptr += 2;
			command[ptr] = (byte)characteristicUuidType;
			characteristicUuid.CopyTo(command, ptr);
			ptr += uuidSize;
			BitConverter.GetBytes(characteristicValueLength).CopyTo(command, ptr);
			ptr += 2;
			command[ptr] = (byte)characteristicProperties;
			ptr += 1;
			command[ptr] = (byte)securityPermissions;
			ptr += 1;
			command[ptr] = (byte)eventMask;
			ptr += 1;
			command[ptr] = encryptionKeySize;
			ptr += 1;
			command[ptr] = (byte)(hasVariableLength ? 0x01 : 0x00);
			ptr += 1;

			var response = new byte[3];
			var rq = new Request
			{
				OpCodeGroup = 0x3f,
				OpCodeCommand = 0x104,
				CommandParameter = command,
				CommandLength = (uint)ptr,
				ResponseParameter = response,
				ResponseLength = 3
			};
			if (_transportLayer.SendRequest(ref rq, false) < 0)
				return BleStatus.Timeout;
			if (response[0] != 0)
				return (BleStatus)response[0];
			characteristicHandle = BitConverter.ToUInt16(response, 1);
			return BleStatus.Success;
		}

		public BleStatus AddCharacteristicDescriptor(
			ushort serviceHandle,
			ushort characteristicHandle,
			UuidType characteristicDescriptorUuidType,
			byte[] characteristicDescriptorUuid,
			byte characteristicDescriptorMaximumLength,
			byte[] characteristicDescriptorValue,
			SecurityPermissions securityPermissions,
			AccessPermissions accessPermissions,
			EventMask eventMask,
			byte encryptionKeySize,
			bool hasVariableLength,
			ref ushort characteristicDescriptorHandle)
		{
			throw new NotImplementedException();
		}

		public BleStatus UpdateCharacteristicValue(
			ushort serviceHandle,
			ushort characteristicHandle,
			byte valueOffset,
			byte characteristicValueLength,
			byte[] characteristicValue)
		{
			throw new NotImplementedException();
		}

		public BleStatus DeleteCharacteristic(ushort serviceHandle, ushort characteristicHandle)
		{
			throw new NotImplementedException();
		}

		public BleStatus DeleteService(ushort serviceHandle)
		{
			throw new NotImplementedException();
		}

		public BleStatus DeleteIncludeService(ushort serviceHandle, ushort includeHandle)
		{
			throw new NotImplementedException();
		}

		public BleStatus SetEventMask(Event eventMask)
		{
			throw new NotImplementedException();
		}

		public BleStatus ExchangeConfiguration(ushort connectionHandle)
		{
			throw new NotImplementedException();
		}

		public BleStatus FindInformationRequest(ushort connectionHandle, ushort startHandle, ushort endHandle)
		{
			throw new NotImplementedException();
		}

		public BleStatus FindByTypeValueRequest(
			ushort connectionHandle,
			ushort startHandle,
			ushort endHandle,
			ushort uuid,
			byte attributeValueLength,
			byte[] attributeValue)
		{
			throw new NotImplementedException();
		}

		public BleStatus ReadByTypeRequest(ushort connectionHandle, ushort startHandle, ushort endHandle, UuidType uuidType, ref byte[] uuid)
		{
			throw new NotImplementedException();
		}

		public BleStatus ReadByGroupTypeRequest(ushort connectionHandle, ushort startHandle, ushort endHandle, UuidType uuidType, ref byte[] uuid)
		{
			throw new NotImplementedException();
		}

		public BleStatus PrepareWriteRequest(
			ushort connectionHandle,
			ushort attributeHandle,
			ushort valueOffset,
			byte attributeValueLength,
			byte[] attributeValue)
		{
			throw new NotImplementedException();
		}

		/// <param name="connectionHandle">identifies the connection written to</param>
		/// <param name="executeAllWrites">
		///     If set to true, all pending prepared values are written immediately, if set to false all
		///     prepared writes will be cancelled.
		/// </param>
		public BleStatus ExecuteWriteRequest(ushort connectionHandle, bool executeAllWrites)
		{
			throw new NotImplementedException();
		}

		public BleStatus DiscoverAllPrimaryServices(ushort connectionHandle)
		{
			throw new NotImplementedException();
		}

		public BleStatus DiscoverAllPrimaryServicesByUuid(ushort connectionHandle, UuidType uuidType, byte[] uuid)
		{
			throw new NotImplementedException();
		}

		public BleStatus FindIncludedServices(ushort connectionHandle, ushort startHandle, ushort endHandle)
		{
			throw new NotImplementedException();
		}

		public BleStatus DiscoverAllCharacteristicsOfService(ushort connectionHandle, ushort startHandle, ushort endHandle)
		{
			throw new NotImplementedException();
		}

		public BleStatus DiscoverAllCharacteristicsByUuid(
			ushort connectionHandle,
			ushort startHandle,
			ushort endHandle,
			UuidType uuidType,
			byte[] uuid)
		{
			throw new NotImplementedException();
		}

		public BleStatus DiscoverAllCharacteristicDescriptors(ushort connectionHandle, ushort startHandle, ushort endHandle)
		{
			throw new NotImplementedException();
		}

		public BleStatus ReadCharacteristicValue(ushort connectionHandle, ushort attributeHandle)
		{
			throw new NotImplementedException();
		}

		public BleStatus ReadUsingCharacteristicUuid(ushort connectionHandle, ushort startHandle, ushort endHandle, UuidType uuidType, byte[] uuid)
		{
			throw new NotImplementedException();
		}

		public BleStatus ReadLongCharacteristicValue(ushort connectionHandle, ushort attributeHandle, ushort valueOffset)
		{
			throw new NotImplementedException();
		}

		public BleStatus ReadMultipleCharacteristicValues(ushort connectionHandle, byte handleCount, ushort[] handles)
		{
			throw new NotImplementedException();
		}

		public BleStatus WriteCharacteristicValue(ushort connectionHandle, ushort attributeHandle, byte attributeValueLength, byte[] attributeValue)
		{
			throw new NotImplementedException();
		}

		public BleStatus WriteLongCharacteristicValue(
			ushort connectionHandle,
			ushort attributeHandle,
			ushort valueOffset,
			byte attributeValueLength,
			byte[] attributeValue)
		{
			throw new NotImplementedException();
		}

		public BleStatus WriteCharacteristicReliably(
			ushort connectionHandle,
			ushort attributeHandle,
			ushort valueOffset,
			byte attributeValueLength,
			byte[] attributeValue)
		{
			throw new NotImplementedException();
		}

		private BleStatus WriteLongCharacteristicDescriptor(
			ushort connectionHandle,
			ushort attributeHandle,
			ushort valueOffset,
			byte attributeValueLength,
			byte[] attributeValue)
		{
			throw new NotImplementedException();
		}

		private BleStatus ReadLongCharacteristicDescriptor(
			ushort connectionHandle,
			ushort attributeHandle,
			ushort valueOffset)
		{
			throw new NotImplementedException();
		}

		private BleStatus WriteCharacteristicDescriptor(
			ushort connectionHandle,
			ushort attributeHandle,
			byte attributeValueLength,
			byte[] attributeValue)
		{
			throw new NotImplementedException();
		}

		private BleStatus ReadCharacteristicDescriptor(
			ushort connectionHandle,
			ushort attributeHandle)
		{
			throw new NotImplementedException();
		}

		private BleStatus WriteWithoutResponse(
			ushort connectionHandle,
			ushort attributeHandle,
			byte attributeValueLength,
			byte[] attributeValue)
		{
			throw new NotImplementedException();
		}

		private BleStatus SignedWriteWithoutResponse(
			ushort connectionHandle,
			ushort attributeHandle,
			byte attributeValueLength,
			byte[] attributeValue)
		{
			throw new NotImplementedException();
		}

		private BleStatus ConfirmIndication(ushort connectionHandle)
		{
			throw new NotImplementedException();
		}

		private BleStatus WriteResponse(
			ushort connectionHandle,
			ushort attributeHandle,
			byte writeStatus,
			byte errorCode,
			byte attributeValueLength,
			byte[] attributeValue)
		{
			throw new NotImplementedException();
		}

		private BleStatus AllowRead(ushort connectionHandle)
		{
			throw new NotImplementedException();
		}

		private BleStatus SetSecurityPermission(
			ushort serviceHandle,
			ushort attributeHandle,
			byte securityPermissions)
		{
			throw new NotImplementedException();
		}

		private BleStatus SetDescriptorValue(
			ushort serviceHandle,
			ushort characteristicHandle,
			ushort characteristicDescriptorHandle,
			ushort valueOffset,
			byte characteristicDescriptorValueLength,
			byte[] characteristicDescriptorValue)
		{
			throw new NotImplementedException();
		}

		private BleStatus ReadHandleValue(
			ushort attributeHandle,
			ushort offset,
			ushort valueLengthRequested,
			ref ushort length,
			ref ushort valueLength,
			ref byte[] value)
		{
			throw new NotImplementedException();
		}

		public BleStatus UpdateCharacteristicValueExtended(
			ushort connectionHandleToNotify,
			ushort serviceHandle,
			ushort characteristicHandle,
			UpdateType updateType,
			ushort charLength,
			ushort valueOffset,
			byte valueLength,
			byte[] value)
		{
			throw new NotImplementedException();
		}

		public BleStatus DenyRead(
			ushort connectionHandle,
			byte errorCode)
		{
			throw new NotImplementedException();
		}

		public BleStatus SetAccessPermission(
			ushort serviceHandle,
			ushort attributeHandle,
			AccessPermissions accessPermissions)
		{
			throw new NotImplementedException();
		}
	}

	[Flags]
	public enum UpdateType : byte
	{
		LocalUpdate = 0x00,
		Notification = 0x01,
		Indication = 0x02,
		DisableRetransmission = 0x04
	}

	[Flags]
	public enum AccessPermissions : byte
	{
		None = 0x00,
		Read = 0x01,
		Write = 0x02,
		WriteWithoutResponse = 0x04,
		SignedWrite = 0x08
	}

	[Flags]
	public enum SecurityPermissions : byte
	{
		None = 0x00,
		AuthenticatedRead = 0x01,
		AuthorizedRead = 0x02,
		EncryptedRead = 0x04,
		AuthenticatedWrite = 0x08,
		AuthorizedWrite = 0x10,
		EncryptedWrite = 0x20
	}

	[Flags]
	public enum CharacteristicProperties : byte
	{
		None = 0x00,
		Broadcast = 0x01,
		Read = 0x02,
		WriteWithoutResponse = 0x04,
		Write = 0x08,
		Notify = 0x10,
		Indicate = 0x20,
		SignedWrite = 0x40,
		Extended = 0x80
	}

	public enum ServiceType
	{
		Primary = 0x01,
		Secondary = 0x02
	}

	public enum UuidType : byte
	{
		Uuid16 = 0x01,
		Uuid128 = 0x02
	}
}
