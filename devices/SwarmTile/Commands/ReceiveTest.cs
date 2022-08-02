// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.

namespace Iot.Device.Swarm
{
    internal static partial class TileCommands
    {
        /// <summary>
        /// Command to set and query the receive test message rate.
        /// </summary>
        internal class ReceiveTest : CommandBase
        {
            public const string Command = "RT";

            public int Value { get; set; }

            public class Reply
            {
                /// <summary>
                /// Gets RSSI for background noise.
                /// </summary>
                public int BackgroundRssi { get; } = int.MinValue;

                /// <summary>
                /// Gets or sets rate (in seconds) between each message.
                /// </summary>
                public uint Rate { get; set; } = uint.MinValue;

                public Reply(NmeaSentence sentence)
                {
                    try
                    {
                        if (sentence.Data.Contains("RSSI"))
                        {
                            // message contains RSSI, estimate the format
                            if (sentence.Data.Length > 20)
                            {
                                // $RT RSSI=<rssi_sat>,SNR=<snr>,FDEV=<fdev>,TS=<time>,DI=<sat_id>*xx
                                //          |         |    |    |     |     |   |     |   |      |
                                //          8         7    9    11    13    15  17    18  22   
                            }
                            else
                            {
                                // $RT RSSI=<rssi_background>*xx
                                //          |               |
                                //          8
                                // get value at this position
                                BackgroundRssi = int.Parse(sentence.Data.Substring(8));
                            }
                        }
                        else if (!sentence.Data.Contains("OK"))
                        {
                            // must be the current RT rate
                            // $DT <rate>* xx 
                            //     |    |
                            //     3       
                            Rate = uint.Parse(sentence.Data.Substring(ReplyStartIndex));
                        }
                    }
                    catch
                    {
                        //// empty on purpose, failed to parse the NMEA sentence
                    }
                }
            }

            public ReceiveTest(int value = 0)
            {
                Value = value;
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
