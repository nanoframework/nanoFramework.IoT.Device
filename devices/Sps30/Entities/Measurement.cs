//
// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Buffers.Binary;

namespace Iot.Device.Sps30.Entities
{
    /// <summary>
    /// Measurement class that can house both response types (Float vs UInt16) by using doubles. Depending on the
    /// amount of bytes passed, we can deduct the type.
    /// </summary>
    public class Measurement
    {
        /// <summary>
        /// Parse the passed data into usable measurements. Depending on the amount of bytes passed, the originally
        /// requested type is deducted and parsed accordingly.
        /// </summary>
        /// <param name="data">The response data on the requested measurement</param>
        public Measurement(byte[] data)
        {
            if (data.Length >= 40)
            {
                // When we have 40 bytes of data, we assume Float was requested and will be parsed as such
                Format = MeasurementOutputFormat.Float;
                MassConcentrationPm10 = ReadSingleBigEndian(data, 0);
                MassConcentrationPm25 = ReadSingleBigEndian(data, 4);
                MassConcentrationPm40 = ReadSingleBigEndian(data, 8);
                MassConcentrationPm100 = ReadSingleBigEndian(data, 12);
                NumberConcentrationPm05 = ReadSingleBigEndian(data, 16);
                NumberConcentrationPm10 = ReadSingleBigEndian(data, 20);
                NumberConcentrationPm25 = ReadSingleBigEndian(data, 24);
                NumberConcentrationPm40 = ReadSingleBigEndian(data, 28);
                NumberConcentrationPm100 = ReadSingleBigEndian(data, 32);
                TypicalParticleSize = ReadSingleBigEndian(data, 36);
            }
            else if (data.Length >= 20)
            {
                // When we have 20 bytes of data, we assume UInt16 was requested and will be parsed as such
                Format = MeasurementOutputFormat.UInt16;
                MassConcentrationPm10 = BinaryPrimitives.ReadUInt16BigEndian(new SpanByte(data, 0, 2));
                MassConcentrationPm25 = BinaryPrimitives.ReadUInt16BigEndian(new SpanByte(data, 2, 2));
                MassConcentrationPm40 = BinaryPrimitives.ReadUInt16BigEndian(new SpanByte(data, 4, 2));
                MassConcentrationPm100 = BinaryPrimitives.ReadUInt16BigEndian(new SpanByte(data, 6, 2));
                NumberConcentrationPm05 = BinaryPrimitives.ReadUInt16BigEndian(new SpanByte(data, 8, 2));
                NumberConcentrationPm10 = BinaryPrimitives.ReadUInt16BigEndian(new SpanByte(data, 10, 2));
                NumberConcentrationPm25 = BinaryPrimitives.ReadUInt16BigEndian(new SpanByte(data, 12, 2));
                NumberConcentrationPm40 = BinaryPrimitives.ReadUInt16BigEndian(new SpanByte(data, 14, 2));
                NumberConcentrationPm100 = BinaryPrimitives.ReadUInt16BigEndian(new SpanByte(data, 16, 2));
                TypicalParticleSize = BinaryPrimitives.ReadUInt16BigEndian(new SpanByte(data, 18, 2));
            }
            else
            {
                throw new ApplicationException($"Not enough bytes received to parse a measurement");
            }
        }

        /// <summary>
        /// Reads a single (32-bit) float value using big endian byte ordering
        /// </summary>
        /// <param name="value">Buffer to read from</param>
        /// <param name="startIndex">Start index where the float value is located</param>
        /// <returns>The read float value</returns>
        /// <remarks>This method is needed for now since <see cref="BinaryPrimitives"/> does not support Single yet</remarks>
        private float ReadSingleBigEndian(byte[] value, int startIndex)
        {
            return BitConverter.IsLittleEndian? BitConverter.ToSingle(new byte[] { value[startIndex + 3], value[startIndex + 2], value[startIndex + 1], value[startIndex] }, 0) : BitConverter.ToSingle(value, startIndex);
        }

        /// <summary>
        /// The format assumed when parsing the data for this measurement instance.
        /// </summary>
        public MeasurementOutputFormat Format { get; protected set; }

        /// <summary>
        /// Mass Concentration PM1.0 [µg/m³]
        /// </summary>
        public double MassConcentrationPm10 { get; protected set; }

        /// <summary>
        /// Mass Concentration PM2.5 [µg/m³]
        /// </summary>
        public double MassConcentrationPm25 { get; protected set; }

        /// <summary>
        /// Mass Concentration PM4.0 [µg/m³]
        /// </summary>
        public double MassConcentrationPm40 { get; protected set; }

        /// <summary>
        /// Mass Concentration PM10.0 [µg/m³]
        /// </summary>
        public double MassConcentrationPm100 { get; protected set; }

        /// <summary>
        /// Number Concentration PM0.5 [#/cm³]
        /// </summary>
        public double NumberConcentrationPm05 { get; protected set; }

        /// <summary>
        /// Number Concentration PM1.0 [#/cm³]
        /// </summary>
        public double NumberConcentrationPm10 { get; protected set; }

        /// <summary>
        /// Number Concentration PM2.5 [#/cm³]
        /// </summary>
        public double NumberConcentrationPm25 { get; protected set; }

        /// <summary>
        /// Number Concentration PM4.0 [#/cm³]
        /// </summary>
        public double NumberConcentrationPm40 { get; protected set; }

        /// <summary>
        /// Number Concentration PM10.0 [#/cm³]
        /// </summary>
        public double NumberConcentrationPm100 { get; protected set; }

        /// <summary>
        /// Typical Particle Size depending on format (in µm for Float and nm for ushort, see <see cref="MeasurementOutputFormat"/>)
        /// </summary>
        public double TypicalParticleSize { get; protected set; }

        /// <summary>
        /// Conveniently show the measurement in a single string.
        /// </summary>
        /// <returns>The measurement as a convenient string</returns>
        public override string ToString()
        {
            return $"MassConcentration PM1.0={MassConcentrationPm10}, PM2.5={MassConcentrationPm25}, PM4.0={MassConcentrationPm40}, PM10.0={MassConcentrationPm100}, NumberConcentration PM0.5={NumberConcentrationPm05}, PM1.0={NumberConcentrationPm10}, PM2.5={NumberConcentrationPm25}, PM4.0={NumberConcentrationPm40}, PM10.0={NumberConcentrationPm100}, TypicalParticleSize={TypicalParticleSize}";
        }
    }
}
