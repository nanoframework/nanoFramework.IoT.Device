// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.BlueNrg2.Aci.Events;

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Class containing Gatt commands.
    /// </summary>
    public class Gatt
    {
        private readonly TransportLayer _transportLayer;

        internal Gatt(TransportLayer transportLayer)
        {
            _transportLayer = transportLayer;
        }

        /// <summary>
        /// Initialize the GATT layer for server and client roles.  It adds also
        /// the GATT service with Service Changed Characteristic.  Until this
        /// command is issued the GATT channel will not process any commands even
        /// if the  connection is opened. This command has to be given before
        /// using any of the GAP features.
        /// </summary>
        /// <returns>Value indicating success or error code.</returns>
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
            return status[0] != 0 ? (BleStatus) status[0] : BleStatus.Success;
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
            command[ptr] = (byte) serviceUuidType;
            ptr += 1;
            serviceUuid.CopyTo(command, ptr);
            ptr += uuidSize;
            command[ptr] = (byte) serviceType;
            ptr += 1;
            command[ptr] = maximumAttributeRecords;
            ptr += 1;

            var response = new byte[3];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x102,
                CommandParameter = command,
                CommandLength = (uint) ptr,
                ResponseParameter = response,
                ResponseLength = 3
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus) response[0];
            serviceHandle = BitConverter.ToUInt16(response, 1);
            return BleStatus.Success;
        }

        /// <summary>
        /// Include a service given by <see cref="includeStartHandle"/> and <see cref="includeEndHandle"/>
        /// to another  service given by <see cref="serviceHandle"/>. Attribute server creates
        /// an INCLUDE definition  attribute and return the handle of this
        /// attribute in <see cref="includeHandle"/>.
        /// </summary>
        /// <param name="serviceHandle">Handle of the Service to which another service has to
        /// be included.</param>
        /// <param name="includeStartHandle">Attribute Handle of the Service which has to be included in service.</param>
        /// <param name="includeEndHandle">End Handle of the Service which has to be included in service.</param>
        /// <param name="includeUuidType">UUID type.</param>
        /// <param name="includeUuid">UUID of service to include.</param>
        /// <param name="includeHandle">Handle of the include declaration.</param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="ArgumentException"><see cref="includeUuid"/> has to be of a length that complies with <see cref="includeUuidType"/></exception>
        public BleStatus IncludeService(
            ushort serviceHandle,
            ushort includeStartHandle,
            ushort includeEndHandle,
            UuidType includeUuidType,
            byte[] includeUuid,
            ref ushort includeHandle
        )
        {
            var uuidSize = includeUuidType == UuidType.Uuid16 ? 2 : 16;

            if (includeUuid is null || includeUuid.Length != uuidSize)
                throw new ArgumentException();

            var command = new byte[7 + uuidSize];
            BitConverter.GetBytes(serviceHandle).CopyTo(command, 0);
            BitConverter.GetBytes(includeStartHandle).CopyTo(command, 2);
            BitConverter.GetBytes(includeEndHandle).CopyTo(command, 4);
            command[6] = (byte) includeUuidType;
            includeUuid.CopyTo(command, 7);

            var response = new byte[3];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x103,
                CommandParameter = command,
                CommandLength = (uint) (7 + uuidSize),
                ResponseParameter = response,
                ResponseLength = 3
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus) response[0];
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
        /// <param name="characteristicValueLength">Maximum length of the characteristic value.</param>
        /// <param name="characteristicProperties">Characteristic Properties (Volume 3, Part G, section 3.3.1.1 of Bluetooth Specification 4.1)</param>
        /// <param name="securityPermissions">Security permission flags.</param>
        /// <param name="characteristicEventMask">GATT event mask.</param>
        /// <param name="encryptionKeySize">Minimum encryption key size required to read the characteristic.</param>
        /// <param name="hasVariableLength">Specify if the characteristic value has a fixed length or a variable length.</param>
        /// <param name="characteristicHandle">
        /// Characteristic handle of the Characteristic that has been added.
        /// It is the handle of the characteristic declaration.
        /// The attribute that holds the characteristic value is allocated at the next handle,
        /// followed by the Client Characteristic Configuration descriptor if the characteristic
        /// has Notify or Indicate properties.
        /// </param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="ArgumentException"><see cref="characteristicUuid"/> has to be of a length that complies with <see cref="characteristicUuidType"/></exception>
        public BleStatus AddCharacteristic(
            ushort serviceHandle,
            UuidType characteristicUuidType,
            byte[] characteristicUuid,
            ushort characteristicValueLength,
            CharacteristicProperties characteristicProperties,
            SecurityPermissions securityPermissions,
            CharacteristicEventMask characteristicEventMask,
            byte encryptionKeySize,
            bool hasVariableLength,
            ref ushort characteristicHandle
        )
        {
            var uuidSize = characteristicUuidType == UuidType.Uuid16 ? 2 : 16;

            if (characteristicUuid is null || characteristicUuid.Length != uuidSize)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[10 + uuidSize];
            BitConverter.GetBytes(serviceHandle).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte) characteristicUuidType;
            characteristicUuid.CopyTo(command, ptr);
            ptr += uuidSize;
            BitConverter.GetBytes(characteristicValueLength).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte) characteristicProperties;
            ptr += 1;
            command[ptr] = (byte) securityPermissions;
            ptr += 1;
            command[ptr] = (byte) characteristicEventMask;
            ptr += 1;
            command[ptr] = encryptionKeySize;
            ptr += 1;
            command[ptr] = (byte) (hasVariableLength ? 0x01 : 0x00);
            ptr += 1;

            var response = new byte[3];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x104,
                CommandParameter = command,
                CommandLength = (uint) ptr,
                ResponseParameter = response,
                ResponseLength = 3
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus) response[0];
            characteristicHandle = BitConverter.ToUInt16(response, 1);
            return BleStatus.Success;
        }

        /// <summary>
        /// Add a characteristic descriptor to a service.
        /// </summary>
        /// <param name="serviceHandle">Handle of service to which the characteristic belongs.</param>
        /// <param name="characteristicHandle">Handle of the characteristic to which description has to be added.</param>
        /// <param name="characteristicDescriptorUuidType">UUID type.</param>
        /// <param name="characteristicDescriptorUuid">UUID of characteristic descriptor.</param>
        /// <param name="characteristicDescriptorValueMaximumLength">The maximum length of the descriptor value.</param>
        /// <param name="characteristicDescriptorValueLength">Current Length of the characteristic descriptor value</param>
        /// <param name="characteristicDescriptorValue">Value of the characteristic description.</param>
        /// <param name="securityPermissions">Security permission flags.</param>
        /// <param name="accessPermissions">Access permission.</param>
        /// <param name="characteristicEventMask">GATT event mask.</param>
        /// <param name="encryptionKeySize">Minimum encryption key size required to read the characteristic.</param>
        /// <param name="hasVariableLength">Specify if the characteristic value has a fixed length or a variable length.</param>
        /// <param name="characteristicDescriptorHandle">Handle of the characteristic descriptor.</param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="ArgumentException"><see cref="characteristicDescriptorUuid"/> has to be of length as indicated by <see cref="characteristicDescriptorUuidType"/>.</exception>
        /// <exception cref="ArgumentNullException"><see cref="characteristicDescriptorValue"/> has to be of length as indicated by <see cref="characteristicDescriptorValueLength"/>.</exception>
        public BleStatus AddCharacteristicDescriptor(
            ushort serviceHandle,
            ushort characteristicHandle,
            UuidType characteristicDescriptorUuidType,
            byte[] characteristicDescriptorUuid,
            byte characteristicDescriptorValueMaximumLength,
            byte characteristicDescriptorValueLength,
            byte[] characteristicDescriptorValue,
            SecurityPermissions securityPermissions,
            AccessPermissions accessPermissions,
            CharacteristicEventMask characteristicEventMask,
            byte encryptionKeySize,
            bool hasVariableLength,
            ref ushort characteristicDescriptorHandle)
        {
            var uuidSize = characteristicDescriptorUuidType == UuidType.Uuid16 ? 2 : 16;

            if (characteristicDescriptorUuid is null || characteristicDescriptorUuid.Length != uuidSize)
                throw new ArgumentException();

            if (characteristicDescriptorValue is null ||
                characteristicDescriptorValue.Length != characteristicDescriptorValueLength)
                throw new ArgumentNullException();

            var ptr = 0;
            var command = new byte[12 + uuidSize + characteristicDescriptorValueLength];
            BitConverter.GetBytes(serviceHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(characteristicHandle).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte) characteristicDescriptorUuidType;
            ptr += 1;
            characteristicDescriptorUuid.CopyTo(command, ptr);
            ptr += uuidSize;
            command[ptr] = characteristicDescriptorValueMaximumLength;
            ptr += 1;
            command[ptr] = characteristicDescriptorValueLength;
            ptr += 1;
            characteristicDescriptorValue.CopyTo(command, ptr);
            ptr += characteristicDescriptorValueLength;
            command[ptr] = (byte) securityPermissions;
            ptr += 1;
            command[ptr] = (byte) accessPermissions;
            ptr += 1;
            command[ptr] = (byte) characteristicEventMask;
            ptr += 1;
            command[ptr] = encryptionKeySize;
            ptr += 1;
            command[ptr] = (byte) (hasVariableLength ? 0x01 : 0x00);
            ptr += 1;

            var response = new byte[3];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x105,
                CommandParameter = command,
                CommandLength = (uint) ptr,
                ResponseParameter = response,
                ResponseLength = 3
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus) response[0];
            characteristicDescriptorHandle = BitConverter.ToUInt16(response, 1);
            return BleStatus.Success;
        }

        /// <summary>
        /// Update a characteristic value in a service.  If notifications (or
        /// indications) are enabled on that characteristic,  a notification (or
        /// indication) will be sent to the client after sending  this command to
        /// the BlueNRG. The command is queued into the BlueNRG command queue.  If
        /// the buffer is full, because previous commands could not be still
        /// processed, the function will return <see cref="BleStatus.InsufficientResources"/>.
        /// This will happen  if notifications (or indications) are enabled and
        /// the application calls <see cref="UpdateCharacteristicValue"/> at an higher
        /// rate than what is allowed by the link.  Throughput on BLE link depends
        /// on connection interval and connection length  parameters (decided by
        /// the master, see <see cref="L2Cap.ConnectionParameterUpdateRequest"/> for
        /// more info on how to suggest new connection parameters from a slave).
        /// If the  application does not want to lose notifications because
        /// BlueNRG buffer becomes full, it has to retry again till the function
        /// returns <see cref="BleStatus.Success"/> or any other error code.
        /// </summary>
        /// <param name="serviceHandle">Handle of service to which the characteristic belongs.</param>
        /// <param name="characteristicHandle">Handle of the characteristic.</param>
        /// <param name="valueOffset">The offset from which the attribute value has to be
        /// updated.  If this is set to 0 and the attribute value is of variable
        /// length, then the length of the attribute will be set to the
        /// <see cref="characteristicValueLength"/>.  If the <see cref="valueOffset"/> is set to a value greater than
        /// 0, then the length of the attribute will be set to the maximum length
        /// as  specified for the attribute while adding the characteristic.</param>
        /// <param name="characteristicValueLength">Length of the characteristic value in octets.</param>
        /// <param name="characteristicValue">Characteristic value.</param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="ArgumentException"><see cref="characteristicValue"/> has to be of length as indicated by <see cref="characteristicValueLength"/>.</exception>
        [Obsolete("still supported but not recommended")]
        public BleStatus UpdateCharacteristicValue(
            ushort serviceHandle,
            ushort characteristicHandle,
            byte valueOffset,
            byte characteristicValueLength,
            byte[] characteristicValue)
        {
            if (characteristicValue is null ||
                characteristicValue.Length != characteristicValueLength)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[10 + characteristicValueLength];
            BitConverter.GetBytes(serviceHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(characteristicHandle).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = valueOffset;
            ptr += 1;
            command[ptr] = characteristicValueLength;
            ptr += 1;
            characteristicValue.CopyTo(command, ptr);
            ptr += characteristicValueLength;

            var response = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x106,
                CommandParameter = command,
                CommandLength = (uint) ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus) response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Delete the specified characteristic from the service.
        /// </summary>
        /// <param name="serviceHandle">Handle of service to which the characteristic belongs.</param>
        /// <param name="characteristicHandle">Handle of the characteristic which has to be deleted.</param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public BleStatus DeleteCharacteristic(ushort serviceHandle, ushort characteristicHandle)
        {
            var ptr = 0;
            var command = new byte[4];
            BitConverter.GetBytes(serviceHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(characteristicHandle).CopyTo(command, ptr);
            ptr += 2;

            var response = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x107,
                CommandParameter = command,
                CommandLength = (uint) ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus) response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Delete the specified service from the GATT server database.
        /// </summary>
        /// <param name="serviceHandle">Handle of the service to be deleted.</param>
        /// <returns>Value indicating success or error code.</returns>
        public BleStatus DeleteService(ushort serviceHandle)
        {
            var ptr = 0;
            var command = new byte[2];
            BitConverter.GetBytes(serviceHandle).CopyTo(command, ptr);
            ptr += 2;

            var response = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x108,
                CommandParameter = command,
                CommandLength = (uint) ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus) response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Delete the Include definition from the service.
        /// </summary>
        /// <param name="serviceHandle">Handle of the service to which the include service belongs.</param>
        /// <param name="includeHandle">Handle of the included service which has to be deleted.</param>
        /// <returns>Value indicating success or error code.</returns>
        public BleStatus DeleteIncludeService(ushort serviceHandle, ushort includeHandle)
        {
            var ptr = 0;
            var command = new byte[4];
            BitConverter.GetBytes(serviceHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(includeHandle).CopyTo(command, ptr);
            ptr += 2;

            var response = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x109,
                CommandParameter = command,
                CommandLength = (uint) ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus) response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Mask events from the GATT. The default configuration is all the events masked.
        /// </summary>
        /// <param name="eventMask">GATT/ATT event mask.</param>
        /// <returns>Value indicating success or error code.</returns>
        public BleStatus SetEventMask(GattEventMask eventMask)
        {
            var command = BitConverter.GetBytes((uint) eventMask);

            var response = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x10a,
                CommandParameter = command,
                CommandLength = 2,
                ResponseParameter = response,
                ResponseLength = 1
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus) response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Perform an ATT MTU exchange procedure. When the ATT MTU exchange
        /// procedure is completed, a <see cref="EventProcessor.AttExchangeMtuResponseEvent"/> event
        /// is generated. A <see cref="EventProcessor.GattProcessCompleteEvent"/> event is also
        /// generated to indicate the end of the procedure.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <returns>Value indicating success or error code.</returns>
        public BleStatus ExchangeConfiguration(ushort connectionHandle)
        {
            var command = BitConverter.GetBytes(connectionHandle);

            var response = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x10b,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = 2,
                ResponseParameter = response,
                ResponseLength = 1
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus) response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Start the GATT client procedure to discover all primary services on
        /// the server. The responses of the procedure are given through the
        /// <see cref="EventProcessor.AttReadByGroupTypeResponseEvent"/> event.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <returns>Value indicating success or error code.</returns>
        public BleStatus DiscoverAllPrimaryServices(ushort connectionHandle)
        {
            var command = BitConverter.GetBytes(connectionHandle);

            var response = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x112,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = 2,
                ResponseParameter = response,
                ResponseLength = 1
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus) response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Start the procedure to discover the primary services of the specified
        /// UUID on the server. The responses of the procedure are given through
        /// the <see cref="EventProcessor.AttFindByTypeValueResponseEvent"/> event. The end of the
        /// procedure is indicated by a <see cref="EventProcessor.GattProcessCompleteEvent"/> event.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="uuidType">UUID type.</param>
        /// <param name="uuid">The UUID to discover.</param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="ArgumentException"><see cref="uuid"/> has to be of the length as indicated by <see cref="uuidType"/></exception>
        public BleStatus DiscoverPrimaryServicesByUuid(ushort connectionHandle, UuidType uuidType, byte[] uuid)
        {
            var uuidSize = uuidType == UuidType.Uuid16 ? 2 : 16;

            if (uuid is null || uuid.Length != uuidSize)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[3 + uuidSize];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte) uuidType;
            ptr += 1;
            uuid.CopyTo(command, ptr);
            ptr += uuidSize;

            var response = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x113,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = (uint) ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus) response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Start the procedure to find all included services. The responses of the procedure are given
        /// through the <see cref="EventProcessor.AttReadByTypeResponseEvent"/> event. The end of the
        /// procedure is indicated by a <see cref="EventProcessor.GattProcessCompleteEvent"/> event.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="startHandle">Start attribute handle of the service.</param>
        /// <param name="endHandle">End attribute handle of the service.</param>
        /// <returns>Value indicating success or error code.</returns>
        public BleStatus FindIncludedServices(ushort connectionHandle, ushort startHandle, ushort endHandle)
        {
            var ptr = 0;
            var command = new byte[6];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(startHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(endHandle).CopyTo(command, ptr);
            ptr += 2;

            var response = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x114,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = (uint) ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus) response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Start the procedure to discover all the characteristics of a given service. When the procedure
        /// is completed, a <see cref="EventProcessor.GattProcessCompleteEvent"/> event is generated.
        /// Before procedure completion the response packets are given through
        /// <see cref="EventProcessor.AttReadByTypeResponseEvent"/> event.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="startHandle">Start attribute handle of the service.</param>
        /// <param name="endHandle">End attribute handle of the service.</param>
        /// <returns>Value indicating success or error code.</returns>
        public BleStatus DiscoverAllCharacteristicsOfService(ushort connectionHandle, ushort startHandle,
            ushort endHandle)
        {
            var ptr = 0;
            var command = new byte[6];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(startHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(endHandle).CopyTo(command, ptr);
            ptr += 2;

            var response = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x114,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = (uint) ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus) response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Start the procedure to discover all the characteristics specified by a UUID. When the
        /// procedure is completed, a <see cref="EventProcessor.GattProcessCompleteEvent"/> event
        /// is generated. Before procedure completion the response packets are given through
        /// <see cref="EventProcessor.GattDiscoverReadCharacteristicByUuidResponseEvent"/> event.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="startHandle">Start attribute handle of the service.</param>
        /// <param name="endHandle">End attribute handle of the service.</param>
        /// <param name="uuidType">UUID type.</param>
        /// <param name="uuid">The UUID to discover.</param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="ArgumentException"><see cref="uuid"/> has to be of the length as indicated by <see cref="uuidType"/></exception>
        public BleStatus DiscoverCharacteristicsByUuid(
            ushort connectionHandle,
            ushort startHandle,
            ushort endHandle,
            UuidType uuidType,
            byte[] uuid)
        {
            var uuidSize = uuidType == UuidType.Uuid16 ? 2 : 16;

            if (uuid is null || uuid.Length != uuidSize)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[3 + uuidSize];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(startHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(endHandle).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte) uuidType;
            ptr += 1;
            uuid.CopyTo(command, ptr);
            ptr += uuidSize;

            var response = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x116,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = (uint) ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus) response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Start the procedure to discover all characteristic descriptors on the server. When the
        /// procedure is completed, a <see cref="EventProcessor.GattProcessCompleteEvent"/> event
        /// is generated. Before procedure completion the response packets are given through
        /// <see cref="EventProcessor.AttFindInfoResponseEvent"/> event.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="startHandle">Handle of the characteristic value.</param>
        /// <param name="endHandle">End handle of the characteristic.</param>
        /// <returns>Value indicating success or error code.</returns>
        public BleStatus DiscoverAllCharacteristicDescriptors(ushort connectionHandle, ushort startHandle,
            ushort endHandle)
        {
            var ptr = 0;
            var command = new byte[6];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(startHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(endHandle).CopyTo(command, ptr);
            ptr += 2;

            var response = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x117,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = (uint) ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus) response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Start the procedure to read the attribute value. When the procedure is completed, a
        /// <see cref="EventProcessor.GattProcessCompleteEvent"/> event is generated. Before procedure
        /// completion the response packet is given through <see cref="EventProcessor.AttReadResponseEvent"/> event.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="attributeHandle">Handle of the attribute to be read.</param>
        /// <returns>Value indicating success or error code.</returns>
        public BleStatus ReadCharacteristicValue(ushort connectionHandle, ushort attributeHandle)
        {
            var ptr = 0;
            var command = new byte[6];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(attributeHandle).CopyTo(command, ptr);
            ptr += 2;

            var response = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x118,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = (uint) ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus) response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Start the procedure to read all the characteristics specified by the UUID. When the
        /// procedure is completed, a <see cref="EventProcessor.GattProcessCompleteEvent"/> event
        /// is generated. Before procedure completion the response packets are given through 
        /// <see cref="EventProcessor.GattDiscoverReadCharacteristicByUuidResponseEvent"/> event.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="startHandle">Starting handle of the range to be searched.</param>
        /// <param name="endHandle">End handle of the range to be searched.</param>
        /// <param name="uuidType">UUID type.</param>
        /// <param name="uuid">The UUID to read.</param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="ArgumentException"></exception>
        public BleStatus ReadUsingCharacteristicUuid(ushort connectionHandle, ushort startHandle, ushort endHandle,
            UuidType uuidType, byte[] uuid)
        {
            var uuidSize = uuidType == UuidType.Uuid16 ? 2 : 16;

            if (uuid is null || uuid.Length != uuidSize)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[3 + uuidSize];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(startHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(endHandle).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte) uuidType;
            ptr += 1;
            uuid.CopyTo(command, ptr);
            ptr += uuidSize;

            var response = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x119,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = (uint) ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus) response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Start the procedure to read a long characteristic value. the procedure is completed, a
        /// <see cref="EventProcessor.GattProcessCompleteEvent"/> event is generated. Before procedure completion
        /// the response packets are given through <see cref="EventProcessor.AttReadBlobResponseEvent"/> event.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="attributeHandle">Handle of the attribute to be read.</param>
        /// <param name="valueOffset">Offset from which the value needs to be read.</param>
        /// <returns>Value indicating success or error code.</returns>
        public BleStatus ReadLongCharacteristicValue(ushort connectionHandle, ushort attributeHandle,
            ushort valueOffset)
        {
            var ptr = 0;
            var command = new byte[6];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(attributeHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(valueOffset).CopyTo(command, ptr);
            ptr += 2;

            var response = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x118,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = (uint) ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus) response[0];
            return BleStatus.Success;
        }

        public BleStatus ReadMultipleCharacteristicValues(ushort connectionHandle, byte handleCount, ushort[] handles)
        {
            throw new NotImplementedException();
        }

        public BleStatus WriteCharacteristicValue(ushort connectionHandle, ushort attributeHandle,
            byte attributeValueLength, byte[] attributeValue)
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

        /// <summary>
        /// Deny the GATT server to send a response to a read request from a
        /// client. The application may send this command when it receives the
        /// <see cref="EventProcessor.GattReadPermitRequestEvent"/> or 
        /// <see cref="EventProcessor.GattReadMultiPermitRequestEvent"/>. This command indicates to the
        /// stack that the client is not allowed to read the requested
        /// characteristic due to e.g. application restrictions. The Error code
        /// shall be either 0x08 (Insufficient Authorization) or a value in the
        /// range 0x80-0x9F (Application Error). The application should issue the
        /// <see cref="DenyRead"/> or <see cref="AllowRead"/> command within 30
        /// seconds from the reception of the <see cref="EventProcessor.GattReadPermitRequestEvent"/>
        /// or  <see cref="EventProcessor.GattReadMultiPermitRequestEvent"/> events otherwise the
        /// GATT procedure will issue a timeout.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="errorCode">Error code for the command.</param>
        /// <returns>Value indicating success or error code.</returns>
        public BleStatus DenyRead(
            ushort connectionHandle,
            AttErrorCode errorCode)
        {
            var ptr = 0;
            var command = new byte[3];

            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte) errorCode;
            ptr += 1;

            var response = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x12d,
                CommandParameter = command,
                CommandLength = (uint) ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus) response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// This command sets the access permission for the attribute handle specified.
        /// </summary>
        /// <param name="serviceHandle">Handle of the service which contains the attribute whose
        /// access permission has to be modified</param>
        /// <param name="attributeHandle">Handle of the attribute whose security permission has to be modified</param>
        /// <param name="accessPermissions">Access permission.</param>
        /// <returns>Value indicating success or error code.</returns>
        public BleStatus SetAccessPermission(
            ushort serviceHandle,
            ushort attributeHandle,
            AccessPermissions accessPermissions)
        {
            var ptr = 0;
            var command = new byte[3];

            BitConverter.GetBytes(serviceHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(attributeHandle).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte) accessPermissions;
            ptr += 1;

            var response = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x12d,
                CommandParameter = command,
                CommandLength = (uint) ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus) response[0];
            return BleStatus.Success;
        }
    }
}