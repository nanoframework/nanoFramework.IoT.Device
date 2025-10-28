// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.DhcpServer.Enums;
using Iot.Device.DhcpServer.Options;
using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Iot.Device.DhcpServer
{
    /// <summary>
    /// A DHCP Server class.
    /// </summary>
    public class DhcpServer : IDisposable
    {
        // Constants
        private const int DhcpPort = 67;
        private const int DhcpClientPort = 68;

        private static Socket _dhcplistener;
        private static Socket _sender;

        private readonly OptionCollection _options = new();

        private ArrayList _dhcpIpList;
        private ArrayList _dhcpHardwareAddressList;
        private ArrayList _dhcpLastRequest;
        private Thread _dhcpServerThread;
        private bool _islistening;
        private IPAddress _ipAddress;
        private IPAddress _mask;
        private Timer _timer;
        private ushort _timeToLeave;

        /// <summary>
        /// Gets or sets the captive portal URL. If null or empty, this will be ignored.
        /// </summary>
        public string CaptivePortalUrl
        {
            get => _options.GetOrDefault(DhcpOptionCode.CaptivePortal, string.Empty);

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _options.Remove(DhcpOptionCode.CaptivePortal);
                }
                else
                {
                    _options.Add(new StringOption(DhcpOptionCode.CaptivePortal, value!));
                }
            }
        }

        /// <summary>
        /// Gets or sets the DNS server to be used by clients. If set to <see cref="IPAddress.Any"/>, this will be ignored.
        /// </summary>
        public IPAddress DnsServer
        {
            get
            {
                var dnsServer = _options.GetOrDefault(DhcpOptionCode.DomainNameServer, IPAddress.Any);
                return !IPAddress.Any.Equals(dnsServer) ? dnsServer : null;
            }

            set
            {
                if (value is null || IPAddress.Any.Equals(value))
                {
                    _options.Remove(DhcpOptionCode.DomainNameServer);
                }
                else
                {
                    _options.Add(new IPAddressOption(DhcpOptionCode.DomainNameServer, value));
                }
            }
        }

        /// <summary>
        /// Gets or sets the gateway to be used by clients. If set to <see cref="IPAddress.Any"/>, this will be ignored.
        /// </summary>
        public IPAddress Gateway
        {
            get
            {
                var gateway = _options.GetOrDefault(DhcpOptionCode.Router, IPAddress.Any);
                return !IPAddress.Any.Equals(gateway) ? gateway : null;
            }

            set
            {
                if (value is null || IPAddress.Any.Equals(value))
                {
                    _options.Remove(DhcpOptionCode.Router);
                }
                else
                {
                    _options.Add(new IPAddressOption(DhcpOptionCode.Router, value));
                }
            }
        }

        /// <summary>
        /// Starts the DHCP Server to start listning.
        /// </summary>
        /// <returns>Returns false in case of error.</returns>
        /// <param name="address">The server IP address.</param>
        /// <param name="mask">The mask used for distributing the IP addess.</param>
        /// <param name="timeToLeave">Default time to leave for bail expiration.</param>
        /// <exception cref="SocketException">Socket exception occurred.</exception>
        /// <exception cref="Exception">An exception occured while setting up the DHCP listner or sender.</exception>
        public bool Start(IPAddress address, IPAddress mask, ushort timeToLeave = 1200)
        {
            if (_dhcplistener == null)
            {
                try
                {
                    // listen socket
                    _dhcplistener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    IPAddress dsip = new IPAddress(0xFFFFFFFF);
                    IPEndPoint ep = new IPEndPoint(dsip, DhcpPort);
                    _dhcplistener.Bind(ep);
                    _ipAddress = address;
                    _mask = mask;

                    // send socket
                    _sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    _sender.Bind(new IPEndPoint(_ipAddress, 0));
                    _sender.SetSocketOption(SocketOptionLevel.Udp, SocketOptionName.Broadcast, true);
                    _sender.Connect(new IPEndPoint(IPAddress.Parse("255.255.255.255"), DhcpClientPort));

                    // make a dynamic ip pool
                    _dhcpIpList = new ArrayList();
                    _dhcpIpList.Add(_ipAddress);
                    _dhcpHardwareAddressList = new ArrayList();
                    _dhcpHardwareAddressList.Add("ESP32");
                    _dhcpLastRequest = new ArrayList();

                    // This one never ever expires
                    _dhcpLastRequest.Add(DateTime.MaxValue);
                    _timeToLeave = timeToLeave;
                    _timer = new Timer(CheckAndCleanList, null, timeToLeave * 1000, timeToLeave * 1000);

                    // start server thread
                    _dhcpServerThread = new Thread(RunServer);
                    _dhcpServerThread.Start();

                    return true;
                }
                catch (SocketException ex)
                {
                    Debug.WriteLine($"DHCP: ** Socket exception occurred: {ex.Message} error code {ex.ErrorCode}!**");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"DHCP: ** Exception occurred: {ex.Message}!**");
                }

                return false;
            }

            // It's already started
            return true;
        }

        private readonly object _dhcpListLock = new object();

        private void CheckAndCleanList(object state)
        {
            lock (_dhcpListLock)
            {
                // Remove items in reverse order to maintain correct indices
                for (int i = _dhcpLastRequest.Count - 1; i >= 0; i--)
                {
                    if ((DateTime)_dhcpLastRequest[i] < DateTime.UtcNow.AddSeconds(-_timeToLeave))
                    {
                        _dhcpIpList.RemoveAt(i);
                        _dhcpHardwareAddressList.RemoveAt(i);
                        _dhcpLastRequest.RemoveAt(i);

                        Debug.WriteLine($"Removing expired lease for IP {(IPAddress)_dhcpIpList[i]}");
                    }
                }
            }
        }

        /// <summary>
        /// Stops the listening.
        /// </summary>
        public void Stop()
        {
            _islistening = false;
        }

        private void RunServer()
        {
            _islistening = true;

            // setup buffer to read data from socket
            byte[] buffer = new byte[1024];

            while (_islistening)
            {
                try
                {
                    // wait for the next packet from the listener
                    int bytes = _dhcplistener.Receive(buffer);

                    // only parse the message in case we have bytes
                    if (bytes > 0)
                    {
                        DhcpMessage dhcpReq = MessageBuilder.Parse(buffer);

                        // debug information
                        Debug.WriteLine(dhcpReq.ToString());

                        // we only respond to requests
                        if (dhcpReq.OperationCode != DhcpOperation.BootRequest)
                        {
                            continue;
                        }

                        switch (dhcpReq.DhcpMessageType)
                        {
                            case DhcpMessageType.Discover:

                                if (!dhcpReq.GatewayIPAddress.Equals(IPAddress.Any))
                                {
                                    // We only respond to requests on our subnet
                                    return;
                                }

                                if (_dhcpIpList.Count > 254)
                                {
                                    // No more available IP Address
                                    break;
                                }

                                byte[] yourIp;

                                // Do we have an option asking for a specific IP address?
                                var reqIp = dhcpReq.RequestedIpAddress;
                                if (reqIp != new IPAddress(0))
                                {
                                    // We do have a request for an IP, maybe it was connected before
                                    if (_dhcpIpList.Contains(reqIp))
                                    {
                                        yourIp = reqIp.GetAddressBytes();
                                    }
                                    else
                                    {
                                        yourIp = GetFirstAvailableIp();
                                    }
                                }
                                else
                                {
                                    yourIp = GetFirstAvailableIp();
                                }

                                dhcpReq.SecondsElapsed = _timeToLeave;

                                _sender.Send(MessageBuilder.CreateOffer(dhcpReq, _ipAddress, new IPAddress(yourIp), _mask, _options).GetBytes());

                                break;

                            case DhcpMessageType.Request:

                                var serverIdentifier = dhcpReq.ServerIdentifier;

                                if (serverIdentifier.Equals(IPAddress.Any))
                                {
                                    if (dhcpReq.ClientIPAddress.Equals(IPAddress.Any))
                                    {
                                        Debug.WriteLine("Received REQUEST without ciaddr, client is INIT-REBOOT");

                                        if (!_dhcpIpList.Contains(dhcpReq.RequestedIpAddress))
                                        {
                                            _dhcpIpList.Add(dhcpReq.RequestedIpAddress);
                                            _dhcpHardwareAddressList.Add(dhcpReq.ClientHardwareAddressAsString);
                                            _dhcpLastRequest.Add(DateTime.UtcNow);
                                        }

                                        _sender.Send(MessageBuilder.CreateAck(dhcpReq, _ipAddress, dhcpReq.RequestedIpAddress, _mask, _options).GetBytes());
                                    }
                                    else
                                    {
                                        Debug.WriteLine($"Received REQUEST with ciaddr, client is RENEWING or REBINDING");

                                        // Find the requested address in the list
                                        lock (_dhcpListLock)
                                        {
                                            // Find the requested address in the list
                                            int inc = -1;
                                            for (int i = 0; i < _dhcpIpList.Count; i++)
                                            {
                                                if (((IPAddress)_dhcpIpList[i]).ToString() == dhcpReq.RequestedIpAddress.ToString())
                                                {
                                                    inc = i;
                                                    break;
                                                }
                                            }

                                            // Verify we found the IP and the hardware address matches
                                            if (inc >= 0 && inc < _dhcpHardwareAddressList.Count)
                                            {
                                                if ((string)_dhcpHardwareAddressList[inc] == dhcpReq.ClientHardwareAddressAsString)
                                                {
                                                    _dhcpLastRequest[inc] = DateTime.UtcNow;
                                                    _sender.Send(MessageBuilder.CreateAck(dhcpReq, _ipAddress, dhcpReq.RequestedIpAddress, _mask, _options).GetBytes());
                                                }
                                                else
                                                {
                                                    // Hardware address mismatch - send NAK
                                                    _sender.Send(MessageBuilder.CreateNak(dhcpReq, _ipAddress).GetBytes());
                                                }
                                            }
                                            else
                                            {
                                                // IP not found in our list - send NAK
                                                Debug.WriteLine($"Received RENEW/REBIND for unknown IP {dhcpReq.RequestedIpAddress}");

                                                _sender.Send(MessageBuilder.CreateNak(dhcpReq, _ipAddress).GetBytes());
                                            }
                                        }

                                        break;
                                    }

                                }
                                else if (serverIdentifier.Equals(_ipAddress))
                                {
                                    Debug.WriteLine("Received REQUEST with server identifier, client is SELECTING");

                                    _sender.Send(MessageBuilder.CreateAck(dhcpReq, _ipAddress, dhcpReq.RequestedIpAddress, _mask, _options).GetBytes());
                                }

                                break;

                            default:
                                Debug.WriteLine($"DHCP: not handled ({dhcpReq.DhcpMessageType}), lenght is: {bytes}, from host: {dhcpReq.HostName}");
                                break;
                        }
                    }
                    else
                    {
                        // free cpu time if no bytes in socket
                        Thread.Sleep(200);
                    }
                }
                catch
                {
                    //// Just pass this, we want to make sure that this loop always works properly.
                }
            }

            try
            {
                _dhcplistener.Close();
                _sender.Close();
            }
            catch
            {
                //// Make sure we catch everything coming in
            }

            _dhcplistener = null;
            _sender = null;
            Debug.WriteLine($"DHCP: stoped");
        }

        private byte[] GetFirstAvailableIp()
        {
            // increment dynamic ip
            byte[] yourIp = ((IPAddress)_dhcpIpList[0]).GetAddressBytes();

            foreach (IPAddress ip in _dhcpIpList)
            {
                yourIp[3]++;
                if (ip.GetAddressBytes()[3] == yourIp[3])
                {
                    yourIp[3]++;
                }

                if (yourIp[3] == 255)
                {
                    yourIp[3] = 1;
                }
            }

            return yourIp;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Stop();
        }
    }
}
