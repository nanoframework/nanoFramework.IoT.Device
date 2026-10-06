// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Max30100
{
    /// <summary>The MAX30100 measurement mode.</summary>
    public enum Max30100Mode : byte
    {
        /// <summary>Heart-rate mode using the infrared LED.</summary>
        HeartRate = 0x02,

        /// <summary>Pulse-oximetry mode using both LEDs.</summary>
        SpO2 = 0x03,
    }

    /// <summary>The MAX30100 sample rate.</summary>
    public enum Max30100SampleRate : byte
    {
        /// <summary>50 samples per second.</summary>
        SamplesPerSecond50 = 0,

        /// <summary>100 samples per second.</summary>
        SamplesPerSecond100 = 1,

        /// <summary>167 samples per second.</summary>
        SamplesPerSecond167 = 2,

        /// <summary>200 samples per second.</summary>
        SamplesPerSecond200 = 3,

        /// <summary>400 samples per second.</summary>
        SamplesPerSecond400 = 4,

        /// <summary>600 samples per second.</summary>
        SamplesPerSecond600 = 5,

        /// <summary>800 samples per second.</summary>
        SamplesPerSecond800 = 6,

        /// <summary>1000 samples per second.</summary>
        SamplesPerSecond1000 = 7,
    }

    /// <summary>The LED pulse width and associated ADC resolution.</summary>
    public enum Max30100PulseWidth : byte
    {
        /// <summary>200 microseconds with 13-bit resolution.</summary>
        Microseconds200 = 0,

        /// <summary>400 microseconds with 14-bit resolution.</summary>
        Microseconds400 = 1,

        /// <summary>800 microseconds with 15-bit resolution.</summary>
        Microseconds800 = 2,

        /// <summary>1600 microseconds with 16-bit resolution.</summary>
        Microseconds1600 = 3,
    }

    /// <summary>The MAX30100 LED drive current.</summary>
    public enum Max30100LedCurrent : byte
    {
        /// <summary>0.0 milliamps.</summary>
        Milliamps0 = 0,

        /// <summary>4.4 milliamps.</summary>
        Milliamps4Point4 = 1,

        /// <summary>7.6 milliamps.</summary>
        Milliamps7Point6 = 2,

        /// <summary>11.0 milliamps.</summary>
        Milliamps11 = 3,

        /// <summary>14.2 milliamps.</summary>
        Milliamps14Point2 = 4,

        /// <summary>17.4 milliamps.</summary>
        Milliamps17Point4 = 5,

        /// <summary>20.8 milliamps.</summary>
        Milliamps20Point8 = 6,

        /// <summary>24.0 milliamps.</summary>
        Milliamps24 = 7,

        /// <summary>27.1 milliamps.</summary>
        Milliamps27Point1 = 8,

        /// <summary>30.6 milliamps.</summary>
        Milliamps30Point6 = 9,

        /// <summary>33.8 milliamps.</summary>
        Milliamps33Point8 = 10,

        /// <summary>37.0 milliamps.</summary>
        Milliamps37 = 11,

        /// <summary>40.2 milliamps.</summary>
        Milliamps40Point2 = 12,

        /// <summary>43.6 milliamps.</summary>
        Milliamps43Point6 = 13,

        /// <summary>46.8 milliamps.</summary>
        Milliamps46Point8 = 14,

        /// <summary>50.0 milliamps.</summary>
        Milliamps50 = 15,
    }
}
