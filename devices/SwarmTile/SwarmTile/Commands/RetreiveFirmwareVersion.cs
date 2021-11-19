//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
//

namespace Iot.Device.Swarm
{
    internal static partial class TileCommands
    {
        /// <summary>
        /// Command to retrieve the firmware details.
        /// </summary>
        internal class RetreiveFirmwareVersion : CommandBase
        {
            public const string Command = "FV";

            public class Reply
            {
                private const int _indexOfTimeStamp = 0;
                private const int _indexOfVersion = 1;

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

                    int startIndex = ReplyStartIndex;

                    // get version now
                    var fwInfo = sentence.Data.Substring(startIndex).Split(',');

                    FirmwareTimeStamp = fwInfo[_indexOfTimeStamp];
                    FirmwareVersion = fwInfo[_indexOfVersion];
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
