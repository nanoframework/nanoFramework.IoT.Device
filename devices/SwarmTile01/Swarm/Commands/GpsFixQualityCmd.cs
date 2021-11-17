//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace Iot.Device.Swarm
{
    public static partial class TileCommands
    {
        /// <summary>
        /// Command to set or query the unsolicited report messages for GPS fix quality.
        /// </summary>
        public class GpsFixQualityCmd : CommandBase
        {
            public const string Command = "GS";

            public int Value;

            public class Reply
            {
                /// <summary>
                /// <see cref="GpsFixQuality"/> information.
                /// </summary>
                public GpsFixQuality Information { get; }

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
                            // $GS <hdop>,<vdop>,<gnss_sats>,<unused>,<fix>*xx
                            //     |                                                |
                            //     3
                            
                            int startIndex = 3;

                            // get details now
                            var fixInfo = sentence.Data.Substring(startIndex).Split(',');

                            Information = new GpsFixQuality();

                            // fill in all details
                            Information.Hdop = uint.Parse(fixInfo[0]);
                            Information.Vdop = uint.Parse(fixInfo[1]);
                            Information.GnssSatellitesCount = uint.Parse(fixInfo[2]);
                            // 3rd position is unused
                            
                            switch(fixInfo[4])
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

                            Rate = uint.Parse(sentence.Data.Substring(3));
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
