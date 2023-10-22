namespace Ld2410.Commands
{
    internal sealed class SetEngineeringModeCommand : CommandFrame
    {
        public SetEngineeringModeCommand(bool enable)
            : base(enable ? CommandWord.EnableEngineeringMode : CommandWord.EndEngineeringMode)
        {
        }
    }

    internal sealed class SetEngineeringModeCommandAck : CommandAckFrame
    {
        public SetEngineeringModeCommandAck(CommandWord command, bool isSuccess)
            : base(command, isSuccess)
        {
        }
    }
}
