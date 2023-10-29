namespace Ld2410.Commands
{
    internal abstract class CommandAckFrame
    {
        internal CommandWord Command { get; }

        internal bool IsSuccess { get; }

        protected CommandAckFrame(CommandWord command, bool isSuccess)
        {
            this.Command = command;
            this.IsSuccess = isSuccess;
        }
    }
}
