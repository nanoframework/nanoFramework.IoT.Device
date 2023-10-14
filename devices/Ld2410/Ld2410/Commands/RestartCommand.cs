namespace Ld2410.Commands
{
    internal sealed class RestartCommand : CommandFrame
    {
        internal RestartCommand()
            : base(CommandWord.Restart)
        {
        }
    }

    internal sealed class RestartCommandAck : CommandAckFrame
    {
        internal RestartCommandAck(bool isSuccess)
            : base(CommandWord.Restart, isSuccess)
        {
        }
    }
}
