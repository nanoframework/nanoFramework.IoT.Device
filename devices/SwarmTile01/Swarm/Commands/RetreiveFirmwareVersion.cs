﻿//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace Iot.Device.Swarm
{
    public static partial class TileCommands
    {
        /// <summary>
        /// Command to retrieve the firmware details.
        /// </summary>
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
    }
}
