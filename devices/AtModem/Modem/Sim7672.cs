// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using Iot.Device.AtModem.Events;
using Iot.Device.AtModem.Gnss;
using Iot.Device.AtModem.Http;
using Iot.Device.AtModem.Mqtt;
using Iot.Device.AtModem.Network;
using Iot.Device.Common.GnssDevice;
using nanoFramework.M2Mqtt;

namespace Iot.Device.AtModem.Modem
{
    /// <summary>
    /// Represents a SIM7672 modem.
    /// </summary>
    public class Sim7672 : ModemBase
    {
        private IFileStorage _fileStorage = null;
        private INetwork _network = null;
        private Sim7672HttpClient _httpClient = null;
        private IMqttClient _mqttClient = null;
        private Sim7672Gnss _gnss = null;
        private CancellationTokenSource _promptArived = new CancellationTokenSource();

        /// <summary>
        /// Initializes a new instance of the <see cref="Sim7672"/> class.
        /// </summary>
        /// <param name="channel">A channel to communicate with the modem.</param>
        public Sim7672(AtChannel channel) : base(channel)
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
            GenericEvent += UnsolicitedEvent;
        }

        private void UnsolicitedEvent(object sender, GenericEventArgs e)
        {
            if (e.Message == ">")
            {
#if DEBUG
                Debug.WriteLine("IN: >");
#endif
                _promptArived.Cancel();
            }
        }        

        /// <inheritdoc/>
        public override IFileStorage FileStorage
        {
            get
            {
                if (_fileStorage == null)
                {
                    _fileStorage = new FileStorage.Sim7672FileStorage(this);
                    IsFileStorageInstancieted = true;
                }

                return _fileStorage;
            }
        }

        /// <inheritdoc/>
        public override INetwork Network
        {
            get
            {
                if (_network == null)
                {
                    _network = new Sim7672Network(this);
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
                    _httpClient = new Sim7672HttpClient(this);
                    IsHttpClientInstancieted = true;
                }

                return _httpClient;
            }
        }

        /// <inheritdoc/>
        public override IMqttClient MqttClient
        {
            get
            {
                if (_mqttClient == null)
                {
                    _mqttClient = new Sim7672MqttClient(this);
                    IsMqttClientInstancieted = true;
                }

                return _mqttClient;
            }
        }

        /// <inheritdoc/>
        public override GnssBase Gnss
        {
            get
            {
                if (_gnss == null)
                {
                    _gnss = new Sim7672Gnss(this);
                    IsGnssIntancieted = true;
                }

                return _gnss;
            }
        }

        /// <summary>
        /// Stores a certificate in a secure way.
        /// </summary>
        /// <param name="certificateName">The name of the file certificate.</param>
        /// <param name="bytes">The UTF8 encoded bytes, the certificate has to be a PEM.</param>
        /// <returns>True in case of success.</returns>
        internal bool SetCertificate(string certificateName, byte[] bytes)
        {
            // We send a raw data the order to upload the data
            return UploadData($"AT+CCERTDOWN=\"{certificateName}\",{bytes.Length}", bytes);
        }

        /// <summary>
        /// Upload data with the > pattern.
        /// </summary>
        /// <param name="command">The command (not /r/n terminated) which should include the full line to send.</param>
        /// <param name="bytes">The data to upload.</param>
        /// <returns>True in case of success.</returns>
        internal bool UploadData(string command, byte[] bytes)
        {
            bool success = true;
            try
            {    
                // We send a raw data the order to upload the data
                _promptArived = new CancellationTokenSource(1000);
                Channel.SendBytesWithoutAck(Encoding.UTF8.GetBytes($"{command}\r\n"));

#if DEBUG
                Debug.WriteLine($"Out: {command}");
#endif

                _promptArived.Token.WaitHandle.WaitOne(1000, true);

                // write data in chunks of 64 bytes because of UART buffer size
                int index = 0;
                const int ChunkSize = 64;
                int bytesToSend;
                SpanByte toSend = bytes;
                while (index < bytes.Length)
                {
                    bytesToSend = (int)(bytes.Length - index);
                    if (bytesToSend > ChunkSize)
                    {
                        bytesToSend = ChunkSize;
                    }

                    Channel.SendBytesWithoutAck(toSend.Slice(index, bytesToSend).ToArray());
                    index += bytesToSend;
                }
            }
            catch
            {
                // Nothing on purpose
                success = false;
            }

            return success;
        }
    }
}
