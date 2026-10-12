// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;

namespace Iot.Device.Max30100
{
    /// <summary>
    /// Processes MAX30100 samples on a background thread.
    /// </summary>
    public sealed class Max30100PulseOximeterWorker : IDisposable
    {
        /// <summary>The default processing-queue duration.</summary>
        public const int DefaultQueueSeconds = 2;

        /// <summary>The default state publication rate.</summary>
        public const int DefaultPublishRate = 10;

        private const int MinimumQueueSeconds = 1;
        private const int MaximumQueueSeconds = 4;
        private const int MinimumPublishRate = 1;
        private readonly AutoResetEvent _sampleAvailable = new AutoResetEvent(false);
        private readonly object _queueLock = new object();
        private readonly object _stateLock = new object();
        private readonly Max30100PulseOximeter _processor;
        private readonly Max30100Reading[] _queue;
        private readonly Thread _workerThread;
        private readonly int _publishInterval;
        private readonly int _publishRate;
        private bool _fingerDetected;
        private bool _hasValidHeartRate;
        private bool _hasValidOxygenSaturation;
        private bool _hasValidReading;
        private bool _isSettling;
        private bool _isHeartRateHeld;
        private bool _calibrationRequested;
        private bool _resetRequested;
        private bool _stopping;
        private double _candidateHeartRate;
        private double _analysisWindowProgress;
        private double _heartRate;
        private double _oxygenSaturation;
        private double _perfusionIndex;
        private double _redPerfusionIndex;
        private double _channelCorrelation;
        private double _ratioOfRatios;
        private double _oxygenSaturationCalibrationIntercept;
        private double _oxygenSaturationCalibrationSlope;
        private double _requestedCalibrationIntercept;
        private double _requestedCalibrationSlope;
        private double _signalQuality;
        private double _infraredAcRms;
        private double _beatDetectionThreshold;
        private double _autocorrelationHeartRate;
        private double _autocorrelationQuality;
        private bool _heartRateEstimatorsDisagree;
        private bool _usedAutocorrelationFallback;
        private bool _usedWeakSignalOxygenSaturation;
        private int _detectedBeatCrossings;
        private int _detectedBeatIntervals;
        private int _rawBeatIntervals;
        private int _queueCount;
        private int _queueReadIndex;
        private int _queueWriteIndex;
        private uint _droppedSampleCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="Max30100PulseOximeterWorker"/> class
        /// with the default signal-processing and worker configuration.
        /// </summary>
        public Max30100PulseOximeterWorker()
            : this(new Max30100PulseOximeterOptions(), DefaultQueueSeconds, DefaultPublishRate)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Max30100PulseOximeterWorker"/> class.
        /// </summary>
        /// <param name="options">The signal-processing configuration.</param>
        /// <param name="queueSeconds">The processing-queue capacity in seconds of sensor samples.</param>
        /// <param name="publishRate">The number of state snapshots published per second.</param>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A worker or processing option is outside its supported range.</exception>
        /// <exception cref="ArgumentException">Two or more processing options are incompatible.</exception>
        public Max30100PulseOximeterWorker(
            Max30100PulseOximeterOptions options,
            int queueSeconds = DefaultQueueSeconds,
            int publishRate = DefaultPublishRate)
            : this(
                GetOptions(options).SampleRate,
                options.WindowSeconds,
                queueSeconds,
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
                options.StableOpticalSignalHeartRateMissLimit,
                publishRate)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Max30100PulseOximeterWorker"/> class.
        /// </summary>
        /// <param name="sampleRate">The configured sensor sample rate in samples per second.</param>
        /// <param name="windowSeconds">The analysis-window length in seconds.</param>
        /// <param name="queueSeconds">The number of seconds of pending samples that can be buffered.</param>
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
        /// <param name="publishRate">The number of state snapshots published per second.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="queueSeconds"/> is outside 1 through 4, or a processor argument is outside its documented range.
        /// </exception>
        private Max30100PulseOximeterWorker(
            int sampleRate = Max30100PulseOximeter.DefaultSampleRate,
            int windowSeconds = Max30100PulseOximeter.DefaultWindowSeconds,
            int queueSeconds = DefaultQueueSeconds,
            int abruptChangeRelativePercent = Max30100PulseOximeter.DefaultAbruptChangeRelativePercent,
            int minimumAbruptChangeCounts = Max30100PulseOximeter.DefaultMinimumAbruptChangeCounts,
            double heartRateCandidateTolerance = Max30100PulseOximeter.DefaultHeartRateCandidateTolerance,
            int heartRateCandidateMissLimit = Max30100PulseOximeter.DefaultHeartRateCandidateMissLimit,
            int fingerDetectionThreshold = Max30100PulseOximeter.DefaultFingerDetectionThreshold,
            double minimumHeartRate = Max30100PulseOximeter.DefaultMinimumHeartRate,
            double maximumHeartRate = Max30100PulseOximeter.DefaultMaximumHeartRate,
            double heartRateEstimatorAgreementTolerance = Max30100PulseOximeter.DefaultHeartRateEstimatorAgreementTolerance,
            double minimumHeartRateQuality = Max30100PulseOximeter.DefaultMinimumHeartRateQuality,
            double minimumAutocorrelationQuality = Max30100PulseOximeter.DefaultMinimumAutocorrelationQuality,
            int minimumBeatIntervalCount = Max30100PulseOximeter.DefaultMinimumBeatIntervalCount,
            double strongCandidateQuality = Max30100PulseOximeter.DefaultStrongCandidateQuality,
            double minimumHeartRatePerfusionIndex = Max30100PulseOximeter.DefaultMinimumHeartRatePerfusionIndex,
            double minimumOxygenSaturationPerfusionIndex = Max30100PulseOximeter.DefaultMinimumOxygenSaturationPerfusionIndex,
            double minimumOxygenSaturationCorrelation = Max30100PulseOximeter.DefaultMinimumOxygenSaturationCorrelation,
            double minimumOxygenSaturationRatio = Max30100PulseOximeter.DefaultMinimumOxygenSaturationRatio,
            double maximumOxygenSaturationRatio = Max30100PulseOximeter.DefaultMaximumOxygenSaturationRatio,
            double baselineTrackingRate = Max30100PulseOximeter.DefaultBaselineTrackingRate,
            double signalSmoothingRate = Max30100PulseOximeter.DefaultSignalSmoothingRate,
            double filterSettlingSeconds = Max30100PulseOximeter.DefaultFilterSettlingSeconds,
            double beatDetectionThresholdFraction = Max30100PulseOximeter.DefaultBeatDetectionThresholdFraction,
            double minimumBeatDetectionThreshold = Max30100PulseOximeter.DefaultMinimumBeatDetectionThreshold,
            double minimumAcRms = Max30100PulseOximeter.DefaultMinimumAcRms,
            double missedBeatIntervalFactor = Max30100PulseOximeter.DefaultMissedBeatIntervalFactor,
            double beatIntervalToleranceFraction = Max30100PulseOximeter.DefaultBeatIntervalToleranceFraction,
            int fingerLossSignalResetSamples = Max30100PulseOximeter.DefaultFingerLossSignalResetSamples,
            int fingerRemovalDebounceSamples = Max30100PulseOximeter.DefaultFingerRemovalDebounceSamples,
            double initialFingerSettlingSeconds = Max30100PulseOximeter.DefaultInitialFingerSettlingSeconds,
            double motionRecoverySettlingSeconds = Max30100PulseOximeter.DefaultMotionRecoverySettlingSeconds,
            double analysisIntervalSeconds = Max30100PulseOximeter.DefaultAnalysisIntervalSeconds,
            int heartRateConfirmationCount = Max30100PulseOximeter.DefaultHeartRateConfirmationCount,
            int alternateHeartRateConfirmationCount = Max30100PulseOximeter.DefaultAlternateHeartRateConfirmationCount,
            double heartRateCandidateSmoothingFactor = Max30100PulseOximeter.DefaultHeartRateCandidateSmoothingFactor,
            double heartRateSmoothingFactor = Max30100PulseOximeter.DefaultHeartRateSmoothingFactor,
            double oxygenSaturationSmoothingFactor = Max30100PulseOximeter.DefaultOxygenSaturationSmoothingFactor,
            double minimumAutocorrelationChannelCorrelation = Max30100PulseOximeter.DefaultMinimumAutocorrelationChannelCorrelation,
            double estimatorDisagreementQualityFactor = Max30100PulseOximeter.DefaultEstimatorDisagreementQualityFactor,
            double autocorrelationFallbackQualityFactor = Max30100PulseOximeter.DefaultAutocorrelationFallbackQualityFactor,
            double intervalDeviationQualityPenalty = Max30100PulseOximeter.DefaultIntervalDeviationQualityPenalty,
            int autocorrelationTargetSampleRate = Max30100PulseOximeter.DefaultAutocorrelationTargetSampleRate,
            double autocorrelationHarmonicThreshold = Max30100PulseOximeter.DefaultAutocorrelationHarmonicThreshold,
            double autocorrelationShoulderFraction = Max30100PulseOximeter.DefaultAutocorrelationShoulderFraction,
            double autocorrelationProminenceForFullQuality = Max30100PulseOximeter.DefaultAutocorrelationProminenceForFullQuality,
            double autocorrelationCyclesForFullQuality = Max30100PulseOximeter.DefaultAutocorrelationCyclesForFullQuality,
            double minimumWeakSignalOxygenSaturationPerfusionIndex = Max30100PulseOximeter.DefaultMinimumWeakSignalOxygenSaturationPerfusionIndex,
            double minimumWeakSignalOxygenSaturationCorrelation = Max30100PulseOximeter.DefaultMinimumWeakSignalOxygenSaturationCorrelation,
            double weakSignalOxygenSaturationRatioTolerance = Max30100PulseOximeter.DefaultWeakSignalOxygenSaturationRatioTolerance,
            int weakSignalOxygenSaturationConfirmationCount = Max30100PulseOximeter.DefaultWeakSignalOxygenSaturationConfirmationCount,
            int stableOpticalSignalHeartRateMissLimit = Max30100PulseOximeter.DefaultStableOpticalSignalHeartRateMissLimit,
            int publishRate = DefaultPublishRate)
        {
            if (queueSeconds < MinimumQueueSeconds || queueSeconds > MaximumQueueSeconds)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (publishRate < MinimumPublishRate || publishRate > sampleRate)
            {
                throw new ArgumentOutOfRangeException();
            }

            _processor = new Max30100PulseOximeter(
                sampleRate,
                windowSeconds,
                abruptChangeRelativePercent,
                minimumAbruptChangeCounts,
                heartRateCandidateTolerance,
                heartRateCandidateMissLimit,
                fingerDetectionThreshold,
                minimumHeartRate,
                maximumHeartRate,
                heartRateEstimatorAgreementTolerance,
                minimumHeartRateQuality,
                minimumAutocorrelationQuality,
                minimumBeatIntervalCount,
                strongCandidateQuality,
                minimumHeartRatePerfusionIndex,
                minimumOxygenSaturationPerfusionIndex,
                minimumOxygenSaturationCorrelation,
                minimumOxygenSaturationRatio,
                maximumOxygenSaturationRatio,
                baselineTrackingRate,
                signalSmoothingRate,
                filterSettlingSeconds,
                beatDetectionThresholdFraction,
                minimumBeatDetectionThreshold,
                minimumAcRms,
                missedBeatIntervalFactor,
                beatIntervalToleranceFraction,
                fingerLossSignalResetSamples,
                fingerRemovalDebounceSamples,
                initialFingerSettlingSeconds,
                motionRecoverySettlingSeconds,
                analysisIntervalSeconds,
                heartRateConfirmationCount,
                alternateHeartRateConfirmationCount,
                heartRateCandidateSmoothingFactor,
                heartRateSmoothingFactor,
                oxygenSaturationSmoothingFactor,
                minimumAutocorrelationChannelCorrelation,
                estimatorDisagreementQualityFactor,
                autocorrelationFallbackQualityFactor,
                intervalDeviationQualityPenalty,
                autocorrelationTargetSampleRate,
                autocorrelationHarmonicThreshold,
                autocorrelationShoulderFraction,
                autocorrelationProminenceForFullQuality,
                autocorrelationCyclesForFullQuality,
                minimumWeakSignalOxygenSaturationPerfusionIndex,
                minimumWeakSignalOxygenSaturationCorrelation,
                weakSignalOxygenSaturationRatioTolerance,
                weakSignalOxygenSaturationConfirmationCount,
                stableOpticalSignalHeartRateMissLimit);
            _oxygenSaturationCalibrationIntercept = _processor.OxygenSaturationCalibrationIntercept;
            _oxygenSaturationCalibrationSlope = _processor.OxygenSaturationCalibrationSlope;
            _queue = new Max30100Reading[sampleRate * queueSeconds];
            _publishRate = publishRate;
            _publishInterval = (sampleRate + publishRate - 1) / publishRate;

            _workerThread = new Thread(ProcessSamples);
            _workerThread.Start();
        }

        /// <summary>Gets the requested state publication rate.</summary>
        /// <value>The requested number of state snapshots per second.</value>
        public int PublishRate => _publishRate;

        /// <summary>Gets a value indicating whether a finger is currently detected.</summary>
        /// <value>True when the current optical levels indicate finger contact.</value>
        public bool FingerDetected
        {
            get
            {
                lock (_stateLock)
                {
                    return _fingerDetected;
                }
            }
        }

        /// <summary>Gets a value indicating whether finger contact is still stabilizing.</summary>
        /// <value>True while initial pressure and optical-level changes are being discarded.</value>
        public bool IsSettling
        {
            get
            {
                lock (_stateLock)
                {
                    return _isSettling;
                }
            }
        }

        /// <summary>Gets the percentage of the analysis window currently filled with stable samples.</summary>
        /// <value>A value from zero through 100.</value>
        public double AnalysisWindowProgress
        {
            get
            {
                lock (_stateLock)
                {
                    return _analysisWindowProgress;
                }
            }
        }

        /// <summary>Gets a value indicating whether pulse and oxygen-saturation estimates are available.</summary>
        /// <value>True when both estimates passed their signal checks.</value>
        public bool HasValidReading
        {
            get
            {
                lock (_stateLock)
                {
                    return _hasValidReading;
                }
            }
        }

        /// <summary>Gets a value indicating whether the pulse-rate estimate is available.</summary>
        /// <value>True when the infrared waveform has a sufficiently periodic pulse signal.</value>
        public bool HasValidHeartRate
        {
            get
            {
                lock (_stateLock)
                {
                    return _hasValidHeartRate;
                }
            }
        }

        /// <summary>Gets a value indicating whether the last confirmed heart rate is being held through a weak analysis window.</summary>
        /// <value>True when the displayed heart rate was retained rather than refreshed by the latest window.</value>
        public bool IsHeartRateHeld
        {
            get
            {
                lock (_stateLock)
                {
                    return _isHeartRateHeld;
                }
            }
        }

        /// <summary>Gets a value indicating whether the oxygen-saturation estimate is available.</summary>
        /// <value>True when both optical channels pass the ratio-of-ratios signal checks.</value>
        public bool HasValidOxygenSaturation
        {
            get
            {
                lock (_stateLock)
                {
                    return _hasValidOxygenSaturation;
                }
            }
        }

        /// <summary>Gets the current unconfirmed pulse-rate candidate.</summary>
        /// <value>The candidate rate in beats per minute, or zero when no candidate is available.</value>
        public double CandidateHeartRate
        {
            get
            {
                lock (_stateLock)
                {
                    return _candidateHeartRate;
                }
            }
        }

        /// <summary>Gets the number of beat intervals accepted during the latest analysis.</summary>
        /// <value>The number of accepted beat intervals.</value>
        public int DetectedBeatIntervals
        {
            get
            {
                lock (_stateLock)
                {
                    return _detectedBeatIntervals;
                }
            }
        }

        /// <summary>Gets the number of positive beat-threshold crossings found during the latest analysis.</summary>
        /// <value>The number of detected threshold crossings.</value>
        public int DetectedBeatCrossings
        {
            get
            {
                lock (_stateLock)
                {
                    return _detectedBeatCrossings;
                }
            }
        }

        /// <summary>Gets the number of physiologically plausible beat intervals found before consistency filtering.</summary>
        /// <value>The number of raw beat intervals.</value>
        public int RawBeatIntervals
        {
            get
            {
                lock (_stateLock)
                {
                    return _rawBeatIntervals;
                }
            }
        }

        /// <summary>Gets the infrared AC root-mean-square amplitude from the latest analysis.</summary>
        /// <value>The filtered infrared amplitude in ADC counts.</value>
        public double InfraredAcRms
        {
            get
            {
                lock (_stateLock)
                {
                    return _infraredAcRms;
                }
            }
        }

        /// <summary>Gets the positive and negative threshold used for beat detection.</summary>
        /// <value>The beat-detection threshold in filtered ADC counts.</value>
        public double BeatDetectionThreshold
        {
            get
            {
                lock (_stateLock)
                {
                    return _beatDetectionThreshold;
                }
            }
        }

        /// <summary>Gets the heart rate independently estimated from waveform periodicity.</summary>
        /// <value>The normalized-autocorrelation estimate in beats per minute, or zero when no periodic peak is available.</value>
        public double AutocorrelationHeartRate
        {
            get
            {
                lock (_stateLock)
                {
                    return _autocorrelationHeartRate;
                }
            }
        }

        /// <summary>Gets the confidence of the normalized-autocorrelation estimate.</summary>
        /// <value>A value from zero through 100 based on peak strength, prominence, and cycle evidence.</value>
        public double AutocorrelationQuality
        {
            get
            {
                lock (_stateLock)
                {
                    return _autocorrelationQuality;
                }
            }
        }

        /// <summary>Gets a value indicating whether interval and autocorrelation estimates disagreed.</summary>
        /// <value>True when both estimators were valid but exceeded the configured agreement tolerance.</value>
        public bool HeartRateEstimatorsDisagree
        {
            get
            {
                lock (_stateLock)
                {
                    return _heartRateEstimatorsDisagree;
                }
            }
        }

        /// <summary>Gets a value indicating whether the current candidate used autocorrelation without valid beat intervals.</summary>
        /// <value>True when periodicity supplied the estimate at reduced confidence.</value>
        public bool UsedAutocorrelationFallback
        {
            get
            {
                lock (_stateLock)
                {
                    return _usedAutocorrelationFallback;
                }
            }
        }

        /// <summary>Gets a value indicating whether adaptive weak-signal validation supplied SpO₂.</summary>
        /// <value>True when the current oxygen-saturation value passed the adaptive weak-signal path.</value>
        public bool UsedWeakSignalOxygenSaturation
        {
            get
            {
                lock (_stateLock)
                {
                    return _usedWeakSignalOxygenSaturation;
                }
            }
        }

        /// <summary>Gets the estimated pulse rate in beats per minute.</summary>
        /// <value>The estimated pulse rate, or zero when no valid estimate is available.</value>
        public double HeartRate
        {
            get
            {
                lock (_stateLock)
                {
                    return _heartRate;
                }
            }
        }

        /// <summary>Gets the experimental oxygen-saturation estimate as a percentage.</summary>
        /// <value>The estimated oxygen saturation, or zero when no valid estimate is available.</value>
        public double OxygenSaturation
        {
            get
            {
                lock (_stateLock)
                {
                    return _oxygenSaturation;
                }
            }
        }

        /// <summary>Gets the infrared AC signal as a percentage of its DC level.</summary>
        /// <value>The infrared perfusion-index approximation.</value>
        public double PerfusionIndex
        {
            get
            {
                lock (_stateLock)
                {
                    return _perfusionIndex;
                }
            }
        }

        /// <summary>Gets the red AC signal as a percentage of its DC level.</summary>
        /// <value>The red-channel perfusion-index approximation.</value>
        public double RedPerfusionIndex
        {
            get
            {
                lock (_stateLock)
                {
                    return _redPerfusionIndex;
                }
            }
        }

        /// <summary>Gets the correlation between the filtered infrared and red waveforms.</summary>
        /// <value>A value from -1 through 1, where 1 indicates matching waveform shapes.</value>
        public double ChannelCorrelation
        {
            get
            {
                lock (_stateLock)
                {
                    return _channelCorrelation;
                }
            }
        }

        /// <summary>Gets the red-to-infrared AC/DC ratio used by the oxygen-saturation approximation.</summary>
        /// <value>The latest ratio of ratios, or zero before an analysis is available.</value>
        public double RatioOfRatios
        {
            get
            {
                lock (_stateLock)
                {
                    return _ratioOfRatios;
                }
            }
        }

        /// <summary>Gets the active oxygen-saturation calibration intercept.</summary>
        /// <value>The intercept in the equation SpO₂ = intercept + slope × ratio.</value>
        public double OxygenSaturationCalibrationIntercept
        {
            get
            {
                lock (_stateLock)
                {
                    return _oxygenSaturationCalibrationIntercept;
                }
            }
        }

        /// <summary>Gets the active oxygen-saturation calibration slope.</summary>
        /// <value>The slope in the equation SpO₂ = intercept + slope × ratio.</value>
        public double OxygenSaturationCalibrationSlope
        {
            get
            {
                lock (_stateLock)
                {
                    return _oxygenSaturationCalibrationSlope;
                }
            }
        }

        /// <summary>Gets the periodic-signal confidence as a percentage.</summary>
        /// <value>A value from zero to 100 representing beat-interval consistency.</value>
        public double SignalQuality
        {
            get
            {
                lock (_stateLock)
                {
                    return _signalQuality;
                }
            }
        }

        /// <summary>Gets the number of samples discarded because the processing queue was full.</summary>
        /// <value>The cumulative number of discarded processing samples.</value>
        public uint DroppedSampleCount
        {
            get
            {
                lock (_queueLock)
                {
                    return _droppedSampleCount;
                }
            }
        }

        /// <summary>Queues one sensor sample for background processing.</summary>
        /// <param name="reading">The sensor sample.</param>
        /// <exception cref="ObjectDisposedException">The worker has been disposed.</exception>
        public void AddSample(Max30100Reading reading)
        {
            lock (_queueLock)
            {
                if (_stopping)
                {
                    throw new ObjectDisposedException(null);
                }

                if (_queueCount == _queue.Length)
                {
                    _droppedSampleCount += (uint)_queueCount;
                    _queueCount = 0;
                    _queueReadIndex = 0;
                    _queueWriteIndex = 0;
                    _resetRequested = true;
                }

                _queue[_queueWriteIndex] = reading;
                _queueWriteIndex = (_queueWriteIndex + 1) % _queue.Length;
                _queueCount++;
            }

            _sampleAvailable.Set();
        }

        /// <summary>
        /// Fits and asynchronously applies oxygen-saturation calibration coefficients from paired measurements.
        /// </summary>
        /// <param name="ratios">Stable ratio-of-ratios measurements.</param>
        /// <param name="referenceOxygenSaturations">Simultaneous reference oxygen-saturation percentages.</param>
        /// <exception cref="ArgumentNullException">Either array is null.</exception>
        /// <exception cref="ArgumentException">
        /// The arrays have different lengths, contain fewer than two points, all ratios are equal, or the fitted slope is not negative.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// A ratio is outside zero through 10, or a reference oxygen-saturation value is outside zero through 100.
        /// </exception>
        /// <exception cref="ObjectDisposedException">The worker has been disposed.</exception>
        public void CalibrateOxygenSaturation(double[] ratios, double[] referenceOxygenSaturations)
        {
            Max30100PulseOximeter.CalculateOxygenSaturationCalibration(
                ratios,
                referenceOxygenSaturations,
                out double intercept,
                out double slope);
            SetOxygenSaturationCalibration(intercept, slope);
        }

        /// <summary>Asynchronously applies previously calculated oxygen-saturation calibration coefficients.</summary>
        /// <param name="intercept">The intercept in the equation SpO₂ = intercept + slope × ratio.</param>
        /// <param name="slope">The negative slope in the equation SpO₂ = intercept + slope × ratio.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="intercept"/> is outside zero through 200, or <paramref name="slope"/> is outside -200 through zero.
        /// </exception>
        /// <exception cref="ObjectDisposedException">The worker has been disposed.</exception>
        public void SetOxygenSaturationCalibration(double intercept, double slope)
        {
            Max30100PulseOximeter.ValidateOxygenSaturationCalibration(intercept, slope);
            lock (_queueLock)
            {
                if (_stopping)
                {
                    throw new ObjectDisposedException(null);
                }

                _requestedCalibrationIntercept = intercept;
                _requestedCalibrationSlope = slope;
                _calibrationRequested = true;
            }

            _sampleAvailable.Set();
        }

        /// <summary>Asynchronously clears queued samples and calculated values.</summary>
        public void Reset()
        {
            lock (_queueLock)
            {
                _queueCount = 0;
                _queueReadIndex = 0;
                _queueWriteIndex = 0;
                _resetRequested = true;
            }

            _sampleAvailable.Set();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            lock (_queueLock)
            {
                if (_stopping)
                {
                    return;
                }

                _stopping = true;
            }

            _sampleAvailable.Set();
            _workerThread.Join();
        }

        private static Max30100PulseOximeterOptions GetOptions(Max30100PulseOximeterOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException();
            }

            return options;
        }

        private bool TryGetWork(out Max30100Reading reading, out bool hasReading, out bool reset)
        {
            lock (_queueLock)
            {
                reset = _resetRequested;
                _resetRequested = false;
                if (_queueCount == 0)
                {
                    reading = default(Max30100Reading);
                    hasReading = false;
                    return reset;
                }

                reading = _queue[_queueReadIndex];
                _queueReadIndex = (_queueReadIndex + 1) % _queue.Length;
                _queueCount--;
                hasReading = true;
                return true;
            }
        }

        private bool TryGetCalibration(out double intercept, out double slope)
        {
            lock (_queueLock)
            {
                if (!_calibrationRequested)
                {
                    intercept = 0;
                    slope = 0;
                    return false;
                }

                intercept = _requestedCalibrationIntercept;
                slope = _requestedCalibrationSlope;
                _calibrationRequested = false;
                return true;
            }
        }

        private void ProcessSamples()
        {
            int samplesSincePublish = 0;
            while (true)
            {
                _sampleAvailable.WaitOne();
                lock (_queueLock)
                {
                    if (_stopping)
                    {
                        return;
                    }
                }

                if (TryGetCalibration(out double intercept, out double slope))
                {
                    _processor.SetOxygenSaturationCalibration(intercept, slope);
                    PublishState();
                }

                while (TryGetWork(out Max30100Reading reading, out bool hasReading, out bool reset))
                {
                    if (reset)
                    {
                        _processor.Reset();
                        samplesSincePublish = 0;
                        PublishState();
                    }

                    if (hasReading)
                    {
                        _processor.AddSample(reading);
                        samplesSincePublish++;
                        if (samplesSincePublish >= _publishInterval)
                        {
                            samplesSincePublish = 0;
                            PublishState();
                        }
                    }
                }
            }
        }

        private void PublishState()
        {
            lock (_stateLock)
            {
                _fingerDetected = _processor.FingerDetected;
                _isSettling = _processor.IsSettling;
                _analysisWindowProgress = _processor.AnalysisWindowProgress;
                _hasValidReading = _processor.HasValidReading;
                _hasValidHeartRate = _processor.HasValidHeartRate;
                _isHeartRateHeld = _processor.IsHeartRateHeld;
                _hasValidOxygenSaturation = _processor.HasValidOxygenSaturation;
                _candidateHeartRate = _processor.CandidateHeartRate;
                _detectedBeatCrossings = _processor.DetectedBeatCrossings;
                _detectedBeatIntervals = _processor.DetectedBeatIntervals;
                _rawBeatIntervals = _processor.RawBeatIntervals;
                _infraredAcRms = _processor.InfraredAcRms;
                _beatDetectionThreshold = _processor.BeatDetectionThreshold;
                _autocorrelationHeartRate = _processor.AutocorrelationHeartRate;
                _autocorrelationQuality = _processor.AutocorrelationQuality;
                _heartRateEstimatorsDisagree = _processor.HeartRateEstimatorsDisagree;
                _usedAutocorrelationFallback = _processor.UsedAutocorrelationFallback;
                _usedWeakSignalOxygenSaturation = _processor.UsedWeakSignalOxygenSaturation;
                _heartRate = _processor.HeartRate;
                _oxygenSaturation = _processor.OxygenSaturation;
                _perfusionIndex = _processor.PerfusionIndex;
                _redPerfusionIndex = _processor.RedPerfusionIndex;
                _channelCorrelation = _processor.ChannelCorrelation;
                _ratioOfRatios = _processor.RatioOfRatios;
                _oxygenSaturationCalibrationIntercept = _processor.OxygenSaturationCalibrationIntercept;
                _oxygenSaturationCalibrationSlope = _processor.OxygenSaturationCalibrationSlope;
                _signalQuality = _processor.SignalQuality;
            }
        }
    }
}
