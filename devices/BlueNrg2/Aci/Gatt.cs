﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci
{
    internal class Gatt
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

        internal Gatt(TransportLayer transportLayer)
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

        /// <summary>
        /// Add a service to GATT Server. When a service is created in the server,
        /// the host needs to reserve the handle ranges for this service using
        /// Max_Attribute_Records parameter. This parameter specifies the maximum number
        /// of attribute records that can be added to this service (including the service attribute,
        /// include attribute, characteristic attribute, characteristic value attribute and
        /// characteristic descriptor attribute). Handle of the created service is returned
        /// in command complete event. Service declaration is taken from the service pool.
        /// The attributes for characteristics and descriptors are allocated from the attribute pool.
        /// </summary>
        /// <param name="serviceUuidType">The type of the service UUID.</param>
        /// <param name="serviceUuid">The service UUID.</param>
        /// <param name="serviceType">The service type.</param>
        /// <param name="maximumAttributeRecords">Maximum number of attribute records that can be added to this service.</param>
        /// <param name="serviceHandle">
        /// Handle of the Service. When this service is added, a handle is allocated by the server
        /// for this service. Server also allocates a range of handles for this service from serviceHandle to
        /// <code>serviceHandle + maximumAttributeRecords - 1</code>
        /// </param>
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

        /// <summary>Add a characteristic to a service.</summary>
        /// <param name="serviceHandle">
        /// Handle of the Service to which the characteristic will be added.
        ///        Values:
        ///        - 0x0001 ... 0xFFFF
        /// </param>
        /// <param name="characteristicUuidType">UUID type.</param>
        /// <param name="characteristicUuid">Uuid of the characteristic.</param>
        /// <param name="characteristicValueLength">
        /// Maximum length of the characteristic value.
        ///        Values:
        ///        - 0 ... 512
        /// </param>
        /// <param name="characteristicProperties">
        /// Characteristic Properties (Volume 3, Part G, section 3.3.1.1 of Bluetooth Specification 4.1)
        /// </param>
        /// <param name="securityPermissions">Security permission flags.</param>
        /// <param name="eventMask">GATT event mask.</param>
        /// <param name="encryptionKeySize">
        /// Minimum encryption key size required to read the characteristic.
        ///        Values:
        ///        - 0x07 ... 0x10
        /// </param>
        /// <param name="hasVariableLength">
        /// Specify if the characteristic value has a fixed length or a variable length.
        /// </param>
        /// <param name="characteristicHandle">
        /// Char_Handle Handle of the Characteristic that has been added.
        /// It is the handle of the characteristic declaration.
        /// The attribute that holds the characteristic value is allocated at the next handle,
        /// followed by the Client Characteristic Configuration descriptor if the characteristic
        /// has Notify or Indicate properties.
        /// </param>
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

        public BleStatus WriteLongCharacteristicDescriptor(
            ushort connectionHandle,
            ushort attributeHandle,
            ushort valueOffset,
            byte attributeValueLength,
            byte[] attributeValue)
        {
            throw new NotImplementedException();
        }

        public BleStatus ReadLongCharacteristicDescriptor(
            ushort connectionHandle,
            ushort attributeHandle,
            ushort valueOffset)
        {
            throw new NotImplementedException();
        }

        public BleStatus WriteCharacteristicDescriptor(
            ushort connectionHandle,
            ushort attributeHandle,
            byte attributeValueLength,
            byte[] attributeValue)
        {
            throw new NotImplementedException();
        }

        public BleStatus ReadCharacteristicDescriptor(
            ushort connectionHandle,
            ushort attributeHandle)
        {
            throw new NotImplementedException();
        }

        public BleStatus WriteWithoutResponse(
            ushort connectionHandle,
            ushort attributeHandle,
            byte attributeValueLength,
            byte[] attributeValue)
        {
            throw new NotImplementedException();
        }

        public BleStatus SignedWriteWithoutResponse(
            ushort connectionHandle,
            ushort attributeHandle,
            byte attributeValueLength,
            byte[] attributeValue)
        {
            throw new NotImplementedException();
        }

        public BleStatus ConfirmIndication(ushort connectionHandle)
        {
            throw new NotImplementedException();
        }

        public BleStatus WriteResponse(
            ushort connectionHandle,
            ushort attributeHandle,
            byte writeStatus,
            byte errorCode,
            byte attributeValueLength,
            byte[] attributeValue)
        {
            throw new NotImplementedException();
        }

        public BleStatus AllowRead(ushort connectionHandle)
        {
            throw new NotImplementedException();
        }

        public BleStatus SetSecurityPermission(
            ushort serviceHandle,
            ushort attributeHandle,
            byte securityPermissions)
        {
            throw new NotImplementedException();
        }

        public BleStatus SetDescriptorValue(
            ushort serviceHandle,
            ushort characteristicHandle,
            ushort characteristicDescriptorHandle,
            ushort valueOffset,
            byte characteristicDescriptorValueLength,
            byte[] characteristicDescriptorValue)
        {
            throw new NotImplementedException();
        }

        public BleStatus ReadHandleValue(
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
    internal enum UpdateType : byte
    {
        LocalUpdate = 0x00,
        Notification = 0x01,
        Indication = 0x02,
        DisableRetransmission = 0x04
    }

    [Flags]
    internal enum AccessPermissions : byte
    {
        None = 0x00,
        Read = 0x01,
        Write = 0x02,
        WriteWithoutResponse = 0x04,
        SignedWrite = 0x08
    }

    [Flags]
    internal enum SecurityPermissions : byte
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
