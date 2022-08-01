// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.BlueNrg2.Aci;
using Iot.Device.BlueNrg2.Aci.Events;

namespace Iot.Device.BlueNrg2
{
    internal class Hci
    {
        private readonly TransportLayer _transportLayer;

        internal Hci(TransportLayer transportLayer)
        {
            _transportLayer = transportLayer;
        }

        public void Init()
        {
            _transportLayer.Reset();
        }

        public BleStatus Disconnect(ushort connectionHandle, byte reason)
        {
            var command = new byte[3];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, 0);
            command[2] = reason;
            var status = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x01,
                OpCodeCommand = 0x006,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = 3,
                ResponseParameter = status,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
        }

        public BleStatus ReadRemoteVersionInformation(ushort connectionHandle)
        {
            var command = BitConverter.GetBytes(connectionHandle);
            var status = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x01,
                OpCodeCommand = 0x006,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = 3,
                ResponseParameter = status,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
        }

        public BleStatus SetEventMask(byte[] eventMask)
        {
            if (eventMask != null && eventMask.Length != 8)
                throw new ArgumentException($"{nameof(eventMask)} needs to be an array of length 8");
            var status = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x03,
                OpCodeCommand = 0x001,
                CommandParameter = eventMask,
                CommandLength = 8,
                ResponseParameter = status,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
        }

        public BleStatus Reset()
        {
            var status = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x03,
                OpCodeCommand = 0x003,
                ResponseParameter = status,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
        }

        public BleStatus ReadTransmitPowerLevel(ushort connectionHandle, TransmitPowerLevelType type, ref int transmitPowerLevel)
        {
            var command = new byte[3];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, 0);
            command[2] = (byte)type;
            var response = new byte[4];
            var rq = new Request
            {
                OpCodeGroup = 0x03,
                OpCodeCommand = 0x02d,
                CommandParameter = command,
                CommandLength = 3,
                ResponseParameter = response,
                ResponseLength = 4
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            transmitPowerLevel = response[3];
            return BleStatus.Success;
        }

        public BleStatus ReadLocalVersionInformation(
            ref byte version,
            ref ushort revision,
            ref byte palVersion,
            ref ushort manufacturerName,
            ref ushort palSubVersion)
        {
            var response = new byte[9];
            var rq = new Request
            {
                OpCodeGroup = 0x04,
                OpCodeCommand = 0x001,
                ResponseParameter = response,
                ResponseLength = 9
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            version = response[1];
            revision = BitConverter.ToUInt16(response, 2);
            palVersion = response[4];
            manufacturerName = BitConverter.ToUInt16(response, 5);
            palSubVersion = BitConverter.ToUInt16(response, 7);
            return BleStatus.Success;
        }

        public BleStatus ReadLocalSupportedCommands(ref byte[] supportedCommands)
        {
            if (supportedCommands is null || supportedCommands.Length != 64)
                throw new ArgumentException($"{nameof(supportedCommands)} has to have a length of 64");
            var response = new byte[65];
            var rq = new Request
            {
                OpCodeGroup = 0x04,
                OpCodeCommand = 0x002,
                ResponseParameter = response,
                ResponseLength = 65
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            Array.Copy(response, 1, supportedCommands, 0, 64);
            return BleStatus.Success;
        }

        public BleStatus ReadLocalSupportedFeatures(ref byte[] features)
        {
            if (features is null || features.Length != 8)
                throw new ArgumentException($"{nameof(features)} has to have a length of 8");
            var response = new byte[9];
            var rq = new Request
            {
                OpCodeGroup = 0x04,
                OpCodeCommand = 0x003,
                ResponseParameter = response,
                ResponseLength = 9
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            Array.Copy(response, 1, features, 0, 8);
            return BleStatus.Success;
        }

        public BleStatus ReadBluetoothDeviceAddress(ref byte[] address)
        {
            if (address is null || address.Length != 6)
                throw new ArgumentException($"{nameof(address)} has to have a length of 6");
            var response = new byte[7];
            var rq = new Request
            {
                OpCodeGroup = 0x04,
                OpCodeCommand = 0x009,
                ResponseParameter = response,
                ResponseLength = 7
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            Array.Copy(response, 1, address, 0, 6);
            return BleStatus.Success;
        }

        public BleStatus ReadRssi(ushort connectionHandle, ref sbyte rssi)
        {
            var response = new byte[4];
            var rq = new Request
            {
                OpCodeGroup = 0x04,
                OpCodeCommand = 0x009,
                ResponseParameter = response,
                ResponseLength = 4
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            rssi = (sbyte)response[3];
            return BleStatus.Success;
        }

        public BleStatus LeSetEventMask(byte[] eventMask)
        {
            if (eventMask != null && eventMask.Length != 8)
                throw new ArgumentException($"{nameof(eventMask)} needs to be an array of length 8");
            var status = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x08,
                OpCodeCommand = 0x001,
                CommandParameter = eventMask,
                CommandLength = 8,
                ResponseParameter = status,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
        }

        public BleStatus LeReadBufferSize(ref ushort packetLength, ref byte packetCount)
        {
            var response = new byte[4];
            var rq = new Request
            {
                OpCodeGroup = 0x08,
                OpCodeCommand = 0x002,
                ResponseParameter = response,
                ResponseLength = 4
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            packetLength = BitConverter.ToUInt16(response, 1);
            packetCount = response[3];
            return BleStatus.Success;
        }

        public BleStatus LeReadLocalSupportedFeatures(ref byte[] features)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeSetRandomAddress(byte[] randomAddress)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeSetAdvertisingParameters(
            ushort minAdvertisingInterval,
            ushort maxAdvertisingInterval,
            AdvertisingType advertisingType,
            AddressType ownAddressType,
            AddressType peerAddressType,
            byte[] peerAddress,
            ChannelMap advertisingChannelMap,
            FilterPolicy advertisingFilterPolicy)
        {
            if (peerAddress is null || peerAddress.Length != 6)
                throw new ArgumentException($"{nameof(peerAddress)} needs to be an array of length 6");
            var command = new byte[15];
            BitConverter.GetBytes(minAdvertisingInterval).CopyTo(command, 0);
            BitConverter.GetBytes(maxAdvertisingInterval).CopyTo(command, 2);
            command[4] = (byte)advertisingType;
            command[5] = (byte)ownAddressType;
            command[6] = (byte)peerAddressType;
            peerAddress.CopyTo(command, 7);
            command[13] = (byte)advertisingChannelMap;
            command[14] = (byte)advertisingFilterPolicy;
            var status = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x08,
                OpCodeCommand = 0x006,
                CommandParameter = command,
                CommandLength = 15,
                ResponseParameter = status,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
        }

        public BleStatus LeReadAdvertisingChannelTxPower(ref sbyte transmitPowerLevel)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeSetAdvertisingData(byte advertisingDataLength, byte[] advertisingData)
        {
            if (advertisingData is null || advertisingData.Length != 31)
                throw new ArgumentException($"{nameof(advertisingData)} has to be an array of length 31");
            var command = new byte[32];
            command[0] = advertisingDataLength;
            advertisingData.CopyTo(command, 1);
            var status = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x08,
                OpCodeCommand = 0x008,
                CommandParameter = command,
                CommandLength = 32,
                ResponseParameter = status,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
        }

        public BleStatus LeSetScanResponseData(byte scanResponseDataLength, byte[] scanResponseData)
        {
            if (scanResponseDataLength > 31)
                throw new ArgumentException($"{nameof(scanResponseData)} has to be an array of length 31 or less");
            var command = new byte[32];
            command[0] = scanResponseDataLength;
            if (scanResponseData is not null)
                scanResponseData.CopyTo(command, 1);
            var status = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x08,
                OpCodeCommand = 0x009,
                CommandParameter = command,
                CommandLength = 32,
                ResponseParameter = status,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
        }

        public BleStatus LeSetAdvertiseEnable(bool enableAdvertising)
        {
            var command = new byte[1];
            command[0] = (byte)(enableAdvertising ? 0x01 : 0x00);
            var status = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x08,
                OpCodeCommand = 0x00a,
                CommandParameter = command,
                CommandLength = 1,
                ResponseParameter = status,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
        }

        public BleStatus LeSetScanParameters(
            ScanType scanType,
            ushort scanInterval,
            ushort scanWindow,
            AddressType ownAddressType,
            FilterPolicy scanningFilterPolicy)
        {
            var command = new byte[7];
            command[0] = (byte)scanType;
            BitConverter.GetBytes(scanInterval).CopyTo(command, 1);
            BitConverter.GetBytes(scanWindow).CopyTo(command, 3);
            command[5] = (byte)ownAddressType;
            command[6] = (byte)scanningFilterPolicy;
            var status = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x08,
                OpCodeCommand = 0x00b,
                CommandParameter = command,
                CommandLength = 7,
                ResponseParameter = status,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
        }

        public BleStatus LeSetScanEnable(bool enableAdvertising, bool filterDuplicates)
        {
            var command = new byte[2];
            command[0] = (byte)(enableAdvertising ? 0x01 : 0x00);
            command[1] = (byte)(filterDuplicates ? 0x01 : 0x00);
            var status = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x08,
                OpCodeCommand = 0x00c,
                CommandParameter = command,
                CommandLength = 2,
                ResponseParameter = status,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
        }

        public BleStatus LeCreateConnection(
            ushort scanInterval,
            ushort scanWindow,
            bool useWhiteList,
            AddressType peerAddressType,
            byte[] peerAddress,
            AddressType ownAddressType,
            ushort minimumConnectionInterval,
            ushort maximumConnectionInterval,
            ushort connectionLatency,
            ushort supervisionTimeout,
            ushort minimumConnectionLength,
            ushort maximumConnectionLength)
        {
            if (peerAddress is null || peerAddress.Length != 6)
                throw new ArgumentException($"{nameof(peerAddress)} needs to be an array of length 6");
            var command = new byte[24];
            BitConverter.GetBytes(scanInterval).CopyTo(command, 0);
            BitConverter.GetBytes(scanWindow).CopyTo(command, 2);
            command[4] = (byte)(useWhiteList ? 0x01 : 0x00);
            command[5] = (byte)peerAddressType;
            peerAddress.CopyTo(command, 6);
            command[12] = (byte)ownAddressType;
            BitConverter.GetBytes(minimumConnectionInterval).CopyTo(command, 13);
            BitConverter.GetBytes(maximumConnectionInterval).CopyTo(command, 15);
            BitConverter.GetBytes(connectionLatency).CopyTo(command, 17);
            BitConverter.GetBytes(supervisionTimeout).CopyTo(command, 19);
            BitConverter.GetBytes(minimumConnectionLength).CopyTo(command, 21);
            BitConverter.GetBytes(maximumConnectionLength).CopyTo(command, 23);
            var status = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x08,
                OpCodeCommand = 0x00d,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = 24,
                ResponseParameter = status,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
        }

        public BleStatus LeCreateConnectionCancel()
        {
            var status = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x08,
                OpCodeCommand = 0x00e,
                ResponseParameter = status,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
        }

        public BleStatus LeReadWhitelistSize(ref byte whiteListSize)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeClearWhiteList()
        {
            var status = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x08,
                OpCodeCommand = 0x010,
                ResponseParameter = status,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
        }

        public BleStatus LeAddDeviceToWhitelist(AddressType addressType, byte[] address)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeRemoveDeviceFromWhitelist(AddressType addressType, byte[] address)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeConnectionUpdate(
            ushort connectionHandle,
            ushort minimumConnectionInterval,
            ushort maximumConnectionInterval,
            ushort connectionLatency,
            ushort supervisionTimeout,
            ushort minimumConnectionLength,
            ushort maximumConnectionLength)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeSetHostChannelClassification(byte[] channelMap)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeReadChannelMap(ushort connectionHandle, byte[] channelMap)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeReadRemoteUsedFeatures(ushort connectionHandle)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeEncrypt(byte[] key, byte[] plaintextData, ref byte[] encryptedData)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeRandom(ref byte[] randomNumber)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeStartEncryption(ushort connectionHandle, byte[] randomNumber, ushort encryptedDiversifier, byte[] longTermKey)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeLongTermKeyRequestReply(ushort connectionHandle, byte[] longTermKey)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeLongTermKeyRequestedNegativeReply(ushort connectionHandle)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeReadSupportedStates(ref byte[] states)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeReceiverTest(byte rxFrequency)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeTransmitterTest(byte txFrequency, byte testDataLength, byte packetPayload)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeTestEnd(ref ushort numberOfPackets)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeSetDataLength(ushort connectionHandle, ushort transmissionOctets, ushort transmissionTime)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeReadSuggestedDefaultDataLength(ref ushort suggestedMaximumTransmissionOctets, ref ushort suggestedMaxTransmissionTime)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeWriteSuggestedDefaultDataLength(ushort suggestedMaximumTransmissionOctets, ushort suggestedMaxTransmissionTime)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeReadLocalP256PublicKey()
        {
            throw new NotImplementedException();
        }

        public BleStatus LeGenerateDhKey(byte[] remoteP256PublicKey)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeAddDeviceToResolvingList(AddressType peerIdentityAddressType, byte[] peerIdentityAddress, byte[] peerIrk, byte[] localIrk)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeRemoveDeviceFromResolvingList(AddressType peerIdentityAddressType, byte[] peerIdentityAddress)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeClearResolvingList()
        {
            throw new NotImplementedException();
        }

        public BleStatus LeReadResolvingListSize(ref byte resolvingListSize)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeReadPeerResolvableAddress(AddressType peerIdentityAddressType, byte[] peerIdentityAddress, ref byte[] peerResolvableAddress)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeReadLocalResolvableAddress(
            AddressType peerIdentityAddressType,
            byte[] peerIdentityAddress,
            ref byte[] localResolvableAddress)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeSetAddressResolutionEnable(bool enableAddressResolution)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeSetResolvablePrivateAddressTimeout(ushort rpaTimeout)
        {
            throw new NotImplementedException();
        }

        public BleStatus LeReadMaximumDataLength(
            ref ushort supportedMaximumOctets,
            ref ushort supportedMaximumTransmissionTime,
            ref ushort supportedMaximumReceivingOctets,
            ref ushort supportedMaxReceivingTime)
        {
            throw new NotImplementedException();
        }

        public void UserEventProcess()
        {
            _transportLayer.UserEventProcess();
        }
    }

    internal enum ScanType : byte
    {
        Passive = 0x00,
        Active = 0x01
    }

    internal enum FilterPolicy
    {
        ScanAnyRequestAny = 0x00,
        ScanWhitelistRequestAny = 0x01,
        ScanAnyRequestWhitelist = 0x02,
        ScanWhitelistRequestWhitelist = 0x03
    }

    [Flags]
    internal enum ChannelMap : byte
    {
        C37 = 0x01,
        C38 = 0x02,
        C39 = 0x04
    }

    internal enum TransmitPowerLevelType
    {
        Current = 0x00,
        Maximum = 0x01
    }
}
