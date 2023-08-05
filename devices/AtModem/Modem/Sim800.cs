// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using IoT.Device.AtModem.Network;

namespace IoT.Device.AtModem.Modem
{
    /// <summary>
    /// Represents a SIM800 modem.
    /// </summary>
    public class Sim800 : ModemBase
    {
        private INetwork _network = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sim800"/> class.
        /// </summary>
        /// <param name="channel">A channel to communicate with the modem.</param>
        public Sim800(AtChannel channel) : base(channel)
        {
            // Wake up the device and set the automatic baud rate detection
            const int MaxRetry = 10;
            int retry = 0;
        retry:
            try
            {
                var res = Channel.SendCommand("ATE0");
                if (!res.Success)
                {
                    Debug.WriteLine($"Failed to wake up the device, retrying {retry}.");
                    if (retry++ < MaxRetry)
                    {
                        goto retry;
                    }
                }
            }
            catch (System.Exception)
            {
                if (retry++ < MaxRetry)
                {
                    goto retry;
                }
            }

            // Getting rid of the echo
            Channel.SendCommand("ATE0");
        }

        /// <inheritdoc/>
        public override INetwork Network
        {
            get
            {
                if (_network == null)
                {
                    _network = new Sim800Network(this);
                }

                return _network;
            }
        }
    }
}
