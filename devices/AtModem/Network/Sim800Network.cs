// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO.Ports;
using System.Threading;
using IoT.Device.AtModem.DTOs;
using IoT.Device.AtModem.Events;
using IoT.Device.AtModem.Modem;
using UnitsNet;

namespace IoT.Device.AtModem.Network
{
    /// <summary>
    /// A network interface for SIM800.
    /// </summary>
    public class Sim800Network : INetwork
    {
        private readonly ModemBase _modem;
        private PersonalIdentificationNumber _pin;
        private AccessPointConfiguration _apn;

        /// <inheritdoc/>
        public event INetwork.ApplicationNetworkEventHandler ApplicationNetworkEvent;

        internal Sim800Network(ModemBase modem)
        {
            _modem = modem;
            _modem.GenericEvent += ModemGenericEvent;
        }

        private void ModemGenericEvent(object sender, GenericEventArgs e)
        {
            if (e.Message.Contains("+SAPBR:"))
            {
                string line = e.Message.Substring(8);
                var parts = line.Split(':');
                if (parts.Length >= 2)
                {
                    if ((parts[0] == "1") && parts[1].Contains("DEACT"))
                    {
                        IsConnected = false;
                        ApplicationNetworkEvent?.Invoke(this, new ApplicationNetworkEventArgs(false));

                        if (AutoReconnect)
                        {
                            _modem.Channel.SendCommand("AT+SAPBR=1,1");
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public NetworkInformation NetworkInformation
        {
            get
            {
                NetworkInformation networkInformation = new NetworkInformation()
                {
                    NetworkOperator = string.Empty,
                    SignalQuality = new SignalStrength(Ratio.FromPercent(99), Ratio.FromPercent(99)),
                    IPAddress = string.Empty,
                };
                AtResponse response = _modem.Channel.SendCommandReadSingleLine("AT+COPS?", "+COPS");

                if (response.Success)
                {
                    string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                    var parts = line.Split(',');
                    if (parts.Length > 2)
                    {
                        networkInformation.NetworkOperator = parts[2].Trim('"');
                    }
                }

                var signqual = _modem.GetSignalStrength();
                if (signqual.IsSuccess)
                {
                    networkInformation.SignalQuality = (SignalStrength)signqual.Result;
                }

                var ipAddress = GetIpAddress();
                if (IsValidIpAddress(ipAddress))
                {
                    networkInformation.IPAddress = ipAddress;
                }

                networkInformation.ConnectionStatus = IsConnected && IsValidIpAddress(ipAddress) ? ConnectionStatus.Connected : ConnectionStatus.Disconnected;
                return networkInformation;
            }
        }

        /// <inheritdoc/>
        public bool IsConnected { get; internal set; } = false;

        /// <inheritdoc/>
        public bool AutoReconnect { get; set; }

        /// <inheritdoc/>
        public bool Reconnect()
        {
            Disconnect();
            return ConnectInternal();
        }

        /// <inheritdoc/>
        public bool Connect(PersonalIdentificationNumber pin = null, AccessPointConfiguration apn = null, int maxRetry = 10)
        {
            _apn = apn;
            _pin = pin;
            return ConnectInternal(maxRetry);
        }

        /// <inheritdoc/>
        private bool ConnectInternal(int maxRetry = 10)
        {
            // Disconnect in all cases connected
            _modem.Channel.SendCommand("AT+CIPSHUT");
            IsConnected = false;

        // Check the pin status
        RetryStatus:
            // Check if the PIN is ready
            var status = _modem.GetSimStatus();
            if (!status.IsSuccess)
            {
                if (maxRetry-- > 0)
                {
                    Thread.Sleep(1000);
                    goto RetryStatus;
                }

                return false;
            }

            // Get the status of the pin
            // Reconnect if needed
            if ((SimStatus)status.Result != SimStatus.SIM_READY)
            {
                if (_pin != null)
                {
                RetryPin:
                    status = _modem.GetSimStatus();
                    if (!status.IsSuccess)
                    {
                        if (maxRetry-- > 0)
                        {
                            goto RetryPin;
                        }

                        return false;
                    }

                    if ((SimStatus)status.Result != SimStatus.SIM_READY)
                    {
                        status = _modem.EnterSimPin(_pin);
                        if (!status.IsSuccess)
                        {
                            if (maxRetry-- > 0)
                            {
                                goto RetryPin;
                            }

                            return false;
                        }
                    }
                }
            }

            // Set multi connection to transparent and the modefor sending data
            var response = _modem.Channel.SendCommand("AT+CIPMUX=1;+CIPQSEND=1", TimeSpan.FromSeconds(85));

            // Then set the access point configuration
            if (_apn != null)
            {
            RetryApn:
                if (string.IsNullOrEmpty(_apn.UserName) && string.IsNullOrEmpty(_apn.Password))
                {
                    response = _modem.Channel.SendCommand($"AT+CSTT=\"{_apn.AccessPointName}\"", TimeSpan.FromSeconds(85));
                }
                else
                {
                    response = _modem.Channel.SendCommand($"AT+CSTT=\"{_apn.AccessPointName}\"{(_apn.UserName)}\",\"{_apn.Password}\"", TimeSpan.FromSeconds(85));
                }

                if (!response.Success)
                {
                    if (maxRetry-- > 0)
                    {
                        Thread.Sleep(2000);
                        goto RetryApn;
                    }

                    return false;
                }

                // Set the bearer configuration
                _modem.Channel.SendCommand($"AT+SAPBR=3,1,\"Contype\",\"GPRS\"");
                _modem.Channel.SendCommand($"AT+SAPBR=3,1,\"APN\",\"{_apn.AccessPointName}\"");
                _modem.Channel.SendCommand($"AT+SAPBR=3,1,\"USER\",\"{_apn.UserName}\"");
                _modem.Channel.SendCommand($"AT+SAPBR=3,1,\"PWD\",\"{_apn.Password}\"");
            }

            // Then connect to the network
            response = _modem.Channel.SendCommand("AT+CIICR", TimeSpan.FromSeconds(85));
            if (!response.Success)
            {
                return false;
            }

            // Connect to get an IP address
            response = _modem.Channel.SendCommand("AT+SAPBR=1,1");
            if (!response.Success)
            {
                return false;
            }

        RetryIp:
            if (!IsValidIpAddress(GetIpAddress()))
            {
                if (maxRetry-- > 0)
                {
                    Thread.Sleep(2000);
                    goto RetryIp;
                }
            }

            IsConnected = true;
            ApplicationNetworkEvent?.Invoke(this, new ApplicationNetworkEventArgs(IsConnected));
            return true;
        }

        /// <inheritdoc/>
        public bool Disconnect()
        {
            var response = _modem.Channel.SendCommand("AT+SAPBR=0,1");
            IsConnected = false;
            return response.Success;
        }

        /// <inheritdoc/>
        public Operator[] GetOperators()
        {
            // Timeout is 120 seconds
            AtResponse response = _modem.Channel.SendCommandReadSingleLine("AT+COPS=?", "+COPS", TimeSpan.FromMinutes(5));
            if (response.Success)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                if (line.Contains("+COPS: "))
                {
                    line = line.Substring(7);

                    // Format is like this: (1,"F-Bouygues Telecom","BYTEL","20820",9),(1,"Orange F","Orange","20801",7),(1,"F-Bouygues Telecom","BYTEL","20820",7),(1,"F SFR","SFR","20810",9),(1,"F SFR","SFR","20810",7),,(0,1,2,3,4),(0,1,2)
                    // we need only the left part of the ,, then split by ,
                    if (line.Contains("),,("))
                    {
                        line = line.Substring(0, line.IndexOf("),,(") + 1);
                    }

                    var parts = line.Split(')');
                    Operator[] ope = new Operator[parts.Length - 1];
                    for (int i = 0; i < parts.Length - 1; i++)
                    {
                        var elements = parts[i].Trim(',').Trim('(').Split(',');
                        ope[i] = new Operator()
                        {
                            OperatorType = (OperatorType)int.Parse(elements[0]),
                            Name = elements[1].Trim('"'),
                            ShortName = elements[2].Trim('"'),
                            Format = elements[3].Trim('"'),
                            SystemMode = SystemMode.GPRS,
                        };
                    }

                    return ope;
                }
            }

            return null;
        }

        private string GetIpAddress()
        {
            var response = _modem.Channel.SendCommandReadSingleLine("AT+SAPBR=2,1", "+SAPBR");
            if (response.Success)
            {
                var line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                var parts = line.Split(',');
                if (parts.Length > 2)
                {
                    return parts[2].Trim('"');
                }
            }

            return string.Empty;
        }

        private bool IsValidIpAddress(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
            {
                return false;
            }

            var parts = ipAddress.Split('.');
            if (parts.Length != 4)
            {
                return false;
            }

            bool allZero = true;
            foreach (var part in parts)
            {
                if (!byte.TryParse(part, out byte value))
                {
                    return false;
                }

                if (value != 0)
                {
                    allZero = false;
                }
            }

            return !allZero;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _modem.GenericEvent -= ModemGenericEvent;
            AutoReconnect = false;
            Disconnect();
        }
    }
}
