// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using UnitsNet;

namespace Iot.Device.Bmxx80
{
    /// <summary>
    /// The heater profile configuration saved on the device.
    /// </summary>
    public class Bme680HeaterProfileConfig
    {
        /// <summary>
        /// Gets or sets chosen heater profile slot, ranging from 0-9.
        /// </summary>
        public Bme680HeaterProfile HeaterProfile { get; set; }

        /// <summary>
        /// Gets or sets heater resistance.
        /// </summary>
        public ushort HeaterResistance { get; set; }

        /// <summary>
        /// Gets or sets heater duration.
        /// </summary>
        public Duration HeaterDuration { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bme680HeaterProfileConfig" /> class.
        /// </summary>
        /// <param name="profile">The used heater profile.</param>
        /// <param name="heaterResistance">The heater resistance in Ohm.</param>
        /// <param name="heaterDuration">The heating duration.</param>
        /// <exception cref="ArgumentOutOfRangeException">Unknown profile setting used.</exception>
        public Bme680HeaterProfileConfig(Bme680HeaterProfile profile, ushort heaterResistance, Duration heaterDuration)
        {
            if ((profile != Bme680HeaterProfile.Profile1) &&
                (profile != Bme680HeaterProfile.Profile2) &&
                (profile != Bme680HeaterProfile.Profile3) &&
                (profile != Bme680HeaterProfile.Profile4) &&
                (profile != Bme680HeaterProfile.Profile5) &&
                (profile != Bme680HeaterProfile.Profile6) &&
                (profile != Bme680HeaterProfile.Profile7) &&
                (profile != Bme680HeaterProfile.Profile8) &&
                (profile != Bme680HeaterProfile.Profile9) &&
                (profile != Bme680HeaterProfile.Profile10))
            {
                throw new ArgumentOutOfRangeException();
            }

            HeaterProfile = profile;
            HeaterResistance = heaterResistance;
            HeaterDuration = heaterDuration;
        }
    }
}
