using System;
using System.Reflection;

using Ld2410.Extensions;

namespace Ld2410.Commands
{
	internal static class CommandAckParser
	{
		internal static bool TryParse(byte[] data, int index, out CommandAckFrame result)
		{
			result = null;

			if (data.Length <= 4)
			{
				return false;
			}

			// check if we have a measurement report frame to parse
			if (data[index] != CommandFrame.Header[0]
				|| data[++index] != CommandFrame.Header[1]
				|| data[++index] != CommandFrame.Header[2]
				|| data[++index] != CommandFrame.Header[3])
			{
				return false;
			}

			// ensure we have a payload size in the buffer
			if (data.Length <= 6)
			{
				return false;
			}

			// read the next 2 bytes to find the length of the payload
			var payloadSize = BitConverter.ToUInt16(data, startIndex: ++index);
			index++; // move the index one step forward to account for ushort size read above

			// make sure we actually have the payload to parse through
			if (data.Length - index < payloadSize)
			{
				return false;
			}

			// the next 2 bytes indicate the command this acknowledgment is for
			var commandWord = BitConverter.ToUInt16(data, startIndex: ++index); // 2 bytes
			index += 2; // move the index one step forward to account for ushort size read above

			// according to protocol spec, the command word is modified in ACK frames like this:
			// Command Word | 0x0100
			commandWord -= 0x0100;

			// at this point, the AckType should be equal to one of the known commands
			switch ((CommandWord)commandWord)
			{
				case CommandWord.EnableConfiguration:
					{
						var status = GetStatus(data, ref index);
						var protocolVersion = BitConverter.ToUInt16(data, startIndex: ++index); // 2 bytes
						var bufferSize = BitConverter.ToUInt16(data, startIndex: index += 2); // 2 bytes

						result = new EnableConfigurationCommandAck(
							status,
							protocolVersion,
							bufferSize
							);

						return true;
					}
				case CommandWord.EndConfiguration:
					{
						var status = GetStatus(data, ref index);
						result = new EndConfigurationCommandAck(status);

						return true;
					}
				case CommandWord.SetMaxDistanceGateAndNoOneDuration:
					{
						var status = GetStatus(data, ref index);
						result = new SetMaxDistanceGateAndNoOneDurationCommandAck(status);

						return true;
					}
				case CommandWord.ReadConfigurations:
					{
						var status = GetStatus(data, ref index);

						// skip the header (0xAA). Not needed.
						index++;

						var maxDistanceGate = data[++index];
						var motionRangeGate = data[++index];
						var staticRangeGate = data[++index];

						var motionSensitivityLevelPerGate = new byte[9];
						var staticSensitivityLevelPerGate = new byte[9];

						Array.Copy(
							sourceArray: data,
							sourceIndex: ++index,
							destinationArray: motionSensitivityLevelPerGate,
							destinationIndex: 0,
							motionSensitivityLevelPerGate.Length
							);

						index += motionSensitivityLevelPerGate.Length;

						Array.Copy(
							sourceArray: data,
							sourceIndex: index,
							destinationArray: staticSensitivityLevelPerGate,
							destinationIndex: 0,
							staticSensitivityLevelPerGate.Length
							);

						index += staticSensitivityLevelPerGate.Length;

						var noOneDuration = TimeSpan.FromSeconds(BitConverter.ToUInt16(data, startIndex: index));

						result = new ReadConfigurationsCommandAck(
							status,
							maxDistanceGate,
							motionRangeGate,
							staticRangeGate,
							motionSensitivityLevelPerGate,
							staticSensitivityLevelPerGate,
							noOneDuration
							);

						return true;
					}
				case CommandWord.EnableEngineeringMode:
				case CommandWord.EndEngineeringMode:
					{
						var status = GetStatus(data, ref index);
						result = new SetEngineeringModeCommandAck((CommandWord)commandWord, status);

						return true;
					}
				case CommandWord.ConfigureGateSensitivity:
					{
						var status = GetStatus(data, ref index);
						result = new SetGateSensitivityCommandAck(status);

						return true;
					}
				case CommandWord.ReadFirmwareVersion:
					{
						var status = GetStatus(data, ref index);
						var firmwareType = BitConverter.ToUInt16(data, startIndex: ++index); // 2 bytes

						index++;

						var minor = data[++index]; // 1 byte
						var major = data[++index]; // 1 byte

						result = new ReadFirmwareVersionCommandAck(
							status,
							firmwareType: firmwareType,
							major: major,
							minor: minor,
							patch: new byte[4]
								{
									data[index + 4],
									data[index + 3],
									data[index + 2],
									data[index + 1]
								}
							);

						return true;
					}
				case CommandWord.SetBaudRate:
					{
						var status = GetStatus(data, ref index);
						result = new SetSerialPortBaudRateCommandAck(status);

						return true;
					}
				case CommandWord.Reset:
					{
						var status = GetStatus(data, ref index);
						result = new FactoryResetCommandAck(status);

						return true;
					}
				case CommandWord.Restart:
					{
						var status = GetStatus(data, ref index);
						result = new RestartCommandAck(status);

						return true;
					}
				default:
					{
						throw new FormatException();
					}
			}
		}

		private static bool GetStatus(byte[] data, ref int index)
		{
			return data[index] == 0x00 && data[++index] == 0x00;
		}
	}
}
