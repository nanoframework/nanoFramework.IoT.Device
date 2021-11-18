//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace Iot.Device.Swarm
{
    internal static partial class TileCommands
    {
        /// <summary>
        /// Command to set or query the unsolicited report messages for geospatial information.
        /// </summary>
        public class GeospatialInfo : CommandBase
        {
            public const string Command = "GN";

            public int Value;

            public class Reply
            {
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
                            Information.Latitude = float.Parse(geoInfo[0]);
                            Information.Longitude = float.Parse(geoInfo[1]);
                            Information.Altitude = float.Parse(geoInfo[2]);
                            Information.Course = float.Parse(geoInfo[3]);
                            Information.Speed = float.Parse(geoInfo[4]);
                        }
                        else if (!sentence.Data.Contains(PromptOkReply))
                        {
                            // must be the current GN rate

                            // $GN <rate>* xx 
                            //     |    |
                            //     3       

                            Rate = uint.Parse(sentence.Data.Substring(3));
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
