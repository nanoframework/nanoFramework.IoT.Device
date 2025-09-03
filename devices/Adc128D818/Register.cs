// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Adc128D818
{
    /// <summary>
    /// ADC128D818 Internal Registers.
    /// </summary>
    public enum Register : byte
    {
        // Details in Table 19. ADC128D818 Internal Registers

        /// <summary>
        /// Configuration Register.
        /// </summary>
        Configuration = 0x00,

        /// <summary>
        /// Interrupt Status Register.
        /// </summary>
        InterruptStatus = 0x01,

        /// <summary>
        /// Interrupt Mask Register.
        /// </summary>
        InterruptMask = 0x03,

        /// <summary>
        /// Conversion Rate Register.
        /// </summary>
        ConversionRate = 0x07,

        /// <summary>
        /// Channel Disable Register.
        /// </summary>
        ChannelDisable = 0x08,

        /// <summary>
        /// One-Shot Register.
        /// </summary>
        OneShot = 0x09,

        /// <summary>
        /// Deep Shutdown Register.
        /// </summary>
        DeepShutdown = 0x0A,

        /// <summary>
        /// Advanced Configuration Register.
        /// </summary>
        AdvancedConfiguration = 0x0B,

        /// <summary>
        /// Busy Status Register.
        /// </summary>
        BusyStatus = 0x0C,

        /// <summary>
        /// Channel reading register 0.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mode 0: IN0 reading.
        /// </para>
        /// <para>
        /// Mode 1: IN0 reading.
        /// </para>
        /// <para>
        /// Mode 2: IN0(+) and IN1(-) Reading.
        /// </para>
        /// <para>
        /// Mode 3: IN0 reading.
        /// </para>
        /// </remarks>
        ChannelReading0 = 0x20,

        /// <summary>
        /// Channel reading register 1.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mode 0: IN1 reading.
        /// </para>
        /// <para>
        /// Mode 1: IN1 reading.
        /// </para>
        /// <para>
        /// Mode 2: IN3(+) and IN2(-) Reading.
        /// </para>
        /// <para>
        /// Mode 3: IN1 reading.
        /// </para>
        /// </remarks>
        ChannelReading1 = 0x21,

        /// <summary>
        /// Channel reading register 2.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mode 0: IN2 reading.
        /// </para>
        /// <para>
        /// Mode 1: IN2 reading.
        /// </para>
        /// <para>
        /// Mode 2: IN4(+) and IN5(-) Reading.
        /// </para>
        /// <para>
        /// Mode 3: IN2 reading.
        /// </para>
        /// </remarks>
        ChannelReading2 = 0x22,

        /// <summary>
        /// Channel reading register 3.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mode 0: IN3 reading.
        /// </para>
        /// <para>
        /// Mode 1: IN3 reading.
        /// </para>
        /// <para>
        /// Mode 2: IN7(+) and IN6(-) Reading.
        /// </para>
        /// <para>
        /// Mode 3: IN3 reading.
        /// </para>
        /// </remarks>
        ChannelReading3 = 0x23,

        /// <summary>
        /// Channel reading register 4.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mode 0: IN4 reading.
        /// </para>
        /// <para>
        /// Mode 1: IN4 reading.
        /// </para>
        /// <para>
        /// Mode 2: Reserved.
        /// </para>
        /// <para>
        /// Mode 3: IN4(+) and IN5(-) Reading.
        /// </para>
        /// </remarks>
        ChannelReading4 = 0x24,

        /// <summary>
        /// Channel reading register 5.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mode 0: IN5 reading.
        /// </para>
        /// <para>
        /// Mode 1: IN5 reading.
        /// </para>
        /// <para>
        /// Mode 2: Reserved.
        /// </para>
        /// <para>
        /// Mode 3: IN7(+) and IN6(-) Reading.
        /// </para>
        /// </remarks>
        ChannelReading5 = 0x25,

        /// <summary>
        /// Channel reading register 6.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mode 0: IN6 reading.
        /// </para>
        /// <para>
        /// Mode 1: IN6 reading.
        /// </para>
        /// <para>
        /// Mode 2: Reserved.
        /// </para>
        /// <para>
        /// Mode 3: Reserved.
        /// </para>
        /// </remarks>
        ChannelReading6 = 0x26,

        /// <summary>
        /// Channel reading register 7.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mode 0: Temperature Reading.
        /// </para>
        /// <para>
        /// Mode 1: IN7 reading.
        /// </para>
        /// <para>
        /// Mode 2: Temperature Reading.
        /// </para>
        /// <para>
        /// Mode 3: Temperature Reading.
        /// </para>
        /// </remarks>
        ChannelReading7 = 0x27,

        /// <summary>
        /// Manufacturer ID Register.
        /// </summary>
        ManufacturerId = 0x3E,

        /// <summary>
        /// Revision ID Register.
        /// </summary>
        RevisionId = 0x3F,
    }
}
