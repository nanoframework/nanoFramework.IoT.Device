// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using UnitsNet;

namespace Iot.Device.Sht4x
{
    /// <summary>
    /// Represents the data obtained from the SHT4x sensor.
    /// </summary>
    public sealed class Sht4XSensorData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sht4XSensorData" /> class.
        /// </summary>
        /// <param name="temperature">Temperature measurement.</param>
        /// <param name="humidity">Humidity measurement.</param>
        internal Sht4XSensorData(Temperature temperature, RelativeHumidity humidity)
        {
            Temperature = temperature;
            RelativeHumidity = humidity;
        }

        /// <summary>
        /// Gets a temperature measurement.
        /// </summary>
        public Temperature Temperature { get; }

        /// <summary>
        /// Gets a relative humidity measurement.
        /// </summary>
        public RelativeHumidity RelativeHumidity { get; }
    }
}