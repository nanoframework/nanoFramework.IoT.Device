//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Globalization;

namespace Iot.Device.Swarm
{
    public  static partial class TileCommands
    {
        /// <summary>
        /// Command to send the Tile to sleep mode for a defined time.
        /// </summary>
        public class SleepMode : CommandBase
        {
            public const string Command = "SL";

            public uint SleepInterval = 0;

            public DateTime WakeupTime = DateTime.MinValue;

            public SleepMode(uint value)
            {
                SleepInterval = value;
            }
            public SleepMode(DateTime value)
            {
                WakeupTime = value;
            }

            internal override NmeaSentence ComposeToSend()
            {
                // set command data
                // rate value otherwise
                string data = "";
                if (SleepInterval > 0)
                {
                    data = $"S={SleepInterval}";
                }
                else
                {
                    data = $"U={WakeupTime.ToString(DateTimeFormatInfo.CurrentInfo.UniversalSortableDateTimePattern)}";
                    // need to remove the trailing Z
                    data = data.Substring(0, data.Length - 1);
                }

                return new NmeaSentence($"{Command} {data}");
            }
        }
    }
}
