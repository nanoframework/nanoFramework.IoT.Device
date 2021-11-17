//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace Iot.Device.Swarm
{
    public static partial class TileCommands
    {
        /// <summary>
        /// Command to set or query the unsolicited report messages for GPS jamming and spoofing indicators.
        /// </summary>
        public class GpsJammingSpoofing : CommandBase
        {
            public const string Command = "GJ";

            public int Value;

            public class Reply
            {
                /// <summary>
                /// <see cref="JammingSpoofingIndication"/> information.
                /// </summary>
                public JammingSpoofingIndication Indication { get; }

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
                            // $GJ <spoof_state>,<jamming_level>*xx
                            //     |                           |
                            //     3
                            
                            int startIndex = 3;

                            // get details now
                            var indication = sentence.Data.Substring(startIndex).Split(',');

                            Indication = new JammingSpoofingIndication();

                            // spoof state
                            Indication.SpoofState = byte.Parse(indication[0]);

                            // jamming 
                            Indication.SpoofState = byte.Parse(indication[1]);
                        }
                        else if (!sentence.Data.Contains(PromptOkReply))
                        {
                            // must be the current GJ rate

                            // $GJ <rate>* xx 
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

            public GpsJammingSpoofing(int value = 0)
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
