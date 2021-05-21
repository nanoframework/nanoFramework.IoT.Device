// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;
using UnitsNet;

namespace Iot.Device.DHTxx
{
    /// <summary>
    /// Temperature and Humidity Sensor DHT11
    /// </summary>
    public class Dht11 : DhtBase
    {
        /// <summary>
        /// Create a DHT11 sensor
        /// </summary>
        /// <param name="pin">The pin number (GPIO number)</param>
        /// <param name="pinNumberingScheme">The GPIO pin numbering scheme</param>
        /// <param name="gpioController"><see cref="GpioController"/> related with operations on pins</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public Dht11(int pin, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical, GpioController? gpioController = null, bool shouldDispose = true)
            : base(pin, pinNumberingScheme, gpioController, shouldDispose)
        {
        }

        internal override RelativeHumidity GetHumidity(byte[] readBuff) => RelativeHumidity.FromPercent(readBuff[0] + readBuff[1] * 0.1);

        internal override Temperature GetTemperature(byte[] readBuff)
        {
            var temp = readBuff[2] + readBuff[3] * 0.1;
            return Temperature.FromDegreesCelsius(temp);
        }
    }
}
