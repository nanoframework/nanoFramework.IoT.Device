// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Spi;

namespace Iot.Device.Adc
{
    /// <summary>
    /// MCP3002 Analog to Digital Converter (ADC)
    /// </summary>
    public class Mcp3002 : Mcp3xxx
    {
        /// <summary>
        /// Constructs Mcp3002 instance
        /// </summary>
        /// <param name="spiDevice">Device used for SPI communication</param>
        public Mcp3002(SpiDevice spiDevice)
            : base(spiDevice, channelCount: 2, adcResolutionBits: 10)
        {
        }
    }
}
