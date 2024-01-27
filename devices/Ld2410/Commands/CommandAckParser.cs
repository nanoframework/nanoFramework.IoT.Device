﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;

namespace Iot.Device.Ld2410.Commands
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

            var dataSpan = new SpanByte(data);

            // read the next 2 bytes to find the length of the payload
            var payloadSize = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(++index));
            index++; // move the index one step forward to account for ushort size read above

            // make sure we actually have the payload to parse through
            if (data.Length - index < payloadSize)
            {
                return false;
            }

            // the next 2 bytes indicate the command this acknowledgment is for
            var commandWord = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(++index)); // 2 bytes
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
                        var protocolVersion = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(++index)); // 2 bytes
                        var bufferSize = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(index += 2)); // 2 bytes

                        result = new EnableConfigurationCommandAck(
                            status,
                            protocolVersion,
                            bufferSize);

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

                        var motionSensitivityLevelPerGate = new SpanByte(new byte[9]);
                        var staticSensitivityLevelPerGate = new SpanByte(new byte[9]);

                        dataSpan.Slice(start: ++index, length: motionSensitivityLevelPerGate.Length)
                            .CopyTo(motionSensitivityLevelPerGate);
                        index += motionSensitivityLevelPerGate.Length;

                        dataSpan.Slice(start: index, length: staticSensitivityLevelPerGate.Length)
                            .CopyTo(staticSensitivityLevelPerGate);
                        index += staticSensitivityLevelPerGate.Length;

                        var noOneDuration = TimeSpan.FromSeconds(BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(index)));

                        result = new ReadConfigurationsCommandAck(
                            status,
                            maxDistanceGate,
                            motionRangeGate,
                            staticRangeGate,
                            motionSensitivityLevelPerGate.ToArray(),
                            staticSensitivityLevelPerGate.ToArray(),
                            noOneDuration);

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
                        var firmwareType = BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(++index)); // 2 bytes

                        index++;

                        var minor = dataSpan[++index]; // 1 byte
                        var major = dataSpan[++index]; // 1 byte
                        var patch = new byte[4]
                                {
                                    dataSpan[index + 4],
                                    dataSpan[index + 3],
                                    dataSpan[index + 2],
                                    dataSpan[index + 1]
                                };

                        result = new ReadFirmwareVersionCommandAck(
                            status,
                            firmwareType,
                            major,
                            minor,
                            patch);

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
