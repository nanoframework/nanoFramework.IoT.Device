// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.AtModem.DTOs
{
    /// <summary>
    /// Represents the signal strength information.
    /// </summary>
    public class SignalStrength
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignalStrength"/> class with the specified RSSI and BER.
        /// </summary>
        /// <param name="rssi">The Received Signal Strength Indicator.</param>
        /// <param name="ber">The Bit Error Rate.</param>
        public SignalStrength(int rssi, int ber)
        {
            Rssi = rssi;
            Ber = ber;
        }

        /// <summary>
        /// Gets the Received Signal Strength Indicator (RSSI).
        /// A value of 99 indicates it is not known or not detectable.
        /// </summary>
        public int Rssi { get; }

        /// <summary>
        /// Gets the Bit Error Rate (BER).
        /// A value of 99 indicates it is not known or not detectable.
        /// </summary>
        public int Ber { get; }

        /// <summary>
        /// Returns a string representation of the signal strength.
        /// </summary>
        /// <returns>A string representing the RSSI and BER.</returns>
        public override string ToString()
        {
            return $"RSSI: {Rssi}, BER: {Ber}";
        }
    }
}
