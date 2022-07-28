// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System;
using System.Buffers.Binary;
using UnitsNet;

namespace Iot.Device.Scd30.Entities
{
    /// <summary>
    /// Provides the measurement values read from the sensor.
    /// </summary>
    public class Measurement
    {
        internal Measurement(SpanByte data)
        {
            Co2Concentration = VolumeConcentration.FromPartsPerMillion(BinaryPrimitives.ReadSingleBigEndian(data));
            Temperature = Temperature.FromDegreesCelsius(BinaryPrimitives.ReadSingleBigEndian(data.Slice(4)));
            RelativeHumidity = RelativeHumidity.FromPercent(BinaryPrimitives.ReadSingleBigEndian(data.Slice(8)));
        }

        /// <summary>
        /// Gets CO2 concentration. Value ranges from 0 to 10000 ppm.
        /// </summary>
        public VolumeConcentration Co2Concentration { get; private set; }

        /// <summary>
        /// Gets Temperature. Value ranges from -40 - 125°C.
        /// </summary>
        public Temperature Temperature { get; private set; }

        /// <summary>
        /// Gets Relative humidity. Value ranges from 0 to 100 % relative humidity.
        /// </summary>
        public RelativeHumidity RelativeHumidity { get; private set; }

        /// <summary>
        /// Conveniently show the measurement in a single string.
        /// </summary>
        /// <returns>The measurement as a convenient string.</returns>
        public override string ToString()
        {
            return $"{nameof(Co2Concentration)}={Co2Concentration.PartsPerMillion} ppm, {nameof(Temperature)}={Temperature.DegreesCelsius} °C, {nameof(RelativeHumidity)}={RelativeHumidity.Percent} %RH";
        }
    }
}
