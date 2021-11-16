//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Threading;

namespace Iot.Device.Swarm
{
    public abstract class CommandBase
    {
        /// <summary>
        /// Command prefix.
        /// </summary>
        //public static string Command { get; internal set; }

        internal AutoResetEvent ProcessedEvent;

        internal abstract NmeaSentence ComposeToSend();
    }
}
