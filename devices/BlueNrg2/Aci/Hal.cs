// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci
{
	public class Hal
	{
		[Flags]
		public enum EventMask : uint
		{
			None = 0x00000000,
			ScanRequestReportEvent = 0x00000001
		}

		private readonly TransportLayer _transportLayer;

		internal Hal(TransportLayer transportLayer)
		{
			_transportLayer = transportLayer;
		}

		public BleStatus get_firmware_build_number(ref ushort buildNumber)
		{
			throw new NotImplementedException();
		}

		public BleStatus get_firmware_details(
			ref byte versionMajor,
			ref byte versionMinor,
			ref byte versionPatch,
			ref TransportLayerMode variant,
			ref ushort buildNumber,
			ref byte stackVersionMajor,
			ref byte stackVersionMinor,
			ref byte stackVersionPatch,
			ref bool isDevelopment,
			ref BleStackConfiguration stackVariant,
			ref ushort stackBuildNumber)
		{
			throw new NotImplementedException();
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
			ref byte dataLength,
			ref byte[] data)
		{
			if (data is null)
				throw new ArgumentNullException($"{nameof(data)} cannot be null");
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

		public BleStatus le_transmitter_test_packet_number(ref uint numberOfPackets)
		{
			throw new NotImplementedException();
		}

		public BleStatus tone_start(
			byte radioFrequencyChannel,
			FrequencyOffset offset)
		{
			throw new NotImplementedException();
		}

		public BleStatus tone_stop()
		{
			throw new NotImplementedException();
		}

		public BleStatus get_link_status(
			byte[] linkStatus,
			ushort[] linkConnectionHandle)
		{
			throw new NotImplementedException();
		}

		public BleStatus SetRadioActivityMask(RadioStateMask radioActivityMask)
		{
			throw new NotImplementedException();
		}

		public BleStatus get_anchor_period(
			ref uint anchorPeriod,
			ref uint maximumFreeSlot)
		{
			throw new NotImplementedException();
		}

		public BleStatus set_event_mask(EventMask eventMask)
		{
			throw new NotImplementedException();
		}

		public BleStatus updater_start()
		{
			throw new NotImplementedException();
		}

		public BleStatus updater_reboot()
		{
			throw new NotImplementedException();
		}

		public BleStatus get_updater_version(ref byte version)
		{
			throw new NotImplementedException();
		}

		public BleStatus get_updater_bufferSize(ref byte bufferSize)
		{
			throw new NotImplementedException();
		}

		public BleStatus updater_erase_blue_flag()
		{
			throw new NotImplementedException();
		}

		public BleStatus updater_reset_blue_flag()
		{
			throw new NotImplementedException();
		}

		public BleStatus updater_erase_sector(uint address)
		{
			throw new NotImplementedException();
		}

		public BleStatus updater_ProgramData_Block(
			uint address,
			ushort dataLength,
			byte[] data)
		{
			throw new NotImplementedException();
		}

		public BleStatus updater_read_Data_Block(
			uint address,
			ushort dataLength,
			byte[] data)
		{
			throw new NotImplementedException();
		}

		public BleStatus updater_CalculateCrc(
			uint address,
			byte numberOfSectors,
			ref uint crc)
		{
			throw new NotImplementedException();
		}

		public BleStatus updater_hw_version(ref byte hwVersion)
		{
			throw new NotImplementedException();
		}

		public BleStatus transmitter_test_packets(
			byte transmitterFrequency,
			byte lengthOfTestData,
			PacketPayloadType packetPayload,
			ushort numberOfPackets)
		{
			throw new NotImplementedException();
		}
	}
}
