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
            index++; // move the index one step forward to account for ushort size read above

            // according to protocol spec, the command word is modified in ACK frames like this:
            // Command Word | 0x0100
            // protocol is in little endian, the following operations account for that.
            // example: an ACK for 0xFF01 is for command 0x00FF because 0x00FF | 0x0100 = 0x01FF
            // 0x01FF in little endian is 0xFF01
            // to get the original command back, subtract 0x0001 and bit shift the result
            commandWord -= 0x0001;
            commandWord >>= 0x08;

            // at this point, the AckType should be equal to one of the known commands
            switch ((CommandWord)commandWord)
            {
                case CommandWord.EnableConfiguration:
                    {
                        var status = data[++index] == 0x00 && data[++index] == 0x00; // 2 bytes
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
                        var status = data[++index] == 0x00 && data[++index] == 0x00; // 2 bytes

                        result = new EndConfigurationCommandAck(status);

                        return true;
                    }
                case CommandWord.SetMaxDistanceGateAndNoOneDuration:
                    {
                        var status = data[++index] == 0x00 && data[++index] == 0x00; // 2 bytes

                        result = new SetMaxDistanceGateAndNoOneDurationCommandAck(status);

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
