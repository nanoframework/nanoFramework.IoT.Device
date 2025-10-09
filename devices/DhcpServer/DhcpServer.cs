// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Iot.Device.DhcpServer.Enums;

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
        public string CaptivePortalUrl { get; set; }

        /// <summary>
        /// Gets or sets the DNS server to be used by clients. If set to <see cref="IPAddress.Any"/>, this will be ignored.
        /// </summary>
        public IPAddress DnsServer { get; set; } = IPAddress.Any;

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

        private void CheckAndCleanList(object state)
        {
            ArrayList toRemove = new ArrayList();
            for (int i = 0; i < _dhcpLastRequest.Count; i++)
            {
                if ((DateTime)_dhcpLastRequest[i] < DateTime.UtcNow)
                {
                    toRemove.Add(i);
                }
            }

            foreach (int val in toRemove)
            {
                _dhcpIpList.RemoveAt(val);
                _dhcpHardwareAddressList.RemoveAt(val);
                _dhcpLastRequest.RemoveAt(val);
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
                        DhcpMessage dhcpReq = new DhcpMessage();
                        dhcpReq.Parse(ref buffer);

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

                                var offer = dhcpReq.Offer(new IPAddress(yourIp), _mask, _ipAddress, GetAdditionalOptions());
                                _sender.Send(offer);

                                break;

                            case DhcpMessageType.Request:
                                // Check the request is for us
                                var dhcpRequsted = dhcpReq.GetOption(DhcpOptionCode.DhcpAddress);
                                if ((dhcpRequsted != null) && (dhcpRequsted.ToString() != _ipAddress.GetAddressBytes().ToString()))
                                {
                                    // Not for us
                                    break;
                                }

                                if (!_dhcpIpList.Contains(dhcpReq.RequestedIpAddress))
                                {
                                    _dhcpIpList.Add(dhcpReq.RequestedIpAddress);
                                    _dhcpHardwareAddressList.Add(dhcpReq.ClientHardwareAddressAsString);
                                    _dhcpLastRequest.Add(DateTime.UtcNow);
                                }
                                else
                                {
                                    // Find the requested address in the list
                                    int inc;
                                    for (inc = 0; inc < _dhcpIpList.Count; inc++)
                                    {
                                        if (((IPAddress)_dhcpIpList[inc]).ToString() == dhcpReq.RequestedIpAddress.ToString())
                                        {
                                            break;
                                        }
                                    }

                                    // Check if the hardware address is the same
                                    if ((string)_dhcpHardwareAddressList[inc] == dhcpReq.ClientHardwareAddressAsString)
                                    {
                                        _dhcpLastRequest[inc] = DateTime.UtcNow;
                                    }
                                    else
                                    {
                                        // In this case make a Nack
                                        _sender.Send(dhcpReq.NotAcknoledge());
                                        break;
                                    }
                                }

                                // Finaly send the acknoledge
                                _sender.Send(dhcpReq.Acknowledge(dhcpReq.RequestedIpAddress, _mask, _ipAddress, GetAdditionalOptions()));
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

        private byte[] GetAdditionalOptions()
        {
            // this is the preamble for options: 
            // 1 byte for option code
            // 1 byte for length
            const byte OptionPreamble = 2;
            const byte IpAddressOptionLength = 4;

            byte[] additionalOptions = null;
            byte[] encodedCaptivePortal = !string.IsNullOrEmpty(CaptivePortalUrl) ? Encoding.UTF8.GetBytes(CaptivePortalUrl) : new byte[0];

            // compute length of additional options
            int length = 0;
            int optionsIndex = 0;

            if (DnsServer != IPAddress.Any)
            {
                length += OptionPreamble;
                length += IpAddressOptionLength;
            }

            if (encodedCaptivePortal.Length > 0)
            {
                length += OptionPreamble;
                length += encodedCaptivePortal.Length;
            }

            // if there are options to send, add the terminating option
            if (length > 0)
            {
                additionalOptions = new byte[length];

                if (DnsServer != IPAddress.Any)
                {
                    // add DNS server option
                    additionalOptions[optionsIndex++] = (byte)DhcpOptionCode.DomainNameServer;
                    additionalOptions[optionsIndex++] = IpAddressOptionLength;
                    Array.Copy(DnsServer.GetAddressBytes(), 0, additionalOptions, optionsIndex, IpAddressOptionLength);

                    optionsIndex += IpAddressOptionLength;
                }

                if (encodedCaptivePortal.Length > 0)
                {
                    // add Captive Portal option
                    additionalOptions[optionsIndex++] = (byte)DhcpOptionCode.CaptivePortal;
                    additionalOptions[optionsIndex++] = (byte)encodedCaptivePortal.Length;
                    Array.Copy(encodedCaptivePortal, 0, additionalOptions, optionsIndex, encodedCaptivePortal.Length);
                    optionsIndex += encodedCaptivePortal.Length;
                }
            }

            return additionalOptions;
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
