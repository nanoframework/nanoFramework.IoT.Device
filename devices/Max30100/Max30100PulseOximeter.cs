// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Max30100
{
    /// <summary>
    /// Processes MAX30100 samples to estimate pulse rate and oxygen saturation.
    /// </summary>
    /// <remarks>
    /// The default oxygen-saturation calculation uses an uncalibrated ratio-of-ratios approximation.
    /// Results are for experimentation only and must not be used for medical purposes.
    /// </remarks>
    public sealed class Max30100PulseOximeter
    {
        /// <summary>The default sensor sample rate expected by the processor.</summary>
        public const int DefaultSampleRate = 100;

        /// <summary>The default analysis-window duration.</summary>
        public const int DefaultWindowSeconds = 6;

        /// <summary>The default one-sample relative change treated as abrupt movement.</summary>
        public const int DefaultAbruptChangeRelativePercent = 4;

        /// <summary>The default minimum one-sample change treated as abrupt movement, in ADC counts.</summary>
        public const int DefaultMinimumAbruptChangeCounts = 750;

        /// <summary>The default oxygen-saturation calibration intercept.</summary>
        public const double DefaultOxygenSaturationCalibrationIntercept = 110.0;

        /// <summary>The default oxygen-saturation calibration slope.</summary>
        public const double DefaultOxygenSaturationCalibrationSlope = -25.0;

        /// <summary>The default maximum difference between consecutive heart-rate candidates.</summary>
        public const double DefaultHeartRateCandidateTolerance = 12.0;

        /// <summary>The default consecutive invalid-analysis limit before clearing a heart-rate candidate.</summary>
        public const int DefaultHeartRateCandidateMissLimit = 3;

        /// <summary>The default minimum red and infrared level used to detect finger contact.</summary>
        public const int DefaultFingerDetectionThreshold = 1000;

        /// <summary>The default minimum supported heart rate.</summary>
        public const double DefaultMinimumHeartRate = 40.0;

        /// <summary>The default maximum supported heart rate.</summary>
        public const double DefaultMaximumHeartRate = 180.0;

        /// <summary>The default maximum difference between interval and periodic heart-rate estimates.</summary>
        public const double DefaultHeartRateEstimatorAgreementTolerance = 12.0;

        /// <summary>The default minimum accepted heart-rate quality.</summary>
        public const double DefaultMinimumHeartRateQuality = 45.0;

        /// <summary>The default minimum accepted autocorrelation quality.</summary>
        public const double DefaultMinimumAutocorrelationQuality = 55.0;

        /// <summary>The default minimum number of consistent beat intervals.</summary>
        public const int DefaultMinimumBeatIntervalCount = 4;

        /// <summary>The default quality required for immediate strong-candidate acceptance.</summary>
        public const double DefaultStrongCandidateQuality = 80.0;

        /// <summary>The default minimum infrared perfusion index for heart-rate estimation.</summary>
        public const double DefaultMinimumHeartRatePerfusionIndex = 0.04;

        /// <summary>The default minimum red and infrared perfusion index for oxygen-saturation estimation.</summary>
        public const double DefaultMinimumOxygenSaturationPerfusionIndex = 0.10;

        /// <summary>The default minimum red and infrared waveform correlation for oxygen-saturation estimation.</summary>
        public const double DefaultMinimumOxygenSaturationCorrelation = 0.80;

        /// <summary>The default minimum ratio of ratios accepted for oxygen-saturation estimation.</summary>
        public const double DefaultMinimumOxygenSaturationRatio = 0.3;

        /// <summary>The default maximum ratio of ratios accepted for oxygen-saturation estimation.</summary>
        public const double DefaultMaximumOxygenSaturationRatio = 1.7;

        /// <summary>The default baseline-tracking rate used by the high-pass filter.</summary>
        public const double DefaultBaselineTrackingRate = 2.0;

        /// <summary>The default smoothing rate used by the low-pass filter.</summary>
        public const double DefaultSignalSmoothingRate = 15.0;

        /// <summary>The default duration discarded while the filters settle.</summary>
        public const double DefaultFilterSettlingSeconds = 0.5;

        /// <summary>The default RMS fraction used for beat-detection hysteresis.</summary>
        public const double DefaultBeatDetectionThresholdFraction = 0.10;

        /// <summary>The default minimum beat-detection hysteresis in ADC counts.</summary>
        public const double DefaultMinimumBeatDetectionThreshold = 1.0;

        /// <summary>The default minimum AC RMS amplitude required for pulse measurements.</summary>
        public const double DefaultMinimumAcRms = 5.0;

        /// <summary>The default interval factor above which a missed beat is inferred.</summary>
        public const double DefaultMissedBeatIntervalFactor = 1.6;

        /// <summary>The default accepted deviation from the median beat interval.</summary>
        public const double DefaultBeatIntervalToleranceFraction = 0.25;

        /// <summary>The default consecutive no-finger samples before clearing signal analysis.</summary>
        public const int DefaultFingerLossSignalResetSamples = 2;

        /// <summary>The default consecutive no-finger samples before confirming finger removal.</summary>
        public const int DefaultFingerRemovalDebounceSamples = 5;

        /// <summary>The default settling duration after initial finger contact.</summary>
        public const double DefaultInitialFingerSettlingSeconds = 2.0;

        /// <summary>The default settling duration after abrupt movement or temporary contact loss.</summary>
        public const double DefaultMotionRecoverySettlingSeconds = 1.0;

        /// <summary>The default interval between complete signal analyses.</summary>
        public const double DefaultAnalysisIntervalSeconds = 2.0;

        /// <summary>The default number of matching candidates required to confirm a heart rate.</summary>
        public const int DefaultHeartRateConfirmationCount = 2;

        /// <summary>The default number of matching alternate candidates required to replace a heart rate.</summary>
        public const int DefaultAlternateHeartRateConfirmationCount = 2;

        /// <summary>The default weight of a new heart-rate candidate during candidate smoothing.</summary>
        public const double DefaultHeartRateCandidateSmoothingFactor = 0.5;

        /// <summary>The default weight of a new confirmed heart rate during output smoothing.</summary>
        public const double DefaultHeartRateSmoothingFactor = 0.3;

        /// <summary>The default weight of a new oxygen-saturation result during output smoothing.</summary>
        public const double DefaultOxygenSaturationSmoothingFactor = 0.2;

        /// <summary>The default minimum red and infrared correlation for autocorrelation fallback.</summary>
        public const double DefaultMinimumAutocorrelationChannelCorrelation = 0.75;

        /// <summary>The default quality multiplier applied when heart-rate estimators disagree.</summary>
        public const double DefaultEstimatorDisagreementQualityFactor = 0.5;

        /// <summary>The default quality multiplier applied when autocorrelation supplies the only estimate.</summary>
        public const double DefaultAutocorrelationFallbackQualityFactor = 0.75;

        /// <summary>The default multiplier converting relative interval deviation into a quality penalty.</summary>
        public const double DefaultIntervalDeviationQualityPenalty = 200.0;

        /// <summary>The default target sample rate used for autocorrelation.</summary>
        public const int DefaultAutocorrelationTargetSampleRate = 25;

        /// <summary>The default relative strength needed to select a half-lag harmonic.</summary>
        public const double DefaultAutocorrelationHarmonicThreshold = 0.85;

        /// <summary>The default lag fraction used to evaluate autocorrelation peak shoulders.</summary>
        public const double DefaultAutocorrelationShoulderFraction = 0.25;

        /// <summary>The default peak prominence that produces full prominence evidence.</summary>
        public const double DefaultAutocorrelationProminenceForFullQuality = 0.20;

        /// <summary>The default number of waveform cycles that produces full cycle evidence.</summary>
        public const double DefaultAutocorrelationCyclesForFullQuality = 3.0;

        /// <summary>The default minimum channel perfusion index for adaptive weak-signal SpO₂.</summary>
        public const double DefaultMinimumWeakSignalOxygenSaturationPerfusionIndex = 0.04;

        /// <summary>The default minimum channel correlation for adaptive weak-signal SpO₂.</summary>
        public const double DefaultMinimumWeakSignalOxygenSaturationCorrelation = 0.90;

        /// <summary>The default maximum ratio change accepted by adaptive weak-signal SpO₂.</summary>
        public const double DefaultWeakSignalOxygenSaturationRatioTolerance = 0.10;

        /// <summary>The default stable-window count required by adaptive weak-signal SpO₂.</summary>
        public const int DefaultWeakSignalOxygenSaturationConfirmationCount = 3;

        /// <summary>The default heart-rate miss limit while the optical signal remains coherent.</summary>
        public const int DefaultStableOpticalSignalHeartRateMissLimit = 6;

        private const int MinimumSampleRate = 50;
        private const int MaximumSampleRate = 1000;
        private const int MinimumWindowSeconds = 3;
        private const int MaximumWindowSeconds = 8;
        private const int MinimumAutocorrelationTargetSampleRate = 10;
        private const int MaximumAnalysisWindowSamples = 1000;
        private const int MinimumBeatIntervalCountLimit = 2;
        private const int MaximumBeatIntervalCount = 20;
        private const int MinimumCandidateMissLimit = 1;
        private const int MaximumCandidateMissLimit = 10;
        private const int MinimumConfigurableHeartRate = 20;
        private const int MaximumConfigurableHeartRate = 300;
        private const int MinimumConfirmationCount = 1;
        private const int MaximumConfirmationCount = 10;
        private const int MinimumDebounceSamples = 1;
        private const int MaximumDebounceSamples = 1000;
        private const double MaximumCandidateTolerance = 180.0;
        private const double MaximumOxygenSaturationRatioLimit = 10.0;
        private const double MaximumMissedBeatIntervalFactor = 3.0;
        private const double MaximumIntervalDeviationQualityPenalty = 1000.0;
        private const double MaximumAutocorrelationShoulderFraction = 0.5;
        private const double MaximumNormalizedAutocorrelationProminence = 2.0;
        private const double MaximumAutocorrelationCyclesForFullQuality = 100.0;
        private const int MinimumCalibrationPointCount = 2;
        private const double MaximumCalibrationRatio = 10.0;
        private const double MinimumReferenceOxygenSaturation = 0.0;
        private const double MaximumReferenceOxygenSaturation = 100.0;
        private const double MinimumCalibrationIntercept = 0.0;
        private const double MaximumCalibrationIntercept = 200.0;
        private const double MinimumCalibrationSlope = -200.0;
        private const double MaximumCalibrationSlope = 0.0;
        private readonly double[] _autocorrelation;
        private readonly int[] _beatIntervals;
        private readonly double[] _filteredInfraredSamples;
        private readonly double[] _filteredRedSamples;
        private readonly ushort[] _infraredSamples;
        private readonly ushort[] _redSamples;
        private readonly int _autocorrelationDecimation;
        private readonly int _analysisIntervalSamples;
        private readonly int _initialFingerSettlingSamples;
        private readonly int _motionRecoverySettlingSamples;
        private readonly int _sampleRate;
        private int _alternateHeartRateCount;
        private double _alternateHeartRate;
        private double _candidateHeartRate;
        private int _candidateMissCount;
        private int _consistentHeartRateCount;
        private int _detectedBeatCrossingCount;
        private int _detectedIntervalCount;
        private int _rawBeatIntervalCount;
        private bool _hasPreviousSample;
        private int _noFingerSampleCount;
        private ushort _previousInfrared;
        private ushort _previousRed;
        private int _sampleCount;
        private int _settlingSamplesRemaining;
        private int _writeIndex;
        private int _samplesSinceCalculation;
        private double _weakSignalOxygenSaturationRatio;
        private int _weakSignalOxygenSaturationCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="Max30100PulseOximeter"/> class
        /// with the default signal-processing configuration.
        /// </summary>
        public Max30100PulseOximeter()
            : this(new Max30100PulseOximeterOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Max30100PulseOximeter"/> class.
        /// </summary>
        /// <param name="options">The signal-processing configuration.</param>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A processing option is outside its supported range.</exception>
        /// <exception cref="ArgumentException">Two or more processing options are incompatible.</exception>
        public Max30100PulseOximeter(Max30100PulseOximeterOptions options)
            : this(
                GetOptions(options).SampleRate,
                options.WindowSeconds,
                options.AbruptChangeRelativePercent,
                options.MinimumAbruptChangeCounts,
                options.HeartRateCandidateTolerance,
                options.HeartRateCandidateMissLimit,
                options.FingerDetectionThreshold,
                options.MinimumHeartRate,
                options.MaximumHeartRate,
                options.HeartRateEstimatorAgreementTolerance,
                options.MinimumHeartRateQuality,
                options.MinimumAutocorrelationQuality,
                options.MinimumBeatIntervalCount,
                options.StrongCandidateQuality,
                options.MinimumHeartRatePerfusionIndex,
                options.MinimumOxygenSaturationPerfusionIndex,
                options.MinimumOxygenSaturationCorrelation,
                options.MinimumOxygenSaturationRatio,
                options.MaximumOxygenSaturationRatio,
                options.BaselineTrackingRate,
                options.SignalSmoothingRate,
                options.FilterSettlingSeconds,
                options.BeatDetectionThresholdFraction,
                options.MinimumBeatDetectionThreshold,
                options.MinimumAcRms,
                options.MissedBeatIntervalFactor,
                options.BeatIntervalToleranceFraction,
                options.FingerLossSignalResetSamples,
                options.FingerRemovalDebounceSamples,
                options.InitialFingerSettlingSeconds,
                options.MotionRecoverySettlingSeconds,
                options.AnalysisIntervalSeconds,
                options.HeartRateConfirmationCount,
                options.AlternateHeartRateConfirmationCount,
                options.HeartRateCandidateSmoothingFactor,
                options.HeartRateSmoothingFactor,
                options.OxygenSaturationSmoothingFactor,
                options.MinimumAutocorrelationChannelCorrelation,
                options.EstimatorDisagreementQualityFactor,
                options.AutocorrelationFallbackQualityFactor,
                options.IntervalDeviationQualityPenalty,
                options.AutocorrelationTargetSampleRate,
                options.AutocorrelationHarmonicThreshold,
                options.AutocorrelationShoulderFraction,
                options.AutocorrelationProminenceForFullQuality,
                options.AutocorrelationCyclesForFullQuality,
                options.MinimumWeakSignalOxygenSaturationPerfusionIndex,
                options.MinimumWeakSignalOxygenSaturationCorrelation,
                options.WeakSignalOxygenSaturationRatioTolerance,
                options.WeakSignalOxygenSaturationConfirmationCount,
                options.StableOpticalSignalHeartRateMissLimit)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Max30100PulseOximeter"/> class.
        /// </summary>
        /// <param name="sampleRate">The configured sensor sample rate in samples per second.</param>
        /// <param name="windowSeconds">The analysis-window length in seconds.</param>
        /// <param name="abruptChangeRelativePercent">
        /// The one-sample relative red or infrared change treated as movement.
        /// </param>
        /// <param name="minimumAbruptChangeCounts">
        /// The minimum one-sample red or infrared change treated as movement, in ADC counts.
        /// </param>
        /// <param name="heartRateCandidateTolerance">
        /// The maximum difference in BPM that is smoothed into the established candidate.
        /// </param>
        /// <param name="heartRateCandidateMissLimit">
        /// The number of consecutive invalid analysis windows that clears the established candidate.
        /// </param>
        /// <param name="fingerDetectionThreshold">The minimum red and infrared level used to detect finger contact.</param>
        /// <param name="minimumHeartRate">The minimum supported heart rate in BPM.</param>
        /// <param name="maximumHeartRate">The maximum supported heart rate in BPM.</param>
        /// <param name="heartRateEstimatorAgreementTolerance">
        /// The maximum difference in BPM between interval and periodic estimates.
        /// </param>
        /// <param name="minimumHeartRateQuality">The minimum accepted heart-rate quality percentage.</param>
        /// <param name="minimumAutocorrelationQuality">The minimum accepted autocorrelation quality percentage.</param>
        /// <param name="minimumBeatIntervalCount">The minimum number of consistent beat intervals.</param>
        /// <param name="strongCandidateQuality">The quality required for immediate strong-candidate acceptance.</param>
        /// <param name="minimumHeartRatePerfusionIndex">
        /// The minimum infrared perfusion index percentage for heart-rate estimation.
        /// </param>
        /// <param name="minimumOxygenSaturationPerfusionIndex">
        /// The minimum red and infrared perfusion index percentage for oxygen-saturation estimation.
        /// </param>
        /// <param name="minimumOxygenSaturationCorrelation">
        /// The minimum red and infrared waveform correlation for oxygen-saturation estimation.
        /// </param>
        /// <param name="minimumOxygenSaturationRatio">
        /// The minimum ratio of ratios accepted for oxygen-saturation estimation.
        /// </param>
        /// <param name="maximumOxygenSaturationRatio">
        /// The maximum ratio of ratios accepted for oxygen-saturation estimation.
        /// </param>
        /// <param name="baselineTrackingRate">The baseline-tracking rate used by the high-pass filter.</param>
        /// <param name="signalSmoothingRate">The smoothing rate used by the low-pass filter.</param>
        /// <param name="filterSettlingSeconds">The duration discarded while the filters settle.</param>
        /// <param name="beatDetectionThresholdFraction">
        /// The infrared AC RMS fraction used for positive and negative beat-detection hysteresis.
        /// </param>
        /// <param name="minimumBeatDetectionThreshold">The minimum beat-detection hysteresis in ADC counts.</param>
        /// <param name="minimumAcRms">The minimum red or infrared AC RMS amplitude required for measurements.</param>
        /// <param name="missedBeatIntervalFactor">
        /// The interval-to-median factor above which one missed beat is inferred.
        /// </param>
        /// <param name="beatIntervalToleranceFraction">
        /// The maximum accepted interval deviation as a fraction of the median interval.
        /// </param>
        /// <param name="fingerLossSignalResetSamples">
        /// The consecutive no-finger samples that clear the current signal analysis.
        /// </param>
        /// <param name="fingerRemovalDebounceSamples">
        /// The consecutive no-finger samples that confirm complete finger removal.
        /// </param>
        /// <param name="initialFingerSettlingSeconds">The settling duration after initial finger contact.</param>
        /// <param name="motionRecoverySettlingSeconds">
        /// The settling duration after abrupt movement or temporary contact loss.
        /// </param>
        /// <param name="analysisIntervalSeconds">The interval between complete signal analyses.</param>
        /// <param name="heartRateConfirmationCount">
        /// The number of matching candidates required to confirm a heart rate.
        /// </param>
        /// <param name="alternateHeartRateConfirmationCount">
        /// The number of matching alternate candidates required to replace an established heart rate.
        /// </param>
        /// <param name="heartRateCandidateSmoothingFactor">
        /// The weight of the newest candidate during candidate smoothing.
        /// </param>
        /// <param name="heartRateSmoothingFactor">The weight of the newest confirmed heart rate.</param>
        /// <param name="oxygenSaturationSmoothingFactor">The weight of the newest oxygen-saturation result.</param>
        /// <param name="minimumAutocorrelationChannelCorrelation">
        /// The minimum red and infrared correlation for autocorrelation fallback.
        /// </param>
        /// <param name="estimatorDisagreementQualityFactor">
        /// The quality multiplier applied when interval and autocorrelation estimates disagree.
        /// </param>
        /// <param name="autocorrelationFallbackQualityFactor">
        /// The quality multiplier applied when autocorrelation supplies the only estimate.
        /// </param>
        /// <param name="intervalDeviationQualityPenalty">
        /// The multiplier converting relative interval deviation into a quality penalty.
        /// </param>
        /// <param name="autocorrelationTargetSampleRate">The target sample rate used for autocorrelation.</param>
        /// <param name="autocorrelationHarmonicThreshold">
        /// The relative peak strength needed to select a half-lag harmonic.
        /// </param>
        /// <param name="autocorrelationShoulderFraction">
        /// The lag fraction used to evaluate autocorrelation peak shoulders.
        /// </param>
        /// <param name="autocorrelationProminenceForFullQuality">
        /// The peak prominence that produces full prominence evidence.
        /// </param>
        /// <param name="autocorrelationCyclesForFullQuality">
        /// The waveform-cycle count that produces full cycle evidence.
        /// </param>
        /// <param name="minimumWeakSignalOxygenSaturationPerfusionIndex">
        /// The minimum channel perfusion index for adaptive weak-signal oxygen saturation.
        /// </param>
        /// <param name="minimumWeakSignalOxygenSaturationCorrelation">
        /// The minimum channel correlation for adaptive weak-signal oxygen saturation.
        /// </param>
        /// <param name="weakSignalOxygenSaturationRatioTolerance">
        /// The maximum ratio change accepted between weak-signal analysis windows.
        /// </param>
        /// <param name="weakSignalOxygenSaturationConfirmationCount">
        /// The consecutive stable weak-signal windows required before publishing oxygen saturation.
        /// </param>
        /// <param name="stableOpticalSignalHeartRateMissLimit">
        /// The heart-rate miss limit used while perfusion and channel correlation remain coherent.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="sampleRate"/> is outside 50 through 1000, <paramref name="windowSeconds"/> is outside 3 through 8,
        /// <paramref name="abruptChangeRelativePercent"/> is outside zero through 100, or
        /// <paramref name="minimumAbruptChangeCounts"/> is outside zero through 65535, or
        /// <paramref name="heartRateCandidateTolerance"/> is outside zero through 180 BPM, or
        /// another processing threshold is outside its documented range.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The requested analysis window contains more than 1000 samples, a minimum is not below its maximum,
        /// or the supported heart-rate range cannot provide enough samples for analysis.
        /// </exception>
        internal Max30100PulseOximeter(
            int sampleRate = DefaultSampleRate,
            int windowSeconds = DefaultWindowSeconds,
            int abruptChangeRelativePercent = DefaultAbruptChangeRelativePercent,
            int minimumAbruptChangeCounts = DefaultMinimumAbruptChangeCounts,
            double heartRateCandidateTolerance = DefaultHeartRateCandidateTolerance,
            int heartRateCandidateMissLimit = DefaultHeartRateCandidateMissLimit,
            int fingerDetectionThreshold = DefaultFingerDetectionThreshold,
            double minimumHeartRate = DefaultMinimumHeartRate,
            double maximumHeartRate = DefaultMaximumHeartRate,
            double heartRateEstimatorAgreementTolerance = DefaultHeartRateEstimatorAgreementTolerance,
            double minimumHeartRateQuality = DefaultMinimumHeartRateQuality,
            double minimumAutocorrelationQuality = DefaultMinimumAutocorrelationQuality,
            int minimumBeatIntervalCount = DefaultMinimumBeatIntervalCount,
            double strongCandidateQuality = DefaultStrongCandidateQuality,
            double minimumHeartRatePerfusionIndex = DefaultMinimumHeartRatePerfusionIndex,
            double minimumOxygenSaturationPerfusionIndex = DefaultMinimumOxygenSaturationPerfusionIndex,
            double minimumOxygenSaturationCorrelation = DefaultMinimumOxygenSaturationCorrelation,
            double minimumOxygenSaturationRatio = DefaultMinimumOxygenSaturationRatio,
            double maximumOxygenSaturationRatio = DefaultMaximumOxygenSaturationRatio,
            double baselineTrackingRate = DefaultBaselineTrackingRate,
            double signalSmoothingRate = DefaultSignalSmoothingRate,
            double filterSettlingSeconds = DefaultFilterSettlingSeconds,
            double beatDetectionThresholdFraction = DefaultBeatDetectionThresholdFraction,
            double minimumBeatDetectionThreshold = DefaultMinimumBeatDetectionThreshold,
            double minimumAcRms = DefaultMinimumAcRms,
            double missedBeatIntervalFactor = DefaultMissedBeatIntervalFactor,
            double beatIntervalToleranceFraction = DefaultBeatIntervalToleranceFraction,
            int fingerLossSignalResetSamples = DefaultFingerLossSignalResetSamples,
            int fingerRemovalDebounceSamples = DefaultFingerRemovalDebounceSamples,
            double initialFingerSettlingSeconds = DefaultInitialFingerSettlingSeconds,
            double motionRecoverySettlingSeconds = DefaultMotionRecoverySettlingSeconds,
            double analysisIntervalSeconds = DefaultAnalysisIntervalSeconds,
            int heartRateConfirmationCount = DefaultHeartRateConfirmationCount,
            int alternateHeartRateConfirmationCount = DefaultAlternateHeartRateConfirmationCount,
            double heartRateCandidateSmoothingFactor = DefaultHeartRateCandidateSmoothingFactor,
            double heartRateSmoothingFactor = DefaultHeartRateSmoothingFactor,
            double oxygenSaturationSmoothingFactor = DefaultOxygenSaturationSmoothingFactor,
            double minimumAutocorrelationChannelCorrelation = DefaultMinimumAutocorrelationChannelCorrelation,
            double estimatorDisagreementQualityFactor = DefaultEstimatorDisagreementQualityFactor,
            double autocorrelationFallbackQualityFactor = DefaultAutocorrelationFallbackQualityFactor,
            double intervalDeviationQualityPenalty = DefaultIntervalDeviationQualityPenalty,
            int autocorrelationTargetSampleRate = DefaultAutocorrelationTargetSampleRate,
            double autocorrelationHarmonicThreshold = DefaultAutocorrelationHarmonicThreshold,
            double autocorrelationShoulderFraction = DefaultAutocorrelationShoulderFraction,
            double autocorrelationProminenceForFullQuality = DefaultAutocorrelationProminenceForFullQuality,
            double autocorrelationCyclesForFullQuality = DefaultAutocorrelationCyclesForFullQuality,
            double minimumWeakSignalOxygenSaturationPerfusionIndex = DefaultMinimumWeakSignalOxygenSaturationPerfusionIndex,
            double minimumWeakSignalOxygenSaturationCorrelation = DefaultMinimumWeakSignalOxygenSaturationCorrelation,
            double weakSignalOxygenSaturationRatioTolerance = DefaultWeakSignalOxygenSaturationRatioTolerance,
            int weakSignalOxygenSaturationConfirmationCount = DefaultWeakSignalOxygenSaturationConfirmationCount,
            int stableOpticalSignalHeartRateMissLimit = DefaultStableOpticalSignalHeartRateMissLimit)
        {
            if (sampleRate < MinimumSampleRate || sampleRate > MaximumSampleRate)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (windowSeconds < MinimumWindowSeconds || windowSeconds > MaximumWindowSeconds)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (abruptChangeRelativePercent < 0 || abruptChangeRelativePercent > 100)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (minimumAbruptChangeCounts < 0 || minimumAbruptChangeCounts > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (double.IsNaN(heartRateCandidateTolerance)
                || heartRateCandidateTolerance < 0
                || heartRateCandidateTolerance > MaximumCandidateTolerance)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (heartRateCandidateMissLimit < MinimumCandidateMissLimit
                || heartRateCandidateMissLimit > MaximumCandidateMissLimit)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (fingerDetectionThreshold < 0 || fingerDetectionThreshold > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (double.IsNaN(minimumHeartRate)
                || double.IsNaN(maximumHeartRate)
                || minimumHeartRate < MinimumConfigurableHeartRate
                || maximumHeartRate > MaximumConfigurableHeartRate)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (minimumHeartRate >= maximumHeartRate)
            {
                throw new ArgumentException();
            }

            if (double.IsNaN(heartRateEstimatorAgreementTolerance)
                || heartRateEstimatorAgreementTolerance < 0
                || heartRateEstimatorAgreementTolerance > maximumHeartRate)
            {
                throw new ArgumentOutOfRangeException();
            }

            ValidatePercentage(minimumHeartRateQuality);
            ValidatePercentage(minimumAutocorrelationQuality);
            ValidatePercentage(strongCandidateQuality);
            ValidatePercentage(minimumHeartRatePerfusionIndex);
            ValidatePercentage(minimumOxygenSaturationPerfusionIndex);

            if (minimumBeatIntervalCount < MinimumBeatIntervalCountLimit
                || minimumBeatIntervalCount > MaximumBeatIntervalCount)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (double.IsNaN(minimumOxygenSaturationCorrelation)
                || minimumOxygenSaturationCorrelation < -1
                || minimumOxygenSaturationCorrelation > 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (double.IsNaN(minimumOxygenSaturationRatio)
                || double.IsNaN(maximumOxygenSaturationRatio)
                || minimumOxygenSaturationRatio < 0
                || maximumOxygenSaturationRatio > MaximumOxygenSaturationRatioLimit)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (minimumOxygenSaturationRatio >= maximumOxygenSaturationRatio)
            {
                throw new ArgumentException();
            }

            if (double.IsNaN(baselineTrackingRate)
                || baselineTrackingRate <= 0
                || baselineTrackingRate > sampleRate)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (double.IsNaN(signalSmoothingRate)
                || signalSmoothingRate <= 0
                || signalSmoothingRate > sampleRate)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (double.IsNaN(filterSettlingSeconds)
                || filterSettlingSeconds < 0
                || filterSettlingSeconds >= windowSeconds)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (double.IsNaN(beatDetectionThresholdFraction)
                || beatDetectionThresholdFraction <= 0
                || beatDetectionThresholdFraction > 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (double.IsNaN(minimumBeatDetectionThreshold)
                || minimumBeatDetectionThreshold < 0
                || minimumBeatDetectionThreshold > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (double.IsNaN(minimumAcRms) || minimumAcRms < 0 || minimumAcRms > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (double.IsNaN(missedBeatIntervalFactor)
                || missedBeatIntervalFactor <= 1
                || missedBeatIntervalFactor > MaximumMissedBeatIntervalFactor)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (double.IsNaN(beatIntervalToleranceFraction)
                || beatIntervalToleranceFraction <= 0
                || beatIntervalToleranceFraction > 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (fingerLossSignalResetSamples < MinimumDebounceSamples
                || fingerLossSignalResetSamples > MaximumDebounceSamples)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (fingerRemovalDebounceSamples < MinimumDebounceSamples
                || fingerRemovalDebounceSamples > MaximumDebounceSamples)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (fingerLossSignalResetSamples >= fingerRemovalDebounceSamples)
            {
                throw new ArgumentException();
            }

            if (double.IsNaN(initialFingerSettlingSeconds)
                || initialFingerSettlingSeconds < 0
                || initialFingerSettlingSeconds > windowSeconds)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (double.IsNaN(motionRecoverySettlingSeconds)
                || motionRecoverySettlingSeconds < 0
                || motionRecoverySettlingSeconds > windowSeconds)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (double.IsNaN(analysisIntervalSeconds)
                || analysisIntervalSeconds <= 0
                || analysisIntervalSeconds > windowSeconds
                || (sampleRate * analysisIntervalSeconds) < 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (heartRateConfirmationCount < MinimumConfirmationCount
                || heartRateConfirmationCount > MaximumConfirmationCount)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (alternateHeartRateConfirmationCount < MinimumConfirmationCount
                || alternateHeartRateConfirmationCount > MaximumConfirmationCount)
            {
                throw new ArgumentOutOfRangeException();
            }

            ValidateSmoothingFactor(heartRateCandidateSmoothingFactor);
            ValidateSmoothingFactor(heartRateSmoothingFactor);
            ValidateSmoothingFactor(oxygenSaturationSmoothingFactor);

            if (double.IsNaN(minimumAutocorrelationChannelCorrelation)
                || minimumAutocorrelationChannelCorrelation < -1
                || minimumAutocorrelationChannelCorrelation > 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            ValidateQualityFactor(estimatorDisagreementQualityFactor);
            ValidateQualityFactor(autocorrelationFallbackQualityFactor);

            if (double.IsNaN(intervalDeviationQualityPenalty)
                || intervalDeviationQualityPenalty <= 0
                || intervalDeviationQualityPenalty > MaximumIntervalDeviationQualityPenalty)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (autocorrelationTargetSampleRate < MinimumAutocorrelationTargetSampleRate
                || autocorrelationTargetSampleRate > sampleRate)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (double.IsNaN(autocorrelationHarmonicThreshold)
                || autocorrelationHarmonicThreshold <= 0
                || autocorrelationHarmonicThreshold > 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (double.IsNaN(autocorrelationShoulderFraction)
                || autocorrelationShoulderFraction <= 0
                || autocorrelationShoulderFraction > MaximumAutocorrelationShoulderFraction)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (double.IsNaN(autocorrelationProminenceForFullQuality)
                || autocorrelationProminenceForFullQuality <= 0
                || autocorrelationProminenceForFullQuality > MaximumNormalizedAutocorrelationProminence)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (double.IsNaN(autocorrelationCyclesForFullQuality)
                || autocorrelationCyclesForFullQuality <= 0
                || autocorrelationCyclesForFullQuality > MaximumAutocorrelationCyclesForFullQuality)
            {
                throw new ArgumentOutOfRangeException();
            }

            ValidatePercentage(minimumWeakSignalOxygenSaturationPerfusionIndex);
            if (minimumWeakSignalOxygenSaturationPerfusionIndex > minimumOxygenSaturationPerfusionIndex)
            {
                throw new ArgumentException();
            }

            if (double.IsNaN(minimumWeakSignalOxygenSaturationCorrelation)
                || minimumWeakSignalOxygenSaturationCorrelation < minimumOxygenSaturationCorrelation
                || minimumWeakSignalOxygenSaturationCorrelation > 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (double.IsNaN(weakSignalOxygenSaturationRatioTolerance)
                || weakSignalOxygenSaturationRatioTolerance <= 0
                || weakSignalOxygenSaturationRatioTolerance > 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (weakSignalOxygenSaturationConfirmationCount < MinimumConfirmationCount
                || weakSignalOxygenSaturationConfirmationCount > MaximumConfirmationCount)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (stableOpticalSignalHeartRateMissLimit < heartRateCandidateMissLimit
                || stableOpticalSignalHeartRateMissLimit > MaximumCandidateMissLimit)
            {
                throw new ArgumentOutOfRangeException();
            }

            int windowSize = sampleRate * windowSeconds;
            if (windowSize > MaximumAnalysisWindowSamples)
            {
                throw new ArgumentException();
            }

            _sampleRate = sampleRate;
            _autocorrelationDecimation = sampleRate / autocorrelationTargetSampleRate;
            if (_autocorrelationDecimation < 1)
            {
                _autocorrelationDecimation = 1;
            }

            int autocorrelationBufferLength =
                (int)((sampleRate * 60) / (minimumHeartRate * _autocorrelationDecimation)) + 2;
            if (autocorrelationBufferLength >= windowSize)
            {
                throw new ArgumentException();
            }

            _autocorrelation = new double[autocorrelationBufferLength];
            _beatIntervals = new int[MaximumBeatIntervalCount];
            _filteredInfraredSamples = new double[windowSize];
            _filteredRedSamples = new double[windowSize];
            _infraredSamples = new ushort[windowSize];
            _redSamples = new ushort[windowSize];
            AbruptChangeRelativePercent = abruptChangeRelativePercent;
            MinimumAbruptChangeCounts = minimumAbruptChangeCounts;
            HeartRateCandidateTolerance = heartRateCandidateTolerance;
            HeartRateCandidateMissLimit = heartRateCandidateMissLimit;
            FingerDetectionThreshold = fingerDetectionThreshold;
            MinimumHeartRate = minimumHeartRate;
            MaximumHeartRate = maximumHeartRate;
            HeartRateEstimatorAgreementTolerance = heartRateEstimatorAgreementTolerance;
            MinimumHeartRateQuality = minimumHeartRateQuality;
            MinimumAutocorrelationQuality = minimumAutocorrelationQuality;
            MinimumBeatIntervalCount = minimumBeatIntervalCount;
            StrongCandidateQuality = strongCandidateQuality;
            MinimumHeartRatePerfusionIndex = minimumHeartRatePerfusionIndex;
            MinimumOxygenSaturationPerfusionIndex = minimumOxygenSaturationPerfusionIndex;
            MinimumOxygenSaturationCorrelation = minimumOxygenSaturationCorrelation;
            MinimumOxygenSaturationRatio = minimumOxygenSaturationRatio;
            MaximumOxygenSaturationRatio = maximumOxygenSaturationRatio;
            BaselineTrackingRate = baselineTrackingRate;
            SignalSmoothingRate = signalSmoothingRate;
            FilterSettlingSeconds = filterSettlingSeconds;
            BeatDetectionThresholdFraction = beatDetectionThresholdFraction;
            MinimumBeatDetectionThreshold = minimumBeatDetectionThreshold;
            MinimumAcRms = minimumAcRms;
            MissedBeatIntervalFactor = missedBeatIntervalFactor;
            BeatIntervalToleranceFraction = beatIntervalToleranceFraction;
            FingerLossSignalResetSamples = fingerLossSignalResetSamples;
            FingerRemovalDebounceSamples = fingerRemovalDebounceSamples;
            InitialFingerSettlingSeconds = initialFingerSettlingSeconds;
            MotionRecoverySettlingSeconds = motionRecoverySettlingSeconds;
            AnalysisIntervalSeconds = analysisIntervalSeconds;
            HeartRateConfirmationCount = heartRateConfirmationCount;
            AlternateHeartRateConfirmationCount = alternateHeartRateConfirmationCount;
            HeartRateCandidateSmoothingFactor = heartRateCandidateSmoothingFactor;
            HeartRateSmoothingFactor = heartRateSmoothingFactor;
            OxygenSaturationSmoothingFactor = oxygenSaturationSmoothingFactor;
            MinimumAutocorrelationChannelCorrelation = minimumAutocorrelationChannelCorrelation;
            EstimatorDisagreementQualityFactor = estimatorDisagreementQualityFactor;
            AutocorrelationFallbackQualityFactor = autocorrelationFallbackQualityFactor;
            IntervalDeviationQualityPenalty = intervalDeviationQualityPenalty;
            AutocorrelationTargetSampleRate = autocorrelationTargetSampleRate;
            AutocorrelationHarmonicThreshold = autocorrelationHarmonicThreshold;
            AutocorrelationShoulderFraction = autocorrelationShoulderFraction;
            AutocorrelationProminenceForFullQuality = autocorrelationProminenceForFullQuality;
            AutocorrelationCyclesForFullQuality = autocorrelationCyclesForFullQuality;
            MinimumWeakSignalOxygenSaturationPerfusionIndex =
                minimumWeakSignalOxygenSaturationPerfusionIndex;
            MinimumWeakSignalOxygenSaturationCorrelation = minimumWeakSignalOxygenSaturationCorrelation;
            WeakSignalOxygenSaturationRatioTolerance = weakSignalOxygenSaturationRatioTolerance;
            WeakSignalOxygenSaturationConfirmationCount = weakSignalOxygenSaturationConfirmationCount;
            StableOpticalSignalHeartRateMissLimit = stableOpticalSignalHeartRateMissLimit;
            _initialFingerSettlingSamples = (int)(sampleRate * initialFingerSettlingSeconds);
            _motionRecoverySettlingSamples = (int)(sampleRate * motionRecoverySettlingSeconds);
            _analysisIntervalSamples = (int)(sampleRate * analysisIntervalSeconds);
            OxygenSaturationCalibrationIntercept = DefaultOxygenSaturationCalibrationIntercept;
            OxygenSaturationCalibrationSlope = DefaultOxygenSaturationCalibrationSlope;
        }

        /// <summary>Gets a value indicating whether a finger is currently detected.</summary>
        /// <value>True when the current optical levels indicate finger contact.</value>
        public bool FingerDetected { get; private set; }

        /// <summary>Gets the one-sample relative change treated as abrupt movement.</summary>
        /// <value>The red or infrared change as a percentage of the previous DC level.</value>
        public int AbruptChangeRelativePercent { get; }

        /// <summary>Gets the minimum one-sample change treated as abrupt movement.</summary>
        /// <value>The minimum red or infrared change in ADC counts.</value>
        public int MinimumAbruptChangeCounts { get; }

        /// <summary>Gets the maximum difference between consecutive heart-rate candidates.</summary>
        /// <value>
        /// The tolerance in BPM. Larger differences are treated as a new candidate that must repeat before replacing the established rate.
        /// </value>
        public double HeartRateCandidateTolerance { get; }

        /// <summary>Gets the consecutive invalid-analysis limit before clearing a heart-rate candidate.</summary>
        /// <value>
        /// The number of invalid windows that clears the candidate. A confirmed rate is held for one fewer windows.
        /// </value>
        public int HeartRateCandidateMissLimit { get; }

        /// <summary>Gets the minimum red and infrared level used to detect finger contact.</summary>
        /// <value>The raw ADC threshold.</value>
        public int FingerDetectionThreshold { get; }

        /// <summary>Gets the minimum supported heart rate.</summary>
        /// <value>The minimum rate in BPM.</value>
        public double MinimumHeartRate { get; }

        /// <summary>Gets the maximum supported heart rate.</summary>
        /// <value>The maximum rate in BPM.</value>
        public double MaximumHeartRate { get; }

        /// <summary>Gets the maximum difference between interval and periodic heart-rate estimates.</summary>
        /// <value>The agreement tolerance in BPM.</value>
        public double HeartRateEstimatorAgreementTolerance { get; }

        /// <summary>Gets the minimum accepted heart-rate quality.</summary>
        /// <value>The minimum quality percentage.</value>
        public double MinimumHeartRateQuality { get; }

        /// <summary>Gets the minimum accepted autocorrelation quality.</summary>
        /// <value>The minimum quality percentage.</value>
        public double MinimumAutocorrelationQuality { get; }

        /// <summary>Gets the minimum number of consistent beat intervals.</summary>
        /// <value>The minimum accepted interval count.</value>
        public int MinimumBeatIntervalCount { get; }

        /// <summary>Gets the quality required for immediate strong-candidate acceptance.</summary>
        /// <value>The strong-candidate quality percentage.</value>
        public double StrongCandidateQuality { get; }

        /// <summary>Gets the minimum infrared perfusion index for heart-rate estimation.</summary>
        /// <value>The minimum perfusion index percentage.</value>
        public double MinimumHeartRatePerfusionIndex { get; }

        /// <summary>Gets the minimum red and infrared perfusion index for oxygen-saturation estimation.</summary>
        /// <value>The minimum perfusion index percentage.</value>
        public double MinimumOxygenSaturationPerfusionIndex { get; }

        /// <summary>Gets the minimum red and infrared waveform correlation for oxygen-saturation estimation.</summary>
        /// <value>The minimum correlation.</value>
        public double MinimumOxygenSaturationCorrelation { get; }

        /// <summary>Gets the minimum ratio of ratios accepted for oxygen-saturation estimation.</summary>
        /// <value>The minimum ratio.</value>
        public double MinimumOxygenSaturationRatio { get; }

        /// <summary>Gets the maximum ratio of ratios accepted for oxygen-saturation estimation.</summary>
        /// <value>The maximum ratio.</value>
        public double MaximumOxygenSaturationRatio { get; }

        /// <summary>Gets the baseline-tracking rate used by the high-pass filter.</summary>
        /// <value>The filter rate per second. The per-sample coefficient is this value divided by the sample rate.</value>
        public double BaselineTrackingRate { get; }

        /// <summary>Gets the smoothing rate used by the low-pass filter.</summary>
        /// <value>The filter rate per second. The per-sample coefficient is this value divided by the sample rate.</value>
        public double SignalSmoothingRate { get; }

        /// <summary>Gets the duration discarded while the filters settle.</summary>
        /// <value>The settling duration in seconds.</value>
        public double FilterSettlingSeconds { get; }

        /// <summary>Gets the RMS fraction used for beat-detection hysteresis.</summary>
        /// <value>The positive and negative hysteresis threshold as a fraction of infrared AC RMS.</value>
        public double BeatDetectionThresholdFraction { get; }

        /// <summary>Gets the minimum beat-detection hysteresis.</summary>
        /// <value>The minimum positive and negative threshold in ADC counts.</value>
        public double MinimumBeatDetectionThreshold { get; }

        /// <summary>Gets the minimum AC RMS amplitude required for pulse measurements.</summary>
        /// <value>The minimum amplitude in ADC counts.</value>
        public double MinimumAcRms { get; }

        /// <summary>Gets the interval factor used to infer one missed beat.</summary>
        /// <value>The interval-to-median factor.</value>
        public double MissedBeatIntervalFactor { get; }

        /// <summary>Gets the accepted deviation from the median beat interval.</summary>
        /// <value>The maximum deviation as a fraction of the median interval.</value>
        public double BeatIntervalToleranceFraction { get; }

        /// <summary>Gets the no-finger sample count that clears signal analysis.</summary>
        /// <value>The consecutive sample count.</value>
        public int FingerLossSignalResetSamples { get; }

        /// <summary>Gets the no-finger sample count that confirms finger removal.</summary>
        /// <value>The consecutive sample count.</value>
        public int FingerRemovalDebounceSamples { get; }

        /// <summary>Gets the settling duration after initial finger contact.</summary>
        /// <value>The duration in seconds.</value>
        public double InitialFingerSettlingSeconds { get; }

        /// <summary>Gets the settling duration after abrupt movement or temporary contact loss.</summary>
        /// <value>The duration in seconds.</value>
        public double MotionRecoverySettlingSeconds { get; }

        /// <summary>Gets the interval between complete signal analyses.</summary>
        /// <value>The interval in seconds.</value>
        public double AnalysisIntervalSeconds { get; }

        /// <summary>Gets the matching-candidate count required to confirm a heart rate.</summary>
        /// <value>The required candidate count.</value>
        public int HeartRateConfirmationCount { get; }

        /// <summary>Gets the matching alternate-candidate count required to replace a heart rate.</summary>
        /// <value>The required alternate-candidate count.</value>
        public int AlternateHeartRateConfirmationCount { get; }

        /// <summary>Gets the weight of a new heart-rate candidate during candidate smoothing.</summary>
        /// <value>A value greater than zero through one.</value>
        public double HeartRateCandidateSmoothingFactor { get; }

        /// <summary>Gets the weight of a new confirmed heart rate during output smoothing.</summary>
        /// <value>A value greater than zero through one.</value>
        public double HeartRateSmoothingFactor { get; }

        /// <summary>Gets the weight of a new oxygen-saturation result during output smoothing.</summary>
        /// <value>A value greater than zero through one.</value>
        public double OxygenSaturationSmoothingFactor { get; }

        /// <summary>Gets the minimum red and infrared correlation for autocorrelation fallback.</summary>
        /// <value>A value from minus one through one.</value>
        public double MinimumAutocorrelationChannelCorrelation { get; }

        /// <summary>Gets the quality multiplier applied when heart-rate estimators disagree.</summary>
        /// <value>A value from zero through one.</value>
        public double EstimatorDisagreementQualityFactor { get; }

        /// <summary>Gets the quality multiplier applied when autocorrelation supplies the only estimate.</summary>
        /// <value>A value from zero through one.</value>
        public double AutocorrelationFallbackQualityFactor { get; }

        /// <summary>Gets the multiplier converting relative interval deviation into a quality penalty.</summary>
        /// <value>The quality-penalty multiplier.</value>
        public double IntervalDeviationQualityPenalty { get; }

        /// <summary>Gets the target sample rate used for autocorrelation.</summary>
        /// <value>The target rate in samples per second.</value>
        public int AutocorrelationTargetSampleRate { get; }

        /// <summary>Gets the relative strength needed to select a half-lag harmonic.</summary>
        /// <value>A value greater than zero through one.</value>
        public double AutocorrelationHarmonicThreshold { get; }

        /// <summary>Gets the lag fraction used to evaluate autocorrelation peak shoulders.</summary>
        /// <value>A value greater than zero through one half.</value>
        public double AutocorrelationShoulderFraction { get; }

        /// <summary>Gets the peak prominence that produces full prominence evidence.</summary>
        /// <value>The normalized prominence.</value>
        public double AutocorrelationProminenceForFullQuality { get; }

        /// <summary>Gets the waveform-cycle count that produces full cycle evidence.</summary>
        /// <value>The cycle count.</value>
        public double AutocorrelationCyclesForFullQuality { get; }

        /// <summary>Gets the minimum channel perfusion index for adaptive weak-signal SpO₂.</summary>
        /// <value>The minimum perfusion index percentage.</value>
        public double MinimumWeakSignalOxygenSaturationPerfusionIndex { get; }

        /// <summary>Gets the minimum channel correlation for adaptive weak-signal SpO₂.</summary>
        /// <value>A value from minus one through one.</value>
        public double MinimumWeakSignalOxygenSaturationCorrelation { get; }

        /// <summary>Gets the maximum ratio change accepted between weak-signal analysis windows.</summary>
        /// <value>The maximum absolute ratio change.</value>
        public double WeakSignalOxygenSaturationRatioTolerance { get; }

        /// <summary>Gets the stable-window count required before publishing weak-signal SpO₂.</summary>
        /// <value>The consecutive analysis-window count.</value>
        public int WeakSignalOxygenSaturationConfirmationCount { get; }

        /// <summary>Gets the heart-rate miss limit used while the optical signal remains coherent.</summary>
        /// <value>The consecutive invalid-analysis limit.</value>
        public int StableOpticalSignalHeartRateMissLimit { get; }

        /// <summary>Gets a value indicating whether finger contact is still stabilizing.</summary>
        /// <value>True while initial pressure and optical-level changes are being discarded.</value>
        public bool IsSettling => FingerDetected && _settlingSamplesRemaining > 0;

        /// <summary>Gets the percentage of the analysis window currently filled with stable samples.</summary>
        /// <value>A value from zero through 100.</value>
        public double AnalysisWindowProgress => _sampleCount * 100.0 / _infraredSamples.Length;

        /// <summary>Gets the reason the current signal has not produced a valid heart rate.</summary>
        /// <value>The current signal diagnostic, or <see cref="Max30100SignalDiagnostic.None"/>.</value>
        public Max30100SignalDiagnostic SignalDiagnostic { get; private set; }

        /// <summary>Gets a value indicating whether pulse and oxygen-saturation estimates are available.</summary>
        /// <value>True when the current estimates passed basic signal checks.</value>
        public bool HasValidReading { get; private set; }

        /// <summary>Gets a value indicating whether the pulse-rate estimate is available.</summary>
        /// <value>True when the infrared waveform has a sufficiently periodic pulse signal.</value>
        public bool HasValidHeartRate { get; private set; }

        /// <summary>Gets a value indicating whether the last confirmed heart rate is being held through a weak analysis window.</summary>
        /// <value>True when the displayed heart rate was retained rather than refreshed by the latest window.</value>
        public bool IsHeartRateHeld { get; private set; }

        /// <summary>Gets a value indicating whether the oxygen-saturation estimate is available.</summary>
        /// <value>True when both optical channels pass the ratio-of-ratios signal checks.</value>
        public bool HasValidOxygenSaturation { get; private set; }

        /// <summary>Gets the current unconfirmed pulse-rate candidate.</summary>
        /// <value>The candidate rate in beats per minute, or zero when no candidate is available.</value>
        public double CandidateHeartRate => _candidateHeartRate;

        /// <summary>Gets the number of beat intervals accepted during the latest analysis.</summary>
        /// <value>The number of accepted beat intervals.</value>
        public int DetectedBeatIntervals => _detectedIntervalCount;

        /// <summary>Gets the number of positive beat-threshold crossings found during the latest analysis.</summary>
        /// <value>The number of detected threshold crossings.</value>
        public int DetectedBeatCrossings => _detectedBeatCrossingCount;

        /// <summary>Gets the number of physiologically plausible beat intervals found before consistency filtering.</summary>
        /// <value>The number of raw beat intervals.</value>
        public int RawBeatIntervals => _rawBeatIntervalCount;

        /// <summary>Gets the infrared AC root-mean-square amplitude from the latest analysis.</summary>
        /// <value>The filtered infrared amplitude in ADC counts.</value>
        public double InfraredAcRms { get; private set; }

        /// <summary>Gets the positive and negative threshold used for beat detection.</summary>
        /// <value>The beat-detection threshold in filtered ADC counts.</value>
        public double BeatDetectionThreshold { get; private set; }

        /// <summary>Gets the heart rate independently estimated from waveform periodicity.</summary>
        /// <value>The normalized-autocorrelation estimate in beats per minute, or zero when no periodic peak is available.</value>
        public double AutocorrelationHeartRate { get; private set; }

        /// <summary>Gets the confidence of the normalized-autocorrelation estimate.</summary>
        /// <value>A value from zero through 100 based on peak strength, prominence, and cycle evidence.</value>
        public double AutocorrelationQuality { get; private set; }

        /// <summary>Gets a value indicating whether interval and autocorrelation estimates disagreed.</summary>
        /// <value>True when both estimators were individually valid but exceeded the configured agreement tolerance.</value>
        public bool HeartRateEstimatorsDisagree { get; private set; }

        /// <summary>Gets a value indicating whether the current candidate used autocorrelation without valid beat intervals.</summary>
        /// <value>True when periodicity supplied the estimate at reduced confidence.</value>
        public bool UsedAutocorrelationFallback { get; private set; }

        /// <summary>Gets a value indicating whether adaptive weak-signal validation supplied SpO₂.</summary>
        /// <value>True when the current oxygen-saturation value passed the adaptive weak-signal path.</value>
        public bool UsedWeakSignalOxygenSaturation { get; private set; }

        /// <summary>Gets the estimated pulse rate in beats per minute.</summary>
        /// <value>The estimated pulse rate, or zero when no valid estimate is available.</value>
        public double HeartRate { get; private set; }

        /// <summary>Gets the experimental oxygen-saturation estimate as a percentage.</summary>
        /// <value>The estimated oxygen saturation, or zero when no valid estimate is available.</value>
        public double OxygenSaturation { get; private set; }

        /// <summary>Gets the infrared AC signal as a percentage of its DC level.</summary>
        /// <value>The infrared perfusion-index approximation.</value>
        public double PerfusionIndex { get; private set; }

        /// <summary>Gets the red AC signal as a percentage of its DC level.</summary>
        /// <value>The red-channel perfusion-index approximation.</value>
        public double RedPerfusionIndex { get; private set; }

        /// <summary>Gets the correlation between the filtered infrared and red waveforms.</summary>
        /// <value>A value from -1 through 1, where 1 indicates matching waveform shapes.</value>
        public double ChannelCorrelation { get; private set; }

        /// <summary>Gets the red-to-infrared AC/DC ratio used by the oxygen-saturation approximation.</summary>
        /// <value>The latest ratio of ratios, or zero before an analysis is available.</value>
        public double RatioOfRatios { get; private set; }

        /// <summary>Gets the intercept used to convert the ratio of ratios to oxygen saturation.</summary>
        /// <value>The intercept in the equation SpO₂ = intercept + slope × ratio.</value>
        public double OxygenSaturationCalibrationIntercept { get; private set; }

        /// <summary>Gets the slope used to convert the ratio of ratios to oxygen saturation.</summary>
        /// <value>The slope in the equation SpO₂ = intercept + slope × ratio.</value>
        public double OxygenSaturationCalibrationSlope { get; private set; }

        /// <summary>Gets the periodic-signal confidence as a percentage.</summary>
        /// <value>A value from zero to 100 representing beat-interval consistency and evidence.</value>
        public double SignalQuality { get; private set; }

        /// <summary>
        /// Fits oxygen-saturation calibration coefficients from paired ratio and reference measurements.
        /// </summary>
        /// <param name="ratios">Stable <see cref="RatioOfRatios"/> measurements.</param>
        /// <param name="referenceOxygenSaturations">Simultaneous reference oxygen-saturation percentages.</param>
        /// <exception cref="ArgumentNullException">Either array is null.</exception>
        /// <exception cref="ArgumentException">
        /// The arrays have different lengths, contain fewer than two points, all ratios are equal, or the fitted slope is not negative.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// A ratio is outside zero through 10, or a reference oxygen-saturation value is outside zero through 100.
        /// </exception>
        public void CalibrateOxygenSaturation(double[] ratios, double[] referenceOxygenSaturations)
        {
            CalculateOxygenSaturationCalibration(
                ratios,
                referenceOxygenSaturations,
                out double intercept,
                out double slope);
            SetOxygenSaturationCalibration(intercept, slope);
        }

        /// <summary>Sets previously calculated oxygen-saturation calibration coefficients.</summary>
        /// <param name="intercept">The intercept in the equation SpO₂ = intercept + slope × ratio.</param>
        /// <param name="slope">The negative slope in the equation SpO₂ = intercept + slope × ratio.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="intercept"/> is outside zero through 200, or <paramref name="slope"/> is outside -200 through zero.
        /// </exception>
        public void SetOxygenSaturationCalibration(double intercept, double slope)
        {
            ValidateOxygenSaturationCalibration(intercept, slope);
            OxygenSaturationCalibrationIntercept = intercept;
            OxygenSaturationCalibrationSlope = slope;
            HasValidOxygenSaturation = false;
            HasValidReading = false;
            OxygenSaturation = 0;
        }

        /// <summary>
        /// Adds one sensor sample to the analysis window.
        /// </summary>
        /// <param name="reading">The sensor sample.</param>
        public void AddSample(Max30100Reading reading)
        {
            bool fingerDetected = reading.Infrared >= FingerDetectionThreshold
                && reading.Red >= FingerDetectionThreshold;
            if (!fingerDetected)
            {
                if (!FingerDetected)
                {
                    Reset();
                    return;
                }

                _noFingerSampleCount++;
                if (_noFingerSampleCount == FingerLossSignalResetSamples)
                {
                    RestartSignalCollection();
                    _settlingSamplesRemaining = _motionRecoverySettlingSamples;
                }

                if (_noFingerSampleCount >= FingerRemovalDebounceSamples)
                {
                    Reset();
                }

                return;
            }

            _noFingerSampleCount = 0;
            if (!FingerDetected)
            {
                FingerDetected = true;
                _settlingSamplesRemaining = _initialFingerSettlingSamples;
                _previousInfrared = reading.Infrared;
                _previousRed = reading.Red;
                _hasPreviousSample = true;
                return;
            }

            if (_hasPreviousSample
                && (IsAbruptChange(_previousInfrared, reading.Infrared)
                    || IsAbruptChange(_previousRed, reading.Red)))
            {
                RestartSignalCollection();
                _settlingSamplesRemaining = _motionRecoverySettlingSamples;
                _previousInfrared = reading.Infrared;
                _previousRed = reading.Red;
                return;
            }

            _previousInfrared = reading.Infrared;
            _previousRed = reading.Red;
            _hasPreviousSample = true;

            if (_settlingSamplesRemaining > 0)
            {
                _settlingSamplesRemaining--;
                return;
            }

            _infraredSamples[_writeIndex] = reading.Infrared;
            _redSamples[_writeIndex] = reading.Red;
            _writeIndex = (_writeIndex + 1) % _infraredSamples.Length;
            if (_sampleCount < _infraredSamples.Length)
            {
                _sampleCount++;
                SignalDiagnostic = Max30100SignalDiagnostic.FillingAnalysisWindow;
            }

            _samplesSinceCalculation++;
            if (_sampleCount == _infraredSamples.Length && _samplesSinceCalculation >= _analysisIntervalSamples)
            {
                Calculate();
                _samplesSinceCalculation = 0;
            }
        }

        /// <summary>Clears buffered samples and all calculated values.</summary>
        public void Reset()
        {
            FingerDetected = false;
            _hasPreviousSample = false;
            _previousInfrared = 0;
            _previousRed = 0;
            _noFingerSampleCount = 0;
            RestartSignalCollection();
        }

        private static Max30100PulseOximeterOptions GetOptions(Max30100PulseOximeterOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException();
            }

            return options;
        }

        private static void ValidatePercentage(double value)
        {
            if (double.IsNaN(value) || value < 0 || value > 100)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        private static void ValidateQualityFactor(double value)
        {
            if (double.IsNaN(value) || value < 0 || value > 1)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        private static void ValidateSmoothingFactor(double value)
        {
            if (double.IsNaN(value) || value <= 0 || value > 1)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        private bool IsAbruptChange(ushort previous, ushort current)
        {
            int difference = current - previous;
            if (difference < 0)
            {
                difference = -difference;
            }

            int threshold = (previous * AbruptChangeRelativePercent) / 100;
            if (threshold < MinimumAbruptChangeCounts)
            {
                threshold = MinimumAbruptChangeCounts;
            }

            return difference > threshold;
        }

        private void RestartSignalCollection()
        {
            HasValidReading = false;
            HasValidHeartRate = false;
            IsHeartRateHeld = false;
            HasValidOxygenSaturation = false;
            HeartRate = 0;
            OxygenSaturation = 0;
            PerfusionIndex = 0;
            RedPerfusionIndex = 0;
            ChannelCorrelation = 0;
            RatioOfRatios = 0;
            SignalQuality = 0;
            InfraredAcRms = 0;
            BeatDetectionThreshold = 0;
            AutocorrelationHeartRate = 0;
            AutocorrelationQuality = 0;
            HeartRateEstimatorsDisagree = false;
            UsedAutocorrelationFallback = false;
            UsedWeakSignalOxygenSaturation = false;
            SignalDiagnostic = Max30100SignalDiagnostic.None;
            _weakSignalOxygenSaturationRatio = 0;
            _weakSignalOxygenSaturationCount = 0;
            _sampleCount = 0;
            _settlingSamplesRemaining = 0;
            _writeIndex = 0;
            _samplesSinceCalculation = 0;
            _candidateHeartRate = 0;
            _candidateMissCount = 0;
            _consistentHeartRateCount = 0;
            _alternateHeartRate = 0;
            _alternateHeartRateCount = 0;
            _detectedBeatCrossingCount = 0;
            _detectedIntervalCount = 0;
            _rawBeatIntervalCount = 0;
        }

        private ushort GetInfraredSample(int index)
        {
            int oldestIndex = _sampleCount == _infraredSamples.Length ? _writeIndex : 0;
            return _infraredSamples[(oldestIndex + index) % _infraredSamples.Length];
        }

        private ushort GetRedSample(int index)
        {
            int oldestIndex = _sampleCount == _redSamples.Length ? _writeIndex : 0;
            return _redSamples[(oldestIndex + index) % _redSamples.Length];
        }

        private void Calculate()
        {
            double infraredMean = 0;
            double redMean = 0;
            for (int index = 0; index < _sampleCount; index++)
            {
                infraredMean += GetInfraredSample(index);
                redMean += GetRedSample(index);
            }

            infraredMean /= _sampleCount;
            redMean /= _sampleCount;

            int filterStart = FilterSamples();
            int filteredSampleCount = _sampleCount - filterStart;
            RemoveLinearTrend(_filteredInfraredSamples, filterStart);
            RemoveLinearTrend(_filteredRedSamples, filterStart);
            double infraredSquared = 0;
            double redSquared = 0;
            double channelProduct = 0;
            for (int index = filterStart; index < _sampleCount; index++)
            {
                infraredSquared += _filteredInfraredSamples[index] * _filteredInfraredSamples[index];
                redSquared += _filteredRedSamples[index] * _filteredRedSamples[index];
                channelProduct += _filteredInfraredSamples[index] * _filteredRedSamples[index];
            }

            double infraredRms = Math.Sqrt(infraredSquared / filteredSampleCount);
            double redRms = Math.Sqrt(redSquared / filteredSampleCount);
            InfraredAcRms = infraredRms;
            PerfusionIndex = infraredRms * 100.0 / infraredMean;
            RedPerfusionIndex = redRms * 100.0 / redMean;
            double correlationDenominator = Math.Sqrt(infraredSquared * redSquared);
            ChannelCorrelation = correlationDenominator > 0 ? channelProduct / correlationDenominator : 0;

            double intervalHeartRate = CalculateHeartRate(filterStart, infraredRms);
            double intervalQuality = SignalQuality;
            AutocorrelationHeartRate = CalculateAutocorrelationHeartRate(filterStart);
            bool intervalEstimateValid = intervalHeartRate >= MinimumHeartRate
                && intervalHeartRate <= MaximumHeartRate
                && intervalQuality >= MinimumHeartRateQuality
                && _detectedIntervalCount >= MinimumBeatIntervalCount;
            bool autocorrelationEstimateValid = AutocorrelationHeartRate >= MinimumHeartRate
                && AutocorrelationHeartRate <= MaximumHeartRate
                && AutocorrelationQuality >= MinimumAutocorrelationQuality
                && ChannelCorrelation >= MinimumAutocorrelationChannelCorrelation;
            bool estimatorsAgree = false;
            HeartRateEstimatorsDisagree = false;
            UsedAutocorrelationFallback = false;
            double heartRate = 0;
            if (intervalEstimateValid && autocorrelationEstimateValid)
            {
                double difference = intervalHeartRate - AutocorrelationHeartRate;
                if (difference < 0)
                {
                    difference = -difference;
                }

                if (difference <= HeartRateEstimatorAgreementTolerance)
                {
                    estimatorsAgree = true;
                    heartRate = ((intervalHeartRate * intervalQuality)
                        + (AutocorrelationHeartRate * AutocorrelationQuality))
                        / (intervalQuality + AutocorrelationQuality);
                    SignalQuality = (intervalQuality + AutocorrelationQuality) / 2;
                }
                else
                {
                    HeartRateEstimatorsDisagree = true;
                    SignalQuality = (intervalQuality < AutocorrelationQuality ? intervalQuality : AutocorrelationQuality)
                        * EstimatorDisagreementQualityFactor;
                }
            }
            else if (intervalEstimateValid)
            {
                heartRate = intervalHeartRate;
            }
            else if (autocorrelationEstimateValid)
            {
                heartRate = AutocorrelationHeartRate;
                SignalQuality = AutocorrelationQuality * AutocorrelationFallbackQualityFactor;
                UsedAutocorrelationFallback = true;
            }

            double ratio = (redRms / redMean) / (infraredRms / infraredMean);
            RatioOfRatios = ratio;
            double oxygenSaturation = OxygenSaturationCalibrationIntercept
                + (OxygenSaturationCalibrationSlope * ratio);
            if (oxygenSaturation > 100)
            {
                oxygenSaturation = 100;
            }
            else if (oxygenSaturation < 0)
            {
                oxygenSaturation = 0;
            }

            bool hadValidHeartRate = HasValidHeartRate;
            bool hadValidOxygenSaturation = HasValidOxygenSaturation;
            bool heartRateCandidateValid = infraredRms >= MinimumAcRms
                && PerfusionIndex >= MinimumHeartRatePerfusionIndex
                && SignalQuality >= MinimumHeartRateQuality
                && heartRate >= MinimumHeartRate
                && heartRate <= MaximumHeartRate;
            bool strongCandidate = estimatorsAgree
                && SignalQuality >= StrongCandidateQuality
                && _detectedIntervalCount >= MinimumBeatIntervalCount;
            bool stableOpticalSignal = infraredRms >= MinimumAcRms
                && PerfusionIndex >= MinimumHeartRatePerfusionIndex
                && ChannelCorrelation >= MinimumOxygenSaturationCorrelation;
            UpdateHeartRateValidity(heartRate, heartRateCandidateValid, strongCandidate, stableOpticalSignal);
            UpdateSignalDiagnostic(infraredRms, heartRate, heartRateCandidateValid);
            bool ratioInRange = ratio >= MinimumOxygenSaturationRatio
                && ratio <= MaximumOxygenSaturationRatio;
            bool strongOxygenSaturationSignal = redRms >= MinimumAcRms
                && PerfusionIndex >= MinimumOxygenSaturationPerfusionIndex
                && RedPerfusionIndex >= MinimumOxygenSaturationPerfusionIndex
                && ChannelCorrelation >= MinimumOxygenSaturationCorrelation
                && ratioInRange;
            bool weakOxygenSaturationSignal = redRms >= MinimumAcRms
                && PerfusionIndex >= MinimumWeakSignalOxygenSaturationPerfusionIndex
                && RedPerfusionIndex >= MinimumWeakSignalOxygenSaturationPerfusionIndex
                && ChannelCorrelation >= MinimumWeakSignalOxygenSaturationCorrelation
                && ratioInRange;
            UpdateWeakSignalOxygenSaturation(ratio, weakOxygenSaturationSignal);
            UsedWeakSignalOxygenSaturation = HasValidHeartRate
                && !strongOxygenSaturationSignal
                && _weakSignalOxygenSaturationCount >= WeakSignalOxygenSaturationConfirmationCount;
            HasValidOxygenSaturation = HasValidHeartRate
                && (strongOxygenSaturationSignal || UsedWeakSignalOxygenSaturation);
            HasValidReading = HasValidHeartRate && HasValidOxygenSaturation;

            if (!HasValidHeartRate)
            {
                HeartRate = 0;
            }
            else if (hadValidHeartRate)
            {
                HeartRate = (HeartRate * (1 - HeartRateSmoothingFactor))
                    + (_candidateHeartRate * HeartRateSmoothingFactor);
            }
            else
            {
                HeartRate = _candidateHeartRate;
            }

            if (!HasValidOxygenSaturation)
            {
                OxygenSaturation = 0;
            }
            else if (hadValidOxygenSaturation)
            {
                OxygenSaturation = (OxygenSaturation * (1 - OxygenSaturationSmoothingFactor))
                    + (oxygenSaturation * OxygenSaturationSmoothingFactor);
            }
            else
            {
                OxygenSaturation = oxygenSaturation;
            }
        }

        private void UpdateSignalDiagnostic(double infraredRms, double heartRate, bool candidateValid)
        {
            if (HasValidHeartRate)
            {
                SignalDiagnostic = Max30100SignalDiagnostic.None;
            }
            else if (infraredRms < MinimumAcRms)
            {
                SignalDiagnostic = Max30100SignalDiagnostic.InsufficientAcAmplitude;
            }
            else if (PerfusionIndex < MinimumHeartRatePerfusionIndex)
            {
                SignalDiagnostic = Max30100SignalDiagnostic.InsufficientPerfusion;
            }
            else if (HeartRateEstimatorsDisagree)
            {
                SignalDiagnostic = Max30100SignalDiagnostic.EstimatorsDisagree;
            }
            else if (UsedAutocorrelationFallback && candidateValid)
            {
                SignalDiagnostic = Max30100SignalDiagnostic.AutocorrelationFallbackPending;
            }
            else if (_detectedBeatCrossingCount < MinimumBeatIntervalCount)
            {
                SignalDiagnostic = Max30100SignalDiagnostic.TooFewBeatCrossings;
            }
            else if (_rawBeatIntervalCount < MinimumBeatIntervalCount)
            {
                SignalDiagnostic = Max30100SignalDiagnostic.TooFewBeatIntervals;
            }
            else if (_detectedIntervalCount < MinimumBeatIntervalCount)
            {
                SignalDiagnostic = Max30100SignalDiagnostic.InconsistentBeatIntervals;
            }
            else if (SignalQuality < MinimumHeartRateQuality)
            {
                SignalDiagnostic = Max30100SignalDiagnostic.LowSignalQuality;
            }
            else if (heartRate < MinimumHeartRate || heartRate > MaximumHeartRate)
            {
                SignalDiagnostic = Max30100SignalDiagnostic.NoHeartRateEstimate;
            }
            else
            {
                SignalDiagnostic = Max30100SignalDiagnostic.CandidateConfirmationPending;
            }
        }

        private int FilterSamples()
        {
            double baselineAlpha = BaselineTrackingRate / _sampleRate;
            double smoothingAlpha = SignalSmoothingRate / _sampleRate;
            if (smoothingAlpha > 1)
            {
                smoothingAlpha = 1;
            }

            double infraredBaseline = GetInfraredSample(0);
            double redBaseline = GetRedSample(0);
            double filteredInfrared = 0;
            double filteredRed = 0;
            for (int index = 0; index < _sampleCount; index++)
            {
                ushort infrared = GetInfraredSample(index);
                ushort red = GetRedSample(index);
                infraredBaseline += baselineAlpha * (infrared - infraredBaseline);
                redBaseline += baselineAlpha * (red - redBaseline);
                filteredInfrared += smoothingAlpha * ((infrared - infraredBaseline) - filteredInfrared);
                filteredRed += smoothingAlpha * ((red - redBaseline) - filteredRed);
                _filteredInfraredSamples[index] = filteredInfrared;
                _filteredRedSamples[index] = filteredRed;
            }

            return (int)(_sampleRate * FilterSettlingSeconds);
        }

        private void RemoveLinearTrend(double[] samples, int startIndex)
        {
            int count = _sampleCount - startIndex;
            double xSum = 0;
            double ySum = 0;
            double xxSum = 0;
            double xySum = 0;
            for (int index = 0; index < count; index++)
            {
                double value = samples[startIndex + index];
                xSum += index;
                ySum += value;
                xxSum += index * index;
                xySum += index * value;
            }

            double denominator = (count * xxSum) - (xSum * xSum);
            if (denominator == 0)
            {
                return;
            }

            double slope = ((count * xySum) - (xSum * ySum)) / denominator;
            double intercept = (ySum - (slope * xSum)) / count;
            for (int index = 0; index < count; index++)
            {
                samples[startIndex + index] -= intercept + (slope * index);
            }
        }

        private void UpdateWeakSignalOxygenSaturation(double ratio, bool signalValid)
        {
            if (!signalValid)
            {
                _weakSignalOxygenSaturationRatio = 0;
                _weakSignalOxygenSaturationCount = 0;
                return;
            }

            if (_weakSignalOxygenSaturationCount == 0)
            {
                _weakSignalOxygenSaturationRatio = ratio;
                _weakSignalOxygenSaturationCount = 1;
                return;
            }

            double difference = ratio - _weakSignalOxygenSaturationRatio;
            if (difference < 0)
            {
                difference = -difference;
            }

            if (difference > WeakSignalOxygenSaturationRatioTolerance)
            {
                _weakSignalOxygenSaturationRatio = ratio;
                _weakSignalOxygenSaturationCount = 1;
                return;
            }

            _weakSignalOxygenSaturationRatio = (_weakSignalOxygenSaturationRatio + ratio) / 2;
            if (_weakSignalOxygenSaturationCount < WeakSignalOxygenSaturationConfirmationCount)
            {
                _weakSignalOxygenSaturationCount++;
            }
        }

        private void UpdateHeartRateValidity(
            double heartRate,
            bool candidateValid,
            bool strongCandidate,
            bool stableOpticalSignal)
        {
            bool wasValid = HasValidHeartRate;
            IsHeartRateHeld = false;
            if (!candidateValid)
            {
                _alternateHeartRate = 0;
                _alternateHeartRateCount = 0;
                _candidateMissCount++;
                int missLimit = stableOpticalSignal
                    ? StableOpticalSignalHeartRateMissLimit
                    : HeartRateCandidateMissLimit;
                if (wasValid && _candidateMissCount < missLimit)
                {
                    HasValidHeartRate = true;
                    IsHeartRateHeld = true;
                    return;
                }

                HasValidHeartRate = false;
                if (_candidateMissCount >= missLimit)
                {
                    _candidateHeartRate = 0;
                    _consistentHeartRateCount = 0;
                }

                return;
            }

            double difference = heartRate - _candidateHeartRate;
            if (difference < 0)
            {
                difference = -difference;
            }

            if (_candidateHeartRate == 0)
            {
                _candidateHeartRate = heartRate;
                _candidateMissCount = 0;
                _consistentHeartRateCount = strongCandidate ? HeartRateConfirmationCount : 1;
                HasValidHeartRate = _consistentHeartRateCount >= HeartRateConfirmationCount;
                return;
            }

            // A jump above the configured tolerance must repeat before replacing the established rate.
            if (difference > HeartRateCandidateTolerance)
            {
                HasValidHeartRate = wasValid;
                IsHeartRateHeld = wasValid;
                _candidateMissCount = 0;
                double alternateDifference = heartRate - _alternateHeartRate;
                if (alternateDifference < 0)
                {
                    alternateDifference = -alternateDifference;
                }

                if (_alternateHeartRate == 0 || alternateDifference > HeartRateCandidateTolerance)
                {
                    _alternateHeartRate = heartRate;
                    _alternateHeartRateCount = 1;
                }
                else
                {
                    _alternateHeartRate = (_alternateHeartRate * (1 - HeartRateCandidateSmoothingFactor))
                        + (heartRate * HeartRateCandidateSmoothingFactor);
                    _alternateHeartRateCount++;
                }

                if (_alternateHeartRateCount >= AlternateHeartRateConfirmationCount)
                {
                    _candidateHeartRate = _alternateHeartRate;
                    _consistentHeartRateCount = strongCandidate ? HeartRateConfirmationCount : 1;
                    HasValidHeartRate = _consistentHeartRateCount >= HeartRateConfirmationCount;
                    IsHeartRateHeld = false;
                    _alternateHeartRate = 0;
                    _alternateHeartRateCount = 0;
                }

                return;
            }

            _candidateMissCount = 0;
            _alternateHeartRate = 0;
            _alternateHeartRateCount = 0;
            _candidateHeartRate = (_candidateHeartRate * (1 - HeartRateCandidateSmoothingFactor))
                + (heartRate * HeartRateCandidateSmoothingFactor);
            _consistentHeartRateCount++;
            HasValidHeartRate = _consistentHeartRateCount >= HeartRateConfirmationCount;
        }

        private double CalculateHeartRate(int filterStart, double infraredRms)
        {
            _detectedBeatCrossingCount = 0;
            _detectedIntervalCount = 0;
            _rawBeatIntervalCount = 0;
            double threshold = infraredRms * BeatDetectionThresholdFraction;
            if (threshold < MinimumBeatDetectionThreshold)
            {
                threshold = MinimumBeatDetectionThreshold;
            }

            BeatDetectionThreshold = threshold;
            int minimumInterval = (int)((_sampleRate * 60) / MaximumHeartRate);
            int maximumInterval = (int)((_sampleRate * 60) / MinimumHeartRate);
            int previousBeat = -1;
            int intervalCount = 0;
            bool armed = false;

            for (int index = filterStart; index < _sampleCount; index++)
            {
                double signal = _filteredInfraredSamples[index];
                if (signal <= -threshold)
                {
                    armed = true;
                    continue;
                }

                if (!armed || signal < threshold)
                {
                    continue;
                }

                armed = false;
                _detectedBeatCrossingCount++;
                if (previousBeat >= 0)
                {
                    int interval = index - previousBeat;
                    if (interval < minimumInterval)
                    {
                        continue;
                    }

                    if (interval <= maximumInterval)
                    {
                        if (intervalCount < _beatIntervals.Length)
                        {
                            _beatIntervals[intervalCount] = interval;
                            intervalCount++;
                        }
                    }
                }

                previousBeat = index;
            }

            _rawBeatIntervalCount = intervalCount;
            if (intervalCount < 2)
            {
                SignalQuality = 0;
                return 0;
            }

            SortBeatIntervals(intervalCount);
            double medianInterval = _beatIntervals[intervalCount / 2];
            double intervalSum = 0;
            double intervalSquaredSum = 0;
            int acceptedIntervalCount = 0;
            for (int index = 0; index < intervalCount; index++)
            {
                double interval = _beatIntervals[index];
                if (interval > medianInterval * MissedBeatIntervalFactor)
                {
                    interval /= 2;
                }

                double difference = interval - medianInterval;
                if (difference < 0)
                {
                    difference = -difference;
                }

                if (difference <= medianInterval * BeatIntervalToleranceFraction)
                {
                    intervalSum += interval;
                    intervalSquaredSum += interval * interval;
                    acceptedIntervalCount++;
                }
            }

            if (acceptedIntervalCount < 2)
            {
                SignalQuality = 0;
                return 0;
            }

            _detectedIntervalCount = acceptedIntervalCount;
            double meanInterval = intervalSum / acceptedIntervalCount;
            double intervalVariance = (intervalSquaredSum / acceptedIntervalCount) - (meanInterval * meanInterval);
            if (intervalVariance < 0)
            {
                intervalVariance = 0;
            }

            double intervalDeviation = Math.Sqrt(intervalVariance);
            SignalQuality = 100 - ((intervalDeviation / meanInterval) * IntervalDeviationQualityPenalty);
            if (SignalQuality < 0)
            {
                SignalQuality = 0;
            }
            else if (SignalQuality > 100)
            {
                SignalQuality = 100;
            }

            double intervalEvidence = (double)acceptedIntervalCount / MinimumBeatIntervalCount;
            if (intervalEvidence < 1)
            {
                SignalQuality *= intervalEvidence;
            }

            SignalQuality *= (double)acceptedIntervalCount / intervalCount;

            return 60.0 * _sampleRate / meanInterval;
        }

        private double CalculateAutocorrelationHeartRate(int filterStart)
        {
            AutocorrelationQuality = 0;
            double autocorrelationSampleRate = (double)_sampleRate / _autocorrelationDecimation;
            int minimumLag = (int)((autocorrelationSampleRate * 60) / MaximumHeartRate);
            int maximumLag = (int)((autocorrelationSampleRate * 60) / MinimumHeartRate);
            int correlationStart = filterStart + (maximumLag * _autocorrelationDecimation);
            if (correlationStart >= _sampleCount - 1)
            {
                return 0;
            }

            for (int lag = minimumLag; lag <= maximumLag; lag++)
            {
                double productSum = 0;
                double currentSquaredSum = 0;
                double delayedSquaredSum = 0;
                int sampleOffset = lag * _autocorrelationDecimation;
                for (int index = correlationStart;
                    index < _sampleCount;
                    index += _autocorrelationDecimation)
                {
                    double current = _filteredInfraredSamples[index];
                    double delayed = _filteredInfraredSamples[index - sampleOffset];
                    productSum += current * delayed;
                    currentSquaredSum += current * current;
                    delayedSquaredSum += delayed * delayed;
                }

                double denominator = Math.Sqrt(currentSquaredSum * delayedSquaredSum);
                _autocorrelation[lag] = denominator > 0 ? productSum / denominator : 0;
            }

            int bestLag = 0;
            double bestCorrelation = 0;
            for (int lag = minimumLag + 1; lag < maximumLag; lag++)
            {
                double correlation = _autocorrelation[lag];
                if (correlation > bestCorrelation
                    && correlation >= _autocorrelation[lag - 1]
                    && correlation > _autocorrelation[lag + 1])
                {
                    bestCorrelation = correlation;
                    bestLag = lag;
                }
            }

            if (bestLag == 0 || bestCorrelation <= 0)
            {
                return 0;
            }

            int halfLag = bestLag / 2;
            if (halfLag >= minimumLag + 1
                && halfLag < maximumLag
                && _autocorrelation[halfLag] >= bestCorrelation * AutocorrelationHarmonicThreshold
                && _autocorrelation[halfLag] >= _autocorrelation[halfLag - 1]
                && _autocorrelation[halfLag] > _autocorrelation[halfLag + 1])
            {
                bestLag = halfLag;
                bestCorrelation = _autocorrelation[bestLag];
            }

            int shoulderOffset = (int)(bestLag * AutocorrelationShoulderFraction);
            if (shoulderOffset < 1)
            {
                shoulderOffset = 1;
            }

            int lowerShoulder = bestLag - shoulderOffset;
            if (lowerShoulder < minimumLag)
            {
                lowerShoulder = minimumLag;
            }

            int upperShoulder = bestLag + shoulderOffset;
            if (upperShoulder > maximumLag)
            {
                upperShoulder = maximumLag;
            }

            double shoulderCorrelation = _autocorrelation[lowerShoulder];
            if (_autocorrelation[upperShoulder] > shoulderCorrelation)
            {
                shoulderCorrelation = _autocorrelation[upperShoulder];
            }

            double prominence = bestCorrelation - shoulderCorrelation;
            if (prominence <= 0)
            {
                return 0;
            }

            double prominenceEvidence = prominence / AutocorrelationProminenceForFullQuality;
            if (prominenceEvidence > 1)
            {
                prominenceEvidence = 1;
            }

            double cycleEvidence = (double)(_sampleCount - filterStart)
                / (bestLag * _autocorrelationDecimation)
                / AutocorrelationCyclesForFullQuality;
            if (cycleEvidence > 1)
            {
                cycleEvidence = 1;
            }

            AutocorrelationQuality = bestCorrelation * prominenceEvidence * cycleEvidence * 100;
            if (AutocorrelationQuality > 100)
            {
                AutocorrelationQuality = 100;
            }

            double interpolatedLag = bestLag;
            double interpolationDenominator = _autocorrelation[bestLag - 1]
                - (2 * bestCorrelation)
                + _autocorrelation[bestLag + 1];
            if (interpolationDenominator != 0)
            {
                double lagOffset = 0.5
                    * (_autocorrelation[bestLag - 1] - _autocorrelation[bestLag + 1])
                    / interpolationDenominator;
                if (lagOffset > 0.5)
                {
                    lagOffset = 0.5;
                }
                else if (lagOffset < -0.5)
                {
                    lagOffset = -0.5;
                }

                interpolatedLag += lagOffset;
            }

            return 60.0 * autocorrelationSampleRate / interpolatedLag;
        }

        private void SortBeatIntervals(int count)
        {
            for (int index = 1; index < count; index++)
            {
                int value = _beatIntervals[index];
                int position = index - 1;
                while (position >= 0 && _beatIntervals[position] > value)
                {
                    _beatIntervals[position + 1] = _beatIntervals[position];
                    position--;
                }

                _beatIntervals[position + 1] = value;
            }
        }

        internal static void CalculateOxygenSaturationCalibration(
            double[] ratios,
            double[] referenceOxygenSaturations,
            out double intercept,
            out double slope)
        {
            if (ratios == null)
            {
                throw new ArgumentNullException();
            }

            if (referenceOxygenSaturations == null)
            {
                throw new ArgumentNullException();
            }

            if (ratios.Length < MinimumCalibrationPointCount || ratios.Length != referenceOxygenSaturations.Length)
            {
                throw new ArgumentException();
            }

            double ratioSum = 0;
            double oxygenSaturationSum = 0;
            double ratioSquaredSum = 0;
            double ratioOxygenSaturationSum = 0;
            for (int index = 0; index < ratios.Length; index++)
            {
                double ratio = ratios[index];
                double oxygenSaturation = referenceOxygenSaturations[index];
                if (double.IsNaN(ratio) || ratio <= 0 || ratio > MaximumCalibrationRatio)
                {
                    throw new ArgumentOutOfRangeException();
                }

                if (double.IsNaN(oxygenSaturation)
                    || oxygenSaturation < MinimumReferenceOxygenSaturation
                    || oxygenSaturation > MaximumReferenceOxygenSaturation)
                {
                    throw new ArgumentOutOfRangeException();
                }

                ratioSum += ratio;
                oxygenSaturationSum += oxygenSaturation;
                ratioSquaredSum += ratio * ratio;
                ratioOxygenSaturationSum += ratio * oxygenSaturation;
            }

            double pointCount = ratios.Length;
            double denominator = (pointCount * ratioSquaredSum) - (ratioSum * ratioSum);
            if (denominator <= 0)
            {
                throw new ArgumentException();
            }

            slope = ((pointCount * ratioOxygenSaturationSum) - (ratioSum * oxygenSaturationSum)) / denominator;
            intercept = (oxygenSaturationSum - (slope * ratioSum)) / pointCount;
            if (slope >= 0)
            {
                throw new ArgumentException();
            }

            ValidateOxygenSaturationCalibration(intercept, slope);
        }

        internal static void ValidateOxygenSaturationCalibration(double intercept, double slope)
        {
            if (double.IsNaN(intercept)
                || intercept < MinimumCalibrationIntercept
                || intercept > MaximumCalibrationIntercept)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (double.IsNaN(slope) || slope < MinimumCalibrationSlope || slope >= MaximumCalibrationSlope)
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }
}
