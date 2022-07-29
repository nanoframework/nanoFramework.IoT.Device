// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.

using System;

namespace Iot.Device.Swarm
{
    internal static partial class TileCommands
    {
        /// <summary>
        /// Command to set and query the data time of the device.
        /// </summary>
        internal class DateTimeStatus : CommandBase
        {
            public const string Command = "DT";

            public int Value { get; set; }

            public class Reply
            {
                /// <summary>
                /// Gets <see cref="DateTimeInfo"/> details.
                /// </summary>
                public DateTimeInfo DateTimeInfo { get; }

                /// <summary>
                /// Gets or sets rate (in seconds) between each message.
                /// </summary>
                public uint Rate { get; set; } = uint.MinValue;

                public Reply(NmeaSentence sentence)
                {
                    try
                    {
                        // $DT <YYYY><MM><DD><hh><mm><ss>,<flag>*xx
                        //     |    |   |   |   |   |  |  |    |
                        //     3    7   9  11  13  15  17 18   
                        // check if this looks like a DateTimeStatus content, by looking at its size
                        if (sentence.Data.Length > 10)
                        {
                            // try to parse NMEA sentence
                            int startIndex = ReplyStartIndex;

                            var year = int.Parse(sentence.Data.Substring(startIndex, 4));
                            startIndex += 4;

                            var month = int.Parse(sentence.Data.Substring(startIndex, 2));
                            startIndex += 2;

                            var day = int.Parse(sentence.Data.Substring(startIndex, 2));
                            startIndex += 2;

                            var hour = int.Parse(sentence.Data.Substring(startIndex, 2));
                            startIndex += 2;

                            var minute = int.Parse(sentence.Data.Substring(startIndex, 2));
                            startIndex += 2;

                            var second = int.Parse(sentence.Data.Substring(startIndex, 2));
                            startIndex += 2;

                            DateTimeInfo = new DateTimeInfo();

                            DateTimeInfo.Value = new DateTime(
                                year,
                                month,
                                day,
                                hour,
                                minute,
                                second);

                            // drop comma
                            startIndex++;

                            if (sentence.Data[startIndex] == 'V')
                            {
                                // date time information is valid
                                DateTimeInfo.IsValid = true;
                            }
                            else
                            {
                                // all the rest assume invalid
                                DateTimeInfo.IsValid = false;
                            }
                        }
                        else if (!sentence.Data.Contains("OK"))
                        {
                            // must be the current DT rate
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

            public DateTimeStatus(int value = 0)
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
