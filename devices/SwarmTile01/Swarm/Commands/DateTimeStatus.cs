//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace Iot.Device.Swarm
{
    public static partial class TileCommands
    {
        public class DateTimeStatus : CommandBase
        {
            public const string Command = "DT";

            public class Reply
            {
                /// <summary>
                /// <see cref="DateTime"/> value of the last date & time information.
                /// </summary>
                public DateTime Value { get; } = DateTime.MinValue;

                /// <summary>
                /// Information if the available <see cref="DateTime"/> of the module is valid.
                /// </summary>
                public bool IsValid { get; }

                /// <summary>
                /// Current rate (in seconds) between each message.
                /// </summary>
                public int Rate { get; set; } = int.MinValue;

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
                            int startIndex = 3;

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

                            Value = new DateTime(
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
                                // date time information is vcalid
                                IsValid = true;
                            }
                            else
                            {
                                // all the rest assume invalid
                                IsValid = false;
                            }
                        }
                        else
                        {
                            // $DT <rate>* xx 
                            //     |    |
                            //     3       
                            // must be the current DT rate
                            Rate = int.Parse(sentence.Data.Substring(1));
                        }
                    }
                    catch
                    {
                        // empty on purpose, failed to parse the NMEA sentence
                    }
                }
            }

            internal override NmeaSentence ComposeToSend()
            {
                return new NmeaSentence(Command);
            }
        }
    }
}
