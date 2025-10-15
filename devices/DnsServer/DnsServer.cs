// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;

#if DEBUG
using nanoFramework.Logging.Debug;
#endif

namespace Iot.Device.DnsServer
{
    /// <summary>
    /// Creates a simple DNS server that listens for DNS queries and responds based on a predefined list of DNS entries.
    /// </summary>
    public class DnsServer : IDisposable
    {
        /// <summary>
        /// The standard DNS port number.
        /// </summary>
        private const int DnsPort = 53;

        // per RFC 1035, the maximum size of a DNS UDP packet is 512 bytes
        private const ushort MaxDnsUdpPacketSize = 512;

        private bool _disposed;

        private Socket _listenerSocket;
        private Thread _dnsListener;
        private bool _started;

        /// <summary>
        /// Gets the server address.
        /// </summary>
        public IPAddress ServerAddress { get; }

        private Hashtable _dnsEntries;

        /// <summary>
        /// Gets the list of DNS entries the server will use to respond to queries. 
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when attempting to modify entries while the server is running.</exception>
        public Hashtable DnsEntries
        {
            get => _dnsEntries;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DnsServer"/> class.
        /// </summary>
        /// <param name="serverAddress">The IP address the DNS server will bind to.</param>
        public DnsServer(IPAddress serverAddress)
            : this(serverAddress, new DnsEntry[0], null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DnsServer"/> class.
        /// </summary>
        /// <param name="serverAddress">The IP address the DNS server will bind to.</param>
        /// <param name="dnsEntries">The list of DNS entries the server will use to respond to queries.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> for logging purposes.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="serverAddress"/> or <paramref name="dnsEntries"/> is <see langword="null"/>.</exception>
        public DnsServer(
            IPAddress serverAddress,
            DnsEntry[] dnsEntries,
            ILogger? logger = null)
        {
            if (serverAddress is null)
            {
                throw new ArgumentNullException();
            }

            if (dnsEntries is null)
            {
                throw new ArgumentNullException();
            }

            // Set the global logger if provided
            if (logger != null)
            {
                Logger.GlobalLogger = logger;
            }
#if DEBUG
            else
            {
                Logger.GlobalLogger = new DebugLogger("[DNS Server]");
                ((DebugLogger)Logger.GlobalLogger).MinLogLevel = LogLevel.Trace;
            }
#endif

            ServerAddress = serverAddress;

            ParseDnsEntries(dnsEntries);
        }

        /// <summary>
        /// Starts the DNS server.
        /// </summary>
        /// <returns><see langword="true"/> if the server started successfully; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no DNS entries are defined.</exception>
        public bool Start()
        {
            if (!_started)
            {
                // check if we have entries
                if (_dnsEntries.Count == 0)
                {
                    Logger.GlobalLogger.LogWarning("No DNS entries defined. The server will not respond to any queries.");

                    throw new InvalidOperationException();
                }

                try
                {
                    // setup listener socket
                    _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    _listenerSocket.Bind(new IPEndPoint(ServerAddress, DnsPort));

                    // start DNS listener thread
                    _dnsListener = new Thread(DnsWorkerThread);

                    // set flag
                    _started = true;

                    // now start the thread
                    _dnsListener.Start();

                    return true;
                }
                catch (SocketException ex)
                {
                    Logger.GlobalLogger.LogError(ex, "Socket exception occurred. Code: {0} Message: {1}", ex.ErrorCode, ex.Message);
                }
                catch (Exception ex)
                {
                    Logger.GlobalLogger.LogError(ex, "Exception occurred. Message: {0}", ex.Message);
                }

                // reached here, something went wrong
                Stop();
            }

            return _started;
        }

        /// <summary>
        /// Stops the DNS server.
        /// </summary>
        public void Stop()
        {
            // clear flag to stop the loop in the thread
            _started = false;

            if (_listenerSocket is not null)
            {
                try
                {
                    // close the socket
                    _listenerSocket.Close();

                    // allow time for the socket to close and the thread to exit
                    Thread.Sleep(200);

                    _listenerSocket = null;
                }
                catch (Exception ex)
                {
                    Logger.GlobalLogger.LogError(ex, "Exception occurred while stopping the server. Message: {0}", ex.Message);
                }
            }

            // stop the thread
            if (_dnsListener is not null && _dnsListener.IsAlive)
            {
                _dnsListener.Join(1000);

                if (_dnsListener.IsAlive)
                {
                    _dnsListener.Abort();
                }

                _dnsListener = null;
            }
        }

        private void DnsWorkerThread()
        {
            Logger.GlobalLogger.LogInformation("DNS server started.");

            byte[] buffer = new byte[MaxDnsUdpPacketSize];

            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            Thread.Sleep(500);

            while (_started)
            {
                try
                {
                    // receive data
                    int receivedBytes = _listenerSocket.ReceiveFrom(buffer, ref remoteEndPoint);

                    if (receivedBytes > 0)
                    {
                        // process DNS request
                        byte[] requestData = new byte[receivedBytes];
                        Array.Copy(buffer, requestData, receivedBytes);

                        DnsReply dnsReply = null;

                        if (ProcessDnsRequest(
                            requestData,
                            ref dnsReply))
                        {
                            _listenerSocket.SendTo(dnsReply.GetBytes(), remoteEndPoint);
                        }
                    }
                }
                catch (SocketException ex)
                {
                    Logger.GlobalLogger.LogError(ex, "Socket exception in DNS worker thread. Code: {0} Message: {1}", ex.ErrorCode, ex.Message);

                    break;
                }
                catch (Exception ex)
                {
                    Logger.GlobalLogger.LogError(ex, "Exception in DNS worker thread. Message: {0}", ex.Message);
                }
            }

            Logger.GlobalLogger.LogInformation("DNS server stopped.");
        }

        private bool ProcessDnsRequest(byte[] requestData, ref DnsReply dnsReply)
        {
            Logger.GlobalLogger.LogDebug("Received DNS request of length {0} bytes.", requestData.Length);

            // Create the DNS reply with the request data and DNS entries
            dnsReply = new DnsReply(requestData, DnsEntries);

            // Parse the request and generate answers
            if (!dnsReply.ParseRequest())
            {
                Logger.GlobalLogger.LogError("Failed to parse DNS request.");

                return false;
            }

            if (dnsReply.Answers.Length == 0)
            {
                Logger.GlobalLogger.LogWarning("No DNS answers generated.");

                return false;
            }
            else
            {
                return true;
            }
        }

        #region Dispose pattern

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Stop();
                }

                _disposed = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        private void ParseDnsEntries(DnsEntry[] dnsEntries)
        {
            _dnsEntries = new Hashtable();

            foreach (DnsEntry entry in dnsEntries)
            {
                _dnsEntries.Add(entry.Name, entry.Address);

                Logger.GlobalLogger.LogDebug("Added DNS entry: {0} -> {1}", entry.Name, entry.Address);
            }
        }
    }
}
