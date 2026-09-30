// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Vcnl4040.Definitions;
using UnitsNet;

namespace Iot.Device.Vcnl4040
{
    /// <summary>Defines ambient light interrupt thresholds and persistence.</summary>
    public class AmbientLightInterruptConfiguration
    {
        /// <summary>Initializes a new instance of the <see cref="AmbientLightInterruptConfiguration"/> class.</summary>
        /// <param name="lowerThreshold">The lowerThreshold value.</param>
        /// <param name="upperThreshold">The upperThreshold value.</param>
        /// <param name="persistence">The persistence value.</param>
        public AmbientLightInterruptConfiguration(Illuminance lowerThreshold, Illuminance upperThreshold, AlsInterruptPersistence persistence)
        {
            LowerThreshold = lowerThreshold;
            UpperThreshold = upperThreshold;
            Persistence = persistence;
        }

        /// <summary>Gets the lower illuminance threshold.</summary>
        public Illuminance LowerThreshold { get; private set; }

        /// <summary>Gets the upper illuminance threshold.</summary>
        public Illuminance UpperThreshold { get; private set; }

        /// <summary>Gets the number of consecutive measurements required to trigger an interrupt.</summary>
        public AlsInterruptPersistence Persistence { get; private set; }
    }
}
