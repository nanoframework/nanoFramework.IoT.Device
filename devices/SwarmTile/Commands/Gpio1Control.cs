// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.

namespace Iot.Device.Swarm
{
    internal static partial class TileCommands
    {
        /// <summary>
        /// Command to set, query and control of the GPIO1 pin to allow indications or control the operation of the Tile.
        /// </summary>
        internal class Gpio1Control : CommandBase
        {
            public const string Command = "GP";

            public GpioMode Mode { get; set; }

            public class Reply
            {
                /// <summary>
                /// Gets <see cref="GpioMode"/> information.
                /// </summary>
                public GpioMode Mode { get; } = GpioMode.Unknwon;

                public Reply(NmeaSentence sentence)
                {
                    try
                    {
                        if (sentence.Data.Length < 6
                            && !sentence.Data.Contains(PromptOkReply))
                        {
                            // $GP <mode>*xx
                            //     |     |
                            //     3
                            int startIndex = ReplyStartIndex;

                            Mode = (GpioMode)byte.Parse(sentence.Data.Substring(startIndex));
                        }
                    }
                    catch
                    {
                        //// empty on purpose, failed to parse the NMEA sentence
                    }
                }
            }

            public Gpio1Control(GpioMode mode)
            {
                Mode = mode;
            }

            public static NmeaSentence GetMode()
            {
                return new NmeaSentence($"{Command} ?");
            }

            internal override NmeaSentence ComposeToSend()
            {
                // set command data
                return new NmeaSentence($"{Command} {(int)Mode}");
            }
        }
    }
}
