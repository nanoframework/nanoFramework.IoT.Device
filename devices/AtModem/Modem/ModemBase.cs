﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text;
using IoT.Device.AtModem.Call;
using IoT.Device.AtModem.DTOs;
using IoT.Device.AtModem.Events;
using IoT.Device.AtModem.Network;
using IoT.Device.AtModem.Sms;
using nanoFramework.M2Mqtt;
using UnitsNet;

namespace IoT.Device.AtModem.Modem
{
    /// <summary>
    /// Represents a basic modem. All other modems should implement this class.
    /// </summary>
    public abstract class ModemBase : IDisposable
    {
        private readonly AtChannel _channel;
        private bool _isDisposed;
        private ISmsProvider _smsProvider;
        private ICall _genericCall;

        /// <summary>
        /// Initializes a new instance of the ModemBase class with the specified AT channel.
        /// </summary>
        /// <param name="channel">The AT channel used for communication with the modem.</param>
        public ModemBase(AtChannel channel)
        {
            _channel = channel;
            if (!_channel.IsOpen)
            {
                _channel.Open();
            }

            // TODO: Uncomment this when the event is implemented
            channel.UnsolicitedEvent += ChannelUnsolicitedEvent;
        }

        #region events

        private void ChannelUnsolicitedEvent(object sender, UnsolicitedEventArgs e)
        {
            if (e.Line1.StartsWith("+CUSD: "))
            {
                UssdResponseReceived?.Invoke(this, UssdResponseEventArgs.CreateFromResponse(e.Line1));
            }
            else if (AtErrorParsers.TryGetError(e.Line1, out Error error))
            {
                ErrorReceived?.Invoke(this, new IoT.Device.AtModem.Events.ErrorEventArgs(error.ToString()));
            }
            else
            {
                GenericEvent?.Invoke(this, new GenericEventArgs(e.Line1));
            }
        }

        /// <summary>
        /// Represents the method that will handle error events.
        /// </summary>
        /// <param name="sender">The source of the error event.</param>
        /// <param name="e">An instance of ErrorEventArgs containing error information.</param>
        public delegate void ErrorEventHandler(object sender, IoT.Device.AtModem.Events.ErrorEventArgs e);

        /// <summary>
        /// Occurs when an error is received.
        /// </summary>
        public event ErrorEventHandler ErrorReceived;

        /// <summary>
        /// Represents the method that will handle generic events.
        /// </summary>
        /// <param name="sender">The source of the generic event.</param>
        /// <param name="e">An instance of GenericEventArgs containing event data.</param>
        public delegate void GenericEventHandler(object sender, GenericEventArgs e);

        /// <summary>
        /// Occurs when a generic event is raised.
        /// </summary>
        public event GenericEventHandler GenericEvent;

        /// <summary>
        /// Represents the method that will handle USSD response events.
        /// </summary>
        /// <param name="sender">The source of the USSD response event.</param>
        /// <param name="e">An instance of UssdResponseEventArgs containing event data.</param>
        public delegate void UssdResponseEventHandler(object sender, UssdResponseEventArgs e);

        /// <summary>
        /// Occurs when a USSD response is received.
        /// </summary>
        public event UssdResponseEventHandler UssdResponseReceived;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the AT channel used to communicate with the modem.
        /// </summary>
        public AtChannel Channel
        {
            get => _channel;
        }

        /// <summary>
        /// Gets a <see cref="IFileStorage"/> object used to store files on the modem..
        /// </summary>
        public virtual IFileStorage FileStorage { get => throw new NotImplementedException(); }

        /// <summary>
        /// Gets a <see cref="IMqttClient"/> object used to communicate with an MQTT broker.
        /// </summary>
        public virtual IMqttClient MqttClient { get => throw new NotImplementedException(); }

        /// <summary>
        /// Gets a <see cref="INetwork"/> object used to communicate with the network.
        /// </summary>
        public virtual INetwork Network { get => throw new NotImplementedException(); }

        /// <summary>
        /// Gets a <see cref="ISmsProvider"/> object used to send and receive SMS messages and mget access to the SMS storage.
        /// </summary>
        public virtual ISmsProvider SmsProvider
        {
            get
            {
                if (_smsProvider == null)
                {
                    _smsProvider = new GenericSmsProvider(this);
                }

                return _smsProvider;
            }
        }

        /// <summary>
        /// Gets a <see cref="ICall"/> object used to make and receive calls.
        /// </summary>
        public virtual ICall Call
        {
            get
            {
                if (_genericCall == null)
                {
                    _genericCall = new GenericCall(this);
                }

                return _genericCall;
            }
        }

        #endregion 

        #region _V_25TER

        /// <summary>
        /// Gets the International Mobile Subscriber Identity (IMSI).
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> containing the IMSI if successful, or an error response.</returns>
        public virtual ModemResponse GetSimCardInformation()
        {
            SimCardInformation simCardInformation = new SimCardInformation();
            AtResponse response = Channel.SendSingleLineCommandAsync("AT+CIMI", string.Empty);

            if (!response.Success)
            {
                return ModemResponse.ResultError();
            }

            string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
            simCardInformation.Imsi = line;

            response = Channel.SendSingleLineCommandAsync("AT+CGSN", string.Empty);

            if (!response.Success)
            {
                return ModemResponse.ResultError();
            }

            line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
            simCardInformation.Imei = line;

            response = Channel.SendSingleLineCommandAsync("AT+CCID", string.Empty);

            if (!response.Success)
            {
                return ModemResponse.ResultError();
            }

            line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
            simCardInformation.Iccm = line;

            return ModemResponse.ResultSuccess(simCardInformation);
        }

        /// <summary>
        /// Disables echo mode.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> indicating the success or failure of the operation.</returns>
        public virtual ModemResponse DisableEcho()
        {
            AtResponse response = Channel.SendCommand("ATE0");
            return ModemResponse.Success(response.Success);
        }

        /// <summary>
        /// Gets the product identification information.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> containing the product identification information if successful, or an error response.</returns>
        public virtual ModemResponse GetProductIdentificationInformation()
        {
            AtResponse response = Channel.SendMultilineCommand("ATI", null);

            if (response.Success)
            {
                StringBuilder builder = new StringBuilder();
                foreach (string line in response.Intermediates)
                {
                    builder.AppendLine(line);
                }

                return ModemResponse.ResultSuccess(new ProductIdentificationInformation(builder.ToString()));
            }

            return ModemResponse.ResultError();
        }

        /// <summary>
        /// Gets the current baud rate.
        /// </summary>
        /// <param name="baudRate">The baud rate to be set.</param>
        /// <returns>A <see cref="ModemResponse"/> indicating the success or failure of the operation.</returns>
        public virtual ModemResponse SetBaudRate(int baudRate)
        {
            AtResponse response = Channel.SendCommand($"AT+IPR={baudRate}");
            return ModemResponse.Success(response.Success);
        }

        /// <summary>
        /// Gets the current baud rate.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> indicating the success or failure of the operation.</returns>
        public virtual ModemResponse GetCurrentBaudRate()
        {
            AtResponse response = Channel.SendSingleLineCommandAsync($"AT+IPR?", "+IPR:");

            if (response.Success)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                if (line.StartsWith("+IPR: "))
                {
                    int baudRate = int.Parse(line.Substring(6, line.Length - 7));
                    return ModemResponse.ResultSuccess(baudRate);
                }
            }

            return ModemResponse.ResultError();
        }

        #endregion

        #region _3GPP_TS_27_007

        /// <summary>
        /// Gets the available character sets.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> containing the available character sets if successful, or an error response.</returns>
        public virtual ModemResponse GetAvailableCharacterSets()
        {
            AtResponse response = Channel.SendSingleLineCommandAsync($"AT+CSCS=?", "+CSCS:");

            if (response.Success)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                ////var match = Regex.Match(line, @"\+CSCS:\s\((?:""(?<characterSet>\w+)"",*)+\)");
                if (line.StartsWith("+CSCS: "))
                {
                    string[] characterSets = line.Substring(8, line.Length - 9).Split(',');
                    for (int i = 0; i < characterSets.Length; i++)
                    {
                        characterSets[i] = characterSets[i].Trim('"');
                    }

                    return ModemResponse.ResultSuccess(characterSets);
                }
            }

            return ModemResponse.ResultError();
        }

        /// <summary>
        /// Gets the current character set.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> containing the current character set if successful, or an error response.</returns>
        public virtual ModemResponse GetCurrentCharacterSet()
        {
            AtResponse response = Channel.SendSingleLineCommandAsync($"AT+CSCS?", "+CSCS:");

            if (response.Success)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                ////var match = Regex.Match(line, @"""(?<characterSet>\w)""");
                if (line.StartsWith("+CSCS: "))
                {
                    string characterSet = line.Substring(8, line.Length - 9);
                    return ModemResponse.ResultSuccess(characterSet);
                }
            }

            return ModemResponse.ResultError();
        }

        /// <summary>
        /// Sets the character set.
        /// </summary>
        /// <param name="characterSet">The character set to be set.</param>
        /// <returns>A <see cref="ModemResponse"/> indicating the success or failure of the operation.</returns>
        public virtual ModemResponse SetCharacterSet(string characterSet)
        {
            AtResponse response = Channel.SendCommand($"AT+CSCS=\"{characterSet}\"");
            return ModemResponse.Success(response.Success);
        }

        /// <summary>
        /// Gets the manufacturer identification and other device information.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> indicating the success of the operation.</returns>
        public virtual ModemResponse GetDeviceInformation()
        {
            // We need to keep for all the commands the intermediate responses, because it contains the information we need
            AtResponse response = Channel.SendSingleLineCommandAsync("AT+CGMI", string.Empty);
            DeviceInformation deviceInformation = new DeviceInformation();

            if (!response.Success)
            {
                return ModemResponse.ResultError();
            }

            string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
            deviceInformation.Manufacturer = line;

            response = Channel.SendSingleLineCommandAsync("AT+CGMM", string.Empty);
            if (!response.Success)
            {
                return ModemResponse.ResultError();
            }

            line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
            deviceInformation.Model = line;

            response = Channel.SendSingleLineCommandAsync("AT+CGMR", string.Empty);
            if (!response.Success)
            {
                return ModemResponse.ResultError();
            }

            line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
            deviceInformation.FirmwareVersion = line;

            response = Channel.SendSingleLineCommandAsync("ATI", string.Empty);
            if (!response.Success)
            {
                return ModemResponse.ResultError();
            }

            line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
            deviceInformation.ProductNumber = line;

            return ModemResponse.ResultSuccess(deviceInformation);
        }

        /// <summary>
        /// Gets the status of the SIM card.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> containing the status of the SIM card.</returns>
        public virtual ModemResponse GetSimStatus()
        {
            AtResponse response = Channel.SendSingleLineCommandAsync("AT+CPIN?", "+CPIN:");

            if (!response.Success)
            {
                if (AtErrorParsers.TryGetError(response.FinalResponse, out Error cmeError))
                {
                    return ModemResponse.ResultError(cmeError.ToString());
                }
            }

            // CPIN? has succeeded, now look at the result
            string cpinLine = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
            ////var match = Regex.Match(cpinLine, @"\+CPIN:\s(?<pinresult>.*)");
            if (cpinLine.StartsWith("+CPIN: "))
            {
                string cpinResult = cpinLine.Substring(7);

                switch (cpinResult)
                {
                    case "SIM PIN":
                        return ModemResponse.ResultSuccess(SimStatus.SIM_PIN);
                    case "SIM PUK":
                        return ModemResponse.ResultSuccess(SimStatus.SIM_PUK);
                    case "PH-NET PIN":
                        return ModemResponse.ResultSuccess(SimStatus.SIM_NETWORK_PERSONALIZATION);
                    case "READY":
                        return ModemResponse.ResultSuccess(SimStatus.SIM_READY);
                    default:
                        // Treat unsupported lock types as "sim absent"
                        return ModemResponse.ResultSuccess(SimStatus.SIM_ABSENT);
                }
            }

            return ModemResponse.ResultError();
        }

        /// <summary>
        /// Enters the SIM PIN.
        /// </summary>
        /// <param name="pin">The Personal Identification Number (PIN) to enter.</param>
        /// <returns>A <see cref="ModemResponse"/> indicating the success of the operation.</returns>
        public virtual ModemResponse EnterSimPin(PersonalIdentificationNumber pin)
        {
            AtResponse response = Channel.SendCommand($"AT+CPIN={pin}");
            return ModemResponse.Success(response.Success);
        }

        /// <summary>
        /// Reinitializes the SIM.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> indicating the success of the operation.</returns>
        public virtual ModemResponse ReInitializeSim()
        {
            AtResponse response = Channel.SendCommand($"AT+CRFSIM");
            return ModemResponse.Success(response.Success);
        }

        /// <summary>
        /// Gets the signal strength.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> containing the signal strength information.</returns>
        public virtual ModemResponse GetSignalStrength()
        {
            AtResponse response = Channel.SendSingleLineCommandAsync("AT+CSQ", "+CSQ:");

            if (response.Success)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                ////var match = Regex.Match(line, @"\+CSQ:\s(?<rssi>\d+),(?<ber>\d+)");
                if (line.StartsWith("+CSQ: "))
                {
                    string[] parts = line.Substring(6).Split(',');
                    int rssi = int.Parse(parts[0]);
                    int ber = int.Parse(parts[1]);
                    return ModemResponse.ResultSuccess(new SignalStrength(Ratio.FromPercent(rssi), Ratio.FromPercent(ber)));
                }
            }

            return ModemResponse.ResultError();
        }

        /// <summary>
        /// Gets the battery status.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> containing the battery status information.</returns>
        public virtual ModemResponse GetBatteryStatus()
        {
            AtResponse response = Channel.SendSingleLineCommandAsync("AT+CBC", "+CBC:");

            if (response.Success)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                ////var match = Regex.Match(line, @"\+CBC:\s(?<bcs>\d+),(?<bcl>\d+)");
                if (line.StartsWith("+CBC: "))
                {
                    string[] parts = line.Substring(6).Split(',');
                    int bcs = 0;
                    int bcl = 0;
                    int voltage = 0;
                    if (parts.Length >= 1)
                    {
                        bcs = int.Parse(parts[0]);
                    }

                    if (parts.Length >= 2)
                    {
                        bcl = int.Parse(parts[1]);
                    }

                    if (parts.Length >= 3)
                    {
                        voltage = int.Parse(parts[2]);
                    }

                    return ModemResponse.ResultSuccess(new BatteryStatus((BatteryChargeStatus)bcs, Ratio.FromPercent(bcl), ElectricPotential.FromMillivolts(voltage)));
                }
            }

            return ModemResponse.ResultError();
        }

        /// <summary>
        /// Sets the date and time of the modem.
        /// </summary>
        /// <param name="value">The date and time value to set.</param>
        /// <returns>A <see cref="ModemResponse"/> indicating the success of the operation.</returns>
        public virtual ModemResponse SetDateTime(DateTime value)
        {
            var sb = new StringBuilder("AT+CCLK=\"");

            // We don't support time zones, so just use UTC
            int offsetQuarters = 0;
            sb.Append(value.ToString(@"yy/MM/dd,HH:mm:ss"));
            sb.Append(offsetQuarters.ToString("+00;-#"));
            sb.Append("\"");
            AtResponse response = Channel.SendCommand(sb.ToString());
            return ModemResponse.Success(response.Success);
        }

        /// <summary>
        /// Gets the date and time of the modem.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> containing the date and time value.</returns>
        public virtual ModemResponse GetDateTime()
        {
            AtResponse response = Channel.SendSingleLineCommandAsync("AT+CCLK?", "+CCLK:");

            if (response.Success)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                ////var match = Regex.Match(line, @"\+CCLK:\s""(?<year>\d\d)/(?<month>\d\d)/(?<day>\d\d),(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)(?<zone>[-+]\d\d)""");
                if (line.StartsWith("+CCLK: "))
                {
                    string[] parts = line.Substring(7).Split(',');
                    string[] dates = parts[0].Split('/');
                    int year = int.Parse(dates[0].Substring(1));
                    int month = int.Parse(dates[1]);
                    int day = int.Parse(dates[2]);
                    string[] times = parts[1].Split(':');
                    int hour = int.Parse(times[0]);
                    int minute = int.Parse(times[1]);
                    int second = int.Parse(times[2].Substring(0, 2));
                    int zone = int.Parse(times[2].Substring(2, 3));
                    DateTime time = new DateTime(2000 + year, month, day, hour, minute, second).Add(TimeSpan.FromMinutes(15 * zone));
                    return ModemResponse.ResultSuccess(time);
                }
            }

            return ModemResponse.ResultError();
        }

        /// <summary>
        /// Sends a USSD code.
        /// </summary>
        /// <param name="code">The USSD code to send.</param>
        /// <param name="codingScheme">The coding scheme to use (optional).</param>
        /// <returns>A <see cref="ModemResponse"/> indicating the success of the operation.</returns>
        public virtual ModemResponse SendUssd(string code, int codingScheme = 15)
        {
            AtResponse response = Channel.SendCommand($"AT+CUSD=1,\"{code}\",{codingScheme}");
            return ModemResponse.Success(response.Success);
        }
        #endregion

        /// <summary>
        /// Sets the error format of the modem.
        /// </summary>
        /// <param name="errorFormat">The error format to set.</param>
        /// <returns>A <see cref="ModemResponse"/> indicating the success of the operation.</returns>
        public virtual ModemResponse SetErrorFormat(int errorFormat)
        {
            AtResponse response = Channel.SendCommand($"AT+CMEE={errorFormat}");
            return ModemResponse.Success(response.Success);
        }

        /// <summary>
        /// Closes the modem connection.
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        #region Dispose

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Channel.Dispose();
                }

                _isDisposed = true;
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
    }
}