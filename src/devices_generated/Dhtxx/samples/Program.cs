// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Common;
using Iot.Device.DHTxx;

Debug.WriteLine("Hello DHT!");
Debug.WriteLine("Select the DHT sensor you want to use:");
Debug.WriteLine(" 1. DHT10 on I2C");
Debug.WriteLine(" 2. DHT11 on GPIO");
Debug.WriteLine(" 3. DHT12 on GPIO");
Debug.WriteLine(" 4. DHT21 on GPIO");
Debug.WriteLine(" 5. DHT22 on GPIO");
var choice = Console.ReadKey();
Debug.WriteLine();
if (choice.KeyChar == '1')
{
    Debug.WriteLine("Press any key to stop the reading");
    // Init DHT10 through I2C
    I2cConnectionSettings settings = new(1, Dht10.DefaultI2cAddress);
    I2cDevice device = I2cDevice.Create(settings);

    using Dht10 dht = new(device);
    Dht(dht);
    return;
}

Debug.WriteLine("Which pin do you want to use in the logical pin schema?");
var pinChoise = Console.ReadLine();
int pin;
try
{
    pin = Convert.ToInt32(pinChoise);
}
catch (Exception ex) when (ex is FormatException || ex is OverflowException)
{
    Debug.WriteLine("Can't convert pin number.");
    return;
}

Debug.WriteLine("Press any key to stop the reading");

switch (choice.KeyChar)
{
    case '2':
        Debug.WriteLine($"Reading temperature and humidity on DHT11, pin {pin}");
        using (Dht11 dht11 = new(pin))
        {
            Dht(dht11);
        }

        break;
    case '3':
        Debug.WriteLine($"Reading temperature and humidity on DHT12, pin {pin}");
        using (Dht12 dht12 = new(pin))
        {
            Dht(dht12);
        }

        break;
    case '4':
        Debug.WriteLine($"Reading temperature and humidity on DHT21, pin {pin}");
        using (Dht21 dht21 = new(pin))
        {
            Dht(dht21);
        }

        break;
    case '5':
        Debug.WriteLine($"Reading temperature and humidity on DHT22, pin {pin}");
        using (Dht22 dht22 = new(pin))
        {
            Dht(dht22);
        }

        break;
    default:
        Debug.WriteLine("Please select one of the option.");
        break;
}

void Dht(DhtBase dht)
{
    while (!Console.KeyAvailable)
    {
        var temp = dht.Temperature;
        var hum = dht.Humidity;
        // You can only display temperature and humidity if the read is successful otherwise, this will raise an exception as
        // both temperature and humidity are NAN
        if (dht.IsLastReadSuccessful)
        {
            Debug.WriteLine($"Temperature: {temp.DegreesCelsius}\u00B0C, Relative humidity: {hum.Percent}%");

            // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
            Debug.WriteLine(
                $"Heat index: {WeatherHelper.CalculateHeatIndex(temp, hum).DegreesCelsius:0.#}\u00B0C");
            Debug.WriteLine(
                $"Dew point: {WeatherHelper.CalculateDewPoint(temp, hum).DegreesCelsius:0.#}\u00B0C");
        }
        else
        {
            Debug.WriteLine("Error reading DHT sensor");
        }

        // You must wait some time before trying to read the next value
        Thread.Sleep(2000);
    }
}
