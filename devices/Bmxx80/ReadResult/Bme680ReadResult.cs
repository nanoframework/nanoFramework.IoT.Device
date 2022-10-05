// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using UnitsNet;

namespace Iot.Device.Bmxx80.ReadResult
{
    /// <summary>
    /// Contains a measurement result of a Bme280 sensor.
    /// </summary>
    public class Bme680ReadResult : Bme280ReadResult
    {
        /// <summary>
        /// Gets the collected gas resistance measurement. NaN if no measurement was performed.
        /// </summary>
        public ElectricResistance GasResistance { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bme680ReadResult" /> class.
        /// </summary>
        /// <param name="temperature">The <see cref="Temperature"/> measurement.</param>
        /// <param name="pressure">The <see cref="Pressure"/> measurement.</param>
        /// <param name="humidity">The humidity measurement.</param>
        /// <param name="gasResistance">The gas resistance measurement.</param>
        public Bme680ReadResult(Temperature temperature, Pressure pressure, RelativeHumidity humidity, ElectricResistance gasResistance)
            : base(temperature, pressure, humidity)
        {
            GasResistance = gasResistance;
        }
    }
}
