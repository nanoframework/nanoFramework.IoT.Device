//
// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

using Iot.Device.SPS30.Utils;

namespace Iot.Device.SPS30.Entities
{
    /// <summary>
    /// Parses the float-variant of the measurement response
    /// </summary>
    public class MeasurementFloat : Measurement
    {
        /// <summary>
        /// Parse the float-variant of the measurement response
        /// </summary>
        /// <param name="data">Raw data response from the sensor</param>
        public MeasurementFloat(byte[] data)
        {
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
    }

}
