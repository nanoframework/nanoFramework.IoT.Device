// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.AtModem.CodingSchemes;
using Iot.Device.AtModem.DTOs;
using Iot.Device.AtModem.Events;
using Iot.Device.AtModem.Modem;
using System.Collections;

namespace Iot.Device.AtModem.Sms
{
    /// <summary>
    /// Interface for SMS providers.
    /// </summary>
    public interface ISmsProvider
    {
        /// <summary>
        /// Represents the method that will handle SMS received events.
        /// </summary>
        /// <param name="sender">The source of the SMS received event.</param>
        /// <param name="e">An instance of SmsReceivedEventArgs containing event data.</param>
        public delegate void SmsReceivedEventHandler(object sender, SmsReceivedEventArgs e);

        /// <summary>
        /// Occurs when an SMS message is received.
        /// </summary>
        event SmsReceivedEventHandler SmsReceived;

        /// <summary>
        /// Sets the new SMS indication settings.
        /// </summary>
        /// <param name="mode">The mode for the new SMS indication.</param>
        /// <param name="mt">The message type for the new SMS indication.</param>
        /// <param name="bm">The buffer management for the new SMS indication.</param>
        /// <param name="ds">The discard status for the new SMS indication.</param>
        /// <param name="bfr">The bit field reporting for the new SMS indication.</param>
        /// <returns>A <see cref="ModemResponse"/> indicating the success or failure of the operation.
        /// If success, Result will contain a <see cref="bool"/> indicating the success of the operation.</returns>
        ModemResponse SetNewSmsIndication(int mode, int mt, int bm, int ds, int bfr);

        /// <summary>
        /// Sends an SMS in text format.
        /// </summary>
        /// <param name="phoneNumber">The phone number of the recipient.</param>
        /// <param name="message">The text message to be sent.</param>
        /// <returns>A <see cref="ModemResponse"/> containing the reference to the sent SMS if successful, or an error response.
        /// If success, Result will contain a <see cref="SmsReference"/> class.</returns>
        ModemResponse SendSmsInTextFormat(PhoneNumber phoneNumber, string message);

        /// <summary>
        /// Sends an SMS in PDU format asynchronously with optional SMS center address length inclusion.
        /// </summary>
        /// <param name="phoneNumber">The phone number of the recipient.</param>
        /// <param name="message">The text message to be sent.</param>
        /// <param name="codingScheme">The coding scheme to be used for encoding the message.</param>
        /// <param name="includeEmptySmscLength">A flag indicating whether to include an empty SMS center address length in the PDU.</param>
        /// <returns>A <see cref="ModemResponse"/> containing the reference to the sent SMS if successful, or an error response.
        /// If success, Result will contain a <see cref="SmsReference"/> class.</returns>
        ModemResponse SendSmsInPduFormat(PhoneNumber phoneNumber, string message, CodingScheme codingScheme, bool includeEmptySmscLength);

        /// <summary>
        /// Reads an SMS.
        /// </summary>
        /// <param name="index">The index of the SMS to read.</param>
        /// <param name="smsTextFormat">The SMS text format (PDU or Text).</param>
        /// <returns>A <see cref="ModemResponse"/> containing the read SMS.
        /// If success, Result will contain a <see cref="DTOs.Sms"/> class.</returns>
        ModemResponse ReadSms(int index, SmsTextFormat smsTextFormat);

        /// <summary>
        /// Gets the SMS message format.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> containing the SMS message format.
        /// If success, Result will contain a <see cref="SmsTextFormat"/> enum.</returns>
        ModemResponse GetSmsMessageFormat();

        /// <summary>
        /// Sets the SMS message format.
        /// </summary>
        /// <param name="format">The SMS text format to be set.</param>
        /// <returns>A <see cref="ModemResponse"/> indicating the success or failure of the operation.
        /// If success, Result will contain a <see cref="bool"/> indicating the success of the operation.</returns>
        ModemResponse SetSmsMessageFormat(SmsTextFormat format);

        /// <summary>
        /// Lists SMS messages asynchronously based on the provided SMS status.
        /// </summary>
        /// <param name="smsStatus">The SMS status to filter the messages.</param>
        /// <returns>A <see cref="ModemResponse"/> containing the list of SMS messages.
        /// If success, Result will contain a <see cref="ArrayList"/> class of <see cref="SmsWithIndex"/>.</returns>
        ModemResponse ListSmss(SmsStatus smsStatus);

        /// <summary>
        /// Deletes an SMS message.
        /// </summary>
        /// <param name="index">The index of the SMS to delete.</param>
        /// <returns>A <see cref="ModemResponse"/> indicating the success of the operation.
        /// If success, Result will contain a <see cref="bool"/> indicating the success of the operation.</returns>
        ModemResponse DeleteSms(int index);

        /// <summary>
        /// Gets the supported preferred message storages.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> containing the supported preferred message storages.
        /// If success, Result will contain a <see cref="SupportedPreferredMessageStorages"/> class.</returns>
        ModemResponse GetSupportedPreferredMessageStorages();

        /// <summary>
        /// Gets the supported preferred message storages.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> containing the supported preferred message storages.
        /// If success, Result will contain a <see cref="PreferredMessageStorages"/> class.</returns>
        ModemResponse GetPreferredMessageStorages();

        /// <summary>
        /// Sets the preferred message storage.
        /// </summary>
        /// <param name="storage1Name">Name of the first storage.</param>
        /// <param name="storage2Name">Name of the second storage.</param>
        /// <param name="storage3Name">Name of the third storage.</param>
        /// <returns>A <see cref="ModemResponse"/> indicating the success of the operation.
        /// If success, Result will contain a <see cref="bool"/> indicating the success of the operation.</returns>
        ModemResponse SetPreferredMessageStorage(string storage1Name, string storage2Name, string storage3Name);
        
        /// <summary>
        /// Gets the value indicating whether the modem is ready to send SMS.
        /// </summary>
        bool IsSmsReady { get; }
    }
}
