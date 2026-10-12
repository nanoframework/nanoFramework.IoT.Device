// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Max30100
{
    /// <summary>Identifies why the current signal has not produced a valid heart rate.</summary>
    public enum Max30100SignalDiagnostic
    {
        /// <summary>No signal problem is currently reported.</summary>
        None,

        /// <summary>The analysis window is still filling with stable samples.</summary>
        FillingAnalysisWindow,

        /// <summary>The infrared AC signal is below the configured minimum amplitude.</summary>
        InsufficientAcAmplitude,

        /// <summary>The infrared perfusion index is below the configured minimum.</summary>
        InsufficientPerfusion,

        /// <summary>Too few prominence-qualified pulse peaks were detected.</summary>
        TooFewBeatPeaks,

        /// <summary>Too few intervals fall within the configured heart-rate range.</summary>
        TooFewBeatIntervals,

        /// <summary>Too few beat intervals agree with the median interval.</summary>
        InconsistentBeatIntervals,

        /// <summary>The interval estimate does not meet the configured quality requirement.</summary>
        LowSignalQuality,

        /// <summary>The interval and autocorrelation estimates disagree.</summary>
        EstimatorsDisagree,

        /// <summary>Autocorrelation supplied the candidate, which is awaiting confirmation.</summary>
        AutocorrelationFallbackPending,

        /// <summary>No heart-rate estimate falls within the configured range.</summary>
        NoHeartRateEstimate,

        /// <summary>A qualifying heart-rate candidate is awaiting confirmation.</summary>
        CandidateConfirmationPending,
    }
}
