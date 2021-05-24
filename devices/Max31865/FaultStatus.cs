// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable CS1591, CS1572, CS1573

namespace Iot.Device.Max31865
{
    /// <summary>
    /// MAX31865 Fault Status
    /// </summary>

    public class FaultStatus
    {
        /// <summary>
        /// FaultStatus Constructor
        /// </summary>
        /// <param name="overUnderVoltage">If an overvoltage or undervoltage has occurred.</param>
        /// <param name="resistanceTemperatureDetectorLow">Resistance temperature detector is low.</param>
        /// <param name="referenceInLow">Reference in is low.</param>
        /// <param name="referenceInHigh">Reference in is high.</param>
        /// <param name="lowThreshold">The ADC conversion is less than or equal to the low threshold.</param>
        /// <param name="highThreshold">The ADC conversion is greater than or equal to the high threshold.</param>
        public FaultStatus(bool overUnderVoltage, bool resistanceTemperatureDetectorLow, bool referenceInLow, bool referenceInHigh, bool lowThreshold, bool highThreshold)
        {
            OverUnderVoltage = overUnderVoltage;
            ResistanceTemperatureDetectorLow = resistanceTemperatureDetectorLow;
            ReferenceInLow = referenceInLow;
            ReferenceInHigh = referenceInHigh;
            LowThreshold = lowThreshold;
            HighThreshold = highThreshold;
        }

        /// <summary>
        /// If an overvoltage or undervoltage has occurred.
        /// </summary>
        public bool OverUnderVoltage { get; set; }

        /// <summary>
        /// Resistance temperature detector is low.
        /// </summary>
        public bool ResistanceTemperatureDetectorLow { get; set; }

        /// <summary>
        /// Reference in is low.
        /// </summary>
        public bool ReferenceInLow { get; set; }

        /// <summary>
        /// Reference in is high.
        /// </summary>
        public bool ReferenceInHigh { get; set; }

        /// <summary>
        /// The ADC conversion is less than or equal to the low threshold.
        /// </summary>
        public bool LowThreshold { get; set; }

        /// <summary>
        /// The ADC conversion is greater than or equal to the high threshold.
        /// </summary>
        public bool HighThreshold { get; set; }
    }
}
