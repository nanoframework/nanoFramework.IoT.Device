// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Max30100
{
    /// <summary>Provides configuration for MAX30100 pulse and oxygen-saturation processing.</summary>
    public sealed class Max30100PulseOximeterOptions
    {
        /// <summary>Gets or sets the sensor sample rate.</summary>
        /// <value>The default is 100 samples per second.</value>
        public int SampleRate { get; set; } = Max30100PulseOximeter.DefaultSampleRate;

        /// <summary>Gets or sets the analysis-window duration in seconds.</summary>
        /// <value>The default is 6 seconds.</value>
        public int WindowSeconds { get; set; } = Max30100PulseOximeter.DefaultWindowSeconds;

        /// <summary>Gets or sets the one-sample relative change treated as abrupt movement.</summary>
        /// <value>The default is 4 percent.</value>
        public int AbruptChangeRelativePercent { get; set; } =
            Max30100PulseOximeter.DefaultAbruptChangeRelativePercent;

        /// <summary>Gets or sets the minimum one-sample change treated as abrupt movement.</summary>
        /// <value>The default is 750 ADC counts.</value>
        public int MinimumAbruptChangeCounts { get; set; } = Max30100PulseOximeter.DefaultMinimumAbruptChangeCounts;

        /// <summary>Gets or sets the maximum difference between consecutive heart-rate candidates.</summary>
        /// <value>The default is 12 BPM.</value>
        public double HeartRateCandidateTolerance { get; set; } =
            Max30100PulseOximeter.DefaultHeartRateCandidateTolerance;

        /// <summary>Gets or sets the invalid-analysis limit before clearing a heart-rate candidate.</summary>
        /// <value>The default is 3 consecutive analyses.</value>
        public int HeartRateCandidateMissLimit { get; set; } = Max30100PulseOximeter.DefaultHeartRateCandidateMissLimit;

        /// <summary>Gets or sets the minimum red and infrared level used to detect finger contact.</summary>
        /// <value>The default is 1000 ADC counts.</value>
        public int FingerDetectionThreshold { get; set; } = Max30100PulseOximeter.DefaultFingerDetectionThreshold;

        /// <summary>Gets or sets the minimum supported heart rate.</summary>
        /// <value>The default is 40 BPM.</value>
        public double MinimumHeartRate { get; set; } = Max30100PulseOximeter.DefaultMinimumHeartRate;

        /// <summary>Gets or sets the maximum supported heart rate.</summary>
        /// <value>The default is 180 BPM.</value>
        public double MaximumHeartRate { get; set; } = Max30100PulseOximeter.DefaultMaximumHeartRate;

        /// <summary>Gets or sets the maximum difference between interval and periodic estimates.</summary>
        /// <value>The default is 12 BPM.</value>
        public double HeartRateEstimatorAgreementTolerance { get; set; } =
            Max30100PulseOximeter.DefaultHeartRateEstimatorAgreementTolerance;

        /// <summary>Gets or sets the minimum accepted heart-rate quality.</summary>
        /// <value>The default is 45 percent.</value>
        public double MinimumHeartRateQuality { get; set; } = Max30100PulseOximeter.DefaultMinimumHeartRateQuality;

        /// <summary>Gets or sets the minimum accepted autocorrelation quality.</summary>
        /// <value>The default is 55 percent.</value>
        public double MinimumAutocorrelationQuality { get; set; } =
            Max30100PulseOximeter.DefaultMinimumAutocorrelationQuality;

        /// <summary>Gets or sets the minimum number of consistent beat intervals.</summary>
        /// <value>The default is 4 intervals.</value>
        public int MinimumBeatIntervalCount { get; set; } = Max30100PulseOximeter.DefaultMinimumBeatIntervalCount;

        /// <summary>Gets or sets the quality required for immediate candidate acceptance.</summary>
        /// <value>The default is 80 percent.</value>
        public double StrongCandidateQuality { get; set; } = Max30100PulseOximeter.DefaultStrongCandidateQuality;

        /// <summary>Gets or sets the minimum infrared perfusion index for heart-rate estimation.</summary>
        /// <value>The default is 0.04 percent.</value>
        public double MinimumHeartRatePerfusionIndex { get; set; } =
            Max30100PulseOximeter.DefaultMinimumHeartRatePerfusionIndex;

        /// <summary>Gets or sets the minimum channel perfusion index for oxygen-saturation estimation.</summary>
        /// <value>The default is 0.10 percent.</value>
        public double MinimumOxygenSaturationPerfusionIndex { get; set; } =
            Max30100PulseOximeter.DefaultMinimumOxygenSaturationPerfusionIndex;

        /// <summary>Gets or sets the minimum channel correlation for oxygen-saturation estimation.</summary>
        /// <value>The default is 0.80.</value>
        public double MinimumOxygenSaturationCorrelation { get; set; } =
            Max30100PulseOximeter.DefaultMinimumOxygenSaturationCorrelation;

        /// <summary>Gets or sets the minimum ratio accepted for oxygen-saturation estimation.</summary>
        /// <value>The default is 0.3.</value>
        public double MinimumOxygenSaturationRatio { get; set; } =
            Max30100PulseOximeter.DefaultMinimumOxygenSaturationRatio;

        /// <summary>Gets or sets the maximum ratio accepted for oxygen-saturation estimation.</summary>
        /// <value>The default is 1.7.</value>
        public double MaximumOxygenSaturationRatio { get; set; } =
            Max30100PulseOximeter.DefaultMaximumOxygenSaturationRatio;

        /// <summary>Gets or sets the baseline-tracking rate.</summary>
        /// <value>The default is 2.0 per second.</value>
        public double BaselineTrackingRate { get; set; } = Max30100PulseOximeter.DefaultBaselineTrackingRate;

        /// <summary>Gets or sets the signal-smoothing rate.</summary>
        /// <value>The default is 15.0 per second.</value>
        public double SignalSmoothingRate { get; set; } = Max30100PulseOximeter.DefaultSignalSmoothingRate;

        /// <summary>Gets or sets the filter-settling duration in seconds.</summary>
        /// <value>The default is 0.5 seconds.</value>
        public double FilterSettlingSeconds { get; set; } = Max30100PulseOximeter.DefaultFilterSettlingSeconds;

        /// <summary>Gets or sets the AC RMS fraction used for beat-detection hysteresis.</summary>
        /// <value>The default is 0.10.</value>
        public double BeatDetectionThresholdFraction { get; set; } =
            Max30100PulseOximeter.DefaultBeatDetectionThresholdFraction;

        /// <summary>Gets or sets the minimum beat-detection hysteresis.</summary>
        /// <value>The default is 1.0 ADC count.</value>
        public double MinimumBeatDetectionThreshold { get; set; } =
            Max30100PulseOximeter.DefaultMinimumBeatDetectionThreshold;

        /// <summary>Gets or sets the minimum AC RMS amplitude required for measurements.</summary>
        /// <value>The default is 5.0 ADC counts.</value>
        public double MinimumAcRms { get; set; } = Max30100PulseOximeter.DefaultMinimumAcRms;

        /// <summary>Gets or sets the interval factor used to infer one missed beat.</summary>
        /// <value>The default is 1.6.</value>
        public double MissedBeatIntervalFactor { get; set; } = Max30100PulseOximeter.DefaultMissedBeatIntervalFactor;

        /// <summary>Gets or sets the accepted deviation from the median beat interval.</summary>
        /// <value>The default is 0.25.</value>
        public double BeatIntervalToleranceFraction { get; set; } =
            Max30100PulseOximeter.DefaultBeatIntervalToleranceFraction;

        /// <summary>Gets or sets the no-finger sample count that clears signal analysis.</summary>
        /// <value>The default is 2 samples.</value>
        public int FingerLossSignalResetSamples { get; set; } =
            Max30100PulseOximeter.DefaultFingerLossSignalResetSamples;

        /// <summary>Gets or sets the no-finger sample count that confirms finger removal.</summary>
        /// <value>The default is 5 samples.</value>
        public int FingerRemovalDebounceSamples { get; set; } =
            Max30100PulseOximeter.DefaultFingerRemovalDebounceSamples;

        /// <summary>Gets or sets the settling duration after initial finger contact.</summary>
        /// <value>The default is 2.0 seconds.</value>
        public double InitialFingerSettlingSeconds { get; set; } =
            Max30100PulseOximeter.DefaultInitialFingerSettlingSeconds;

        /// <summary>Gets or sets the settling duration after motion or temporary contact loss.</summary>
        /// <value>The default is 1.0 second.</value>
        public double MotionRecoverySettlingSeconds { get; set; } =
            Max30100PulseOximeter.DefaultMotionRecoverySettlingSeconds;

        /// <summary>Gets or sets the interval between complete analyses.</summary>
        /// <value>The default is 2.0 seconds.</value>
        public double AnalysisIntervalSeconds { get; set; } = Max30100PulseOximeter.DefaultAnalysisIntervalSeconds;

        /// <summary>Gets or sets the matching-candidate count required to confirm a heart rate.</summary>
        /// <value>The default is 2 candidates.</value>
        public int HeartRateConfirmationCount { get; set; } = Max30100PulseOximeter.DefaultHeartRateConfirmationCount;

        /// <summary>Gets or sets the alternate-candidate count required to replace a heart rate.</summary>
        /// <value>The default is 2 candidates.</value>
        public int AlternateHeartRateConfirmationCount { get; set; } =
            Max30100PulseOximeter.DefaultAlternateHeartRateConfirmationCount;

        /// <summary>Gets or sets the weight of a new heart-rate candidate.</summary>
        /// <value>The default is 0.5.</value>
        public double HeartRateCandidateSmoothingFactor { get; set; } =
            Max30100PulseOximeter.DefaultHeartRateCandidateSmoothingFactor;

        /// <summary>Gets or sets the weight of a new confirmed heart rate.</summary>
        /// <value>The default is 0.3.</value>
        public double HeartRateSmoothingFactor { get; set; } =
            Max30100PulseOximeter.DefaultHeartRateSmoothingFactor;

        /// <summary>Gets or sets the weight of a new oxygen-saturation result.</summary>
        /// <value>The default is 0.2.</value>
        public double OxygenSaturationSmoothingFactor { get; set; } =
            Max30100PulseOximeter.DefaultOxygenSaturationSmoothingFactor;

        /// <summary>Gets or sets the minimum channel correlation for autocorrelation fallback.</summary>
        /// <value>The default is 0.75.</value>
        public double MinimumAutocorrelationChannelCorrelation { get; set; } =
            Max30100PulseOximeter.DefaultMinimumAutocorrelationChannelCorrelation;

        /// <summary>Gets or sets the quality multiplier used when estimators disagree.</summary>
        /// <value>The default is 0.5.</value>
        public double EstimatorDisagreementQualityFactor { get; set; } =
            Max30100PulseOximeter.DefaultEstimatorDisagreementQualityFactor;

        /// <summary>Gets or sets the quality multiplier used for autocorrelation fallback.</summary>
        /// <value>The default is 0.75.</value>
        public double AutocorrelationFallbackQualityFactor { get; set; } =
            Max30100PulseOximeter.DefaultAutocorrelationFallbackQualityFactor;

        /// <summary>Gets or sets the relative interval-deviation quality penalty.</summary>
        /// <value>The default is 200.0.</value>
        public double IntervalDeviationQualityPenalty { get; set; } =
            Max30100PulseOximeter.DefaultIntervalDeviationQualityPenalty;

        /// <summary>Gets or sets the target sample rate used for autocorrelation.</summary>
        /// <value>The default is 25 samples per second.</value>
        public int AutocorrelationTargetSampleRate { get; set; } =
            Max30100PulseOximeter.DefaultAutocorrelationTargetSampleRate;

        /// <summary>Gets or sets the relative strength needed to select a half-lag harmonic.</summary>
        /// <value>The default is 0.85.</value>
        public double AutocorrelationHarmonicThreshold { get; set; } =
            Max30100PulseOximeter.DefaultAutocorrelationHarmonicThreshold;

        /// <summary>Gets or sets the lag fraction used to evaluate autocorrelation peak shoulders.</summary>
        /// <value>The default is 0.25.</value>
        public double AutocorrelationShoulderFraction { get; set; } =
            Max30100PulseOximeter.DefaultAutocorrelationShoulderFraction;

        /// <summary>Gets or sets the peak prominence that produces full prominence evidence.</summary>
        /// <value>The default is 0.20.</value>
        public double AutocorrelationProminenceForFullQuality { get; set; } =
            Max30100PulseOximeter.DefaultAutocorrelationProminenceForFullQuality;

        /// <summary>Gets or sets the waveform-cycle count that produces full cycle evidence.</summary>
        /// <value>The default is 3.0 cycles.</value>
        public double AutocorrelationCyclesForFullQuality { get; set; } =
            Max30100PulseOximeter.DefaultAutocorrelationCyclesForFullQuality;

        /// <summary>Gets or sets the minimum channel perfusion index for adaptive weak-signal SpO₂.</summary>
        /// <value>The default is 0.04 percent.</value>
        public double MinimumWeakSignalOxygenSaturationPerfusionIndex { get; set; } =
            Max30100PulseOximeter.DefaultMinimumWeakSignalOxygenSaturationPerfusionIndex;

        /// <summary>Gets or sets the minimum channel correlation for adaptive weak-signal SpO₂.</summary>
        /// <value>The default is 0.90.</value>
        public double MinimumWeakSignalOxygenSaturationCorrelation { get; set; } =
            Max30100PulseOximeter.DefaultMinimumWeakSignalOxygenSaturationCorrelation;

        /// <summary>Gets or sets the maximum ratio change accepted between weak-signal analysis windows.</summary>
        /// <value>The default is 0.10.</value>
        public double WeakSignalOxygenSaturationRatioTolerance { get; set; } =
            Max30100PulseOximeter.DefaultWeakSignalOxygenSaturationRatioTolerance;

        /// <summary>Gets or sets the stable-window count required before publishing weak-signal SpO₂.</summary>
        /// <value>The default is 3 consecutive analysis windows.</value>
        public int WeakSignalOxygenSaturationConfirmationCount { get; set; } =
            Max30100PulseOximeter.DefaultWeakSignalOxygenSaturationConfirmationCount;

        /// <summary>Gets or sets the heart-rate miss limit used while the optical signal remains coherent.</summary>
        /// <value>The default is 6 consecutive analysis windows.</value>
        public int StableOpticalSignalHeartRateMissLimit { get; set; } =
            Max30100PulseOximeter.DefaultStableOpticalSignalHeartRateMissLimit;
    }
}
