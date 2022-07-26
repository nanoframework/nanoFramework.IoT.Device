//
// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

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
            var bytes = data.ToArray();
            Co2ConcentrationInPpm = BinaryPrimitives.ReadSingleBigEndian(bytes);
            Temperature = Temperature.FromDegreesCelsius(BinaryPrimitives.ReadSingleBigEndian(new SpanByte(bytes, 4, 4)));
            RelativeHumidity = RelativeHumidity.FromPercent(BinaryPrimitives.ReadSingleBigEndian(new SpanByte(bytes, 8, 4)));
        }

        /// <summary>
        /// CO2 concentration (0-10000 ppm)
        /// </summary>
        /// <remarks>
        /// We are using the <see cref="float"/> unit since UnitsNet.Turbidity is missing an implementation for PPM.
        /// </remarks>
        public float Co2ConcentrationInPpm { get; private set; }

        /// <summary>
        /// Temperature (-40 - 125°C)
        /// </summary>
        public Temperature Temperature { get; private set; }

        /// <summary>
        /// Relative humidity (0-100 %RH)
        /// </summary>
        public RelativeHumidity RelativeHumidity { get; private set; }

        /// <summary>
        /// Conveniently show the measurement in a single string.
        /// </summary>
        /// <returns>The measurement as a convenient string</returns>
        public override string ToString()
        {
            return $"{nameof(Co2ConcentrationInPpm)}={Co2ConcentrationInPpm} ppm, {nameof(Temperature)}={Temperature.DegreesCelsius} °C, {nameof(RelativeHumidity)}={RelativeHumidity.Percent} %RH";
        }
    }
}
