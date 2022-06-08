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
                MassConcentrationPm10 = BinaryPrimitives.ReadSingleBigEndian(new SpanByte(data, 0, 4));
                MassConcentrationPm25 = BinaryPrimitives.ReadSingleBigEndian(new SpanByte(data, 4, 4));
                MassConcentrationPm40 = BinaryPrimitives.ReadSingleBigEndian(new SpanByte(data, 8, 4));
                MassConcentrationPm100 = BinaryPrimitives.ReadSingleBigEndian(new SpanByte(data, 12, 4));
                NumberConcentrationPm05 = BinaryPrimitives.ReadSingleBigEndian(new SpanByte(data, 16, 4));
                NumberConcentrationPm10 = BinaryPrimitives.ReadSingleBigEndian(new SpanByte(data, 20, 4));
                NumberConcentrationPm25 = BinaryPrimitives.ReadSingleBigEndian(new SpanByte(data, 24, 4));
                NumberConcentrationPm40 = BinaryPrimitives.ReadSingleBigEndian(new SpanByte(data, 28, 4));
                NumberConcentrationPm100 = BinaryPrimitives.ReadSingleBigEndian(new SpanByte(data, 32, 4));
                TypicalParticleSize = BinaryPrimitives.ReadSingleBigEndian(new SpanByte(data, 36, 4));
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
