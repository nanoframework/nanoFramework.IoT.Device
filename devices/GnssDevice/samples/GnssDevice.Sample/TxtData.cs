// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Common.GnssDevice;
using System;

namespace GnssDevice.Sample
{
    /// <summary>
    /// Implements a simple TXT data from a GNSS device.
    /// </summary>
    internal class TxtData : NmeaData
    {
        /// <inheritdoc/>
        public override string MessageId => "TXT";

        /// <summary>
        /// Gets the decoded text.
        /// </summary>
        public string Text { get; internal set; }

        /// <summary>
        /// Gets the severity of the message.
        /// </summary>
        public MessageSeverity Severity { get; internal set; }

        /// <inheritdoc/>
        public override NmeaData Parse(string inputData)
        {
            if (!IsMatch(inputData))
            {
                throw new ArgumentException();
            }

            try
            {
                var subfields = GetSubFields(inputData);
                if(subfields.Length < 5)
                {
                    return null;
                }

                var txt = subfields[4];
                var sev = (MessageSeverity)int.Parse(subfields[3]);
                return new TxtData(txt, sev);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid NMEA TXT data", ex);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TxtData"/> class.
        /// </summary>
        public TxtData() 
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TxtData"/> class.
        /// </summary>
        /// <param name="txt">The TXT entry.</param>
        /// <param name="sev">The severity of the message.</param>
        public TxtData(string txt, MessageSeverity sev)
        {
            Text = txt;
            Severity = sev;
        }
    }
}
