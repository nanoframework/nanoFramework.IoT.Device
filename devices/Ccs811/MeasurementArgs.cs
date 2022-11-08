// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using UnitsNet;

namespace Iot.Device.Ccs811
{
    /// <summary>
    /// Arguments of the Measurement Threshold event
    /// Contains the measurements done and potential error.
    /// </summary>
    public class MeasurementArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether measurement is successful.
        /// </summary>
        public bool MeasurementSuccess { get; set; }

        /// <summary>
        /// Gets or sets equivalent CO2, best is to use PartsPerMilion for a readable range.
        /// </summary>
        public VolumeConcentration EquivalentCO2 { get; set; }

        /// <summary>
        /// Gets or sets equivalent Total Volatile Organic Compound, best is to use PartsPerBilion for a readable range.
        /// </summary>
        public VolumeConcentration EquivalentTotalVolatileOrganicCompound { get; set; }

        /// <summary>
        /// Gets or sets raw current selected, best to use MicroAmpere for a readable range.
        /// </summary>
        public ElectricCurrent RawCurrentSelected { get; set; }

        /// <summary>
        /// Gets or sets raw ADC reading.
        /// </summary>
        public int RawAdcReading { get; set; }
    }
}
