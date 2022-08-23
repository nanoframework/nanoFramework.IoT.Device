// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.

namespace Iot.Device.Swarm
{
    internal static partial class TileCommands
    {
        /// <summary>
        /// Command to retrieve the configuration settings for the Swarm device ID.
        /// </summary>
        internal class ConfigurationSettings : CommandBase
        {
            public const string Command = "CS";

            public class Reply
            {
                private const int IndexOfDeviceId = 0;
                private const int IndexOfDeviceName = 1;

                /// <summary>
                /// Gets device ID that identifies this device on the Swarm network.
                /// </summary>
                public string DeviceId { get; }

                /// <summary>
                /// Gets device type name.
                /// </summary>
                public string DeviceName { get; }

                public Reply(NmeaSentence sentence)
                {
                    try
                    {
                        // $CS DI=<dev_ID>,DN=<dev_name>*xx
                        //     |                       |
                        //     3
                        int startIndex = ReplyStartIndex;

                        // get version now
                        var configuration = sentence.Data.Substring(startIndex).Split(',');

                        // device ID
                        var deviceIdRaw = configuration[IndexOfDeviceId].Split('=');
                        DeviceId = deviceIdRaw[1];

                        // device name
                        var deviceNameRaw = configuration[IndexOfDeviceName].Split('=');
                        DeviceName = deviceNameRaw[1];
                    }
                    catch
                    {
                        //// empty on purpose, failed to parse the NMEA sentence
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
