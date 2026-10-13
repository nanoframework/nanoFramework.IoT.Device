// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Iot.Device.Max30100;
using nanoFramework.Hardware.Esp32;

namespace Iot.Device.Max30100.Samples.Esp32S3BoxLite
{
    internal static class Program
    {
        private const int I2cDataPin = 41;
        private const int I2cClockPin = 40;
        private const int DisplayRefreshMilliseconds = 250;

        private static Max30100PulseOximeterWorker _pulseOximeter;
        private static BoxLiteDisplay _display;
        private static bool _sensorDisconnected;

        public static void Main()
        {
            _display = new BoxLiteDisplay();
            _display.Initialize();
            _display.ShowStarting();

            Debug.WriteLine("Connecting MAX30100 on SDA GPIO41 and SCL GPIO40.");
            Configuration.SetPinFunction(I2cDataPin, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(I2cClockPin, DeviceFunction.I2C1_CLOCK);

            I2cConnectionSettings settings = new I2cConnectionSettings(1, Max30100.DefaultI2cAddress);
            I2cDevice i2cDevice = new I2cDevice(settings);
            Max30100 sensor = new Max30100(i2cDevice);

            Max30100PulseOximeterOptions options = new Max30100PulseOximeterOptions
            {
                SampleRate = 50,
                WindowSeconds = 8,
            };

            _pulseOximeter = new Max30100PulseOximeterWorker(options, queueSeconds: 2);

            Thread displayThread = new Thread(UpdateDisplay)
            {
                Priority = ThreadPriority.Normal,
            };

            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            _sensorDisconnected = true;
            displayThread.Start();

            uint processedDroppedSamples = 0;

            while (true)
            {
                if (_sensorDisconnected)
                {
                    try
                    {
                        byte reconnectPartId = sensor.PartId;
                        if (reconnectPartId == Max30100.ExpectedPartId)
                        {
                            ConfigureSensor(sensor);
                            processedDroppedSamples = sensor.DroppedSampleCount;
                            _pulseOximeter.Reset();
                            _sensorDisconnected = false;
                            Debug.WriteLine("MAX30100 connected and configured.");
                        }
                        else
                        {
                            Debug.WriteLine($"Unexpected I2C device while reconnecting: 0x{reconnectPartId:X2}.");
                        }
                    }
                    catch (IOException exception)
                    {
                        ReportDisconnectedSensor(exception);
                    }
                    catch (System.InvalidOperationException exception)
                    {
                        ReportDisconnectedSensor(exception);
                    }

                    if (_sensorDisconnected)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                }

                try
                {
                    while (sensor.TryReadSample(out Max30100Reading reading))
                    {
                        if (sensor.DroppedSampleCount != processedDroppedSamples)
                        {
                            processedDroppedSamples = sensor.DroppedSampleCount;
                            _pulseOximeter.Reset();
                        }

                        _pulseOximeter.AddSample(reading);
                    }
                }
                catch (IOException exception)
                {
                    Debug.WriteLine($"MAX30100 disconnected: {exception.Message}");
                    _sensorDisconnected = true;
                    _pulseOximeter.Reset();
                }

                Thread.Sleep(2);
            }
        }

        private static void ConfigureSensor(Max30100 sensor)
        {
            sensor.Reset();
            sensor.Configure(
                Max30100Mode.SpO2,
                Max30100SampleRate.SamplesPerSecond50,
                Max30100PulseWidth.Microseconds1600,
                Max30100LedCurrent.Milliamps27Point1,
                Max30100LedCurrent.Milliamps40Point2);
        }

        private static void ReportDisconnectedSensor(System.Exception exception)
        {
            _sensorDisconnected = true;
            _pulseOximeter.Reset();
            Debug.WriteLine($"MAX30100 disconnected or failed to reset: {exception.Message}");
        }

        private static void UpdateDisplay()
        {
            while (true)
            {
                if (_sensorDisconnected)
                {
                    _display.ShowSensorDisconnected();
                }
                else if (!_pulseOximeter.FingerDetected)
                {
                    _display.ShowAcquiring("PLACE FINGER", "Cover the sensor and hold still", 0, 0);
                }
                else if (_pulseOximeter.IsSettling)
                {
                    _display.ShowAcquiring("STABILIZING", "Keep your finger steady", 0, 0);
                }
                else if (!_pulseOximeter.HasValidHeartRate)
                {
                    _display.ShowAcquiring(
                        "ACQUIRING SIGNAL",
                        GetAcquisitionStatus(_pulseOximeter.SignalDiagnostic),
                        _pulseOximeter.AnalysisWindowProgress,
                        _pulseOximeter.SignalQuality);
                }
                else
                {
                    _display.ShowMeasurements(
                        _pulseOximeter.HeartRate,
                        _pulseOximeter.IsHeartRateHeld,
                        _pulseOximeter.HasValidOxygenSaturation,
                        _pulseOximeter.OxygenSaturation,
                        _pulseOximeter.UsedWeakSignalOxygenSaturation,
                        _pulseOximeter.SignalQuality);
                }

                Thread.Sleep(DisplayRefreshMilliseconds);
            }
        }

        private static string GetAcquisitionStatus(Max30100SignalDiagnostic diagnostic)
        {
            switch (diagnostic)
            {
                case Max30100SignalDiagnostic.FillingAnalysisWindow:
                    return "Building the analysis window";
                case Max30100SignalDiagnostic.InsufficientAcAmplitude:
                case Max30100SignalDiagnostic.InsufficientPerfusion:
                    return "Adjust finger position";
                case Max30100SignalDiagnostic.TooFewBeatPeaks:
                case Max30100SignalDiagnostic.TooFewBeatIntervals:
                    return "Finding a steady pulse";
                case Max30100SignalDiagnostic.InconsistentBeatIntervals:
                case Max30100SignalDiagnostic.LowSignalQuality:
                case Max30100SignalDiagnostic.EstimatorsDisagree:
                    return "Hold still for a cleaner signal";
                case Max30100SignalDiagnostic.AutocorrelationFallbackPending:
                case Max30100SignalDiagnostic.CandidateConfirmationPending:
                    return "Confirming the measurement";
                default:
                    return "Collecting optical samples";
            }
        }
    }
}
