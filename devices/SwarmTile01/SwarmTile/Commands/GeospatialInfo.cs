//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
//

namespace Iot.Device.Swarm
{
    internal static partial class TileCommands
    {
        /// <summary>
        /// Command to set or query the unsolicited report messages for geospatial information.
        /// </summary>
        internal class GeospatialInfo : CommandBase
        {
            public const string Command = "GN";

            public int Value;

            public class Reply
            {
                private const int _indexOfLatitude = 0;
                private const int _indexOfLongitude = 1;
                private const int _indexOfAltitude = 2;
                private const int _indexOfCourse = 3;
                private const int _indexOfSpeed = 4;

                /// <summary>
                /// <see cref="GeospatialInformation"/> information.
                /// </summary>
                public GeospatialInformation Information { get; }

                /// <summary>
                /// Current rate (in seconds) between each message.
                /// </summary>
                public uint Rate { get; set; } = uint.MinValue;

                public Reply(NmeaSentence sentence)
                {
                    try
                    {
                        if (sentence.Data.Length >= 6)
                        {
                            // $GN <latitude>,<longitude>,<altitude>,<course>,<speed>*xx
                            //     |                                                |
                            //     3

                            int startIndex = ReplyStartIndex;

                            // get details now
                            var geoInfo = sentence.Data.Substring(startIndex).Split(',');

                            Information = new GeospatialInformation();

                            // fill in all details
                            Information.Latitude = float.Parse(geoInfo[_indexOfLatitude]);
                            Information.Longitude = float.Parse(geoInfo[_indexOfLongitude]);
                            Information.Altitude = float.Parse(geoInfo[_indexOfAltitude]);
                            Information.Course = float.Parse(geoInfo[_indexOfCourse]);
                            Information.Speed = float.Parse(geoInfo[_indexOfSpeed]);
                        }
                        else if (!sentence.Data.Contains(PromptOkReply))
                        {
                            // must be the current GN rate

                            // $GN <rate>* xx 
                            //     |    |
                            //     3       

                            Rate = uint.Parse(sentence.Data.Substring(ReplyStartIndex));
                        }
                    }
                    catch
                    {
                        // empty on purpose, failed to parse the NMEA sentence
                    }
                }
            }

            public GeospatialInfo(int value = 0)
            {
                Value = value;
            }

            public static NmeaSentence GetLast()
            {
                return new NmeaSentence($"{Command} @");
            }

            internal override NmeaSentence ComposeToSend()
            {
                // set command data
                // query if value is 0
                // rate value otherwise
                string data = Value < 0 ? "?" : $"{Value}";

                return new NmeaSentence($"{Command} {data}");
            }
        }
    }
}
