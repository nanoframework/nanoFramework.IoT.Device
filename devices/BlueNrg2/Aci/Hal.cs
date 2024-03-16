// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci
{
    public class Hal
    {
        private readonly TransportLayer _transportLayer;

        internal Hal(TransportLayer transportLayer)
        {
            _transportLayer = transportLayer;
        }

        public BleStatus get_firmware_build_number(out ushort buildNumber)
        {
            buildNumber = 0;

            const uint responseLength = 3;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x000,
                ResponseParameter = response,
                ResponseLength = responseLength
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];

            buildNumber = BitConverter.ToUInt16(response, 1);

            return BleStatus.Success;
        }

        public BleStatus get_firmware_details(
            out byte versionMajor,
            out byte versionMinor,
            out byte versionPatch,
            out TransportLayerMode variant,
            out ushort buildNumber,
            out byte stackVersionMajor,
            out byte stackVersionMinor,
            out byte stackVersionPatch,
            out bool isDevelopment,
            out BleStackConfiguration stackVariant,
            out ushort stackBuildNumber)
        {
            versionMajor = 0;
            versionMinor = 0;
            versionPatch = 0;
            variant = 0;
            buildNumber = 0;
            stackVersionMajor = 0;
            stackVersionMinor = 0;
            stackVersionPatch = 0;
            isDevelopment = false;
            stackVariant = 0;
            stackBuildNumber = 0;

            const uint responseLength = 15;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x001,
                ResponseParameter = response,
                ResponseLength = responseLength
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];

            var ptr = 1;
            versionMajor = response[ptr];
            ptr += 1;
            versionMinor = response[ptr];
            ptr += 1;
            versionPatch = response[ptr];
            ptr += 1;
            variant = (TransportLayerMode)response[ptr];
            ptr += 1;
            buildNumber = BitConverter.ToUInt16(response, ptr);
            ptr += 2;
            stackVersionMajor = response[ptr];
            ptr += 1;
            stackVersionMinor = response[ptr];
            ptr += 1;
            stackVersionPatch = response[ptr];
            ptr += 1;
            isDevelopment = response[ptr] == 0x01;
            ptr += 1;
            stackVariant = (BleStackConfiguration)BitConverter.ToUInt16(response, ptr);
            ptr += 2;
            stackBuildNumber = BitConverter.ToUInt16(response, ptr);

            return BleStatus.Success;
        }

        public BleStatus WriteConfigData(
            Offset offset,
            byte length,
            byte[] value)
        {
            if (value is null || value.Length != length)
                throw new ArgumentException($"length of {nameof(value)} should be equal to {nameof(length)}");
            var command = new byte[2 + length];
            uint ptr = 0;
            command[ptr] = (byte)offset;
            ptr++;
            command[ptr] = length;
            ptr++;
            value.CopyTo(command, (int)ptr);
            ptr += length;

            var status = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x00C,
                CommandParameter = command,
                CommandLength = ptr,
                ResponseParameter = status,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
        }

        public BleStatus ReadConfigData(
            Offset offset,
            out byte dataLength,
            out byte[] data)
        {
            dataLength = 0;
            data = null;

            var command = new byte[1];
            command[0] = (byte)offset;

            var response = new byte[128];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x00D,
                CommandParameter = command,
                CommandLength = 1,
                ResponseParameter = response,
                ResponseLength = 128
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if ((BleStatus)response[0] != BleStatus.Success)
                return (BleStatus)response[0];
            dataLength = response[1];
            data = new byte[dataLength];
            Array.Copy(response, 2, data, 0, dataLength);
            return BleStatus.Success;
        }

        public BleStatus SetTransmitterPowerLevel(
            bool enableHighPower,
            byte powerAmplifierLevel)
        {
            var command = new byte[2];
            command[0] = (byte)(enableHighPower ? 0x01 : 0x00);
            command[1] = powerAmplifierLevel;

            var status = new byte[1];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x00F,
                CommandParameter = command,
                CommandLength = 2,
                ResponseParameter = status,
                ResponseLength = 1
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
        }

        public BleStatus LeTransmitterTestPacketNumber(out uint numberOfPackets)
        {
            numberOfPackets = 0;

            const uint responseLength = 5;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x014,
                ResponseParameter = response,
                ResponseLength = responseLength
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];

            numberOfPackets = BitConverter.ToUInt32(response, 1);

            return BleStatus.Success;
        }

        public BleStatus ToneStart(
            byte radioFrequencyChannel,
            FrequencyOffset offset)
        {
            var ptr = 0;
            var command = new byte[258];

            command[ptr] = radioFrequencyChannel;
            ptr += 1;
            command[ptr] = (byte)offset;
            ptr += 1;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x015,
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

        public BleStatus ToneStop()
        {
            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x016,
                ResponseParameter = response,
                ResponseLength = responseLength
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        public BleStatus GetLinkStatus(
            out byte[] linkStatus,
            out ushort[] linkConnectionHandle)
        {
            linkStatus = null;
            linkConnectionHandle = null;

            const uint responseLength = 25;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x017,
                ResponseParameter = response,
                ResponseLength = responseLength
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];

            linkStatus = new byte[8];
            linkConnectionHandle = new ushort[8];

            var ptr = 1;
            Array.Copy(response, ptr, linkStatus, 0, 8);
            for (int i = 0; i < 8; i++)
            {
                linkConnectionHandle[i] = BitConverter.ToUInt16(response, ptr);
                ptr += 2;
            }

            return BleStatus.Success;
        }

        public BleStatus SetRadioActivityMask(RadioStateMask radioActivityMask)
        {
            var ptr = 0;
            var command = new byte[258];

            BitConverter.GetBytes((ushort)radioActivityMask).CopyTo(command, ptr);
            ptr += 2;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x018,
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

        public BleStatus GetAnchorPeriod(
            out uint anchorPeriod,
            out uint maximumFreeSlot)
        {
            anchorPeriod = 0;
            maximumFreeSlot = 0;

            const uint responseLength = 9;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x019,
                ResponseParameter = response,
                ResponseLength = responseLength
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];

            var ptr = 1;
            anchorPeriod = BitConverter.ToUInt32(response, ptr);
            ptr += 4;
            maximumFreeSlot = BitConverter.ToUInt32(response, ptr);

            return BleStatus.Success;
        }

        public BleStatus SetEventMask(HalEventMask eventMask)
        {
            var ptr = 0;
            var command = new byte[258];

            BitConverter.GetBytes((uint)eventMask).CopyTo(command, ptr);
            ptr += 4;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x01a,
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

        public BleStatus UpdaterStart()
        {
            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x020,
                ResponseParameter = response,
                ResponseLength = responseLength
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        public BleStatus UpdaterReboot()
        {
            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x021,
                ResponseParameter = response,
                ResponseLength = responseLength
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        public BleStatus GetUpdaterVersion(out byte version)
        {
            version = 0;

            const uint responseLength = 2;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x022,
                ResponseParameter = response,
                ResponseLength = responseLength
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];

            var ptr = 1;
            version = response[ptr];

            return BleStatus.Success;
        }

        public BleStatus GetUpdaterBufferSize(out byte bufferSize)
        {
            bufferSize = 0;

            const uint responseLength = 2;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x023,
                ResponseParameter = response,
                ResponseLength = responseLength
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];

            var ptr = 1;
            bufferSize = response[ptr];

            return BleStatus.Success;
        }

        public BleStatus UpdaterEraseBlueFlag()
        {
            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x024,
                ResponseParameter = response,
                ResponseLength = responseLength
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        public BleStatus UpdaterResetBlueFlag()
        {
            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x025,
                ResponseParameter = response,
                ResponseLength = responseLength
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        public BleStatus UpdaterEraseSector(uint address)
        {
            var ptr = 0;
            var command = new byte[258];

            BitConverter.GetBytes(address).CopyTo(command, ptr);
            ptr += 4;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x026,
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

        public BleStatus UpdaterProgramDataBlock(
            uint address,
            ushort dataLength,
            byte[] data)
        {
            if (data is null || data.Length != dataLength)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[258];

            BitConverter.GetBytes(address).CopyTo(command, ptr);
            ptr += 4;
            BitConverter.GetBytes(dataLength).CopyTo(command, ptr);
            ptr += 2;
            data.CopyTo(command, ptr);
            ptr += dataLength;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x027,
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

        public BleStatus updater_read_Data_Block(
            uint address,
            ushort dataLength,
            out byte[] data)
        {
            data = null;

            var ptr = 0;
            var command = new byte[258];

            BitConverter.GetBytes(address).CopyTo(command, ptr);
            ptr += 4;
            BitConverter.GetBytes(dataLength).CopyTo(command, ptr);
            ptr += 2;

            const uint responseLength = 128;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x028,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];

            data = new byte[dataLength];

            Array.Copy(response, 1, data, 0, dataLength);

            return BleStatus.Success;
        }

        public BleStatus updater_CalculateCrc(
            uint address,
            byte numberOfSectors,
            out uint crc)
        {
            crc = 0;

            var ptr = 0;
            var command = new byte[258];

            BitConverter.GetBytes(address).CopyTo(command, ptr);
            ptr += 4;
            command[ptr] = numberOfSectors;
            ptr += 1;

            const uint responseLength = 5;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x029,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];

            crc = BitConverter.ToUInt32(response, 1);

            return BleStatus.Success;
        }

        public BleStatus updater_hw_version(out byte hwVersion)
        {
            hwVersion = 0;

            const uint responseLength = 2;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x02a,
                ResponseParameter = response,
                ResponseLength = responseLength
            };
            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];

            hwVersion = response[1];

            return BleStatus.Success;
        }

        public BleStatus transmitter_test_packets(
            byte transmitterFrequency,
            byte lengthOfTestData,
            PacketPayloadType packetPayload,
            ushort numberOfPackets)
        {
            var ptr = 0;
            var command = new byte[258];

            command[ptr] = transmitterFrequency;
            ptr += 1;
            command[ptr] = lengthOfTestData;
            ptr += 1;
            command[ptr] = (byte)packetPayload;
            ptr += 1;
            BitConverter.GetBytes(numberOfPackets).CopyTo(command, ptr);
            ptr += 2;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x027,
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
