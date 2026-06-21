// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ltr553AlsWa
{
    /// <summary>
    /// LTR-553ALS-WA internal register addresses.
    /// </summary>
    internal enum Register : byte
    {
        /// <summary>ALS control (mode and gain).</summary>
        AlsContr = 0x80,

        /// <summary>PS control (mode).</summary>
        PsContr = 0x81,

        /// <summary>PS LED settings (pulse frequency, duty cycle, peak current).</summary>
        PsLed = 0x82,

        /// <summary>PS number of LED pulses.</summary>
        PsNPulses = 0x83,

        /// <summary>PS measurement rate.</summary>
        PsMeasRate = 0x84,

        /// <summary>ALS measurement rate and integration time.</summary>
        AlsMeasRate = 0x85,

        /// <summary>Part ID.</summary>
        PartId = 0x86,

        /// <summary>Manufacturer ID.</summary>
        ManufacturerId = 0x87,

        /// <summary>ALS data channel 1 low byte.</summary>
        AlsDataCh1Low = 0x88,

        /// <summary>ALS data channel 1 high byte.</summary>
        AlsDataCh1High = 0x89,

        /// <summary>ALS data channel 0 low byte.</summary>
        AlsDataCh0Low = 0x8A,

        /// <summary>ALS data channel 0 high byte.</summary>
        AlsDataCh0High = 0x8B,

        /// <summary>ALS and PS status register.</summary>
        AlsPsStatus = 0x8C,

        /// <summary>PS data low byte.</summary>
        PsDataLow = 0x8D,

        /// <summary>PS data high byte.</summary>
        PsDataHigh = 0x8E,

        /// <summary>Interrupt configuration.</summary>
        Interrupt = 0x8F,

        /// <summary>PS upper threshold low byte.</summary>
        PsThresholdUpLow = 0x90,

        /// <summary>PS upper threshold high byte.</summary>
        PsThresholdUpHigh = 0x91,

        /// <summary>PS lower threshold low byte.</summary>
        PsThresholdLowLow = 0x92,

        /// <summary>PS lower threshold high byte.</summary>
        PsThresholdLowHigh = 0x93,

        /// <summary>ALS upper threshold low byte.</summary>
        AlsThresholdUpLow = 0x97,

        /// <summary>ALS upper threshold high byte.</summary>
        AlsThresholdUpHigh = 0x98,

        /// <summary>ALS lower threshold low byte.</summary>
        AlsThresholdLowLow = 0x99,

        /// <summary>ALS lower threshold high byte.</summary>
        AlsThresholdLowHigh = 0x9A,

        /// <summary>Interrupt persistence (consecutive out-of-range count).</summary>
        InterruptPersist = 0x9E,
    }
}
