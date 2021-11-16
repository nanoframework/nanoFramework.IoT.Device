//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace Iot.Device.Swarm
{
    public  static partial class TileCommands
    {
        /// <summary>
        /// Command to send the Tile to power off.
        /// </summary>
        public class PowerOff : CommandBase
        {
            public const string Command = "PO";

            internal override NmeaSentence ComposeToSend()
            {
                return new NmeaSentence(Command);
            }
        }
    }
}
