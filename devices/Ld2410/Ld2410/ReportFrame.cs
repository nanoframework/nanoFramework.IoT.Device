using System;

using UnitsNet;
using UnitsNet.Units;

namespace Ld2410
{
    public abstract class ReportFrame
    {
        public static byte[] Header = new byte[4] { 0xF4, 0xF3, 0xF2, 0xF1 };
        public static byte[] End = new byte[4] { 0xF8, 0xF7, 0xF6, 0xF5 };

        public ReportingDataType DataType { get; internal set; }
    }

    public class BasicReportFrame : ReportFrame
    {
        public TargetState TargetState { get; internal set; }

        public Length MovementTargetDistance { get; internal set; }

        public byte MovementTargetEnergy { get; internal set; }

        public Length StationaryTargetDistance { get; internal set; }

        public byte StationaryTargetEnergy { get; internal set; }

        public Length DetectionDistance { get; internal set; }

        internal BasicReportFrame()
        {
            this.DataType = ReportingDataType.BasicMode;
        }
    }

    public sealed class EngineeringModeReportFrame : BasicReportFrame
    {
        public byte MaxMovingDistanceGate { get; internal set; }

        public byte MaxStaticDistanceGate { get; internal set; }

        public byte Gate0MovingDistanceEnergy { get; internal set; }
        public byte Gate1MovingDistanceEnergy { get; internal set; }
        public byte Gate2MovingDistanceEnergy { get; internal set; }
        public byte Gate3MovingDistanceEnergy { get; internal set; }
        public byte Gate4MovingDistanceEnergy { get; internal set; }
        public byte Gate5MovingDistanceEnergy { get; internal set; }
        public byte Gate6MovingDistanceEnergy { get; internal set; }
        public byte Gate7MovingDistanceEnergy { get; internal set; }
        public byte Gate8MovingDistanceEnergy { get; internal set; }

        public byte Gate0StaticDistanceEnergy { get; internal set; }
        public byte Gate1StaticDistanceEnergy { get; internal set; }
        public byte Gate2StaticDistanceEnergy { get; internal set; }
        public byte Gate3StaticDistanceEnergy { get; internal set; }
        public byte Gate4StaticDistanceEnergy { get; internal set; }
        public byte Gate5StaticDistanceEnergy { get; internal set; }
        public byte Gate6StaticDistanceEnergy { get; internal set; }
        public byte Gate7StaticDistanceEnergy { get; internal set; }
        public byte Gate8StaticDistanceEnergy { get; internal set; }

        public byte[] AdditionalData { get; private set; }

        public EngineeringModeReportFrame()
        {
            this.DataType = ReportingDataType.EngineeringMode;
        }
    }

    public static class ReportFrameDecoder
    {
        public static bool TryParse(byte[] data, int startIndex, out ReportFrame result)
        {
            result = null;

            if (data.Length <= 4)
            {
                return false;
            }

            // check if we have a measurement report frame to parse
            if (data[startIndex] != ReportFrame.Header[0]
                || data[++startIndex] != ReportFrame.Header[1]
                || data[++startIndex] != ReportFrame.Header[2]
                || data[++startIndex] != ReportFrame.Header[3])
            {
                return false;
            }

            // ensure we have a payload size in the buffer
            if (data.Length <= 6)
            {
                return false;
            }

            // read the next 2 bytes to find the length of the payload
            var payloadSize = BitConverter.ToUInt16(data, startIndex: ++startIndex);

            // make sure we actually have the payload to parse through
            if (data.Length - startIndex < payloadSize)
            {
                return false;
            }

            // the next byte dictates the type of report: basic (0x02) vs engineering (0x01)
            switch (data[(startIndex += 2)])
            {
                case (byte)ReportingDataType.BasicMode:
                    {
                        result = CreateBasicReportFrame(data, ++startIndex, payloadSize);
                        return true;
                    }
                case (byte)ReportingDataType.EngineeringMode:
                    {
                        result = CreateEngineeringReportFrame(data, ++startIndex, payloadSize);
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

                Gate0MovingDistanceEnergy = data[++startIndex],
                Gate1MovingDistanceEnergy = data[++startIndex],
                Gate2MovingDistanceEnergy = data[++startIndex],
                Gate3MovingDistanceEnergy = data[++startIndex],
                Gate4MovingDistanceEnergy = data[++startIndex],
                Gate5MovingDistanceEnergy = data[++startIndex],
                Gate6MovingDistanceEnergy = data[++startIndex],
                Gate7MovingDistanceEnergy = data[++startIndex],
                Gate8MovingDistanceEnergy = data[++startIndex],

                Gate0StaticDistanceEnergy = data[++startIndex],
                Gate1StaticDistanceEnergy = data[++startIndex],
                Gate2StaticDistanceEnergy = data[++startIndex],
                Gate3StaticDistanceEnergy = data[++startIndex],
                Gate4StaticDistanceEnergy = data[++startIndex],
                Gate5StaticDistanceEnergy = data[++startIndex],
                Gate6StaticDistanceEnergy = data[++startIndex],
                Gate7StaticDistanceEnergy = data[++startIndex],
                Gate8StaticDistanceEnergy = data[++startIndex],
            };
        }
    }
}
