namespace Ld2410.Commands
{
    internal sealed class EnableConfigurationCommand : CommandFrame
    {
        internal EnableConfigurationCommand()
            : base(CommandWord.EnableConfiguration)
        {
            base.Value = new byte[] { 0x01, 0x00 };
        }
    }

    internal sealed class EnableConfigurationCommandAck : CommandAckFrame
    {
        internal ushort ProtocolVersion { get; }

        internal ushort BufferSize { get; }

        internal EnableConfigurationCommandAck(
            bool isSuccess,
            ushort protocolVersion,
            ushort bufferSize) : base(CommandWord.EnableConfiguration, isSuccess)
        {
            this.ProtocolVersion = protocolVersion;
            this.BufferSize = bufferSize;
        }
    }
}
