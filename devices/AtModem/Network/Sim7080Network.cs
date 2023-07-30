// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO.Ports;
using System.Threading;
using IoT.Device.AtModem.DTOs;
using IoT.Device.AtModem.Modem;
using UnitsNet;

namespace IoT.Device.AtModem.Network
{
    /// <summary>
    /// A network interface for SIM7080.
    /// </summary>
    public class Sim7080Network : INetwork
    {
        private readonly ModemBase _modem;

        internal Sim7080Network(ModemBase modem)
        {
            _modem = modem;
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
                AtResponse response = _modem.Channel.SendSingleLineCommandAsync("AT+COPS?", "+COPS");

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
        public bool Connect(PersonalIdentificationNumber pin = null, AccessPointConfiguration apn = null, int maxRetry = 10)
        {
            AtResponse response;

        Retry:
            // set the APN if nay to set
            if (!string.IsNullOrEmpty(apn.AccessPointName))
            {
                // Disable the RF function
                response = _modem.Channel.SendCommand("AT+CFUN=0");
                if (!response.Success)
                {
                    return false;
                }

                response = _modem.Channel.SendCommand($"AT+CGDCONT=1,\"IP\",\"{apn.AccessPointName}\"");
                if (!response.Success)
                {
                    // Try to set back on regardless of the result
                    _modem.Channel.SendCommand("AT+CFUN=1");
                    return false;
                }

                response = _modem.Channel.SendCommand("AT+CFUN=1");
                if (!response.Success)
                {
                    return false;
                }

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
                    if (pin != null)
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
                            status = _modem.EnterSimPin(pin);
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

            // Check the quality of the signal and wait a bit if needed
            WaitForSignal:
                var signqual = _modem.GetSignalStrength();
                if (!signqual.IsSuccess || ((SignalStrength)signqual.Result).Rssi.Percent == 99)
                {
                    if (maxRetry-- > 0)
                    {
                        Thread.Sleep(1500);
                        goto WaitForSignal;
                    }

                    return false;
                }

                // Set the operator selection to automatic
                response = _modem.Channel.SendCommand("AT+COPS=0");
                if (!response.Success)
                {
                    if (maxRetry-- > 0)
                    {
                        Thread.Sleep(1000);
                        goto Retry;
                    }

                    return false;
                }

                // now connect with the apn, user and password
                response = _modem.Channel.SendCommand($"AT+CNCFG=0,1,\"{apn.AccessPointName}\"{(string.IsNullOrEmpty(apn.UserName) ? string.Empty : $",\"{apn.UserName}\"")}{(string.IsNullOrEmpty(apn.Password) ? string.Empty : $",\"{apn.Password}\"")}");
                if (!response.Success)
                {
                    if (maxRetry-- > 0)
                    {
                        goto Retry;
                    }

                    return false;
                }

                // We need to wait for the connection to be made
                Thread.Sleep(1500);
            }

            // Make the connection
            response = _modem.Channel.SendCommand("AT+CNACT=0,1");
            if (!response.Success)
            {
                if (maxRetry-- > 0)
                {
                    goto Retry;
                }

                return false;
            }

        RetryIp:
            // check if the connection is made
            var ipAddress = GetIpAddress();
            if (!IsValidIpAddress(ipAddress))
            {
                if (maxRetry-- > 0)
                {
                    Thread.Sleep(2000);
                    goto RetryIp;
                }

                return false;
            }

            IsConnected = true;
            return true;
        }

        private string GetIpAddress()
        {
            var response = _modem.Channel.SendSingleLineCommandAsync("AT+CNACT?", "+CNACT");
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
        public bool Disconnect()
        {
            var response = _modem.Channel.SendCommand("AT+CNACT=0,0");
            IsConnected = !response.Success;
            return response.Success;
        }

        /// <inheritdoc/>
        public Operator[] GetOperators()
        {
            // Timeout is 120 seconds
            AtResponse response = _modem.Channel.SendSingleLineCommandAsync("AT+COPS=?", "+COPS", TimeSpan.FromMinutes(5));
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
                            SystemMode = (SystemMode)int.Parse(elements[4]),
                        };
                    }

                    return ope;
                }
            }

            return null;
        }
    }
}
