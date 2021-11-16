//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace Iot.Device.Swarm
{
    public  static partial class TileCommands
    {
        public class TransmitData : CommandBase
        {
            public const string Command = "TD";

            private Message _message;

            public class Reply
            {
                /// <summary>
                /// Id assigned to the message after being placed in the local queue.
                /// </summary>
                public string MessageId { get; }

                /// <summary>
                /// Error message.
                /// </summary>
                public string ErrorMessage { get; }

                public Reply(NmeaSentence sentence)
                {
                    if (sentence.Data.StartsWith("TD OK"))
                    {
                        // $TD OK,5354468575855*2a
                        //        |           |
                        //        6                     

                        int startIndex = 6;

                        // get message ID
                        MessageId = sentence.Data.Substring(startIndex);
                    }
                    else if (sentence.Data.StartsWith("TD ERROR"))
                    {
                        // $TD ERR,BADDATA*0e
                        //         |     |
                        //         7                     

                        int startIndex = 7;

                        // get error message
                        ErrorMessage = sentence.Data.Substring(startIndex);
                    }
                }
            }

            public TransmitData(Message message)
            {
                _message = message;
            }

            internal override NmeaSentence ComposeToSend()
            {
                // set optional parameters
                string hdParam = "";
                string etParam = "";

                if (_message.HoldDuration > 0)
                {
                    hdParam = $",HD={_message.HoldDuration}";
                }

                if(_message.ExpireTime > DateTime.MinValue)
                {
                    etParam = $",ET={_message.ExpireTime.ToUnixTimeSeconds()}";
                }

                return new NmeaSentence($"{Command} AI={_message.ApplicationID}{hdParam}{etParam},\"{_message.Data}\"");
            }
        }
    }
}
