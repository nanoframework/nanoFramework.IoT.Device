//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace Iot.Device.Swarm
{
    internal class NmeaUtilities
    {
        /// <summary>
        /// Computes NMEA checksum of a NMEA message.
        /// </summary>
        /// <param name="payload">Payload message to compute checksum</param>
        /// <returns>Th NMEA checksum of the <paramref name="payload"/>.</returns>
        public static byte ComputeChecksum(string payload)
        {
            int index = 0;
            byte checksum = 0;

            // check for $ start of sentence
            if(payload[0] == '$')
            {
                index++;
            }

            for (; index < payload.Length; index++)
            {
                // stop at '*'
                if (payload[index] == '*')
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
