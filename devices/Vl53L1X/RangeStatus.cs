// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vl53L1X
{
    /// <summary>
    /// The range status reported by the device.
    /// </summary>
    public enum RangeStatus : byte
    {
        /// <summary>
        /// The range is valid.
        /// </summary>
        NoError = 0,

        /// <summary>
        /// The measurement repeatability is outside the configured sigma threshold.
        /// </summary>
        SigmaFailure = 1,

        /// <summary>
        /// The return signal is below the configured signal threshold.
        /// </summary>
        SignalFailure = 2,

        /// <summary>
        /// The range is valid, but the target is below the minimum detection threshold.
        /// </summary>
        RangeValidMinRangeClipped = 3,

        /// <summary>
        /// The measured phase is outside the valid limits.
        /// </summary>
        OutOfBounds = 4,

        /// <summary>
        /// A hardware failure occurred.
        /// </summary>
        HardwareFailure = 5,

        /// <summary>
        /// The range is valid, but the wraparound check was not performed.
        /// </summary>
        RangeValidNoWrapCheck = 6,

        /// <summary>
        /// A wrapped target was detected.
        /// </summary>
        WrapAround = 7,

        /// <summary>
        /// An internal processing underflow or overflow occurred.
        /// </summary>
        ProcessingFailure = 8,

        /// <summary>
        /// The crosstalk signal is too high.
        /// </summary>
        XtalkSignalFailure = 9,

        /// <summary>
        /// The first synchronization interrupt occurred after ranging started.
        /// </summary>
        SynchronizationInterrupt = 10,

        /// <summary>
        /// The range is valid, but the result contains merged pulses.
        /// </summary>
        RangeValidMergedPulse = 11,

        /// <summary>
        /// A target is present, but the signal is insufficient.
        /// </summary>
        TargetPresentLackOfSignal = 12,

        /// <summary>
        /// The configured region of interest is outside the SPAD array.
        /// </summary>
        MinRangeFailure = 13,

        /// <summary>
        /// The low-level driver returned an invalid negative range.
        /// </summary>
        RangeInvalid = 14,

        /// <summary>
        /// The device returned an unrecognized status.
        /// </summary>
        Unknown = 255
    }
}
