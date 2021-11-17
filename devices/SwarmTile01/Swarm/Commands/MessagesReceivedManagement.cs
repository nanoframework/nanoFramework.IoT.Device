//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace Iot.Device.Swarm
{
    public static partial class TileCommands
    {
        /// <summary>
        /// Command to set, query and control of the GPIO1 pin to allow indications or control the operation of the Tile.
        /// </summary>
        public class MessagesReceivedManagement : CommandBase
        {
            private const int _indexOfAppId = 0;
            private const int _indexOfData = 1;
            private const int _indexOfMessageId = 2;
            private const int _indexOfEpoch = 3;

            public const string Command = "MM";


            public class Reply
            {
                /// <summary>
                /// Number of messages.
                /// </summary>
                public int MessageCount { get; } = -1;

                /// <summary>
                /// Message read.
                /// </summary>
                public MessageReceived Message { get; }

                public Reply(NmeaSentence sentence)
                {
                    try
                    {
                        if (sentence.Data.Length < 6
                            && !sentence.Data.Contains(PromptOkReply))
                        {
                            // $MM <msg_count>*xx
                            //     |     |
                            //     3

                            int startIndex = 3;

                            MessageCount = int.Parse(sentence.Data.Substring(startIndex));
                        }
                        else if (sentence.Data.Length > 9
                                 && !sentence.Data.Contains(PromptErrorReply))
                        {
                            // $MM <appID>,<data>,<msg_id>,<es>*xx
                            //     |                           |
                            //     3

                            int startIndex = 3;

                            // get details now
                            var messageData = sentence.Data.Substring(startIndex).Split(',');

                            // get message details
                            var applicationID = uint.Parse(messageData[_indexOfAppId]);

                            // build message
                            Message = new MessageReceived(
                                messageData[_indexOfData],
                                applicationID);

                            Message.ID = messageData[_indexOfMessageId];
                            Message.TimeStamp = DateTime.FromUnixTimeSeconds(long.Parse(messageData[_indexOfEpoch]));
                        }
                    }
                    catch
                    {
                        // empty on purpose, failed to parse the NMEA sentence
                    }
                }
            }

            public static NmeaSentence GetMessageCount(bool unreadOnly)
            {
                string param = unreadOnly ? "U" : "*";

                return new NmeaSentence($"{Command} C={param}");
            }

            public static NmeaSentence DeleteMessage(string id)
            {
                return new NmeaSentence($"{Command} D={id}");
            }

            public static NmeaSentence DeleteMessages(bool readOnly)
            {
                string param = readOnly ? "R" : "*";

                return new NmeaSentence($"{Command} D={param}");
            }

            public static NmeaSentence MarkMessagesRead(string id)
            {
                string param = string.IsNullOrEmpty(id) ? "*" : $"{id}";

                return new NmeaSentence($"{Command} M={param}");
            }

            public static NmeaSentence ReadMessage(string id)
            {
                return new NmeaSentence($"{Command} R={id}");
            }

            public static NmeaSentence ReadMessage(bool newest)
            {
                string param = newest ? "N" : "O";

                return new NmeaSentence($"{Command} R={param}");
            }

            internal override NmeaSentence ComposeToSend()
            {
                // set command data
                return new NmeaSentence($"{Command} ");
            }
        }
    }
}
