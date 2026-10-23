// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.LoRa
{
    /// <summary>
    /// Represents a received LoRa packet.
    /// </summary>
    public sealed class LoRaMessage
    {
        /// <summary>
        /// Gets the raw payload bytes.
        /// </summary>
        public byte[] Payload { get; }

        /// <summary>
        /// Gets the signal RSSI in dBm (typically -30 to -120).
        /// </summary>
        public int Rssi { get; }

        /// <summary>
        /// Gets the signal-to-noise ratio in dB.
        /// </summary>
        public float Snr { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoRaMessage" /> class.
        /// </summary>
        /// <param name="payload">The received payload bytes.</param>
        /// <param name="rssi">The measured RSSI in dBm.</param>
        /// <param name="snr">The measured SNR in dB.</param>
        public LoRaMessage(byte[] payload, int rssi, float snr)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            Payload = new byte[payload.Length];
            Array.Copy(payload, Payload, payload.Length);
            Rssi = rssi;
            Snr = snr;
        }
    }
}
