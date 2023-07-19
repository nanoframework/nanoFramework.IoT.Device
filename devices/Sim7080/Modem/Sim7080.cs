// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using IoT.Device.AtModem.CodingSchemes;
using IoT.Device.AtModem.DTOs;

namespace IoT.Device.AtModem.Modem
{
    /// <summary>
    /// Represents a SIM7080 modem.
    /// </summary>
    public class Sim7080 : ModemBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sim7080"/> class.
        /// </summary>
        /// <param name="channel">A channel to communicate with the modem.</param>
        public Sim7080(AtChannel channel) : base(channel)
        {
        }

        /// <inheritdoc/>
        public override ModemResponse SendSmsInPduFormatAsync(PhoneNumber phoneNumber, string message, CodingScheme codingScheme)
        {
            throw new System.NotImplementedException();
        }
    }
}
