// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Vcnl4040.Definitions;

namespace Iot.Device.Vcnl4040
{
    /// <summary>Defines the proximity sensor emitter configuration.</summary>
    public class EmitterConfiguration
    {
        /// <summary>Initializes a new instance of the <see cref="EmitterConfiguration"/> class.</summary>
        /// <param name="current">The current value.</param>
        /// <param name="dutyRatio">The dutyRatio value.</param>
        /// <param name="integrationTime">The integrationTime value.</param>
        /// <param name="multiPulses">The multiPulses value.</param>
        public EmitterConfiguration(PsLedCurrent current, PsDuty dutyRatio, PsIntegrationTime integrationTime, PsMultiPulse multiPulses)
        {
            Current = current;
            DutyRatio = dutyRatio;
            IntegrationTime = integrationTime;
            MultiPulses = multiPulses;
        }

        /// <summary>Gets the IR LED peak current.</summary>
        public PsLedCurrent Current { get; private set; }

        /// <summary>Gets the IR LED duty ratio.</summary>
        public PsDuty DutyRatio { get; private set; }

        /// <summary>Gets the proximity integration time.</summary>
        public PsIntegrationTime IntegrationTime { get; private set; }

        /// <summary>Gets the number of pulses per measurement.</summary>
        public PsMultiPulse MultiPulses { get; private set; }
    }
}
