//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace Iot.Device.Swarm
{
    public static class TileCommands
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

        public class ReceiveTest : CommandBase
        {
            public const string Command = "RT";

            public uint Value;

            public class Reply
            {
                /// <summary>
                /// RSSI for background noise.
                /// </summary>
                public int BackgroundRssi { get; } = int.MinValue;

                /// <summary>
                /// Current rate (in seconds) between each message.
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
                                // get value at this postion
                                BackgroundRssi = int.Parse(sentence.Data.Substring(8));
                            }
                        }
                        else
                        {
                            // this is a rate reply
                            // $DT <rate>* xx 
                            //     |    |
                            //     3       
                            // must be the current RT rate
                            Rate = uint.Parse(sentence.Data.Substring(3));
                        }
                    }
                    catch
                    {
                        // empty on purpose, failed to parse the NMEA sentence
                    }
                }
            }

            public ReceiveTest(uint value = 0)
            {
                Value = value;
            }

            internal override NmeaSentence ComposeToSend()
            {
                // set command data
                // query if value is 0
                // rate value otherwise
                string data = Value == 0 ? "?" : $"{Value}";

                return new NmeaSentence($"{Command} {data}");
            }
        }

        public class RetreiveFirmwareVersion : CommandBase
        {
            public const string Command = "FV";

            public class Reply
            {
                /// <summary>
                /// Returns the device firmware version.
                /// </summary>
                public string FirmwareVersion { get; }

                /// <summary>
                /// Returns the time stamp of the device firmware.
                /// </summary>
                public string FirmwareTimeStamp { get; }

                public Reply(NmeaSentence sentence)
                {
                    // $FV 2021-07-16-00:10:21,v1.1.0*74
                    //     |                  |     |
                    //     3                     

                    int startIndex = 3;

                    // get version now
                    var fwInfo = sentence.Data.Substring(startIndex).Split(',');

                    FirmwareTimeStamp = fwInfo[0];
                    FirmwareVersion = fwInfo[1];
                }
            }

            public RetreiveFirmwareVersion()
            {

            }

            internal override NmeaSentence ComposeToSend()
            {
                return new NmeaSentence(Command);
            }
        }

        /// <summary>
        /// Command to send the Tile to power off.
        /// </summary>
        public class PowerOff : CommandBase
        {
            public const string Command = "PO";

            //public PowerOff()
            //{
            //}

            //public PowerOff(NmeaSentence sentence)
            //{
            //    // $FV 2021-07-16-00:10:21,v1.1.0*74
            //    //     |                  |     |
            //    //     3                     

            //    int startIndex = 3;
            //}

            internal override NmeaSentence ComposeToSend()
            {
                return new NmeaSentence(Command);
            }
        }
    }
}
