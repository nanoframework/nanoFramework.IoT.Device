// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci
{
    public class Gap
    {
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
        /// elapsed), controller generates <see cref="E:BlueNrg2.Events.GapLimitedDiscoverableEvent"/> event
        /// </summary>
        /// <param name="advertisingType">Advertising type.</param>
        /// <param name="minimumAdvertisingInterval">Minimum advertising interval for undirected
        /// and low duty cycle directed advertising. Time = N * 0.625 ms.</param>
        /// <param name="maximumAdvertisingInterval">Maximum advertising interval. Time = N * 0.625 ms.</param>
        /// <param name="ownAddressType">Own address type:
        /// <list type="bullet">
        /// <item>0x00: Public Device Address (it is allowed only if privacy is disabled)</item>
        /// <item>0x01: Random Device Address (it is allowed only if privacy is disabled)</item>
        /// <item>0x02: Resolvable Private Address (it is allowed only if privacy is enabled)</item>
        /// <item>0x03: Non Resolvable Private Address (it is allowed only if privacy is enabled)</item>
        /// </list>
        /// </param>
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
        /// <param name="minimumSlaveConnectionInterval">Minimum value for slave connection interval
        /// suggested by the Peripheral. If <see cref="minimumSlaveConnectionInterval"/> and
        /// <see cref="maximumSlaveConnectionInterval"/> are not 0x0000, Slave Connection Interval
        /// Range AD structure will be added in advertising data. Connection
        /// interval is defined in the following manner:
        /// <code>minimum connection interval = <see cref="minimumSlaveConnectionInterval"/> x 1.25ms.</code>
        /// </param>
        /// <param name="maximumSlaveConnectionInterval">Slave connection interval maximum value
        /// suggested by Peripheral. If <see cref="minimumSlaveConnectionInterval"/> and
        /// <see cref="maximumSlaveConnectionInterval"/> are not 0x0000, Slave Connection Interval
        /// Range AD structure will be added in advertising data. Connection
        /// interval is defined in the following manner:
        /// <code>maximum connection interval = <see cref="maximumSlaveConnectionInterval"/> x 1.25ms</code>
        /// </param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="ArgumentException"><see cref="localName"/> or <see cref="serviceUuidList"/> is of incorrect length.</exception>
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
            if (localName is null || localName.Length != localNameLength)
                throw new ArgumentException(nameof(localName));

            if (serviceUuidList is null || serviceUuidList.Length != serviceUuidLength)
                throw new ArgumentException(nameof(serviceUuidList));

            var ptr = 0;
            var command = new byte[258];
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
            localName.CopyTo(command, ptr);
            ptr += localNameLength;
            command[ptr] = serviceUuidLength;
            ptr += 1;
            serviceUuidList.CopyTo(command, ptr);
            ptr += serviceUuidLength;
            BitConverter.GetBytes(minimumSlaveConnectionInterval).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(maximumSlaveConnectionInterval).CopyTo(command, ptr);
            ptr += 2;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x082,
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
            FilterPolicy advertisingFilterPolicy,
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
            command[ptr] = (byte)advertisingFilterPolicy;
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
            if (directAddress is null || directAddress.Length != 6)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[258];
            command[ptr] = (byte)addressType;
            ptr += 1;
            command[ptr] = (byte)directedAdvertisingType;
            ptr += 1;
            command[ptr] = (byte)directAddressType;
            ptr += 1;
            directAddress.CopyTo(command, ptr);
            ptr += 6;
            BitConverter.GetBytes(minimumAdvertisingInterval).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(maximumAdvertisingInterval).CopyTo(command, ptr);
            ptr += 2;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x082,
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
            out ushort serviceHandle,
            out ushort deviceNameCharacteristicHandle,
            out ushort appearanceCharacteristicHandle)
        {
            serviceHandle = 0;
            deviceNameCharacteristicHandle = 0;
            appearanceCharacteristicHandle = 0;

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
            AdvertisingType advertisingEventType,
            AddressType ownAddressType
        )
        {
            var command = new byte[2];
            command[0] = (byte)advertisingEventType;
            command[1] = (byte)ownAddressType;

            var response = new byte[7];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x08b,
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

        public BleStatus SetUndirectedConnectable(
            ushort minimumAdvertisingInterval,
            ushort maximumAdvertisingInterval,
            AddressType ownAddressType,
            FilterPolicy advertisingFilterPolicy
        )
        {
            var ptr = 0;
            var command = new byte[6];
            BitConverter.GetBytes(minimumAdvertisingInterval).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(maximumAdvertisingInterval).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte)ownAddressType;
            ptr += 1;
            command[ptr] = (byte)advertisingFilterPolicy;
            ptr += 1;

            var response = new byte[7];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x08c,
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
            var command = new byte[1];
            command[0] = (byte)advertisingType;

            var status = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x08f,
                CommandParameter = command,
                CommandLength = 1,
                ResponseParameter = status,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
        }

        public BleStatus GetSecurityLevel(
            ushort connectionHandle,
            out byte securityMode,
            out byte securityLevel
        )
        {
            securityMode = 0;
            securityLevel = 0;

            var ptr = 0;
            var command = new byte[2];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;

            var response = new byte[3];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x090,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = 3
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            securityMode = response[1];
            securityLevel = response[2];
            return BleStatus.Success;
        }

        public BleStatus SetEventMask(
            GapEventMask eventMask
        )
        {
            var ptr = 0;
            var command = new byte[2];
            BitConverter.GetBytes((ushort)eventMask).CopyTo(command, ptr);
            ptr += 2;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x091,
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

        public BleStatus ConfigureWhitelist()
        {
            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x092,
                ResponseParameter = response,
                ResponseLength = responseLength
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        public BleStatus Terminate(ushort connectionHandle, Reason reason)
        {
            var ptr = 0;
            var command = new byte[3];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte)reason;
            ptr += 1;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x093,
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

        public BleStatus AllowRebond(ushort connectionHandle)
        {
            var ptr = 0;
            var command = new byte[2];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x095,
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

        public BleStatus StartLimitedDiscoveryProcedure(
            ushort scanInterval,
            ushort scanWindow,
            AddressType ownAddressType,
            bool filterDuplicates
        )
        {
            var ptr = 0;
            var command = new byte[6];
            BitConverter.GetBytes(scanInterval).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(scanWindow).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte)ownAddressType;
            ptr += 1;
            command[ptr] = (byte)(filterDuplicates ? 0x01 : 0x00);
            ptr += 1;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x096,
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

        public BleStatus StartGeneralDiscoveryProcedure(
            ushort scanInterval,
            ushort scanWindow,
            AddressType ownAddressType,
            bool filterDuplicates
        )
        {
            var ptr = 0;
            var command = new byte[6];
            BitConverter.GetBytes(scanInterval).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(scanWindow).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte)ownAddressType;
            ptr += 1;
            command[ptr] = (byte)(filterDuplicates ? 0x01 : 0x00);
            ptr += 1;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x097,
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
            ushort minimumConnectionEventLength,
            ushort maximumConnectionEventLength
        )
        {
            if (peerAddress is null || peerAddress.Length != 6)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[24];
            BitConverter.GetBytes(scanInterval).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(scanWindow).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte)peerAddressType;
            ptr += 1;
            peerAddress.CopyTo(command, ptr);
            ptr += 6;
            command[ptr] = (byte)ownAddressType;
            ptr += 1;
            BitConverter.GetBytes(minimumConnectionInterval).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(maximumConnectionInterval).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(connectionLatency).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(supervisionTimeout).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(minimumConnectionEventLength).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(maximumConnectionEventLength).CopyTo(command, ptr);
            ptr += 2;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x098,
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

        public BleStatus StartAutomaticConnectionEstablishProcedure(
            ushort scanInterval,
            ushort scanWindow,
            AddressType ownAddressType,
            ushort minimumConnectionInterval,
            ushort maximumConnectionInterval,
            ushort connectionLatency,
            ushort supervisionTimeout,
            ushort minimumConnectionEventLength,
            ushort maximumConnectionEventLength,
            byte whitelistEntryCount,
            DeviceAddressEntry[] whitelistEntries
        )
        {
            if (whitelistEntries is null || whitelistEntries.Length != whitelistEntryCount)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(scanInterval).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(scanWindow).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte)ownAddressType;
            ptr += 1;
            BitConverter.GetBytes(minimumConnectionInterval).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(maximumConnectionInterval).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(connectionLatency).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(supervisionTimeout).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(minimumConnectionEventLength).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(maximumConnectionEventLength).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = whitelistEntryCount;
            ptr += 1;
            for (int i = 0; i < whitelistEntryCount; i++)
            {
                command[ptr] = (byte)whitelistEntries[i].AddressType;
                ptr += 1;
                whitelistEntries[i].Address.CopyTo(command, ptr);
                ptr += 6;
            }

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x099,
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

        public BleStatus StartGeneralConnectionEstablishProcedure(
            ScanType scanType,
            ushort scanInterval,
            ushort scanWindow,
            AddressType ownAddressType,
            FilterPolicy scanningFilterPolicy,
            bool filterDuplicates
        )
        {
            var ptr = 0;
            var command = new byte[258];
            command[ptr] = (byte)scanType;
            ptr += 1;
            BitConverter.GetBytes(scanInterval).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(scanWindow).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte)ownAddressType;
            ptr += 1;
            command[ptr] = (byte)scanningFilterPolicy;
            ptr += 1;
            command[ptr] = (byte)(filterDuplicates ? 0x01 : 0x00);
            ptr += 1;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x09a,
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
            if (whitelistEntries is null || whitelistEntries.Length != whitelistEntryCount)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[258];
            command[ptr] = (byte)scanType;
            ptr += 1;
            BitConverter.GetBytes(scanInterval).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(scanWindow).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte)ownAddressType;
            ptr += 1;
            command[ptr] = (byte)scanningFilterPolicy;
            ptr += 1;
            command[ptr] = (byte)(filterDuplicates ? 0x01 : 0x00);
            ptr += 1;
            command[ptr] = whitelistEntryCount;
            ptr += 1;
            for (int i = 0; i < whitelistEntryCount; i++)
            {
                command[ptr] = (byte)whitelistEntries[i].AddressType;
                ptr += 1;
                whitelistEntries[i].Address.CopyTo(command, ptr);
                ptr += 6;
            }

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x09b,
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
            ushort minimumConnectionEventLength,
            ushort maximumConnectionEventLength
        )
        {
            if (peerAddress is null || peerAddress.Length != 6)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(scanInterval).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(scanWindow).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte)peerAddressType;
            ptr += 1;
            peerAddress.CopyTo(command, ptr);
            ptr += 6;
            command[ptr] = (byte)ownAddressType;
            ptr += 1;
            BitConverter.GetBytes(minimumConnectionInterval).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(maximumConnectionInterval).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(connectionLatency).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(supervisionTimeout).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(minimumConnectionEventLength).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(maximumConnectionEventLength).CopyTo(command, ptr);
            ptr += 2;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x09c,
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

        public BleStatus TerminateGapProcedure(ProcedureCode procedureCode)
        {
            var ptr = 0;
            var command = new byte[258];
            command[ptr] = (byte)procedureCode;
            ptr += 1;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x09d,
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

        public BleStatus StartConnectionUpdate(
            ushort connectionHandle,
            ushort minimumConnectionInterval,
            ushort maximumConnectionInterval,
            ushort connectionLatency,
            ushort supervisionTimeout,
            ushort minimumConnectionEventLength,
            ushort maximumConnectionEventLength
        )
        {
            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(minimumConnectionInterval).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(maximumConnectionInterval).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(connectionLatency).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(supervisionTimeout).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(minimumConnectionEventLength).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(maximumConnectionEventLength).CopyTo(command, ptr);
            ptr += 2;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x09e,
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

        public BleStatus SendPairingRequest(ushort connectionHandle, bool forceRebond)
        {
            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte)(forceRebond ? 0x01 : 0x00);
            ptr += 1;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x09f,
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

        public BleStatus ResolvePrivateAddress(byte[] address, out byte[] actualAddress)
        {
            if (address is null || address.Length != 6)
                throw new ArgumentException();

            actualAddress = null;

            var ptr = 0;
            var command = new byte[258];
            address.CopyTo(command, ptr);
            ptr += 6;

            const uint responseLength = 7;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x0a0,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];

            actualAddress = new byte[6];

            Array.Copy(response, 1, actualAddress, 0, 6);

            return BleStatus.Success;
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
            if (advertisingData is null || advertisingData.Length != advertisingDataLength)
                throw new ArgumentException(nameof(advertisingData));

            if (whitelistEntries is null || whitelistEntries.Length != whitelistEntryCount)
                throw new ArgumentException(nameof(whitelistEntries));

            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(minimumAdvertisingInterval).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(maximumAdvertisingInterval).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte)advertisingType;
            ptr += 1;
            command[ptr] = (byte)ownAddressType;
            ptr += 1;
            command[ptr] = advertisingDataLength;
            ptr += 1;
            advertisingData.CopyTo(command, ptr);
            ptr += advertisingDataLength;
            command[ptr] = whitelistEntryCount;
            ptr += 1;
            for (int i = 0; i < whitelistEntryCount; i++)
            {
                command[ptr] = (byte)whitelistEntries[i].AddressType;
                ptr += 1;
                whitelistEntries[i].Address.CopyTo(command, ptr);
                ptr += 6;
            }

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x0a1,
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

        public BleStatus StartObservationProcedure(
            ushort scanInterval,
            ushort scanWindow,
            ScanType scanType,
            AddressType ownAddressType,
            byte filterDuplicates,
            FilterPolicy scanningFilterPolicy
        )
        {
            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(scanInterval).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(scanWindow).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte)scanType;
            ptr += 1;
            command[ptr] = (byte)ownAddressType;
            ptr += 1;
            command[ptr] = filterDuplicates;
            ptr += 1;
            command[ptr] = (byte)scanningFilterPolicy;
            ptr += 1;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x0a2,
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

        public BleStatus GetBondedDevices(
            out byte addressCount,
            out DeviceAddressEntry[] bondedDeviceAddresses
        )
        {
            addressCount = 0;
            bondedDeviceAddresses = null;

            const uint responseLength = 128;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x0a3,
                ResponseParameter = response,
                ResponseLength = responseLength
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];

            var ptr = 1;
            addressCount = response[ptr];
            ptr += 1;

            bondedDeviceAddresses = new DeviceAddressEntry[addressCount];

            for (int i = 0; i < addressCount; i++)
            {
                bondedDeviceAddresses[i].AddressType = (AddressType)response[ptr];
                ptr += 1;
                bondedDeviceAddresses[i].Address = new byte[6];
                Array.Copy(response, ptr, bondedDeviceAddresses[i].Address, 0, 6);
                ptr += 6;
            }

            return BleStatus.Success;
        }

        public BleStatus IsDeviceBonded(AddressType peerAddressType, byte[] peerAddress)
        {
            if (peerAddress is null || peerAddress.Length != 6)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[258];
            command[ptr] = (byte)peerAddressType;
            ptr += 1;
            peerAddress.CopyTo(command, ptr);
            ptr += 6;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x0a4,
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

        public BleStatus NumericComparisonValueConfirmYesNo(ushort connectionHandle, bool confirmYesNo)
        {
            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte)(confirmYesNo ? 0x01 : 0x00);
            ptr += 1;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x0a5,
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

        public BleStatus PasskeyInput(ushort connectionHandle, KeyPressNotificationType inputType)
        {
            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte)inputType;
            ptr += 1;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x0a6,
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

        public BleStatus GetOobData(
            OobDataType oobDataType,
            out AddressType addressType,
            out byte[] address,
            out byte oobDataLength,
            out byte[] oobData)
        {
            addressType = 0;
            address = null;
            oobDataLength = 0;
            oobData = null;

            var ptr = 0;
            var command = new byte[258];
            command[ptr] = (byte)oobDataType;
            ptr += 1;

            const uint responseLength = 25;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x0a7,
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
            addressType = (AddressType)response[ptr];
            ptr += 1;

            address = new byte[6];

            Array.Copy(response, ptr, address, 0, 6);
            ptr += 6;
            oobDataLength = response[ptr];
            ptr += 1;

            oobData = new byte[6];
            Array.Copy(response, ptr, oobData, 0, 16);

            return BleStatus.Success;
        }

        public BleStatus SetOobData(
            byte deviceType,
            AddressType addressType,
            byte[] address,
            OobDataType oobDataType,
            byte oobDataLength,
            byte[] oobData)
        {
            if (address is null || address.Length != 6)
                throw new ArgumentException(nameof(address));

            if (oobData is null || oobData.Length != oobDataLength)
                throw new ArgumentException(nameof(oobData));

            var ptr = 0;
            var command = new byte[258];
            command[ptr] = deviceType;
            ptr += 1;
            command[ptr] = (byte)addressType;
            ptr += 1;
            address.CopyTo(command, ptr);
            ptr += 6;
            command[ptr] = (byte)oobDataType;
            ptr += 1;
            command[ptr] = oobDataLength;
            ptr += 1;
            oobData.CopyTo(command, ptr);
            ptr += oobDataLength;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x0a8,
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

        public BleStatus AddDeviceToResolvingList(
            byte resolvingListEntriesCount,
            DeviceAddressEntry[] whiteListIdentityEntry,
            bool clearResolvingList
        )
        {
            if (whiteListIdentityEntry is null || whiteListIdentityEntry.Length != resolvingListEntriesCount)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[258];
            command[ptr] = resolvingListEntriesCount;
            ptr += 1;

            for (int i = 0; i < resolvingListEntriesCount; i++)
            {
                command[ptr] = (byte)whiteListIdentityEntry[i].AddressType;
                ptr += 1;
                whiteListIdentityEntry[i].Address.CopyTo(command, ptr);
                ptr += 6;
            }

            command[ptr] = (byte)(clearResolvingList ? 0x01 : 0x00);
            ptr += 1;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x0a9,
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

        public BleStatus RemoveBondedDevice(
            AddressType peerIdentityAddressType,
            byte[] peerIdentityAddress
        )
        {
            if (peerIdentityAddress is null || peerIdentityAddress.Length != 6)
                throw new ArgumentException(nameof(peerIdentityAddress));

            var ptr = 0;
            var command = new byte[258];
            command[ptr] = (byte)peerIdentityAddressType;
            ptr += 1;
            peerIdentityAddress.CopyTo(command, ptr);
            ptr += 6;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x0aa,
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
    }
}
