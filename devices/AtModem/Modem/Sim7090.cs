// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.AtModem.Modem
{
    /// <summary>
    /// Represents a SIM7090 modem.
    /// </summary>
    public class Sim7090 : Sim7080
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sim7090"/> class.
        /// </summary>
        /// <param name="channel">A channel to communicate with the modem.</param>
        public Sim7090(AtChannel channel) : base(channel)
        {
        }
    }
}
