// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using IoT.Device.AtModem.DTOs;
using IoT.Device.AtModem.Events;
using IoT.Device.AtModem.Modem;

namespace IoT.Device.AtModem.Call
{
    /// <summary>
    /// Interface for call management.
    /// </summary>
    public interface ICall
    {
        /// <summary>
        /// Represents the method that will handle incoming call events.
        /// </summary>
        /// <param name="sender">The source of the incoming call event.</param>
        /// <param name="e">An instance of IncomingCallEventArgs containing event data.</param>
        public delegate void IncomingCallEventHandler(object sender, IncomingCallEventArgs e);

        /// <summary>
        /// Occurs when an incoming call is received.
        /// </summary>
        public event IncomingCallEventHandler IncomingCall;

        /// <summary>
        /// Represents the method that will handle missed call events.
        /// </summary>
        /// <param name="sender">The source of the missed call event.</param>
        /// <param name="e">An instance of MissedCallEventArgs containing event data.</param>
        public delegate void MissedCallEventHandler(object sender, MissedCallEventArgs e);

        /// <summary>
        /// Occurs when a missed call event is received.
        /// </summary>
        public event MissedCallEventHandler MissedCall;

        /// <summary>
        /// Represents the method that will handle call started events.
        /// </summary>
        /// <param name="sender">The source of the call started event.</param>
        /// <param name="e">An instance of CallStartedEventArgs containing event data.</param>
        public delegate void CallStartedEventHandler(object sender, CallStartedEventArgs e);

        /// <summary>
        /// Occurs when a call is started.
        /// </summary>
        public event CallStartedEventHandler CallStarted;

        /// <summary>
        /// Represents the method that will handle call ended events.
        /// </summary>
        /// <param name="sender">The source of the call ended event.</param>
        /// <param name="e">An instance of CallEndedEventArgs containing event data.</param>
        public delegate void CallEndedEventHandler(object sender, CallEndedEventArgs e);

        /// <summary>
        /// Occurs when a call is ended.
        /// </summary>
        public event CallEndedEventHandler CallEnded;

        /// <summary>
        /// Answers an incoming call.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> indicating the success or failure of the operation.</returns>
        ModemResponse AnswerIncomingCall();

        /// <summary>
        /// Dials a phone number.
        /// </summary>
        /// <param name="phoneNumber">The phone number to dial.</param>
        /// <param name="hideCallerNumber">A flag indicating whether to hide the caller number.</param>
        /// <param name="closedUserGroup">A flag indicating whether to use the closed user group feature.</param>
        /// <returns>A <see cref="ModemResponse"/> indicating the success or failure of the operation.</returns>
        ModemResponse Dial(PhoneNumber phoneNumber, bool hideCallerNumber = false, bool closedUserGroup = false);

        /// <summary>
        /// Hangs up the current call.
        /// </summary>
        /// <returns>A <see cref="ModemResponse"/> indicating the success or failure of the operation.</returns>
        ModemResponse Hangup();
    }
}
