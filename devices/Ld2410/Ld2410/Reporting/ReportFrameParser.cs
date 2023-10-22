using System;

using UnitsNet;
using UnitsNet.Units;

namespace Ld2410.Reporting
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

            // read the next 2 bytes to find the length of the payload
            var payloadSize = BitConverter.ToUInt16(data, startIndex: ++index);

            // make sure we actually have the payload to parse through
            if (data.Length - index < payloadSize)
            {
                return false;
            }

            // the next byte dictates the type of report: basic (0x02) vs engineering (0x01)
            switch (data[(index += 2)])
            {
                case (byte)ReportingType.BasicMode:
                    {
                        result = CreateBasicReportFrame(data, ++index, payloadSize);
                        return true;
                    }
                case (byte)ReportingType.EngineeringMode:
                    {
                        result = CreateEngineeringReportFrame(data, ++index, payloadSize);
                        return true;
                    }
                default:
                    {
                        throw new FormatException();
                    }
            }
        }

        private static BasicReportFrame CreateBasicReportFrame(byte[] data, int startIndex, ushort length)
        {
            if (data[startIndex] != 0xAA) // the head "magic" byte is not here
            {
                throw new FormatException();
            }

            // at this point, we probably have a valid payload to construct a BasicReportFrame
            return new BasicReportFrame
            {
                TargetState = (TargetState)data[++startIndex], // 1 byte
                MovementTargetDistance = new Length(BitConverter.ToInt16(data, ++startIndex), LengthUnit.Centimeter), // 2 bytes
                MovementTargetEnergy = data[(startIndex += 2)], // 1 byte
                StationaryTargetDistance = new Length(BitConverter.ToInt16(data, ++startIndex), LengthUnit.Centimeter), // 2 bytes
                StationaryTargetEnergy = data[(startIndex += 2)], // 1 byte
                DetectionDistance = new Length(BitConverter.ToInt16(data, ++startIndex), LengthUnit.Centimeter), // 2 bytes
            };
        }

        private static EngineeringModeReportFrame CreateEngineeringReportFrame(byte[] data, int startIndex, ushort length)
        {
            if (data[startIndex] != 0xAA) // the head "magic" byte is not here
            {
                throw new FormatException();
            }

            // at this point, we probably have a valid payload to construct an EngineeringModeReportFrame
            return new EngineeringModeReportFrame
            {
                TargetState = (TargetState)data[++startIndex], // 1 byte
                MovementTargetDistance = new Length(BitConverter.ToInt16(data, ++startIndex), LengthUnit.Centimeter), // 2 bytes
                MovementTargetEnergy = data[(startIndex += 2)], // 1 byte
                StationaryTargetDistance = new Length(BitConverter.ToInt16(data, ++startIndex), LengthUnit.Centimeter), // 2 bytes
                StationaryTargetEnergy = data[(startIndex += 2)], // 1 byte
                DetectionDistance = new Length(BitConverter.ToInt16(data, ++startIndex), LengthUnit.Centimeter), // 2 bytes
                MaxMovingDistanceGate = data[(startIndex += 2)], // 1 byte
                MaxStaticDistanceGate = data[++startIndex], // 1 byte

                Gate0MovementEnergy = data[++startIndex],
                Gate1MovementEnergy = data[++startIndex],
                Gate2MovementEnergy = data[++startIndex],
                Gate3MovementEnergy = data[++startIndex],
                Gate4MovementEnergy = data[++startIndex],
                Gate5MovementEnergy = data[++startIndex],
                Gate6MovementEnergy = data[++startIndex],
                Gate7MovementEnergy = data[++startIndex],
                Gate8MovementEnergy = data[++startIndex],

                Gate0StaticEnergy = data[++startIndex],
                Gate1StaticEnergy = data[++startIndex],
                Gate2StaticEnergy = data[++startIndex],
                Gate3StaticEnergy = data[++startIndex],
                Gate4StaticEnergy = data[++startIndex],
                Gate5StaticEnergy = data[++startIndex],
                Gate6StaticEnergy = data[++startIndex],
                Gate7StaticEnergy = data[++startIndex],
                Gate8StaticEnergy = data[++startIndex],
            };
        }
    }
}
