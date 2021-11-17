//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace Iot.Device.Swarm
{
    public abstract class CommandBase
    {
        // this is the prompt found on OK replies from the Tile.
        public const string PromptOkReply = " OK";

        // this is the prompt found on ERROR replies from the Tile.
        public const string PromptErrorReply = " ERR";

        /// <summary>
        /// Command prefix.
        /// </summary>
        //public static string Command { get; internal set; }

        internal abstract NmeaSentence ComposeToSend();
    }
}
