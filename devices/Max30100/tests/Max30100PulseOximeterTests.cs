// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.TestFramework;
using System.Threading;

namespace Iot.Device.Max30100.Tests
{
    [TestClass]
    public class Max30100PulseOximeterTests
    {
        [TestMethod]
        public void AddSampleCalculatesPulseAndOxygenSaturation()
        {
            Max30100PulseOximeter pulseOximeter = new Max30100PulseOximeter();

            for (int index = 0; index < 850; index++)
            {
                int phase = index % 100;
                int triangle = phase <= 50 ? phase : 100 - phase;
                ushort infrared = (ushort)(20000 + (triangle * 20));
                ushort red = (ushort)(25000 + (triangle * 12));
                pulseOximeter.AddSample(new Max30100Reading(infrared, red));
            }

            Assert.IsTrue(pulseOximeter.FingerDetected);
            Assert.IsTrue(pulseOximeter.HasValidHeartRate);
            Assert.IsTrue(pulseOximeter.HasValidOxygenSaturation);
            Assert.IsTrue(pulseOximeter.HasValidReading);
            Assert.IsTrue(pulseOximeter.HeartRate > 59 && pulseOximeter.HeartRate < 61);
            Assert.IsTrue(pulseOximeter.AutocorrelationHeartRate > 59 && pulseOximeter.AutocorrelationHeartRate < 61);
            Assert.IsTrue(pulseOximeter.AutocorrelationQuality > 90);
            Assert.IsTrue(pulseOximeter.OxygenSaturation > 90 && pulseOximeter.OxygenSaturation <= 100);
        }

        [TestMethod]
        public void ConstructorConfiguresProcessingHeuristics()
        {
            Max30100PulseOximeterOptions options = new Max30100PulseOximeterOptions
            {
                BaselineTrackingRate = 3,
                SignalSmoothingRate = 12,
                FilterSettlingSeconds = 0.75,
                BeatDetectionThresholdFraction = 0.15,
                MinimumBeatDetectionThreshold = 2,
                MinimumAcRms = 8,
                MissedBeatIntervalFactor = 1.8,
                BeatIntervalToleranceFraction = 0.20,
                FingerLossSignalResetSamples = 3,
                FingerRemovalDebounceSamples = 6,
                InitialFingerSettlingSeconds = 1.5,
                MotionRecoverySettlingSeconds = 0.5,
                AnalysisIntervalSeconds = 1,
                HeartRateConfirmationCount = 3,
                AlternateHeartRateConfirmationCount = 4,
                HeartRateCandidateSmoothingFactor = 0.4,
                HeartRateSmoothingFactor = 0.25,
                OxygenSaturationSmoothingFactor = 0.15,
                MinimumAutocorrelationChannelCorrelation = 0.8,
                EstimatorDisagreementQualityFactor = 0.4,
                AutocorrelationFallbackQualityFactor = 0.7,
                IntervalDeviationQualityPenalty = 250,
                AutocorrelationTargetSampleRate = 20,
                AutocorrelationHarmonicThreshold = 0.9,
                AutocorrelationShoulderFraction = 0.3,
                AutocorrelationProminenceForFullQuality = 0.25,
                AutocorrelationCyclesForFullQuality = 4,
                MinimumWeakSignalOxygenSaturationPerfusionIndex = 0.03,
                MinimumWeakSignalOxygenSaturationCorrelation = 0.85,
                WeakSignalOxygenSaturationRatioTolerance = 0.08,
                WeakSignalOxygenSaturationConfirmationCount = 4,
                StableOpticalSignalHeartRateMissLimit = 7,
            };
            Max30100PulseOximeter pulseOximeter = new Max30100PulseOximeter(options);

            Assert.AreEqual(3.0, pulseOximeter.BaselineTrackingRate);
            Assert.AreEqual(12.0, pulseOximeter.SignalSmoothingRate);
            Assert.AreEqual(0.75, pulseOximeter.FilterSettlingSeconds);
            Assert.AreEqual(0.15, pulseOximeter.BeatDetectionThresholdFraction);
            Assert.AreEqual(2.0, pulseOximeter.MinimumBeatDetectionThreshold);
            Assert.AreEqual(8.0, pulseOximeter.MinimumAcRms);
            Assert.AreEqual(1.8, pulseOximeter.MissedBeatIntervalFactor);
            Assert.AreEqual(0.20, pulseOximeter.BeatIntervalToleranceFraction);
            Assert.AreEqual(3, pulseOximeter.FingerLossSignalResetSamples);
            Assert.AreEqual(6, pulseOximeter.FingerRemovalDebounceSamples);
            Assert.AreEqual(1.5, pulseOximeter.InitialFingerSettlingSeconds);
            Assert.AreEqual(0.5, pulseOximeter.MotionRecoverySettlingSeconds);
            Assert.AreEqual(1.0, pulseOximeter.AnalysisIntervalSeconds);
            Assert.AreEqual(3, pulseOximeter.HeartRateConfirmationCount);
            Assert.AreEqual(4, pulseOximeter.AlternateHeartRateConfirmationCount);
            Assert.AreEqual(0.4, pulseOximeter.HeartRateCandidateSmoothingFactor);
            Assert.AreEqual(0.25, pulseOximeter.HeartRateSmoothingFactor);
            Assert.AreEqual(0.15, pulseOximeter.OxygenSaturationSmoothingFactor);
            Assert.AreEqual(0.8, pulseOximeter.MinimumAutocorrelationChannelCorrelation);
            Assert.AreEqual(0.4, pulseOximeter.EstimatorDisagreementQualityFactor);
            Assert.AreEqual(0.7, pulseOximeter.AutocorrelationFallbackQualityFactor);
            Assert.AreEqual(250.0, pulseOximeter.IntervalDeviationQualityPenalty);
            Assert.AreEqual(20, pulseOximeter.AutocorrelationTargetSampleRate);
            Assert.AreEqual(0.9, pulseOximeter.AutocorrelationHarmonicThreshold);
            Assert.AreEqual(0.3, pulseOximeter.AutocorrelationShoulderFraction);
            Assert.AreEqual(0.25, pulseOximeter.AutocorrelationProminenceForFullQuality);
            Assert.AreEqual(4.0, pulseOximeter.AutocorrelationCyclesForFullQuality);
            Assert.AreEqual(0.03, pulseOximeter.MinimumWeakSignalOxygenSaturationPerfusionIndex);
            Assert.AreEqual(0.85, pulseOximeter.MinimumWeakSignalOxygenSaturationCorrelation);
            Assert.AreEqual(0.08, pulseOximeter.WeakSignalOxygenSaturationRatioTolerance);
            Assert.AreEqual(4, pulseOximeter.WeakSignalOxygenSaturationConfirmationCount);
            Assert.AreEqual(7, pulseOximeter.StableOpticalSignalHeartRateMissLimit);
        }

        [TestMethod]
        public void AddSampleClearsReadingWhenFingerIsRemoved()
        {
            Max30100PulseOximeter pulseOximeter = new Max30100PulseOximeter();
            pulseOximeter.AddSample(new Max30100Reading(20000, 25000));
            for (int index = 0; index < 5; index++)
            {
                pulseOximeter.AddSample(new Max30100Reading(10, 10));
            }

            Assert.IsFalse(pulseOximeter.FingerDetected);
            Assert.IsFalse(pulseOximeter.HasValidHeartRate);
            Assert.IsFalse(pulseOximeter.HasValidOxygenSaturation);
            Assert.IsFalse(pulseOximeter.HasValidReading);
            Assert.AreEqual(0.0, pulseOximeter.HeartRate);
            Assert.AreEqual(0.0, pulseOximeter.OxygenSaturation);
        }

        [TestMethod]
        public void AddSampleRejectsLinearDriftWithoutPulse()
        {
            Max30100PulseOximeter pulseOximeter = new Max30100PulseOximeter(
                new Max30100PulseOximeterOptions
                {
                    SampleRate = 100,
                    WindowSeconds = 4,
                });

            for (int index = 0; index < 850; index++)
            {
                ushort infrared = (ushort)(34000 - (index * 4));
                ushort red = (ushort)(31000 - (index * 3));
                pulseOximeter.AddSample(new Max30100Reading(infrared, red));
            }

            Assert.IsTrue(pulseOximeter.FingerDetected);
            Assert.IsFalse(pulseOximeter.HasValidHeartRate);
            Assert.AreEqual(0.0, pulseOximeter.CandidateHeartRate);
            Assert.IsTrue(pulseOximeter.DetectedBeatIntervals < 3);
        }

        [TestMethod]
        public void CalibrateOxygenSaturationFitsLinearCoefficients()
        {
            Max30100PulseOximeter pulseOximeter = new Max30100PulseOximeter();
            double[] ratios = new double[] { 0.5, 1.0, 1.5 };
            double[] referenceOxygenSaturations = new double[] { 100, 90, 80 };

            pulseOximeter.CalibrateOxygenSaturation(ratios, referenceOxygenSaturations);

            Assert.AreEqual(110.0, pulseOximeter.OxygenSaturationCalibrationIntercept);
            Assert.AreEqual(-20.0, pulseOximeter.OxygenSaturationCalibrationSlope);
        }

        [TestMethod]
        public void AddSampleRestartsSettlingAfterAbruptMovement()
        {
            Max30100PulseOximeter pulseOximeter = new Max30100PulseOximeter(
                new Max30100PulseOximeterOptions
                {
                    SampleRate = 50,
                });
            for (int index = 0; index < 110; index++)
            {
                pulseOximeter.AddSample(new Max30100Reading(20000, 25000));
            }

            pulseOximeter.AddSample(new Max30100Reading(23000, 28000));

            Assert.IsTrue(pulseOximeter.FingerDetected);
            Assert.IsTrue(pulseOximeter.IsSettling);
            Assert.IsFalse(pulseOximeter.HasValidHeartRate);
        }

        [TestMethod]
        public void WorkerProcessesSamplesAndResetsAsynchronously()
        {
            Max30100PulseOximeterWorker worker = new Max30100PulseOximeterWorker(
                new Max30100PulseOximeterOptions
                {
                    SampleRate = 50,
                    WindowSeconds = 3,
                },
                publishRate: 5);
            try
            {
                Assert.AreEqual(5, worker.PublishRate);

                for (int index = 0; index < 10; index++)
                {
                    worker.AddSample(new Max30100Reading(20000, 25000));
                }

                Thread.Sleep(100);
                Assert.IsTrue(worker.FingerDetected);

                worker.Reset();
                Thread.Sleep(100);
                Assert.IsFalse(worker.FingerDetected);
                Assert.AreEqual((uint)0, worker.DroppedSampleCount);

                worker.CalibrateOxygenSaturation(
                    new double[] { 0.5, 1.5 },
                    new double[] { 100, 80 });
                Thread.Sleep(100);
                Assert.AreEqual(110.0, worker.OxygenSaturationCalibrationIntercept);
                Assert.AreEqual(-20.0, worker.OxygenSaturationCalibrationSlope);
            }
            finally
            {
                worker.Dispose();
            }
        }
    }
}
