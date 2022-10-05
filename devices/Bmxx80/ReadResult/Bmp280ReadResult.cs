// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using UnitsNet;

namespace Iot.Device.Bmxx80.ReadResult
{
    /// <summary>
    /// Contains a measurement result of a Bmp280 sensor.
    /// </summary>
    public class Bmp280ReadResult
    {
        /// <summary>
        /// Gets the collected temperature measurement.
        /// </summary>
        public Temperature Temperature { get; }

        /// <summary>
        /// Gets the collected pressure measurement.
        /// </summary>
        public Pressure Pressure { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bmp280ReadResult" /> class.
        /// </summary>
        /// <param name="temperature">The <see cref="Temperature"/> measurement.</param>
        /// <param name="pressure">The <see cref="Pressure"/> measurement.</param>
        public Bmp280ReadResult(Temperature temperature, Pressure pressure)
        {
            Temperature = temperature;
            Pressure = pressure;
        }
    }
}
