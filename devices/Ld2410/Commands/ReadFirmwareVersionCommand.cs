namespace Ld2410.Commands
{
	internal sealed class ReadFirmwareVersionCommand : CommandFrame
    {
        internal ReadFirmwareVersionCommand() : base(CommandWord.ReadFirmwareVersion)
        {
        }
    }

    internal sealed class ReadFirmwareVersionCommandAck : CommandAckFrame
    {
        internal ushort FirmwareType { get; }

        internal byte Major { get; }

        internal byte Minor { get; }

        internal byte[] Patch { get; }

        internal ReadFirmwareVersionCommandAck(bool isSuccess, ushort firmwareType, byte major, byte minor, byte[] patch)
            : base(CommandWord.ReadFirmwareVersion, isSuccess)
        {
            this.FirmwareType = firmwareType;
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
        }
    }
}
