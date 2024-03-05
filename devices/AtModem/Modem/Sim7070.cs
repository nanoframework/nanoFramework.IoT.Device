// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.AtModem.Modem
{
    /// <summary>
    /// Represents a SIM7070 modem.
    /// </summary>
    public class Sim7070 : Sim7080
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sim7070"/> class.
        /// </summary>
        /// <param name="channel">A channel to communicate with the modem.</param>
        public Sim7070(AtChannel channel) : base(channel)
        {
        }
    }
}
