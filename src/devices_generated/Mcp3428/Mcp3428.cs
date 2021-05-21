// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;

namespace Iot.Device.Mcp3428
{
    /// <summary>
    /// Represents MCP3428 ADC
    /// </summary>
    public class Mcp3428 : Mcp342x
    {
        private const int NumChannels = 4;

        /// <summary>
        /// Constructs Mcp3428 instance
        /// </summary>
        /// <param name="i2CDevice">I2C device used to communicate with the device</param>
        public Mcp3428(I2cDevice i2CDevice)
            : base(i2CDevice, NumChannels)
        {
        }

        /// <summary>
        /// Constructs Mcp3428 instance
        /// </summary>
        /// <param name="i2CDevice">I2C device used to communicate with the device</param>
        /// <param name="mode">ADC operation mode</param>
        /// <param name="resolution">ADC resolution</param>
        /// <param name="pgaGain">PGA gain</param>
        public Mcp3428(I2cDevice i2CDevice, AdcMode mode = AdcMode.Continuous,
            AdcResolution resolution = AdcResolution.Bit12, AdcGain pgaGain = AdcGain.X1)
            : this(i2CDevice) => SetConfig(0, mode: mode, resolution: resolution, pgaGain: pgaGain);

        /// <summary>
        /// Determine device I2C address based on the configuration pin states.
        /// </summary>
        /// <param name="adr0">The adr0 pin state</param>
        /// <param name="adr1">The adr1 pin state</param>
        /// <returns>System.Int32.</returns>
        public static int I2CAddressFromPins(PinState adr0, PinState adr1) => Helpers.I2CAddressFromPins(adr0, adr1);
    }
}
