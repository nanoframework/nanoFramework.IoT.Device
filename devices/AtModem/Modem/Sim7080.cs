// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Net.Http;
using IoT.Device.AtModem.CodingSchemes;
using IoT.Device.AtModem.DTOs;
using IoT.Device.AtModem.Http;
using IoT.Device.AtModem.Mqtt;
using IoT.Device.AtModem.Network;
using nanoFramework.M2Mqtt;

namespace IoT.Device.AtModem.Modem
{
    /// <summary>
    /// Represents a SIM7080 modem.
    /// </summary>
    public class Sim7080 : ModemBase
    {
        private IFileStorage _fileStorage = null;
        private IMqttClient _mqttClient = null;
        private INetwork _network = null;
        private Sim7080HttpClient _httpClient = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sim7080"/> class.
        /// </summary>
        /// <param name="channel">A channel to communicate with the modem.</param>
        public Sim7080(AtChannel channel) : base(channel)
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
        public override IFileStorage FileStorage
        {
            get
            {
                if (_fileStorage == null)
                {
                    _fileStorage = new FileStorage.Sim7080FileStorage(this);
                    IsFileStorageInstancieted = true;
                }

                return _fileStorage;
            }
        }

        /// <inheritdoc/>
        public override IMqttClient MqttClient
        {
            get
            {
                if (_mqttClient == null)
                {
                    _mqttClient = new Sim7080MqttClient(this);
                    IsMqttClientInstancieted = true;
                }

                return _mqttClient;
            }
        }

        /// <inheritdoc/>
        public override INetwork Network
        {
            get
            {
                if (_network == null)
                {
                    _network = new Sim7080Network(this);
                    IsNetworkInstancieted = true;
                }

                return _network;
            }
        }

        /// <inheritdoc/>
        public override HttpClient HttpClient
        {
            get
            {
                if (_httpClient == null)
                {
                    _httpClient = new Sim7080HttpClient(this);
                    IsHttpClientInstancieted = true;
                }

                return _httpClient;
            }
        }
    }
}
