// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Vcnl4040.Definitions;

namespace Iot.Device.Vcnl4040
{
    /// <summary>Defines proximity interrupt thresholds, persistence, and mode.</summary>
    public class ProximityInterruptConfiguration
    {
        /// <summary>Initializes a new instance of the <see cref="ProximityInterruptConfiguration"/> class.</summary>
        /// <param name="lowerThreshold">The lowerThreshold value.</param>
        /// <param name="upperThreshold">The upperThreshold value.</param>
        /// <param name="persistence">The persistence value.</param>
        /// <param name="smartPersistenceEnabled">The smartPersistenceEnabled value.</param>
        /// <param name="mode">The mode value.</param>
        public ProximityInterruptConfiguration(ushort lowerThreshold, ushort upperThreshold, PsInterruptPersistence persistence, bool smartPersistenceEnabled, ProximityInterruptMode mode)
        {
            LowerThreshold = lowerThreshold;
            UpperThreshold = upperThreshold;
            Persistence = persistence;
            SmartPersistenceEnabled = smartPersistenceEnabled;
            Mode = mode;
        }

        /// <summary>Gets the lower proximity threshold.</summary>
        public ushort LowerThreshold { get; private set; }

        /// <summary>Gets the upper proximity threshold.</summary>
        public ushort UpperThreshold { get; private set; }

        /// <summary>Gets the interrupt persistence setting.</summary>
        public PsInterruptPersistence Persistence { get; private set; }

        /// <summary>Gets a value indicating whether smart persistence is enabled.</summary>
        public bool SmartPersistenceEnabled { get; private set; }

        /// <summary>Gets the proximity interrupt mode.</summary>
        public ProximityInterruptMode Mode { get; private set; }
    }
}
