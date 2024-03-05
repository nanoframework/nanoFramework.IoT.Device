// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.AtModem.DTOs;
using Iot.Device.AtModem.Events;
using Iot.Device.AtModem.Modem;
using static Iot.Device.AtModem.Call.ICall;

namespace Iot.Device.AtModem.Call
{
    /// <summary>
    /// Represents a generic call management.
    /// </summary>
    public class GenericCall : ICall
    {
        internal ModemBase ModemBase { get; }

        internal GenericCall(ModemBase modemBase)
        {
            ModemBase = modemBase;
            ModemBase.GenericEvent += ModemBaseGenericEvent;
        }

        /// <inheritdoc/>
        public event IncomingCallEventHandler IncomingCall;

        /// <inheritdoc/>
        public event MissedCallEventHandler MissedCall;

        /// <inheritdoc/>
        public event CallStartedEventHandler CallStarted;

        /// <inheritdoc/>
        public event CallEndedEventHandler CallEnded;

        private void ModemBaseGenericEvent(object sender, GenericEventArgs e)
        {
            if (e.Message == "RING")
            {
                IncomingCall?.Invoke(this, new IncomingCallEventArgs());
            }
            else if (e.Message.StartsWith("VOICE CALL: BEGIN"))
            {
                CallStarted?.Invoke(this, new CallStartedEventArgs());
            }
            else if (e.Message.StartsWith("VOICE CALL: END"))
            {
                CallEnded?.Invoke(this, CallEndedEventArgs.CreateFromResponse(e.Message));
            }
            else if (e.Message.StartsWith("MISSED_CALL: "))
            {
                MissedCall?.Invoke(this, MissedCallEventArgs.CreateFromResponse(e.Message));
            }
        }

        /// <summary>
        /// Answers an incoming call.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> indicating the success or failure of the operation.</returns>
        public virtual ModemResponse AnswerIncomingCall()
        {
            AtResponse response = ModemBase.Channel.SendCommand("ATA");
            return ModemResponse.Success(response.Success);
        }

        /// <summary>
        /// Dials a phone number.
        /// </summary>
        /// <param name="phoneNumber">The phone number to dial.</param>
        /// <param name="hideCallerNumber">A flag indicating whether to hide the caller number.</param>
        /// <param name="closedUserGroup">A flag indicating whether to use the closed user group feature.</param>
        /// <returns>A <see cref="ModemResponse"/> indicating the success or failure of the operation.</returns>
        public virtual ModemResponse Dial(PhoneNumber phoneNumber, bool hideCallerNumber = false, bool closedUserGroup = false)
        {
            string command = $"ATD{phoneNumber}{(hideCallerNumber ? 'I' : 'i')}{(closedUserGroup ? 'G' : 'g')};";
            AtResponse response = ModemBase.Channel.SendCommand(command);
            return ModemResponse.Success(response.Success);
        }

        /// <summary>
        /// Hangs up the current call.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> indicating the success or failure of the operation.</returns>
        public virtual ModemResponse Hangup()
        {
            AtResponse response = ModemBase.Channel.SendCommand($"AT+CHUP");
            return ModemResponse.Success(response.Success);
        }
    }
}
