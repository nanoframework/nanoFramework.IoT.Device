// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using IoT.Device.AtModem.CodingSchemes;
using IoT.Device.AtModem.DTOs;
using UnitsNet;

namespace IoT.Device.AtModem.Modem
{
    /// <summary>
    /// Represents a basic modem. All other modems should implement this class.
    /// </summary>
    public abstract class ModemBase : IDisposable
    {
        /// <summary>
        /// The AT channel used to communicate with the modem.
        /// </summary>
        protected readonly AtChannel Channel;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the ModemBase class with the specified AT channel.
        /// </summary>
        /// <param name="channel">The AT channel used for communication with the modem.</param>
        public ModemBase(AtChannel channel)
        {
            Channel = channel;

            // TODO: Uncomment this when the event is implemented
            ////channel.UnsolicitedEvent += Channel_UnsolicitedEvent;
        }

        ////private void Channel_UnsolicitedEvent(object sender, UnsolicitedEventArgs e)
        ////{
        ////    if (e.Line1 == "RING")
        ////    {
        ////        IncomingCall?.Invoke(this, new IncomingCallEventArgs());
        ////    }
        ////    else if (e.Line1.StartsWith("VOICE CALL: BEGIN"))
        ////    {
        ////        CallStarted?.Invoke(this, new CallStartedEventArgs());
        ////    }
        ////    else if (e.Line1.StartsWith("VOICE CALL: END"))
        ////    {
        ////        CallEnded?.Invoke(this, CallEndedEventArgs.CreateFromResponse(e.Line1));
        ////    }
        ////    else if (e.Line1.StartsWith("MISSED_CALL: "))
        ////    {
        ////        MissedCall?.Invoke(this, MissedCallEventArgs.CreateFromResponse(e.Line1));
        ////    }
        ////    else if (e.Line1.StartsWith("+CMTI: "))
        ////    {
        ////        SmsReceived?.Invoke(this, SmsReceivedEventArgs.CreateFromResponse(e.Line1));
        ////    }
        ////    else if (e.Line1.StartsWith("+CUSD: "))
        ////    {
        ////        UssdResponseReceived?.Invoke(this, UssdResponseEventArgs.CreateFromResponse(e.Line1));
        ////    }
        ////    else if (AtErrorParsers.TryGetError(e.Line1, out Error error))
        ////    {
        ////        ErrorReceived?.Invoke(this, new ErrorEventArgs(error.ToString()));
        ////    }
        ////    else
        ////    {
        ////        GenericEvent?.Invoke(this, new GenericEventArgs(e.Line1));
        ////    }
        ////}

        ////public event EventHandler<ErrorEventArgs> ErrorReceived;
        ////public event EventHandler<GenericEventArgs> GenericEvent;

        #region _V_25TER
        ////public event EventHandler<IncomingCallEventArgs> IncomingCall;
        ////public event EventHandler<MissedCallEventArgs> MissedCall;
        ////public event EventHandler<CallStartedEventArgs> CallStarted;
        ////public event EventHandler<CallEndedEventArgs> CallEnded;
        ////public event EventHandler<UssdResponseEventArgs> UssdResponseReceived;
        ////public event EventHandler<SmsReceivedEventArgs> SmsReceived;

        /// <summary>
        /// Gets the International Mobile Subscriber Identity (IMSI) asynchronously.
        /// </summary>
        /// <returns>A ModemResponse containing the IMSI if successful, or an error response.</returns>
        public virtual ModemResponse GetImsiAsync()
        {
            AtResponse response = Channel.SendSingleLineCommandAsync("AT+CIMI", string.Empty);

            if (response.Success)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                var match = Regex.Match(line, @"(?<imsi>\d+)");
                if (match.Success)
                {
                    string imsi = match.Groups["imsi"].Value;
                    return ModemResponse.ResultSuccess(new Imsi(imsi));
                }
            }

            return ModemResponse.ResultError();
        }

        /// <summary>
        /// Answers an incoming call asynchronously.
        /// </summary>
        /// <returns>A ModemResponse indicating the success or failure of the operation.</returns>
        public virtual ModemResponse AnswerIncomingCallAsync()
        {
            AtResponse response = Channel.SendCommand("ATA");
            return ModemResponse.Success(response.Success);
        }

        /// <summary>
        /// Dials a phone number asynchronously.
        /// </summary>
        /// <param name="phoneNumber">The phone number to dial.</param>
        /// <param name="hideCallerNumber">A flag indicating whether to hide the caller number.</param>
        /// <param name="closedUserGroup">A flag indicating whether to use the closed user group feature.</param>
        /// <returns>A ModemResponse indicating the success or failure of the operation.</returns>
        public virtual ModemResponse DialAsync(PhoneNumber phoneNumber, bool hideCallerNumber = false, bool closedUserGroup = false)
        {
            string command = $"ATD{phoneNumber}{(hideCallerNumber ? 'I' : 'i')}{(closedUserGroup ? 'G' : 'g')};";
            AtResponse response = Channel.SendCommand(command);
            return ModemResponse.Success(response.Success);
        }

        /// <summary>
        /// Disables echo mode asynchronously.
        /// </summary>
        /// <returns>A ModemResponse indicating the success or failure of the operation.</returns>
        public virtual ModemResponse DisableEchoAsync()
        {
            AtResponse response = Channel.SendCommand("ATE0");
            return ModemResponse.Success(response.Success);
        }

        /// <summary>
        /// Gets the product identification information asynchronously.
        /// </summary>
        /// <returns>A ModemResponse containing the product identification information if successful, or an error response.</returns>
        public virtual ModemResponse GetProductIdentificationInformationAsync()
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
        /// Hangs up the current call asynchronously.
        /// </summary>
        /// <returns>A ModemResponse indicating the success or failure of the operation.</returns>
        public virtual ModemResponse HangupAsync()
        {
            AtResponse response = Channel.SendCommand($"AT+CHUP");
            return ModemResponse.Success(response.Success);
        }

        /// <summary>
        /// Gets the available character sets asynchronously.
        /// </summary>
        /// <returns>A ModemResponse containing the available character sets if successful, or an error response.</returns>
        public virtual ModemResponse GetAvailableCharacterSetsAsync()
        {
            AtResponse response = Channel.SendSingleLineCommandAsync($"AT+CSCS=?", "+CSCS:");

            if (response.Success)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                var match = Regex.Match(line, @"\+CSCS:\s\((?:""(?<characterSet>\w+)"",*)+\)");
                if (match.Success)
                {
                    string[] characterSets = new string[match.Groups["characterSet"].Captures.Count];
                    int i = 0;
                    foreach (Capture cap in match.Groups["characterSet"].Captures)
                    {
                        characterSets[i] = cap.Value;
                        i++;
                    }

                    return ModemResponse.ResultSuccess(characterSets);
                }
            }

            return ModemResponse.ResultError();
        }

        /// <summary>
        /// Gets the current character set asynchronously.
        /// </summary>
        /// <returns>A ModemResponse containing the current character set if successful, or an error response.</returns>
        public virtual ModemResponse GetCurrentCharacterSetAsync()
        {
            AtResponse response = Channel.SendSingleLineCommandAsync($"AT+CSCS?", "+CSCS:");

            if (response.Success)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                var match = Regex.Match(line, @"""(?<characterSet>\w)""");
                if (match.Success)
                {
                    string characterSet = match.Groups["characterSet"].Value;
                    return ModemResponse.ResultSuccess(characterSet);
                }
            }

            return ModemResponse.ResultError();
        }

        /// <summary>
        /// Sets the character set asynchronously.
        /// </summary>
        /// <param name="characterSet">The character set to be set.</param>
        /// <returns>A ModemResponse indicating the success or failure of the operation.</returns>
        public virtual ModemResponse SetCharacterSetAsync(string characterSet)
        {
            AtResponse response = Channel.SendCommand($"AT+CSCS=\"{characterSet}\"");
            return ModemResponse.Success(response.Success);
        }

        #endregion

        #region _3GPP_TS_27_005

        /// <summary>
        /// Sets the SMS message format asynchronously.
        /// </summary>
        /// <param name="format">The SMS text format to be set.</param>
        /// <returns>A ModemResponse indicating the success or failure of the operation.</returns>
        public virtual ModemResponse SetSmsMessageFormatAsync(SmsTextFormat format)
        {
            AtResponse response = Channel.SendCommand($"AT+CMGF={(int)format}");
            return ModemResponse.Success(response.Success);
        }

        /// <summary>
        /// Sets the new SMS indication settings asynchronously.
        /// </summary>
        /// <param name="mode">The mode for the new SMS indication.</param>
        /// <param name="mt">The message type for the new SMS indication.</param>
        /// <param name="bm">The buffer management for the new SMS indication.</param>
        /// <param name="ds">The discard status for the new SMS indication.</param>
        /// <param name="bfr">The bit field reporting for the new SMS indication.</param>
        /// <returns>A ModemResponse indicating the success or failure of the operation.</returns>
        public virtual ModemResponse SetNewSmsIndication(int mode, int mt, int bm, int ds, int bfr)
        {
            if (mode < 0 || mode > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(mode));
            }

            if (mt < 0 || mt > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(mt));
            }

            if (!(bm == 0 || bm == 2))
            {
                throw new ArgumentOutOfRangeException(nameof(bm));
            }

            if (ds < 0 || ds > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(ds));
            }

            if (bfr < 0 || bfr > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(bfr));
            }

            AtResponse response = Channel.SendCommand($"AT+CNMI={mode},{mt},{bm},{ds},{bfr}");
            return ModemResponse.Success(response.Success);
        }

        /// <summary>
        /// Sends an SMS in text format asynchronously.
        /// </summary>
        /// <param name="phoneNumber">The phone number of the recipient.</param>
        /// <param name="message">The text message to be sent.</param>
        /// <returns>A ModemResponse containing the reference to the sent SMS if successful, or an error response.</returns>
        public virtual ModemResponse SendSmsInTextFormatAsync(PhoneNumber phoneNumber, string message)
        {
            if (phoneNumber is null)
            {
                throw new ArgumentNullException(nameof(phoneNumber));
            }

            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            string cmd1 = $"AT+CMGS=\"{phoneNumber}\"";
            string cmd2 = message;
            AtResponse response = Channel.SendSmsAsync(cmd1, cmd2, "+CMGS:");

            if (response.Success)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                var match = Regex.Match(line, @"\+CMGS:\s(?<mr>\d+)");
                if (match.Success)
                {
                    int mr = int.Parse(match.Groups["mr"].Value);
                    return ModemResponse.ResultSuccess(new SmsReference(mr));
                }
            }

            return ModemResponse.ResultError();
        }

        /// <summary>
        /// Sends an SMS in PDU format asynchronously.
        /// </summary>
        /// <param name="phoneNumber">The phone number of the recipient.</param>
        /// <param name="message">The text message to be sent.</param>
        /// <param name="codingScheme">The coding scheme to be used for encoding the message.</param>
        /// <returns>A ModemResponse containing the reference to the sent SMS if successful, or an error response.</returns>
        public abstract ModemResponse SendSmsInPduFormatAsync(PhoneNumber phoneNumber, string message, CodingScheme codingScheme);

        /// <summary>
        /// Sends an SMS in PDU format asynchronously with optional SMS center address length inclusion.
        /// </summary>
        /// <param name="phoneNumber">The phone number of the recipient.</param>
        /// <param name="message">The text message to be sent.</param>
        /// <param name="codingScheme">The coding scheme to be used for encoding the message.</param>
        /// <param name="includeEmptySmscLength">A flag indicating whether to include an empty SMS center address length in the PDU.</param>
        /// <returns>A ModemResponse containing the reference to the sent SMS if successful, or an error response.</returns>
        protected virtual ModemResponse SendSmsInPduFormatAsync(PhoneNumber phoneNumber, string message, CodingScheme codingScheme, bool includeEmptySmscLength)
        {
            if (phoneNumber is null)
            {
                throw new ArgumentNullException(nameof(phoneNumber));
            }

            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            byte dataCodingScheme;
            string encodedMessage;
            switch (codingScheme)
            {
                case CodingScheme.Ansi:
                    encodedMessage = Ansi.Encode(message);
                    dataCodingScheme = Ansi.DataCodingSchemeCode;
                    break;
                case CodingScheme.Gsm7:
                    encodedMessage = Gsm7.Encode(message);
                    dataCodingScheme = Gsm7.DataCodingSchemeCode;
                    break;
                case CodingScheme.UCS2:
                    encodedMessage = UCS2.Encode(message);
                    dataCodingScheme = UCS2.DataCodingSchemeCode;
                    break;
                default:
                    throw new ArgumentException("The encoding scheme is not supported");
            }

            // TODO
            ////string pdu = Pdu.EncodeSmsSubmit(phoneNumber, encodedMessage, dataCodingScheme, includeEmptySmscLength);
            string pdu = string.Empty;
            string cmd1 = $"AT+CMGS={(pdu.Length) / 2}";
            string cmd2 = pdu;
            AtResponse response = Channel.SendSmsAsync(cmd1, cmd2, "+CMGS:", TimeSpan.FromSeconds(30));

            if (response.Success)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                var match = Regex.Match(line, @"\+CMGS:\s(?<mr>\d+)");
                if (match.Success)
                {
                    int mr = int.Parse(match.Groups["mr"].Value);
                    return ModemResponse.ResultSuccess(new SmsReference(mr));
                }
            }
            else if (AtErrorParsers.TryGetError(response.FinalResponse, out Error error))
            {
                return ModemResponse.ResultError(error.ToString());
            }

            return ModemResponse.ResultError();
        }

        /// <summary>
        /// Gets the supported preferred message storages asynchronously.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> containing the supported preferred message storages.</returns>
        public virtual ModemResponse GetSupportedPreferredMessageStoragesAsync()
        {
            AtResponse response = Channel.SendSingleLineCommandAsync($"AT+CPMS=?", "+CPMS:");

            if (response.Success && response.Intermediates.Count > 0)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                var match = Regex.Match(line, @"\+CPMS:\s\((?<s1Storages>(""\w+"",?)+)\),\((?<s2Storages>(""\w+"",?)+)\),\((?<s3Storages>(""\w+"",?)+)\)");
                if (match.Success)
                {
                    string[] s1Storages = match.Groups["s1Storages"].Value.Split(',');
                    for (int i = 0; i < s1Storages.Length; i++)
                    {
                        s1Storages[i] = s1Storages[i].Trim('"');
                    }

                    string[] s2Storages = match.Groups["s2Storages"].Value.Split(',');
                    for (int i = 0; i < s2Storages.Length; i++)
                    {
                        s2Storages[i] = s2Storages[i].Trim('"');
                    }

                    string[] s3Storages = match.Groups["s3Storages"].Value.Split(',');
                    for (int i = 0; i < s3Storages.Length; i++)
                    {
                        s3Storages[i] = s3Storages[i].Trim('"');
                    }

                    return ModemResponse.ResultSuccess(new SupportedPreferredMessageStorages(s1Storages, s2Storages, s3Storages));
                }
            }

            return ModemResponse.ResultError();
        }

        /// <summary>
        /// Gets the supported preferred message storages asynchronously.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> containing the supported preferred message storages.</returns>
        public virtual ModemResponse GetPreferredMessageStoragesAsync()
        {
            AtResponse response = Channel.SendSingleLineCommandAsync($"AT+CPMS?", "+CPMS:");

            if (response.Success && response.Intermediates.Count > 0)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty; 
                var match = Regex.Match(line, @"\+CPMS:\s(?<storage1>""\w+"",\d+,\d+),(?<storage2>""\w+"",\d+,\d+),(?<storage3>""\w+"",\d+,\d+)");
                if (match.Success)
                {
                    string[] s1Split = match.Groups["storage1"].Value.Split(',');
                    string[] s2Split = match.Groups["storage2"].Value.Split(',');
                    string[] s3Split = match.Groups["storage3"].Value.Split(',');

                    return ModemResponse.ResultSuccess(new PreferredMessageStorages(
                        new PreferredMessageStorage(s1Split[0].Trim('"'), int.Parse(s1Split[1]), int.Parse(s1Split[2])),
                        new PreferredMessageStorage(s2Split[0].Trim('"'), int.Parse(s2Split[1]), int.Parse(s2Split[2])),
                        new PreferredMessageStorage(s3Split[0].Trim('"'), int.Parse(s3Split[1]), int.Parse(s3Split[2]))));
                }
            }

            return ModemResponse.ResultError();
        }

        /// <summary>
        /// Sets the preferred message storage asynchronously.
        /// </summary>
        /// <param name="storage1Name">Name of the first storage.</param>
        /// <param name="storage2Name">Name of the second storage.</param>
        /// <param name="storage3Name">Name of the third storage.</param>
        /// <returns>A <see cref="ModemResponse"/> indicating the success of the operation.</returns>
        public virtual ModemResponse SetPreferredMessageStorageAsync(string storage1Name, string storage2Name, string storage3Name)
        {
            AtResponse response = Channel.SendSingleLineCommandAsync($"AT+CPMS=\"{storage1Name}\",\"{storage2Name}\",\"{storage3Name}\"", "+CPMS:");

            if (response.Success && response.Intermediates.Count > 0)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty; 
                var match = Regex.Match(line, @"\+CPMS:\s(?<s1Used>\d+),(?<s1Total>\d+),(?<s2Used>\d+),(?<s2Total>\d+),(?<s3Used>\d+),(?<s3Total>\d+)");
                if (match.Success)
                {
                    int s1Used = int.Parse(match.Groups["s1Used"].Value);
                    int s1Total = int.Parse(match.Groups["s1Total"].Value);
                    int s2Used = int.Parse(match.Groups["s2Used"].Value);
                    int s2Total = int.Parse(match.Groups["s2Total"].Value);
                    int s3Used = int.Parse(match.Groups["s3Used"].Value);
                    int s3Total = int.Parse(match.Groups["s3Total"].Value);

                    return ModemResponse.ResultSuccess(new PreferredMessageStorages(
                        new PreferredMessageStorage(storage1Name, s1Used, s1Total),
                        new PreferredMessageStorage(storage2Name, s2Used, s2Total),
                        new PreferredMessageStorage(storage3Name, s3Used, s3Total)));
                }
            }

            return ModemResponse.ResultError();
        }

        /// <summary>
        /// Reads an SMS asynchronously.
        /// </summary>
        /// <param name="index">The index of the SMS to read.</param>
        /// <param name="smsTextFormat">The SMS text format (PDU or Text).</param>
        /// <returns>A <see cref="ModemResponse"/> containing the read SMS.</returns>
        public virtual ModemResponse ReadSmsAsync(int index, SmsTextFormat smsTextFormat)
        {
            switch (smsTextFormat)
            {
                case SmsTextFormat.PDU:
                    AtResponse pduResponse = Channel.SendMultilineCommand($"AT+CMGR={index},0", null);

                    if (pduResponse.Success)
                    {
                        string line1 = (string)pduResponse.Intermediates[0];
                        var line1Match = Regex.Match(line1, @"\+CMGR:\s(?<status>\d),(""(?<alpha>\w*)"")*,(?<length>\d+)");
                        string line2 = (string)pduResponse.Intermediates[1];
                        var line2Match = Regex.Match(line2, @"(?<status>[0-9A-Z]*)");
                        if (line1Match.Success && line2Match.Success)
                        {
                            int statusCode = int.Parse(line1Match.Groups["status"].Value);
                            SmsStatus status = SmsStatusHelpers.ToSmsStatus(statusCode);

                            string pdu = line2Match.Groups["status"].Value;

                            // TODO: Decode PDU
                            ////SmsDeliver pduMessage = Pdu.DecodeSmsDeliver(pdu.AsSpan());

                            ////return ModemResponse.ResultSuccess(new Sms(status, pduMessage.SenderNumber, pduMessage.Timestamp, pduMessage.Message));
                            return ModemResponse.ResultSuccess(new Sms(status, null, default, null));
                        }
                    }

                    break;
                case SmsTextFormat.Text:
                    AtResponse textResponse = Channel.SendMultilineCommand($"AT+CMGR={index},0", null);

                    if (textResponse.Success && textResponse.Intermediates.Count > 0)
                    {
                        string line = (string)textResponse.Intermediates[0];
                        var match = Regex.Match(line, @"\+CMGR:\s""(?<status>[A-Z\s]+)"",""(?<sender>\+\d+)"",,""(?<received>(?<year>\d\d)/(?<month>\d\d)/(?<day>\d\d),(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)(?<zone>[-+]\d\d))""");
                        if (match.Success)
                        {
                            SmsStatus status = SmsStatusHelpers.ToSmsStatus(match.Groups["status"].Value);
                            PhoneNumber sender = new PhoneNumber(match.Groups["sender"].Value);
                            int year = int.Parse(match.Groups["year"].Value);
                            int month = int.Parse(match.Groups["month"].Value);
                            int day = int.Parse(match.Groups["day"].Value);
                            int hour = int.Parse(match.Groups["hour"].Value);
                            int minute = int.Parse(match.Groups["minute"].Value);
                            int second = int.Parse(match.Groups["second"].Value);
                            int zone = int.Parse(match.Groups["zone"].Value);
                            DateTime received = new DateTime(2000 + year, month, day, hour, minute, second).Add(TimeSpan.FromMinutes(15 * zone));
                            string message = (string)textResponse.Intermediates[textResponse.Intermediates.Count - 1];
                            return ModemResponse.ResultSuccess(new Sms(status, sender, received, message));
                        }
                    }

                    break;
                default:
                    throw new NotSupportedException("The format is not supported");
            }

            return ModemResponse.ResultError();
        }

        /// <summary>
        /// Lists SMS messages asynchronously based on the provided SMS status.
        /// </summary>
        /// <param name="smsStatus">The SMS status to filter the messages.</param>
        /// <returns>A <see cref="ModemResponse"/> containing the list of SMS messages.</returns>
        public virtual ModemResponse ListSmssAsync(SmsStatus smsStatus)
        {
            AtResponse response = Channel.SendMultilineCommand($"AT+CMGL=\"{SmsStatusHelpers.ToString(smsStatus)}\",0", null);

            ArrayList smss = new ArrayList();
            if (response.Success)
            {
                string metaRegEx = @"\+CMGL:\s(?<index>\d+),""(?<status>[A-Z\s]+)"",""(?<sender>\+*\d+)"",,""(?<received>(?<year>\d\d)/(?<month>\d\d)/(?<day>\d\d),(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)(?<zone>[-+]\d\d))""";

                var enumerator = response.Intermediates.GetEnumerator();
                {
                    string line = null;
                    AdvanceIterator();
                    while (line != null)
                    {
                        var match = Regex.Match(line, metaRegEx);
                        if (match.Success)
                        {
                            int index = int.Parse(match.Groups["index"].Value);
                            SmsStatus status = SmsStatusHelpers.ToSmsStatus(match.Groups["status"].Value);
                            PhoneNumber sender = new PhoneNumber(match.Groups["sender"].Value);
                            int year = int.Parse(match.Groups["year"].Value);
                            int month = int.Parse(match.Groups["month"].Value);
                            int day = int.Parse(match.Groups["day"].Value);
                            int hour = int.Parse(match.Groups["hour"].Value);
                            int minute = int.Parse(match.Groups["minute"].Value);
                            int second = int.Parse(match.Groups["second"].Value);
                            int zone = int.Parse(match.Groups["zone"].Value);
                            DateTime received = new DateTime(2000 + year, month, day, hour, minute, second).Add(TimeSpan.FromMinutes(15 * zone));

                            StringBuilder messageBuilder = new StringBuilder();
                            AdvanceIterator();
                            while (line != null && !Regex.Match(line, metaRegEx).Success)
                            {
                                messageBuilder.AppendLine(line);
                                AdvanceIterator();
                            }

                            smss.Add(new SmsWithIndex(index, status, sender, received, messageBuilder.ToString()));
                        }
                    }

                    void AdvanceIterator()
                    {
                        line = enumerator.MoveNext() ? (string)enumerator.Current : null;
                    }
                }
            }

            return ModemResponse.ResultSuccess(smss);
        }

        /// <summary>
        /// Deletes an SMS message asynchronously.
        /// </summary>
        /// <param name="index">The index of the SMS to delete.</param>
        /// <returns>A <see cref="ModemResponse"/> indicating the success of the operation.</returns>
        public virtual ModemResponse DeleteSmsAsync(int index)
        {
            AtResponse response = Channel.SendCommand($"AT+CMGD={index}");
            return ModemResponse.Success(response.Success);
        }

        #endregion

        #region _3GPP_TS_27_007

        /// <summary>
        /// Gets the status of the SIM card asynchronously.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> containing the status of the SIM card.</returns>
        public virtual ModemResponse GetSimStatusAsync()
        {
            AtResponse response = Channel.SendSingleLineCommandAsync("AT+CPIN?", "+CPIN:", TimeSpan.FromSeconds(10));

            if (!response.Success)
            {
                if (AtErrorParsers.TryGetError(response.FinalResponse, out Error cmeError))
                {
                    return ModemResponse.ResultError(cmeError.ToString());
                }
            }

            // CPIN? has succeeded, now look at the result
            string cpinLine = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty; 
            var match = Regex.Match(cpinLine, @"\+CPIN:\s(?<pinresult>.*)");
            if (match.Success)
            {
                string cpinResult = match.Groups["pinresult"].Value;

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
        /// Enters the SIM PIN asynchronously.
        /// </summary>
        /// <param name="pin">The Personal Identification Number (PIN) to enter.</param>
        /// <returns>A <see cref="ModemResponse"/> indicating the success of the operation.</returns>
        public virtual ModemResponse EnterSimPinAsync(PersonalIdentificationNumber pin)
        {
            AtResponse response = Channel.SendCommand($"AT+CPIN={pin}");
            return ModemResponse.Success(response.Success);
        }

        /// <summary>
        /// Reinitializes the SIM asynchronously.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> indicating the success of the operation.</returns>
        public virtual ModemResponse ReInitializeSimAsync()
        {
            AtResponse response = Channel.SendCommand($"AT+CRFSIM");
            return ModemResponse.Success(response.Success);
        }

        /// <summary>
        /// Gets the signal strength asynchronously.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> containing the signal strength information.</returns>
        public virtual ModemResponse GetSignalStrengthAsync()
        {
            AtResponse response = Channel.SendSingleLineCommandAsync("AT+CSQ", "+CSQ:");

            if (response.Success)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty; 
                var match = Regex.Match(line, @"\+CSQ:\s(?<rssi>\d+),(?<ber>\d+)");
                if (match.Success)
                {
                    int rssi = int.Parse(match.Groups["rssi"].Value);
                    int ber = int.Parse(match.Groups["ber"].Value);
                    return ModemResponse.ResultSuccess(new SignalStrength(Ratio.FromPercent(rssi), Ratio.FromPercent(ber)));
                }
            }

            return ModemResponse.ResultError();
        }

        /// <summary>
        /// Gets the battery status asynchronously.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> containing the battery status information.</returns>
        public virtual ModemResponse GetBatteryStatusAsync()
        {
            AtResponse response = Channel.SendSingleLineCommandAsync("AT+CBC", "+CBC:");

            if (response.Success)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty; 
                var match = Regex.Match(line, @"\+CBC:\s(?<bcs>\d+),(?<bcl>\d+)");
                if (match.Success)
                {
                    int bcs = int.Parse(match.Groups["bcs"].Value);
                    int bcl = int.Parse(match.Groups["bcl"].Value);
                    return ModemResponse.ResultSuccess(new BatteryStatus((BatteryChargeStatus)bcs, Ratio.FromPercent(bcl)));
                }
            }

            return ModemResponse.ResultError();
        }

        /// <summary>
        /// Sets the date and time of the modem asynchronously.
        /// </summary>
        /// <param name="value">The date and time value to set.</param>
        /// <returns>A <see cref="ModemResponse"/> indicating the success of the operation.</returns>
        public virtual ModemResponse SetDateTimeAsync(DateTime value)
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
        /// Gets the date and time of the modem asynchronously.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> containing the date and time value.</returns>
        public virtual ModemResponse GetDateTimeAsync()
        {
            AtResponse response = Channel.SendSingleLineCommandAsync("AT+CCLK?", "+CCLK:");

            if (response.Success)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty; 
                var match = Regex.Match(line, @"\+CCLK:\s""(?<year>\d\d)/(?<month>\d\d)/(?<day>\d\d),(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)(?<zone>[-+]\d\d)""");
                if (match.Success)
                {
                    int year = int.Parse(match.Groups["year"].Value);
                    int month = int.Parse(match.Groups["month"].Value);
                    int day = int.Parse(match.Groups["day"].Value);
                    int hour = int.Parse(match.Groups["hour"].Value);
                    int minute = int.Parse(match.Groups["minute"].Value);
                    int second = int.Parse(match.Groups["second"].Value);
                    int zone = int.Parse(match.Groups["zone"].Value);
                    DateTime time = new DateTime(2000 + year, month, day, hour, minute, second).Add(TimeSpan.FromMinutes(15 * zone));
                    return ModemResponse.ResultSuccess(time);
                }
            }

            return ModemResponse.ResultError();
        }

        /// <summary>
        /// Sends a USSD code asynchronously.
        /// </summary>
        /// <param name="code">The USSD code to send.</param>
        /// <param name="codingScheme">The coding scheme to use (optional).</param>
        /// <returns>A <see cref="ModemResponse"/> indicating the success of the operation.</returns>
        public virtual ModemResponse SendUssdAsync(string code, int codingScheme = 15)
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
