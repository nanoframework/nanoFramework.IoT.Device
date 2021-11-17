//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace Iot.Device.Swarm
{
    /// <summary>
    /// Base class for commands.
    /// </summary>
    public abstract class CommandBase
    {
        /// <summary>
        /// This is the prompt found on OK replies from the Tile.
        /// </summary>
        public const string PromptOkReply = " OK";


        /// <summary>
        /// this is the prompt found on ERROR replies from the Tile.
        /// </summary>
        public const string PromptErrorReply = " ERR";

        internal abstract NmeaSentence ComposeToSend();
    }
}
