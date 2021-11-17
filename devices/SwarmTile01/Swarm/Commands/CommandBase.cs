//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace Iot.Device.Swarm
{
    public abstract class CommandBase
    {
        /// <summary>
        /// Command prefix.
        /// </summary>
        //public static string Command { get; internal set; }

        internal abstract NmeaSentence ComposeToSend();
    }
}
