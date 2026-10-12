// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Max30100;
using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

const int DiagnosticIntervalMilliseconds = 4000;

Configuration.SetPinFunction(40, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(41, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new I2cConnectionSettings(1, Max30100.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(settings);
Max30100 sensor = new Max30100(i2cDevice);
Max30100PulseOximeterOptions pulseOximeterOptions = new Max30100PulseOximeterOptions
{
    SampleRate = 50,
    WindowSeconds = 8,
};
Max30100PulseOximeterWorker pulseOximeter = new Max30100PulseOximeterWorker(
    pulseOximeterOptions,
    queueSeconds: 2);

Debug.WriteLine($"MAX30100 part ID: 0x{sensor.PartId:X2}, revision: 0x{sensor.RevisionId:X2}");
if (sensor.PartId != Max30100.ExpectedPartId)
{
    throw new System.InvalidOperationException();
}

uint reportedDroppedSamples = 0;
uint reportedProcessingDroppedSamples = 0;
uint processedDroppedSamples = 0;
int latestInfrared = 0;
int latestRed = 0;

Thread diagnosticThread = new Thread(() =>
{
    while (true)
    {
        Thread.Sleep(DiagnosticIntervalMilliseconds);

        string overflow = string.Empty;
        if (sensor.DroppedSampleCount != reportedDroppedSamples)
        {
            overflow = $" Sensor FIFO dropped samples: {sensor.DroppedSampleCount}.";
        }

        if (pulseOximeter.DroppedSampleCount != reportedProcessingDroppedSamples)
        {
            overflow += $" Processing queue dropped samples: {pulseOximeter.DroppedSampleCount}.";
        }

        reportedDroppedSamples = sensor.DroppedSampleCount;
        reportedProcessingDroppedSamples = pulseOximeter.DroppedSampleCount;

        if (!pulseOximeter.FingerDetected)
        {
            Debug.WriteLine($"Place a finger on the sensor.{overflow}");
        }
        else if (pulseOximeter.IsSettling)
        {
            Debug.WriteLine($"Stabilizing finger contact. IR: {latestInfrared}, red: {latestRed}.{overflow}");
        }
        else if (!pulseOximeter.HasValidHeartRate)
        {
            string diagnostic;
            switch (pulseOximeter.SignalDiagnostic)
            {
                case Max30100SignalDiagnostic.FillingAnalysisWindow:
                    diagnostic = $"filling window ({pulseOximeter.AnalysisWindowProgress:F0}%)";
                    break;
                case Max30100SignalDiagnostic.InsufficientAcAmplitude:
                    diagnostic = "infrared AC amplitude is too low";
                    break;
                case Max30100SignalDiagnostic.InsufficientPerfusion:
                    diagnostic = "infrared perfusion is too low";
                    break;
                case Max30100SignalDiagnostic.TooFewBeatPeaks:
                    diagnostic = "too few prominence-qualified pulse peaks";
                    break;
                case Max30100SignalDiagnostic.TooFewBeatIntervals:
                    diagnostic = "too few intervals in the configured BPM range";
                    break;
                case Max30100SignalDiagnostic.InconsistentBeatIntervals:
                    diagnostic = "beat intervals are inconsistent";
                    break;
                case Max30100SignalDiagnostic.LowSignalQuality:
                    diagnostic = "periodic confidence is too low";
                    break;
                case Max30100SignalDiagnostic.EstimatorsDisagree:
                    diagnostic = "beat intervals and periodicity disagree";
                    break;
                case Max30100SignalDiagnostic.AutocorrelationFallbackPending:
                    diagnostic = "using periodic fallback; waiting for confirmation";
                    break;
                case Max30100SignalDiagnostic.NoHeartRateEstimate:
                    diagnostic = "no heart-rate estimate is in the configured range";
                    break;
                default:
                    diagnostic = "waiting for candidate confirmation";
                    break;
            }

            Debug.WriteLine(
                $"Collecting a stable signal. IR: {latestInfrared}, red: {latestRed}, "
                + $"perfusion: {pulseOximeter.PerfusionIndex:F2}%, "
                + $"red perfusion: {pulseOximeter.RedPerfusionIndex:F2}%, "
                + $"correlation: {pulseOximeter.ChannelCorrelation:F2}, "
                + $"ratio: {pulseOximeter.RatioOfRatios:F2}, "
                + $"quality: {pulseOximeter.SignalQuality:F0}%, "
                + $"candidate: {pulseOximeter.CandidateHeartRate:F1} BPM, "
                + $"periodic: {pulseOximeter.AutocorrelationHeartRate:F1} BPM/"
                + $"{pulseOximeter.AutocorrelationQuality:F0}%, "
                + $"AC RMS: {pulseOximeter.InfraredAcRms:F1}, "
                + $"threshold: {pulseOximeter.BeatDetectionThreshold:F1}, "
                + $"prominence: {pulseOximeter.BeatDetectionProminenceThreshold:F1}, "
                + $"peaks: {pulseOximeter.DetectedBeatPeaks}, "
                + $"intervals: {pulseOximeter.RawBeatIntervals}/{pulseOximeter.DetectedBeatIntervals}, "
                + $"diagnostic: {diagnostic}.{overflow}");
        }
        else
        {
            string heartRateStatus = pulseOximeter.IsHeartRateHeld ? " (held)" : string.Empty;
            string oxygenSaturation = pulseOximeter.HasValidOxygenSaturation
                ? $"{pulseOximeter.OxygenSaturation:F1}%"
                : "unavailable";
            string oxygenSaturationStatus = pulseOximeter.UsedWeakSignalOxygenSaturation
                ? " (weak signal)"
                : string.Empty;
            Debug.WriteLine(
                $"Pulse: {pulseOximeter.HeartRate:F1} BPM{heartRateStatus}, "
                + $"SpO2: {oxygenSaturation}{oxygenSaturationStatus}, "
                + $"perfusion: {pulseOximeter.PerfusionIndex:F2}%, "
                + $"red perfusion: {pulseOximeter.RedPerfusionIndex:F2}%, "
                + $"correlation: {pulseOximeter.ChannelCorrelation:F2}, "
                + $"ratio: {pulseOximeter.RatioOfRatios:F2}, "
                + $"periodic: {pulseOximeter.AutocorrelationHeartRate:F1} BPM/"
                + $"{pulseOximeter.AutocorrelationQuality:F0}%, "
                + $"quality: {pulseOximeter.SignalQuality:F0}%.{overflow}");
        }
    }
});
Thread.CurrentThread.Priority = ThreadPriority.Highest;
diagnosticThread.Priority = ThreadPriority.Lowest;
diagnosticThread.Start();

sensor.Reset();
sensor.Configure(
    Max30100Mode.SpO2,
    Max30100SampleRate.SamplesPerSecond50,
    Max30100PulseWidth.Microseconds1600,
    Max30100LedCurrent.Milliamps27Point1,
    Max30100LedCurrent.Milliamps40Point2);

while (true)
{
    while (sensor.TryReadSample(out Max30100Reading reading))
    {
        if (sensor.DroppedSampleCount != processedDroppedSamples)
        {
            processedDroppedSamples = sensor.DroppedSampleCount;
            pulseOximeter.Reset();
        }

        pulseOximeter.AddSample(reading);
        latestInfrared = reading.Infrared;
        latestRed = reading.Red;
    }

    Thread.Sleep(2);
}
