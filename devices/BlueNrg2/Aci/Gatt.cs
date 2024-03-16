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
            out ushort serviceHandle
        )
        {
            serviceHandle = 0;

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
            out ushort includeHandle
        )
        {
            includeHandle = 0;

            var uuidSize = includeUuidType == UuidType.Uuid16 ? 2 : 16;

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
            out ushort characteristicHandle
        )
        {
            characteristicHandle = 0;

            var uuidSize = characteristicUuidType == UuidType.Uuid16 ? 2 : 16;

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
            command[ptr] = (byte)characteristicEventMask;
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
            out ushort characteristicDescriptorHandle)
        {
            characteristicDescriptorHandle = 0;

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
            command[ptr] = (byte)characteristicDescriptorUuidType;
            ptr += 1;
            characteristicDescriptorUuid.CopyTo(command, ptr);
            ptr += uuidSize;
            command[ptr] = characteristicDescriptorValueMaximumLength;
            ptr += 1;
            command[ptr] = characteristicDescriptorValueLength;
            ptr += 1;
            characteristicDescriptorValue.CopyTo(command, ptr);
            ptr += characteristicDescriptorValueLength;
            command[ptr] = (byte)securityPermissions;
            ptr += 1;
            command[ptr] = (byte)accessPermissions;
            ptr += 1;
            command[ptr] = (byte)characteristicEventMask;
            ptr += 1;
            command[ptr] = encryptionKeySize;
            ptr += 1;
            command[ptr] = (byte)(hasVariableLength ? 0x01 : 0x00);
            ptr += 1;

            var response = new byte[3];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x105,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = 3
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
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
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Delete the specified characteristic from the service.
        /// </summary>
        /// <param name="serviceHandle">Handle of service to which the characteristic belongs.</param>
        /// <param name="characteristicHandle">Handle of the characteristic which has to be deleted.</param>
        /// <returns>Value indicating success or error code.</returns>
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
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
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
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
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
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Mask events from the GATT. The default configuration is all the events masked.
        /// </summary>
        /// <param name="eventMask">GATT/ATT event mask.</param>
        /// <returns>Value indicating success or error code.</returns>
        public BleStatus SetEventMask(GattEventMask eventMask)
        {
            var command = BitConverter.GetBytes((uint)eventMask);

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
                return (BleStatus)response[0];
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
                return (BleStatus)response[0];
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
                return (BleStatus)response[0];
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
            command[ptr] = (byte)uuidType;
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
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
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
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
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
        public BleStatus DiscoverAllCharacteristicsOfService(
            ushort connectionHandle,
            ushort startHandle,
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
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
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
            command[ptr] = (byte)uuidType;
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
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
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
        public BleStatus DiscoverAllCharacteristicDescriptors(
            ushort connectionHandle,
            ushort startHandle,
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
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
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
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
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
        public BleStatus ReadUsingCharacteristicUuid(
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
            command[ptr] = (byte)uuidType;
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
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
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
        public BleStatus ReadLongCharacteristicValue(
            ushort connectionHandle,
            ushort attributeHandle,
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
                OpCodeCommand = 0x11a,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Start a procedure to read multiple characteristic values from a server.
        /// This sub-procedure is used to read multiple Characteristic Values from a
        /// server when the client knows the Characteristic Value Handles. When the
        /// procedure is completed, a <see cref="EventProcessor.GattProcessCompleteEvent"/>
        /// event is generated. Before procedure completion the response packets are given
        /// through <see cref="EventProcessor.AttReadMultipleResponseEvent"/> event.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="handleCount">The number of handles for which the value has to be read. From 2 to (ATT_MTU-1)/2</param>
        /// <param name="handles">List of handles of the characteristics that should be read.</param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="ArgumentException"><see cref="handles"/> should have the length indicated by <see cref="handleCount"/></exception>
        public BleStatus ReadMultipleCharacteristicValues(ushort connectionHandle, byte handleCount, ushort[] handles)
        {
            if (handles is null || handleCount != handles.Length)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = handleCount;
            ptr += 1;
            for (int i = 0; i < handleCount; i++)
            {
                BitConverter.GetBytes(handles[i]).CopyTo(command, ptr);
                ptr += 2;
            }

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x11b,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Start the procedure to write a characteristic value. When the
        /// procedure is completed, a <see cref="EventProcessor.GattProcessCompleteEvent"/> event is
        /// generated. During the procedure, <see cref="EventProcessor.AttPrepareWriteResponseEvent"/>
        /// and <see cref="EventProcessor.AttExecuteWriteResponseEvent"/> events are raised.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="attributeHandle">Handle of the attribute to be written.</param>
        /// <param name="attributeValueLength">Length of the value to be written.</param>
        /// <param name="attributeValue">Value to be written.</param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="ArgumentException"><see cref="attributeValue"/> should have the length indicated by <see cref="attributeValueLength"/></exception>
        public BleStatus WriteCharacteristicValue(
            ushort connectionHandle,
            ushort attributeHandle,
            byte attributeValueLength,
            byte[] attributeValue)
        {
            if (attributeValue is null || attributeValueLength != attributeValue.Length)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(attributeHandle).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = attributeValueLength;
            ptr += 1;
            attributeValue.CopyTo(command, ptr);
            ptr += attributeValueLength;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x11c,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Start the procedure to write a long characteristic value. When the
        /// procedure is completed, a <see cref="EventProcessor.GattProcessCompleteEvent"/> event is
        /// generated. During the procedure, <see cref="EventProcessor.AttPrepareWriteResponseEvent"/>
        /// and <see cref="EventProcessor.AttExecuteWriteResponseEvent"/> events are raised.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="attributeHandle">Handle of the attribute to be written.</param>
        /// <param name="valueOffset">Offset at which the attribute has to be written.</param>
        /// <param name="attributeValueLength">Length of the value to be written.</param>
        /// <param name="attributeValue">Value to be written.</param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="ArgumentException"><see cref="attributeValue"/> should have the length indicated by <see cref="attributeValueLength"/></exception>
        public BleStatus WriteLongCharacteristicValue(
            ushort connectionHandle,
            ushort attributeHandle,
            ushort valueOffset,
            byte attributeValueLength,
            byte[] attributeValue)
        {
            if (attributeValue is null || attributeValueLength != attributeValue.Length)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(attributeHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(valueOffset).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = attributeValueLength;
            ptr += 1;
            attributeValue.CopyTo(command, ptr);
            ptr += attributeValueLength;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x11d,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Start the procedure to write a characteristic value reliably. When the
        /// procedure is completed, a <see cref="EventProcessor.GattProcessCompleteEvent"/> event is
        /// generated. During the procedure, <see cref="EventProcessor.AttPrepareWriteResponseEvent"/>
        /// and <see cref="EventProcessor.AttExecuteWriteResponseEvent"/> events are raised.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="attributeHandle">Handle of the attribute to be written.</param>
        /// <param name="valueOffset">Offset at which the attribute has to be written.</param>
        /// <param name="attributeValueLength">Length of the value to be written.</param>
        /// <param name="attributeValue">Value to be written.</param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="ArgumentException"><see cref="attributeValue"/> should have the length indicated by <see cref="attributeValueLength"/></exception>
        public BleStatus WriteCharacteristicReliably(
            ushort connectionHandle,
            ushort attributeHandle,
            ushort valueOffset,
            byte attributeValueLength,
            byte[] attributeValue)
        {
            if (attributeValue is null || attributeValueLength != attributeValue.Length)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(attributeHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(valueOffset).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = attributeValueLength;
            ptr += 1;
            attributeValue.CopyTo(command, ptr);
            ptr += attributeValueLength;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x11e,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Start the procedure to write a characteristic descriptor. When the
        /// procedure is completed, a <see cref="EventProcessor.GattProcessCompleteEvent"/> event is
        /// generated. During the procedure, <see cref="EventProcessor.AttPrepareWriteResponseEvent"/>
        /// and <see cref="EventProcessor.AttExecuteWriteResponseEvent"/> events are raised.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="attributeHandle">Handle of the attribute to be written.</param>
        /// <param name="valueOffset">Offset at which the attribute has to be written.</param>
        /// <param name="attributeValueLength">Length of the value to be written.</param>
        /// <param name="attributeValue">Value to be written.</param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="ArgumentException"><see cref="attributeValue"/> should have the length indicated by <see cref="attributeValueLength"/></exception>
        public BleStatus WriteLongCharacteristicDescriptor(
            ushort connectionHandle,
            ushort attributeHandle,
            ushort valueOffset,
            byte attributeValueLength,
            byte[] attributeValue)
        {
            if (attributeValue is null || attributeValueLength != attributeValue.Length)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(attributeHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(valueOffset).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = attributeValueLength;
            ptr += 1;
            attributeValue.CopyTo(command, ptr);
            ptr += attributeValueLength;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x11f,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Start the procedure to read a long characteristic value. When the
        /// procedure is completed, a <see cref="EventProcessor.GattProcessCompleteEvent"/> event is
        /// generated. Before procedure completion the response packets are given
        /// through <see cref="EventProcessor.AttReadBlobResponseEvent"/> event.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="attributeHandle">Handle of the characteristic descriptor.</param>
        /// <param name="valueOffset">Offset from which the value needs to be read.</param>
        /// <returns>Value indicating success or error code.</returns>
        public BleStatus ReadLongCharacteristicDescriptor(
            ushort connectionHandle,
            ushort attributeHandle,
            ushort valueOffset)
        {
            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(attributeHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(valueOffset).CopyTo(command, ptr);
            ptr += 2;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x120,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Start the procedure to write a characteristic descriptor. When the procedure is completed,
        /// a <see cref="EventProcessor.GattProcessCompleteEvent"/> event is generated.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="attributeHandle">Handle of the attribute to be written.</param>
        /// <param name="attributeValueLength">Length of the value to be written.</param>
        /// <param name="attributeValue">Value to be written.</param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="ArgumentException"><see cref="attributeValue"/> should have the length indicated by <see cref="attributeValueLength"/></exception>
        public BleStatus WriteCharacteristicDescriptor(
            ushort connectionHandle,
            ushort attributeHandle,
            byte attributeValueLength,
            byte[] attributeValue)
        {
            if (attributeValue is null || attributeValueLength != attributeValue.Length)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(attributeHandle).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = attributeValueLength;
            ptr += 1;
            attributeValue.CopyTo(command, ptr);
            ptr += attributeValueLength;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x121,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Start the procedure to read a the descriptor specified. When the
        /// procedure is completed, a <see cref="EventProcessor.GattProcessCompleteEvent"/> event is
        /// generated. Before procedure completion the response packets are given
        /// through <see cref="EventProcessor.AttReadResponseEvent"/> event.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="attributeHandle">Handle of the descriptor to be read.</param>
        /// <returns>Value indicating success or error code.</returns>
        public BleStatus ReadCharacteristicDescriptor(
            ushort connectionHandle,
            ushort attributeHandle)
        {
            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(attributeHandle).CopyTo(command, ptr);
            ptr += 2;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x122,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Start the procedure to write a characteristic value without waiting
        /// for any response from the server. No events are generated after this
        /// command is executed.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="attributeHandle">Handle of the attribute to be written.</param>
        /// <param name="attributeValueLength">Length of the value to be written (maximum value is ATT_MTU - 3).</param>
        /// <param name="attributeValue">Value to be written.</param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="ArgumentException"><see cref="attributeValue"/> should have the length indicated by <see cref="attributeValueLength"/></exception>
        public BleStatus WriteWithoutResponse(
            ushort connectionHandle,
            ushort attributeHandle,
            byte attributeValueLength,
            byte[] attributeValue)
        {
            if (attributeValue is null || attributeValueLength != attributeValue.Length)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(attributeHandle).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = attributeValueLength;
            ptr += 1;
            attributeValue.CopyTo(command, ptr);
            ptr += attributeValueLength;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x123,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Start a signed write without response from the server. The procedure
        /// is used to write a characteristic value with an authentication
        /// signature without waiting for any response from the server. It cannot
        /// be used when the link is encrypted.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="attributeHandle">Handle of the attribute to be written.</param>
        /// <param name="attributeValueLength">Length of the value to be written (maximum value is ATT_MTU - 13).</param>
        /// <param name="attributeValue">Value to be written.</param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="ArgumentException"><see cref="attributeValue"/> should have the length indicated by <see cref="attributeValueLength"/></exception>
        public BleStatus SignedWriteWithoutResponse(
            ushort connectionHandle,
            ushort attributeHandle,
            byte attributeValueLength,
            byte[] attributeValue)
        {
            if (attributeValue is null || attributeValueLength != attributeValue.Length)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(attributeHandle).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = attributeValueLength;
            ptr += 1;
            attributeValue.CopyTo(command, ptr);
            ptr += attributeValueLength;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x124,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Allow application to confirm indication. This command has to be sent
        /// when the application receives the event <see cref="EventProcessor.GattIndicationEvent"/>.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <returns>Value indicating success or error code.</returns>
        public BleStatus ConfirmIndication(ushort connectionHandle)
        {
            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x125,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Allow or reject a write request from a client. This command has to be
        /// sent by the application when it receives the <see cref="EventProcessor.GattWritePermitRequestEvent"/> event.
        /// If the write can be allowed, then the
        /// status and error code has to be set to 0. If the write cannot be
        /// allowed, then the status has to be set to 1 and the error code has to
        /// be set to the error code that has to be passed to the client.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="attributeHandle">Handle of the attribute that was passed in the event <see cref="EventProcessor.GattWritePermitRequestEvent"/></param>
        /// <param name="canWrite">If the value can be written or not.</param>
        /// <param name="errorCode">The error code that has to be passed to the client in case the write has to be rejected.</param>
        /// <param name="attributeValueLength">Length of the value to be written as passed in the event <see cref="EventProcessor.GattWritePermitRequestEvent"/></param>
        /// <param name="attributeValue">Value as passed in the event <see cref="EventProcessor.GattWritePermitRequestEvent"/></param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="ArgumentException"><see cref="attributeValue"/> should have the length indicated by <see cref="attributeValueLength"/></exception>
        public BleStatus WriteResponse(
            ushort connectionHandle,
            ushort attributeHandle,
            bool canWrite,
            byte errorCode,
            byte attributeValueLength,
            byte[] attributeValue)
        {
            if (attributeValue is null || attributeValueLength != attributeValue.Length)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(attributeHandle).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte)(canWrite ? 0x01 : 0x00);
            ptr += 1;
            command[ptr] = errorCode;
            ptr += 1;
            command[ptr] = attributeValueLength;
            ptr += 1;
            attributeValue.CopyTo(command, ptr);
            ptr += attributeValueLength;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x126,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Allow the GATT server to send a response to a read request from a
        /// client. The application has to send this command when it receives the
        /// <see cref="EventProcessor.GattReadPermitRequestEvent"/> event or
        /// <see cref="EventProcessor.GattReadMultiPermitRequestEvent"/> event.
        /// This command indicates to the stack that the response can be sent to
        /// the client. So if the application wishes to update any of the attributes
        /// before they are read by the client, it has to update the characteristic
        /// values using the <see cref="UpdateCharacteristicValue"/> and then give
        /// this command. The application should perform the required operations
        /// within 30 seconds. Otherwise the GATT procedure will be timeout.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <returns>Value indicating success or error code.</returns>
        public BleStatus AllowRead(ushort connectionHandle)
        {
            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x127,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// This command sets the security permission for the attribute handle
        /// specified. Currently the setting of security permission is allowed
        /// only for client configuration descriptor.
        /// </summary>
        /// <param name="serviceHandle">Handle of the service which contains the attribute whose
        /// security permission has to be modified</param>
        /// <param name="attributeHandle">Handle of the attribute whose security permission has to be modified</param>
        /// <param name="securityPermissions">Security permission flags.</param>
        /// <returns>Value indicating success or error code.</returns>
        public BleStatus SetSecurityPermission(
            ushort serviceHandle,
            ushort attributeHandle,
            SecurityPermissions securityPermissions)
        {
            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(serviceHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(attributeHandle).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte)securityPermissions;
            ptr += 1;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x128,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// This command sets the value of the descriptor specified by <see cref="characteristicDescriptorHandle"/>.
        /// </summary>
        /// <param name="serviceHandle">Handle of the service which contains the characteristic descriptor.</param>
        /// <param name="characteristicHandle">Handle of the characteristic which contains the descriptor.</param>
        /// <param name="characteristicDescriptorHandle">Handle of the descriptor whose value has to be set.</param>
        /// <param name="valueOffset">Offset from which the descriptor value has to be updated.</param>
        /// <param name="characteristicDescriptorValueLength">Length of the descriptor value.</param>
        /// <param name="characteristicDescriptorValue">Descriptor value.</param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="ArgumentException"><see cref="characteristicDescriptorValue"/> should have the length indicated by <see cref="characteristicDescriptorValueLength"/></exception>
        public BleStatus SetDescriptorValue(
            ushort serviceHandle,
            ushort characteristicHandle,
            ushort characteristicDescriptorHandle,
            ushort valueOffset,
            byte characteristicDescriptorValueLength,
            byte[] characteristicDescriptorValue)
        {
            if (characteristicDescriptorValue is null || characteristicDescriptorValueLength != characteristicDescriptorValue.Length)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(serviceHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(characteristicHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(characteristicDescriptorHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(valueOffset).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = characteristicDescriptorValueLength;
            ptr += 1;
            characteristicDescriptorValue.CopyTo(command, ptr);
            ptr += characteristicDescriptorValueLength;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x129,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Reads the value of the attribute handle specified from the local GATT database.
        /// </summary>
        /// <param name="attributeHandle">Handle of the attribute to read.</param>
        /// <param name="offset">Offset from which the value needs to be read.</param>
        /// <param name="valueLengthRequested">Maximum number of octets to be returned as
        /// attribute value</param>
        /// <param name="length">Length of the attribute value.</param>
        /// <param name="valueLength">Length in octets of the Value parameter.</param>
        /// <param name="value">Attribute value.</param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="ArgumentNullException"><see cref="value"/> has to be instantiated.</exception>
        public BleStatus ReadHandleValue(
            ushort attributeHandle,
            ushort offset,
            ushort valueLengthRequested,
            out ushort length,
            out ushort valueLength,
            out byte[] value)
        {
            length = 0;
            valueLength = 0;
            value = null;

            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(attributeHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(offset).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(valueLengthRequested).CopyTo(command, ptr);
            ptr += 2;

            const uint responseLength = 128;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x12a,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];

            ptr = 1;
            length = BitConverter.ToUInt16(response, ptr);
            ptr += 2;
            valueLength = BitConverter.ToUInt16(response, ptr);
            ptr += 2;

            value = new byte[valueLength];
            Array.Copy(response, ptr, value, 0, valueLength);

            return BleStatus.Success;
        }

        /// <summary>
        /// This command is a more flexible version of <see cref="UpdateCharacteristicValue"/> to support update of
        /// long attribute up to 512 bytes and indicate selectively the generation
        /// of Indication/Notification.
        /// </summary>
        /// <param name="connectionHandleToNotify">Connection handle to notify. Notify all
        /// subscribed clients if equal to 0x0000: DEPRECATED feature (still
        /// supported but not recommended).</param>
        /// <param name="serviceHandle">Handle of service to which the characteristic belongs.</param>
        /// <param name="characteristicHandle">Handle of the characteristic.</param>
        /// <param name="updateType">Allow Notification or Indication generation, if enabled in
        /// the client characteristic configuration descriptor. If bit 3 is set,
        /// standard BLE Link Layer retransmission mechanism for notifications
        /// PDUs si disabled: PDUs will be transmitted only once, even if they
        /// have not been acknowledged.</param>
        /// <param name="charLength">Total length of the characteristic value. In case of a
        /// variable size characteristic, this field specifies the new length of
        /// the characteristic value after the update; in case of fixed length
        /// characteristic this field is ignored.</param>
        /// <param name="valueOffset">The offset from which the attribute value has to be updated.</param>
        /// <param name="valueLength">Length of the Value parameter in octets.</param>
        /// <param name="value">Updated characteristic value.</param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="ArgumentException"><see cref="value"/> should have the length indicated by <see cref="valueLength"/></exception>
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
            if (value is null || value.Length != valueLength)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(connectionHandleToNotify).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(serviceHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(characteristicHandle).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte)updateType;
            ptr += 1;
            BitConverter.GetBytes(charLength).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(valueOffset).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = valueLength;
            ptr += 1;
            value.CopyTo(command, ptr);
            ptr += valueLength;

            const uint responseLength = 128;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x12c,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
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
            command[ptr] = (byte)errorCode;
            ptr += 1;

            var response = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x12d,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
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
            command[ptr] = (byte)accessPermissions;
            ptr += 1;

            var response = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x12d,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }
    }
}
