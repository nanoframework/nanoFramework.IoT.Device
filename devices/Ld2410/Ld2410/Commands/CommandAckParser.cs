using System;

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
                        var status = data[index] == 0x00 && data[++index] == 0x00; // 2 bytes
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
                        var status = data[index] == 0x00 && data[++index] == 0x00; // 2 bytes

                        result = new EndConfigurationCommandAck(status);

                        return true;
                    }
                case CommandWord.SetMaxDistanceGateAndNoOneDuration:
                    {
                        var status = data[index] == 0x00 && data[++index] == 0x00; // 2 bytes

                        result = new SetMaxDistanceGateAndNoOneDurationCommandAck(status);

                        return true;
                    }
                case CommandWord.ReadConfigurations:
                    {
                        var status = data[index] == 0x00 && data[++index] == 0x00; // 2 bytes

                        // skip the header (0xAA). Not needed.
                        index++;

                        result = new ReadConfigurationsCommandAck(status)
                        {
                            MaxDistanceGate = data[++index],
                            MaxMovingDistanceGate = data[++index],
                            MaxStaticDistanceGate = data[++index],

                            Gate0MotionSensitivity = data[++index],
                            Gate1MotionSensitivity = data[++index],
                            Gate2MotionSensitivity = data[++index],
                            Gate3MotionSensitivity = data[++index],
                            Gate4MotionSensitivity = data[++index],
                            Gate5MotionSensitivity = data[++index],
                            Gate6MotionSensitivity = data[++index],
                            Gate7MotionSensitivity = data[++index],
                            Gate8MotionSensitivity = data[++index],

                            Gate0RestSensitivity = data[++index],
                            Gate1RestSensitivity = data[++index],
                            Gate2RestSensitivity = data[++index],
                            Gate3RestSensitivity = data[++index],
                            Gate4RestSensitivity = data[++index],
                            Gate5RestSensitivity = data[++index],
                            Gate6RestSensitivity = data[++index],
                            Gate7RestSensitivity = data[++index],
                            Gate8RestSensitivity = data[++index],

                            NoOneDuration = TimeSpan.FromSeconds(BitConverter.ToUInt16(data, startIndex: ++index))
                        };

                        return true;
                    }
                default:
                    {
                        throw new FormatException();
                    }
            }
        }
    }
}
