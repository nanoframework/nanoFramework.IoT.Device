// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.

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
        internal const string PromptOkReply = " OK";

        /// <summary>
        /// This is the prompt found on ERROR replies from the Tile.
        /// </summary>
        internal const string PromptErrorReply = " ERR";

        /// <summary>
        /// Default index of the start of data in a typical reply from the Tile.
        /// </summary>
        internal const int ReplyStartIndex = 3;

        internal abstract NmeaSentence ComposeToSend();
    }
}
