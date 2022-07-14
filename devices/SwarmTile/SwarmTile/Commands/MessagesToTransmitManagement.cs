// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.

using System;

namespace Iot.Device.Swarm
{
    /// <summary>
    /// Class holding all commands that are sent to the Tile.
    /// </summary>
    internal static partial class TileCommands
    {
        /// <summary>
        /// Command to manage messages received database.
        /// </summary>
        internal class MessagesToTransmitManagement : CommandBase
        {
            private const int _indexOfData = 0;
            private const int _indexOfMessageId = 1;
            private const int _indexOfEpoch = 2;

            public const string Command = "MT";

            public class Reply
            {
                /// <summary>
                /// Number of messages.
                /// </summary>
                public int MessageCount { get; } = -1;

                /// <summary>
                /// Message to transmit.
                /// </summary>
                public MessageToTransmit Message { get; }

                public Reply(NmeaSentence sentence)
                {
                    try
                    {
                        if (sentence.Data.Length < 6
                            && !sentence.Data.Contains(PromptOkReply))
                        {
                            // $MT <msg_count>*xx
                            //     |     |
                            //     3

                            int startIndex = ReplyStartIndex;

                            MessageCount = int.Parse(sentence.Data.Substring(startIndex));
                        }
                        else if (sentence.Data.Length > 9
                                 && !sentence.Data.Contains(PromptErrorReply))
                        {
                            // $MT <data>,<msg_id>,<es>*xx
                            //     |                   |
                            //     3

                            int startIndex = ReplyStartIndex;

                            // get details now
                            var messageData = sentence.Data.Substring(startIndex).Split(',');

                            // build message
                            Message = new MessageToTransmit(
                                messageData[_indexOfData]);

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

            public static NmeaSentence GetMessageCount()
            {
                return new NmeaSentence($"{Command} C=U");
            }

            public static NmeaSentence DeleteMessages(string id)
            {
                string param = string.IsNullOrEmpty(id) ? "U" : $"{id}";

                return new NmeaSentence($"{Command} D={param}");
            }

            public static NmeaSentence ListMessage(string id)
            {
                string param = string.IsNullOrEmpty(id) ? "U" : $"{id}";

                return new NmeaSentence($"{Command} L={param}");
            }

            internal override NmeaSentence ComposeToSend()
            {
                // set command data
                return new NmeaSentence($"{Command} ");
            }
        }
    }
}
