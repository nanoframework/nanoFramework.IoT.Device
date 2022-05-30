//
// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

namespace Iot.Device.SPS30.Entities
{
    /// <summary>
    /// Abstract Measurement class that can house both response types (Float vs UInt16) by using doubles.
    /// </summary>
    public abstract class Measurement
    {
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
