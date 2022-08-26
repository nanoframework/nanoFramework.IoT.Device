// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Ds1621
{
    /// <summary>
    /// Register mask constants for the Ds1621.
    /// </summary>
    internal enum RegisterMask : byte
    {
        #region Configuration Register

        /// <summary>
        /// The temperature conversion mode.
        /// </summary>
        /// <remarks>
        /// <para>1 = The Ds1621 will perform a single temperature conversion upon receipt of the Measure Temperature command.</para>
        /// <para>0 = The Ds1621 will continuously perform temperature conversions after receipt of the Measure Temperature command.</para>
        /// <para>This bit is nonvolatile.</para>
        /// </remarks>
        OneShotConversionModeMask = 0b0000_0001,

        /// <summary>
        /// The polarity of the thermostat output signal.
        /// </summary>
        /// <remarks>
        /// <para>1 = Active high.</para>
        /// <para>0 = Active low.</para>
        /// <para>This bit is nonvolatile.</para>
        /// </remarks>
        OutputPolarityMask = 0b0000_0010,

        /// <summary>
        /// The status of E2 memory cell.
        /// </summary>
        /// <remarks>
        /// <para>1 = Write to memory in progress.</para>
        /// <para>0 = Memory is not busy.</para>
        /// <para>A memory write cycle may take up to 10 ms.</para></remarks>
        NonvolatileMemoryBusyMask = 0b0001_0000,

        /// <summary>
        /// Flag that provides a method of determining if the Ds1621 has ever been subjected to temperatures below TL.
        /// </summary>
        /// <remarks>This bit will be set to 1 when the temperature is less than or equal to the value of the TL register. It will remain 1 until reset by either writing 0 into this location or removing power from the device.</remarks>
        LowTemperatureAlarmMask = 0b0010_0000,

        /// <summary>
        /// Flag that provides a method of determining if the Ds1621 has ever been subjected to temperatures above TH.
        /// </summary>
        /// <remarks>This bit will be set to 1 when the temperature is greater than or equal to the value of the TH register. It will remain 1 until reset by either writing 0 into this location or removing power from the device.</remarks>
        HighTemperatureAlarmMask = 0b0100_0000,

        /// <summary>
        /// Temperature conversion status bit.
        /// </summary>
        /// <remarks>
        /// <para>1 = Temperature conversion is complete.</para>
        /// <para>0 = Temperature conversion in progress.</para>
        /// </remarks>
        TemperatureConversionDoneMask = 0b1000_0000

        #endregion
    }
}
