// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.

using System;

namespace Iot.Device.Swarm
{
    internal static partial class TileCommands
    {
        /// <summary>
        /// Command to manage messages received database.
        /// </summary>
        internal class MessagesReceivedManagement : CommandBase
        {
            private const int IndexOfAppId = 0;
            private const int IndexOfData = 1;
            private const int IndexOfMessageId = 2;
            private const int IndexOfEpoch = 3;

            public const string Command = "MM";

            public class Reply
            {
                /// <summary>
                /// Gets number of messages.
                /// </summary>
                public int MessageCount { get; } = -1;

                /// <summary>
                /// Gets message read.
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
                            int startIndex = ReplyStartIndex;

                            MessageCount = int.Parse(sentence.Data.Substring(startIndex));
                        }
                        else if (sentence.Data.Length > 9
                                 && !sentence.Data.Contains(PromptErrorReply))
                        {
                            // $MM <appID>,<data>,<msg_id>,<es>*xx
                            //     |                           |
                            //     3
                            int startIndex = ReplyStartIndex;

                            // get details now
                            var messageData = sentence.Data.Substring(startIndex).Split(',');

                            // get message details
                            var applicationID = uint.Parse(messageData[IndexOfAppId]);

                            // build message
                            Message = new MessageReceived(
                                messageData[IndexOfData],
                                applicationID);

                            Message.ID = messageData[IndexOfMessageId];
                            Message.TimeStamp = DateTime.FromUnixTimeSeconds(long.Parse(messageData[IndexOfEpoch]));
                        }
                    }
                    catch
                    {
                        //// empty on purpose, failed to parse the NMEA sentence
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
