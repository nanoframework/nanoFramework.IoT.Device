// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.IO;
using System.Net;
using System.Threading;
using Iot.Device.Imu;
using Iot.Device.Magnetometer;

Debug.WriteLine("Hello MPU9250!");

if (args.Length != 0)
{
    MagnetometerCalibrationDeepDive(Convert.ToInt32(args[0]));
}
else
{
    Debug.WriteLine("If you want to run a deep dive calibration data export, run this sample with an argument for the number of calibration cycles you want:");
    Debug.WriteLine("To run a calibration with 1000 sample and exporting all data: ./Mpu9250.sample 1000");
    MainTest();
}

void MagnetometerCalibrationDeepDive(int calibrationCount)
{
    I2cConnectionSettings mpui2CConnectionSettingmpus = new(1, Mpu9250.DefaultI2cAddress);
    using Mpu9250 mpu9250 = new(I2cDevice.Create(mpui2CConnectionSettingmpus));
    // In case you have an exception with AK8963. In some configuration AK8963 has its I2C address exposed
    // So you can try the following:
    // using Mpu9250 mpu9250 = new(I2cDevice.Create(mpui2CConnectionSettingmpus), i2CDeviceAk8963: I2cDevice.Create(new I2cConnectionSettings(1, Ak8963.DefaultI2cAddress)));
    mpu9250.MagnetometerOutputBitMode = OutputBitMode.Output16bit;
    mpu9250.MagnetometerMeasurementMode = MeasurementMode.ContinuousMeasurement100Hz;
    Debug.WriteLine("Please move the magnetometer during calibration");
    using StreamWriter ioWriter = new("mag.csv");
    // First we read the data without calibration at all
    Debug.WriteLine("Reading magnetometer data without calibration");
    ioWriter.WriteLine($"X;Y;Z");
    for (int i = 0; i < calibrationCount; i++)
    {
        try
        {
            var magne = mpu9250.ReadMagnetometerWithoutCorrection();
            ioWriter.WriteLine($"{magne.X};{magne.Y};{magne.Z}");
            // 10 ms = 100Hz, so waiting to make sure we have new data
            Thread.Sleep(10);
        }
        catch (TimeoutException)
        {
            Debug.WriteLine("Error reading");
        }
    }

    Debug.WriteLine("Performing calibration");
    // then we calibrate
    var magnetoBias = mpu9250.CalibrateMagnetometer(calibrationCount);
    ioWriter.WriteLine();
    ioWriter.WriteLine("Factory calibration data");
    ioWriter.WriteLine($"X;Y;Z");
    ioWriter.WriteLine($"{magnetoBias.X};{magnetoBias.Y};{magnetoBias.Z}");
    ioWriter.WriteLine();
    ioWriter.WriteLine("Magnetometer bias calibration data");
    ioWriter.WriteLine($"X;Y;Z");
    ioWriter.WriteLine($"{mpu9250.MagnometerBias.X};{mpu9250.MagnometerBias.Y};{mpu9250.MagnometerBias.Z}");
    ioWriter.WriteLine();
    // Finally we read the data again
    Debug.WriteLine("Reading magnetometer data including calibration");
    ioWriter.WriteLine($"X corr;Y corr;Z corr");
    for (int i = 0; i < calibrationCount; i++)
    {
        try
        {
            var magne = mpu9250.ReadMagnetometer();
            ioWriter.WriteLine($"{magne.X};{magne.Y};{magne.Z}");
            // 10 ms = 100Hz, so waiting to make sure we have new data
            Thread.Sleep(10);
        }
        catch (TimeoutException)
        {
            Debug.WriteLine("Error reading");
        }
    }

    Debug.WriteLine("Calibration deep dive over, file name is mag.csv");
}

void MainTest()
{
    I2cConnectionSettings mpui2CConnectionSettingmpus = new(1, Mpu9250.DefaultI2cAddress);
    using Mpu9250 mpu9250 = new Mpu9250(I2cDevice.Create(mpui2CConnectionSettingmpus));
    Debug.WriteLine($"Check version magnetometer: {mpu9250.GetMagnetometerVersion()}");
    Debug.WriteLine(
        "Magnetometer calibration is taking couple of seconds, please be patient! Please make sure you are not close to any magnetic field like magnet or phone.");
    Debug.WriteLine(
        "Please move your sensor as much as possible in all direction in space to get as many points in space as possible");
    var mag = mpu9250.CalibrateMagnetometer();
    Debug.WriteLine($"Hardware bias multiplicative:");
    Debug.WriteLine($"Mag X = {mag.X}");
    Debug.WriteLine($"Mag Y = {mag.Y}");
    Debug.WriteLine($"Mag Z = {mag.Z}");
    Debug.WriteLine($"Calculated corrected bias:");
    Debug.WriteLine($"Mag X = {mpu9250.MagnometerBias.X}");
    Debug.WriteLine($"Mag Y = {mpu9250.MagnometerBias.Y}");
    Debug.WriteLine($"Mag Z = {mpu9250.MagnometerBias.Z}");

    var resSelfTest = mpu9250.RunGyroscopeAccelerometerSelfTest();
    Debug.WriteLine($"Self test:");
    Debug.WriteLine($"Gyro X = {resSelfTest.GyroscopeAverage.X} vs >0.005");
    Debug.WriteLine($"Gyro Y = {resSelfTest.GyroscopeAverage.Y} vs >0.005");
    Debug.WriteLine($"Gyro Z = {resSelfTest.GyroscopeAverage.Z} vs >0.005");
    Debug.WriteLine($"Acc X = {resSelfTest.AccelerometerAverage.X} vs >0.005 & <0.015");
    Debug.WriteLine($"Acc Y = {resSelfTest.AccelerometerAverage.Y} vs >0.005 & <0.015");
    Debug.WriteLine($"Acc Z = {resSelfTest.AccelerometerAverage.Z} vs >0.005 & <0.015");
    Debug.WriteLine("Running Gyroscope and Accelerometer calibration");
    mpu9250.CalibrateGyroscopeAccelerometer();
    Debug.WriteLine("Calibration results:");
    Debug.WriteLine($"Gyro X bias = {mpu9250.GyroscopeBias.X}");
    Debug.WriteLine($"Gyro Y bias = {mpu9250.GyroscopeBias.Y}");
    Debug.WriteLine($"Gyro Z bias = {mpu9250.GyroscopeBias.Z}");
    Debug.WriteLine($"Acc X bias = {mpu9250.AccelerometerBias.X}");
    Debug.WriteLine($"Acc Y bias = {mpu9250.AccelerometerBias.Y}");
    Debug.WriteLine($"Acc Z bias = {mpu9250.AccelerometerBias.Z}");
    Debug.WriteLine("Press a key to continue");
    var readKey = Console.ReadKey();
    mpu9250.GyroscopeBandwidth = GyroscopeBandwidth.Bandwidth0250Hz;
    mpu9250.AccelerometerBandwidth = AccelerometerBandwidth.Bandwidth0460Hz;
    Console.Clear();

    while (!Console.KeyAvailable)
    {
        Console.CursorTop = 0;
        var gyro = mpu9250.GetGyroscopeReading();
        Debug.WriteLine($"Gyro X = {gyro.X,15}");
        Debug.WriteLine($"Gyro Y = {gyro.Y,15}");
        Debug.WriteLine($"Gyro Z = {gyro.Z,15}");
        var acc = mpu9250.GetAccelerometer();
        Debug.WriteLine($"Acc X = {acc.X,15}");
        Debug.WriteLine($"Acc Y = {acc.Y,15}");
        Debug.WriteLine($"Acc Z = {acc.Z,15}");
        Debug.WriteLine($"Temp = {mpu9250.GetTemperature().DegreesCelsius.ToString("0.00")} °C");
        var magne = mpu9250.ReadMagnetometer();
        Debug.WriteLine($"Mag X = {magne.X,15}");
        Debug.WriteLine($"Mag Y = {magne.Y,15}");
        Debug.WriteLine($"Mag Z = {magne.Z,15}");
        Thread.Sleep(100);
    }

    readKey = Console.ReadKey();
    // SetWakeOnMotion
    mpu9250.SetWakeOnMotion(300, AccelerometerLowPowerFrequency.Frequency0Dot24Hz);
    // You'll need to attach the INT pin to a GPIO and read the level. Once going up, you have
    // some data and the sensor is awake
    // In order to simulate this without a GPIO pin, you will see that the refresh rate is very low
    // Setup here at 0.24Hz which means, about every 4 seconds
    Console.Clear();

    while (!Console.KeyAvailable)
    {
        Console.CursorTop = 0;
        var acc = mpu9250.GetAccelerometer();
        Debug.WriteLine($"Acc X = {acc.X,15}");
        Debug.WriteLine($"Acc Y = {acc.Y,15}");
        Debug.WriteLine($"Acc Z = {acc.Z,15}");
        Thread.Sleep(100);
    }
}
