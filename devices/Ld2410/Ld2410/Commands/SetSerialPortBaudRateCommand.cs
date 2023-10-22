using Ld2410.Extensions;

namespace Ld2410.Commands
{
    internal sealed class SetSerialPortBaudRateCommand : CommandFrame
    {
        public SetSerialPortBaudRateCommand(BaudRate baudRate = BaudRate.BaudRate256000)
            : base(CommandWord.SetBaudRate)
        {
            this.Value = ((ushort)baudRate).ToLittleEndianBytes();
        }
    }

    internal sealed class SetSerialPortBaudRateCommandAck : CommandAckFrame
    {
        public SetSerialPortBaudRateCommandAck(bool isSuccess)
            : base(CommandWord.SetBaudRate, isSuccess)
        {
        }
    }
}
