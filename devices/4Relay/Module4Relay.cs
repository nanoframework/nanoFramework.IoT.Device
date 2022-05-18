// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;

namespace Iot.Device.Relay
{
    /// <summary>
    /// Module 4 Relay. This module only contains 4 relays and no leds.
    /// </summary>
    public class Module4Relay : Base4Relay
    {
        /// <summary>
        /// Creates a Module 4 Relay.
        /// </summary>
        /// <param name="i2cDevice">The I2C device.</param>
        public Module4Relay(I2cDevice i2cDevice) : base(i2cDevice, RelayType.Module)
        {
        }
    }
}
