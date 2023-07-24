// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Text;
using IoT.Device.AtModem.CodingSchemes;
using IoT.Device.AtModem.DTOs;
using IoT.Device.AtModem.Events;
using IoT.Device.AtModem.Modem;
using IoT.Device.AtModem.PDU;
using static IoT.Device.AtModem.Sms.ISmsProvider;

namespace IoT.Device.AtModem.Sms
{
    internal class GenericSmsProvider : ISmsProvider
    {
        internal ModemBase ModemBase { get; }

        internal GenericSmsProvider(ModemBase modem)
        {
            ModemBase = modem;
            ModemBase.GenericEvent += ModemBaseGenericEvent;
        }

        private void ModemBaseGenericEvent(object sender, GenericEventArgs e)
        {
            if (e.Message.StartsWith("+CMTI: "))
            {
                SmsReceived?.Invoke(this, SmsReceivedEventArgs.CreateFromResponse(e.Message));
            }
        }

        /// <inheritdoc/>
        public event SmsReceivedEventHandler SmsReceived;

        #region list send read sms

        /// <summary>
        /// Sets the new SMS indication settings.
        /// </summary>
        /// <param name="mode">The mode for the new SMS indication.</param>
        /// <param name="mt">The message type for the new SMS indication.</param>
        /// <param name="bm">The buffer management for the new SMS indication.</param>
        /// <param name="ds">The discard status for the new SMS indication.</param>
        /// <param name="bfr">The bit field reporting for the new SMS indication.</param>
        /// <returns>A <see cref="ModemResponse"/> indicating the success or failure of the operation.</returns>
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

            AtResponse response = ModemBase.Channel.SendCommand($"AT+CNMI={mode},{mt},{bm},{ds},{bfr}");
            return ModemResponse.Success(response.Success);
        }

        /// <summary>
        /// Sends an SMS in text format.
        /// </summary>
        /// <param name="phoneNumber">The phone number of the recipient.</param>
        /// <param name="message">The text message to be sent.</param>
        /// <returns>A <see cref="ModemResponse"/> containing the reference to the sent SMS if successful, or an error response.</returns>
        public virtual ModemResponse SendSmsInTextFormat(PhoneNumber phoneNumber, string message)
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
            AtResponse response = ModemBase.Channel.SendSmsAsync(cmd1, cmd2, "+CMGS:");

            if (response.Success)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                ////var match = Regex.Match(line, @"\+CMGS:\s(?<mr>\d+)");
                if (line.StartsWith("+CMGS: "))
                {
                    int mr = int.Parse(line.Substring(7));
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
        /// Sends an SMS in PDU format asynchronously with optional SMS center address length inclusion.
        /// </summary>
        /// <param name="phoneNumber">The phone number of the recipient.</param>
        /// <param name="message">The text message to be sent.</param>
        /// <param name="codingScheme">The coding scheme to be used for encoding the message.</param>
        /// <param name="includeEmptySmscLength">A flag indicating whether to include an empty SMS center address length in the PDU.</param>
        /// <returns>A <see cref="ModemResponse"/> containing the reference to the sent SMS if successful, or an error response.</returns>
        public virtual ModemResponse SendSmsInPduFormat(PhoneNumber phoneNumber, string message, CodingScheme codingScheme, bool includeEmptySmscLength)
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

            string pdu = Pdu.EncodeSmsSubmit(phoneNumber, encodedMessage, dataCodingScheme, includeEmptySmscLength);
            string cmd1 = $"AT+CMGS={(pdu.Length) / 2}";
            string cmd2 = pdu;
            AtResponse response = ModemBase.Channel.SendSmsAsync(cmd1, cmd2, "+CMGS:", TimeSpan.FromSeconds(30));

            if (response.Success)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                if (line.StartsWith("+CMGS: "))
                {
                    int mr = int.Parse(line.Substring(7));
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
        /// Reads an SMS.
        /// </summary>
        /// <param name="index">The index of the SMS to read.</param>
        /// <param name="smsTextFormat">The SMS text format (PDU or Text).</param>
        /// <returns>A <see cref="ModemResponse"/> containing the read SMS.</returns>
        public virtual ModemResponse ReadSms(int index, SmsTextFormat smsTextFormat)
        {
            switch (smsTextFormat)
            {
                case SmsTextFormat.PDU:
                    AtResponse pduResponse = ModemBase.Channel.SendMultilineCommand($"AT+CMGR={index},0", null);

                    if (pduResponse.Success)
                    {
                        string line1 = (string)pduResponse.Intermediates[0];
                        ////var line1Match = Regex.Match(line1, @"\+CMGR:\s(?<status>\d),(""(?<alpha>\w*)"")*,(?<length>\d+)");                        
                        string line2 = (string)pduResponse.Intermediates[1];
                        ////var line2Match = Regex.Match(line2, @"(?<status>[0-9A-Z]*)");
                        if (line1.StartsWith("+CMGR: "))
                        {
                            int statusCode = int.Parse(line1.Substring(7).Split(',')[0]);
                            SmsStatus status = SmsStatusHelpers.ToSmsStatus(statusCode);

                            var pdu = new SpanChar(line2.ToCharArray());
                            var pduType = Pdu.GetPduType(pdu);
                            PhoneNumber sender = null;
                            DateTime received = default;
                            string message = string.Empty;
                            if (pduType == PduType.SMS_SUBMIT)
                            {
                                var pduMessage = Pdu.DecodeSmsSubmit(pdu);
                                sender = pduMessage.SenderNumber;

                                // No time stamp for non sent SMS
                                ////received = pduMessage.TimeStamp;
                                message = pduMessage.Message;
                            }
                            else if (pduType == PduType.SMS_DELIVER)
                            {
                                var pduMessage = Pdu.DecodeSmsDeliver(pdu);
                                sender = pduMessage.SenderNumber;
                                received = pduMessage.TimeStamp;
                                message = pduMessage.Message;
                            }

                            return ModemResponse.ResultSuccess(new DTOs.Sms(status, sender, received, message));
                        }
                    }

                    break;
                case SmsTextFormat.Text:
                    AtResponse textResponse = ModemBase.Channel.SendMultilineCommand($"AT+CMGR={index},0", null);

                    if (textResponse.Success && textResponse.Intermediates.Count > 0)
                    {
                        string line = (string)textResponse.Intermediates[0];
                        ////var match = Regex.Match(line, @"\+CMGR:\s""(?<status>[A-Z\s]+)"",""(?<sender>\+\d+)"",,""(?<received>(?<year>\d\d)/(?<month>\d\d)/(?<day>\d\d),(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)(?<zone>[-+]\d\d))""");
                        if (line.StartsWith("+CMGR: "))
                        {
                            string[] parts = line.Substring(7).Split(',');
                            SmsStatus status = SmsStatusHelpers.ToSmsStatus(parts[0].Trim('"'));
                            PhoneNumber sender = new PhoneNumber(parts[1].Trim('"'));
                            string[] dates = parts[3].Trim('"').Split('/');
                            int year = int.Parse(dates[0]);
                            int month = int.Parse(dates[1]);
                            int day = int.Parse(dates[2]);
                            string[] times = parts[4].Trim('"').Split(':');
                            int hour = int.Parse(times[0]);
                            int minute = int.Parse(times[1]);
                            int second = int.Parse(times[2].Substring(0, 2));
                            int zone = int.Parse(times[2].Substring(2, 3));
                            DateTime received = new DateTime(2000 + year, month, day, hour, minute, second).Add(TimeSpan.FromMinutes(15 * zone));
                            string message = (string)textResponse.Intermediates[textResponse.Intermediates.Count - 1];
                            return ModemResponse.ResultSuccess(new DTOs.Sms(status, sender, received, message));
                        }
                    }

                    break;
                default:
                    throw new NotSupportedException("The format is not supported");
            }

            return ModemResponse.ResultError();
        }

        /// <summary>
        /// Gets the SMS message format.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> containing the SMS message format.</returns>
        public virtual ModemResponse GetSmsMessageFormat()
        {
            AtResponse response = ModemBase.Channel.SendSingleLineCommandAsync($"AT+CMGF?", "+CMGF:");

            if (response.Success)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                if (line.StartsWith("+CMGF: "))
                {
                    int format = int.Parse(line.Substring(7));
                    return ModemResponse.ResultSuccess((SmsTextFormat)format);
                }
            }

            return ModemResponse.ResultError();
        }

        /// <summary>
        /// Sets the SMS message format.
        /// </summary>
        /// <param name="format">The SMS text format to be set.</param>
        /// <returns>A <see cref="ModemResponse"/> indicating the success or failure of the operation.</returns>
        public virtual ModemResponse SetSmsMessageFormat(SmsTextFormat format)
        {
            AtResponse response = ModemBase.Channel.SendCommand($"AT+CMGF={(int)format}");
            return ModemResponse.Success(response.Success);
        }

        /// <summary>
        /// Lists SMS messages asynchronously based on the provided SMS status.
        /// </summary>
        /// <param name="smsStatus">The SMS status to filter the messages.</param>
        /// <returns>A <see cref="ModemResponse"/> containing the list of SMS messages.</returns>
        public virtual ModemResponse ListSmss(SmsStatus smsStatus)
        {
            // Gets the SMS format to be able to run the proper querry
            var formatResponse = GetSmsMessageFormat();
            if (!formatResponse.IsSuccess)
            {
                return ModemResponse.ResultError();
            }

            AtResponse response;
            if ((SmsTextFormat)formatResponse.Result == SmsTextFormat.Text)
            {
                response = ModemBase.Channel.SendMultilineCommand($"AT+CMGL=\"{SmsStatusHelpers.ToString(smsStatus)}\",0", null, TimeSpan.FromSeconds(20));
            }
            else
            {
                response = ModemBase.Channel.SendMultilineCommand($"AT+CMGL={(int)smsStatus},0", null, TimeSpan.FromSeconds(20));
            }

            ArrayList smss = new ArrayList();
            if (response.Success)
            {
                ////string metaRegEx = @"\+CMGL:\s(?<index>\d+),""(?<status>[A-Z\s]+)"",""(?<sender>\+*\d+)"",,""(?<received>(?<year>\d\d)/(?<month>\d\d)/(?<day>\d\d),(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)(?<zone>[-+]\d\d))""";

                var enumerator = response.Intermediates.GetEnumerator();
                {
                    string line = null;
                    AdvanceIterator();
                    while (line != null)
                    {
                        var match = line.Substring(7).Split(',');
                        if (match.Length >= 4)
                        {
                            StringBuilder messageBuilder = new StringBuilder();
                            int index = 0;
                            SmsStatus status = SmsStatus.REC_UNREAD;
                            PhoneNumber sender = null;
                            DateTime received = default;
                            if ((SmsTextFormat)formatResponse.Result == SmsTextFormat.Text)
                            {
                                index = int.Parse(match[0]);
                                status = SmsStatusHelpers.ToSmsStatus(match[1].Trim('\"'));
                                sender = new PhoneNumber(match[2].Trim('\"'));
                                var dates = match[4].Split('/');
                                int year = int.Parse(dates[0].Substring(1));
                                int month = int.Parse(dates[1]);
                                int day = int.Parse(dates[2]);
                                var times = match[5].Split(':');
                                int hour = int.Parse(times[0]);
                                int minute = int.Parse(times[1]);
                                int second = int.Parse(times[2].Substring(0, 2));
                                int zone = int.Parse(times[2].Substring(2, 3));
                                received = new DateTime(2000 + year, month, day, hour, minute, second).Add(TimeSpan.FromMinutes(15 * zone));

                                AdvanceIterator();
                                while (line != null && !line.StartsWith("+CMGL: "))
                                {
                                    messageBuilder.AppendLine(line);
                                    AdvanceIterator();
                                }
                            }
                            else
                            {
                                index = int.Parse(match[0]);
                                status = SmsStatusHelpers.ToSmsStatus(int.Parse(match[1]));
                                AdvanceIterator();
                                var pdu = new SpanChar(line.ToCharArray());
                                var pduType = Pdu.GetPduType(pdu);
                                if (pduType == PduType.SMS_SUBMIT)
                                {
                                    var pduMessage = Pdu.DecodeSmsSubmit(pdu);
                                    sender = pduMessage.SenderNumber;

                                    // No time stamp for non sent SMS
                                    ////received = pduMessage.TimeStamp;
                                    messageBuilder.Append(pduMessage.Message);
                                }
                                else if (pduType == PduType.SMS_DELIVER)
                                {
                                    var pduMessage = Pdu.DecodeSmsDeliver(pdu);
                                    sender = pduMessage.SenderNumber;
                                    received = pduMessage.TimeStamp;
                                    messageBuilder.Append(pduMessage.Message);
                                }

                                AdvanceIterator();
                                while (line != null && !line.StartsWith("+CMGL: "))
                                {
                                    // We pass anything else because for PDU, it's only 1 line
                                    AdvanceIterator();
                                }
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
        /// Deletes an SMS message.
        /// </summary>
        /// <param name="index">The index of the SMS to delete.</param>
        /// <returns>A <see cref="ModemResponse"/> indicating the success of the operation.</returns>
        public virtual ModemResponse DeleteSms(int index)
        {
            AtResponse response = ModemBase.Channel.SendCommand($"AT+CMGD={index}");
            return ModemResponse.Success(response.Success);
        }

        #endregion

        #region sms storage

        /// <summary>
        /// Gets the supported preferred message storages.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> containing the supported preferred message storages.</returns>
        public virtual ModemResponse GetSupportedPreferredMessageStorages()
        {
            AtResponse response = ModemBase.Channel.SendSingleLineCommandAsync($"AT+CPMS=?", "+CPMS:");

            if (response.Success && response.Intermediates.Count > 0)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                ////var match = Regex.Match(line, @"\+CPMS:\s\((?<s1Storages>(""\w+"",?)+)\),\((?<s2Storages>(""\w+"",?)+)\),\((?<s3Storages>(""\w+"",?)+)\)");
                if (line.StartsWith("+CPMS: "))
                {
                    string[] storages = line.Substring(7).Split(')');
                    for (int i = 0; i < storages.Length; i++)
                    {
                        storages[i] = storages[i].TrimStart(',');
                    }

                    string[] s1Storages = storages[0].Split(',');
                    for (int i = 0; i < s1Storages.Length; i++)
                    {
                        s1Storages[i] = s1Storages[i].Trim('(').Trim('"');
                    }

                    string[] s2Storages = storages[1].Split(',');
                    for (int i = 0; i < s2Storages.Length; i++)
                    {
                        s2Storages[i] = s2Storages[i].Trim('(').Trim('"');
                    }

                    string[] s3Storages = storages[2].Split(',');
                    for (int i = 0; i < s3Storages.Length; i++)
                    {
                        s3Storages[i] = s3Storages[i].Trim('(').Trim('"');
                    }

                    return ModemResponse.ResultSuccess(new SupportedPreferredMessageStorages(s1Storages, s2Storages, s3Storages));
                }
            }

            return ModemResponse.ResultError();
        }

        /// <summary>
        /// Gets the supported preferred message storages.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> containing the supported preferred message storages.</returns>
        public virtual ModemResponse GetPreferredMessageStorages()
        {
            AtResponse response = ModemBase.Channel.SendSingleLineCommandAsync($"AT+CPMS?", "+CPMS:");

            if (response.Success && response.Intermediates.Count > 0)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                ////var match = Regex.Match(line, @"\+CPMS:\s(?<storage1>""\w+"",\d+,\d+),(?<storage2>""\w+"",\d+,\d+),(?<storage3>""\w+"",\d+,\d+)");
                if (line.StartsWith("+CPMS: "))
                {
                    string[] storages = line.Substring(7).Split(',');

                    return ModemResponse.ResultSuccess(new PreferredMessageStorages(
                        new PreferredMessageStorage(storages[0].Trim('"'), int.Parse(storages[1]), int.Parse(storages[2])),
                        new PreferredMessageStorage(storages[3].Trim('"'), int.Parse(storages[4]), int.Parse(storages[5])),
                        new PreferredMessageStorage(storages[6].Trim('"'), int.Parse(storages[7]), int.Parse(storages[8]))));
                }
            }

            return ModemResponse.ResultError();
        }

        /// <summary>
        /// Sets the preferred message storage.
        /// </summary>
        /// <param name="storage1Name">Name of the first storage.</param>
        /// <param name="storage2Name">Name of the second storage.</param>
        /// <param name="storage3Name">Name of the third storage.</param>
        /// <returns>A <see cref="ModemResponse"/> indicating the success of the operation.</returns>
        public virtual ModemResponse SetPreferredMessageStorage(string storage1Name, string storage2Name, string storage3Name)
        {
            AtResponse response = ModemBase.Channel.SendSingleLineCommandAsync($"AT+CPMS=\"{storage1Name}\",\"{storage2Name}\",\"{storage3Name}\"", "+CPMS:");

            if (response.Success && response.Intermediates.Count > 0)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                ////var match = Regex.Match(line, @"\+CPMS:\s(?<s1Used>\d+),(?<s1Total>\d+),(?<s2Used>\d+),(?<s2Total>\d+),(?<s3Used>\d+),(?<s3Total>\d+)");
                if (line.StartsWith("+CPMS: "))
                {
                    string[] storages = line.Substring(7).Split(',');
                    int s1Used = int.Parse(storages[0]);
                    int s1Total = int.Parse(storages[1]);
                    int s2Used = int.Parse(storages[2]);
                    int s2Total = int.Parse(storages[3]);
                    int s3Used = int.Parse(storages[4]);
                    int s3Total = int.Parse(storages[5]);

                    return ModemResponse.ResultSuccess(new PreferredMessageStorages(
                        new PreferredMessageStorage(storage1Name, s1Used, s1Total),
                        new PreferredMessageStorage(storage2Name, s2Used, s2Total),
                        new PreferredMessageStorage(storage3Name, s3Used, s3Total)));
                }
            }

            return ModemResponse.ResultError();
        }

        #endregion
    }
}
