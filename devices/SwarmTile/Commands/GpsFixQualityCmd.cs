// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.

namespace Iot.Device.Swarm
{
    internal static partial class TileCommands
    {
        /// <summary>
        /// Command to set or query the unsolicited report messages for GPS fix quality.
        /// </summary>
        internal class GpsFixQualityCmd : CommandBase
        {
            public const string Command = "GS";

            public int Value { get; set; }

            public class Reply
            {
                private const int IndexOfHdop = 0;
                private const int IndexOfVdop = 1;
                private const int IndexOfGnssSatellitesCount = 2;
                private const int IndexOfFixType = 4;

                /// <summary>
                /// Gets <see cref="GpsFixQuality"/> information.
                /// </summary>
                public GpsFixQuality Information { get; }

                /// <summary>
                /// Gets or sets rate (in seconds) between each message.
                /// </summary>
                public uint Rate { get; set; } = uint.MinValue;

                public Reply(NmeaSentence sentence)
                {
                    try
                    {
                        if (sentence.Data.Length >= 6)
                        {
                            // $GS <hdop>,<vdop>,<gnss_sats>,<unused>,<fix>*xx
                            //     |                                                |
                            //     3
                            int startIndex = ReplyStartIndex;

                            // get details now
                            var fixInfo = sentence.Data.Substring(startIndex).Split(',');

                            Information = new GpsFixQuality();

                            // fill in all details
                            Information.Hdop = uint.Parse(fixInfo[IndexOfHdop]);
                            Information.Vdop = uint.Parse(fixInfo[IndexOfVdop]);
                            Information.GnssSatellitesCount = uint.Parse(fixInfo[IndexOfGnssSatellitesCount]);
                            
                            switch (fixInfo[IndexOfFixType])
                            {
                                case "DR":
                                    Information.FixType = GpsFixType.DeadReckoning;
                                    break;

                                case "G2":
                                    Information.FixType = GpsFixType.Standalone2D;
                                    break;

                                case "G3":
                                    Information.FixType = GpsFixType.Standalone3D;
                                    break;

                                case "D2":
                                    Information.FixType = GpsFixType.Differential2D;
                                    break;

                                case "D3":
                                    Information.FixType = GpsFixType.Differential3D;
                                    break;

                                case "RK":
                                    Information.FixType = GpsFixType.Combined;
                                    break;

                                case "TT":
                                    Information.FixType = GpsFixType.TimeOnly;
                                    break;

                                case "NF":
                                default:
                                    Information.FixType = GpsFixType.None;
                                    break;
                            }
                        }
                        else if (!sentence.Data.Contains(PromptOkReply))
                        {
                            // must be the current GS rate

                            // $GS <rate>* xx 
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

            public GpsFixQualityCmd(int value = 0)
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
