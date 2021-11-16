//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Diagnostics;

namespace Iot.Device.Swarm
{
    /// <summary>
    /// Represents a NMEA-0183 sentence
    /// </summary>
    /// <remarks>For NMEN specs, please check http://freenmea.net/docs </remarks>
    public class NmeaSentence
    {
        // asterisk position in the string counting backwards
        // nnnn*CC
        private const int _asteriskPosition = 2 + 1;

        /// <summary>
        /// Data for the NMEA sentence.
        /// </summary>
        public string Data { get; private set; }

        /// <summary>
        /// Constructor for an <see cref="NmeaSentence"/> from a sentence data.
        /// </summary>
        /// <param name="data">Sentence data.</param>
        public NmeaSentence(string data)
        {
            Data = data;
        }

        /// <summary>
        /// Constructor for a <see cref="NmeaSentence"/> from raw sentence data.
        /// The sentence format and the checksum are validated.
        /// </summary>
        /// <param name="sentence">The NMEA sentence to parse.</param>
        /// <returns>A <see cref="NmeaSentence"/> object with the parsed sentence data.- or- <see langword="null"/> if the parse failed because of formating or wrong checksum.</returns>
        public static NmeaSentence FromRawSentence(string sentence)
        {
            // sanity checks
            // format is: $ttsss,d1,d2,....CRLF
            // CRLF has already been stripped out

            // check for empty sentence
            if (string.IsNullOrEmpty(sentence))
            {
                return null;
            }

            // check if starts with '$'
            if (sentence[0] != '$')
            {
                return null;
            }

            // check if ends with *NN
            if (sentence[sentence.Length - _asteriskPosition] != '*')
            {
                return null;
            }

            // compute NMEA checksum for this sentence
            var checksum = NmeaUtilities.ComputeChecksum(sentence);

            if (checksum.ToString("x2")
                != sentence.Substring(sentence.Length - _asteriskPosition + 1, 2))
            {
                return null;
            }

            // all good, construct an NMEA and store the data
            var newNemaSentence = new NmeaSentence(
                sentence.Substring(1, sentence.Length - _asteriskPosition - 1));

            return newNemaSentence;
        }

        public override string ToString()
        {
            // compute checksum
            var checksum = NmeaUtilities.ComputeChecksum(Data);

            return $"${Data}*{checksum.ToString("x2")}";
        }
    }
}
