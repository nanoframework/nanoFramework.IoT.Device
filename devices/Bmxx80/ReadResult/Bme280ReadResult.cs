// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using UnitsNet;

namespace Iot.Device.Bmxx80.ReadResult
{
    /// <summary>
    /// Contains a measurement result of a Bme280 sensor.
    /// </summary>
    public class Bme280ReadResult : Bmp280ReadResult
    {
        /// <summary>
        /// Collected humidity measurement.
        /// </summary>
        public RelativeHumidity Humidity { get; }

      /// <summary>
      /// BHL...
      /// </summary>
      public bool HumidityIsValid { get; set; }

      /// <summary>
      /// Initialize a new instance of the <see cref="Bme280ReadResult"/> class.
      /// </summary>
      /// <param name="temperature">The <see cref="Temperature"/> measurement.</param>
      /// <param name="temperatureIsValid">BHL...measurement.</param>
      /// <param name="pressure">The <see cref="Pressure"/> measurement.</param>
      /// <param name="pressureIsValid">BHL...</param>
      /// <param name="humidity">The humidity measurement.</param>
      /// <param name="humidityIsValid">BHL...</param>
      public Bme280ReadResult(Temperature temperature, bool temperatureIsValid, Pressure pressure, bool pressureIsValid, 
         RelativeHumidity humidity, bool humidityIsValid)
            : base(temperature, temperatureIsValid, pressure, pressureIsValid)
        {
            Humidity = humidity;

            HumidityIsValid = humidityIsValid;
        }
    }
}
