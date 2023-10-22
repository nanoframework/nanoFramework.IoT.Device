namespace Ld2410.Commands
{
    internal sealed class FactoryResetCommand : CommandFrame
    {
        internal FactoryResetCommand()
            : base(CommandWord.Reset)
        {
        }
    }

    internal sealed class FactoryResetCommandAck : CommandAckFrame
    {
        internal FactoryResetCommandAck(bool isSuccess) 
            : base(CommandWord.Reset, isSuccess)
        {
        }
    }
}
