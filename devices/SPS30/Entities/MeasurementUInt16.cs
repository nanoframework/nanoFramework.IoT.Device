//
// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

using Iot.Device.SPS30.Utils;

namespace Iot.Device.SPS30.Entities
{

    /// <summary>
    /// Parses the UInt16-variant of the measurement response
    /// </summary>
    public class MeasurementUInt16 : Measurement
    {
        /// <summary>
        /// Parses the Uint16-variant of the measurement response
        /// </summary>
        /// <param name="data">Raw data response from the sensor</param>
        public MeasurementUInt16(byte[] data)
        {
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
    }
}
