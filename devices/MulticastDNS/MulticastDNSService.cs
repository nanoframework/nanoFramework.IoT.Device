// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using Iot.Device.MulticastDNS.Entities;
using Iot.Device.MulticastDNS.EventArgs;

namespace Iot.Device.MulticastDNS
{
    /// <summary>
    /// Multicast DNS (mDNS) is a computer networking protocol that resolves hostnames to IP addresses within local networks.
    /// </summary>
    public sealed class MulticastDnsService : IDisposable
    {
        private const string MulticastDnsAddress = "224.0.0.251";
        private const int MulticastDnsPort = 5353;

        private bool _listening = false;
        private IPAddress _multicastAddress;
        private UdpClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="MulticastDnsService" /> class.
        /// </summary>
        public MulticastDnsService()
        {
            _multicastAddress = IPAddress.Parse(MulticastDnsAddress);
            _client = new(new IPEndPoint(IPAddress.Any, MulticastDnsPort));
        }

        /// <summary>
        /// Start the worker thread that will listen for Multicast packets.
        /// </summary>
        public void Start()
        {
            if (!_listening)
            {
                _listening = true;
                new Thread(Run).Start();
            }
        }

        /// <summary>
        /// Stop the worker thread that is listening for Multicast packets.
        /// </summary>
        public void Stop() => _listening = false;

        /// <summary>
        /// The delegate that will be invoked when a Multicast DNS message is received.
        /// </summary>
        /// <param name="sender">The MulticastDNSService instance that received the message.</param>
        /// <param name="e">The MessageReceivedEventArgs containing the received message.</param>
        public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);

        /// <summary>
        /// The event that is raised when a Multicast DNS message is received.
        /// </summary>
        public event MessageReceivedEventHandler MessageReceived;

        /// <summary>
        /// Sends a Multicast DNS message.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        public void Send(Message message) => _client.Send(message.GetBytes(), new(_multicastAddress, MulticastDnsPort));

        private void Run()
        {
            _client.JoinMulticastGroup(_multicastAddress);

            IPEndPoint multicastEndpoint = new(_multicastAddress, MulticastDnsPort);
            IPEndPoint remoteEndpoint = new(IPAddress.Any, 0);

            byte[] buffer = new byte[2048];

            while (_listening)
            {
                int length = _client.Receive(buffer, ref remoteEndpoint);
                if (length == 0)
                {
                    continue;
                }

                Message msg = new(buffer);

                if (msg != null)
                {
                    MessageReceivedEventArgs eventArgs = new(msg);

                    MessageReceived?.Invoke(this, eventArgs);

                    if (eventArgs.Response != null)
                    {
                        _client.Send(eventArgs.Response.GetBytes(), multicastEndpoint);
                    }
                }
            }

            _client.DropMulticastGroup(_multicastAddress);
        }

        /// <summary>
        /// Dispose the Multicast DNS Service which causes it to stop listening.
        /// </summary>
        public void Dispose()
        {
            Stop();

            _client.Close();
            _client.Dispose();
        }
    }
}
