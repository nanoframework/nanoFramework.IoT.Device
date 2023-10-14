namespace Ld2410.Commands
{
    internal sealed class GetFirmwareVersionCommand : CommandFrame
    {
        internal GetFirmwareVersionCommand() : base(CommandWord.ReadFirmwareVersion)
        {
        }
    }

    internal sealed class GetFirmwareVersionCommandAck : CommandAckFrame
    {
        internal ushort FirmwareType { get; }

        internal byte Major { get; }

        internal byte Minor { get; }

        internal uint Patch { get; }

        internal GetFirmwareVersionCommandAck(bool isSuccess, ushort firmwareType, byte major, byte minor, uint patch) 
            : base(CommandWord.ReadFirmwareVersion, isSuccess)
        {
            this.FirmwareType = firmwareType;
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
        }
    }
}
