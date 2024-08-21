// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using UnitsNet;

namespace Iot.Device.Sdc4x
{
    /// <summary>
    /// Represents the data obtained from the Sdc4x sensor.
    /// </summary>
    public sealed class Sdc4xSensorData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sdc4xSensorData" /> class.
        /// </summary>
        /// <param name="temperature">Temperature measurement.</param>
        /// <param name="humidity">Humidity measurement.</param>
        /// <param name="co2">CO2 measurment.</param>
        internal Sdc4xSensorData(Temperature temperature, RelativeHumidity humidity, ushort co2)
        {
            Temperature = temperature;
            RelativeHumidity = humidity;
            CO2 = co2;
        }

        /// <summary>
        /// Gets a temperature measurement.
        /// </summary>
        public Temperature Temperature { get; }

        /// <summary>
        /// Gets a relative humidity measurement.
        /// </summary>
        public RelativeHumidity RelativeHumidity { get; }

        /// <summary>
        /// Gets a CO2 measurment.
        /// </summary>
        public ushort CO2 { get; }
    }
}