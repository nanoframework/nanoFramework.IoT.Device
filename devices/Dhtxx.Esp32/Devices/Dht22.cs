﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;
using UnitsNet;

namespace Iot.Device.DHTxx.Esp32
{
    /// <summary>
    /// Temperature and Humidity Sensor DHT22
    /// </summary>
    public class Dht22 : DhtBase
    {
        /// <summary>
        /// Create a DHT22 sensor
        /// </summary>
        /// <param name="pinEcho">The pin number which is used as echo (GPIO number)</param>
        /// <param name="pinTrigger">The pin number which is used as trigger (GPIO number)</param>
        /// <param name="pinNumberingScheme">The GPIO pin numbering scheme</param>
        /// <param name="gpioController"><see cref="GpioController"/> related with operations on pins</param>
        /// <param name="shouldDispose"><see langword="true"/> to dispose the <see cref="GpioController"/></param>
        public Dht22(int pinEcho, int pinTrigger, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical, GpioController? gpioController = null, bool shouldDispose = true)
            : base(pinEcho, pinTrigger, pinNumberingScheme, gpioController, shouldDispose)
        {
        }

        internal override RelativeHumidity GetHumidity(byte[] readBuff) => RelativeHumidity.FromPercent((readBuff[0] << 8 | readBuff[1]) * 0.1);

        internal override Temperature GetTemperature(byte[] readBuff)
        {
            var temp = ((readBuff[2] & 0x7F) << 8 | readBuff[3]) * 0.1;
            // if MSB = 1 we have negative temperature
            temp = ((readBuff[2] & 0x80) == 0 ? temp : -temp);

            return Temperature.FromDegreesCelsius(temp);
        }
    }
}
