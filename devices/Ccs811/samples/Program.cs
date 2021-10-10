// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ccs811;
using Microsoft.Extensions.Logging;
using nanoFramework.Logging.Stream;
using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using UnitsNet;

////////////////////////////////////////////////////////
// Adjust the variables below to configure the sample //
////////////////////////////////////////////////////////

// set here the pin number for the WAKE UP pin, leave -1 to not use it
int wakeupPin = -1;

// set here the pin number for the RESET pin, leave -1 to not use it
int pinReset = -1;

// set here the pin number for the INTERRUPT pin, leave -1 to not use it
int pinInterrupt = -1;

// enable threshold interruption
bool enableThresholdInterruption = false;

// Which I2C address do you want to use? Use the first one if Address pin is to ground, use the second one if to VCC.
var addressChoice = Ccs811Sensor.I2cFirstAddress;

// select option for OperationMode
OperationMode operationMode = OperationMode.Idle;

// select test option 
TestOption testOption = TestOption.ReadAndDisplayInfo_10Times;


Debug.WriteLine("Hello CCS811!");
Debug.WriteLine($"Using 0x{addressChoice:X2} address");

Debug.WriteLine("");

Debug.WriteLine("Creating an instance of a CCS811 using native I2C/GPIO");

Debug.WriteLine("Creating an instance of a CCS811 using the platform drivers.");

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

using (var ccs811 = new Ccs811Sensor(I2cDevice.Create(new I2cConnectionSettings(3, addressChoice)), pinWake: wakeupPin, pinInterruption: pinInterrupt, pinReset: pinReset))
{
    Sample(ccs811);
}

void Sample(Ccs811Sensor ccs811)
{
    DisplayBasicInformation(ccs811);

    ccs811.OperationMode = operationMode;
    Debug.WriteLine($"Current operating mode: {ccs811.OperationMode}");

    Debug.WriteLine($"Warning: the sensor needs to run for 48h in operation mode {OperationMode.ConstantPower1Second} before getting accurate results. " +
        $"Also, every time you'll start the sensor, it will need approximately 20 minutes to get accurate results as well");

    if (pinInterrupt != -1)
    {
        Debug.WriteLine("Interruption mode selected.");

        if (enableThresholdInterruption)
        {
            Debug.WriteLine("Threshold interruption enabled");

            TestThresholdAndInterrupt(ccs811);
        }
        else
        {
            ccs811.MeasurementReady += Ccs811MeasurementReady;
            Thread.Sleep(10);
        }
    }
    else
    {
        switch (testOption)
        {
            case TestOption.ReadAndDisplayInfo_10Times:
                Debug.WriteLine("Read and display gas information for 10 times.");
                ReadAnDisplay(ccs811);
                break;
            case TestOption.ReadAndDisplayInfo_1000Times:
                Debug.WriteLine("Read and display gas information for 1000 times.");
                ReadAnDisplay(ccs811, 1000);
                break;
            case TestOption.ReadAndDisplayDetailInfo_10Times:
                Debug.WriteLine("Read and display detailed gas information for 10 times.");
                ReadAndDisplayDetails(ccs811);
                break;
            case TestOption.ReadAndDisplayDetailInfo_1000Times:
                Debug.WriteLine("Read and display detailed gas information for 1000 times.");
                ReadAndDisplayDetails(ccs811, 1000);
                break;
            case TestOption.ReadLoadAndChangeBaseline:
                Debug.WriteLine("Read, load and change back the baseline.");
                TestBaseline(ccs811);
                break;
            case TestOption.TestTemperatureHumidityChange:
                Debug.WriteLine("Test temperature and humidity changes.");
                TestTemperatureHumidityAdjustment(ccs811);
                break;
            case TestOption.ReadAndLogGasInfo_1000Times:
                Debug.WriteLine("Read and log gas information 10000 times.");
                Debug.WriteLine("Result file will be log.csv. The file is flushed on the disk every 100 results.");
                ReadAndLog(ccs811, 10000);
                break;
        }
    }

    Debug.WriteLine($"Current operating mode: {ccs811.OperationMode}, changing for {OperationMode.Idle}");
    ccs811.OperationMode = OperationMode.Idle;
}

void Ccs811MeasurementReady(object sender, MeasurementArgs args)
{
    Debug.WriteLine($"Measurement Event: Success: {args.MeasurementSuccess}, eCO2: {args.EquivalentCO2.PartsPerMillion} ppm, " +
        $"eTVOC: {args.EquivalentTotalVolatileOrganicCompound.PartsPerBillion} ppb, Current: {args.RawCurrentSelected.Microamperes} µA, " +
        $"ADC: {args.RawAdcReading} = {args.RawAdcReading * 1.65 / 1023} V.");
}

void DisplayBasicInformation(Ccs811Sensor ccs811)
{
    Debug.WriteLine($"Hardware identification: 0x{ccs811.HardwareIdentification:X2}, must be 0x81");
    Debug.WriteLine($"Hardware version: 0x{ccs811.HardwareVersion:X2}, must be 0x1X where any X is valid");
    Debug.WriteLine($"Application version: {ccs811.ApplicationVersion}");
    Debug.WriteLine($"Boot loader version: {ccs811.BootloaderVersion}");
}

void TestBaseline(Ccs811Sensor ccs811)
{
    var baseline = ccs811.BaselineAlgorithmCalculation;
    Debug.WriteLine($"Baseline calculation value: {baseline}, changing baseline");
    // Please refer to documentation, baseline is not a human readable number
    ccs811.BaselineAlgorithmCalculation = 50300;
    Debug.WriteLine($"Baseline calculation value: {ccs811.BaselineAlgorithmCalculation}, changing baseline for the previous one");
    ccs811.BaselineAlgorithmCalculation = baseline;
    Debug.WriteLine($"Baseline calculation value: {ccs811.BaselineAlgorithmCalculation}");
}

void TestTemperatureHumidityAdjustment(Ccs811Sensor ccs811)
{
    Debug.WriteLine("Drastically change the temperature and humidity to see the impact on the calculation " +
        "In real life, we'll get normal data and won't change them that often. " +
        "The system does not react the best way when shake like this");

    // First use with the default ones, no changes should appear
    Temperature temp = Temperature.FromDegreesCelsius(25);
    RelativeHumidity hum = RelativeHumidity.FromPercent(50);
    Debug.WriteLine($"Changing temperature and humidity reference to {temp.DegreesCelsius:0.00} C, {hum.Percent:0.0} %, baseline for calculation: {ccs811.BaselineAlgorithmCalculation}");
    ccs811.SetEnvironmentData(temp, hum);
    ReadAndDisplayDetails(ccs811, 100);
    
    // Changing with very different temperature
    temp = Temperature.FromDegreesCelsius(70);
    hum = RelativeHumidity.FromPercent(53.8);
    Debug.WriteLine($"Changing temperature and humidity reference to {temp.DegreesCelsius:0.00} C, {hum.Percent:0.0} %, baseline for calculation: {ccs811.BaselineAlgorithmCalculation}");
    ccs811.SetEnvironmentData(temp, hum);
    ReadAndDisplayDetails(ccs811, 100);

    temp = Temperature.FromDegreesCelsius(-25);
    hum = RelativeHumidity.FromPercent(0.5);
    
    Debug.WriteLine($"Changing temperature and humidity reference to {temp.DegreesCelsius:0.00} C, {hum.Percent:0.0} %, baseline for calculation: {ccs811.BaselineAlgorithmCalculation}");
    
    ccs811.SetEnvironmentData(temp, hum);
    ReadAndDisplayDetails(ccs811, 100);
    
    // Back to normal which still can lead to different results than initially
    // This is due to the baseline
    temp = Temperature.FromDegreesCelsius(25);
    hum = RelativeHumidity.FromPercent(50);
    
    Debug.WriteLine($"Changing temperature and humidity reference to {temp.DegreesCelsius:0.00} C, {hum.Percent:0.0} %, baseline for calculation: {ccs811.BaselineAlgorithmCalculation}");
    
    ccs811.SetEnvironmentData(temp, hum);
    ReadAndDisplayDetails(ccs811, 100);
}

void TestThresholdAndInterrupt(Ccs811Sensor ccs811)
{
    if (!ccs811.InterruptEnable)
    {
        Debug.WriteLine("Error: interrupt needs to be activated to run this test");
        return;
    }

    ccs811.MeasurementReady += Ccs811MeasurementReady;
    
    // Setting up a range where we will see something in a normal environment
    VolumeConcentration low = VolumeConcentration.FromPartsPerMillion(400);
    VolumeConcentration high = VolumeConcentration.FromPartsPerMillion(600);

    Debug.WriteLine($"Setting up {low.PartsPerMillion}-{high.PartsPerMillion} range, in clear environment, that should raise interrupts. Wait 3 minutes and change mode. Blow on the sensor and wait a bit.");
    Debug.WriteLine("Warning: only the first measurement to cross the threshold is raised.");

    ccs811.SetThreshold(low, high);
    DateTime dt = DateTime.UtcNow.AddMinutes(3);
    while (dt > DateTime.UtcNow)
    {
        Thread.Sleep(10);
    }

    low = VolumeConcentration.FromPartsPerMillion(15000);
    high = VolumeConcentration.FromPartsPerMillion(20000);
    
    Debug.WriteLine($"Changing threshold for {low.PartsPerMillion}-{high.PartsPerMillion}, a non reachable range in clear environment. No measurement should appear in next 3 minutes");
    
    dt = DateTime.UtcNow.AddMinutes(3);
    
    ccs811.SetThreshold(low, high);
    while (dt > DateTime.UtcNow)
    {
        Thread.Sleep(10);
    }
}

void ReadAndLog(Ccs811Sensor ccs811, int count = 10)
{
    const string logFilePath = "C:\\log.csv";

    var loggerFactory = new StreamLoggerFactory(logFilePath);
    var _logger = loggerFactory.CreateLogger("data");

    // header
    _logger.LogInformation("Time;Success;eCO2 (ppm);eTVOC (ppb);Current (µA);ADC;ADC (V);Baseline");

    for (int i = 0; i < count; i++)
    {
        while (!ccs811.IsDataReady)
        {
            Thread.Sleep(10);
        }

        var error = ccs811.TryReadGasData(out VolumeConcentration eCO2, out VolumeConcentration eTVOC, out ElectricCurrent curr, out int adc);
        _logger.LogInformation($"{DateTime.UtcNow};{error};{eCO2.PartsPerMillion};{eTVOC.PartsPerBillion};{curr.Microamperes};{adc};{adc * 1.65 / 1023};{ccs811.BaselineAlgorithmCalculation}");
    }
}

void ReadAnDisplay(Ccs811Sensor ccs811, int count = 10)
{
    for (int i = 0; i < count; i++)
    {
        while (!ccs811.IsDataReady)
        {
            Thread.Sleep(10);
        }

        var error = ccs811.TryReadGasData(out VolumeConcentration eCO2, out VolumeConcentration eTVOC);
        
        Debug.WriteLine($"Success: {error}, eCO2: {eCO2.PartsPerMillion} ppm, eTVOC: {eTVOC.PartsPerBillion} ppb");
    }
}

void ReadAndDisplayDetails(Ccs811Sensor ccs811, int count = 10)
{
    for (int i = 0; i < count; i++)
    {
        while (!ccs811.IsDataReady)
        {
            Thread.Sleep(10);
        }

        var error = ccs811.TryReadGasData(out VolumeConcentration eCO2, out VolumeConcentration eTVOC, out ElectricCurrent curr, out int adc);
        
        Debug.WriteLine($"Success: {error}, eCO2: {eCO2.PartsPerMillion} ppm, eTVOC: {eTVOC.PartsPerBillion} ppb, Current: {curr.Microamperes} µA, ADC: {adc} = {adc * 1.65 / 1023} V.");
    }
}

enum TestOption : byte
{
    /// <summary>
    /// Read and display gas information for 10 times.
    /// </summary>
    ReadAndDisplayInfo_10Times,

    /// <summary>
    /// Read and display gas information for 1000 times.
    /// </summary>
    ReadAndDisplayInfo_1000Times,

    /// <summary>
    /// Read and display detailed gas information for 10 times.
    /// </summary>
    ReadAndDisplayDetailInfo_10Times,

    /// <summary>
    /// Read and display detailed gas information for 1000 times.
    /// </summary>
    ReadAndDisplayDetailInfo_1000Times,

    /// <summary>
    /// Read, load and change back the baseline.
    /// </summary>
    ReadLoadAndChangeBaseline,

    /// <summary>
    /// Test temperature and humidity changes.
    /// </summary>
    TestTemperatureHumidityChange,

    /// <summary>
    /// Read and log gas information 10000 times.
    /// </summary>
    ReadAndLogGasInfo_1000Times
}
