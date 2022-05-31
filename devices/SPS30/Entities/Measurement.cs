//
// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

using System;
using Iot.Device.Sps30.Utils;

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
                MassConcentrationPM10 = BigEndianBitConverter.ToSingle(data, 0);
                MassConcentrationPM25 = BigEndianBitConverter.ToSingle(data, 4);
                MassConcentrationPM40 = BigEndianBitConverter.ToSingle(data, 8);
                MassConcentrationPM100 = BigEndianBitConverter.ToSingle(data, 12);
                NumberConcentrationPM05 = BigEndianBitConverter.ToSingle(data, 16);
                NumberConcentrationPM10 = BigEndianBitConverter.ToSingle(data, 20);
                NumberConcentrationPM25 = BigEndianBitConverter.ToSingle(data, 24);
                NumberConcentrationPM40 = BigEndianBitConverter.ToSingle(data, 28);
                NumberConcentrationPM40 = BigEndianBitConverter.ToSingle(data, 32);
                TypicalParticleSize = BigEndianBitConverter.ToSingle(data, 36);
            }
            else if (data.Length >= 20)
            {
                // When we have 20 bytes of data, we assume UInt16 was requested and will be parsed as such
                Format = MeasurementOutputFormat.UInt16;
                MassConcentrationPM10 = BigEndianBitConverter.ToUInt16(data, 0);
                MassConcentrationPM25 = BigEndianBitConverter.ToUInt16(data, 2);
                MassConcentrationPM40 = BigEndianBitConverter.ToUInt16(data, 4);
                MassConcentrationPM100 = BigEndianBitConverter.ToUInt16(data, 6);
                NumberConcentrationPM05 = BigEndianBitConverter.ToUInt16(data, 8);
                NumberConcentrationPM10 = BigEndianBitConverter.ToUInt16(data, 10);
                NumberConcentrationPM25 = BigEndianBitConverter.ToUInt16(data, 12);
                NumberConcentrationPM40 = BigEndianBitConverter.ToUInt16(data, 14);
                NumberConcentrationPM40 = BigEndianBitConverter.ToUInt16(data, 16);
                TypicalParticleSize = BigEndianBitConverter.ToUInt16(data, 18);
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
        public double MassConcentrationPM10 { get; protected set; }

        /// <summary>
        /// Mass Concentration PM2.5 [µg/m³]
        /// </summary>
        public double MassConcentrationPM25 { get; protected set; }

        /// <summary>
        /// Mass Concentration PM4.0 [µg/m³]
        /// </summary>
        public double MassConcentrationPM40 { get; protected set; }

        /// <summary>
        /// Mass Concentration PM10.0 [µg/m³]
        /// </summary>
        public double MassConcentrationPM100 { get; protected set; }

        /// <summary>
        /// Number Concentration PM0.5 [#/cm³]
        /// </summary>
        public double NumberConcentrationPM05 { get; protected set; }

        /// <summary>
        /// Number Concentration PM1.0 [#/cm³]
        /// </summary>
        public double NumberConcentrationPM10 { get; protected set; }

        /// <summary>
        /// Number Concentration PM2.5 [#/cm³]
        /// </summary>
        public double NumberConcentrationPM25 { get; protected set; }

        /// <summary>
        /// Number Concentration PM4.0 [#/cm³]
        /// </summary>
        public double NumberConcentrationPM40 { get; protected set; }

        /// <summary>
        /// Number Concentration PM10.0 [#/cm³]
        /// </summary>
        public double NumberConcentrationPM100 { get; protected set; }

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
            return $"MassConcentration PM1.0={MassConcentrationPM10}, PM2.5={MassConcentrationPM25}, PM4.0={MassConcentrationPM40}, PM10.0={MassConcentrationPM100}, NumberConcentration PM0.5={NumberConcentrationPM05}, PM1.0={NumberConcentrationPM10}, PM2.5={NumberConcentrationPM25}, PM4.0={NumberConcentrationPM40}, PM10.0={NumberConcentrationPM100}, TypicalParticleSize={TypicalParticleSize}";
        }
    }
}
