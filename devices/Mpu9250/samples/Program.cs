// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Imu;
using Iot.Device.Magnetometer;

Debug.WriteLine("Hello MPU9250!");

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

Debug.WriteLine("This will run the calibration on 1000, comment if you've done it already.");
Debug.WriteLine("Note that you'll be able to copy/paste the data easily from the debug output and create a csv file that you'll be able to exploit in Excel for example.");
MagnetometerCalibrationDeepDive(1000);
MainTest();

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
    
    // First we read the data without calibration at all
    Debug.WriteLine("Reading magnetometer data without calibration");
    Debug.WriteLine($"X;Y;Z");
    for (int i = 0; i < calibrationCount; i++)
    {
        try
        {
            var magne = mpu9250.ReadMagnetometerWithoutCorrection();
            Debug.WriteLine($"{magne.X};{magne.Y};{magne.Z}");
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
    Debug.WriteLine("");
    Debug.WriteLine("Factory calibration data");
    Debug.WriteLine($"X;Y;Z");
    Debug.WriteLine($"{magnetoBias.X};{magnetoBias.Y};{magnetoBias.Z}");
    Debug.WriteLine("");
    Debug.WriteLine("Magnetometer bias calibration data");
    Debug.WriteLine($"X;Y;Z");
    Debug.WriteLine($"{mpu9250.MagnometerBias.X};{mpu9250.MagnometerBias.Y};{mpu9250.MagnometerBias.Z}");
    Debug.WriteLine("");
    // Finally we read the data again
    Debug.WriteLine("Reading magnetometer data including calibration");
    Debug.WriteLine($"X corr;Y corr;Z corr");
    for (int i = 0; i < calibrationCount; i++)
    {
        try
        {
            var magne = mpu9250.ReadMagnetometer();
            Debug.WriteLine($"{magne.X};{magne.Y};{magne.Z}");
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
    Debug.WriteLine($"Gyro X = {resSelfTest.Gyroscope.X} vs >0.005");
    Debug.WriteLine($"Gyro Y = {resSelfTest.Gyroscope.Y} vs >0.005");
    Debug.WriteLine($"Gyro Z = {resSelfTest.Gyroscope.Z} vs >0.005");
    Debug.WriteLine($"Acc X = {resSelfTest.Accelerometer.X} vs >0.005 & <0.015");
    Debug.WriteLine($"Acc Y = {resSelfTest.Accelerometer.Y} vs >0.005 & <0.015");
    Debug.WriteLine($"Acc Z = {resSelfTest.Accelerometer.Z} vs >0.005 & <0.015");
    Debug.WriteLine("Running Gyroscope and Accelerometer calibration");
    mpu9250.CalibrateGyroscopeAccelerometer();
    Debug.WriteLine("Calibration results:");
    Debug.WriteLine($"Gyro X bias = {mpu9250.GyroscopeBias.X}");
    Debug.WriteLine($"Gyro Y bias = {mpu9250.GyroscopeBias.Y}");
    Debug.WriteLine($"Gyro Z bias = {mpu9250.GyroscopeBias.Z}");
    Debug.WriteLine($"Acc X bias = {mpu9250.AccelerometerBias.X}");
    Debug.WriteLine($"Acc Y bias = {mpu9250.AccelerometerBias.Y}");
    Debug.WriteLine($"Acc Z bias = {mpu9250.AccelerometerBias.Z}");

    mpu9250.GyroscopeBandwidth = GyroscopeBandwidth.Bandwidth0250Hz;
    mpu9250.AccelerometerBandwidth = AccelerometerBandwidth.Bandwidth0460Hz;

    Debug.WriteLine("This will read 200 positions in a row");
    for(int i=0; i<200; i++)
    {
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
    
    // SetWakeOnMotion
    mpu9250.SetWakeOnMotion(300, AccelerometerLowPowerFrequency.Frequency0Dot24Hz);
    // You'll need to attach the INT pin to a GPIO and read the level. Once going up, you have
    // some data and the sensor is awake
    // In order to simulate this without a GPIO pin, you will see that the refresh rate is very low
    // Setup here at 0.24Hz which means, about every 4 seconds

    Debug.WriteLine("This will read 10 positions in a row");
    for (int i = 0; i < 10; i++)
    {
        var acc = mpu9250.GetAccelerometer();
        Debug.WriteLine($"Acc X = {acc.X,15}");
        Debug.WriteLine($"Acc Y = {acc.Y,15}");
        Debug.WriteLine($"Acc Z = {acc.Z,15}");
        Thread.Sleep(100);
    }
}
