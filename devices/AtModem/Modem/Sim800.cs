// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Net.Http;
using Iot.Device.AtModem.FileStorage;
using Iot.Device.AtModem.Http;
using Iot.Device.AtModem.Network;

namespace Iot.Device.AtModem.Modem
{
    /// <summary>
    /// Represents a SIM800 modem.
    /// </summary>
    public class Sim800 : ModemBase
    {
        private INetwork _network = null;
        private Sim800FileStorage _fileStorage = null;
        private Sim800HttpClient _httpClient = null;

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

            // Set full functionality
            Enabled = true;

            // Ask for network registration changes.
            channel.SendCommand("AT+CREG=1");
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

        /// <inheritdoc/>
        public override IFileStorage FileStorage
        {
            get
            {
                if (_fileStorage == null)
                {
                    _fileStorage = new Sim800FileStorage(this);
                }

                return _fileStorage;
            }
        }

        /// <inheritdoc/>
        public override HttpClient HttpClient
        {
            get
            {
                if (_httpClient == null)
                {
                    _httpClient = new Sim800HttpClient(this);
                }

                return _httpClient;
            }
        }
    }
}
