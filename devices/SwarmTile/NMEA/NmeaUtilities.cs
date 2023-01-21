// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.

namespace Iot.Device.Swarm
{
    internal class NmeaUtilities
    {
        /// <summary>
        /// Computes NMEA checksum of a NMEA message.
        /// </summary>
        /// <param name="payload">Payload message to compute checksum.</param>
        /// <param name="receivedSentence">Set to <see langword="true"/> if this a received sentence.</param>
        /// <returns>Th NMEA checksum of the <paramref name="payload"/>.</returns>
        internal static byte ComputeChecksum(string payload, bool receivedSentence = true)
        {
            int index = 0;
            byte checksum = 0;

            // check for $ start of sentence
            if (payload[0] == '$')
            {
                index++;
            }

            for (; index < payload.Length; index++)
            {
                // stop at '*' if this is a received sentence
                // if 
                if (receivedSentence
                    && payload[index] == '*')
                {
                    // done here
                    break;
                }

                checksum ^= (byte)payload[index];
            }

            return checksum;
        }
    }
}
