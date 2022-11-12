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
        /// Gets a value indicating whether last gas resistance value read was successful.
        /// </summary>
        public bool GasResistanceIsValid { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bme680ReadResult" /> class.
        /// </summary>
        /// <param name="temperature">The <see cref="Temperature"/> measurement.</param>
        /// <param name="temperatureIsValid">Last temperature value read was successful.</param>
        /// <param name="pressure">The <see cref="Pressure"/> measurement.</param>
        /// <param name="pressureIsValid">Last pressure value read was successful.</param>
        /// <param name="humidity">The humidity measurement.</param>
        /// <param name="humidityIsValid">Last humidity value read was successful.</param>
        /// <param name="gasResistance">The gas resistance measurement.</param>
        /// <param name="gasResistanceIsValid">Last gas resistance value read successful.</param>
        public Bme680ReadResult(Temperature temperature, bool temperatureIsValid, Pressure pressure, bool pressureIsValid, RelativeHumidity humidity, bool humidityIsValid, ElectricResistance gasResistance, bool gasResistanceIsValid)
            : base(temperature, temperatureIsValid, pressure, pressureIsValid, humidity, humidityIsValid)
        {
            GasResistance = gasResistance;

            GasResistanceIsValid = gasResistanceIsValid;
        }
    }
}
