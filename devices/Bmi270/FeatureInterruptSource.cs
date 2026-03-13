// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Bmi270
{
    /// <summary>
    /// Feature interrupt sources that can be mapped to INT1 or INT2 pins.
    /// Corresponds to bits in INT1_MAP_FEAT (0x56) and INT2_MAP_FEAT (0x57).
    /// </summary>
    [Flags]
    public enum FeatureInterruptSource : byte
    {
        /// <summary>No feature interrupts.</summary>
        None = 0x00,

        /// <summary>Significant motion detection.</summary>
        SignificantMotion = 0x01,

        /// <summary>Step counter watermark or step detector.</summary>
        StepCounter = 0x02,

        /// <summary>Activity recognition change (still/walking/running).</summary>
        Activity = 0x04,

        /// <summary>Wrist wear wakeup.</summary>
        WristWearWakeUp = 0x08,

        /// <summary>Wrist gesture detection.</summary>
        WristGesture = 0x10,

        /// <summary>No-motion detection.</summary>
        NoMotion = 0x20,

        /// <summary>Any-motion detection.</summary>
        AnyMotion = 0x40,
    }
}
