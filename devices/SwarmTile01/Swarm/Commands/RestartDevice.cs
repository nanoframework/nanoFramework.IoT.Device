﻿//
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
        /// Command to perform a hardware reset on the Tile.
        /// </summary>
        public class RestartDevice : CommandBase
        {
            public const string Command = "RS";

            internal override NmeaSentence ComposeToSend()
            {
                return new NmeaSentence(Command);
            }
        }
    }
}