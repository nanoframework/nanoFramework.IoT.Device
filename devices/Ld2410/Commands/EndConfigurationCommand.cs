namespace Ld2410.Commands
{
    internal sealed class EndConfigurationCommand : CommandFrame
    {
        internal EndConfigurationCommand()
            : base(CommandWord.EndConfiguration)
        {
        }
    }

    internal sealed class EndConfigurationCommandAck : CommandAckFrame
    {
        internal EndConfigurationCommandAck(bool isSuccess) 
            : base(CommandWord.EndConfiguration, isSuccess)
        {
        }
    }
}
