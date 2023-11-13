// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;

using UnitsNet;
using UnitsNet.Units;

namespace Iot.Device.Ld2410.Reporting
{
    internal static class ReportFrameParser
    {
        internal static bool TryParse(byte[] data, int index, out ReportFrame result)
        {
            result = null;

            if (data.Length <= 4)
            {
                return false;
            }

            // check if we have a measurement report frame to parse
            if (data[index] != ReportFrame.Header[0]
                || data[++index] != ReportFrame.Header[1]
                || data[++index] != ReportFrame.Header[2]
                || data[++index] != ReportFrame.Header[3])
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

            // make sure we actually have the payload to parse through
            if (dataSpan.Length - index < payloadSize)
            {
                return false;
            }

            // the next byte dictates the type of report: basic (0x02) vs engineering (0x01)
            switch (dataSpan[index += 2])
            {
                case (byte)ReportingType.BasicMode:
                    {
                        result = CreateBasicReportFrame(dataSpan.Slice(start: ++index, length: payloadSize));
                        return true;
                    }

                case (byte)ReportingType.EngineeringMode:
                    {
                        result = CreateEngineeringReportFrame(dataSpan.Slice(start: ++index, length: payloadSize));
                        return true;
                    }

                default:
                    {
                        return false;
                    }
            }
        }

        private static BasicReportFrame CreateBasicReportFrame(SpanByte data)
        {
            var index = 0;

            // the head "magic" byte is not here
            if (data[index] != 0xAA)
            {
                throw new FormatException();
            }

            return new BasicReportFrame
            {
                TargetState = (TargetState)data[++index], // 1 byte
                MovementTargetDistance = new Length(BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(++index)), LengthUnit.Centimeter), // 2 bytes
                MovementTargetEnergy = data[index += 2], // 1 byte
                StationaryTargetDistance = new Length(BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(++index)), LengthUnit.Centimeter), // 2 bytes
                StationaryTargetEnergy = data[index += 2],
                DetectionDistance = new Length(BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(++index)), LengthUnit.Centimeter) // 2 bytes
            };
        }

        private static EngineeringModeReportFrame CreateEngineeringReportFrame(SpanByte data)
        {
            var index = 0;

            // the head "magic" byte is not here
            if (data[index] != 0xAA)
            {
                throw new FormatException();
            }

            // at this point, we probably have a valid payload to construct an EngineeringModeReportFrame
            var engineeringFrame = new EngineeringModeReportFrame
            {
                TargetState = (TargetState)data[++index], // 1 byte
                MovementTargetDistance = new Length(BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(++index)), LengthUnit.Centimeter), // 2 bytes
                MovementTargetEnergy = data[index += 2], // 1 byte
                StationaryTargetDistance = new Length(BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(++index)), LengthUnit.Centimeter), // 2 bytes
                StationaryTargetEnergy = data[index += 2], // 1 byte
                DetectionDistance = new Length(BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(++index)), LengthUnit.Centimeter), // 2 bytes
                MaxMovingDistanceGate = data[index += 2], // 1 byte
                MaxStaticDistanceGate = data[++index], // 1 byte
            };

            for (var gate = 0; gate < 9; gate++)
            {
                engineeringFrame.GateData[gate] = new GateEnergy(
                    movementEnergy: data[++index],
                    staticEnergy: data[gate + 9]);
            }

            return engineeringFrame;
        }
    }
}
